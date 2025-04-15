using OpenTK.Mathematics;

namespace OpenGL;

public abstract class Object // Make abstract if never instantiated directly
{
    // Protected constructor for base class
    protected Object(
        string name,
        Vector3 position,
        Vector3 rotation,
        Vector3 scale,
        System.Numerics.Vector3 color, // Store as System.Numerics if used by ImGui/elsewhere
        float mass,
        string type, // Store the type name
        bool isEmissive = false)
    {
        Name = name;
        Position = position;
        Rotation = rotation;
        Scale = scale;
        Color = color; // Assign System.Numerics.Vector3
        Mass = mass;
        Type = type; // Assign the type name
        IsEmissive = isEmissive;
        Velocity = Vector3.Zero;
        Acceleration = Vector3.Zero;
    }

    // Public properties for external access
    public Vector3 Position { get; protected set; } // OpenTK Position
    public Vector3 Rotation { get; protected set; }
    public Vector3 Scale { get; protected set; }
    public System.Numerics.Vector3 Color { get; set; } // System.Numerics Color
    public string Name { get; set; }
    public string Type { get; set; } // Public getter for Type
    public float Mass { get; protected set; } // Public getter for Mass
    public bool IsEmissive { get; set; }

    // Protected or private for internal physics state
    protected Vector3 Velocity { get; set; }
    protected Vector3 Acceleration { get; set; }

    public virtual void Update(double deltaTime)
    {
        // Basic Euler integration
        Velocity += Acceleration * (float)deltaTime;
        Position += Velocity * (float)deltaTime;
        Acceleration = Vector3.Zero; // Reset acceleration for next frame
    }

    public virtual Matrix4 GetModelMatrix()
    {
        return Matrix4.CreateScale(Scale) *
               Matrix4.CreateRotationX(Rotation.X) * // X-axis rotation
               Matrix4.CreateRotationY(Rotation.Y) * // Y-axis rotation
               Matrix4.CreateRotationZ(Rotation.Z) * // Z-axis rotation
               Matrix4.CreateTranslation(Position);
    }
}

public class Sphere : Object
{
    // Constructor takes all necessary parameters including type name
    public Sphere(
        string name,
        Vector3 position, // Absolute world position
        Vector3 rotation, // Rotation
        Vector3 scale, // Scale
        System.Numerics.Vector3 color, // Color (System.Numerics)
        float mass,
        float orbitRadius, // Distance from parent (or origin if no parent)
        float angularSpeed, // Angular speed for orbit
        string planetTypeName, // The name of the type
        bool isEmissive = false,
        Object? parent = null)
        : base(name, position, rotation, scale, color, mass, planetTypeName, isEmissive) // Pass typeName to base
    {
        OrbitRadius = orbitRadius;
        AngularSpeed = angularSpeed;
        Parent = parent;
        if (Parent != null && OrbitRadius > 0.001f)
        {
            // Set initial position relative to parent
            Position = Parent.Position + new Vector3(OrbitRadius, 0, 0);
            // Calculate initial tangential velocity for orbit
            float orbitalSpeed = MathF.Sqrt(Constants.GRAVITATIONAL_CONSTANT * Parent.Mass / OrbitRadius);
            Velocity = new Vector3(0, 0, orbitalSpeed); // Adjust direction as needed
        }
    }

    // Properties specific to orbiting spheres
    public float OrbitRadius { get; set; }
    public int TextureID { get; set; }
    public float AngularSpeed { get; } // Typically constant once set
    private float Angle { get; set; } // Current angle in the orbit
    public Object? Parent { get; set; } // The object this sphere orbits

    public override void Update(double deltaTime)
    {
        // Apply gravitational forces
        foreach (var other in Renderer.Spheres.OfType<Sphere>())
            if (other != this)
                ApplyGravity(other, deltaTime);

        // Update position via physics
        base.Update(deltaTime);
    }

    private void ApplyGravity(Sphere other, double deltaTime)
    {
        var direction = other.Position - Position; // Corrected direction
        float distanceSq = direction.LengthSquared;

        if (distanceSq < 0.01f) return;

        float distance = MathF.Sqrt(distanceSq);
        var forceDir = direction / distance;

        float forceMagnitude = Constants.GRAVITATIONAL_CONSTANT * (Mass * other.Mass) / distanceSq;
        var acceleration = forceDir * (forceMagnitude / Mass);

        Acceleration += acceleration;
    }


    // Instance method to generate mesh data based on instance properties
    public (float[] Vertices, uint[] Indices) GenerateSphere(int sectors = 36, int stacks = 18)
    {
        float radius = Scale.X; // Base radius from scale
        // If scale is zero (or near zero), use a default fallback.
        if (radius < 0.001f) radius = Constants.INITIAL_SPHERE_RADIUS;

        List<float> vertices = new();
        List<uint> indices = new();

        float sectorStep = 2 * MathF.PI / sectors;
        float stackStep = MathF.PI / stacks;
        float stackAngle, sectorAngle;

        // Add vertex data: position (x,y,z) and color (r,g,b)
        for (int i = 0; i <= stacks; ++i)
        {
            stackAngle = MathF.PI / 2 - i * stackStep; // Starting from pi/2 to -pi/2
            float xy = radius * MathF.Cos(stackAngle); // r * cos(u)
            float z = radius * MathF.Sin(stackAngle); // r * sin(u)

            // Add (sectorCount+1) vertices per stack
            for (int j = 0; j <= sectors; ++j)
            {
                sectorAngle = j * sectorStep; // Starting from 0 to 2pi

                // Vertex position (x, y, z)
                float x = xy * MathF.Cos(sectorAngle); // r * cos(u) * cos(v)
                float y = xy * MathF.Sin(sectorAngle); // r * cos(u) * sin(v)
                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(z);

                float u = (float)j / sectors;
                float v = 1.0f - (float)i / stacks; // Flip V coordinate

                vertices.Add(u);
                vertices.Add(v);
                // Vertex color (r, g, b) using the object's System.Numerics.Vector3 Color
                // vertices.Add(Color.X);
                // vertices.Add(Color.Y);
                // vertices.Add(Color.Z);
            }
        }

        // Add index data
        uint k1, k2;
        for (int i = 0; i < stacks; ++i)
        {
            k1 = (uint)(i * (sectors + 1)); // Beginning of current stack
            k2 = (uint)(k1 + sectors + 1); // Beginning of next stack

            for (int j = 0; j < sectors; ++j, ++k1, ++k2)
            {
                // 2 triangles per sector excluding first and last stacks
                if (i != 0)
                {
                    indices.Add(k1);
                    indices.Add(k2);
                    indices.Add(k1 + 1);
                }

                if (i != stacks - 1)
                {
                    indices.Add(k1 + 1);
                    indices.Add(k2);
                    indices.Add(k2 + 1);
                }
            }
        }

        // Console.WriteLine($"Generated mesh for sphere '{Name}'"); // Optional logging
        return (vertices.ToArray(), indices.ToArray());
    }
}