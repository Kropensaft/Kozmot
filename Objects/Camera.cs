using OpenTK.Mathematics;

namespace OpenGL.Objects;

/// <summary>
///     Arcball Camera implementation for orbiting around a pivot point.
/// </summary>
public class Camera
{
    public static Vector3 _pivot = Vector3.Zero; // Central point to orbit around

    // Distance from the pivot
    private float _distance = 5.0f;

    // Camera direction vectors

    // Camera rotation
    private float _pitch; // Rotation around the X axis
    private float _yaw = -MathHelper.PiOver2; // Rotation around the Y axis (initialized to look forward)

    public Camera(Vector3 position)
    {
        Position = position;
        UpdateVectors();
    }

    // Camera position (automatically computed relative to pivot)
    public Vector3 Position { get; private set; }

    // Properties to expose direction vectors
    public Vector3 Front { get; private set; } = -Vector3.UnitZ;

    public Vector3 Up { get; private set; } = Vector3.UnitY;
    public Vector3 Right { get; private set; } = Vector3.UnitX;

    public float Pitch
    {
        get => MathHelper.RadiansToDegrees(_pitch);
        set
        {
            float angle = MathHelper.Clamp(value, -Constants.CAMERA_ANGLE_CLAMP, Constants.CAMERA_ANGLE_CLAMP);
            _pitch = MathHelper.DegreesToRadians(angle);
            UpdateVectors();
        }
    }

    public float Yaw
    {
        get => MathHelper.RadiansToDegrees(_yaw);
        set
        {
            _yaw = MathHelper.DegreesToRadians(value);
            UpdateVectors();
        }
    }

    public float Distance
    {
        get => _distance;
        set
        {
            _distance = Math.Max(value, 0.1f); // Ensure distance is never zero
            UpdateVectors();
        }
    }

    public Vector3 Pivot
    {
        get => _pivot;
        set
        {
            _pivot = value;
            UpdateVectors();
        }
    }

    public float Speed { get; set; } = Constants.CAMERA_SPEED;
    public float Sensitivity { get; set; } = Constants.CAMERA_SENSITIVITY;

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, _pivot, Up);
    }

    public Matrix4 GetProjectionMatrix(float aspectRatio)
    {
        return Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(45f),
            aspectRatio,
            0.1f,
            1000f
        );
    }

    private void UpdateVectors()
    {
        Front = new Vector3(
            MathF.Cos(_pitch) * MathF.Cos(_yaw),
            MathF.Sin(_pitch),
            MathF.Cos(_pitch) * MathF.Sin(_yaw)
        ).Normalized();

        Position = _pivot - Front * _distance; // Orbit around pivot
        Right = Vector3.Cross(Front, Vector3.UnitY).Normalized();
        Up = Vector3.Cross(Right, Front).Normalized();
    }
}