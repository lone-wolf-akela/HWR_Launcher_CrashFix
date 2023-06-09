using System.Runtime.InteropServices;

namespace Launcher
{
	public class NativeMethods
	{
		public enum ScreenOrientation
		{
			Angle0,
			Angle90,
			Angle180,
			Angle270
		}

		public struct DEVMODE
		{
			private const int CCHDEVICENAME = 32;

			private const int CCHFORMNAME = 32;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string dmDeviceName;

			public short dmSpecVersion;

			public short dmDriverVersion;

			public short dmSize;

			public short dmDriverExtra;

			public int dmFields;

			public int dmPositionX;

			public int dmPositionY;

			public ScreenOrientation dmDisplayOrientation;

			public int dmDisplayFixedOutput;

			public short dmColor;

			public short dmDuplex;

			public short dmYResolution;

			public short dmTTOption;

			public short dmCollate;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string dmFormName;

			public short dmLogPixels;

			public int dmBitsPerPel;

			public int dmPelsWidth;

			public int dmPelsHeight;

			public int dmDisplayFlags;

			public int dmDisplayFrequency;

			public int dmICMMethod;

			public int dmICMIntent;

			public int dmMediaType;

			public int dmDitherType;

			public int dmReserved1;

			public int dmReserved2;

			public int dmPanningWidth;

			public int dmPanningHeight;
		}

		[DllImport("user32.dll")]
		internal static extern bool EnumDisplaySettings([MarshalAs(UnmanagedType.LPWStr)] string deviceName, int modeNum, ref DEVMODE devMode);
	}
}
