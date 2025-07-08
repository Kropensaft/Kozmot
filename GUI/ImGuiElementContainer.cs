using System.Numerics;
using ImGuiNET;
using OpenGL.Objects;

namespace OpenGL.GUI;

internal abstract class ImGuiElementContainer : IDisposable
{
    //floats
    private const float DEFAULT_ANGULAR_SPEED = 0.2f;

    //Bool
    private static bool IsGUITransparent;

    // (U)Int
    private static int defaultPlanetTypeIndex;
    private static int selectedParentIndex;
    private static int selectedRemovalIndex;
    public static int selectedPivotIndex; // Separate index for camera pivot

    //String
    private static string nameBuffer = Constants.DEFAULT_NAME_BUFFER; // Initialize from Constants
    private static readonly string[] planetTypes = Constants.planetTypes;

    //Vec3
    public static Vector3 position = new(
        Constants.DEFAULT_INDICATOR_POSITION.X,
        Constants.DEFAULT_INDICATOR_POSITION.Y,
        Constants.DEFAULT_INDICATOR_POSITION.Z); // Convert to system.numerics for ImGuI

    public static Vector3 Velocity = new(0f, 0f, 0f);
    private static Vector3 IndicatorColor = Constants.INDICATOR_COLOR;
    private static Vector3 color = Constants.ROCKY_PLANET_COLOR;
    private static readonly OpenTK.Mathematics.Vector3 DEFAULT_ROTATION = OpenTK.Mathematics.Vector3.Zero;
    private static readonly OpenTK.Mathematics.Vector3 DEFAULT_SCALE = OpenTK.Mathematics.Vector3.One;

    //Lists
    public static List<Object> celestialBodies = new();
    private static float scale = 1f;
    private static float angularSpeed = DEFAULT_ANGULAR_SPEED;

    public static bool IsEditing =>
        !string.IsNullOrEmpty(nameBuffer) || // Check nameBuffer directly
        ImGui.IsAnyItemActive(); // Simplified check

    public static uint selectedPlanetTypeRef => (uint)defaultPlanetTypeIndex;


    public void Dispose()
    {
        celestialBodies.Clear();
        GC.SuppressFinalize(this);
    }

    public static void SubmitUI()
    {
        string[] parentNames = celestialBodies
            .OfType<Sphere>()
            .Where(s => s.Parent == null)
            .Select(s => s.Name)
            .ToArray();

        string[] allBodyNames = celestialBodies
            .Select(b => b.Name)
            .ToArray();

        if (IsGUITransparent)
        {
            if (!ImGui.Begin("GUI",
                    ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground))
            {
                ImGui.End();
                return;
            }
        }
        else
        {
            if (!ImGui.Begin("GUI", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar))
            {
                ImGui.End();
                return;
            }
        }

        try
        {
            if (ImGui.BeginTabBar("Settings#left"))
            {
                if (ImGui.BeginTabItem("Starter Guide"))
                    try
                    {
                        ImGui.Text("=== Planet Creator Guide ===");
                        ImGui.BulletText("1. Enter Name and Select Type.");
                        ImGui.BulletText("2. Set Position (relative to parent for moons, else origin).");
                        ImGui.BulletText("3. Set angular speed (time it takes to complete a full rotation). ");
                        ImGui.BulletText("4. For Moons: Select Parent from 'Orbits around' dropdown.");
                        ImGui.BulletText("5. For 'Custom' select color, and scale");
                        ImGui.BulletText("6. Click 'Create'.");
                        ImGui.Separator();
                        ImGui.Text("=== Camera Controls ===");
                        ImGui.BulletText("Mouse Wheel / Up/Down Arrows: Zoom");
                        ImGui.BulletText("Right Mouse Button + Drag: Orbit camera");
                        ImGui.BulletText("Spacebar + Drag: Orbit (Touchpad alternative)");
                        ImGui.BulletText("If you wish to centralize a planet in its movement, select it as a pivot");
                        ImGui.Separator();
                        ImGui.Text("=== Keyboard Shortcuts ===");
                        ImGui.BulletText("ESC: Quit Application");
                        ImGui.Separator();
                        ImGui.Text("=== Notes ===");
                        ImGui.BulletText("Keyboard input inactive when GUI has focus.");
                        ImGui.BulletText("Green sphere indicates position of the planet you're creating.");
                    }
                    finally
                    {
                        ImGui.EndTabItem();
                    }

                if (ImGui.BeginTabItem("Planet Creator"))
                    try
                    {
                        // Name Input (required for identification)
                        ImGui.Text("Name:");
                        AddToolTip("Unique name for the celestial body");
                        ImGui.SetNextItemWidth(Constants.BESPOKE_TEXTEDIT_WIDE_WIDTH);
                        ImGui.InputText("##PlanetName", ref nameBuffer, 20);

                        // Reset Button
                        AddToolTip("Resets the simulation to the state it was started in",
                            ImGui.GetWindowWidth() -
                            (Constants.BESPOKE_TEXTEDIT_WIDE_WIDTH - Constants.BESPOKE_TEXTEDIT_WIDTH) +
                            Constants.BESPOKE_BUTTON_SIZE.X / 2,
                            false);

                        ImGui.SameLine(ImGui.GetWindowWidth() -
                                       (Constants.BESPOKE_TEXTEDIT_WIDE_WIDTH - Constants.BESPOKE_TEXTEDIT_WIDTH));
                        if (ImGui.Button("Reset", Constants.BESPOKE_BUTTON_SIZE))
                        {
                            ResetUI();
                            Renderer.ResetSimulation();
                        }

                        //Display the time elapsed
                        ImGui.SameLine(ImGui.GetWindowWidth() -
                                       (Constants.BESPOKE_TEXTEDIT_WIDE_WIDTH * 2 - Constants.BESPOKE_TEXTEDIT_WIDTH));
                        ImGui.TextColored(new Vector4(Constants.DESERT_PLANET_COLOR, 1f),
                            $"Time : {WindowManager.GlobalTime}");

                        // Planet Type
                        ImGui.Text("Type:");
                        AddToolTip("Determines visual appearance");
                        ImGui.Combo("##PlanetType", ref defaultPlanetTypeIndex, planetTypes, planetTypes.Length);

                        // Orbit Settings
                        ImGui.Text("Orbit Radius:");
                        AddToolTip("Distance from the parent body or center");
                        ImGui.DragFloat("##OrbitRadius", ref position.X, 0.2f, 0.1f, Constants.GRID_SIZE);

                        ImGui.Text("Angular Speed:");
                        AddToolTip("How fast the object orbits (in radians per second)");
                        ImGui.DragFloat("##AngularSpeed", ref angularSpeed, 0.3f, 0.0f, 4.0f);


                        ImGui.Text("Scale:");
                        AddToolTip("Size in celestial units (1 unit = 1 grid field)");
                        ImGui.DragFloat("##Scale", ref scale, 0.05f, 0.05f, 20f);

                        // Parent Selection for Moons
                        ImGui.Text("Parent Body:");
                        AddToolTip("Select a parent body to orbit around");
                        parentNames = celestialBodies
                            .OfType<Sphere>()
                            .Where(s => s.Parent == null)
                            .Select(s => s.Name)
                            .ToArray();

                        if (parentNames.Length > 0)
                            ImGui.Combo("##Parent", ref selectedParentIndex, parentNames, parentNames.Length);
                        else
                            //Sun is always present
                            selectedParentIndex = 0;


                        // Simulation Control
                        ImGui.Checkbox("Pause Simulation", ref Renderer.IsSimulationPaused);
                        AddToolTip("Freezes the physics simulation while turned on");

                        // Create Button
                        if (ImGui.Button("Create", Constants.BESPOKE_BUTTON_SIZE))
                        {
                            var newSphere = SaveUIValues();
                            if (newSphere != null)
                            {
                                newSphere.TextureID = TextureLoader.LoadTexture(
                                    Constants._TexturePaths[Array.IndexOf(Constants.planetTypes, newSphere.Type)]);
                                Renderer.AddObject(newSphere);
                                celestialBodies.Add(newSphere);
                                ResetUI();
                            }
                        }

                        // Object Management
                        ImGui.SameLine();
                        // Get list of removable objects (excluding suns)
                        var removableObjects = celestialBodies
                            .Select((body, index) => new { Body = body, Index = index })
                            .Where(item => item.Body.Type != Constants.planetTypes[1])
                            .ToList();

                        string[] removalNames = removableObjects.Select(item => item.Body.Name).ToArray();

                        if (removalNames.Length > 0)
                        {
                            if (selectedRemovalIndex >= removalNames.Length)
                                selectedRemovalIndex = removalNames.Length - 1;

                            ImGui.SetNextItemWidth(Constants.BESPOKE_TEXTEDIT_WIDE_WIDTH);
                            ImGui.Combo("##SelectToRemove", ref selectedRemovalIndex, removalNames,
                                removalNames.Length);
                            ImGui.SameLine();

                            if (ImGui.Button("Remove Selected", Constants.BESPOKE_BUTTON_SIZE))
                                if (selectedRemovalIndex >= 0 && selectedRemovalIndex < removableObjects.Count)
                                {
                                    int actualIndex = removableObjects[selectedRemovalIndex].Index;
                                    celestialBodies.RemoveAt(actualIndex);
                                    Renderer.Spheres.RemoveAt(actualIndex);
                                    selectedPivotIndex = celestialBodies.Count - 1;
                                    selectedRemovalIndex = Math.Clamp(selectedRemovalIndex, 0, removalNames.Length - 1);
                                }
                        }
                        else
                        {
                            ImGui.Text("No objects to remove.");
                        }

                        // Object List
                        ImGui.Separator();
                        ImGui.Text("Created objects:");
                        foreach (var body in celestialBodies)
                            if (body is Sphere sphere)
                            {
                                float rotationPeriod = sphere.AngularSpeed > 0
                                    ? MathF.Round(2 * MathF.PI / sphere.AngularSpeed, 2)
                                    : float.PositiveInfinity;

                                string periodInfo = rotationPeriod == float.PositiveInfinity
                                    ? "∞"
                                    : // Display infinity symbol for zero angular speed
                                    rotationPeriod.ToString("F2");

                                ImGui.Text($"{body.Name} (Type: {body.Type}), (Rotation period: {periodInfo} seconds)");
                            }
                    }
                    finally
                    {
                        ImGui.EndTabItem();
                    }

                if (ImGui.BeginTabItem("Camera settings"))
                    try
                    {
                        ImGui.Text("Pivot:");
                        AddToolTip("Uses the celestial object position as the central pivot");
                        if (allBodyNames.Length > 0)
                        {
                            ImGui.Combo("##CameraPivot", ref selectedPivotIndex, allBodyNames, allBodyNames.Length);
                            if (selectedPivotIndex >= 0 && selectedPivotIndex < celestialBodies.Count)
                                Camera._pivot = celestialBodies[selectedPivotIndex].Position;
                        }
                        else
                        {
                            ImGui.Text("No objects created");
                        }

                        ImGui.Text("Zoom Sensitivity");
                        AddToolTip("How fast the camera zooms in/out (default 1.5)");
                        if (ImGui.SliderFloat(" ", ref Constants.CAMERA_ZOOM_SENSITIVITY, 0.1f, 10f))

                            ImGui.Separator();
                        ImGui.Text("=== Indicator Settings ===");

                        ImGui.Checkbox("Render Grid", ref Grid.RenderGrid);
                        ImGui.Checkbox("Render Indicator", ref Renderer.RenderIndicator);

                        ImGui.Text("Color:");
                        AddToolTip("color of the scale/position indicating sphere");
                        if (ImGui.ColorEdit3("##IndicatorColor", ref IndicatorColor))
                            Constants.INDICATOR_COLOR = IndicatorColor;

                        ImGui.Text("Transparency:");
                        AddToolTip("Transparency of the indicator sphere");
                        ImGui.SliderFloat("##IndicatorAlpha", ref Constants.INDICATOR_ALPHA, 0.0f, 0.9f);
                    }
                    finally
                    {
                        ImGui.EndTabItem();
                    }

                if (ImGui.BeginTabItem("Window Settings"))
                    try
                    {
                        ImGui.Checkbox("Show metrics window", ref Renderer.showFPS);
                        AddToolTip(
                            "Shows a mainly debugging window which also include metrics such as\n FPS \n ms/frame \n vertex & index count and more");
                        ImGui.Checkbox("Transparent window", ref IsGUITransparent);
                        AddToolTip(
                            "Renders all of the UI with semi-transparent backgrounds (handy with smaller monitor sizes)");
                    }
                    finally
                    {
                        ImGui.EndTabItem();
                    }

                ImGui.EndTabBar();
            }
        }
        finally
        {
            ImGui.End();
        }
    }

    private static void AddToolTip(string message)
    {
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text(message);
            ImGui.EndTooltip();
        }

        ImGui.SameLine();
    }

    private static void AddToolTip(string message, float xOffset, bool renderQM = true)
    {
        ImGui.SameLine(xOffset);
        ImGui.TextDisabled(renderQM ? "(?)" : " ");


        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text(message);
            ImGui.EndTooltip();
        }

        ImGui.SameLine(xOffset);
    }

    private static Sphere? SaveUIValues()
    {
        if (string.IsNullOrWhiteSpace(nameBuffer))
        {
            Logger.WriteLine("Error: Name cannot be empty");
            return null;
        }


        string planetTypeName = planetTypes[defaultPlanetTypeIndex];

        var relativePosition = new OpenTK.Mathematics.Vector3(
            position.X,
            position.Y,
            position.Z
        );

        OpenTK.Mathematics.Vector3 worldPosition;
        float orbitRadius;
        Object? parent = null;

        if (defaultPlanetTypeIndex == 3) // Index 3 is Moon
        {
            List<Sphere> potentialParents = celestialBodies
                .OfType<Sphere>()
                .Where(s => s.Parent == null)
                .ToList();

            if (selectedParentIndex >= 0 && selectedParentIndex < potentialParents.Count)
            {
                parent = potentialParents[selectedParentIndex];
                worldPosition = parent.Position + relativePosition;
                orbitRadius = relativePosition.Length;

                if (orbitRadius < 0.001f)
                {
                    if (relativePosition.LengthSquared < 0.00001f)
                    {
                        relativePosition = new OpenTK.Mathematics.Vector3(Constants.INITIAL_SPHERE_RADIUS * 2, 0, 0);
                        worldPosition = parent.Position + relativePosition;
                    }

                    orbitRadius = relativePosition.Length;
                }
            }
            else
            {
                Logger.WriteLine(
                    "Warning: Moon type selected, but no valid parent chosen or index out of bounds. Creating as non-orbiting object at specified coords.");
                worldPosition = relativePosition;
                orbitRadius = worldPosition.Length;
                parent = null;
            }
        }
        else
        {
            worldPosition = relativePosition;
            orbitRadius = worldPosition.Length;
            parent = null;
        }


        float radius;
        switch (defaultPlanetTypeIndex)
        {
            case 0: // Rocky Planet
                radius = Constants.ROCKY_PLANET_RADIUS;
                break;
            case 1: // Star
                radius = Constants.STAR_RADIUS;
                break;
            case 2: // Gas Giant
                radius = Constants.GAS_GIANT_RADIUS;
                break;
            case 3: // Moon
                radius = Constants.MOON_RADIUS;
                break;
            case 4: // Desert Planet
                radius = Constants.DESERT_PLANET_RADIUS;
                break;
            case 5: // Ice Giant
                radius = Constants.ICE_GIANT_RADIUS;
                break;
            case 6: // Custom
                radius = Constants.CUSTOM_RADIUS;
                break;
            default: // Fallback to default rocky planet size
                Logger.WriteLine(
                    $"Warning: Unknown planet type index {defaultPlanetTypeIndex}. Defaulting to Rocky Planet radius.");
                radius = Constants.ROCKY_PLANET_RADIUS;
                break;
        }


        var sphereScale = new OpenTK.Mathematics.Vector3(radius, radius, radius) * scale;
        var sphereColor = color; // Already System.Numerics.Vector3
        var sphereRotation = DEFAULT_ROTATION;
        float angularSpeed = Renderer.IsSimulationPaused ? 0f : // Force 0 speed when paused
            parent != null || orbitRadius > 0.001f ? DEFAULT_ANGULAR_SPEED : 0f;

        try
        {
#if DEBUG
            Logger.WriteLine($"\n" +
                             $"Creating new Sphere : {nameBuffer} \n " +
                             $"World pos :{worldPosition + Constants.DEFAULT_INDICATOR_POSITION}\n" +
                             $"Rotation : {sphereRotation}\n" +
                             $"Scale :{sphereScale}\n" +
                             $"Color : {sphereColor}\n" +
                             $"Orbit radius: {orbitRadius}\n" +
                             $"Speed: {angularSpeed}\n" +
                             $"Type: {planetTypeName}\n" +
                             $"Parent ? :{parent}");
#endif

            return new Sphere(
                nameBuffer,
                worldPosition,
                sphereRotation,
                sphereScale,
                sphereColor,
                orbitRadius,
                angularSpeed,
                planetTypeName,
                parent,
                new OpenTK.Mathematics.Vector3(0, 0, 0)
            );
        }
        catch (Exception ex)
        {
            Logger.WriteLine($"Error creating Sphere: {ex.Message}");
            return null;
        }
    }

    public static void ResetUI()
    {
        nameBuffer = Constants.DEFAULT_NAME_BUFFER;
        color = Constants.ROCKY_PLANET_COLOR;
        position = new Vector3(Constants.DEFAULT_INDICATOR_POSITION.X, Constants.DEFAULT_INDICATOR_POSITION.Y,
            Constants.DEFAULT_INDICATOR_POSITION.Z);
        defaultPlanetTypeIndex = 0;
        selectedParentIndex = -1; // Reset to invalid/none
        Velocity = Vector3.Zero;
    }
}