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
    private static GameWindow? _gameWindow;
    private static ImGuiController? _controller;
    
    private static string buffer = "0.0";
    private static bool submit = false;
    private static bool emissive = false;
    private static System.Numerics.Vector3 color;
    
    private void OnLoad()
    {
        _gameWindow = WindowManager.GetWindow();
        _controller = Renderer.GetController();
    }

    /// <summary>
    /// Setup your own GUI window in here, if you wish to set UI elements size set it before calling its constructor with SetWindowSize(..
    /// </summary>
    public static void SubmitUI()
    {
        // Create a window
        if (ImGui.Begin("Planet Creator"))
        {
            
            ImGui.SetNextItemWidth(Constants.BESPOKE_TEXTEDIT_WIDTH);
            //? True when changed
            if (ImGui.InputText("Kg", ref buffer, 15))
            { 
                Console.WriteLine("Input changed: " + buffer);
            }

            if (ImGui.ColorEdit3("Color", ref color))
            {
                
            }
            
            if (ImGui.Checkbox("Emissive", ref emissive))
            {
                emissive = true;
            }

            if (ImGui.Button("Create", Constants.BESPOKE_BUTTON_SIZE))
            {
                SaveUIValues();
                ResetUI();
                Renderer.AddObject(InputHandler.GenerateSphere(color));
                
            }
        }
        ImGui.End();
    }

    private static void SaveUIValues()
    {
        return;
    }
    private static void ResetUI()
    {
        emissive = false;
        buffer = Constants.BESPOKE_TEXT_DEFAULT;
    }

    public void Dispose(){}
}