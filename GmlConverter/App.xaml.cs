using GmlConverter.ViewModels;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace GmlConverter
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		/// <summary>
		/// アプリを通して使用するサービスプロバイダーのインスタンス
		/// </summary>
		public IServiceProvider? ServiceProvider { get; private set; }

		protected override void OnStartup(StartupEventArgs e)
		{
			MagickNET.Initialize();
			ServiceProvider = ConfigureServices();
			base.OnStartup(e);
		}

		/// <summary>
		/// SharedViewModel のインスタンス生成を制御するためのサービスプロバイダーの構築
		/// </summary>
		/// <returns></returns>
		private IServiceProvider ConfigureServices()
		{
			ServiceCollection serviceCollection = new();
			serviceCollection.AddSingleton<SharedViewModel>();
			return serviceCollection.BuildServiceProvider();
		}
	}

}
