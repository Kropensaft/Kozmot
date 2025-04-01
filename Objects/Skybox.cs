using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.IO;
using StbImageSharp;

namespace OpenGL.Objects
{
    public class Skybox
    {
        private readonly int _vao;
        private readonly int _vbo;
        private readonly int _shaderProgram;
        private readonly int _textureId;

        public Skybox()
        {
            _shaderProgram = Shader.CreateShaderProgram(
                Constants.skyboxVertexShaderPath,
                Constants.skyboxFragmentShaderPath
            );

            _textureId = LoadCubeMap(Constants.SkyboxTexturePaths);

            // VAO/VBO setup
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Constants._skyboxVertices.Length * sizeof(float),
                        Constants._skyboxVertices, BufferUsageHint.StaticDraw);

            // EBO setup
            var ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Constants._skyboxIndices.Length * sizeof(uint),
                        Constants._skyboxIndices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);
        }

        private static int LoadCubeMap(string[] texturePaths)
        {
            int textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, textureID);

            for (int i = 0; i < texturePaths.Length; i++)
            {
                using var stream = File.OpenRead(texturePaths[i]);
                var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0,
                    PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                    PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            return textureID;
        }

        public void Render(Matrix4 view, Matrix4 projection)
        {
            GL.DepthFunc(DepthFunction.Lequal);
            GL.UseProgram(_shaderProgram);

            var viewNoTranslation = new Matrix4(new Matrix3(view));
            int viewLoc = GL.GetUniformLocation(_shaderProgram, "view");
            int projLoc = GL.GetUniformLocation(_shaderProgram, "projection");
            GL.UniformMatrix4(viewLoc, false, ref viewNoTranslation);
            GL.UniformMatrix4(projLoc, false, ref projection);

            GL.BindVertexArray(_vao);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, _textureId);
            GL.DrawElements(PrimitiveType.Triangles, Constants._skyboxIndices.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            GL.DepthFunc(DepthFunction.Less);
        }
    }
}