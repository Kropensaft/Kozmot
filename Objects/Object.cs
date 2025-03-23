using OpenTK.Mathematics;

namespace OpenGL;

/// <summary>
///     Parent class for shapes.
///     TODO: implement a planet object instead of a basic sphere
/// </summary>
public class Object
{
    protected Object(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    private Vector3 Position { get; }
    protected Vector3 Rotation { get; set; }
    protected Vector3 Scale { get; set; }

    public virtual Matrix4 GetModelMatrix()
    {
        return Matrix4.CreateScale(Scale) *
               Matrix4.CreateRotationX(Rotation.X) *
               Matrix4.CreateRotationY(Rotation.Y) *
               Matrix4.CreateRotationZ(Rotation.Z) *
               Matrix4.CreateTranslation(Position);
    }
}

public class Sphere(Vector3 position, Vector3 rotation, Vector3 scale) : Object(position, rotation, scale)
{
    public Sphere(Vector3 position, Vector3 rotation, Vector3 scale, float orbitRadius, float speed)
        : this(position, rotation, scale)
    {
        Position = position;
        Speed = speed;
        Radius = orbitRadius;
        Angle = 0; // Start at angle 0
    }

    private Vector3 Position { get; set; }
    public float Radius { get; set; } // Orbital orbitRadius
    private float Speed { get; } // Orbital speed
    private float Angle { get; set; } // Current angle in radians

    public void UpdateOrbit(double deltaTime)
    {
        // Update the angle based on speed and time
        Angle += Speed * (float)deltaTime;

        // Calculate new position using polar coordinates
        Position = new Vector3(
            Radius * MathF.Cos(Angle), // X
            0, // Y (fixed height)
            Radius * MathF.Sin(Angle) // Z
        );
    }

    public override Matrix4 GetModelMatrix()
    {
        return Matrix4.CreateScale(Scale) *
               Matrix4.CreateRotationX(Rotation.X) *
               Matrix4.CreateRotationY(Rotation.Y) *
               Matrix4.CreateRotationZ(Rotation.Z) *
               Matrix4.CreateTranslation(Position);
    }

    public static (float[] Vertices, uint[] Indices) GenerateSphere(float orbitRadius, int sectors, int stacks,
        Vector3 scale = new())
    {
        //Default parameter and a failsafe if a Vec3.Zero is set as scale 
        if (scale == Vector3.Zero)
            scale = Vector3.One;


        List<float> vertices = new();
        List<uint> indices = new();

        float sectorStep = 2 * MathF.PI / sectors;
        float stackStep = MathF.PI / stacks;

        for (int i = 0; i <= stacks; ++i)
        {
            float stackAngle = MathF.PI / 2 - i * stackStep;
            float xy = orbitRadius * MathF.Cos(stackAngle);
            float z = orbitRadius * MathF.Sin(stackAngle);

            for (int j = 0; j <= sectors; ++j)
            {
                float sectorAngle = j * sectorStep;

                float x = xy * MathF.Cos(sectorAngle);
                float y = xy * MathF.Sin(sectorAngle);

                //used for coloring vertices
                var random = new Random();

                // Vertex position
                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(z);

                // Vertex color
                vertices.Add(Math.Clamp((float)random.NextDouble(), 0f, 1f)); // R
                vertices.Add(Math.Clamp((float)random.NextDouble(), 0f, 1f)); // G
                vertices.Add(Math.Clamp((float)random.NextDouble(), 0f, 1f)); // B
            }
        }

        for (int i = 0; i < stacks; ++i)
        for (int j = 0; j < sectors; ++j)
        {
            uint first = (uint)(i * (sectors + 1) + j);
            uint second = first + (uint)sectors + 1;

            indices.Add(first);
            indices.Add(second);
            indices.Add(first + 1);

            indices.Add(second);
            indices.Add(second + 1);
            indices.Add(first + 1);
        }

        return (vertices.ToArray(), indices.ToArray());
    }
}

public class Cube(Vector3 position, Vector3 rotation, Vector3 scale) : Object(position, rotation, scale);