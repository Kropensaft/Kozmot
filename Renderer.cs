using System.Drawing;
using ImGuiNET;
using OpenGL.GUI;
using OpenGL.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace OpenGL;

internal static class Renderer
{
    
    //virtual array object, virtual buffer object, element buffer object, ---//---
    private static int _vao, _vbo, _ebo, _shaderProgram;
    
    // ? window can be null during initialization
    private static GameWindow? _window;
    
    //View and projection matrices used for rendering and transformations
    private static Matrix4 _projection, _view;
    
    // ! To be renamed in the future and to be used with a new type  
    public static List<Sphere> Spheres = new List<Sphere>();
    
    //vertex and index arrays
    private static float[]?  _vertices;
    private static uint[]? _indices;
    
    //GUI 
    private static ImGuiController? _controller;
    
    //Initalize grid
    private static Grid? _grid;
    //Initialize camera
    public static Camera? _camera;

    
    // ! After program ends delete all objects used by OpenGL and objects allocated at runtime
    public static void ResourceCleanup()
    {
        
        int elapsedtime = DateTime.Now.Millisecond;
        Console.WriteLine("Starting Resource Cleanup...\n");
        
        
        Console.WriteLine("Deleting Primary buffers...");
        // Delete VAO, VBO, and EBO
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
        
        
        Console.WriteLine("Deleting Shader programs...");
        // Delete shader program
        GL.DeleteProgram(_shaderProgram);
        
        
        Console.WriteLine("Deleting Grid buffers...");
        //Delete buffers
        GL.DeleteVertexArray(Objects.Grid._vao);
        GL.DeleteBuffer(Objects.Grid._vbo);
        GL.DeleteBuffer(Objects.Grid._ebo);
        
        Console.WriteLine("Deleting Objects...\n");
        // Deallocate the objects
        Spheres.Clear();
        
        
        Console.WriteLine($"Resource cleanup completed in {DateTime.Now.Millisecond-elapsedtime} ms.");
    }
    
    public static void OnLoad()
    {
        GL.ClearColor(Color.Black);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        _window = WindowManager.GetWindow();
        _controller = new ImGuiController(_window.Size.X, _window.Size.Y);
        
        // Initialize matrices
        _projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(45f),
            _window.Size.X / (float)_window.Size.Y,
            0.1f,
            100f
        );

        // ! We're aware that camera is declared as possibly null, however Camera class is never null when passing references
        _view = _camera!.GetViewMatrix();
        
        //new grid instance
        _grid = new Grid(size: 200, step: 1.0f);
        
        
        // Generate sphere vertices and indices (or cube, etc.)
        var sphereData = Sphere.GenerateSphere(.2f, 36, 18);
        _vertices = sphereData.Vertices;
        _indices = sphereData.Indices;

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

        // Shader setup
        _shaderProgram = Shader.CreateShaderProgram();
        GL.UseProgram(_shaderProgram);

        // Attribute pointers
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        // Get uniform locations of respective matrices
        int modelLoc = GL.GetUniformLocation(_shaderProgram, "model_matrix");
        int viewLoc = GL.GetUniformLocation(_shaderProgram, "view_matrix");
        int projLoc = GL.GetUniformLocation(_shaderProgram, "projection_matrix");

        // Set projection and view matrices
        GL.UniformMatrix4(projLoc, false, ref _projection);
        GL.UniformMatrix4(viewLoc, false, ref _view);
        
        // Add initial objects
        Spheres.Add(new Sphere(new Vector3(-2, 0, 0), Vector3.Zero, Vector3.One, orbitRadius: 3.0f, speed: 1.0f));
        Spheres.Add(new Sphere(new Vector3(2, 0, 0), Vector3.Zero, new(0.5f, 0.5f, 0.5f), orbitRadius: 2.0f, speed: 0.5f));
        
        // ! Check correct initialization
        if (!_window.Exists)
        {
            _window.Close();
            Console.WriteLine("Game Window not initialized.");
        }
        
    }

    // ? Called each frame
    public static void OnUpdate(FrameEventArgs args)
    {
        
        _controller!.Update(_window!, (float)args.Time);
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.UseProgram(_shaderProgram);
        GL.BindVertexArray(_vao);

        // Update the view matrix using the shared camera
        Matrix4 view = _camera!.GetViewMatrix();
        GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "view_matrix"), false, ref view);


        
        // Recalculate position for each sphere spawned
        foreach (var obj in Spheres)
        {
                obj.UpdateOrbit(args.Time);
        }
        
        // Render each object
        foreach (var obj in Spheres)
        {
            Matrix4 model = obj.GetModelMatrix();
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model_matrix"), false, ref model);
            GL.DrawElements(PrimitiveType.Triangles, _indices!.Length, DrawElementsType.UnsignedInt, 0);
        }

        
        //render the spheres
        GL.DepthFunc(DepthFunction.Lequal);
        _grid!.Render(_shaderProgram, _camera.GetViewMatrix(), _projection);
        GL.DepthFunc(DepthFunction.Less);
        
        // ? Toggles fullscreen - ImGui.DockSpaceOverViewport();
        
        ImGui.ShowMetricsWindow();
        
        _controller.Render();
        
        ImGuiController.CheckGLError("End of Frame");
        //swap the buffer for a new one 
        _window!.SwapBuffers();
    }

    public static void AddObject(Sphere obj)
    {
        Spheres.Add(obj);

    }

    public static void RemoveObject()
    {
        Spheres.RemoveAt(Spheres.Count - 1);
    }

}