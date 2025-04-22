#version 330 core
            out vec4 FragColor;

uniform vec4 color; // Combined color and alpha

void main()
{
    FragColor = color;
}