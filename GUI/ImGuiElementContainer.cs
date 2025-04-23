using System.Globalization;
using System.Numerics;
using ImGuiNET;
using OpenGL.Objects;

namespace OpenGL.GUI;

internal abstract class ImGuiElementContainer : IDisposable
{
    private const float DEFAULT_ANGULAR_SPEED = 0.3f;
    private static readonly string[] planetTypes = Constants.planetTypes;
    private static string massBuffer = Constants.DEFAULT_MASS_BUFFER; // Initialize from Constants
    private static string nameBuffer = Constants.DEFAULT_NAME_BUFFER; // Initialize from Constants
    private static bool emissive;
    public static Vector3 position = new(1f, 0f, 0f); // Use System.Numerics for ImGui
    public static Vector3 Velocity = new(0f, 0f, 0f);
    private static int defaultPlanetTypeIndex;
    private static int selectedParentIndex;
    private static int selectedRemovalIndex;
    public static int selectedPivotIndex; // Separate index for camera pivot

    private static float mass = Constants.ROCKY_PLANET_MASS; // Initialize mass
    public static Vector3 color = Constants.ROCKY_PLANET_COLOR; // Use System.Numerics for ImGui
    public static List<Object> celestialBodies = new();

    // Use System.Numerics.Vector3 for ImGui ColorEdit3
    private static Vector3 IndicatorColor = Constants.INDICATOR_COLOR;
    private static readonly OpenTK.Mathematics.Vector3 DEFAULT_ROTATION = OpenTK.Mathematics.Vector3.Zero;
    private static readonly OpenTK.Mathematics.Vector3 DEFAULT_SCALE = OpenTK.Mathematics.Vector3.One * 0.1f;
    private static bool IsGUITransparent;

    public static uint selectedPlanetTypeRef => (uint)defaultPlanetTypeIndex;

    public static bool IsEditing =>
        !string.IsNullOrEmpty(nameBuffer) || // Check nameBuffer directly
        ImGui.IsAnyItemActive(); // Simplified check

    public void Dispose()
    {
        celestialBodies.Clear();
        GC.SuppressFinalize(this); // Standard Dispose pattern
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
                        ImGui.BulletText("3. For Moons: Select Parent from 'Orbits around' dropdown.");
                        ImGui.BulletText("4. For 'Custom' select color, and scale");
                        ImGui.BulletText("5. Click 'Create'.");
                        ImGui.Separator();
                        ImGui.Text("=== Camera Controls ===");
                        ImGui.BulletText("Mouse Wheel / Up/Down Arrows: Zoom");
                        ImGui.BulletText("Right Mouse Button + Drag: Orbit camera");
                        ImGui.BulletText("Spacebar + Drag: Orbit (Touchpad alternative)");
                        ImGui.BulletText("If you wish to centralize a planet in its movement, select it as a pivot");
                        ImGui.Separator();
                        ImGui.Text("=== Keyboard Shortcuts ===");
                        ImGui.BulletText("ESC: Quit Application");
                        ImGui.BulletText("C: Remove Last Added Object");
                        ImGui.Separator();
                        ImGui.Text("=== Notes ===");
                        ImGui.BulletText("Keyboard input inactive when GUI has focus.");
                        ImGui.BulletText("Green sphere indicates position during creation.");
                    }
                    finally
                    {
                        ImGui.EndTabItem();
                    }

                if (ImGui.BeginTabItem("Planet Creator"))
                    try
                    {
                        // Name Input
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

                        // Planet Type
                        ImGui.Text("Type:");
                        AddToolTip("Determines physical properties and behavior");
                        if (ImGui.Combo("##PlanetType", ref defaultPlanetTypeIndex, planetTypes, planetTypes.Length))
                        {
                            emissive = defaultPlanetTypeIndex == 1;
                            mass = defaultPlanetTypeIndex switch
                            {
                                0 => Constants.ROCKY_PLANET_MASS,
                                1 => Constants.STAR_MASS,
                                2 => Constants.GAS_GIANT_MASS,
                                3 => Constants.MOON_MASS,
                                4 => Constants.DESERT_MASS,
                                5 => Constants.ICE_GIANT_MASS,
                                6 => Constants.FLOAT_ONE,
                                _ => Constants.ROCKY_PLANET_MASS
                            };
                            massBuffer = mass.ToString(CultureInfo.InvariantCulture);

                            color = defaultPlanetTypeIndex switch
                            {
                                0 => Constants.ROCKY_PLANET_COLOR,
                                1 => Constants.STAR_COLOR,
                                2 => Constants.GAS_GIANT_COLOR,
                                3 => Constants.MOON_COLOR,
                                4 => Constants.DESERT_PLANET_COLOR,
                                5 => Constants.ICE_GIANT_COLOR,
                                6 => Constants.MOON_COLOR,
                                _ => Constants.ROCKY_PLANET_COLOR
                            };
                        }

                        // Custom Settings
                        if (defaultPlanetTypeIndex == Constants.planetTypes.Length - 1)
                        {
                            ImGui.Text("Radius:");
                            AddToolTip("Visual size only (doesn't affect physics)");

                            ImGui.SliderFloat("##CustomRadius", ref Constants.CUSTOM_RADIUS, 0.05f, 2f);

                            ImGui.Text("Color:");
                            AddToolTip("Visual size only (doesn't affect physics");

                            ImGui.ColorEdit3("##Color", ref color);
                        }

                        // Moon Parent Selection
                        if (defaultPlanetTypeIndex == 3)
                        {
                            ImGui.Text("Parent:");
                            AddToolTip("Select a parent body for a stable orbit right out of the box");
                            if (parentNames.Length > 0)
                            {
                                ImGui.Combo("##MoonParent", ref selectedParentIndex, parentNames, parentNames.Length);
                            }
                            else
                            {
                                ImGui.Text("No available parent bodies");
                                selectedParentIndex = -1;
                            }
                        }

                        // Position
                        ImGui.Text("Position:");
                        AddToolTip("Position relative to the X,Y intersection (red & blue lines)");
                        ImGui.DragFloat3("##Position", ref position, 0.2f, -Constants.GRID_SIZE, Constants.GRID_SIZE);

                        ImGui.Text("Velocity:");
                        AddToolTip(
                            "Velocity is calculated as the Norm of said vector and is applied to the object, ex: (0,10,10) => velocity = new Vec(sqrt(0^2 + 10^2 + 10^2)) ~ 14.4");
                        ImGui.DragFloat3("##Velocity", ref Velocity, 0.2f, -10f, 10f);

                        // Mass
                        ImGui.Text("Mass:");
                        AddToolTip("Mass in solar units ex. 1.4 ~ Star");
                        ImGui.SetNextItemWidth(Constants.BESPOKE_TEXTEDIT_WIDTH);
                        ImGui.InputText("##Mass", ref massBuffer, 15);

                        // Pause Toggle
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

                        // Removal Section
                        ImGui.SameLine();
                        string[] removalNames = celestialBodies.Select(b => b.Name).ToArray();

                        ImGui.Text("Remove:");
                        AddToolTip("Removes the selected celestial body");
                        if (removalNames.Length > 1)
                        {
                            if (selectedRemovalIndex >= removalNames.Length)
                                selectedRemovalIndex = removalNames.Length - 1;

                            ImGui.SetNextItemWidth(Constants.BESPOKE_TEXTEDIT_WIDE_WIDTH);
                            ImGui.Combo("##SelectToRemove", ref selectedRemovalIndex, removalNames,
                                removalNames.Length);
                            ImGui.SameLine();
                            if (ImGui.Button("Remove Selected", Constants.BESPOKE_BUTTON_SIZE))
                                if (selectedRemovalIndex >= 0 && selectedRemovalIndex < celestialBodies.Count)
                                {
                                    celestialBodies.RemoveAt(selectedRemovalIndex);
                                    Renderer.Spheres.RemoveAt(selectedRemovalIndex);
                                    selectedPivotIndex = celestialBodies.Count - 1;
                                    selectedRemovalIndex =
                                        Math.Clamp(selectedRemovalIndex, 0, celestialBodies.Count - 1);
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
                            ImGui.Text($"{body.Name} ({body.Type})");
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

        if (!float.TryParse(massBuffer, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedMass))
        {
            Logger.WriteLine("Error: Invalid mass value: " + massBuffer);
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


        var sphereScale = new OpenTK.Mathematics.Vector3(radius, radius, radius);
        var sphereColor = color; // Already System.Numerics.Vector3
        var sphereRotation = DEFAULT_ROTATION;
        float angularSpeed = Renderer.IsSimulationPaused ? 0f : // Force 0 speed when paused
            parent != null || orbitRadius > 0.001f ? DEFAULT_ANGULAR_SPEED : 0f;

        try
        {
#if DEBUG
            Logger.WriteLine($"\n" +
                             $"Creating new Sphere : {nameBuffer} \n " +
                             $"World pos :{worldPosition}\n" +
                             $"Rotation : {sphereRotation}\n" +
                             $"Scale :{sphereScale}\n" +
                             $"Color : {sphereColor}\n" +
                             $"Mass :{parsedMass}\n" +
                             $"Orbit radius: {orbitRadius}\n" +
                             $"Speed: {angularSpeed}\n" +
                             $"Type: {planetTypeName}\n" +
                             $"Emissive? :{emissive}\n" +
                             $"Parent ? :{parent}");
#endif

            return new Sphere(
                nameBuffer,
                worldPosition,
                sphereRotation,
                sphereScale,
                sphereColor,
                parsedMass,
                orbitRadius,
                angularSpeed,
                planetTypeName,
                emissive,
                parent,
                new OpenTK.Mathematics.Vector3(Velocity.X, Velocity.Y, Velocity.Z)
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
        massBuffer = Constants.DEFAULT_MASS_BUFFER;
        mass = Constants.ROCKY_PLANET_MASS;
        color = Constants.ROCKY_PLANET_COLOR;
        position = new Vector3(1f, 0f, 0f);
        defaultPlanetTypeIndex = 0;
        selectedParentIndex = -1; // Reset to invalid/none
        Velocity = Vector3.Zero;
    }
}