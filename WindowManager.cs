using Silk.NET.Windowing;

namespace C_Sharp_GL;

internal static class WindowManager
{
    private static IWindow _window;

    public static void Initialize(IWindow window)
    {
        _window = window;

        // Register event handlers
        _window.Load += Renderer.OnLoad;
        _window.Update += Renderer.OnUpdate;
        _window.Render += Renderer.OnRender;
    }

    public static IWindow GetWindow() => _window;
}