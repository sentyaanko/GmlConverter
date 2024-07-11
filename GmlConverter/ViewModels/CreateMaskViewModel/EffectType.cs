/*
# Effect を使用してマスク画像を描画する方法に関する調査結果

いまのところの、表示がマシな組み合わせ

* Image.RenderOptions.BitmapScalingMode="NearestNeighbor" を指定する
	* これ以外を指定すると、イメージの拡大縮小時に下位バイトの 255 - 0 の切り替わる場所を検出してしまう。
	* NearestNeighbor ではそれは発生しない。（が、格子状にラインが入ってしまうが、幾分マシ）
* RegisterPixelShaderSamplerProperty() の第四引数で SamplingMode.Bilinear を指定する。
	* これ以外を指定すると、イメージの拡大縮小時に正常に検出ができなくなる。
* 16bit grayscale 画像から自前で チャンネル分解してシェーダー内で統合する。
	* 詳しくは DevidePngViewModel.cs のコメント参照。
 
 */

namespace GmlConverter.ViewModels
{
    public enum EffectType
    {
        Height,
        HillShade,
        AmbientOcclusion,
        Curvature,
    }
}
