using System.Drawing;
using System.Drawing.Design;
using System.Text;
using ImGuiNET;
using OpenTK;
using OpenTK.Windowing.Desktop;
using static ImGuiNET.ImGuiNative;

#pragma warning disable IDE1006 // Naming Styles

namespace OpenGL.GUI;
internal abstract class ImGuiElementContainer : IDisposable
{
    private static GameWindow? _gameWindow;
    private static ImGuiController? _controller;
    
    
    public static bool _buttonClicked; // Track button clicks
    

    private void OnLoad()
    {
        _gameWindow = WindowManager.GetWindow();
        _controller = Renderer.GetController();
    }


    /// <summary>
    /// Setup your own GUI window in here
    /// </summary>
    public static unsafe void SubmitUI()
    {
        // Create a window
        if (ImGui.Begin("My Window"))
        {
            if (ImGui.Button("Submit"))
            {
              
            }

        }
        ImGui.End();
    }

    

    public void Dispose(){}
    
    
}