using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace GmlConverter.Effects
{
	internal class CurvatureShaderEffect : ShaderEffect
	{
		public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(
			"Input", typeof(CurvatureShaderEffect), 0, SamplingMode.Bilinear);

		//public static readonly DependencyProperty InputSecondProperty = RegisterPixelShaderSamplerProperty(
		//	"InputSecond", typeof(CurvatureShaderEffect), 1, SamplingMode.Bilinear);

		public static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register(
			"ImageWidth", typeof(double), typeof(CurvatureShaderEffect),
				new UIPropertyMetadata(2048.0, PixelShaderConstantCallback(0)));
		public static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register(
			"ImageHeight", typeof(double), typeof(CurvatureShaderEffect),
				new UIPropertyMetadata(2048.0, PixelShaderConstantCallback(1)));
		public static readonly DependencyProperty DistanceProperty = DependencyProperty.Register(
			"Distance", typeof(double), typeof(CurvatureShaderEffect),
				new UIPropertyMetadata(5.0, PixelShaderConstantCallback(2)));
		public static readonly DependencyProperty AdjustProperty = DependencyProperty.Register(
			"Adjust", typeof(double), typeof(CurvatureShaderEffect),
				new UIPropertyMetadata(50.0, PixelShaderConstantCallback(3)));
		public static readonly DependencyProperty ExponentProperty = DependencyProperty.Register(
			"Exponent", typeof(double), typeof(CurvatureShaderEffect),
				new UIPropertyMetadata(1.0, PixelShaderConstantCallback(4)));
		public static readonly DependencyProperty MaskHeightMinProperty = DependencyProperty.Register(
			"MaskHeightMin", typeof(double), typeof(CurvatureShaderEffect),
				new UIPropertyMetadata(-200.0, PixelShaderConstantCallback(5)));
		public static readonly DependencyProperty MaskHeightMaxProperty = DependencyProperty.Register(
			"MaskHeightMax", typeof(double), typeof(CurvatureShaderEffect),
				new UIPropertyMetadata(4000.0, PixelShaderConstantCallback(6)));
		public static readonly DependencyProperty PositiveProperty = DependencyProperty.Register(
			"Positive", typeof(double), typeof(CurvatureShaderEffect),
				new UIPropertyMetadata(4.0, PixelShaderConstantCallback(7)));
		public static readonly DependencyProperty AngleMulProperty = DependencyProperty.Register(
			"AngleMul", typeof(double), typeof(CurvatureShaderEffect),
				new UIPropertyMetadata(1.0, PixelShaderConstantCallback(8)));
		public static readonly DependencyProperty AngleMul0Property = DependencyProperty.Register(
			"AngleMul0", typeof(double), typeof(CurvatureShaderEffect),
				new UIPropertyMetadata(1.0, PixelShaderConstantCallback(9)));
		public static readonly DependencyProperty AngleMul1Property = DependencyProperty.Register(
			"AngleMul1", typeof(double), typeof(CurvatureShaderEffect),
				new UIPropertyMetadata(1.0, PixelShaderConstantCallback(10)));
		public static readonly DependencyProperty AngleMul2Property = DependencyProperty.Register(
			"AngleMul2", typeof(double), typeof(CurvatureShaderEffect),
				new UIPropertyMetadata(1.0, PixelShaderConstantCallback(11)));
		public static readonly DependencyProperty AngleMul3Property = DependencyProperty.Register(
			"AngleMul3", typeof(double), typeof(CurvatureShaderEffect),
				new UIPropertyMetadata(1.0, PixelShaderConstantCallback(12)));
		public static readonly DependencyProperty AngleBaseProperty = DependencyProperty.Register(
			"AngleBase", typeof(double), typeof(CurvatureShaderEffect),
				new UIPropertyMetadata(0.0, PixelShaderConstantCallback(13)));

		public Brush Input
		{
			get { return (Brush)GetValue(InputProperty); }
			set { SetValue(InputProperty, value); }
		}
		//public Brush InputSecond
		//{
		//	get { return (Brush)GetValue(InputSecondProperty); }
		//	set { SetValue(InputSecondProperty, value); }
		//}

		public double ImageWidth
		{
			get { return (double)GetValue(ImageWidthProperty); }
			set { SetValue(ImageWidthProperty, value); }
		}
		public double ImageHeight
		{
			get { return (double)GetValue(ImageHeightProperty); }
			set { SetValue(ImageHeightProperty, value); }
		}
		public double Distance
		{
			get { return (double)GetValue(DistanceProperty); }
			set { SetValue(DistanceProperty, value); }
		}
		public double Adjust
		{
			get { return (double)GetValue(AdjustProperty); }
			set { SetValue(AdjustProperty, value); }
		}
		public double Exponent
		{
			get { return (double)GetValue(ExponentProperty); }
			set { SetValue(ExponentProperty, value); }
		}
		public double MaskHeightMin
		{
			get { return (double)GetValue(MaskHeightMinProperty); }
			set { SetValue(MaskHeightMinProperty, value); }
		}
		public double MaskHeightMax
		{
			get { return (double)GetValue(MaskHeightMaxProperty); }
			set { SetValue(MaskHeightMaxProperty, value); }
		}
		public double Positive
		{
			get { return (double)GetValue(PositiveProperty); }
			set { SetValue(PositiveProperty, value); }
		}
		public double AngleMul
		{
			get { return (double)GetValue(AngleMulProperty); }
			set { SetValue(AngleMulProperty, value); }
		}
		public double AngleMul0
		{
			get { return (double)GetValue(AngleMul0Property); }
			set { SetValue(AngleMul0Property, value); }
		}
		public double AngleMul1
		{
			get { return (double)GetValue(AngleMul1Property); }
			set { SetValue(AngleMul1Property, value); }
		}
		public double AngleMul2
		{
			get { return (double)GetValue(AngleMul2Property); }
			set { SetValue(AngleMul2Property, value); }
		}
		public double AngleMul3
		{
			get { return (double)GetValue(AngleMul3Property); }
			set { SetValue(AngleMul3Property, value); }
		}
		public double AngleBase
		{
			get { return (double)GetValue(AngleBaseProperty); }
			set { SetValue(AngleBaseProperty, value); }
		}



		public CurvatureShaderEffect()
		{
			var a = typeof(MainWindow).Assembly;
			string? assemblyName = a.GetName().Name;
			if (assemblyName == null)
				return;

			string uri = "pack://application:,,,/" + assemblyName + ";component/Shaders/CurvatureMapPixelShader.cso";
			var pixelShader = new PixelShader();

			try
			{
				pixelShader.UriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			pixelShader.Freeze();

			PixelShader = pixelShader;

			UpdateShaderValue(InputProperty);
			//UpdateShaderValue(InputSecondProperty);
			UpdateShaderValue(ImageWidthProperty);
			UpdateShaderValue(ImageHeightProperty);
			UpdateShaderValue(DistanceProperty);
			UpdateShaderValue(AdjustProperty);
			UpdateShaderValue(ExponentProperty);
			UpdateShaderValue(MaskHeightMinProperty);
			UpdateShaderValue(MaskHeightMaxProperty);
			UpdateShaderValue(PositiveProperty);
			UpdateShaderValue(AngleMulProperty);
			UpdateShaderValue(AngleMul0Property);
			UpdateShaderValue(AngleMul1Property);
			UpdateShaderValue(AngleMul2Property);
			UpdateShaderValue(AngleMul3Property);
			UpdateShaderValue(AngleBaseProperty);
		}
	}
}
