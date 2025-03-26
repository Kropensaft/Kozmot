using System.Drawing;
using System.Drawing.Design;
using System.Text;
using ImGuiNET;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static ImGuiNET.ImGuiNative;

#pragma warning disable IDE1006 // Naming Styles

namespace OpenGL.GUI;
internal abstract class ImGuiElementContainer : IDisposable
{
    
    private static string buffer = "0.0";
    private static string nameBuffer = "";
    private static bool emissive = false;
    private static float posFromStar = 0f;
    private static int defaultPlanetTypeIndex = 0;
    private static System.Numerics.Vector3 color;
    
    
    /// <summary>
    /// Setup your own GUI window in here, if you wish to set UI elements size set it before calling its constructor with SetWindowSize(..
    /// </summary>
    public static void SubmitUI()
    {
        // Create a window
        if (ImGui.Begin("Planet Creator"))
        {
            ImGui.SetNextItemWidth(Constants.BESPOKE_TEXTEDIT_WIDE_WIDTH);
            //? True when changed
            if (ImGui.InputText("Planet name", ref nameBuffer, 20))
            { 
                Console.WriteLine("Input changed: " + nameBuffer);
            }
            
            if (ImGui.Combo("Select a planet type", ref defaultPlanetTypeIndex, Constants.planetTypes, Constants.planetTypes.Length))
            {
                Console.WriteLine($"selected planet type: {defaultPlanetTypeIndex}");
                emissive = defaultPlanetTypeIndex == 1;
                
                // ? if the planet is a moon
                if (defaultPlanetTypeIndex == 3)
                {
                    //ImGui.Combo("Select a parent planet" , ref defaultPlanetTypeIndex , Renderer.Spheres)
                }
            }
            
            
            if (ImGui.SliderFloat("Distance from the 'center'", ref posFromStar, 0f, 2f))
            {
                //TODO : Render on screen position from the center (0,0,0) 
            }
            
            ImGui.SetNextItemWidth(Constants.BESPOKE_TEXTEDIT_WIDTH);
            //? True when changed
            if (ImGui.InputText("Kg", ref buffer, 15))
            { 
                Console.WriteLine("Input changed: " + buffer);
            }

            //? Color selection
            ImGui.ColorEdit3("Color", ref color);
            
            //TODO : Create light from emissive planets
            if (ImGui.Checkbox("Emissive", ref emissive))
            {
                emissive = true;
            }

            if (ImGui.Button("Create", Constants.BESPOKE_BUTTON_SIZE))
            {
                SaveUIValues();
                ResetUI();
                
                /*! After modifying the constructor properly get rid of the InputHandler call
                 ! and instead push an instance of a sphere which will be created as the elements are changed
                */
                Renderer.AddObject(InputHandler.GenerateSphere(color));
                
            }
        }
        ImGui.End();
    }

    private static Sphere SaveUIValues()
    {
        return null;
    }
    private static void ResetUI()
    {
        emissive = false;
        defaultPlanetTypeIndex = 0;
        buffer = Constants.BESPOKE_TEXT_DEFAULT;
    }

    public void Dispose(){}
}