#version 330 core
layout (location = 0) in vec3 aPos;

out vec3 texCoords;

uniform mat4 projection;
uniform mat4 view;

void main()
{
    texCoords = aPos;
    vec4 pos = vec4(aPos, 1.0) * view * projection;
    gl_Position = vec4(pos.xy, pos.w, pos.w);
}  