using GmlConverter.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace GmlConverter.Views.UserControls
{
    /// <summary>
    /// Interaction logic for TilePng.xaml
    /// </summary>
    public partial class TilePng : UserControl, IDisposable
	{
		private TilePngViewModel _vm;

		public TilePng()
		{
			_vm = new();
			DataContext = _vm;
			InitializeComponent();
		}

		~TilePng() => Dispose();

		public void Dispose()
		{
			_vm.Dispose();
		}

		private void LoadButton_Click(object sender, RoutedEventArgs e) => _vm.Load();
		private void SaveButton_Click(object sender, RoutedEventArgs e) => _vm.Save();

		private void SectionSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				var i = e.AddedItems[0];
				if(i != null)
				{
					var pair = (KeyValuePair<SectionSize, string>)(i);
					_vm.MarginSectionSize = pair.Key;
				}
			}
		}
	}

	public class SectionSizeStringDictionary
	{
		// ComboBoxの一覧に表示するデータ
		public Dictionary<SectionSize, string> EffectTypeNameDictionary { get; } = new();

		public SectionSizeStringDictionary()
		{
			EffectTypeNameDictionary.Add(SectionSize.invalid, "0");
			EffectTypeNameDictionary.Add(SectionSize.x7, "7x7");
			EffectTypeNameDictionary.Add(SectionSize.x15, "15x15");
			EffectTypeNameDictionary.Add(SectionSize.x31, "31x31");
			EffectTypeNameDictionary.Add(SectionSize.x63, "63x63");
			EffectTypeNameDictionary.Add(SectionSize.x127, "127x127");
			EffectTypeNameDictionary.Add(SectionSize.x255, "255x255");
		}
	}
}
