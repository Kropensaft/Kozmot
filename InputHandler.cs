using ImGuiNET;
using OpenGL.GUI;
using OpenGL.Objects;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector2 = OpenTK.Mathematics.Vector2;

namespace OpenGL;

/// <summary>
///     Event based subscriber for inputs
/// </summary>
internal static class InputHandler
{
    private static Camera? _camera;
    private static Vector2 _lastMousePosition;
    private static DateTime _lastWheelLog = DateTime.MinValue;
    private static readonly float mousePosDiv = 2f;
    private static bool _isOrbitEnabled;
    private static bool? _isRightMouseDown { get; set; }


    public static void InitializeInputs(GameWindow window, Camera camera)
    {
        _camera = camera;
        window.KeyDown += OnKeyDown;
        window.UpdateFrame += OnUpdateFrame;
        window.MouseMove += OnMouseMove;
        window.MouseWheel += OnMouseWheel;
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


    private static void OnMouseWheel(MouseWheelEventArgs e)
    {
        var logInterval = TimeSpan.FromMilliseconds(Constants.LOG_INTERVAL_MS);

        if (e.OffsetY > 0)
        {
            if (DateTime.Now - _lastWheelLog > logInterval)
            {
                Logger.WriteLine("Mouse wheel scrolled UP");
                _lastWheelLog = DateTime.Now;
            }

            _camera!.Distance -= Constants.CAMERA_ZOOM_SENSITIVITY;
        }
        else if (e.OffsetY < 0)
        {
            if (DateTime.Now - _lastWheelLog > logInterval)
            {
                Logger.WriteLine("Mouse wheel scrolled UP");
                _lastWheelLog = DateTime.Now;
            }

            _camera!.Distance += Constants.CAMERA_ZOOM_SENSITIVITY;
        }
    }

    private static void OnMouseUp(MouseButtonEventArgs e)
    {
        if (e.Button == MouseButton.Right) _isRightMouseDown = false;
    }

    private static void OnKeyDown(KeyboardKeyEventArgs e)
    {
        if (ImGui.IsWindowFocused(ImGuiFocusedFlags.AnyWindow)) return;

        if (e.Key == Keys.Space)
            _isOrbitEnabled = !_isOrbitEnabled;

        if (e.Key == Keys.Escape)
        {
            Logger.WriteLine("Closing window...");
            WindowManager.GetWindow().Close();
        }

        if (e.Key == Keys.C)
        {
            Logger.WriteLine("Remove last object key pressed");
            RemoveLastAdded();
        }

        // Zoom in/out with mouse wheel (optional)
        if (e.Key == Keys.Up) _camera!.Distance -= 0.1f;
        if (e.Key == Keys.Down) _camera!.Distance += 0.1f;
    }

    public static void RemoveLastAdded()
    {
        if (Renderer.Spheres.Count > 1 && ImGuiElementContainer.celestialBodies.Count > 1)
        {
            Renderer.RemoveObject();
            ImGuiElementContainer.celestialBodies.RemoveAt(ImGuiElementContainer.celestialBodies.Count - 1);
        }
    }

    private static void OnUpdateFrame(FrameEventArgs args)
    {
        var window = WindowManager.GetWindow();
        var input = window.KeyboardState;
        float cameraSpeed = _camera!.Speed * (float)args.Time;
    }
}