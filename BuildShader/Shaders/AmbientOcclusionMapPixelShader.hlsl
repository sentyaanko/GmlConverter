#include "common.hlsli"

float imageWidth : register(c1) = 2048.0;
float imageHeight : register(c2) = 2048.0;
float pixelDistance : register(c3) = 5.0;
float adjust : register(c4) = 8.0;
float exponent : register(c5) = 1.0;


float4 main(float2 uv : TEXCOORD) : COLOR
{
    float distanceBase = pixelDistance;
    float distanceHypotenuse = distanceBase * sqrt(2);
    float4 distance4 = float4(distanceHypotenuse, distanceBase, distanceHypotenuse, distanceBase);
    
    float oneX = 1.0 / imageWidth;
    float oneY = 1.0 / imageHeight;
    float4 offset01 = float4(-oneX, -oneY,  0.0, -oneY);
    float4 offset23 = float4( oneX, -oneY, -oneX, 0.0);

    
    float height4 = getHeight(input, uv);
    // atan [-PI/2, PI/2]
    float4 angle0123 = atan((getHeight4(input, uv,  offset01,  offset23) - height4) / distance4);
    float4 angle8765 = atan((getHeight4(input, uv, -offset01, -offset23) - height4) / distance4);
    
    // *2/PI することで[-1,1]にする。
    float c = 2.0 / PI;
    //clamp することで負の角の場合 0 にする。
    // 1 - n とすることで、余角を求める。
    float4 ao0123 = 1.0 - clamp(angle0123 * c, 0, 1);
    float4 ao8765 = 1.0 - clamp(angle8765 * c, 0, 1);
    //float4 ao0123 = 1.0 - min(1.0, max(0.0, atan((getHeight4(input, uv,  offset01,  offset23) - height4) / distance4) * c));
    //float4 ao8765 = 1.0 - min(1.0, max(0.0, atan((getHeight4(input, uv, -offset01, -offset23) - height4) / distance4) * c));
    //float4 ao0123 = 1.0 - min(1.0, max(0.0, atan((getHeight4(input, uv, offset01, offset23) - height4) / distance4)) * c);
    //float4 ao8765 = 1.0 - min(1.0, max(0.0, atan((getHeight4(input, uv, -offset01, -offset23) - height4) / distance4)) * c);
    // [0,1] の相加平均なので、結果も [0,1]
    float aoav = (ao0123.x + ao0123.y + ao0123.z + ao0123.w + ao8765.x + ao8765.y + ao8765.z + ao8765.w) / 8.0;
    // * adjust することで値の底上げをする。
    float ao = clamp(pow(aoav * adjust, 1 / exponent), 0, 1);

    float heightNotZero = 1 - step(height4, 0);
    float ao2 = ao * heightNotZero;
    return float4(ao2, ao2, ao2, heightNotZero);
}