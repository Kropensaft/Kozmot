using System.Drawing;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Runtime.InteropServices;

namespace C_Sharp_GL;

internal static class Renderer
{
    private static GL _gl = null!;
    private static uint _vao, _vbo, _ebo, _shaderProgram;
    private static IWindow _window = WindowManager.GetWindow();
    private static Matrix4X4<float> _projection, _view, _model;
    private static int _modelLoc, _viewLoc, _projLoc;
    private static float _rotationTime;
    
    private static bool _isPaused = false;
    public static bool IsPaused => _isPaused;
    public static void Pause() => _isPaused = !_isPaused;

    private static readonly float[] Vertices = {
        // Positions          // Colors
        -0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 0.0f,
         0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.0f,
         0.5f,  0.5f, -0.5f,  0.0f, 0.0f, 1.0f,
        -0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.0f,
        -0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 1.0f,
         0.5f, -0.5f,  0.5f,  0.0f, 1.0f, 1.0f,
         0.5f,  0.5f,  0.5f,  0.5f, 0.5f, 1.0f,
        -0.5f,  0.5f,  0.5f,  1.0f, 0.5f, 0.5f
    };

    private static readonly uint[] Indices = {
        0, 1, 2, 2, 3, 0,    // Back face
        4, 5, 6, 6, 7, 4,    // Front face
        4, 0, 3, 3, 7, 4,    // Left face
        1, 5, 6, 6, 2, 1,    // Right face
        3, 2, 6, 6, 7, 3,    // Top face
        4, 5, 1, 1, 0, 4     // Bottom face
    };

    public static unsafe void OnLoad()
    {
        _gl = WindowManager.GetWindow().CreateOpenGL();
        _gl.ClearColor(Color.Black);
        _gl.Enable(GLEnum.DepthTest);

        InputHandler.InitializeInputs();

        // Initialize matrices
        _projection = Matrix4X4.CreatePerspectiveFieldOfView<float>(
            Scalar.DegreesToRadians(45f),
            _window.Size.X / (float)_window.Size.Y,
            0.1f,
            100f
        );

        _view = Matrix4X4.CreateLookAt<float>(
            new Vector3D<float>(0, 0, 3),
            Vector3D<float>.Zero,
            Vector3D<float>.UnitY
        );

        // VAO/VBO setup
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* buf = Vertices)
        {
            _gl.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(Vertices.Length * sizeof(float)),
                buf,
                BufferUsageARB.StaticDraw
            );
        }

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        fixed (uint* buf = Indices)
        {
            _gl.BufferData(
                BufferTargetARB.ElementArrayBuffer,
                (nuint)(Indices.Length * sizeof(uint)),
                buf,
                BufferUsageARB.StaticDraw
            );
        }

        // Shader setup
        _shaderProgram = Shader.CreateShaderProgram(_gl);
        _gl.UseProgram(_shaderProgram);

        // Attribute pointers
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)0);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)(3 * sizeof(float)));
        _gl.EnableVertexAttribArray(1);

        // Get uniform locations
        _modelLoc = _gl.GetUniformLocation(_shaderProgram, "model_matrix");
        _viewLoc = _gl.GetUniformLocation(_shaderProgram, "view_matrix");
        _projLoc = _gl.GetUniformLocation(_shaderProgram, "projection_matrix");

        // Set projection and view matrices
        SetMatrixUniform(_projLoc, _projection);
        SetMatrixUniform(_viewLoc, _view);
    }

    private static unsafe void SetMatrixUniform(int location, Matrix4X4<float> matrix)
    {
        Span<float> matrixSpan = MemoryMarshal.Cast<Matrix4X4<float>, float>(
            MemoryMarshal.CreateSpan(ref matrix, 1)
        );
        _gl.UniformMatrix4(location, 1, false, ref matrixSpan[0]);
    }

    public static unsafe void OnUpdate(double deltaTime)
    {
        if (!_isPaused)
        {
            _rotationTime += (float)deltaTime;
        }
        
        // Update model matrix
        _model = Matrix4X4.CreateRotationY<float>(_rotationTime) * 
                Matrix4X4.CreateRotationX<float>(_rotationTime * 0.5f);

        // Clear and draw
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gl.UseProgram(_shaderProgram);
        _gl.BindVertexArray(_vao);
        SetMatrixUniform(_modelLoc, _model);
        _gl.DrawElements(GLEnum.Triangles, (uint)Indices.Length, GLEnum.UnsignedInt, (void*)0);
        _window.SwapBuffers();
    }
    
    
    public static GL GetGL() => _gl;
}