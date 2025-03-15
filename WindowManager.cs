using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Desktop;
using ErrorCode = OpenTK.Graphics.OpenGL.ErrorCode;

namespace OpenGL;

internal static class WindowManager
{
    private static GameWindow _window;

    public static void Initialize(GameWindow window)
    {
        _window = window;

        // Register event handlers
        _window.Load += Renderer.OnLoad;
        _window.UpdateFrame += Renderer.OnUpdate;
    }

    public static void CheckGLErrors()
    {
        ErrorCode error = GL.GetError();
        if (error != ErrorCode.NoError)
        {
            Console.WriteLine($"OpenGL Error: {error}");
        }
    }

    public static GameWindow GetWindow() => _window;
}