using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace OpenGL;

internal static class InputHandler
{
    public static void InitializeInputs(GameWindow window)
    {
        window.KeyDown += KeyDown;
    }

    private static void KeyDown(KeyboardKeyEventArgs e)
    {
        if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape)
        {
            Console.WriteLine("Closing window...");
            WindowManager.GetWindow().Close();
        }

        if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space)
        {
            Console.WriteLine("Toggling simulation...");
            Renderer.Pause();
        }
    }
}