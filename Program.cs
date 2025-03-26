using System.Diagnostics;
using OpenGL.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;


// Constants located in Constants.cs

namespace OpenGL;

internal static class Program
{
    /// <summary>
    ///     Entry-point of the program, used for setting up the window parameters a camera and calling all main functions of
    ///     scripts
    /// </summary>
    private static void Main()
    {
        
        var windowSettings = GameWindowSettings.Default;
        var nativeWindowSettings = NativeWindowSettings.Default;


        //Program settings 
        nativeWindowSettings.ClientSize = new Vector2i(Constants.SCREEN_WIDTH, Constants.SCREEN_HEIGHT);
        nativeWindowSettings.Title = "C# GL";
        nativeWindowSettings.StartFocused = true;
        nativeWindowSettings.Vsync = VSyncMode.Off;
        nativeWindowSettings.WindowState = Debugger.IsAttached ? WindowState.Fullscreen : WindowState.Fullscreen;


        var window = new GameWindow(windowSettings, nativeWindowSettings);

        //Mouse cursor settings
        window.Cursor = MouseCursor.Crosshair;


        // Initialize camera
        var camera = new Camera(Constants.INITIAL_CAMERA_POS);

        //Pass references
        InputHandler.InitializeInputs(window, camera);
        Renderer._camera = camera;


        WindowManager.Initialize(window);


        //Check for GL errors during startup initialization 
        WindowManager.CheckGlErrors();

        window.Run();
    }
    
}