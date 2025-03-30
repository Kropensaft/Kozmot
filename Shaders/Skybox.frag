#version 330 core
in vec3 vTexCoord;
out vec4 FragColor;
uniform samplerCube skybox;

void main()
{
    // Use normalized direction for sampling
    FragColor = texture(skybox, normalize(vTexCoord));
    // Replace texture sampling with:
    FragColor = vec4(1.0, 0.0, 0.0, 1.0); // Should make screen solid red
}