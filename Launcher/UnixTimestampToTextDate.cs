using System;
using System.Globalization;
using System.Threading;
using System.Windows.Data;

namespace Launcher
{
	public class UnixTimestampToTextDate : IValueConverter
	{
		private static readonly DateTime dtEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

		public static int DateTimeToUnixTime(DateTime dt)
		{
			return checked((int)(dt - dtEpoch).TotalSeconds);
		}

		public static DateTime UnixTimeToDateTime(int UnixTime)
		{
			return dtEpoch.AddSeconds(UnixTime);
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			CultureInfo actualCulture = Thread.CurrentThread.CurrentUICulture;
			return UnixTimeToDateTime((int)value).ToString(actualCulture.DateTimeFormat.MonthDayPattern.Replace("MMMM", "MMM"));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
