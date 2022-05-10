#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 2) in vec2 aTexCoords;
/*实例化数组*/
layout (location = 3) in mat4 aInstanceMatrix;

out vec2 TexCoords;

/*普通用法
uniform mat4 model;
*/
uniform mat4 projection;
uniform mat4 view;

void main()
{
    TexCoords = aTexCoords;
    /*实例化数组*/
    gl_Position = projection * view * aInstanceMatrix * vec4(aPos, 1.0f);
    
    /*普通用法
    gl_Position = projection * view * model * vec4(aPos, 1.0f);
    */
}