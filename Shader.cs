using OpenTK.Graphics.OpenGL4;
using System;

namespace OpenGL;

internal static class Shader
{
    private const string vertexShaderPath = "Shaders/Shader.vert";
    private const string fragmentShaderPath = "Shaders/Shader.frag";

    public static int CreateShaderProgram()
    {
        string vertexShaderCode = File.ReadAllText(vertexShaderPath);
        string fragmentShaderCode = File.ReadAllText(fragmentShaderPath);
        
        int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderCode);
        int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderCode);

        int program = GL.CreateProgram();
        GL.AttachShader(program, vertexShader);
        GL.AttachShader(program, fragmentShader);
        GL.LinkProgram(program);

        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int linkStatus);
        if (linkStatus == (int)All.False)
            throw new Exception("Program linking failed: " + GL.GetProgramInfoLog(program));

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        return program;
    }

    private static int CompileShader(ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
        if (status == (int)All.False)
            throw new Exception($"{type} shader compilation failed: " + GL.GetShaderInfoLog(shader));

        return shader;
    }
}