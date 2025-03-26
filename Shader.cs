using OpenTK.Graphics.OpenGL4;

namespace OpenGL;

/// <summary>
///     Main shader program manager, responsible for object shading not grid or other classes with their respective shaders
/// </summary>
internal static class Shader
{
    

    public static int CreateShaderProgram(string vertexShaderPath, string fragmentShaderPath)
    {
        //Create the shader.vert/frag codes from their respective files
        string vertexShaderCode = File.ReadAllText(vertexShaderPath);
        string fragmentShaderCode = File.ReadAllText(fragmentShaderPath);


        //Compile each shader respectively
        int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderCode);
        int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderCode);


        //create new shader program
        int program = GL.CreateProgram();
        //attach frag and vert to program
        GL.AttachShader(program, vertexShader);
        GL.AttachShader(program, fragmentShader);
        //link program to GPU
        GL.LinkProgram(program);

        //If shader program doesn't link throw an exception
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int linkStatus);
        if (linkStatus == (int)All.False)
            throw new Exception("Program linking failed: " + GL.GetProgramInfoLog(program));


        //since we've compiled and linked the program we can now delete the unneeded data from GPU memory
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        return program;
    }
    
    private static int CompileShader(ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        //If compilation fails throw an exception
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
        if (status == (int)All.False)
            throw new Exception($"{type} shader compilation failed: " + GL.GetShaderInfoLog(shader));

        return shader;
    }
}