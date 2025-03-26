using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace OpenGL;

public class Object
{
    public Vector3 Position { get; protected set; }
    public Vector3 Rotation { get; protected set; }
    public Vector3 Scale { get; protected set; }
    public System.Numerics.Vector3 Color { get; set; }
    public string Name { get; set; }
    public float Mass { get; set; }
    public bool IsEmissive { get; set; }

    public Object(string name, Vector3 position, Vector3 rotation, Vector3 scale, 
                 System.Numerics.Vector3 color, float mass, bool isEmissive = false)
    {
        Name = name;
        Position = position;
        Rotation = rotation;
        Scale = scale;
        Color = color;
        Mass = mass;
        IsEmissive = isEmissive;
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
    public float OrbitRadius { get; set; }
    public float Speed { get; private set; }
    private float Angle { get; set; } = 0;
    public Object Parent { get; set; } // For moons to reference their parent planet

    public Sphere(string name, Vector3 position, Vector3 rotation, Vector3 scale,
                 System.Numerics.Vector3 color, float mass, float orbitRadius, 
                 float speed, bool isEmissive = false, Object parent = null)
        : base(name, position, rotation, scale, color, mass, isEmissive)
    {
        OrbitRadius = orbitRadius;
        Speed = speed;
        Parent = parent;
    }

    public void UpdateOrbit(double deltaTime)
    {
        Angle += Speed * (float)deltaTime;
        var center = Parent?.Position ?? Vector3.Zero;
        Position = new Vector3(
            center.X + OrbitRadius * MathF.Cos(Angle),
            center.Y,
            center.Z + OrbitRadius * MathF.Sin(Angle)
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

    public (float[] Vertices, uint[] Indices) GenerateSphere(int sectors, int stacks)
    {
        Vector3 scale = Scale == Vector3.Zero ? Vector3.One*3 : Scale;
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