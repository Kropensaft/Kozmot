using System.ComponentModel;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Desktop;
using ErrorCode = OpenTK.Graphics.OpenGL.ErrorCode;

namespace OpenGL;

/// <summary>
///     Class responsible for subscribing to internal events, resource cleanup management, GL-error check sequence and
///     reference passing to outer namespaces
/// </summary>
internal static class WindowManager
{
    private static GameWindow? _window;

    public static void Initialize(GameWindow window)
    {
        _window = window;
        // Register event handlers
        _window.Load += Renderer.OnLoad;
        _window.UpdateFrame += Renderer.OnUpdate;
        _window.Closing += OnWindowClosing;
    }

    private static void
        OnWindowClosing(
            CancelEventArgs e) //CancelEventArgs is unused but is present simply so we can subscribe to the event
    {
        Renderer.ResourceCleanup();
    }

    public static void CheckGlErrors()
    {
        Console.WriteLine("Starting error checking sequence...");
        var error = GL.GetError();
        Console.WriteLine(error != ErrorCode.NoError ? $"OpenGL Error: {error}" : "No errors detected");
    }

    //Function to return a reference to _window from other files
    public static GameWindow GetWindow()
    {
        return _window!;
    }
}