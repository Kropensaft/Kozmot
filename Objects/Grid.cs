using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenGL.Objects;

/// <summary>
///     Separated class for the "infinite grid", has its own vbo,ebo,vao so that the main buffer isn't polluted by the
///     unchanging grid vertices
/// </summary>

// TODO : Should probably be IDisposable since there is no need to keep it in memory 
public class Grid
{
    public static int _vao, _vbo, _ebo;
    public static float[]? _vertices;
    public static uint[]? _indices;
    public static bool RenderGrid = true;

    public Grid(int size = 200, float step = 1.0f)
    {
        GenerateGridGeometry(size, step);
        InitializeBuffers();
    }


    private void GenerateGridGeometry(int size, float step)
    {
        List<float> vertices = new();
        List<uint> indices = new();
        uint index = 0;
        float halfSize = size * step / 2;

        // X-axis lines (gray by default, red at X=0)
        for (float x = -halfSize; x <= halfSize; x += step)
        {
            // Determine color: red for X=0, gray otherwise
            float r = Math.Abs(x) < Constants.GRID_COMPARISON_FLOAT
                ? Constants.GRID_RED_VALUE
                : Constants.GRID_FALLBACK_FLOAT; // Red for X=0, gray otherwise
            float g = Math.Abs(x) < Constants.GRID_COMPARISON_FLOAT
                ? Constants.FLOAT_ZERO
                : Constants.GRID_FALLBACK_FLOAT;
            float b = Math.Abs(x) < Constants.GRID_COMPARISON_FLOAT
                ? Constants.FLOAT_ZERO
                : Constants.GRID_FALLBACK_FLOAT;
            float a = 0.3f; // Transparency

            vertices.AddRange(new[] { x, Constants.GRID_YPOS_FLOAT, -halfSize, r, g, b, a }); // Start point
            vertices.AddRange(new[] { x, Constants.GRID_YPOS_FLOAT, halfSize, r, g, b, a }); // End point
            indices.AddRange(new[] { index++, index++ });
        }

        // Z-axis lines (gray by default, blue at Z=0)
        for (float z = -halfSize; z <= halfSize; z += step)
        {
            // Determine color: blue for Z=0, gray otherwise
            float r = Math.Abs(z) < Constants.GRID_COMPARISON_FLOAT
                ? Constants.FLOAT_ZERO
                : Constants.GRID_FALLBACK_FLOAT; // Blue for Z=0, gray otherwise
            float g = Math.Abs(z) < Constants.GRID_COMPARISON_FLOAT
                ? Constants.FLOAT_ZERO
                : Constants.GRID_FALLBACK_FLOAT;
            float b = Math.Abs(z) < Constants.GRID_COMPARISON_FLOAT
                ? Constants.GRID_RED_VALUE
                : Constants.GRID_FALLBACK_FLOAT;
            float a = 0.9f; // Transparency

            vertices.AddRange(new[] { -halfSize, Constants.GRID_YPOS_FLOAT, z, r, g, b, a }); // Start point
            vertices.AddRange(new[] { halfSize, Constants.GRID_YPOS_FLOAT, z, r, g, b, a }); // End point
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
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices!.Length * sizeof(float), _vertices,
            BufferUsageHint.StaticDraw);

        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices!.Length * sizeof(uint), _indices,
            BufferUsageHint.StaticDraw);

        // Vertex attributes (position + RGBA color)
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float,
            false, Constants.VERTEX_ATRIBB_STRIDE * sizeof(float), 0);

        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float,
            false, Constants.VERTEX_ATRIBB_STRIDE * sizeof(float), 3 * sizeof(float));

        GL.EnableVertexAttribArray(1);
    }

    public void Render(int shaderProgram, Matrix4 viewMatrix, Matrix4 projectionMatrix)
    {
        GL.UseProgram(shaderProgram);
        GL.BindVertexArray(_vao);

        // Static model matrix (no translation)
        var modelMatrix = Matrix4.Identity;

        // Set uniforms
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "model_matrix"), false, ref modelMatrix);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "view_matrix"), false, ref viewMatrix);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection_matrix"), false, ref projectionMatrix);

        // Render grid lines
        GL.DrawElements(PrimitiveType.Lines, _indices!.Length, DrawElementsType.UnsignedInt, 0);
    }
}