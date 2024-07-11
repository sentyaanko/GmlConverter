#include "common.hlsli"

//sampler2D inputDummy : register(s1);

float imageWidth : register(c0) = 2048.0;
float imageHeight : register(c1) = 2048.0;
float pixelDistance : register(c2) = 50.0;
float adjust : register(c3) = 1.0;
float exponent : register(c4) = 4.0;
float maskHeightMin : register(c5) = -200.0;
float maskHeightMax : register(c6) = 4000.0;
float positive : register(c7) = 1.0;
float angleMul : register(c8) = 1.0;
float angleMul0 : register(c9) = 1.0;
float angleMul1 : register(c10) = 1.0;
float angleMul2 : register(c11) = 1.0;
float angleMul3 : register(c12) = 1.0;
float angleBase : register(c13) = 0.0;


float4 main(float2 uv : TEXCOORD) : COLOR
{
    float distanceBase = pixelDistance;
    float distanceHypotenuse = distanceBase * sqrt(2);
    float4 distance4 = float4(distanceHypotenuse, distanceBase, distanceHypotenuse, distanceBase);

    float oneX = 1.0 / imageWidth;
    float oneY = 1.0 / imageHeight;
    float4 offset01 = float4(-oneX, -oneY, 0.0, -oneY);
    float4 offset23 = float4(oneX, -oneY, -oneX, 0.0);

    //uv = floor(uv * float2(imageWidth, imageHeight));
    //uv = uv * float2(imageWidth, imageHeight);
    //float oneX = 1.0;
    //float oneY = 1.0;
    //float4 offset01 = float4(-oneX, -oneY, 0.0, -oneY);
    //float4 offset23 = float4(oneX, -oneY, -oneX, 0.0);
    
    float height4 = getHeight(input, uv);
    float4 angle0123 = atan((getHeight4(input, uv,  offset01,  offset23) - height4) / distance4);
    float4 angle8765 = atan((getHeight4(input, uv, -offset01, -offset23) - height4) / distance4);
    
    // angle0123 �� angle8765 �̕ω������B
    // "-" �ł͂Ȃ��̂� angle8765 ����������Pixel�����ɂ����p�x�A�܂蕄�����t�̒l��ݒ肵�Ă���A��������Z���邱�Ƃō�������Ă���B
    //float4 deltaAngles = angle0123 + angle8765;
    float4 deltaAngles = angle0123 + angle8765 * angleMul + angleBase;
    float4 cuvatures = deltaAngles / distance4;

    //float cuvature = (cuvatures.x + cuvatures.y + cuvatures.z + cuvatures.w) * adjust * adjust / 4;
    // 16 �{���Ă�̂́A���̂��炢���Ȃ��ƒl���Ⴗ���邩��B
    //float cuvatureav = (cuvatures.x + cuvatures.y + cuvatures.z + cuvatures.w) / 4 * 16;
    // angle �� [-PI / 2, PI / 2]
    // cuvatures �� [-PI, PI]
    // PI �Ŋ��邱�Ƃ� cuvatureav �� [0.0, 1.0] �ɂ��Ă���
    // 50 ���|���邱�ƂŁA cuvatureav �� [0.0, 50.0] �ɂ��Ă���B�܂�� 3.6 �x�� 1.0 �ɒB����B
    //float cuvatureav = (cuvatures.x + cuvatures.y + cuvatures.z + cuvatures.w) / 4 / PI;
    //float cuvatureav = dot(cuvatures, float4(angleMul0, angleMul1, angleMul2, angleMul3)) / 4 / PI;
    //angleMul0-3 �� -1 or 1 or 0 �Ȃ̂ŁA���g�� dot ���邱�Ƃ� 1 or 0 �ƂȂ�A 0 �ȊO�̍��ڐ����v�Z�ł��A����𑊉����ώZ�o���ɗ��p����B
    float4 angles = float4(angleMul0, angleMul1, angleMul2, angleMul3);
    float anglesLen = dot(angles, angles);
    float cuvatureav = dot(cuvatures, angles) / anglesLen / PI;
    float cuvature = clamp(pow(abs(cuvatureav * adjust), 1 / exponent), 0.0, 1.0);
    float ho = 500;
    float isRange = step(maskHeightMin + ho, height4) * step(height4, maskHeightMax + ho);
    float cuvature2 = isRange * cuvature;
    //float isMinus = step(cuvature2, 0);
    //float outg = isMinus * -cuvature2;
    //float outb = cuvature2 - outg;
    //float heightNotZero = 1 - step(height4, 0);
    //return float4(0, outg, outb, heightNotZero);
    float o = cuvature2 * step(0, positive * cuvatureav);
    float heightNotZero = 1 - step(height4, 0);
    return float4(o, o, o, heightNotZero);

}