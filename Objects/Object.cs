using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace OpenGL;

public class Object
{
    protected Object(Vector3 position, Vector3 rotation, Vector3 scale, System.Numerics.Vector3 color)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
        Color = color;
    }

    private Vector3 Position { get; }
    protected Vector3 Rotation { get; set; }
    protected Vector3 Scale { get; set; }
    public System.Numerics.Vector3 Color { get; set; } // Instance property

    public virtual Matrix4 GetModelMatrix()
    {
        return Matrix4.CreateScale(Scale) *
               Matrix4.CreateRotationX(Rotation.X) *
               Matrix4.CreateRotationY(Rotation.Y) *
               Matrix4.CreateRotationZ(Rotation.Z) *
               Matrix4.CreateTranslation(Position);
    }
}

public class Sphere(
    Vector3 position,
    Vector3 rotation,
    Vector3 scale,
    System.Numerics.Vector3 color,
    float orbitRadius,
    float speed)
    : Object(position, rotation, scale, color)
{
    public float Radius { get; set; } = orbitRadius;
    private float Speed { get; } = speed;
    private float Angle { get; set; } = 0;

    public void UpdateOrbit(double deltaTime)
    {
        Angle += Speed * (float)deltaTime;
        Position = new Vector3(
            Radius * MathF.Cos(Angle),
            0,
            Radius * MathF.Sin(Angle)
        );
    }

    private Vector3 Position { get; set; }

    public override Matrix4 GetModelMatrix()
    {
        return Matrix4.CreateScale(Scale) *
               Matrix4.CreateRotationX(Rotation.X) *
               Matrix4.CreateRotationY(Rotation.Y) *
               Matrix4.CreateRotationZ(Rotation.Z) *
               Matrix4.CreateTranslation(Position);
    }

    // Changed to instance method; no longer static
    public (float[] Vertices, uint[] Indices) GenerateSphere(int sectors, int stacks)
    {
        Vector3 scale = Scale == Vector3.Zero ? Vector3.One : Scale;

        List<float> vertices = new();
        List<uint> indices = new();

        float sectorStep = 2 * MathF.PI / sectors;
        float stackStep = MathF.PI / stacks;

        for (int i = 0; i <= stacks; ++i)
        {
            float stackAngle = MathF.PI / 2 - i * stackStep;
            float xy = Radius * MathF.Cos(stackAngle);
            float z = Radius * MathF.Sin(stackAngle);

            for (int j = 0; j <= sectors; ++j)
            {
                float sectorAngle = j * sectorStep;
                float x = xy * MathF.Cos(sectorAngle);
                float y = xy * MathF.Sin(sectorAngle);

                // Vertex position (scaled by instance's Scale)
                vertices.Add(x * scale.X);
                vertices.Add(y * scale.Y);
                vertices.Add(z * scale.Z);

                // Use instance's Color
                vertices.Add(Color.X);
                vertices.Add(Color.Y);
                vertices.Add(Color.Z);
            }
        }

        // Index generation remains the same
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

        Console.WriteLine($"Created sphere with color {Color}");
        return (vertices.ToArray(), indices.ToArray());
    }
}