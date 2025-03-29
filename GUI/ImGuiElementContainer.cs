using System.Globalization;
using System.Numerics;
using ImGuiNET;


namespace OpenGL.GUI;

internal abstract class ImGuiElementContainer : IDisposable
{
    private static string[] planetTypes = Constants.planetTypes;
    private static string massBuffer = "";
    private static string nameBuffer = "";
    private static bool emissive;
    private static Vector3 position = new(1f, 0f, 0f); // Replaced posFromStar with Vector3 position
    private static int defaultPlanetTypeIndex;
    private static int selectedParentIndex;
    private static float mass;
    private static Vector3 color = new(0.5f, 0.5f, 0.5f);
    public static List<Object> celestialBodies = new();

    public void Dispose()
    {
        celestialBodies.Clear();
    }

    public static void SubmitUI()
    {
        string[] parentNames = celestialBodies
            .Where(b => b is Sphere && ((Sphere)b).Parent == null)
            .Select(b => b.Name)
            .ToArray();
        
        if (!ImGui.Begin("GUI", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar))
            return;

        try
        {
            if (ImGui.BeginTabBar("Settings#left"))
            {
                // Planet Creator Tab
                if (ImGui.BeginTabItem("Planet Creator"))
                {
                    try
                    {
                        ImGui.SetNextItemWidth(Constants.BESPOKE_TEXTEDIT_WIDE_WIDTH);
                        if (ImGui.InputText("Planet name", ref nameBuffer, 20))
                            Console.WriteLine("Input changed: " + nameBuffer);

                        if (ImGui.Combo("Planet type", ref defaultPlanetTypeIndex, planetTypes, planetTypes.Length))
                        {
                            Console.WriteLine($"Selected planet type: {planetTypes[defaultPlanetTypeIndex]}");
                            emissive = defaultPlanetTypeIndex == 1;

                            mass = defaultPlanetTypeIndex switch
                            {
                                0 => Constants.ROCKY_PLANET_MASS,
                                1 => Constants.STAR_MASS,
                                2 => Constants.GAS_GIANT_MASS,
                                3 => Constants.MOON_MASS,
                                4 => Constants.DESERT_MASS,
                                5 => Constants.ICE_GIANT_MASS,
                                _ => Constants.ROCKY_PLANET_MASS
                            };
                            massBuffer = mass.ToString();

                            color = defaultPlanetTypeIndex switch
                            {
                                0 => Constants.ROCKY_PLANET_COLOR,
                                1 => Constants.STAR_COLOR,
                                2 => Constants.GAS_GIANT_COLOR,
                                3 => Constants.MOON_COLOR,
                                4 => Constants.DESERT_PLANET_COLOR,
                                5 => Constants.ICE_GIANT_COLOR,
                                _ => Constants.ROCKY_PLANET_COLOR
                            };
                        }

                        if (defaultPlanetTypeIndex == 3)
                        {

                            if (parentNames.Length > 0)
                            {
                                if (ImGui.Combo("Orbits around", ref selectedParentIndex, parentNames,
                                        parentNames.Length))
                                    Console.WriteLine($"Selected parent: {parentNames[selectedParentIndex]}");
                            }
                            else
                            {
                                ImGui.Text("No available parent bodies");
                            }
                        }

                        if (ImGui.DragFloat3("Position", ref position, 0.01f, -10f, 10f))
                            Console.WriteLine($"Position changed: X={position.X}, Y={position.Y}, Z={position.Z}");

                        ImGui.SetNextItemWidth(Constants.BESPOKE_TEXTEDIT_WIDTH);
                        if (ImGui.InputText("Mass cca 200-0.05", ref massBuffer, 15))
                            Console.WriteLine("Mass changed: " + massBuffer);

                        ImGui.ColorEdit3("Color", ref color);

                        if (ImGui.Checkbox("Emissive", ref emissive))
                            Console.WriteLine($"Emissive: {emissive}");

                        if (ImGui.Button("Create", Constants.BESPOKE_BUTTON_SIZE))
                        {
                            var newSphere = SaveUIValues();
                            if (newSphere != null)
                            {
                                Renderer.AddObject(newSphere);
                                celestialBodies.Add(newSphere);
                            }

                            ResetUI();
                        }

                        ImGui.Separator();
                        ImGui.Text("Created Objects:");
                        foreach (var body in celestialBodies)
                            ImGui.Text($"{body.Name} ({planetTypes[defaultPlanetTypeIndex]})");
                    }
                    finally
                    {
                        ImGui.EndTabItem();
                    }
                }

                // How to Use Tab
                if (ImGui.BeginTabItem("How to use"))
                {
                    try
                    {
                        ImGui.Text("=== Planet Creator Guide ===");
                        ImGui.BulletText("1. Select a planet type from dropdown");
                        ImGui.BulletText("2. Set a position using the XYZ coordinates");
                        ImGui.BulletText("3. For moons, select a parent body");
                        ImGui.BulletText("4. Adjust the mass and color parameters");
                        ImGui.BulletText("5. Click 'Create' to spawn the object");

                        ImGui.Separator();
                        ImGui.Text("=== Keyboard and orientation ===");
                        ImGui.BulletText("Input is handled through keyboard and mouse / touchpad");
                        ImGui.Text("=== Zoom ===");
                        ImGui.BulletText("zoom using your mousewheel / touchpad or the up and down arrow keys");
                        ImGui.Text("=== Rotating around objects ===");
                        ImGui.BulletText("Use your right mouse button or the left mouse button and the spacebar");
                        ImGui.Text("=== Keyboard shortcuts ===");
                        ImGui.BulletText("Esc - Quit the application");
                        ImGui.BulletText("C - Remove the last object added");
                        ImGui.BulletText("Up / Down - Zoom the camera in or out");
                        ImGui.BulletText("Spacebar - (for touchpads) use it to rotate");
                        ImGui.Separator();
                        ImGui.Text("=== Disclaimer ===");
                        ImGui.BulletText("Keyboard input won't work if the UI window is focused");
                    }
                    finally
                    {
                        ImGui.EndTabItem();
                    }
                }

                //Camera settings tab
                if (ImGui.BeginTabItem("Camera settings"))
                {
                    try
                    {
                        ImGui.Text("Select a central pivot for the camera");
                        ImGui.Separator();
                        if (ImGui.Combo("Celestial objects", ref selectedParentIndex, parentNames,
                                parentNames.Length))
                            Console.WriteLine($"Selected pivot: {parentNames[selectedParentIndex]}");
                        {
                            Console.WriteLine(planetTypes[defaultPlanetTypeIndex]);
                        }

                        if (ImGui.Button("Switch pivots"))
                        {
                            Console.WriteLine($"Selected pivot: {selectedParentIndex}, changing...");
                        }
                    }
                    finally
                    {
                        ImGui.EndTabItem();
                    }
                }

                ImGui.EndTabBar();
            }
        }
        finally
        {
            ImGui.End();
        }
    }

    private static Object? GetSelectedParent()
    {
        if (selectedParentIndex >= 0 && selectedParentIndex < celestialBodies.Count)
            return celestialBodies[selectedParentIndex];
        return null;
    }

    private static Sphere SaveUIValues()
    {
        if (string.IsNullOrWhiteSpace(nameBuffer))
        {
            Console.WriteLine("Error: Name cannot be empty");
            return null;
        }

        if (!float.TryParse(massBuffer, NumberStyles.Float, CultureInfo.CurrentCulture, out float parsedMass))
        {
            Console.WriteLine("Error: Invalid mass value");
            return null;
        }

        // Convert System.Numerics.Vector3 to OpenTK.Mathematics.Vector3
        var openTkPosition = new OpenTK.Mathematics.Vector3(
            position.X,
            position.Y,
            position.Z
        );

        Object parent = null;
        if (defaultPlanetTypeIndex == 3 && celestialBodies.Count > 0) // Moon
        {
            List<Sphere> potentialParents = celestialBodies
                .Where(b => b is Sphere && ((Sphere)b).Parent == null)
                .Cast<Sphere>()
                .ToList();

            if (selectedParentIndex < potentialParents.Count)
            {
                parent = potentialParents[selectedParentIndex];
                // Add parent's OpenTK position to moon's position
                openTkPosition += potentialParents[selectedParentIndex].Position;
            }
        }

        return new Sphere(
            nameBuffer,
            openTkPosition, // OpenTK Vector3
            OpenTK.Mathematics.Vector3.Zero,
            OpenTK.Mathematics.Vector3.One * Constants.INITIAL_SPHERE_RADIUS,
            color,
            parsedMass,
            openTkPosition.Length, // Use OpenTK's Vector3 Length property
            Constants.INITIAL_SPHERE_VELOCITY,
            emissive,
            defaultPlanetTypeIndex == 3 ? GetSelectedParent() : null
        );
    }

    public static void ResetUI()
    {
        nameBuffer = Constants.DEFAULT_NAME_BUFFER;
        massBuffer = Constants.DEFAULT_MASS_BUFFER;
        mass = Constants.ROCKY_PLANET_MASS;
        color = Constants.ROCKY_PLANET_COLOR;
        position = new Vector3(1f, 0f, 0f); // Reset to default position
        defaultPlanetTypeIndex = 0;
        selectedParentIndex = 0;
        emissive = false;
    }
}