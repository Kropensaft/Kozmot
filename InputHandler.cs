using System.Numerics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace OpenGL;

internal static class InputHandler
{
    private static Objects.Camera? _camera;
    private static Vector2 _lastMousePosition;

    public static void InitializeInputs(GameWindow window, Objects.Camera camera)
    {
        _camera = camera;
        window.KeyDown += OnKeyDown;
        window.UpdateFrame += OnUpdateFrame;
        window.MouseMove += OnMouseMove;
        _lastMousePosition = new Vector2(window.Size.X / 2f, window.Size.Y / 2f);
    }

    private static void OnMouseMove(MouseMoveEventArgs e)
    {
        float deltaX = e.X - _lastMousePosition.X;
        float deltaY = e.Y - _lastMousePosition.Y;
        _lastMousePosition = new Vector2(e.X, e.Y);

        float sensitivity = _camera.Sensitivity;
        _camera.Yaw += deltaX * sensitivity;
        _camera.Pitch -= deltaY * sensitivity; // Inverted Y axis
    }

    private static void OnKeyDown(KeyboardKeyEventArgs e)
    {
        if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape)
        {
            Console.WriteLine("Closing window...");
            WindowManager.GetWindow().Close();
        }

        if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.G)
        {
            Console.WriteLine("Adding a sphere");
            
            Random random = new Random();
            
            float x = (float)(random.NextDouble() * 2 - 1);
            float y = (float)(random.NextDouble() * 2 - 1);
            float z = (float)(random.NextDouble() * 2 - 1);
            
            Vector3 pos = new Vector3(x,y,z);
            
            Renderer.AddObject(new Sphere(pos, Vector3.Zero, Vector3.One));

        }

        if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.C)
        {
            Console.WriteLine("Removing last object added");
            Renderer.RemoveObject();
        }
    }

    private static void OnUpdateFrame(FrameEventArgs args)
    {
        var window = WindowManager.GetWindow();
        var input = window.KeyboardState;

        float cameraSpeed = _camera.Speed * (float)args.Time;
        

        if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W)) // forward
            _camera.Position += _camera.Front * cameraSpeed;
        if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S)) // backward
            _camera.Position -= _camera.Front * cameraSpeed;
        
        if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A)) // left
            _camera.Position -= _camera.Right * cameraSpeed;
        if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D)) // right
            _camera.Position += _camera.Right * cameraSpeed;
        
        if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.E)) // Vertical Up
            _camera.Position += _camera.Up * cameraSpeed;
        if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Q)) // Vertical Down
            _camera.Position -= _camera.Up * cameraSpeed;
        
        if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.R)) // reset position to Vec(0,0,0)
            _camera.Position = Vector3.Zero;
            
        
        //Console.WriteLine($"Camera X = {_camera.Position.X} Y = {_camera.Position.Y}");
    }
}