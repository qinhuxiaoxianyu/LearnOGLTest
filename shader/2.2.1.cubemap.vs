#version 330 core
layout (location = 0) in vec3 aPos;

out vec3 WorldPos;

uniform mat4 projection;
uniform mat4 view;

void main()
{
    WorldPos = aPos;//环境立方体贴图就不用了左乘model矩阵变换到世界空间了
    gl_Position =  projection * view * vec4(WorldPos, 1.0);
}