using System;
using System.Collections.Generic;

namespace Launcher
{
	internal class ResolutionHelper
	{
		public struct SupportedResolution : IEquatable<SupportedResolution>, IComparable<SupportedResolution>
		{
			public int Width;

			public int Height;

			public bool Equals(SupportedResolution other)
			{
				if (Width == other.Width)
				{
					return Height == other.Height;
				}
				return false;
			}

			public int GetArea()
			{
				return checked(Width * Height);
			}

			public int CompareTo(SupportedResolution other)
			{
				int comparison = Width.CompareTo(other.Width);
				if (comparison == 0)
				{
					comparison = Height.CompareTo(other.Height);
				}
				return comparison;
			}
		}

		private static List<SupportedResolution> _supportedResolutions;

		public static List<SupportedResolution> SupportedResolutions
		{
			get
			{
				if (_supportedResolutions == null)
				{
					_supportedResolutions = new List<SupportedResolution>();
					PopulateSupportedResolutions();
				}
				return _supportedResolutions;
			}
		}

		private static void PopulateSupportedResolutions()
		{
			NativeMethods.DEVMODE vDevMode = default(NativeMethods.DEVMODE);
			SupportedResolutions.Clear();
			for (int i = 0; NativeMethods.EnumDisplaySettings(null, i, ref vDevMode); i = checked(i + 1))
			{
				SupportedResolution supportedResolution = default(SupportedResolution);
				supportedResolution.Width = vDevMode.dmPelsWidth;
				supportedResolution.Height = vDevMode.dmPelsHeight;
				SupportedResolution res = supportedResolution;
				if (!SupportedResolutions.Contains(res))
				{
					SupportedResolutions.Add(res);
				}
			}
			SupportedResolutions.Sort();
		}
	}
}
