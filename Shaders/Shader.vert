#version 330 core

layout (location = 0) in vec3 aPos;

out vec3 fragColor;


uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;


uniform vec3 object_color;

void main()
{
    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(aPos, 1.0);
    fragColor = object_color;
}
