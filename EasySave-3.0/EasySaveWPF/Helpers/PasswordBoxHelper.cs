using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EasySaveWPF.Helpers
{
    internal class PasswordBoxHelper
    {
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged)
                {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        private static bool _isUpdating;

        public static string GetBoundPassword(DependencyObject d)
        {
            return (string)d.GetValue(BoundPasswordProperty);
        }

        public static void SetBoundPassword(DependencyObject d, string value)
        {
            d.SetValue(BoundPasswordProperty, value);
        }

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PasswordBox box) return;

            // Prevent recursive updates
            box.PasswordChanged -= PasswordChanged;

            if (!_isUpdating)
            {
                _isUpdating = true;
                box.Password = e.NewValue?.ToString() ?? string.Empty;
                _isUpdating = false;
            }

            // Remove the line that uses SelectionStart as PasswordBox does not support it
            // box.SelectionStart = box.Password.Length;

            box.PasswordChanged += PasswordChanged;
        }

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdating) return;

            if (sender is PasswordBox box)
            {
                _isUpdating = true;
                SetBoundPassword(box, box.Password);
                _isUpdating = false;
            }
        }
    }
}