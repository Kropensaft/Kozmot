using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGL.Objects;

/// <summary>
/// </summary>
[Obsolete("For now RayTracing will not be implemented for this cause", true)]
public static class ObjectPicker
{
    private static readonly int _lineVao;
    private static readonly int _lineVbo;
    private static readonly int _lineShaderProgram; // This is the shader program ID (int)

    // Location of uniforms in the shader
    private static readonly int _colorLocation;
    private static readonly int _modelLocation;
    private static readonly int _viewLocation;
    private static readonly int _projectionLocation;


    static ObjectPicker()
    {
        // Compile shaders and get uniform locations
        _lineShaderProgram = Shader.CreateShaderProgram(Constants.LineVertPath, Constants.LineFragPath);

        // Get uniform locations
        _colorLocation = GL.GetUniformLocation(_lineShaderProgram, "color");
        _modelLocation = GL.GetUniformLocation(_lineShaderProgram, "model");
        _viewLocation = GL.GetUniformLocation(_lineShaderProgram, "view");
        _projectionLocation = GL.GetUniformLocation(_lineShaderProgram, "projection");

        // Generate VAO and VBO for the line
        _lineVao = GL.GenVertexArray();
        _lineVbo = GL.GenBuffer();

        GL.BindVertexArray(_lineVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _lineVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, 2 * Vector3.SizeInBytes, (Vector3[])null, BufferUsageHint.DynamicDraw);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.BindVertexArray(0);
    }

    public static Vector3? GetClickedObjectPosition(Camera camera, MouseState mouseState,
        MouseButtonEventArgs mouseClick, Vector2 windowSize, Matrix4 projectionMatrix)
    {
        // Debug: Print mouse and window coordinates
        Console.WriteLine($"\n Mouse: ({mouseState.X}, {mouseState.Y}), Window: {windowSize}");

        var ndc = ScreenToNDC(new Vector2(mouseState.X, mouseState.Y), windowSize);
        Console.WriteLine($"NDC: {ndc}");

        var ray = ScreenPointToRay(camera, ndc, projectionMatrix);

        Console.WriteLine($"Ray Origin: {ray.Origin}, Direction: {ray.Direction}");
        Console.WriteLine($"Camera: {camera.Position}");

        // ? Draw the Ray for debugging
        DrawRay(ray.Origin, ray.Direction, new Vector3(0f, 1f, 0f));

        // Debug: Print all spheres being checked
        Console.WriteLine($"Checking {Renderer.Spheres.Count} spheres:");
        foreach (var sphere in Renderer.Spheres)
        {
            Console.WriteLine($"- {sphere.Name}: Pos={sphere.Position}, Scale={sphere.Scale}");
            if (RaySphereIntersection(ray, sphere.Position, sphere.Scale.X / 2f, out var hit))
            {
                Console.WriteLine($"HIT: {hit}");
                return hit - sphere.Scale;
            }
        }

        Console.WriteLine("");
        return null;
    }

    private static Vector2 ScreenToNDC(Vector2 screenPos, Vector2 windowSize)
    {
        //? Clamp between <-1, 1>
        screenPos.X = screenPos.X / (windowSize.X * 0.5f) - 1.0f;
        screenPos.Y = screenPos.Y / (windowSize.Y * 0.5f) - 1.0f;

        return new Vector2(
            2f * screenPos.X / windowSize.X - 1f,
            1f - 2f * screenPos.Y / windowSize.Y
        );
    }

    private static Ray ScreenPointToRay(Camera camera, Vector2 ndc, Matrix4 projection)
    {
        // Step 1: Create clip space coordinates
        var clipCoords = new Vector4(ndc.X, ndc.Y, -1.0f, 1.0f);

        // Step 2: Convert to view space using matrix multiplication
        var invProj = projection.Inverted();
        var viewCoords = invProj * clipCoords; // Use matrix multiplication operator
        viewCoords.Z = 1.0f;
        viewCoords.W = 0.0f;

        // Step 3: Convert to world space using matrix multiplication
        var invView = camera.GetViewMatrix().Inverted();
        var worldCoords = invView * viewCoords; // Use matrix multiplication operator

        var worldDir = new Vector3(worldCoords.X, worldCoords.Y, worldCoords.Z).Normalized();

        return new Ray(camera.Position, worldDir);
    }

    private static bool RaySphereIntersection(Ray ray, Vector3 center, float radius, out Vector3 intersectionPoint)
    {
        var oc = ray.Origin - center;
        float a = Vector3.Dot(ray.Direction, ray.Direction);
        float b = 2f * Vector3.Dot(oc, ray.Direction);
        float c = Vector3.Dot(oc, oc) - radius * radius;
        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            intersectionPoint = Vector3.Zero;
            return false;
        }

        float t = (-b - MathF.Sqrt(discriminant)) / (2f * a);
        if (t < 0)
        {
            t = (-b + MathF.Sqrt(discriminant)) / (2f * a);
            if (t < 0)
            {
                intersectionPoint = Vector3.Zero;
                return false;
            }
        }

        intersectionPoint = ray.Origin + ray.Direction * t;
        return true;
    }

    public static void DrawRay(Vector3 origin, Vector3 direction, Vector3 color)
    {
        // Calculate the end point
        var end = origin + direction * 100f;
        Vector3[] vertices = { origin, end };

        // Update VBO
        GL.BindBuffer(BufferTarget.ArrayBuffer, _lineVbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * Vector3.SizeInBytes, vertices);

        // Use shader program
        GL.UseProgram(_lineShaderProgram);

        // Set uniforms
        GL.Uniform3(_colorLocation, color);

        var model = Matrix4.Identity;
        var view = Renderer._camera.GetViewMatrix();
        var projection = Renderer.GetProjectionMatrix();

        GL.UniformMatrix4(_modelLocation, false, ref model);
        GL.UniformMatrix4(_viewLocation, false, ref view);
        GL.UniformMatrix4(_projectionLocation, false, ref projection);

        // Draw the line
        GL.BindVertexArray(_lineVao);
        GL.DrawArrays(PrimitiveType.Lines, 0, 2);
        GL.BindVertexArray(0);
    }
}

public struct Ray
{
    public Vector3 Origin;
    public Vector3 Direction;

    public Ray(Vector3 origin, Vector3 direction)
    {
        Origin = origin;
        Direction = direction.Normalized();
    }
}