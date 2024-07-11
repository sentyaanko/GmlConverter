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

//2. rgbの導出を一行に、s の if 文は 0 なら結局影響でなくなるので削除
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

//3. float3 へ置き換え
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

//4. 長い式を置き換え
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

//5. frac へ置き換え
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

//6. switch を使わなくする
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

//7. rgb で式を分解
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

//8. gb の位相をずらし、rと式を合わせる
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

//9. 共通部分を関数に
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

//10. func をシンプルに(変数を減らす)
float func(float h, float s, float v, float phase)
{
    ////0. オリジナル
    //hx6 = h * 6;
    //hx6_ = frac((hx6 + phase) / 6) * 6;
    //final float f = frac(hx6);

    ////1. f の計算の変形
    ////f は h [0,1) を 6 倍した後の小数部。
    ////phase は 0, -2,-4 のいずれかなので、 frac(hx6) == frac(hx6 + phase)
    //hx6 = h * 6;
    //hx6_ = frac((hx6 + phase) / 6) * 6;
    //final float f = frac(hx6_);

    //2. hx6 の省略
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

//12. func をシンプルに(if 文を f 依存部分で関数を分ける)
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

//13. func2 をシンプルに、の説明
float func2(float hx6)
{
    //final float f = frac(hx6);
    //if(hx6 < 1)        return = 0;      //hex6 [0,1) は 0 固定
    //else if(hx6 < 2)   return = f;      //hex6 [1,2) は f に正の比例
    //else if(hx6 < 3)   return = 1;      //hex6 [2,3) は 1 固定
    //else if(hx6 < 4)   return = 1;      //hex6 [3,4) は 1 固定
    //else if(hx6 < 5)   return = 1 - f;  //hex6 [4,5) は f に負の比例
    //else               return = 0;      //hex6 [5,6) は 0 固定
    //つまり以下のような感じ
    // 1|        ********
    //  |       *        *
    //  |      *          *
    //  |     *            *
    // 0|*****              *****
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6→
    //-1 をかけて傾きを逆にすると
    //if(hx6 < 1)        return =  0;      //hex6 [0,1) は  0 固定
    //else if(hx6 < 2)   return = -f;      //hex6 [1,2) は  f に負の比例
    //else if(hx6 < 3)   return = -1;      //hex6 [2,3) は -1 固定
    //else if(hx6 < 4)   return = -1;      //hex6 [3,4) は -1 固定
    //else if(hx6 < 5)   return = -1 + f;  //hex6 [4,5) は  f に正の比例
    //else               return =  0;      //hex6 [5,6) は  0 固定
    // 0|*****              *****
    //  |     *            *
    //  |      *          *
    //  |       *        *
    //-1|        ********
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6→
    //この形なら平行移動と clamp と abs で式にできる。
    // 1. まず[1,2) の区間の傾きに合わせた一次関数にする
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
    //   0   1   2   3   4   5   6 hx6→
    // 2. [4,5) の区間の傾きに合わせるために abs 関数でくくる
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
    //   0   1   2   3   4   5   6 hx6→
    // 3. [0,1) [2,4) [5,6) の区間で [0,1] の外になるように平行移動
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
    //   0   1   2   3   4   5   6 hx6→
    // 4. [0,1) [2,4) [5,6) の区間の傾きに合わせるために clamp 関数でくくる
    // clamp(abs(-hx6 + 3)-1, 0, 1)
    // 1|*****               ****
    //  |     *             *
    //  |      *           *
    //  |       *         *
    // 0|        *********
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6→
    // 5. (1 - x) する
    // 1-clamp(abs(-hx6 + 3)-1, 0, 1)
    // abs 内を * -1 する（絶対値なので結果は変わらない。乗算を減らす目的）
    // 1-clamp(abs(hx6 - 3)-1, 0, 1)
    // 1|        ********
    //  |       *        *
    //  |      *          *
    //  |     *            *
    // 0|*****              *****
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6→

    //以下も同等
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
    //   0   1   2   3   4   5   6 hx6→
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
    //   0   1   2   3   4   5   6 hx6→
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
    //   0   1   2   3   4   5   6 hx6→
    //clamp(abs(hx6 - 3) - 1, 0, 1)
    // 1|*****               *****
    //  |     *             *
    //  |      *           *
    //  |       *         *
    // 0|        *********
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6→
    //1 - clamp(abs(hx6 - 3) - 1, 0, 1)
    // 1|        *********
    //  |       *         *
    //  |      *           *
    //  |     *             *
    // 0|*****               *****
    //   -------------------------
    //   0   1   2   3   4   5   6 hx6→

    //最終的にはどちらも同じ数式。
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

//14. 式を展開
float3 hsv2rgb(float h, float s, float v)
{
    //展開すると以下。
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
