#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D screenTexture;

void main()
{
    //FragColor = vec4(texture(screenTexture, TexCoords).rgb, 1.0);//普通效果
    //FragColor = vec4(vec3(1.0 - texture(screenTexture, TexCoords)), 1.0);//颜色反向
    /*灰度
    FragColor = texture(screenTexture, TexCoords);
    //float average = (FragColor.r + FragColor.g + FragColor.b) / 3.0;//平均灰度
    float average = 0.2126 * FragColor.r + 0.7152 * FragColor.g + 0.0722 * FragColor.b;//加权灰度
    FragColor = vec4(average, average, average, 1.0);
    */
    /*核效果，模糊，边缘检测*/
    const float offset = 1.0 / 300.0;   //偏移量常量
    vec2 offsets[9] = vec2[](           //偏移量数组
        vec2(-offset,  offset), // 左上
        vec2( 0.0f,    offset), // 正上
        vec2( offset,  offset), // 右上
        vec2(-offset,  0.0f),   // 左
        vec2( 0.0f,    0.0f),   // 中
        vec2( offset,  0.0f),   // 右
        vec2(-offset, -offset), // 左下
        vec2( 0.0f,   -offset), // 正下
        vec2( offset, -offset)  // 右下
    );
    /*
    float kernel[9] = float[](          //自定义核，核
        -1, -1, -1,
        -1,  9, -1,
        -1, -1, -1
    );*/
    /*
    float kernel[9] = float[](          //自定义核，模糊
        1.0 / 16, 2.0 / 16, 1.0 / 16,
        2.0 / 16, 4.0 / 16, 2.0 / 16,
        1.0 / 16, 2.0 / 16, 1.0 / 16  
    );
    */
    float kernel[9] = float[](          //自定义核，边缘检测
        1, 1, 1,
        1, -8, 1,
        1, 1, 1
    );
    vec3 sampleTex[9];
    for(int i = 0; i < 9; i++)
    {
        sampleTex[i] = vec3(texture(screenTexture, TexCoords.st + offsets[i]));//将偏移量加到纹理坐标上
    }
    vec3 col = vec3(0.0);
    for(int i = 0; i < 9; i++)
        col += sampleTex[i] * kernel[i];                                        //加权到一起
    FragColor = vec4(col, 1.0);
    
    /*模糊*/
} 