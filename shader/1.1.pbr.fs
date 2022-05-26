#version 330 core

out vec4 FragColor;

in vec2 TexCoords;
in vec3 FragPos;//
in vec3 Normal;

// material parameters
uniform sampler2D albedoMap;//反照率贴图，计算菲涅尔F0有使用
uniform sampler2D normalMap;//法线贴图
uniform sampler2D metallicMap;//金属度贴图
uniform sampler2D roughnessMap;//粗糙度贴图
uniform sampler2D aoMap;//环境光遮蔽贴图

// lights
uniform vec3 lightPositions[4];
uniform vec3 lightColors[4];

uniform vec3 camPos;

const float PI = 3.14159265359;
// ----------------------------------------------------------------------------
// Easy trick to get tangent-normals to world-space to keep PBR code simplified.
// Don't worry if you don't get what's going on; you generally want to do normal 
// mapping the usual way for performance anways; I do plan make a note of this 
// technique somewhere later in the normal mapping tutorial.
vec3 getNormalFromMap()//切线空间再回顾下
{
    vec3 tangentNormal = texture(normalMap, TexCoords).xyz * 2.0 - 1.0;

    vec3 Q1  = dFdx(FragPos);
    vec3 Q2  = dFdy(FragPos);
    vec2 st1 = dFdx(TexCoords);
    vec2 st2 = dFdy(TexCoords);

    vec3 N   = normalize(Normal);
    vec3 T  = normalize(Q1*st2.t - Q2*st1.t);
    vec3 B  = -normalize(cross(N, T));
    mat3 TBN = mat3(T, B, N);

    return normalize(TBN * tangentNormal);
}
// ----------------------------------------------------------------------------
float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a = roughness * roughness;//光照在正太分布函数中采用粗糙度的平方会让光照看起来更加自然。
    float NdotH = max(dot(N, H), 0.0);

    float nom   = a * a;
    float denom = (NdotH * NdotH * (a * a - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySchlickGGX(float cosTheta, float roughness)
{
    float a = roughness*roughness;//光照在几何遮蔽函数中采用粗糙度的平方会让光照看起来更加自然。
    float k = (a + 1.0) * (a + 1.0) / 8.0;

    float nom   = cosTheta;
    float denom = cosTheta * (1.0 - k) + k;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);//几何遮蔽
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);//几何阴影

    return ggx1 * ggx2;
}
// ----------------------------------------------------------------------------
vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}
// ----------------------------------------------------------------------------
void main()
{		
    vec3  albedo    = pow(texture(albedoMap, TexCoords).rgb, vec3(2.2));//gamma矫正相关的再回顾一下
    float metallic  = texture(metallicMap, TexCoords).r;
    float roughness = texture(roughnessMap, TexCoords).r;
    float ao        = texture(aoMap, TexCoords).r;

    vec3 N = getNormalFromMap();
    vec3 V = normalize(camPos - FragPos);//视角方向向量

    // calculate reflectance at normal incidence; if dia-electric (like plastic) use F0 
    // of 0.04 and if it's a metal, use the albedo color as F0 (metallic workflow)    
    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, albedo, metallic);//return F0 * (1 - metallic) + albedo * metallic

    // reflectance equation
    vec3 Lo = vec3(0.0);
    for(int i = 0; i < 4; ++i) 
    {
        // calculate per-light radiance
        vec3 wi = lightPositions[i] - FragPos;
        float distance = length(wi);
        float attenuation = 1.0 / (distance * distance);
        vec3 radiance = lightColors[i] * attenuation;//此处还没有计算cosTheta
        //以上就是反射率方程中Li(p, wi)的部分，还没有计算n * wi

        vec3 L = normalize(wi);//光线方向向量
        vec3 H = normalize(V + L);//半程向量
        float HdotV = max(dot(H, V), 0.0);
        // Cook-Torrance BRDF
        float NDF = DistributionGGX(N, H, roughness);   
        float G   = GeometrySmith(N, V, L, roughness);      
        vec3  F   = fresnelSchlick(HdotV, F0);
        //以上是反射率方程中DFG的部分，目前NDF中分母的NdotH还有点问题

        vec3 numerator    = NDF * G * F; 
        float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001; // + 0.0001 to prevent divide by zero
        vec3 specular = numerator / denominator;
        //以上是反射率方程中 DFG / (4 * (wo * n) * (wi * n))的部分，F就是ks
        
        // kS is equal to Fresnel
        vec3 kS = F;
        // for energy conservation, the diffuse and specular light can't
        // be above 1.0 (unless the surface emits light); to preserve this
        // relationship the diffuse component (kD) should equal 1.0 - kS.
        vec3 kD = vec3(1.0) - kS;
        // multiply kD by the inverse metalness such that only non-metals 
        // have diffuse lighting, or a linear blend if partly metal (pure metals
        // have no diffuse light).
        kD *= 1.0 - metallic;
        //以上是反射率方程中kd * c / PI部分要用到的东西

        // scale light by NdotL
        float NdotL = max(dot(N, L), 0.0);
        //以上是反射率方程n * wi部分

        // add to outgoing radiance Lo
        Lo += (kD * albedo / PI + specular) * radiance * NdotL;  // note that we already multiplied the BRDF by the Fresnel (kS) so we won't multiply by kS again
    }   
    
    // ambient lighting (note that the next IBL tutorial will replace 
    // this ambient lighting with environment lighting).
    vec3 ambient = vec3(0.03) * albedo * ao;
    
    vec3 color = ambient + Lo;

    // HDR tonemapping
    color = color / (color + vec3(1.0));
    // gamma correct
    color = pow(color, vec3(1.0/2.2)); 

    FragColor = vec4(color, 1.0);
}