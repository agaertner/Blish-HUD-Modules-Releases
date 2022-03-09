#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_2_0
#define PS_SHADERMODEL ps_2_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float Intensity = 1.0f;
float Opacity = 1.0f;

sampler s0 = sampler_state {
    AddressU = Mirror;
    AddressV = Mirror;
};

float4 DrawGrayscale(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0 {
    float4 sam = tex2D(s0, TexCoords);
    float mean = 0.21 * sam.r + 0.72 * sam.g + 0.07 * sam.b;

    return float4(lerp(sam.rgb, float3(mean, mean, mean), Intensity), sam.a) * Opacity;
}

technique
{
    pass icon
    {
        PixelShader = compile PS_SHADERMODEL DrawGrayscale();
    }
}