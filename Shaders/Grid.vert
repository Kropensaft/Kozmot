#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColor;

out vec3 ourColor;

uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;

void main()
{
    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(aPos, 1.0);
    ourColor = aColor;
}