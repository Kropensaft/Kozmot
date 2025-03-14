using Silk.NET.Windowing;
using Silk.NET.Maths;

namespace C_Sharp_GL;

internal class Program
{
    private static IWindow _window;

    private static void Main()
    {
        // Window options
        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(800, 600),
            Title = "C# GL"
        };

        _window = Window.Create(options);

        // Initialize core functionalities
        WindowManager.Initialize(_window);

        // Run the application
        _window.Run();
    }
}