using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace OpenGL;

internal static class Renderer
{
    private static int _vao, _vbo, _ebo, _shaderProgram;
    private static GameWindow _window = WindowManager.GetWindow();
    private static Matrix4 _projection, _view, _model;
    private static int _modelLoc, _viewLoc, _projLoc;
    private static float _rotationTime;
    
    private static bool _isPaused = false;
    public static bool IsPaused => _isPaused;
    public static void Pause() => _isPaused = !_isPaused;

    private static readonly float[] Vertices = {
        // Positions          // Colors (grayscale)
        -0.5f, -0.5f, -0.5f,  0.5f, 0.5f, 0.5f, // Gray
        0.5f, -0.5f, -0.5f,  0.7f, 0.7f, 0.7f, // Light Gray
        0.5f,  0.5f, -0.5f,  0.9f, 0.9f, 0.9f, // Very Light Gray
        -0.5f,  0.5f, -0.5f,  0.3f, 0.3f, 0.3f, // Dark Gray

        -0.5f, -0.5f,  0.5f,  0.5f, 0.5f, 0.5f, // Gray
        0.5f, -0.5f,  0.5f,  0.7f, 0.7f, 0.7f, // Light Gray
        0.5f,  0.5f,  0.5f,  0.9f, 0.9f, 0.9f, // Very Light Gray
        -0.5f,  0.5f,  0.5f,  0.3f, 0.3f, 0.3f  // Dark Gray
    };

    private static readonly uint[] Indices = {
        0, 1, 2, 2, 3, 0,    // Back face
        4, 5, 6, 6, 7, 4,    // Front face
        4, 0, 3, 3, 7, 4,    // Left face
        1, 5, 6, 6, 2, 1,    // Right face
        3, 2, 6, 6, 7, 3,    // Top face
        4, 5, 1, 1, 0, 4     // Bottom face
    };

    public static void OnLoad()
    {
        GL.ClearColor(Color.Black);
        GL.Enable(EnableCap.DepthTest);

        InputHandler.InitializeInputs(_window);

        // Initialize matrices
        _projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(45f),
            _window.Size.X / (float)_window.Size.Y,
            0.1f,
            100f
        );

        _view = Matrix4.LookAt(
            new Vector3(0, 0, 3),
            Vector3.Zero,
            Vector3.UnitY
        );

        // VAO/VBO setup
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StaticDraw);

        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices, BufferUsageHint.StaticDraw);

        // Shader setup
        _shaderProgram = Shader.CreateShaderProgram();
        GL.UseProgram(_shaderProgram);

        // Attribute pointers
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        // Get uniform locations
        _modelLoc = GL.GetUniformLocation(_shaderProgram, "model_matrix");
        _viewLoc = GL.GetUniformLocation(_shaderProgram, "view_matrix");
        _projLoc = GL.GetUniformLocation(_shaderProgram, "projection_matrix");

        // Set projection and view matrices
        GL.UniformMatrix4(_projLoc, false, ref _projection);
        GL.UniformMatrix4(_viewLoc, false, ref _view);
    }

    private static double _fpsTimer = 0;
    private static int _fpsFrameCount = 0;
    private static int _currentFPS = 0;
    
    public static void OnUpdate(FrameEventArgs args)
    {
        WindowManager.CheckGLErrors();
        
        if (!_isPaused)
        {
            _rotationTime += (float)args.Time;
        }
        
        _fpsTimer += args.Time;
        _fpsFrameCount++;
        
        if (_fpsTimer >= 1.0)
        {
            _currentFPS = _fpsFrameCount;
            _fpsFrameCount = 0;
            _fpsTimer -= 1.0;
            
            // Update window title
            _window.Title = $"C# GL - FPS: {_currentFPS}";
        }
        
        // Update model matrix
        _model = Matrix4.CreateRotationY(_rotationTime) * 
                Matrix4.CreateRotationX(_rotationTime * 0.5f);

        // Clear and draw
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.UseProgram(_shaderProgram);
        GL.BindVertexArray(_vao);
        GL.UniformMatrix4(_modelLoc, false, ref _model);
        GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
        _window.SwapBuffers();
    }
    
}