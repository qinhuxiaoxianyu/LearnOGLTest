#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D texture1;

void main()
{             
    vec4 color = texture(texture1, TexCoords);
    if (color.r >= 0.2) color.a = 0.1;

    FragColor = color;
}