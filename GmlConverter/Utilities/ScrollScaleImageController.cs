using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GmlConverter.Utilities
{
    internal class ScrollScaleImageController
    {
        Point _lastMousePosition = new();
		bool _isImageDragging = false;
		Image? _image = null;
		ScaleTransform? _scaleTransform = null;
		ScrollViewer? _scrollViewer = null;

		internal ScrollScaleImageController()
        {
        }
        internal void Loaded(Image image, ScaleTransform scaleTransform, ScrollViewer scrollViewer)
        {
            _image = image;
            _scaleTransform = scaleTransform;
            _scrollViewer = scrollViewer;
        }

        internal void PreviewScaleChange(object sender, ExecutedRoutedEventArgs e)
        {
            var s = _image?.Source;
            if (s == null)
                return;
            var param = e.Parameter as string;
            if (param == null)
                return;
            switch (param)
            {
                case "Reset":
                case "Up":
                case "Down":
                    SetPreviewScale(param, new(s.Width / 2, s.Height / 2));
                    break;
            };
        }
        internal void MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_scrollViewer == null)
                return;

            _lastMousePosition = e.GetPosition(_scrollViewer);
            _isImageDragging = true;
            _scrollViewer.CaptureMouse();
        }
        internal void MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_scrollViewer == null)
                return;

            _isImageDragging = false;
            _scrollViewer.ReleaseMouseCapture();
        }
        internal void MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isImageDragging)
                return;
            if (_scrollViewer == null)
                return;

            var currentPosition = e.GetPosition(_scrollViewer);
            var delta = currentPosition - _lastMousePosition;
            _scrollViewer.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset - delta.X);
            _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset - delta.Y);

            _lastMousePosition = currentPosition;
        }

        internal void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                return;

            SetPreviewScale(e.Delta > 0 ? "Up" : "Down", e.GetPosition(_image));
        }

        private void SetPreviewScale(string mode, Point anchor)
        {
            var maxScale = 8;
            var minScale = 0.01;

            if (_scrollViewer == null)
                return;
            if (_scaleTransform == null)
                return;

            var oldScale = _scaleTransform.ScaleX;
            var newScale = mode switch
            {
                "Reset" => 1,
                "Up" => oldScale < maxScale ? Math.Min(maxScale, oldScale * 1.1) : oldScale,
                "Down" => oldScale > minScale ? Math.Max(minScale, oldScale / 1.1) : oldScale,
                _ => oldScale
            };
            if (oldScale != newScale)
            {
                _scaleTransform.ScaleX = _scaleTransform.ScaleY = newScale;

                var offsetX = anchor.X * (newScale - oldScale);
                var offsetY = anchor.Y * (newScale - oldScale);
                _scrollViewer.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset + offsetX);
                _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset + offsetY);
            }
        }
    }

}
