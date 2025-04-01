#version 330 core
out vec4 FragColor;

in vec3 TexCoords;

uniform samplerCube skyboxSampler; // Cubemap texture sampler

void main()
{
    FragColor = texture(skyboxSampler, TexCoords);
}