#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D texture1;

/*深度缓冲可视化，线性*/
float near = 0.1;
float far = 100.0;
float LinearizeDepth(float depth);

void main()
{    
    //FragColor = texture(texture1, TexCoords);
    /*深度缓冲可视化*/
    /*非线性
    FragColor = vec4(vec3(gl_FragCoord.z), 1.0);//gl_FragCoord内置变量
    */
    /*线性，因为z值在经过投影矩阵后就非线性了，使其线性需要逆操作*/
    float depth = LinearizeDepth(gl_FragCoord.z) / far; // 为了演示除以 far
    FragColor = vec4(vec3(depth), 1.0);
}

/*深度缓冲可视化，线性*/
float LinearizeDepth(float depth){
    float z = depth * 2.0 - 1.0; // back to NDC 
    return (2.0 * near * far) / (far + near - z * (far - near));  
}