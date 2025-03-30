using System.Drawing;
using ImGuiNET;
using OpenGL.GUI;
using OpenGL.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace OpenGL;

/// <summary>
///     Responsible for all of the 3D rendering, houses the main logic for OnNewFrameUpdate, ALL GL settings should be
///     applied in OnLoad()
/// </summary>
internal static class Renderer
{
    //virtual array object, virtual buffer object, element buffer object, ---//---
    private static int _vao, _vbo, _ebo;


    private static readonly Dictionary<string, int> _shaderPrograms = new();

    // ? window can be null during initialization
    private static GameWindow? _window;

    //View and projection matrices used for rendering and transformations
    private static Matrix4 _projection, _view;

    // ! To be renamed in the future and to be used with a new type  
    public static List<Sphere> Spheres = new();

    //vertex and index arrays
    private static float[]? _vertices;
    private static uint[]? _indices;

    //GUI 
    private static ImGuiController? _controller;
    private static bool UIinitcalled;
    public static bool RenderIndicator = true;

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
        foreach (int shader in _shaderPrograms.Values)
            GL.DeleteProgram(shader);


        Console.WriteLine("Deleting Grid buffers...");
        //Delete buffers
        GL.DeleteVertexArray(Grid._vao);
        GL.DeleteBuffer(Grid._vbo);
        GL.DeleteBuffer(Grid._ebo);

        Console.WriteLine("Deleting Objects...");
        // Deallocate the objects
        Spheres.Clear();

        // ? ImGui
        Console.WriteLine("Deleting ImGui Buffers...\n");
        ImGuiController.DestroyDeviceObjects();

        Console.WriteLine($"Resource cleanup completed in {DateTime.Now.Millisecond - elapsedtime} ms.");
    }

    public static void OnLoad()
    {
        GL.ClearColor(Color.Black);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        
         Indicator.Initialize();
        _window = WindowManager.GetWindow();
        _controller = new ImGuiController(_window.Size.X, _window.Size.Y);

        // Initialize matrices
        _projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(Constants.PROJECTION_MATRIX_RADIAN_CONSTANT),
            _window.Size.X / (float)_window.Size.Y,
            Constants.NEAR_DEPTH_CONSTANT,
            Constants.FAR_DEPTH_CONSTANT
        );

        // ! We're aware that camera is declared as possibly null, however Camera class is never null when passing references_view = _camera!.GetViewMatrix();

        _shaderPrograms["grid"] =
            Shader.CreateShaderProgram(Constants.gridVertexShaderPath, Constants.gridFragmentShaderPath);
        _shaderPrograms["default"] =
            Shader.CreateShaderProgram(Constants.vertexShaderPath, Constants.fragmentShaderPath);
        //new grid instance
        _grid = new Grid(Constants.GRID_SIZE);


        //? Generate a new sphere
        var sphere = new Sphere(
            "Test Sphere 2",
            new Vector3(4, 0, 0),
            Vector3.Zero,
            new Vector3(2f, 2f, 2f),
            new System.Numerics.Vector3(0f, 0.5f, 0.5f),
            1.0f,
            2.0f,
            0.5f
        );
        Spheres.Add(sphere);
        ImGuiElementContainer.celestialBodies.Add(sphere);


        (float[] Vertices, uint[] Indices) sphereData = Spheres.ElementAt(Spheres.Count - 1)
            .GenerateSphere(Constants.SPHERE_SECTOR_COUNT, Constants.SPHERE_STACK_COUNT);


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


        // Attribute pointers
        GL.VertexAttribPointer(0, Constants.VERTEX_ATRIBB_SIZE,
            VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, Constants.VERTEX_ATRIBB_SIZE,
            VertexAttribPointerType.Float, false, 6 * sizeof(float), Constants.VERTEX_ATRIBB_SIZE * sizeof(float));
        GL.EnableVertexAttribArray(1);

        // Get uniform locations of respective matrices
        //int colorLoc = GL.GetUniformLocation(_shaderPrograms["default"], "object_color");
        //int modelLoc = GL.GetUniformLocation(_shaderPrograms["default"], "model_matrix");
        int viewLoc = GL.GetUniformLocation(_shaderPrograms["default"], "view_matrix");
        int projLoc = GL.GetUniformLocation(_shaderPrograms["default"], "projection_matrix");

        GL.UseProgram(_shaderPrograms["default"]);

        // Set projection and view matrices
        GL.UniformMatrix4(projLoc, false, ref _projection);
        GL.UniformMatrix4(viewLoc, false, ref _view);


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
        GL.UseProgram(_shaderPrograms["default"]);
        GL.BindVertexArray(_vao);

        // Update the view matrix using the shared camera
        var view = _camera!.GetViewMatrix();
        GL.UniformMatrix4(GL.GetUniformLocation(_shaderPrograms["default"], "view_matrix"), false, ref view);


        // Recalculate position for each sphere spawned
        foreach (var obj in Spheres) obj.Update(args.Time);

        // Render each object
        foreach (var obj in Spheres)
        {
            var model = obj.GetModelMatrix();
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderPrograms["default"], "model_matrix"), false, ref model);

            int colorLoc = GL.GetUniformLocation(_shaderPrograms["default"], "object_color");
            GL.Uniform3(colorLoc, obj.Color.X, obj.Color.Y, obj.Color.Z);

            GL.DrawElements(PrimitiveType.Triangles, _indices!.Length, DrawElementsType.UnsignedInt, 0);
        }

    
        //render the spheres
        GL.DepthFunc(DepthFunction.Lequal);
        GL.UseProgram(_shaderPrograms["grid"]);
        _grid!.Render(_shaderPrograms["grid"], _camera.GetViewMatrix(), _projection);
        GL.DepthFunc(DepthFunction.Less);

        // ? Toggles GUI fullscreen - ImGui.DockSpaceOverViewport();


        //? Call to our own bespoke UI        
        ImGuiElementContainer.SubmitUI();

        //? Makes it so that on program start the UI is reset and only at that point
        if (!UIinitcalled)
        {
            ImGuiElementContainer.ResetUI();
            UIinitcalled = true;
        }
        
        var IndColor = (Constants.INDICATOR_COLOR == Vector3.Zero ? Constants.INDICATOR_COLOR_DEF : Constants.INDICATOR_COLOR); 
        var IndFloat = (Constants.INDICATOR_ALPHA == 0.0f ? Constants.INDICATOR_ALPHA_DEF : Constants.INDICATOR_ALPHA);
        
        //? Render the indicator after the spheres
        if(RenderIndicator)
            Indicator.Render(_camera!.GetViewMatrix(), _projection, IndColor, IndFloat);
        
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

    public static ImGuiController GetController()
    {
        return _controller!;
    }

    public static Matrix4 GetProjectionMatrix()
    {
        return _projection; // Assuming you store it in Renderer
    }
}