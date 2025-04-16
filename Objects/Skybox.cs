using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

// For image loading

// For Path operations

namespace OpenGL.Objects;

internal class Skybox : IDisposable
{
    private readonly int _shaderProgram;
    private readonly int _textureId;
    private readonly int _vao;
    private readonly int _vbo;


    /// <summary>
    ///     Creates and initializes the Skybox.
    /// </summary>
    /// <param name="facePaths">
    ///     Array of 6 paths to the cubemap faces in the order:
    ///     Right (+X), Left (-X), Top (+Y), Bottom (-Y), Front (+Z), Back (-Z)
    /// </param>
    /// <param name="shaderBasePath">Path to the directory containing skybox.vert and skybox.frag</param>
    public Skybox(string[] facePaths, string shaderBasePath = "Shaders")
    {
        if (facePaths == null || facePaths.Length != 6)
            throw new ArgumentException("Must provide exactly 6 face paths for the cubemap.", nameof(facePaths));

        string vertexShaderPath = Path.Combine(shaderBasePath, "skybox.vert");
        string fragmentShaderPath = Path.Combine(shaderBasePath, "skybox.frag");

        // 1. Load Shaders
        _shaderProgram =
            Shader.CreateShaderProgram(vertexShaderPath, fragmentShaderPath); // Use your existing Shader helper
        CheckGLError("Skybox Shader Creation");


        // 2. Load Cubemap Texture
        _textureId = LoadCubemap(facePaths);
        CheckGLError("Skybox Texture Loading");


        // 3. Setup VAO and VBO
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);
        CheckGLError("Skybox Gen/Bind VAO");


        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, Constants._skyboxVertices.Length * sizeof(float),
            Constants._skyboxVertices,
            BufferUsageHint.StaticDraw);
        CheckGLError("Skybox VBO Setup");


        // Vertex Positions attribute (location = 0)
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        CheckGLError("Skybox Attrib Pointer");


        // Unbind VAO (good practice)
        GL.BindVertexArray(0);
        // Unbind VBO (it's associated with the VAO state now)
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


        CheckGLError("Skybox Initialization Complete");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Renders the skybox.
    /// </summary>
    /// <param name="view">The camera's view matrix.</param>
    /// <param name="projection">The camera's projection matrix.</param>
    public void Render(Matrix4 view, Matrix4 projection)
    {
        CheckGLError("Skybox Render Start");

        // --- Optimization/Depth Handling ---
        // Change depth function so fragments pass when depth is less than or equal to existing depth.
        // Since the skybox vertex shader sets z=w (max depth), it will pass only if the buffer is empty (cleared)
        // or contains the skybox's own depth from a previous frame.
        GL.DepthFunc(DepthFunction.Lequal);
        CheckGLError("Skybox Set Depth Func");


        // Alternative: Disable depth writing entirely (skybox drawn first)
        // GL.DepthMask(false);

        // --- Shader and State ---
        GL.UseProgram(_shaderProgram);
        CheckGLError("Skybox Use Shader");
        GL.BindVertexArray(_vao);
        CheckGLError("Skybox Bind VAO/Program");


        // --- View Matrix Manipulation ---
        // Remove the translation component from the view matrix
        // This makes the skybox follow camera rotation but not position
        var viewNoTranslation = view.ClearTranslation(); // OpenTK Extension
        // Or manually: var view3x3 = new Matrix3(view); var viewNoTranslation = new Matrix4(view3x3);

        // --- Set Uniforms ---
        int viewLoc = GL.GetUniformLocation(_shaderProgram, "view");
        int projLoc = GL.GetUniformLocation(_shaderProgram, "projection");
        int skyboxSamplerLoc = GL.GetUniformLocation(_shaderProgram, "skyboxSampler");

        if (viewLoc != -1) GL.UniformMatrix4(viewLoc, false, ref viewNoTranslation);
        if (projLoc != -1) GL.UniformMatrix4(projLoc, false, ref projection);

        // --- Texture Binding ---
        GL.ActiveTexture(TextureUnit.Texture0); // Activate texture unit 0
        CheckGLError("Skybox Texture Loading");
        GL.BindTexture(TextureTarget.TextureCubeMap, _textureId); // Bind the cubemap texture
        if (skyboxSamplerLoc != -1) GL.Uniform1(skyboxSamplerLoc, 0); // Tell shader sampler to use texture unit 0
        CheckGLError("Skybox Set Uniforms & Texture");


        // --- Draw ---
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36); // 36 vertices for a cube (6 faces * 2 triangles * 3 vertices)
        CheckGLError("Skybox DrawArrays");


        // --- Restore State ---
        GL.BindVertexArray(0); // Unbind VAO
        CheckGLError("Skybox Vertex Binding");
        GL.BindTexture(TextureTarget.TextureCubeMap, 0); // Unbind cubemap texture
        CheckGLError("Skybox Bind Texture");

        // Restore default depth function
        GL.DepthFunc(DepthFunction.Less);
        CheckGLError("Skybox Restore Depth Func");


        // Alternative: Re-enable depth writing if it was disabled
        // GL.DepthMask(true);
    }


    /// <summary>
    ///     Loads the 6 faces of a cubemap texture.
    /// </summary>
    /// <param name="faces">Array of 6 file paths (Right, Left, Top, Bottom, Front, Back).</param>
    /// <returns>The OpenGL texture ID.</returns>
    private int LoadCubemap(string[] faces)
    {
        int textureID = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureCubeMap, textureID);

        // Load images using StbImageSharp
        StbImage.stbi_set_flip_vertically_on_load(
            0); // Cubemaps often don't need flipping, adjust if textures are upside down

        for (int i = 0; i < faces.Length; i++)
        {
            string path = faces[i];
            if (!File.Exists(path))
            {
                Logger.WriteLine($"Error: Cubemap texture file not found: {path}");
                // Handle error appropriately - maybe load a placeholder?
                continue; // Skip this face
            }

            try
            {
                using (var stream = File.OpenRead(path))
                {
                    var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha); // Request RGBA

                    if (image.Data != null && image.Width > 0 && image.Height > 0)
                    {
                        // Target faces in OpenGL enum order: +X, -X, +Y, -Y, +Z, -Z
                        var targetFace = TextureTarget.TextureCubeMapPositiveX + i;
                        GL.TexImage2D(targetFace,
                            0, // Level
                            PixelInternalFormat.Rgba, // Internal format
                            image.Width,
                            image.Height,
                            0, // Border
                            PixelFormat.Rgba, // Format of pixel data
                            PixelType.UnsignedByte, // Data type of pixel data
                            image.Data); // Pixel data
#if DEBUG
                        if (Debugger.IsAttached)
                            Logger.WriteLine(
                                $"Loaded cubemap face: {targetFace} ({path}) Width={image.Width}, Height={image.Height}");
#endif
                    }
                    else
                    {
                        Logger.WriteLine($"Error: Failed to load image data from {path}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Error loading cubemap face {path}: {ex.Message}");
            }
        }

        // Set texture parameters
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR,
            (int)TextureWrapMode.ClampToEdge); // Wrap R for the 3rd dimension

        GL.BindTexture(TextureTarget.TextureCubeMap, 0); // Unbind
        return textureID;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Cleanup managed resources if any in the future
        }

        // Cleanup unmanaged OpenGL resources
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteTexture(_textureId);
        GL.DeleteProgram(
            _shaderProgram); // Ensure your Shader class doesn't double-delete if it tracks shaders globally
    }

    // Optional: Add finalizer for safety, although explicit Dispose is better
    ~Skybox()
    {
        Dispose(false);
        Logger.WriteLine("Warning: Skybox finalizer called. Dispose() should be used.");
    }

    public static void CheckGLError(string stage)
    {
#if DEBUG // Only run error checks in debug builds for performance
        var error = GL.GetError();
        if (error != ErrorCode.NoError)
        {
            Logger.WriteLine($"OpenGL Error ({stage}): {error}");
            Debugger.Break(); // Break execution in debugger
        }
#endif
    }
}