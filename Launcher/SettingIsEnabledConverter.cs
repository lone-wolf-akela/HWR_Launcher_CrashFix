using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Launcher
{
	public class SettingIsEnabledConverter : IValueConverter
	{
		private static DependencyObject obj = new DependencyObject();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (DesignerProperties.GetIsInDesignMode(obj))
			{
				return true;
			}
			if (!(value is Setting) || targetType != typeof(bool))
			{
				throw new InvalidOperationException("Must pass a setting to return IsEnabled (bool) for. Passed " + value.ToString() + targetType.ToString());
			}
			return true;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
