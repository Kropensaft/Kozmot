using OpenGL.GUI;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

// Needed for Math.Abs

namespace OpenGL;

internal class Indicator
{
    private static int _vao, _vbo, _ebo, _shaderProgram;
    private static float[]? _vertices;
    private static uint[]? _indices;
    private static float _currentRadius = -1f; // Track radius used for mesh generation

    public static float getCurrentIndicatorRadius()
    {
        return Math.Max(0.01f, _currentRadius);
    }

    public static float GetRadii()
    {
        uint selectedRadius = ImGuiElementContainer.selectedPlanetTypeRef;
        // Use Math.Max to ensure radius is positive, just in case constants are zero/negative
        float radii = Math.Max(0.001f, selectedRadius switch
        {
            0 => Constants.ROCKY_PLANET_RADIUS,
            1 => Constants.STAR_RADIUS,
            2 => Constants.GAS_GIANT_RADIUS,
            3 => Constants.MOON_RADIUS,
            4 => Constants.DESERT_PLANET_RADIUS,
            5 => Constants.ICE_GIANT_RADIUS,
            6 => Constants.CUSTOM_RADIUS,
            _ => Constants.ROCKY_PLANET_RADIUS
        });
        // Logger.WriteLine($"GetRadii: {radii}"); // Debug
        return radii;
    }

    /// <summary>
    ///     Generates sphere mesh data in C# arrays if radius changed.
    ///     Does NOT upload to GPU here.
    /// </summary>
    private static void UpdateIndicatorMeshData()
    {
        float newRadius = GetRadii();

        // Avoid regenerating mesh if radius hasn't changed
        if (Math.Abs(newRadius - _currentRadius) < 0.000001f && _vertices != null &&
            _indices != null) return;

        _currentRadius = newRadius;

        if (_currentRadius <= 0)
        {
            Logger.WriteLine($"Warning: Indicator radius is non-positive ({_currentRadius}). Using fallback.");
            _currentRadius = 0.1f; // Fallback small radius
        }


        // Generate sphere mesh
        var sphere = new Sphere("Indicator_Internal",
            Constants.DEFAULT_INDICATOR_POSITION,
            Vector3.Zero,
            new Vector3(_currentRadius, _currentRadius, _currentRadius),
            System.Numerics.Vector3.One,
            0f, // orbitRadius
            0f, // velocity
            ""); // emissive


        try
        {
            (float[] Vertices, uint[] Indices) sphereData =
                sphere.GenerateSphere(Constants.SPHERE_SECTOR_COUNT, Constants.SPHERE_STACK_COUNT);

            // Basic validation of generated data
            if (sphereData.Vertices == null || sphereData.Vertices.Length == 0 || sphereData.Indices == null ||
                sphereData.Indices.Length == 0)
            {
                Logger.WriteLine("Error: Sphere generation returned null or empty data.");
                // Keep old data if possible, or handle error appropriately
                if (_vertices == null || _indices == null)
                {
                    // Create minimal fallback data to avoid null refs later
                    _vertices = new float[] { 0, 0, 0, 0, 0, 0 }; // Single point?
                    _indices = new uint[] { 0 };
                }

                return; // Don't update with bad data
            }

            _vertices = sphereData.Vertices;
            _indices = sphereData.Indices;
        }
        catch (Exception ex)
        {
            Logger.WriteLine($"Error during Sphere.GenerateSphere: {ex.Message}");
            // Handle error: potentially keep old data, log, etc.
            if (_vertices == null || _indices == null)
            {
                _vertices = new float[] { 0, 0, 0, 0, 0, 0 };
                _indices = new uint[] { 0 };
            }
        }
    }

    public static void Initialize()
    {
        // Generate initial mesh data
        UpdateIndicatorMeshData();

        // Ensure initial data is valid before proceeding
        if (_vertices == null || _indices == null || _vertices.Length == 0 || _indices.Length == 0)
        {
            Logger.WriteLine("Error: Indicator failed to initialize mesh data. Cannot create GL buffers.");
            // Handle this critical failure - maybe throw, or set a flag to prevent rendering
            return;
        }

        // Compile shaders
        _shaderProgram = Shader.CreateShaderProgram(
            Constants.indicatorVertexShaderPath,
            Constants.indicatorFragmentShaderPath
        );
        CheckGLError("Indicator Shader Compilation");


        // VAO/VBO setup
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);
        CheckGLError("Indicator Gen/Bind VAO");


        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        // Use DynamicDraw as we intend to update frequently in Render
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices,
            BufferUsageHint.DynamicDraw);
        CheckGLError("Indicator VBO Initialization");


        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices,
            BufferUsageHint.DynamicDraw);
        CheckGLError("Indicator EBO Initialization");


        // Updated attribute pointers for 5 floats per vertex (position + UV)
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // Texture coordinate attribute
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        CheckGLError("Indicator Vertex Attrib Pointers");


        // --- Important: Unbind All After Setup ---
        GL.BindVertexArray(0); // Unbind the VAO
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // Unbind the VBO from the ArrayBuffer target
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0); // Unbind the EBO from the ElementArrayBuffer target
        CheckGLError("Indicator Initialization Unbind");
    }

    public static void Render(Matrix4 view, Matrix4 projection, Vector3 color, float alpha)
    {
        // Don't render if toggled off or not editing
        // Combine your conditions as needed
        if (!Renderer.RenderIndicator || !ImGuiElementContainer.IsEditing) return;

        // 1. Ensure Mesh Data is Ready (regenerates C# arrays if radius changed)
        UpdateIndicatorMeshData();

        // Safety check: Ensure buffers/shader were initialized and mesh data exists
        if (_vao <= 0 || _vbo <= 0 || _ebo <= 0 || _shaderProgram <= 0 || _vertices == null || _indices == null ||
            _indices.Length == 0)
        {
            Logger.WriteLine("Indicator Render skipped: Invalid state (VAO/VBO/EBO/Shader/Mesh Data)."); // Debug
            return;
        }

        // 2. Use Indicator's Shader
        GL.UseProgram(_shaderProgram);
        CheckGLError("Indicator UseProgram");


        // 3. Bind Indicator's VAO
        //    This sets up the VBO/EBO binding *for this VAO* and enables attribute pointers
        GL.BindVertexArray(_vao);
        CheckGLError("Indicator BindVertexArray");


        // --- 4. Update GPU Buffers (Every Frame - As Requested) ---
        // Bind the specific buffers before updating data. Although the VAO
        // remembers which buffers are associated, explicitly binding to the
        // targets (ArrayBuffer, ElementArrayBuffer) is required for BufferData.

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices,
            BufferUsageHint.DynamicDraw);
        CheckGLError($"Indicator VBO Update (Size: {_vertices.Length * sizeof(float)})");


        GL.BindBuffer(BufferTarget.ElementArrayBuffer,
            _ebo); // This binding is implicitly part of the VAO state for drawing, but explicit bind needed for BufferData sometimes.
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices,
            BufferUsageHint.DynamicDraw);
        CheckGLError($"Indicator EBO Update (Size: {_indices.Length * sizeof(uint)})");

        // --- End Buffer Update ---


        // 5. Set Uniforms
        var position = new Vector3(
            ImGuiElementContainer.position.X,
            ImGuiElementContainer.position.Y,
            ImGuiElementContainer.position.Z
        );


        var model = Matrix4.CreateTranslation(position);
        int modelLoc = GL.GetUniformLocation(_shaderProgram, "model");
        int viewLoc = GL.GetUniformLocation(_shaderProgram, "view");
        int projLoc = GL.GetUniformLocation(_shaderProgram, "projection");
        int colorLoc = GL.GetUniformLocation(_shaderProgram, "color");

        if (modelLoc != -1) GL.UniformMatrix4(modelLoc, false, ref model);
        if (viewLoc != -1) GL.UniformMatrix4(viewLoc, false, ref view);
        if (projLoc != -1) GL.UniformMatrix4(projLoc, false, ref projection);
        if (colorLoc != -1) GL.Uniform4(colorLoc, new Vector4(color, alpha));
        CheckGLError("Indicator Set Uniforms");


        // 6. Draw
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        CheckGLError("Indicator DrawElements");


        // --- 7. CRUCIAL: Clean up OpenGL State ---
        // Unbind the VAO. This is the MOST important step to prevent
        // the indicator's VAO state (including its VBO/EBO bindings and
        // attribute pointers) from interfering with subsequent draw calls.
        GL.BindVertexArray(0);

        // Optionally, unbind buffers from the general targets, although BindVertexArray(0)
        // often makes this redundant for the *next* VAO-based draw call.
        // GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        // GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

        // Optionally, unbind the shader program
        // GL.UseProgram(0); // Only if your Renderer doesn't guarantee the next object binds its own shader.

        CheckGLError("Indicator Render Cleanup");
    }

    // No changes needed in Dispose
    public static void Dispose()
    {
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
        GL.DeleteProgram(_shaderProgram);
        _vao = _vbo = _ebo = _shaderProgram = 0; // Mark as disposed
        _vertices = null;
        _indices = null;
    }

    // Optional: Error Checking Helper (Place in a common utility class or here)
    private static void CheckGLError(string stage)
    {
#if DEBUG // Only run error checks in debug builds for performance
        var error = GL.GetError();
        if (error != ErrorCode.NoError) Logger.WriteLine($"OpenGL Error ({stage}): {error}");
        // System.Diagnostics.Debugger.Break(); // Break execution in debugger
#endif
    }
}