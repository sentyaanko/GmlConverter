/*

//1. original
float3 hsv2rgb(float h, float s, float v)
{
    float r = v;
    float g = v;
    float b = v;
    if (s > 0.0f) {
        h *= 6;
        final int i = (int) h;
        final float f = h - (float) i;
        switch (i) {
            default:
            case 0:
                g *= 1 - s * (1 - f);
                b *= 1 - s;
                break;
            case 1:
                r *= 1 - s * f;
                b *= 1 - s;
                break;
            case 2:
                r *= 1 - s;
                b *= 1 - s * (1 - f);
                break;
            case 3:
                r *= 1 - s;
                g *= 1 - s * f;
                break;
            case 4:
                r *= 1 - s * (1 - f);
                g *= 1 - s;
                break;
            case 5:
                g *= 1 - s;
                b *= 1 - s * f;
                break;
        }
    }
}

//2. rgb�̓��o����s�ɁAs �� if ���� 0 �Ȃ猋�ǉe���łȂ��Ȃ�̂ō폜
float3 hsv2rgb(float h, float s, float v)
{
    h *= 6;
    final int i = (int) h;
    final float f = h - (float) i;
    switch (i) {
    case 0:
        r = v;
        g = v * (1 - s * (1 - f));
        b = v * (1 - s);
        break;
    case 1:
        r = v * (1 - s * f);
        g = v;
        b = v * (1 - s);
        break;
    case 2:
        r = v * (1 - s);
        g = v;
        b = v * (1 - s * (1 - f));
        break;
    case 3:
        r = v * (1 - s);
        g = v * (1 - s * f);
        b = v;
        break;
    case 4:
        r = v * (1 - s * (1 - f));
        g = v * (1 - s);
        b = v;
        break;
    case 5:
        r = v;
        g = v * (1 - s);
        b = v * (1 - s * f);
        break;
    }
}

//3. float3 �֒u������
float3 hsv2rgb(float h, float s, float v)
{
    h *= 6;
    final int i = (int) h;
    final float f = h - (float) i;
    switch (i) {
    case 0: return float3(1,               1 - s * (1 - f), 1 - s) * v;
    case 1: return float3(1 - s * f,       1,               1 - s) * v;
    case 2: return float3(1 - s,           1,               1 - s * (1 - f)) * v;
    case 3: return float3(1 - s,           1 - s * f,       1) * v;
    case 4: return float3(1 - s * (1 - f), 1 - s,           1) * v;
    case 5: return float3(1,               1 - s,           1 - s * f) * v;
    }
}

//4. ��������u������
float3 hsv2rgb(float h, float s, float v)
{
    h *= 6;
    final int i = (int) h;
    final float f = h - (float) i;

    v0 = 1 - s * (1 - f);
    v1 = 1 - s * f;
    v2 = 1 - s;

    switch (i) {
    case 0: return float3(1,  v0, v2) * v;
    case 1: return float3(v1, 1,  v2) * v;
    case 2: return float3(v2, 1,  v0) * v;
    case 3: return float3(v2, v1, 1) * v;
    case 4: return float3(v0, v2, 1) * v;
    case 5: return float3(1,  v2, v1) * v;
    }
}

//5. frac �֒u������
float3 hsv2rgb(float h, float s, float v)
{
    h *= 6;
    final int i = (int) h;
    final float f = frac(h);

    v0 = 1 - s * (1 - f);
    v1 = 1 - s * f;
    v2 = 1 - s;

    switch (i) {
    case 0: return float3( 1, v0, v2) * v;
    case 1: return float3(v1,  1, v2) * v;
    case 2: return float3(v2,  1, v0) * v;
    case 3: return float3(v2, v1,  1) * v;
    case 4: return float3(v0, v2,  1) * v;
    case 5: return float3( 1, v2, v1) * v;
    }
}

//6. switch ���g��Ȃ�����
float3 hsv2rgb(float h, float s, float v)
{
    h *= 6;
    final float f = frac(h);

    v0 = 1 - s * (1 - f);
    v1 = 1 - s * f;
    v2 = 1 - s;

    if(h < 1)       return float3( 1, v0, v2) * v;
    else if(h < 2)  return float3(v1,  1, v2) * v;
    else if(h < 3)  return float3(v2,  1, v0) * v;
    else if(h < 4)  return float3(v2, v1,  1) * v;
    else if(h < 5)  return float3(v0, v2,  1) * v;
    else            return float3( 1, v2, v1) * v;
}

//7. rgb �Ŏ��𕪉�
float3 hsv2rgb(float h, float s, float v)
{
    h *= 6;
    final float f = frac(h);

    v0 = 1 - s * (1 - f);
    v1 = 1 - s * f;
    v2 = 1 - s;

    if(h < 1)       r = 1;
    else if(h < 2)  r =v1;
    else if(h < 3)  r =v2;
    else if(h < 4)  r =v2;
    else if(h < 5)  r =v0;
    else            r = 1;

    if(h < 1)       g = v0;
    else if(h < 2)  g =  1;
    else if(h < 3)  g =  1;
    else if(h < 4)  g = v1;
    else if(h < 5)  g = v2;
    else            g = v2;

    if(h < 1)       b = v2;
    else if(h < 2)  b = v2;
    else if(h < 3)  b = v0;
    else if(h < 4)  b =  1;
    else if(h < 5)  b =  1;
    else            b = v1;

    return float3(r, g, b) * v;
}

//8. gb �̈ʑ������炵�Ar�Ǝ������킹��
float3 hsv2rgb(float h, float s, float v)
{
    hx6 = h * 6;
    final float f = frac(hx6);

    v0 = 1 - s * (1 - f);
    v1 = 1 - s * f;
    v2 = 1 - s;

    hr = frac((hx6 + 0) / 6) * 6;
    if(hr < 1)      r = 1;
    else if(hr < 2) r =v1;
    else if(hr < 3) r =v2;
    else if(hr < 4) r =v2;
    else if(hr < 5) r =v0;
    else            r = 1;

    hg = frac((hx6 + 4) / 6) * 6;
    if(hg < 1)      g =  1;
    else if(hg < 2) g = v1;
    else if(hg < 3) g = v2;
    else if(hg < 4) g = v2;
    else if(hg < 5) g = v0;
    else            g =  1;

    hb = frac((hx6 + 2) / 6) * 6;
    if(hb < 1)      b =  1;
    else if(hb < 2) b = v1;
    else if(hb < 3) b = v2;
    else if(hb < 4) b = v2;
    else if(hb < 5) b = v0;
    else            b =  1;

    return float3(r, g, b) * v;
}

//9. ���ʕ������֐���
float func(float h, float s, float v, float phase)
{
    hx6 = h * 6;
    final float f = frac(hx6);

    v0 = 1 - s * (1 - f);
    v1 = 1 - s * f;
    v2 = 1 - s;

    hx6_ = frac((hx6 + phase) / 6) * 6;
    if(hx6_ < 1)        return = 1;
    else if(hx6_ < 2)   return =v1;
    else if(hx6_ < 3)   return =v2;
    else if(hx6_ < 4)   return =v2;
    else if(hx6_ < 5)   return =v0;
    else                return = 1;
}

float3 hsv2rgb(float h, float s, float v)
{
    return float3(func(h, s, v, 0), func(h, s, v, 4), func(h, s, v, 2)) * v;
}

//10. func ���V���v����(�ϐ������炷)
float func(float h, float s, float v, float phase)
{
    ////0. �I���W�i��
    //hx6 = h * 6;
    //hx6_ = frac((hx6 + phase) / 6) * 6;
    //final float f = frac(hx6);

    ////1. f �̌v�Z�̕ό`
    ////f �� h [0,1) �� 6 �{������̏������B
    ////phase �� 0, -2,-4 �̂����ꂩ�Ȃ̂ŁA frac(hx6) == frac(hx6 + phase)
    //hx6 = h * 6;
    //hx6_ = frac((hx6 + phase) / 6) * 6;
    //final float f = frac(hx6_);

    //2. hx6 �̏ȗ�
    hx6_ = frac(h + phase / 6) * 6;
    final float f = frac(hx6_);

    v0 = 1 - s * (1 - f);
    v1 = 1 - s * f;
    v2 = 1 - s * 1;
    v3 = 1 - s * 0;

    if(hx6_ < 1)        return =v3;
    else if(hx6_ < 2)   return =v1;
    else if(hx6_ < 3)   return =v2;
    else if(hx6_ < 4)   return =v2;
    else if(hx6_ < 5)   return =v0;
    else                return =v3;
}

float3 hsv2rgb(float h, float s, float v)
{
    return float3(func(h, s, v, 0), func(h, s, v, 4), func(h, s, v, 2)) * v;
}

//12. func ���V���v����(if ���� f �ˑ������Ŋ֐��𕪂���)
float func2(float hx6)
{
    final float f = frac(hx6);
    if(hx6 < 1)        return = 0;
    else if(hx6 < 2)   return = f;
    else if(hx6 < 3)   return = 1;
    else if(hx6 < 4)   return = 1;
    else if(hx6 < 5)   return = 1 - f;
    else               return = 0;
}

float func(float h, float s, float v, float phase)
{
    return 1 - s * fanc2(frac(h + phase / 6) * 6);
}

float3 hsv2rgb(float h, float s, float v)
{
    return float3(func(h, s, v, 0), func(h, s, v, 4), func(h, s, v, 2)) * v;
}

//13. func2 ���V���v���ɁA�̐���
float func2(float hx6)
{
    //final float f = frac(hx6);
    //if(hx6 < 1)        return = 0;      //hex6 [0,1) �� 0 �Œ�
    //else if(hx6 < 2)   return = f;      //hex6 [1,2) �� f �ɐ��̔��
    //else if(hx6 < 3)   return = 1;      //hex6 [2,3) �� 1 �Œ�
    //else if(hx6 < 4)   return = 1;      //hex6 [3,4) �� 1 �Œ�
    //else if(hx6 < 5)   return = 1 - f;  //hex6 [4,5) �� f �ɕ��̔��
    //else               return = 0;      //hex6 [5,6) �� 0 �Œ�
    //�܂�ȉ��̂悤�Ȋ���
    // 1|        ********
    //  |       *        *
    //  |      *          *
    //  |     *            *
    // 0|*****              *****
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6��
    //-1 �������ČX�����t�ɂ����
    //if(hx6 < 1)        return =  0;      //hex6 [0,1) ��  0 �Œ�
    //else if(hx6 < 2)   return = -f;      //hex6 [1,2) ��  f �ɕ��̔��
    //else if(hx6 < 3)   return = -1;      //hex6 [2,3) �� -1 �Œ�
    //else if(hx6 < 4)   return = -1;      //hex6 [3,4) �� -1 �Œ�
    //else if(hx6 < 5)   return = -1 + f;  //hex6 [4,5) ��  f �ɐ��̔��
    //else               return =  0;      //hex6 [5,6) ��  0 �Œ�
    // 0|*****              *****
    //  |     *            *
    //  |      *          *
    //  |       *        *
    //-1|        ********
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6��
    //���̌`�Ȃ畽�s�ړ��� clamp �� abs �Ŏ��ɂł���B
    // 1. �܂�[1,2) �̋�Ԃ̌X���ɍ��킹���ꎟ�֐��ɂ���
    // -hx6 + 3
    // 3|*
    //  | *
    //  |  *
    //  |   *
    // 2|    *
    //  |     *
    //  |      *
    //  |       *
    // 1|        *
    //  |         *
    //  |          *
    //  |           *
    // 0|            *
    //  |             *
    //  |              *
    //  |               *
    //-1|                *
    //  |                 *
    //  |                  *
    //  |                   *
    //-2|                    *
    //  |                     *
    //  |                      *
    //  |                       *
    //-3|                        *
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6��
    // 2. [4,5) �̋�Ԃ̌X���ɍ��킹�邽�߂� abs �֐��ł�����
    // abs(-hx6 + 3)
    // 3|*
    //  | *                     *
    //  |  *                   *
    //  |   *                 *
    // 2|    *               *
    //  |     *             *
    //  |      *           *
    //  |       *         *
    // 1|        *       *
    //  |         *     *
    //  |          *   *
    //  |           * *
    // 0|            *
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6��
    // 3. [0,1) [2,4) [5,6) �̋�Ԃ� [0,1] �̊O�ɂȂ�悤�ɕ��s�ړ�
    // abs(-hx6 + 3)-1
    // 2|*
    //  | *                     *
    //  |  *                   *
    //  |   *                 *
    // 1|    *               *
    //  |     *             *
    //  |      *           *
    //  |       *         *
    // 0|        *       *
    //  |         *     *
    //  |          *   *
    //  |           * *
    //-1|            *
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6��
    // 4. [0,1) [2,4) [5,6) �̋�Ԃ̌X���ɍ��킹�邽�߂� clamp �֐��ł�����
    // clamp(abs(-hx6 + 3)-1, 0, 1)
    // 1|*****               ****
    //  |     *             *
    //  |      *           *
    //  |       *         *
    // 0|        *********
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6��
    // 5. (1 - x) ����
    // 1-clamp(abs(-hx6 + 3)-1, 0, 1)
    // abs ���� * -1 ����i��Βl�Ȃ̂Ō��ʂ͕ς��Ȃ��B��Z�����炷�ړI�j
    // 1-clamp(abs(hx6 - 3)-1, 0, 1)
    // 1|        ********
    //  |       *        *
    //  |      *          *
    //  |     *            *
    // 0|*****              *****
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6��

    //�ȉ�������
    //hx6 - 3
    // 3|                        *
    //  |                       *
    //  |                      *
    //  |                     *
    // 2|                    *
    //  |                   *
    //  |                  *
    //  |                 *
    // 1|                *
    //  |               *
    //  |              *
    //  |             *
    // 0|            *
    //  |           *
    //  |          *
    //  |         *
    //-1|        *
    //  |       *
    //  |      *
    //  |     *
    //-2|    *
    //  |   *
    //  |  *
    //  | *
    //-3|*
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6��
    //abs(hx6 - 3)
    // 3|*                       *
    //  | *                     *
    //  |  *                   *
    //  |   *                 *
    // 2|    *               *
    //  |     *             *
    //  |      *           *
    //  |       *         *
    // 1|        *       *
    //  |         *     *
    //  |          *   *
    //  |           * *
    // 0|            *
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6��
    //abs(hx6 - 3) - 1
    // 2|*                       *
    //  | *                     *
    //  |  *                   *
    //  |   *                 *
    // 1|    *               *
    //  |     *             *
    //  |      *           *
    //  |       *         *
    // 0|        *       *
    //  |         *     *
    //  |          *   *
    //  |           * *
    //-1|            *
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6��
    //clamp(abs(hx6 - 3) - 1, 0, 1)
    // 1|*****               *****
    //  |     *             *
    //  |      *           *
    //  |       *         *
    // 0|        *********
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6��
    //1 - clamp(abs(hx6 - 3) - 1, 0, 1)
    // 1|        *********
    //  |       *         *
    //  |      *           *
    //  |     *             *
    // 0|*****               *****
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6��

    //�ŏI�I�ɂ͂ǂ�������������B
    return 1 - clamp(abs(hx6 - 3) - 1, 0, 1);
}

float func(float h, float s, float v, float phase)
{
    return 1 - s * fanc2(frac(h + phase / 6) * 6);
}

float3 hsv2rgb(float h, float s, float v)
{
    return float3(func(h, s, v, 0), func(h, s, v, 4), func(h, s, v, 2)) * v;
}

//14. ����W�J
float3 hsv2rgb(float h, float s, float v)
{
    //�W�J����ƈȉ��B
    //return func(h, s, float3(0, 4, 2)) * v;
    //return (1 - s * (1 - clamp(abs((frac(h + float3(0, 4, 2) / 6) * 6) - 3) - 1, 0, 1))) * v;
    return ((clamp(abs((frac(h + float3(0, 4, 2) / 6) * 6) - 3) - 1, 0, 1) -1) * s + 1) * v;
}

float3 hsv2rgb(float3 hsv)
{
    return ((clamp(abs((frac(hsv[0] + float3(0, 4, 2) / 6) * 6) - 3) - 1, 0, 1) -1) * hsv[1] + 1) * hsv[2];
}

*/


float3 hsv2rgb(float3 hsv)
{
    //return ((clamp(abs(frac(hsv[0] + float3(0, 2, 1) / 3) * 6 - 3) - 1, 0, 1) - 1) * hsv[1] + 1) * hsv[2];
    return ((clamp(abs(frac(hsv[0] + float3(0, 4, 2) / 6) * 6 - 3) - 1, 0, 1) - 1) * hsv[1] + 1) * hsv[2];
}
