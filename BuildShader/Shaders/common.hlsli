#define PI 3.1415926535897932f

sampler2D input : register(s0);

float getHeight(sampler2D s, float2 t : TEXCOORD)
{
    float4 color = tex2D(s, t);
    float hs = 0.1;
    return (color.g * 256.0 + color.b) * 255.0 * hs;
}

float4 getHeight4(sampler2D s, float2 t : TEXCOORD, float4 offset01, float4 offset23)
{
    return float4(
        getHeight(s, t + offset01.xy),
        getHeight(s, t + offset01.zw),
        getHeight(s, t + offset23.xy),
        getHeight(s, t + offset23.zw)
    );
}
