// DirectionIndicator.cs

using OpenGL.GUI;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
// Added for MathF
// Added for List
// Added for Concat
// Keep System.Numerics for ImGuiElementContainer compatibility
// Assuming Shader class is here
using Quaternion = OpenTK.Mathematics.Quaternion;
using Vector3 = OpenTK.Mathematics.Vector3;
using Vector4 = OpenTK.Mathematics.Vector4;

namespace OpenGL.Objects; // Assuming Sphere is in this namespace

internal class DirectionIndicator : IDisposable
{
    // Constants for arrow geometry
    private const float TotalLength = 0.8f; // Smaller base size
    private const float ShaftLengthRatio = 0.7f;
    private const float HeadLengthRatio = 1.0f - ShaftLengthRatio;
    private const float HeadRadiusRatio = 2.5f; // Head radius relative to shaft radius
    private const float ShaftRadius = 0.04f * TotalLength;
    private const float ArrowHeadRadius = ShaftRadius * HeadRadiusRatio;
    private const float ArrowShaftLength = TotalLength * ShaftLengthRatio;
    private const float ArrowHeadHeight = TotalLength * HeadLengthRatio;

    private const float epsilon = 0.005f;
    private static int _vao = -1, _vbo = -1, _ebo = -1, _shaderProgram = -1; // Initialize to -1
    private static float[] _vertices = Array.Empty<float>();
    private static uint[] _indices = Array.Empty<uint>();
    private static bool _initialized;

    // If you ever need instance-based disposal
    public void Dispose()
    {
        // This might not be directly used if the class remains static,
        // but follows the IDisposable pattern.
        DisposeResources();
        GC.SuppressFinalize(this); // Prevent finalizer from running if it exists
    }

    public static void Initialize()
    {
        if (_initialized) return;

        try
        {
            GenerateArrowMesh();
            SetupBuffers();
            CompileShaders();
            _initialized = true;
            CheckGLError("DirectionIndicator Initialize Complete");
        }
        catch (Exception ex)
        {
            Logger.WriteLine($"Error initializing DirectionIndicator: {ex.Message}\n{ex.StackTrace}");
            // Optionally dispose of any partially created resources
            DisposeResources(); // Call a static dispose method
            _initialized = false; // Ensure it's marked as not initialized
        }
    }

    private static void GenerateArrowMesh()
    {
        List<float> vertices = new();
        List<uint> indices = new();
        uint currentIndex = 0;

        const int segments = 12; // Increased segments for smoother look

        // --- Shaft (Cylinder along +Z axis) ---
        uint shaftBaseStartIndex = currentIndex;
        for (int i = 0; i <= segments; i++)
        {
            float angle = MathHelper.TwoPi * i / segments;
            float x = MathF.Cos(angle) * ShaftRadius;
            float y = MathF.Sin(angle) * ShaftRadius;
            vertices.AddRange(new[]
                { x, y, 0f, (x / ShaftRadius + 1f) * 0.5f, (y / ShaftRadius + 1f) * 0.5f }); // Pos, UV
            currentIndex++;
        }

        uint shaftBaseEndIndex = currentIndex - 1;


        uint shaftTopStartIndex = currentIndex;
        // Top ring vertices (at the start of the arrowhead)
        for (int i = 0; i <= segments; i++)
        {
            float angle = MathHelper.TwoPi * i / segments;
            float x = MathF.Cos(angle) * ShaftRadius;
            float y = MathF.Sin(angle) * ShaftRadius;
            vertices.AddRange(new[]
            {
                x, y, ArrowShaftLength, (x / ShaftRadius + 1f) * 0.5f, (y / ShaftRadius + 1f) * 0.5f
            }); // Pos, UV
            currentIndex++;
        }

        uint shaftTopEndIndex = currentIndex - 1;


        // Shaft side indices (quads made of two triangles)
        for (uint i = 0; i < segments; i++)
        {
            uint bl = shaftBaseStartIndex + i;
            uint br = shaftBaseStartIndex + i + 1;
            uint tl = shaftTopStartIndex + i;
            uint tr = shaftTopStartIndex + i + 1;

            indices.AddRange(new[] { bl, br, tr }); // Triangle 1
            indices.AddRange(new[] { tr, tl, bl }); // Triangle 2
        }

        // --- Head (Cone along +Z axis) ---
        uint headBaseStartIndex = currentIndex;
        // Base ring vertices (wider radius)
        for (int i = 0; i <= segments; i++)
        {
            float angle = MathHelper.TwoPi * i / segments;
            float x = MathF.Cos(angle) * ArrowHeadRadius;
            float y = MathF.Sin(angle) * ArrowHeadRadius;
            vertices.AddRange(new[]
            {
                x, y, ArrowShaftLength, (x / ArrowHeadRadius + 1f) * 0.5f, (y / ArrowHeadRadius + 1f) * 0.5f
            }); // Pos, UV
            currentIndex++;
        }

        uint headBaseEndIndex = currentIndex - 1;

        // Tip vertex
        vertices.AddRange(new[] { 0f, 0f, ArrowShaftLength + ArrowHeadHeight, 0.5f, 0.5f }); // Pos, UV (center)
        uint tipIndex = currentIndex++;

        // Head side indices (triangles forming the cone)
        for (uint i = 0; i < segments; i++)
            indices.AddRange(new[] { headBaseStartIndex + i, headBaseStartIndex + i + 1, tipIndex });

        // Optional: Head base cap (Connects arrowhead base to shaft top - creates a disc)
        // This is visually better than leaving it open
        uint shaftTopCenterIndex =
            shaftTopStartIndex + segments + 1; // Assume a center vertex if needed, or just use existing
        // We reuse the shaft top vertices for the inner ring of the base cap
        for (uint i = 0; i < segments; i++)
        {
            // Connect Head Base vertex, Shaft Top vertex, next Shaft Top vertex
            indices.AddRange(new[] { headBaseStartIndex + i, shaftTopStartIndex + i, shaftTopStartIndex + i + 1 });
            // Connect Head Base vertex, next Shaft Top vertex, next Head Base vertex
            indices.AddRange(new[]
                { headBaseStartIndex + i, shaftTopStartIndex + i + 1, headBaseStartIndex + i + 1 });
        }

        _vertices = vertices.ToArray();
        _indices = indices.ToArray();
    }


    private static void SetupBuffers()
    {
        if (_vertices.Length == 0 || _indices.Length == 0)
        {
            Logger.WriteLine("Error: Cannot setup buffers for DirectionIndicator with empty mesh data.");
            return; // Prevent GL errors
        }

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

        // Vertex Position attribute (location = 0)
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // Vertex UV/Texture Coordinate attribute (location = 1) - Even if unused by shader, setup is needed if data exists
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(
            1); // Enable it if the shader *might* use it or if you want to keep the VBO structure consistent

        GL.BindVertexArray(0); // Unbind VAO
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // Unbind VBO
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0); // Unbind EBO

        CheckGLError("DirectionIndicator SetupBuffers");
    }

    private static void CompileShaders()
    {
        if (!File.Exists(Constants.dirIndicatorVertexPath) ||
            !File.Exists(Constants.dirIndicatorFragmentPath))
        {
            Logger.WriteLine(
                $"Error: DirectionIndicator shader files not found: {Constants.dirIndicatorVertexPath}, {Constants.dirIndicatorFragmentPath}");
            // Handle error: throw, return, or use fallback shader?
            throw new FileNotFoundException("DirectionIndicator shader not found.");
        }


        _shaderProgram =
            Shader.CreateShaderProgram(Constants.dirIndicatorVertexPath, Constants.dirIndicatorFragmentPath);
        CheckGLError("DirectionIndicator CompileShaders");

        // Optional: Check if shader compiled and linked successfully
        GL.GetProgram(_shaderProgram, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(_shaderProgram);
            Logger.WriteLine($"Error linking DirectionIndicator shader program: {infoLog}");
            // Handle error appropriately
            throw new InvalidOperationException($"Failed to link DirectionIndicator shader: {infoLog}");
        }
    }

    public static void Render(Matrix4 view, Matrix4 projection, System.Numerics.Vector3 color, float alpha,
        Vector3 velocity)
    {
        // 1. Check Initialization and Velocity
        if (!_initialized || _shaderProgram <= 0 || _vao <= 0) // Check if resources are valid
            // Logger.WriteLine("DirectionIndicator.Render skipped: Not initialized or invalid GL objects.");
            return;

        if (velocity.LengthSquared < 0.001f) return; // No direction to indicate

        // 2. Store and Modify OpenGL State (Disable Culling)
        bool cullFaceEnabled = GL.IsEnabled(EnableCap.CullFace);
        // bool depthTestEnabled = GL.IsEnabled(EnableCap.DepthTest); // Depth test should likely remain enabled

        GL.Disable(EnableCap.CullFace); // Disable culling for the indicator
        // GL.Disable(EnableCap.DepthTest); // Optional: Disable depth test if it should always draw on top

        CheckGLError("DirectionIndicator Render - State Setup");


        // 3. Use Shader and Bind VAO
        GL.UseProgram(_shaderProgram);
        GL.BindVertexArray(_vao); // Binds VAO, and associated VBO/EBO

        CheckGLError("DirectionIndicator Render - UseProgram/BindVAO");

        // 4. Calculate Model Matrix
        var position = new Vector3(
            ImGuiElementContainer.position.X,
            ImGuiElementContainer.position.Y,
            ImGuiElementContainer.position.Z
        );

        var direction = velocity.Normalized();
        var defaultDirection = Vector3.UnitZ; // Our arrow points along +Z
        var rotation = Quaternion.Identity; // Default rotation

        // Avoid issues when direction is parallel or anti-parallel to defaultDirection
        float dot = Vector3.Dot(defaultDirection, direction);
        if (MathF.Abs(dot) < 0.9999f) // If not almost parallel/anti-parallel
        {
            var rotationAxis = Vector3.Cross(defaultDirection, direction).Normalized();
            float rotationAngle = MathF.Acos(MathHelper.Clamp(dot, -1.0f, 1.0f)); // Clamp dot product for safety
            rotation = Quaternion.FromAxisAngle(rotationAxis, rotationAngle);
        }
        else if (dot < 0.0f) // Anti-parallel (pointing opposite direction)
        {
            // Rotate 180 degrees around any axis perpendicular to defaultDirection, e.g., Y-axis
            rotation = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.Pi);
        }
        // If dot is close to 1, direction is already aligned, use Identity rotation

        var arrowBasePosition = position + direction * (Indicator.getCurrentIndicatorRadius() + epsilon);

        var scaleMatrix = Matrix4.CreateScale(Indicator.getCurrentIndicatorRadius());

        var model = scaleMatrix * // Use geometry constants for size control now
                    Matrix4.CreateFromQuaternion(rotation) *
                    Matrix4.CreateTranslation(arrowBasePosition); // Apply translation last


        // 5. Set Uniforms
        int modelLoc = GL.GetUniformLocation(_shaderProgram, "model");
        int viewLoc = GL.GetUniformLocation(_shaderProgram, "view");
        int projLoc = GL.GetUniformLocation(_shaderProgram, "projection");
        int colorLoc = GL.GetUniformLocation(_shaderProgram, "color"); // Shader expects 'color' (vec4)

        if (modelLoc != -1) GL.UniformMatrix4(modelLoc, false, ref model);
        else Logger.WriteLine("Warning: model uniform not found in dirIndicator shader.");
        if (viewLoc != -1) GL.UniformMatrix4(viewLoc, false, ref view);
        else Logger.WriteLine("Warning: view uniform not found in dirIndicator shader.");
        if (projLoc != -1) GL.UniformMatrix4(projLoc, false, ref projection);
        else Logger.WriteLine("Warning: projection uniform not found in dirIndicator shader.");

        // Convert System.Numerics.Vector3 to OpenTK's Vector4 for the uniform
        var finalColor = new Vector4(color.X, color.Y, color.Z, alpha);
        if (colorLoc != -1) GL.Uniform4(colorLoc, finalColor);
        else Logger.WriteLine("Warning: color uniform not found in dirIndicator shader.");

        CheckGLError("DirectionIndicator Render - Set Uniforms");


        // 6. Draw
        if (_indices.Length > 0)
        {
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            CheckGLError("DirectionIndicator Render - DrawElements");
        }
        else
        {
            Logger.WriteLine("Warning: Skipping DrawElements for DirectionIndicator, no indices.");
        }


        // 7. Clean Up State
        GL.BindVertexArray(0); // Unbind VAO !! IMPORTANT !!
        GL.UseProgram(0); // Unbind Shader (good practice)

        // Restore previous OpenGL state
        if (cullFaceEnabled) GL.Enable(EnableCap.CullFace);
        // if (!depthTestEnabled) GL.Disable(EnableCap.DepthTest); // Restore depth test if it was changed

        CheckGLError("DirectionIndicator Render - State Restore");
    }

    // Implement IDisposable properly if instances are created,
    // but for a static class, provide a static Dispose method.
    public static void DisposeResources()
    {
        // Check for valid handles before deleting
        if (_vao != -1) GL.DeleteVertexArray(_vao);
        if (_vbo != -1) GL.DeleteBuffer(_vbo);
        if (_ebo != -1) GL.DeleteBuffer(_ebo);
        if (_shaderProgram != -1) GL.DeleteProgram(_shaderProgram);

        // Reset handles to indicate they are invalid
        _vao = _vbo = _ebo = _shaderProgram = -1;
        _vertices = Array.Empty<float>();
        _indices = Array.Empty<uint>();
        _initialized = false; // Mark as uninitialized

        // Logger.WriteLine("DirectionIndicator resources disposed."); // Optional log
    }

    // Static Error Check Helper
    private static void CheckGLError(string stage)
    {
#if DEBUG
        var error = GL.GetError();
        if (error != ErrorCode.NoError) Logger.WriteLine($"OpenGL Error ({stage}) in DirectionIndicator: {error}");
        // System.Diagnostics.Debugger.Break(); // Optional: break in debugger
#endif
    }
}