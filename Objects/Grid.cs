using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenGL.Objects;

public class Grid
{
    private int _vao, _vbo, _ebo;
    private float[] _vertices;
    private uint[] _indices;

    public Grid(int size = 500, float step = 1.0f)
    {
        GenerateGridGeometry(size, step);
        InitializeBuffers();
    }

    private void GenerateGridGeometry(int size, float step)
    {
        List<float> vertices = new List<float>();
        List<uint> indices = new List<uint>();
        uint index = 0;
        float halfSize = size * step / 2;

        // X-axis lines 
        for (float x = -halfSize; x <= halfSize; x += step)
        {
            vertices.AddRange(new[] { x, 0, -halfSize, Color.SlateGray.R, Color.SlateGray.G, Color.SlateGray.B , 0.9f}); // Start point
            vertices.AddRange(new[] { x, 0, halfSize, Color.SlateGray.R, Color.SlateGray.G, Color.SlateGray.B , 0.9f});  // End point
            indices.AddRange(new[] { index++, index++ });
        }

        // Z-axis lines 
        for (float z = -halfSize; z <= halfSize; z += step)
        {
            vertices.AddRange(new[] { -halfSize, 0, z, Color.SlateGray.R, Color.SlateGray.G, Color.SlateGray.B , 0.1f}); // Start point
            vertices.AddRange(new[] { halfSize, 0, z, Color.SlateGray.R, Color.SlateGray.G, Color.SlateGray.B, 0.1f});  // End point
            indices.AddRange(new[] { index++, index++ });
        }

        _vertices = vertices.ToArray(); 
        _indices = indices.ToArray();
    }

    private void InitializeBuffers()
    {
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        // Vertex attributes (position + RGBA color)
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
    }

    public void Render(int shaderProgram, Matrix4 viewMatrix, Matrix4 projectionMatrix)
    {
        GL.UseProgram(shaderProgram);
        GL.BindVertexArray(_vao);

        // Static model matrix (no translation)
        Matrix4 modelMatrix = Matrix4.Identity;
        
        // Set uniforms
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "model_matrix"), false, ref modelMatrix);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "view_matrix"), false, ref viewMatrix);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection_matrix"), false, ref projectionMatrix);

        // Render grid lines
        GL.DrawElements(PrimitiveType.Lines, _indices.Length, DrawElementsType.UnsignedInt, 0);
    }
}