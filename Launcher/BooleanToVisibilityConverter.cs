using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Launcher
{
	public class BooleanToVisibilityConverter : IValueConverter
	{
		private Visibility valueWhenTrue;

		private Visibility valueWhenFalse = Visibility.Collapsed;

		private Visibility valueWhenNull = Visibility.Hidden;

		public Visibility ValueWhenTrue
		{
			get
			{
				return valueWhenTrue;
			}
			set
			{
				valueWhenTrue = value;
			}
		}

		public Visibility ValueWhenFalse
		{
			get
			{
				return valueWhenFalse;
			}
			set
			{
				valueWhenFalse = value;
			}
		}

		public Visibility ValueWhenNull
		{
			get
			{
				return valueWhenNull;
			}
			set
			{
				valueWhenNull = value;
			}
		}

		private object GetVisibility(object value)
		{
			if (!(value is bool) || value == null)
			{
				return ValueWhenNull;
			}
			if ((bool)value)
			{
				return valueWhenTrue;
			}
			return valueWhenFalse;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return GetVisibility(value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
