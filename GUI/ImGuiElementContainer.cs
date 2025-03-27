using System.Drawing;
using System.Numerics;
using ImGuiNET;

namespace OpenGL.GUI;

internal abstract class ImGuiElementContainer : IDisposable
{
    private static readonly string[] planetTypes = new[] { "Planet", "Star", "Gas Giant", "Moon" };
    private static string buffer = "";
    private static string nameBuffer = "";
    private static bool emissive;
    private static float posFromStar;
    private static int defaultPlanetTypeIndex;
    private static int selectedParentIndex;
    private static float mass;
    private static Vector3 color = new(0.5f, 0.5f, 0.5f);
    private static readonly List<Object> celestialBodies = new();

    public void Dispose()
    {
        celestialBodies.Clear();
    }

    public static void SubmitUI()
    {
        if (ImGui.Begin("Planet Creator"))
        {
            ImGui.SetNextItemWidth(Constants.BESPOKE_TEXTEDIT_WIDTH);
            if (ImGui.InputText("Planet name", ref nameBuffer, 20)) Console.WriteLine("Input changed: " + nameBuffer);

            if (ImGui.Combo("Planet type", ref defaultPlanetTypeIndex, planetTypes, planetTypes.Length))
            {
                Console.WriteLine($"Selected planet type: {planetTypes[defaultPlanetTypeIndex]}");
                emissive = defaultPlanetTypeIndex == 1; // Stars are emissive (they emit light)

                mass = defaultPlanetTypeIndex switch
                {
                    //Earth
                    0 => Constants.ROCKY_PLANET_MASS,
                    //Star
                    1 => Constants.STAR_MASS,
                    //Gas Giant
                    2 => Constants.GAS_GIANT_MASS,
                    //Moon
                    3 => Constants.MOON_MASS,
                    //Desert planet (mars)
                    4 => Constants.DESERT_MASS,
                    //Ice giant (Nepture)
                    5 => Constants.ICE_GIANT_MASS,
                    //Default
                    _ => Constants.ROCKY_PLANET_MASS
                };
                buffer = mass.ToString();
            }

            // Show parent selector only for moons
            if (defaultPlanetTypeIndex == 3) // Moon
            {
                string[] parentNames = celestialBodies
                    .Where(b => b is Sphere && ((Sphere)b).Parent == null) // Only non-moon objects
                    .Select(b => b.Name)
                    .ToArray();

                if (parentNames.Length > 0)
                {
                    if (ImGui.Combo("Orbits around", ref selectedParentIndex, parentNames, parentNames.Length))
                        Console.WriteLine($"Selected parent: {parentNames[selectedParentIndex]}");
                }
                else
                {
                    ImGui.Text("No available parent bodies");
                }
            }

            if (ImGui.SliderFloat("Distance from center", ref posFromStar, 0f, 2f))
            {
                // Visual feedback could be added here
            }

            ImGui.SetNextItemWidth(Constants.BESPOKE_TEXTEDIT_WIDTH);
            if (ImGui.InputText("Mass cca 200-0.05", ref buffer, 15)) Console.WriteLine("Mass changed: " + buffer);

            ImGui.ColorEdit3("Color", ref color);

            if (ImGui.Checkbox("Emissive", ref emissive)) Console.WriteLine($"Emissive: {emissive}");

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

            // Display created objects
            ImGui.Separator();
            ImGui.Text("Created Objects:");
            foreach (var body in celestialBodies)
                ImGui.Text($"{body.Name} ({planetTypes[body is Sphere s && s.Parent != null ? 3 : 0]})");
        }

        ImGui.End();
    }

    /// <summary>
    /// Helper method for checking if a celestial body has a parent 'planet'
    /// </summary>
    /// <returns></returns>
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

        if (!float.TryParse(buffer, out float mass))
        {
            Console.WriteLine("Error: Invalid mass value");
            return null;
        }

        var position = OpenTK.Mathematics.Vector3.UnitX * posFromStar; 

        Object parent = null;
        if (defaultPlanetTypeIndex == 3 && celestialBodies.Count > 0) // Moon
        {
            List<Object>? potentialParents =
                celestialBodies.Where(b => b is Sphere && ((Sphere)b).Parent == null).ToList();
            if (selectedParentIndex < potentialParents.Count)
            {
                parent = potentialParents[selectedParentIndex];
                position = parent.Position + OpenTK.Mathematics.Vector3.UnitX * posFromStar;
            }
        }

        return new Sphere(
                name: nameBuffer,
                position: position,
                rotation: OpenTK.Mathematics.Vector3.Zero,
                scale: OpenTK.Mathematics.Vector3.One * Constants.INITIAL_SPHERE_RADIUS,
                color: color,
                mass: float.TryParse(buffer, out float massValue) ? massValue : 1.0f,
                orbitRadius: posFromStar,
                angularSpeed: Constants.INITIAL_SPHERE_VELOCITY,
                isEmissive: emissive,
                parent: defaultPlanetTypeIndex == 3 ? GetSelectedParent() : null
            );
        
    }
    
    
    private static void ResetUI()
    {
        nameBuffer = "";
        buffer = Constants.BESPOKE_TEXT_DEFAULT;
        color = new Vector3(0.5f, 0.5f, 0.5f);
        posFromStar = 1f;
        defaultPlanetTypeIndex = 0;
        selectedParentIndex = 0;
        emissive = false;
    }
}