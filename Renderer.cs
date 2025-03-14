using System.Drawing;
using Silk.NET.OpenGL;

namespace C_Sharp_GL;

internal static class Renderer
{
    private static GL _gl;
    private static uint _vao, _vbo, _ebo, _shaderProgram;

    private static readonly float[] Vertices =
    {
        0.5f, 0.5f, 0.0f,
        0.5f, -0.5f, 0.0f,
        -0.5f, -0.5f, 0.0f,
        -0.5f, 0.5f, 0.0f
    };

    private static readonly uint[] Indices =
    {
        0u, 1u, 3u,
        1u, 2u, 3u
    };

    public static unsafe void OnLoad()
    {
        _gl = WindowManager.GetWindow().CreateOpenGL(); // ✅ OpenGL initialized after window creation
        _gl.ClearColor(Color.CornflowerBlue);

        // Initialize Input Handling ✅
        InputHandler.InitializeInputs();

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* buf = Vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(Vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
        }

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        fixed (uint* buf = Indices)
        {
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(Indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
        }

        _shaderProgram = Shader.CreateShaderProgram(_gl);
        _gl.UseProgram(_shaderProgram);

        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }

    public static void OnUpdate(double deltaTime) { }

    public static unsafe void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _gl.BindVertexArray(_vao);
        _gl.UseProgram(_shaderProgram);
        _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
    }
}
