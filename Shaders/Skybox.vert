#version 330 core
uniform mat4 uProjection;
uniform mat4 uView;

in vec3 aPosition;
out vec3 vTexCoord;

void main()
{
    // Remove translation from view matrix
    mat4 viewRotation = mat4(mat3(uView));
    vec4 clipPos = uProjection * viewRotation * vec4(aPosition, 1.0);

    // Use .xyww to ensure depth = 1.0 (far plane)
    gl_Position = clipPos.xyww;

    // Pass raw position as texture coordinates
    vTexCoord = aPosition;
}