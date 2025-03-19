using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGL;

internal static class Program
{
    private static unsafe void Main()
    {
        var windowSettings = GameWindowSettings.Default;
        var nativeWindowSettings = NativeWindowSettings.Default;
        
        
        //Program settings 
        nativeWindowSettings.ClientSize = new Vector2i(800, 600);
        nativeWindowSettings.Title = "C# GL";
        nativeWindowSettings.StartFocused = true;
        nativeWindowSettings.Vsync = VSyncMode.Off;
        //nativeWindowSettings.WindowState = WindowState.Fullscreen;
        
        
        var window = new GameWindow(windowSettings, nativeWindowSettings);
        
        //Mouse cursor settings
        window.Cursor = MouseCursor.Crosshair;
        
        
        // Initialize camera
        var camera = new Objects.Camera(new Vector3(-1, 3, 10));
        
        //Pass references
        InputHandler.InitializeInputs(window, camera);
        Renderer._camera = camera;
        
        
        WindowManager.Initialize(window);
        
        
        //Check for GL errors during startup initialization 
        WindowManager.CheckGlErrors();

        window.Run();
    }
}