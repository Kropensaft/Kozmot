using Silk.NET.Windowing;
using Silk.NET.Maths;

namespace C_Sharp_GL;

internal class Program
{
    private static unsafe void Main()
    {
        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(800, 600),
            Title = "C# GL",
            API = GraphicsAPI.Default
        };

        var window = Window.Create(options);
        WindowManager.Initialize(window);
        window.Run();
        
    }
}