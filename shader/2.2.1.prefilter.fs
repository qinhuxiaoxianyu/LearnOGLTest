/*
float DistributionGGX
float RadicalInverse_VdC(uint bits)
vec2 Hammersley(uint i, uint N)
vec3 ImportanceSampleGGX(vec2 Xi, vec3 N, float roughness)
*/
#version 330 core
out vec4 FragColor;
in vec3 WorldPos;

uniform samplerCube environmentMap;
uniform float roughness;

const float PI = 3.14159265359;
// ----------------------------------------------------------------------------
float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a = roughness*roughness;
    float a2 = a*a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float nom   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}
/**/
float RadicalInverse(uint Base, uint i)
{
	float Radical = 1.0 / float(Base);
	float Digit = 1.0 / float(Base);
	float Inverse = 0.0;
	for(i; i > 0u; i /= Base)
	{
		Inverse += Digit * (float(i % Base));
		Digit *= Radical;

		// i /= Base;
	}
	return Inverse;
}

vec2 Halton(uint i)
{
	return vec2(RadicalInverse(2u, i), RadicalInverse(3u, i));
}

vec2 Hammersley(uint i, uint N)
{
	return vec2(float(i)/float(N), RadicalInverse(2u, i));//Hammersley 序列是基于 Van Der Corput 序列
}

// ----------------------------------------------------------------------------
// http://holger.dammertz.org/stuff/notes_HammersleyOnHemisphere.html
// efficient VanDerCorpus calculation.
/*
float RadicalInverse_VdC(uint bits) //Van Der Corput 序列
{
     bits = (bits << 16u) | (bits >> 16u);
     bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
     bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
     bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
     bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
     return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}
// ----------------------------------------------------------------------------
vec2 Hammersley(uint i, uint N)
{
	return vec2(float(i)/float(N), RadicalInverse_VdC(i));//Hammersley 序列是基于 Van Der Corput 序列
}
*/
// ----------------------------------------------------------------------------
vec3 ImportanceSampleGGX(vec2 Xi, vec3 N, float roughness)//这个有时间也要再看看？
{
	float a = roughness*roughness;
	
	float phi = 2.0 * PI * Xi.x;
	float cosTheta = sqrt((1.0 - Xi.y) / (1.0 + (a*a - 1.0) * Xi.y));
	float sinTheta = sqrt(1.0 - cosTheta*cosTheta);
    //辐照度图里也有这个cosTheta和sinTheta不知道怎么生成的
	
	// from spherical coordinates to cartesian coordinates - halfway vector
	vec3 H;
	H.x = cos(phi) * sinTheta;
	H.y = sin(phi) * sinTheta;
	H.z = cosTheta;//辐照度图里也有这个东西，其实不知道是怎么生成的
    //这里得到的H还是切线空间的（生成H还没有用到N向量）
	
	// from tangent-space H vector to world-space sample vector
	vec3 up          = abs(N.z) < 0.999 ? vec3(0.0, 0.0, 1.0) : vec3(1.0, 0.0, 0.0);
	vec3 tangent   = normalize(cross(up, N));
	vec3 bitangent = cross(N, tangent);
    //切线空间，就是让tangent作为x轴，bitangent作为y轴，N作为z轴的坐标空间
	
	vec3 sampleVec = tangent * H.x + bitangent * H.y + N * H.z;
	return normalize(sampleVec);
}
// ----------------------------------------------------------------------------
void main()
{		
    vec3 N = normalize(WorldPos);
    
    // make the simplyfying assumption that V equals R equals the normal 
    vec3 R = N;
    vec3 V = R;

    const uint SAMPLE_COUNT = 32u;//32 64 128 256 512 1024 2048
    vec3 prefilteredColor = vec3(0.0);
    float totalWeight = 0.0001;
    
    for(uint i = 0u; i < SAMPLE_COUNT; ++i)
    {
        // generates a sample vector that's biased towards the preferred alignment direction (importance sampling).
        // vec2 Xi = Hammersley(i, SAMPLE_COUNT);//低差异序列，Hammersley 序列
        vec2 Xi = Halton(i);
        vec3 H = ImportanceSampleGGX(Xi, N, roughness);//根据低差异序列，采样方向和粗糙度生成采样向量，但实际并不用这个向量取样
        vec3 L  = normalize(2.0 * dot(V, H) * H - V);//这里不是很懂在干嘛？
        //通过画图，大概可以理解到L是用于计算镜面波瓣的边界？
        //但是下面采样用L就不理解了？？难道L才是实际采样向量

        float NdotL = max(dot(N, L), 0.0);
        //超过这个边界的就不采样，通过画图看到就是N和H不能超过45°
        if(NdotL > 0.0)
        {
            // sample from the environment's mip level based on roughness/pdf
            //解决预过滤卷积的亮点
            //可以在预过滤卷积时，不直接采样环境贴图，而是基于积分的 PDF 和粗糙度采样环境贴图的 mipmap ，以减少伪像
            float D   = DistributionGGX(N, H, roughness);
            float NdotH = max(dot(N, H), 0.0);
            float HdotV = max(dot(H, V), 0.0);
            float pdf = D * NdotH / (4.0 * HdotV) + 0.0001; 

            float resolution = 512.0; // resolution of source cubemap (per face)，源立方体贴图的分辨率
            float saTexel  = 4.0 * PI / (6.0 * resolution * resolution);
            float saSample = 1.0 / (float(SAMPLE_COUNT) * pdf + 0.0001);

            float mipLevel = roughness == 0.0 ? 0.0 : 0.5 * log2(saSample / saTexel); 
            
            prefilteredColor += textureLod(environmentMap, L, mipLevel).rgb * NdotL;//在不考虑预过滤卷积的亮点的情况下，只有这两步
            //prefilteredColor += textureLod(environmentMap, L, 0.0).rgb * NdotL;
            //textureLod(sampler, P, Lod);sampler采样器，P采样向量，Lod level of detail
            totalWeight      += NdotL;
        }
    }

    prefilteredColor = prefilteredColor / totalWeight;

    FragColor = vec4(prefilteredColor, 1.0);
    //FragColor = vec4(1.0);
}