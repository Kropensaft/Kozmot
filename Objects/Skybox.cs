using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenGL;

public class Skybox : IDisposable
{
    private readonly int _vao, _vbo, _textureId, _shaderProgram;

    public Skybox()
    {
        float[]? vertices =
            // Cube vertices (3 components each)
            [
            // Front face
            -1, -1, 1, 1, -1, 1, 1, 1, 1,
            1, 1, 1, -1, 1, 1, -1, -1, 1,
            // Back face
            -1, -1, -1, -1, 1, -1, 1, 1, -1,
            1, 1, -1, 1, -1, -1, -1, -1, -1,
            // Top face
            -1, 1, -1, -1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, -1, -1, 1, -1,
            // Bottom face
            -1, -1, -1, 1, -1, -1, 1, -1, 1,
            1, -1, 1, -1, -1, 1, -1, -1, -1,
            // Right face
            1, -1, -1, 1, 1, -1, 1, 1, 1,
            1, 1, 1, 1, -1, 1, 1, -1, -1,
            // Left face
            -1, -1, -1, -1, -1, 1, -1, 1, 1,
            -1, 1, 1, -1, 1, -1, -1, -1, -1
        ];

        // In Render()
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36); // 12 triangles × 3 vertices = 36

        // Create buffers
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices,
            BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(
            index: 0,
            size: 3, // Must match vec3 in shader
            type: VertexAttribPointerType.Float,
            normalized: false,
            stride: 3 * sizeof(float),
            offset: 0
        );
        GL.EnableVertexAttribArray(0);

        // Load cubemap texture
        _textureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureCubeMap, _textureId);

        
        //Console.WriteLine("=== Begin Texture Binding ===\n");
        for (int i = 0; i < 6; i++)
        {
            //Console.WriteLine($"Loading: {Constants.SkyboxTexturePaths[i]}");
    
            using var image = Image.Load<Rgba32>(Constants.SkyboxTexturePaths[i]);
            //Console.WriteLine($"Image {i}: {image.Width}x{image.Height}, Format: {image.PixelType}\n");
    
            image.Mutate(x => x.Flip(FlipMode.Vertical)); // Keep/remove based on previous test
    
            var pixels = new byte[4 * image.Width * image.Height];
            image.CopyPixelDataTo(pixels);
    
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, 
                PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
    
            //Console.WriteLine($"Loaded face {i} successfully\n");
        }
        //Console.WriteLine("=== End Texture Binding ===\n");

        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR,
            (int)TextureWrapMode.ClampToEdge);

        // Compile shaders
        _shaderProgram =
            Shader.CreateShaderProgram(Constants.skyboxVertexShaderPath, Constants.skyboxFragmentShaderPath);
        // After shader compilation:
        Console.WriteLine($"Shader Program ID: {_shaderProgram}");
        Console.WriteLine($"Shader Link Status: {GL.GetError()}");
        Console.WriteLine($"Shader Error Log: {GL.GetProgramInfoLog(_shaderProgram)}");
        
        // Add debug output for vertex data
        Console.WriteLine($"Vertex count: {vertices.Length / 3}"); // Should output 36 for 12 triangles
        Console.WriteLine($"First vertex: {vertices[0]}, {vertices[1]}, {vertices[2]}");
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteTexture(_textureId);
        GL.DeleteProgram(_shaderProgram);
    }

    public void Render(Matrix4 view, Matrix4 projection)
    {
        GL.UseProgram(_shaderProgram);
        
        // Set uniforms with exact names
        GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "uProjection"), false, ref projection);
        Matrix4 viewNoTranslation = view.ClearTranslation(); // Implement this extension method
        GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "uView"), false, ref viewNoTranslation);
    
        // Explicit texture unit assignment
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.TextureCubeMap, _textureId);
        GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "skybox"), 0); // Critical
    
        // Depth configuration
        GL.DepthMask(false);
        GL.DepthFunc(DepthFunction.Lequal);
    
        // Draw call
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
    
        // Restore state
        GL.DepthMask(true);
        GL.DepthFunc(DepthFunction.Less);
    }
    
}