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
    private static int defaultPlanetTypeIndex;
    private static int selectedParentIndex;
    private static int selectedPivotIndex; // Separate index for camera pivot

    private static float mass = Constants.ROCKY_PLANET_MASS; // Initialize mass
    public static Vector3 color = Constants.ROCKY_PLANET_COLOR; // Use System.Numerics for ImGui
    public static List<Object> celestialBodies = new();

    // Use System.Numerics.Vector3 for ImGui ColorEdit3
    private static Vector3 IndicatorColor = Constants.INDICATOR_COLOR;
    private static readonly OpenTK.Mathematics.Vector3 DEFAULT_ROTATION = OpenTK.Mathematics.Vector3.Zero;
    private static readonly OpenTK.Mathematics.Vector3 DEFAULT_SCALE = OpenTK.Mathematics.Vector3.One * 0.1f;

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
        // Get names of potential parents (parentless Spheres)
        string[] parentNames = celestialBodies
            .OfType<Sphere>()
            .Where(s => s.Parent == null)
            .Select(s => s.Name)
            .ToArray();

        // Get names of all celestial bodies for pivot selection
        string[] allBodyNames = celestialBodies
            .Select(b => b.Name)
            .ToArray();


        if (!ImGui.Begin("GUI", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar))
        {
            ImGui.End(); // Make sure to End() even if Begin() returns false
            return;
        }

        try
        {
            if (ImGui.BeginTabBar("Settings#left"))
            {
                // Planet Creator Tab
                if (ImGui.BeginTabItem("Planet Creator"))
                    try
                    {
                        ImGui.SetNextItemWidth(Constants.BESPOKE_TEXTEDIT_WIDE_WIDTH);
                        if (ImGui.InputText("Planet name", ref nameBuffer, 20))
                        {
                        } // No action needed on change here

                        if (ImGui.Combo("Planet type", ref defaultPlanetTypeIndex, planetTypes, planetTypes.Length))
                        {
                            emissive = defaultPlanetTypeIndex == 1; // Index 1 is Star

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
                            massBuffer = mass.ToString(CultureInfo.InvariantCulture); // Use invariant culture

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

                        // Moon parent selection
                        if (defaultPlanetTypeIndex == 3) // Index 3 is Moon
                        {
                            if (parentNames.Length > 0)
                            {
                                // Use selectedParentIndex for moon parent choice
                                if (ImGui.Combo("Orbits around", ref selectedParentIndex, parentNames,
                                        parentNames.Length))
                                {
                                } // No action needed on change here
                            }
                            else
                            {
                                ImGui.Text("No available parent bodies (create a non-moon first)");
                                selectedParentIndex = -1; // Indicate no valid selection
                            }
                        }
                        else
                        {
                            selectedParentIndex = -1; // Reset if not a moon
                        }


                        if (ImGui.DragFloat3("Position (Relative)", ref position, 0.01f, Constants.SLIDER_CLAMP_MIN,
                                Constants.SLIDER_CLAMP_MAX))
                        {
                        } // No action needed on change here

                        ImGui.SetNextItemWidth(Constants.BESPOKE_TEXTEDIT_WIDTH);
                        if (ImGui.InputText("Mass (e.g., 1.0)", ref massBuffer, 15))
                        {
                        } // No action needed on change here

                        ImGui.ColorEdit3("Color", ref color);

                        // Ensure emissive checkbox reflects current type unless manually changed
                        if (ImGui.Checkbox("Emissive", ref emissive))
                        {
                        } // Allow manual override

                        if (ImGui.Button("Create", Constants.BESPOKE_BUTTON_SIZE))
                        {
                            var newSphere = SaveUIValues();
                            if (newSphere != null)
                            {
                                // Assuming Renderer has a method like AddObject
                                Renderer.AddObject(newSphere); // Replace with your actual method
                                celestialBodies.Add(newSphere);
                                ResetUI(); // Reset after successful creation
                            }
                            // Error messages are printed within SaveUIValues
                        }

                        ImGui.Separator();
                        ImGui.Text("Created Objects:");
                        foreach (var body in celestialBodies)
                            // Display using the Type stored in the object
                            ImGui.Text($"{body.Name} ({body.Type})");
                    }
                    finally
                    {
                        ImGui.EndTabItem();
                    }

                // How to Use Tab
                if (ImGui.BeginTabItem("How to use"))
                    try
                    {
                        ImGui.Text("=== Planet Creator Guide ===");
                        ImGui.BulletText("1. Enter Name, Select Type.");
                        ImGui.BulletText("2. Set Position (relative to parent for moons, else origin).");
                        ImGui.BulletText("3. For Moons: Select Parent from 'Orbits around' dropdown.");
                        ImGui.BulletText("4. Adjust Mass, Color, Emissive properties.");
                        ImGui.BulletText("5. Click 'Create'.");
                        ImGui.Separator();
                        ImGui.Text("=== Camera Controls ===");
                        ImGui.BulletText("Mouse Wheel / Up/Down Arrows: Zoom");
                        ImGui.BulletText("Right Mouse Button + Drag: Orbit camera");
                        ImGui.BulletText("Spacebar + Left Mouse Button + Drag: Orbit (Touchpad alt)");
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

                // Camera settings tab
                if (ImGui.BeginTabItem("Camera settings"))
                    try
                    {
                        ImGui.Text("Select a central pivot for the camera");
                        ImGui.Separator();

                        if (allBodyNames.Length > 0)
                        {
                            // Use selectedPivotIndex for camera pivot choice
                            ImGui.Combo("Camera Pivot Object", ref selectedPivotIndex, allBodyNames,
                                allBodyNames.Length);

                            if (ImGui.Button("Set Camera Pivot"))
                            {
                                if (selectedPivotIndex >= 0 && selectedPivotIndex < celestialBodies.Count)
                                {
                                    Console.WriteLine(
                                        $"Setting camera pivot to: {celestialBodies[selectedPivotIndex].Name}");
                                    Camera._pivot =
                                        celestialBodies[selectedPivotIndex]
                                            .Position; // Assuming Camera._pivot exists and is OpenTK.Vector3
                                }
                                else
                                {
                                    Console.WriteLine("Invalid pivot selection.");
                                }
                            }
                        }
                        else
                        {
                            ImGui.Text("No objects created to pivot around.");
                        }


                        ImGui.Separator();
                        ImGui.Text("=== Indicator sphere settings ===");

                        if (ImGui.Checkbox("Render Grid", ref Grid.RenderGrid))
                        {
                        }

                        if (ImGui.Checkbox("Render Indicator", ref Renderer.RenderIndicator))
                        {
                        }

                        if (ImGui.ColorEdit3("Indicator Color", ref IndicatorColor))
                            // Update the actual constant color used by the indicator renderer
                            Constants.INDICATOR_COLOR = Constants.INDICATOR_COLOR;

                        if (ImGui.SliderFloat("Indicator Transparency", ref Constants.INDICATOR_ALPHA, 0.0f, 0.9f))
                        {
                        } // Assuming Constants.INDICATOR_ALPHA exists
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


    private static Sphere? SaveUIValues()
    {
        if (string.IsNullOrWhiteSpace(nameBuffer))
        {
            Console.WriteLine("Error: Name cannot be empty");
            return null;
        }

        if (!float.TryParse(massBuffer, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedMass))
        {
            Console.WriteLine("Error: Invalid mass value: " + massBuffer);
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
                Console.WriteLine(
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
            default: // Fallback to default rocky planet size
                Console.WriteLine(
                    $"Warning: Unknown planet type index {defaultPlanetTypeIndex}. Defaulting to Rocky Planet radius.");
                radius = Constants.ROCKY_PLANET_RADIUS;
                break;
        }

        var sphereScale = new OpenTK.Mathematics.Vector3(radius, radius, radius);
        var sphereColor = color; // Already System.Numerics.Vector3
        var sphereRotation = DEFAULT_ROTATION;
        float angularSpeed =
            parent != null || orbitRadius > 0.001f
                ? DEFAULT_ANGULAR_SPEED
                : 0f; // Only orbit if parent or not at origin

        try
        {
#if DEBUG
            Console.WriteLine($"\n" +
                              $"Creating new Sphere : {nameBuffer} \n " +
                              $"World pos :{worldPosition}\n" +
                              $"Rotation : {sphereRotation}\n" +
                              $"Scale :{sphereScale}\n" +
                              $"Color : {sphereColor}\n" +
                              $"Mass :{parsedMass}\n" +
                              $"Orbit radius: {orbitRadius}\n" +
                              $"Speed: {angularSpeed}\n" +
                              $"Name: {planetTypeName}\n" +
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
                parent
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating Sphere: {ex.Message}");
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
        selectedPivotIndex = 0; // Reset pivot selection
        emissive = false;
    }
}