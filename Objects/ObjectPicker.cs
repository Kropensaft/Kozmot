using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGL.Objects;

public static class ObjectPicker
{
    public static Vector3? GetClickedObjectPosition(Camera camera, MouseState mouseState, MouseButtonEventArgs mouseClick, Vector2 windowSize, Matrix4 projectionMatrix)
    {
        // Debug: Print mouse and window coordinates
        Console.WriteLine($"Mouse: ({mouseState.X}, {mouseState.Y}), Window: {windowSize}");

        Vector2 ndc = ScreenToNDC(new Vector2(mouseState.X, mouseState.Y), windowSize);
        Console.WriteLine($"NDC: {ndc}");

        Ray ray = ScreenPointToRay(camera, ndc, projectionMatrix);
        Console.WriteLine($"Ray Origin: {ray.Origin}, Direction: {ray.Direction}");
        Console.WriteLine($"Camera: {camera.Position}");
        
        
        // Debug: Print all spheres being checked
        Console.WriteLine($"Checking {Renderer.Spheres.Count} spheres:");
        foreach (var sphere in Renderer.Spheres)
        {
            Console.WriteLine($"- {sphere.Name}: Pos={sphere.Position}, Scale={sphere.Scale}");
            if (RaySphereIntersection(ray, sphere.Position, sphere.Scale.X / 2f, out Vector3 hit))
            {
                Console.WriteLine($"HIT: {hit}");
                return hit;
            }
        }
        return null;
    }

    private static Vector2 ScreenToNDC(Vector2 screenPos, Vector2 windowSize)
    {
        return new Vector2(
            2f * screenPos.X / windowSize.X - 1f,
            1f - 2f * screenPos.Y / windowSize.Y
        );
    }

    private static Ray ScreenPointToRay(Camera camera, Vector2 ndc, Matrix4 projection)
    {
        // Step 1: Create clip space coordinates
        Vector4 clipCoords = new Vector4(ndc.X, ndc.Y, -1.0f, 1.0f);

        // Step 2: Convert to view space using matrix multiplication
        Matrix4 invProj = projection.Inverted();
        Vector4 viewCoords = invProj * clipCoords; // Use matrix multiplication operator
        viewCoords.Z = -1.0f;
        viewCoords.W = 0.0f;

        // Step 3: Convert to world space using matrix multiplication
        Matrix4 invView = camera.GetViewMatrix().Inverted();
        Vector4 worldCoords = invView * viewCoords; // Use matrix multiplication operator
    
        Vector3 worldDir = new Vector3(worldCoords.X, worldCoords.Y, worldCoords.Z).Normalized();

        return new Ray(camera.Position, worldDir);
    }

    private static bool RaySphereIntersection(Ray ray, Vector3 center, float radius, out Vector3 intersectionPoint)
    {
        Vector3 oc = ray.Origin - center;
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