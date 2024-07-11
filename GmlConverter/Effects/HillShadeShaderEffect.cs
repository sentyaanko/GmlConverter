using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace GmlConverter.Effects
{
	internal class HillShadeShaderEffect : ShaderEffect
	{
		public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(
			"Input", typeof(HillShadeShaderEffect), 0, SamplingMode.Bilinear);

		public static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register(
			"ImageWidth", typeof(double), typeof(HillShadeShaderEffect),
				new UIPropertyMetadata(2048.0, PixelShaderConstantCallback(0)));
		public static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register(
			"ImageHeight", typeof(double), typeof(HillShadeShaderEffect),
				new UIPropertyMetadata(2048.0, PixelShaderConstantCallback(1)));
		public static readonly DependencyProperty DistanceProperty = DependencyProperty.Register(
			"Distance", typeof(double), typeof(HillShadeShaderEffect),
				new UIPropertyMetadata(5.0, PixelShaderConstantCallback(2)));
		public static readonly DependencyProperty LightSourceAzimuthProperty = DependencyProperty.Register(
			"LightSourceAzimuth", typeof(double), typeof(HillShadeShaderEffect),
				new UIPropertyMetadata(315.0, PixelShaderConstantCallback(3)));
		public static readonly DependencyProperty LightSourceAltitudeProperty = DependencyProperty.Register(
			"LightSourceAltitude", typeof(double), typeof(HillShadeShaderEffect),
				new UIPropertyMetadata(45.0, PixelShaderConstantCallback(4)));
		public static readonly DependencyProperty DrawShadeProperty = DependencyProperty.Register(
			"DrawShade", typeof(double), typeof(HillShadeShaderEffect),
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
		public double LightSourceAzimuth
		{
			get { return (double)GetValue(LightSourceAzimuthProperty); }
			set { SetValue(LightSourceAzimuthProperty, value); }
		}
		public double LightSourceAltitude
		{
			get { return (double)GetValue(LightSourceAltitudeProperty); }
			set { SetValue(LightSourceAltitudeProperty, value); }
		}
		public double DrawShade
		{
			get { return (double)GetValue(DrawShadeProperty); }
			set { SetValue(DrawShadeProperty, value); }
		}


		public HillShadeShaderEffect()
		{
			var a = typeof(MainWindow).Assembly;
			string? assemblyName = a.GetName().Name;
			if (assemblyName == null)
				return;

			string uri = "pack://application:,,,/" + assemblyName + ";component/Shaders/HillShadePixelShader.cso";
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
			UpdateShaderValue(LightSourceAzimuthProperty);
			UpdateShaderValue(LightSourceAltitudeProperty);
			UpdateShaderValue(DrawShadeProperty);
		}
	}
}
