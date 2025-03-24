using OpenGL.Objects;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace OpenGL;

/// <summary>
/// Input register and handler, use it for mapping actions and logic to specific inputs
/// </summary>


// TODO : Modify the input for ImGui keyboard redirection
internal static class InputHandler
{
    private static Camera? _camera;
    private static Vector2 _lastMousePosition;

    private static readonly float mousePosDiv = 2f; 
    public static void InitializeInputs(GameWindow window, Camera camera)
    {
        _camera = camera;
        window.KeyDown += OnKeyDown;
        window.UpdateFrame += OnUpdateFrame;
        window.MouseMove += OnMouseMove;
        _lastMousePosition = new Vector2(window.Size.X / mousePosDiv, window.Size.Y / mousePosDiv);
    }

    private static void OnMouseMove(MouseMoveEventArgs e)
    {
        float deltaX = e.X - _lastMousePosition.X;
        float deltaY = e.Y - _lastMousePosition.Y;
        _lastMousePosition = new Vector2(e.X, e.Y);

        float sensitivity = _camera!.Sensitivity;
        _camera.Yaw += deltaX * sensitivity;
        _camera.Pitch -= deltaY * sensitivity; // Inverted Y axis
    }

    private static void OnKeyDown(KeyboardKeyEventArgs e)
    {
        if (e.Key == Keys.Escape)
        {
            Console.WriteLine("Closing window...");
            WindowManager.GetWindow().Close();
        }

        if (e.Key == Keys.G)
        {
            Console.WriteLine("Adding a sphere");


            var lastSphereAdded = Renderer.Spheres.LastOrDefault();

            var random = new Random();

            
            //TODO : Change the sphere position generation
            float x = (float)(random.NextDouble() * 2 - 1);
            float y = (float)(random.NextDouble() * 2 - 1);
            float z = 0;

            var pos = new Vector3(x, y, z);

            Renderer.AddObject(new Sphere(pos, Vector3.Zero, Vector3.One,
                lastSphereAdded!.Radius + Constants.INITIAL_SPHERE_RADIUS, Constants.INITIAL_SPHERE_VELOCITY));
        }

        if (e.Key == Keys.C)
        {
            Console.WriteLine("Removing last object added");
            if (Renderer.Spheres.Count > 1)
                Renderer.RemoveObject();
        }
    }

    private static void OnUpdateFrame(FrameEventArgs args)
    {
        var window = WindowManager.GetWindow();
        var input = window.KeyboardState;

        float cameraSpeed = _camera!.Speed * (float)args.Time;


        if (input.IsKeyDown(Keys.W)) // forward
            _camera.Position += _camera.Front * cameraSpeed;
        if (input.IsKeyDown(Keys.S)) // backward
            _camera.Position -= _camera.Front * cameraSpeed;

        if (input.IsKeyDown(Keys.A)) // left
            _camera.Position -= _camera.Right * cameraSpeed;
        if (input.IsKeyDown(Keys.D)) // right
            _camera.Position += _camera.Right * cameraSpeed;

        if (input.IsKeyDown(Keys.E)) // Vertical Up
            _camera.Position += _camera.Up * cameraSpeed;
        if (input.IsKeyDown(Keys.Q)) // Vertical Down
            _camera.Position -= _camera.Up * cameraSpeed;

        if (input.IsKeyDown(Keys.R)) // reset position to Vec(0,0,0)
            _camera.Position = Vector3.Zero;

        //Console.WriteLine($"{_camera.Position.X},{_camera.Position.Y},{_camera.Position.Z}");
    }
}