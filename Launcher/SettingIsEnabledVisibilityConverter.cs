using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Launcher
{
	public class SettingIsEnabledVisibilityConverter : IValueConverter
	{
		private static DependencyObject obj = new DependencyObject();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (DesignerProperties.GetIsInDesignMode(obj))
			{
				return Visibility.Visible;
			}
			if (!(value is Setting) || targetType != typeof(Visibility))
			{
				throw new InvalidOperationException("Must pass a setting to return a visibility for. Passed " + value.ToString() + targetType.ToString());
			}
			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
