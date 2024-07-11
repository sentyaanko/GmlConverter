# NoDataToSlope

* [TilePng](TilePng.md) で出力された Png ファイルの輝度値が 0 の部分を 0 以外の部分から傾斜にして出力するモードです。
* 下の画像はこのモードに切り替えた直後の画面です。
	* ![](Images/NoDataToSlope/NoDataToSlope_00.png)

> [!NOTE]
> * この機能は離島など、海に面した地形を利用する際に使用することを想定しています。


## UI の機能

| 名称									| 概要																										|
|----									|----																										|
| Load									| 選択した Png ファイルを読み込みます。																		|
| Save									| 選択した Png ファイルに書き込みます。																		|
| Informations							| 現在読み込んでいるファイルの情報を表示します。															|
| Input FileName						| 現在読み込んでいるファイル名です。																		|
| Preview Scale							| プレビューの拡大率です。																					|
| DrawShade								| 陰影起伏の描画をするかです。プレビュー用の設定で、出力に影響しません。									|
| LightSourceAzimuth					| 陰影起伏の光源の方角（ 0 が北で時計回り）です。プレビュー用の設定で、出力に影響しません。					|
| LightSourceAltiude					| 陰影起伏の光源の高度（ 0 が水平で 90 が天頂）です。プレビュー用の設定で、出力に影響しません。				|
| DepthScale							| 傾斜の係数です。																							|
| DistanceScale							| 距離の係数です。																							|
| InitialDepth							| 初期の深さです。																							|
| Apply									| Slope の設定をプレビューに反映させます。																	|


* プレビューの操作
	* 右ドラッグ
		* 表示位置を移動します。
	* Ctrl + ホイール
		* 画像を拡大・縮小します。
	* Ctrl + 0
		* 拡大率を 100% に戻します。


## 操作手順

1. __Load ボタン__ で Png ファイルを指定します。
	* 読み込むファイルは [TilePng](TilePng.md) で出力される命名規則に従ったファイルのみで以下のような命名規則です。
		* __(.*)-(1|5|10).png__
			* $1 __(.*)__: 任意の文字列
			* $2 __(1|5|10)__: ピクセル間距離
	* ルールに沿っていないファイルを指定しても読み込みません。
	* 逆に、それさえ守っている 16bit Grayscale Png であれば、別のツールで作った Png でも読み込み可能です。
2. DepthScale / DistanceScale / InitialDepth の設定を調整します。
	* 下の画像は初期値での様子です。
		* ![](Images/NoDataToSlope/NoDataToSlope_01.png)
3. __Apply ボタン__ でプレビューを更新します。
	* 下の画像は値を変更したときの様子です。
		* ![](Images/NoDataToSlope/NoDataToSlope_02.png)
4. __Save ボタン__ で現在設定中に従い Png ファイルが保存されます。
	* ファイルの内容
		* フォーマットは 16bit Grayscale Png です。

このあとは [DividePng](DividePng.md) を使い、画像を定型サイズに切り分けることを想定しています。


> [!NOTE]
> * 湖沼エリアの対応
> 	* この機能を拡張する必要があります。（未対応）
> * 輝度値 0 と 非 0 までの距離
> 	* 算出には [OpenCV の distanceTransform](https://docs.opencv.org/4.10.0/d7/d1b/group__imgproc__misc.html#ga25c259e7e2fa2ac70de4606ea800f12f) を利用しています。

以上。

----
