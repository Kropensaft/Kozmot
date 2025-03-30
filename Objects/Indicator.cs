using OpenGL.GUI;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenGL;

internal static class Indicator
{
    private static int _vao, _vbo, _ebo, _shaderProgram;
    private static float[]? _vertices;
    private static uint[]? _indices;

    public static void Initialize()
    {
        // Generate sphere mesh (simplified version of Sphere.GenerateSphere)
        var sphere = new Sphere("Indicator", Vector3.Zero, Vector3.Zero,
            Vector3.One,
            System.Numerics.Vector3.One,
            0f,
            0f,
            0f);

        (float[] Vertices, uint[] Indices) sphereData =
            sphere.GenerateSphere(Constants.SPHERE_SECTOR_COUNT, Constants.SPHERE_STACK_COUNT);
        _vertices = sphereData.Vertices;
        _indices = sphereData.Indices;

        // Compile shaders
        _shaderProgram = Shader.CreateShaderProgram(
            Constants.indicatorVertexShaderPath,
            Constants.indicatorFragmentShaderPath
        );

        // VAO/VBO setup
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices,
            BufferUsageHint.StaticDraw);

        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices,
            BufferUsageHint.StaticDraw);

        // Attribute pointers (matches existing sphere layout)
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
    }

    public static void Render(Matrix4 view, Matrix4 projection, Vector3 color, float alpha)
    {
        if (!ImGuiElementContainer.IsEditing) return;

        GL.UseProgram(_shaderProgram);
        GL.BindVertexArray(_vao);

        // Get UI values
        var position = new Vector3(
            ImGuiElementContainer.position.X,
            ImGuiElementContainer.position.Y,
            ImGuiElementContainer.position.Z
        );

        // Set uniforms
        var model = Matrix4.CreateTranslation(position);
        int modelLoc = GL.GetUniformLocation(_shaderProgram, "model");
        int viewLoc = GL.GetUniformLocation(_shaderProgram, "view");
        int projLoc = GL.GetUniformLocation(_shaderProgram, "projection");
        int colorLoc = GL.GetUniformLocation(_shaderProgram, "color");

        GL.UniformMatrix4(modelLoc, false, ref model);
        GL.UniformMatrix4(viewLoc, false, ref view);
        GL.UniformMatrix4(projLoc, false, ref projection);
        GL.Uniform4(colorLoc, new Vector4(color, alpha));

        // Draw
        GL.DrawElements(PrimitiveType.Triangles, _indices!.Length, DrawElementsType.UnsignedInt, 0);
    }
}