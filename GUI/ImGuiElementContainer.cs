using System;
using ImGuiNET;
using OpenTK.Mathematics;
using System.Numerics;

namespace OpenGL.GUI;

internal abstract class ImGuiElementContainer : IDisposable
{
    private static string[] planetTypes = new[] { "Planet", "Star", "Gas Giant", "Moon" };
    private static string buffer = "0.0";
    private static string nameBuffer = "";
    private static bool emissive = false;
    private static float posFromStar = 0f;
    private static int defaultPlanetTypeIndex = 0;
    private static int selectedParentIndex = 0;
    private static System.Numerics.Vector3 color = new System.Numerics.Vector3(0.5f, 0.5f, 0.5f);
    private static List<Object> celestialBodies = new();

    public static void SubmitUI()
    {
        if (ImGui.Begin("Planet Creator"))
        {
            ImGui.SetNextItemWidth(Constants.BESPOKE_TEXTEDIT_WIDTH);
            if (ImGui.InputText("Planet name", ref nameBuffer, 20))
            {
                Console.WriteLine("Input changed: " + nameBuffer);
            }

            if (ImGui.Combo("Planet type", ref defaultPlanetTypeIndex, planetTypes, planetTypes.Length))
            {
                Console.WriteLine($"Selected planet type: {planetTypes[defaultPlanetTypeIndex]}");
                emissive = defaultPlanetTypeIndex == 1; // Stars are emissive
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
                    {
                        Console.WriteLine($"Selected parent: {parentNames[selectedParentIndex]}");
                    }
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
            if (ImGui.InputText("Mass (kg)", ref buffer, 15))
            {
                Console.WriteLine("Mass changed: " + buffer);
            }

            ImGui.ColorEdit3("Color", ref color);

            if (ImGui.Checkbox("Emissive", ref emissive))
            {
                Console.WriteLine($"Emissive: {emissive}");
            }

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
            {
                ImGui.Text($"{body.Name} ({planetTypes[body is Sphere s && s.Parent != null ? 3 : 0]})");
            }
        }
        ImGui.End();
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

        OpenTK.Mathematics.Vector3 position = OpenTK.Mathematics.Vector3.UnitX * posFromStar;
        OpenTK.Mathematics.Vector3 scale = OpenTK.Mathematics.Vector3.One * Constants.INITIAL_SPHERE_RADIUS;
        OpenTK.Mathematics.Vector3 rotation = OpenTK.Mathematics.Vector3.Zero;

        Object parent = null;
        if (defaultPlanetTypeIndex == 3 && celestialBodies.Count > 0) // Moon
        {
            var potentialParents = celestialBodies.Where(b => b is Sphere && ((Sphere)b).Parent == null).ToList();
            if (selectedParentIndex < potentialParents.Count)
            {
                parent = potentialParents[selectedParentIndex];
                position = parent.Position + OpenTK.Mathematics.Vector3.UnitX * posFromStar;
            }
        }

        var newSphere = new Sphere(
            name: nameBuffer,
            position: position,
            rotation: rotation,
            scale: scale,
            color: color,
            mass: mass,
            orbitRadius: posFromStar,
            speed: Constants.INITIAL_SPHERE_VELOCITY,
            isEmissive: emissive,
            parent: parent
        );

        Console.WriteLine($"Created new {planetTypes[defaultPlanetTypeIndex]}: {newSphere.Name}");
        return newSphere;
    }

    private static void ResetUI()
    {
        nameBuffer = "";
        buffer = Constants.BESPOKE_TEXT_DEFAULT;
        color = new System.Numerics.Vector3(0.5f, 0.5f, 0.5f);
        posFromStar = 0f;
        defaultPlanetTypeIndex = 0;
        selectedParentIndex = 0;
        emissive = false;
    }

    public void Dispose()
    {
        celestialBodies.Clear();
    }
}