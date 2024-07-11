#include "common.hlsli"


float4 main(float2 uv : TEXCOORD) : COLOR
{
    float height4 = getHeight(input, uv);
    float heightNotZero = 1 - step(height4, 0);
    float ho = 500;
    float h = heightNotZero * (height4 - ho);
    float c = h / (255 * 256.0 * 0.1);
    return float4(c, c, c, heightNotZero);
}