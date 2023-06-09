using System;
using System.Globalization;
using System.Windows.Data;

namespace Launcher
{
	public class OpacityToBool : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double && parameter != null)
			{
				try
				{
					return object.Equals(value, double.Parse(parameter as string, CultureInfo.InvariantCulture));
				}
				catch (Exception)
				{
				}
				double parsedValue = 0.0;
				if (!double.TryParse(parameter as string, out parsedValue))
				{
					return value.ToString() == "1";
				}
				return object.Equals(value, parsedValue);
			}
			throw new Exception("Unexpected value type. Value must be a double and the converter parameter must be the double that 'value' should be equal to.");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
