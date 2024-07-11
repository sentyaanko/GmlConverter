#include "common.hlsli"
#include "hsv.hlsli"

float imageWidth : register(c0) = 2048.0;
float imageHeight : register(c1) = 2048.0;
float pixelDistance : register(c2) = 50.0;

float lightSourceAzimuth : register(c3) = 315.0;
float lightSourceAltitude : register(c4) = 45.0;
float drawShade: register(c5) = 1.0;


float4 main(float2 uv : TEXCOORD) : COLOR
{
    float distanceBase = pixelDistance;
    float distanceHypotenuse = distanceBase * sqrt(2);
    float4 distance4 = float4(distanceHypotenuse, distanceBase, distanceHypotenuse, distanceBase);

    float oneX = 1.0 / imageWidth;
    float oneY = 1.0 / imageHeight;
    float4 offset01 = float4(-oneX, -oneY, 0.0, -oneY);
    float4 offset23 = float4(oneX, -oneY, -oneX, 0.0);

    float height4 = getHeight(input, uv);
    float4 height0123 = getHeight4(input, uv,  offset01,  offset23);
    float4 height8765 = getHeight4(input, uv, -offset01, -offset23);
    
    //float div = distanceBase * 8;
    //var dzdx = (h20 - h00 + (h21 - h01) * 2 + (h22 - h02)) / div;
    //var dzdy = (h02 - h00 + (h12 - h10) * 2 + (h22 - h20)) / div;
    //float dzdx = (height0123[2] - height0123[0] + (height8765[3] - height0123[3]) * 2 + (height8765[0] - height8765[2])) / div;
    //float dzdy = (height8765[2] - height0123[0] + (height8765[1] - height0123[1]) * 2 + (height8765[0] - height0123[2])) / div;
    //var dzdx = (h20 + h21 + h21 + h22 - h00 - h01 - h01 - h02) / div;
    //var dzdy = (h02 + h12 + h12 + h22 - h00 - h10 - h10 - h20) / div;
    float4 dzdy4 = height8765.zyxw - height0123.xyzw;
    float4 dzdx4 = float4(height8765.x - height8765.z, dzdy4.w, dzdy4.w, height0123.z - height0123.x);
    dzdy4.w = dzdy4.y;
    float div = 1 / (distanceBase * 8);
    //float dzdx = dot(dzdx4, div);
    //float dzdy = dot(dzdy4, div);
    float2 dzdxdzdy = float2(dot(dzdx4, div), dot(dzdy4, div));
    
    //float4 angle0123 = atan((height0123 - height4) / distance4);
    //float4 angle8765 = atan((height8765 - height4) / distance4);

    //float zenith = radians(90.0 - lightSourceAltitude);

    //// �V���p(0 �x���^��A 90 �x������)�̃R�T�C���l�̃L���b�V���B
    //float cosZenith = cos(zenith);

    //// �V���p(0 �x���^��A 90 �x������)�̃T�C���l�̃L���b�V���B
    //float sinZenith = sin(zenith);

    //// �����̕����B
    //// 0 �����Ŕ����v���B
    //float azimuth = radians(360.0 + 90.0 - lightSourceAzimuth);
    
    //float z_factor = 1.0; //�W���B�p�r�s���B

    ////�X�Ίp(radian)
    //float Slope = atan(z_factor * sqrt(dzdx * dzdx + dzdy * dzdy));
    //float cosSlope = cos(Slope);
    //float sinSlope = sin(Slope);

/*
		private static double CalculateAspect(double dzdx, double dzdy)
			=> dzdx != 0
				? NormalizeRad(Math.Atan2(dzdy, -dzdx))
				: dzdy == 0
					? 0
					: dzdy > 0 ? Math.PI / 2.0 : Math.PI + Math.PI / 2.0;
*/
	//�X�Ε���(radian)
    float dzdxIsNotZero = (dzdxdzdy.x != 0) ? 1 : 0;
    float dzdxIsZero = 1 - dzdxIsNotZero;
    float dzdyIsNotZero = (dzdxdzdy.y != 0) ? 1 : 0;
    float dzdyIsZero = 1 - dzdyIsNotZero;
    float aspect = 
        dzdxIsNotZero * atan2(dzdxdzdy.y, -dzdxdzdy.x) +
        dzdxIsZero * (
            dzdyIsZero * 0 + 
            dzdyIsNotZero * ((dzdxdzdy.y > 0) ? (PI / 2.0) : (PI + PI / 2.0))
        );
    
    //float s = (cosZenith * cosSlope + sinZenith * sinSlope * cos(azimuth - aspect)) * drawShade + 1 - drawShade;
    
    float z_factor = 1.0; //�W���B�p�r�s���B
    
    //�X�Ίp(radian)
    //float radSlope = atan(z_factor * sqrt(dzdx * dzdx + dzdy * dzdy));
    float radSlope = atan(z_factor * distance(dzdxdzdy, 0));

    // x: �����̕���(0 �����Ŕ����v���A�v�� xy ���ʂ� x ���� 0 �ɂ���B)(���w�P��(0 ���k�Ŏ��v���B315 �͖k��)����n���P�ʂɕϊ�)
    //  �ʑ����z��� 180 ����Ă���̂ŁA +180 ���Ă���B�����炭�Q�l�ɂ����T�C�g�̐������씼���ŁA���{���k�����Ȃ̂ŁA���̃Y���B
    // y: �V���p(0 �x���^��A 90 �x������)(���x(0 �x�������A 90 �x���^��): Altitude ����ϊ�)
    float2 radAzimuthZenith = radians(float2(180.0 + 90.0 - lightSourceAzimuth, 90.0 - lightSourceAltitude));

    // x: �V���p(0 �x���^��A 90 �x������)�̃R�T�C���l�B
    // y: �V���p(0 �x���^��A 90 �x������)�̃T�C���l�B
    // z: �X�Ίp�̃T�C���l�B
    // w: �X�Ίp�̃T�C���l�B
    float4 cossinZenithSlope = cos(float4(radAzimuthZenith.y, PI / 2.0 - radAzimuthZenith.y, radSlope, PI / 2.0 - radSlope));

    //�V���p�ƌX�Ίp�̈�v��̒���
    float s = dot(cossinZenithSlope.xy * cossinZenithSlope.zw, float2(1, cos(radAzimuthZenith.x - aspect))) * drawShade + 1 - drawShade;
    
    // S �͉A�e�N���Ɏg�p
    float3 heighHsvColor = float3(6.0 / 360.0, s, 160 / 256.0); // �ō��W���� HSV �F=(6.0, 255 / 255.0, 160 / 255.0);
    float3 lowHsvColor = float3(149.0 / 360.0, s, 192 / 256.0); // �Œ�W���� HSV �F=(149.0, 255 / 255.0, 192 / 255.0);

    //�ō��W�� 3780 + ���� 500 ���ő�l
    float ammount = (height4 - -500.0) / (3780.0 - -500.0);
    
    //�Œ�W�� -200 + ���� 500 ���ŏ��l
    //0 �͖����Ȓl
    
    float heightNotZero = 1 - step(height4, 0);
    float3 col = hsv2rgb(lerp(lowHsvColor, heighHsvColor, ammount)) * heightNotZero;
    //float3 col = lerp(lowHsvColor, heighHsvColor, ammount) * heightNotZero;
   
    
    //float ho = 500;
    //float h = heightNotZero * (height4 - ho);
    //float c = h / (255 * 256.0 * 0.1);
    return float4(col.r, col.g, col.b, heightNotZero);
}