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

    public static bool showFPS {get; set;}

    //Initialize the skybox
    private static Skybox? _skybox;

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
        Console.WriteLine("Deleting ImGui Buffers...");
        ImGuiController.DestroyDeviceObjects();


        // ? Indicator
        Console.WriteLine("Deleting Indicator buffers...\n");
        Indicator.Dispose();

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

        try
        {
            _skybox = new Skybox(Constants.SkyboxFaces, "Shaders/");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize Skybox: {ex.Message}");
            _skybox = null; // Prevent rendering if initialization failed
        }

        // Initialize matrices
        _projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(Constants.PROJECTION_MATRIX_RADIAN_CONSTANT),
            _window.Size.X / (float)_window.Size.Y,
            Constants.NEAR_DEPTH_CONSTANT,
            Constants.FAR_DEPTH_CONSTANT
        );

        _shaderPrograms["grid"] =
            Shader.CreateShaderProgram(Constants.gridVertexShaderPath, Constants.gridFragmentShaderPath);
        _shaderPrograms["default"] =
            Shader.CreateShaderProgram(Constants.vertexShaderPath, Constants.fragmentShaderPath);
        //new grid instance
        if (Grid.RenderGrid)
            _grid = new Grid(Constants.GRID_SIZE);

        
        

        //? Generate a new sphere
        var sphere = new Sphere(
            "Default Planet",
            new Vector3(4, 0, 0),
            Vector3.Zero,
            new Vector3(.5f, .5f, .5f),
            new System.Numerics.Vector3(0f, 0.5f, 0.5f),
            1.0f,
            2.0f,
            0.5f,
            Constants.planetTypes[0]
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
        int colorLoc = GL.GetUniformLocation(_shaderPrograms["default"], "object_color");
        int modelLoc = GL.GetUniformLocation(_shaderPrograms["default"], "model_matrix");
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
            throw new Exception("Game Window not initialized.");
        }
    }

    // ? Called each frame
    public static void OnUpdate(FrameEventArgs args)
    {
        // 1. Update ImGui input state (needs to happen early)
        _controller!.Update(_window!, (float)args.Time);

        // 2. Clear Buffers (once per frame)
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // 3. Get Camera Matrices (once per frame)
        float aspectRatio = _window!.Size.X / (float)_window.Size.Y;
        var currentView = _camera!.GetViewMatrix();
        var currentProjection = _camera.GetProjectionMatrix(aspectRatio);
        // Use 'currentProjection' below instead of the potentially stale '_projection' member

        // --- 4. Render Skybox ---
        if (_skybox != null)
        {
            // Skybox.Render handles its own state (shader, VAO, uniforms, depth func)
            _skybox.Render(currentView, currentProjection);
            Skybox.CheckGLError("After Skybox render"); // Use the static method if it exists
        }
        // Skybox.Render should have restored DepthFunc to Less

        // --- 5. Render Spheres ---
        // Set the shader program FOR THE SPHERES
        GL.UseProgram(_shaderPrograms["default"]);
        CheckGLError("Use Default Program for Spheres");

        // Set camera uniforms FOR THE SPHERES SHADER (only need to set once if shader doesn't change)
        GL.UniformMatrix4(GL.GetUniformLocation(_shaderPrograms["default"], "view_matrix"), false, ref currentView);
        GL.UniformMatrix4(GL.GetUniformLocation(_shaderPrograms["default"], "projection_matrix"), false,
            ref currentProjection); // Use currentProjection
        CheckGLError("Set Sphere View/Projection Uniforms");


        // Bind the VAO FOR THE SPHERES
        GL.BindVertexArray(_vao); // <-- Bind the sphere VAO HERE
        CheckGLError("Bind Sphere VAO");

        // Update and Render each sphere object
        // ! Spheres.ToList is crucial since we need the original List only as a reference of objects 
        foreach (var obj in Spheres.ToList())
        {
            if (obj.Position.X > Constants.GRID_SIZE)
            {
                //TODO : If a planet is too far do something
            }

            // Object-specific updates
            obj.Update(args.Time); // Assuming this doesn't change GL state
            
            // Set object-specific uniforms
            var model = obj.GetModelMatrix();
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderPrograms["default"], "model_matrix"), false, ref model);
            GL.Uniform3(GL.GetUniformLocation(_shaderPrograms["default"], "object_color"), obj.Color.X, obj.Color.Y,
                obj.Color.Z);
            CheckGLError($"Set Uniforms for Sphere: {obj.Name}");


            // Draw THIS sphere using the bound VAO and EBO
            // Ensure _indices is not null (should be set in OnLoad)
            if (_indices != null)
            {
                GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
                CheckGLError($"Draw Sphere: {obj.Name}");
            }
        }

        // Unbind Sphere VAO (good practice)
        GL.BindVertexArray(0);
        CheckGLError("Unbind Sphere VAO");

        // --- 6. Render Grid ---
        // Grid.Render should handle its own state (shader, VAO, uniforms, depth func changes)
        // Make sure Grid.Render takes projection matrix as argument
        if (Grid.RenderGrid)
            _grid?.Render(_shaderPrograms["grid"], currentView, currentProjection); // Pass currentProjection
        CheckGLError("After Grid Render");
        // Grid.Render should restore DepthFunc to Less if it changed it

        // --- 7. Render Indicator ---
        // Indicator.Render handles its own state (shader, VAO, uniforms)
        if (RenderIndicator)
        {
            // 1. Determine the base color (now comparing System.Numerics == System.Numerics)
            var baseColorNum = Constants.INDICATOR_COLOR == System.Numerics.Vector3.Zero // This comparison now works
                ? Constants.INDICATOR_COLOR_DEF // This is also System.Numerics.Vector3
                : Constants.INDICATOR_COLOR;

            // 2. Determine the alpha value
            float alpha = Constants.INDICATOR_ALPHA <= 0.0f
                ? Constants.INDICATOR_ALPHA_DEF
                : Constants.INDICATOR_ALPHA;

            // 3. CONVERT TO OPENTK TYPE FOR RENDERING
            var finalIndicatorColorTk = new Vector3(
                baseColorNum.X,
                baseColorNum.Y,
                baseColorNum.Z
            );

            Indicator.Render(currentView, currentProjection, finalIndicatorColorTk, alpha);
            CheckGLError("After Indicator Render");
        }

        // --- 8. Render ImGui UI ---
        // Submit UI definitions
        ImGuiElementContainer.SubmitUI();
        
        if(showFPS)
            ImGui.ShowMetricsWindow();
        CheckGLError("After SubmitUI");

        // Reset UI only once (logic seems okay)
        if (!UIinitcalled)
        {
            ImGuiElementContainer.ResetUI();
            UIinitcalled = true;
        }

        // Render the ImGui draw data
        _controller.Render();
        CheckGLError("After ImGui Render");


        // --- 9. Swap Buffers ---
        ImGuiController.CheckGLError("End of Frame"); // Check error before swap
        _window!.SwapBuffers();
    }

    // Helper Error Check (ensure it's defined or remove calls if not)
    private static void CheckGLError(string stage)
    {
#if DEBUG
        var error = GL.GetError();
        if (error != ErrorCode.NoError) Console.WriteLine($"OpenGL Error ({stage}): {error}");
        // System.Diagnostics.Debugger.Break();
#endif
    }

    public static void AddObject(Sphere obj)
    {
        Spheres.Add(obj);
    }

    public static void RemoveObject(Object obj = null)
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