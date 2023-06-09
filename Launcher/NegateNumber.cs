using System;
using System.Globalization;
using System.Windows.Data;

namespace Launcher
{
	public class NegateNumber : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int)
			{
				return checked(-(int)value);
			}
			if (value is float)
			{
				return 0f - (float)value;
			}
			if (value is double)
			{
				return 0.0 - (double)value;
			}
			throw new Exception(string.Format("Supplied value was of unexpected type {0}.", value.GetType()));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
