using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace GmlConverter.Effects
{
	internal class HeightShaderEffect : ShaderEffect
	{
		public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(
			"Input", typeof(HeightShaderEffect), 0, SamplingMode.Bilinear);

		public Brush Input
		{
			get { return (Brush)GetValue(InputProperty); }
			set { SetValue(InputProperty, value); }
		}

		public HeightShaderEffect()
		{
			var a = typeof(MainWindow).Assembly;
			string? assemblyName = a.GetName().Name;
			if (assemblyName == null)
				return;

			string uri = "pack://application:,,,/" + assemblyName + ";component/Shaders/HeightMapPixelShader.cso";
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
		}
	}
}
