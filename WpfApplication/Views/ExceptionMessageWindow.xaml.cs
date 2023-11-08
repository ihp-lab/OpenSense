#nullable enable

using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenSense.WPF.Views {
    public sealed partial class ExceptionMessageWindow : Window {

        private ExceptionMessageWindow(Exception exception, string title, string message) {
            InitializeComponent();

            /* Load error icon */
            var icon = SystemIcons.Error.ToBitmap();
            ImageError.Source = Imaging.CreateBitmapSourceFromHIcon(
                icon.GetHicon(),
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            Title = title;
            TextBlockMessage.Text = message;
            TextBoxException.Text = exception.ToString();
        }

        public static void Show(DependencyObject control, Exception exception, string title, string message) {
            var owner = GetParentWindow(control);
            var window = new ExceptionMessageWindow(exception, title, message) { 
                Owner = owner,
            };
            window.ShowDialog();
        }

        #region Control Event Handlers
        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            Close();
        }
        #endregion

        private static Window? GetParentWindow(DependencyObject? obj) {
            if (obj is Window window) {
                return window;
            }
            if (obj is null) {
                return null;
            }
            var parent = VisualTreeHelper.GetParent(obj);
            var result = GetParentWindow(parent);
            return result;
        }
    }
}
