using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenGL;

public static class TextureLoader
{
    public static int LoadTexture(string path)
    {
        // Add missing error handling and format support
        string[] extensions = { ".png", ".jpg", ".jpeg" };
        string validPath = extensions.Select(ext => path + ext).FirstOrDefault(File.Exists);

        if (validPath == null)
        {
            Logger.WriteLine($"Texture not found: {path}");
            return 0; // Consider a fallback texture
        }

        using Image<Rgba32> image = Image.Load<Rgba32>(validPath);
        //image.Mutate(x => x.Flip(FlipMode.Vertical));
        

        byte[] pixels = new byte[4 * image.Width * image.Height];
        image.CopyPixelDataTo(pixels);

        int textureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureId);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
            image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        return textureId;
    }
}