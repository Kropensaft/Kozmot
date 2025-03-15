using OpenTK.Mathematics;


namespace OpenGL.Objects;

public class Camera
{
    // Camera position
    public Vector3 Position { get; set; }

    // Camera direction vectors (fields, not properties)
    private Vector3 _front = -Vector3.UnitZ;
    private Vector3 _up = Vector3.UnitY;
    private Vector3 _right = Vector3.UnitX;

    // Properties to expose direction vectors
    public Vector3 Front => _front;
    public Vector3 Up => _up;
    public Vector3 Right => _right;

    // Camera rotation
    private float _pitch; // Rotation around the X axis
    private float _yaw = -MathHelper.PiOver2; // Rotation around the Y axis (initialized to look forward)

    public float Pitch
    {
        get => MathHelper.RadiansToDegrees(_pitch);
        set
        {
            float angle = MathHelper.Clamp(value, -89f, 89f);
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

    // Camera movement speed and mouse sensitivity
    public float Speed { get; set; } = 2.5f;
    public float Sensitivity { get; set; } = 0.2f;

    public Camera(Vector3 position)
    {
        Position = position;
        UpdateVectors();
    }

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, Position + _front, _up);
    }

    private void UpdateVectors()
    {
        // Calculate the new Front vector
        _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
        _front.Y = MathF.Sin(_pitch);
        _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);
        _front = Vector3.Normalize(_front);

        // Recalculate the Right and Up vectors
        _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
        _up = Vector3.Normalize(Vector3.Cross(_right, _front));
    }
}