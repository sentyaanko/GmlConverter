using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace GmlConverter.Effects
{
	internal class AmbientOcclusionShaderEffect : ShaderEffect
	{
		public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(
			"Input", typeof(AmbientOcclusionShaderEffect), 0, SamplingMode.Bilinear);

		public static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register(
			"ImageWidth", typeof(double), typeof(AmbientOcclusionShaderEffect),
				new UIPropertyMetadata(2048.0, PixelShaderConstantCallback(1)));
		public static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register(
			"ImageHeight", typeof(double), typeof(AmbientOcclusionShaderEffect),
				new UIPropertyMetadata(2048.0, PixelShaderConstantCallback(2)));
		public static readonly DependencyProperty DistanceProperty = DependencyProperty.Register(
			"Distance", typeof(double), typeof(AmbientOcclusionShaderEffect),
				new UIPropertyMetadata(5.0, PixelShaderConstantCallback(3)));

		public static readonly DependencyProperty AdjustProperty = DependencyProperty.Register(
			"Adjust", typeof(double), typeof(AmbientOcclusionShaderEffect),
				new UIPropertyMetadata(1.0, PixelShaderConstantCallback(4)));
		public static readonly DependencyProperty ExponentProperty = DependencyProperty.Register(
			"Exponent", typeof(double), typeof(AmbientOcclusionShaderEffect),
				new UIPropertyMetadata(1.0, PixelShaderConstantCallback(5)));

		public Brush Input
		{
			get { return (Brush)GetValue(InputProperty); }
			set { SetValue(InputProperty, value); }
		}

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
		

		public AmbientOcclusionShaderEffect()
		{
			var a = typeof(MainWindow).Assembly;
			string? assemblyName = a.GetName().Name;
			if (assemblyName == null)
				return;

			string uri = "pack://application:,,,/" + assemblyName + ";component/Shaders/AmbientOcclusionMapPixelShader.cso";
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
			UpdateShaderValue(ImageWidthProperty);
			UpdateShaderValue(ImageHeightProperty);
			UpdateShaderValue(DistanceProperty);
			UpdateShaderValue(AdjustProperty);
			UpdateShaderValue(ExponentProperty);
		}
	}
}
