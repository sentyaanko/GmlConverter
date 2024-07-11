using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GmlConverter.Views.UserControls
{
	/// <summary>
	/// GridControl.xaml の相互作用ロジック
	/// </summary>
	public partial class GridControl : UserControl
	{
		public static readonly DependencyProperty CenterPointProperty =
			DependencyProperty.Register(
				nameof(CenterPoint),
				typeof(System.Drawing.Point),
				typeof(GridControl),
				new PropertyMetadata(new System.Drawing.Point(-1), new(OnPropertyChanged))
			);
		public static readonly DependencyProperty GridSpacingProperty =
			DependencyProperty.Register(
				nameof(GridSpacing),
				typeof(int),
				typeof(GridControl),
				new PropertyMetadata(0, new(OnPropertyChanged))
			);

		private static void OnPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			var gridControl = d as GridControl;
			if(gridControl == null)
				return;
			if (e.OldValue != e.NewValue)
				gridControl.UpdateGrid();
		}

		public System.Drawing.Point CenterPoint
		{
			get => (System.Drawing.Point)GetValue(CenterPointProperty);
			set => SetValue(CenterPointProperty, value);
		}
		public int GridSpacing
		{
			get => (int)GetValue(GridSpacingProperty);
			set => SetValue(GridSpacingProperty, value);
		}

		public GridControl()
		{
			InitializeComponent();
		}

		private void UpdateGrid()
		{
			if (mainCanvas == null)
				return;
			if (CenterPoint.X < 0 || CenterPoint.Y < 0 || GridSpacing == 0)
				return;
			mainCanvas.Width = Width;
			mainCanvas.Height = Height;

			var toCorner = new System.Drawing.Size(GridSpacing / 2, GridSpacing / 2);
			var cornerPoint = CenterPoint + toCorner;

			mainCanvas.Children.Clear();
			for (int y = cornerPoint.Y % GridSpacing; y < (int)mainCanvas.Height; y += GridSpacing)
			{
				mainCanvas.Children.Add(new Line()
				{
					X1 = 0,
					Y1 = y,
					X2 = mainCanvas.Width,
					Y2 = y,
					Stroke = Brushes.LightGreen,
					StrokeThickness = 1,
				});
			}
			for (int x = cornerPoint.X % GridSpacing; x < (int)mainCanvas.Width; x += GridSpacing)
			{
				mainCanvas.Children.Add(new Line()
				{
					X1 = x,
					Y1 = 0,
					X2 = x,
					Y2 = mainCanvas.Height,
					Stroke = Brushes.LightGreen,
					StrokeThickness = 1,
				});
			}
		}
	}
}
