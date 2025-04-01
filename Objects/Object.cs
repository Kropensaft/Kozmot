using OpenTK.Mathematics;

// Added for Renderer access

namespace OpenGL;

public class Object
{
    protected Object(string name, Vector3 position, Vector3 rotation, Vector3 scale,
        System.Numerics.Vector3 color, float mass, string type, bool isEmissive = false)
    {
        Name = name;
        Position = position;
        Rotation = rotation;
        Scale = scale;
        Color = color;
        Mass = mass;
        Type = type;
        IsEmissive = isEmissive;
        Velocity = Vector3.Zero;
        Acceleration = Vector3.Zero;
    }

    public Vector3 Position { get; protected set; }
    public Vector3 Rotation { get; protected set; }
    public Vector3 Scale { get; protected set; }
    public System.Numerics.Vector3 Color { get; set; }
    public string Name { get; set; }

    public string Type { get; set; }
    protected float Mass { get; set; }
    public bool IsEmissive { get; set; }
    private Vector3 Velocity { get; set; }
    protected Vector3 Acceleration { get; set; }

    public virtual void Update(double deltaTime)
    {
        Velocity += Acceleration * (float)deltaTime;
        Position += Velocity * (float)deltaTime;
        Acceleration = Vector3.Zero;
    }

    public virtual Matrix4 GetModelMatrix()
    {
        return Matrix4.CreateScale(Scale) *
               Matrix4.CreateRotationX(Rotation.X) *
               Matrix4.CreateRotationY(Rotation.Y) *
               Matrix4.CreateRotationZ(Rotation.Z) *
               Matrix4.CreateTranslation(Position);
    }
}

public class Sphere : Object
{
    public Sphere(string name, Vector3 position, Vector3 rotation, Vector3 scale,
        System.Numerics.Vector3 color, float mass, float orbitRadius,
        float angularSpeed, bool isEmissive = false, Object? parent = null)
        : base(name, position, rotation, scale, color, mass, "", isEmissive)
    {
        OrbitRadius = orbitRadius;
        AngularSpeed = angularSpeed;
        Parent = parent;
    }

    public float OrbitRadius { get; set; }
    public float AngularSpeed { get; }
    private float Angle { get; set; }
    public Object? Parent { get; set; }

    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);

        foreach (var other in Renderer.Spheres)
            if (other != this)
                ApplyGravity(other, deltaTime);

        //? If the planet is a moon and a parent is selected, calculate the orbit and best location for the moon
        if (Parent != null)
        {
            Angle += AngularSpeed * (float)deltaTime;
            Position = Parent.Position + new Vector3(
                OrbitRadius * MathF.Cos(Angle),
                0,
                OrbitRadius * MathF.Sin(Angle)
            );
        }
    }

    private void ApplyGravity(Sphere other, double deltaTime)
    {
        var direction = other.Position - Position;
        float distance = direction.Length;

        if (distance < 0.1f) return;

        direction = Vector3.Normalize(direction);
        float forceMagnitude = Constants.GRAVITATIONAL_CONSTANT * (Mass * other.Mass) / (distance * distance);
        var acceleration = direction * (forceMagnitude / Mass);
        Acceleration += acceleration * (float)deltaTime;
    }

    public (float[] Vertices, uint[] Indices) GenerateSphere(int sectors, int stacks)
    {
        var scale = Scale == Vector3.Zero ? Vector3.One * 3 : Scale;
        float radius = Constants.INITIAL_SPHERE_RADIUS;

        List<float> vertices = new();
        List<uint> indices = new();

        float sectorStep = 2 * MathF.PI / sectors;
        float stackStep = MathF.PI / stacks;

        for (int i = 0; i <= stacks; ++i)
        {
            float stackAngle = MathF.PI / 2 - i * stackStep;
            float xy = radius * MathF.Cos(stackAngle);
            float z = radius * MathF.Sin(stackAngle);

            for (int j = 0; j <= sectors; ++j)
            {
                float sectorAngle = j * sectorStep;
                float x = xy * MathF.Cos(sectorAngle);
                float y = xy * MathF.Sin(sectorAngle);

                vertices.Add(x * scale.X);
                vertices.Add(y * scale.Y);
                vertices.Add(z * scale.Z);

                vertices.Add(Color.X);
                vertices.Add(Color.Y);
                vertices.Add(Color.Z);
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

        Console.WriteLine($"Created sphere '{Name}' with color {Color}");
        return (vertices.ToArray(), indices.ToArray());
    }
}