using OpenTK.Mathematics;

namespace OpenGL;


//TODO : sjednodit do jednoho objektu Sphere / Object
public abstract class Object // Make abstract if never instantiated directly
{
    // Protected constructor for base class
    protected Object(
        string name,
        Vector3 position,
        Vector3 rotation,
        Vector3 scale,
        System.Numerics.Vector3 color,
        string type,
        Vector3 initialVelocity = default, // New parameter
        Vector3 initialAngularVelocity = default) // New parameter
    {
        // Existing assignments
        Name = name;
        Position = position;
        Rotation = rotation;
        Scale = scale;
        Color = color;
        Type = type;

        // Initialize movement and rotation
        Velocity = initialVelocity;
        AngularVelocity = initialAngularVelocity; // Ensure AngularVelocity is a Vector3 property
        Acceleration = Vector3.Zero;
    }

    protected Object()
    {
    }

    // Add this property if not already present
    public Vector3 AngularVelocity { get; set; } = new(0, 1f, 0); // Default Y-axis rotation

    // Public properties for external access
    public Vector3 Position { get; set; } // OpenTK Position
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }
    public System.Numerics.Vector3 Color { get; set; } // System.Numerics Color
    public string Name { get; set; }
    public string Type { get; set; } // Public getter for Type


    public float RotationSpeed { get; set; } = 0f; // Radians per second

    // Protected or private for internal physics state
    public Vector3 Velocity { get; set; }
    public Vector3 Acceleration { get; set; }

    public virtual void Update(float deltaTime)
    {
        Rotation += new Vector3(
            0,
            RotationSpeed * deltaTime,
            0
        );
    }

    public virtual Matrix4 GetModelMatrix()
    {
        return Matrix4.CreateScale(Scale) *
               Matrix4.CreateRotationX(MathHelper.DegreesToRadians(90)) * // X-axis rotation
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
        Vector3 position,
        Vector3 rotation,
        Vector3 scale,
        System.Numerics.Vector3 color,
        float orbitRadius,
        float angularSpeed,
        string planetTypeName,
        Object? parent = null,
        Vector3 initialVelocity = default,
        Vector3 initialAngularVelocity = default,
        float rotationSpeed = 0f
        )
        
    {
        Name = name;
        Position = position;
        Rotation = rotation;
        Scale = scale;
        Color = color;
        Type = planetTypeName;
        Velocity = initialVelocity;
        AngularVelocity = initialAngularVelocity;
        Acceleration = Vector3.Zero;
        OrbitRadius = orbitRadius;
        AngularSpeed = angularSpeed;
        Parent = parent;
        RotationSpeed = rotationSpeed;
        
        //Indicator is without type
        if(Type != "")
         TextureID =
            TextureLoader.LoadTexture(Constants._TexturePaths[Array.IndexOf(Constants.planetTypes, Type)]);
    }

    
    // Properties specific to orbiting spheres
    public float OrbitRadius { get; set; }
    public int TextureID { get; set; }
    public float AngularSpeed { get; } // Typically constant once set
    private float Angle { get; set; } // Current angle in the orbit
    public Object? Parent { get; set; } // The object this sphere orbits


    public override void Update(float time)
    {
        // Increment global time in the simulation

        float angle = AngularSpeed * time;

        if (Parent == null)
            Position = new Vector3(
                MathF.Cos(angle) * OrbitRadius,
                0,
                MathF.Sin(angle) * OrbitRadius
            );
        else
            // For child spheres, calculate position relative to parent
            Position = Parent.Position + new Vector3(
                MathF.Cos(angle) * OrbitRadius,
                0,
                MathF.Sin(angle) * OrbitRadius
            );

        // Update rotation
        Rotation = new Vector3(
            0,
            RotationSpeed * time,
            0
        );
    }

    // ? Remnant of previous gravity implementations
    /*
    private void ApplyGravity(Sphere other, double deltaTime = 0.0)
    {
        var direction = other.Position - Position;
        float distanceSq = direction.LengthSquared;

        if (distanceSq < 0.01f) return;

        float distance = MathF.Sqrt(distanceSq);
        var forceDir = direction / distance;

        float forceMagnitude = Constants.GRAVITATIONAL_CONSTANT * (Mass * other.Mass) / distanceSq;
        var acceleration = forceDir * (forceMagnitude / Mass);


        Acceleration += acceleration;
    }
    */


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
            float xy = MathF.Cos(stackAngle); // r * cos(u)
            float z = MathF.Sin(stackAngle); // r * sin(u)

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

        // Logger.WriteLine($"Generated mesh for sphere '{Name}'"); // Optional logging
        return (vertices.ToArray(), indices.ToArray());
    }
}