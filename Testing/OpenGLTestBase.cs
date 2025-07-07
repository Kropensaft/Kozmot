using OpenTK.Windowing.Desktop;
using NUnit.Framework;
using OpenTK.Windowing.Common;

[TestFixture]
public class OpenGLTestBase
{
    protected GameWindow Window { get; private set; }

    [OneTimeSetUp]
    public void Setup()
    {
        // Configure settings for a headless window
        var settings = new NativeWindowSettings
        {
            StartVisible = false,    // No visible window
            API = ContextAPI.OpenGL,
            Profile = ContextProfile.Core,
            Flags = ContextFlags.Default
        };

        // Create a hidden GameWindow to initialize OpenGL
        Window = new GameWindow(GameWindowSettings.Default, settings);
        Window.MakeCurrent(); // Activate the OpenGL context
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Window?.Dispose(); // Clean up context
    }
}