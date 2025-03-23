using OpenTK.Mathematics;

namespace OpenGL.Objects;
/// <summary>
/// Camera implementation for omnidirectional movement among the scene
/// </summary>
public class Camera
{
    // Camera direction vectors (fields, not properties)
    private Vector3 _front = -Vector3.UnitZ;

    // Camera rotation
    private float _pitch; // Rotation around the X axis
    private float _yaw = -MathHelper.PiOver2; // Rotation around the Y axis (initialized to look forward)

    public Camera(Vector3 position)
    {
        Position = position;
        UpdateVectors();
    }

    // Camera position
    public Vector3 Position { get; set; }

    // Properties to expose direction vectors
    public Vector3 Front => _front;
    public Vector3 Up { get; private set; } = Vector3.UnitY;

    public Vector3 Right { get; private set; } = Vector3.UnitX;

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

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, Position + _front, Up);
    }

    private void UpdateVectors()
    {
        // Calculate the new Front vector
        _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
        _front.Y = MathF.Sin(_pitch);
        _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);
        _front = Vector3.Normalize(_front);

        // Recalculate the Right and Up vectors
        Right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, _front));
    }
}