using ImGuiNET;
using OpenGL.Objects;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace OpenGL;

internal static class InputHandler
{
    private static Camera? _camera;
    private static Vector2 _lastMousePosition;
    private static readonly float mousePosDiv = 2f;
    private static int _sphereCounter = 1;
    private static bool _isRightMouseDown = false;
    private static bool _isOrbitEnabled = false;

    public static void InitializeInputs(GameWindow window, Camera camera)
    {
        _camera = camera;
        window.KeyDown += OnKeyDown;
        window.UpdateFrame += OnUpdateFrame;
        window.MouseMove += OnMouseMove;
        window.MouseDown += OnMouseDown;
        window.MouseUp += OnMouseUp;
        _lastMousePosition = new Vector2(window.Size.X / mousePosDiv, window.Size.Y / mousePosDiv);
    }

    private static void OnMouseMove(MouseMoveEventArgs e)
    {
        var window = WindowManager.GetWindow();
        bool isSpaceDown = window.KeyboardState.IsKeyDown(Keys.Space);
        bool isRightDown = window.MouseState.IsButtonDown(MouseButton.Right);

        if (!isSpaceDown && !isRightDown) return;

        float deltaX = e.X - _lastMousePosition.X;
        float deltaY = e.Y - _lastMousePosition.Y;
        _lastMousePosition = new Vector2(e.X, e.Y);

        float sensitivity = _camera!.Sensitivity;
        _camera.Yaw += deltaX * sensitivity;
        _camera.Pitch -= deltaY * sensitivity; // Inverted Y axis
    }

    private static void OnMouseDown(MouseButtonEventArgs e)
    {
        if (e.Button == MouseButton.Right)
        {
            
            Vector3? clickedPos = ObjectPicker.GetClickedObjectPosition(
                _camera!,
                WindowManager.GetWindow().MouseState,
                e,
                WindowManager.GetWindow().Size,
                _camera.GetProjectionMatrix((float)WindowManager.GetWindow().Size.X / WindowManager.GetWindow().Size.Y) // Pass correct aspect ratio
            );

            if (clickedPos != null)
            {
                _camera!.Pivot = clickedPos.Value; // This triggers UpdateVectors()
                _lastMousePosition = new Vector2(WindowManager.GetWindow().MouseState.X, WindowManager.GetWindow().MouseState.Y); // Reset to avoid jumps
            }
        }
    }

    private static void OnMouseUp(MouseButtonEventArgs e)
    {
        if (e.Button == MouseButton.Right)
        {
            _isRightMouseDown = false;
        }
    }

    private static void OnKeyDown(KeyboardKeyEventArgs e)
    {
        if (ImGui.IsWindowFocused(ImGuiFocusedFlags.AnyWindow)) return;

        if(e.Key == Keys.Space)
            _isOrbitEnabled = !_isOrbitEnabled;
        
        if (e.Key == Keys.Escape)
        {
            Console.WriteLine("Closing window...");
            WindowManager.GetWindow().Close();
        }

        if (e.Key == Keys.G)
        {
            Console.WriteLine("Adding a sphere");
            Renderer.AddObject(GenerateSphere(System.Numerics.Vector3.One));
        }

        if (e.Key == Keys.C)
        {
            Console.WriteLine("Removing last object added");
            if (Renderer.Spheres.Count > 1)
                Renderer.RemoveObject();
        }

        // Zoom in/out with mouse wheel (optional)
        if (e.Key == Keys.Up)
        {
            _camera!.Distance -= 0.1f;
        }
        if (e.Key == Keys.Down)
        {
            _camera!.Distance += 0.1f;
        }
    }

    public static Sphere GenerateSphere(System.Numerics.Vector3 color)
    {
        var random = new Random();
        float x = (float)(random.NextDouble() * 2 - 1);
        float y = (float)(random.NextDouble() * 2 - 1);
        float z = 0;
        var pos = new Vector3(x, y, z);

        return new Sphere(
            name: $"Sphere {_sphereCounter++}",
            position: pos,
            rotation: Vector3.Zero,
            scale: Vector3.One,
            color: color,
            mass: 1.0f,
            orbitRadius: Constants.DEFAULT_ORBIT_RADIUS,
            angularSpeed: Constants.INITIAL_SPHERE_VELOCITY
        );
    }

    private static void OnUpdateFrame(FrameEventArgs args)
    {
        var window = WindowManager.GetWindow();
        var input = window.KeyboardState;
        float cameraSpeed = _camera!.Speed * (float)args.Time;
    }
}