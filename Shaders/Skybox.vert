#version 330 core
layout (location = 0) in vec3 aPos;

out vec3 TexCoords;

uniform mat4 projection;
uniform mat4 view; // View matrix WITHOUT translation

void main()
{
    TexCoords = aPos;
    // Remove translation from view matrix before applying projection
    // Pass position to fragment shader, ensuring z = w for max depth
    vec4 pos = projection * view * vec4(aPos, 1.0);
    gl_Position = pos.xyww;
}