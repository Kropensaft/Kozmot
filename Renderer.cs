using System.Drawing;
using OpenGL.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace OpenGL;

internal static class Renderer
{
    private static int _vao, _vbo, _ebo, _shaderProgram;
    private static GameWindow? _window;
    private static Matrix4 _projection, _view;
    private static List<Object> _objects = new List<Object>();
    private static float[] _vertices;
    private static uint[] _indices;
    
    //Initialize camera
    public static Camera _camera;

    public static void OnLoad()
    {
        GL.ClearColor(Color.Black);
        GL.Enable(EnableCap.DepthTest);
        
        _window = WindowManager.GetWindow();
        // Initialize matrices
        _projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(45f),
            _window.Size.X / (float)_window.Size.Y,
            0.1f,
            100f
        );

        _view = _camera.GetViewMatrix();

        // Generate sphere vertices and indices (or cube, etc.)
        var sphereData = Sphere.GenerateSphere(.2f, 36, 18);
        _vertices = sphereData.Vertices;
        _indices = sphereData.Indices;

        // VAO/VBO setup
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        // Shader setup
        _shaderProgram = Shader.CreateShaderProgram();
        GL.UseProgram(_shaderProgram);

        // Attribute pointers
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        // Get uniform locations
        int modelLoc = GL.GetUniformLocation(_shaderProgram, "model_matrix");
        int viewLoc = GL.GetUniformLocation(_shaderProgram, "view_matrix");
        int projLoc = GL.GetUniformLocation(_shaderProgram, "projection_matrix");

        // Set projection and view matrices
        GL.UniformMatrix4(projLoc, false, ref _projection);
        GL.UniformMatrix4(viewLoc, false, ref _view);

        // Add initial objects
        _objects.Add(new Sphere(new Vector3(-1, 0, 0), Vector3.Zero, Vector3.One));
        _objects.Add(new Cube(new Vector3(1, 0, 0), Vector3.Zero, Vector3.One));
    }

    public static void OnUpdate(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.UseProgram(_shaderProgram);
        GL.BindVertexArray(_vao);

        // Update the view matrix using the shared camera
        Matrix4 view = _camera.GetViewMatrix();
        GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "view_matrix"), false, ref view);

        // Render each object
        foreach (var obj in _objects)
        {
            Matrix4 model = obj.GetModelMatrix();
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model_matrix"), false, ref model);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        _window.SwapBuffers();
    }

    public static void AddObject(Object obj)
    {
        _objects.Add(obj);
    }
    
}