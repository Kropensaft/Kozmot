using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace OpenGL;

internal class Program
{
    private static unsafe void Main()
    {
        var windowSettings = GameWindowSettings.Default;
        var nativeWindowSettings = NativeWindowSettings.Default;
        nativeWindowSettings.ClientSize = new Vector2i(800, 600);
        nativeWindowSettings.Title = "C# GL - FPS: 0";
        var window = new GameWindow(windowSettings, nativeWindowSettings);
        
        WindowManager.Initialize(window);
        window.Run();
        
    }
}