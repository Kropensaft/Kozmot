using Silk.NET.OpenGL;
using System;

namespace C_Sharp_GL;

internal static class Shader
{
    private const string vertexShaderPath = "Program/../Shaders/Shader.vert";
    private const string fragmentShaderPath = "Program/../Shaders/Shader.frag";

    public static uint CreateShaderProgram(GL gl)
    {
        string VertexShaderCode = File.ReadAllText(vertexShaderPath);
        string FragmentShaderCode = File.ReadAllText(fragmentShaderPath);
        
        uint vertexShader = CompileShader(gl, ShaderType.VertexShader, VertexShaderCode);
        uint fragmentShader = CompileShader(gl, ShaderType.FragmentShader, FragmentShaderCode);

        uint program = gl.CreateProgram();
        gl.AttachShader(program, vertexShader);
        gl.AttachShader(program, fragmentShader);
        gl.LinkProgram(program);

        gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int linkStatus);
        if (linkStatus != (int)GLEnum.True)
            throw new Exception("Program linking failed: " + gl.GetProgramInfoLog(program));

        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);

        return program;
    }

    private static uint CompileShader(GL gl, ShaderType type, string source)
    {
        uint shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);

        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
        if (status != (int)GLEnum.True)
            throw new Exception($"{type} shader compilation failed: " + gl.GetShaderInfoLog(shader));

        return shader;
    }
}