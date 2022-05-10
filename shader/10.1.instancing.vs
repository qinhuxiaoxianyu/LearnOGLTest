
#version 330 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec3 aColor;
/*实例化数组*/
layout (location = 2) in vec2 aOffset;


out vec3 fColor;

/*实例化
uniform vec2 offsets[100];
*/

void main()
{
    /*实例化数组*/
    vec2 pos = aPos * (gl_InstanceID / 100.0);
    gl_Position = vec4(pos + aOffset, 0.0, 1.0);
    
    /*实例化
    vec2 offset = offsets[gl_InstanceID];
    gl_Position = vec4(aPos + offset, 0.0, 1.0);
    */
    fColor = aColor;
}