using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Launcher
{
	public class LauncherSettings : INotifyPropertyChanged
	{
		private bool _bWindowed;

		private bool _bMuteAudio;

		private static readonly string UserCachedDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "Homeworld Remastered", "LauncherData");

		private string LoadedFromPath;

		public bool bWindowed
		{
			get
			{
				return _bWindowed;
			}
			set
			{
				if (_bWindowed != value)
				{
					_bWindowed = value;
					NotifyPropertyChanged("bWindowed");
				}
			}
		}

		public bool bMuteAudio
		{
			get
			{
				return _bMuteAudio;
			}
			set
			{
				if (_bMuteAudio != value)
				{
					_bMuteAudio = value;
					NotifyPropertyChanged("bMuteAudio");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public string GetHW2WindowedParam()
		{
			if (!_bWindowed)
			{
				return "";
			}
			return " -windowed";
		}

		public string GetHWWindowedParam()
		{
			if (!_bWindowed)
			{
				return "";
			}
			return " /window";
		}

		public void NotifyPropertyChanged(string PropertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
			}
		}

		public static LauncherSettings Load(string FromFile = "LauncherSettings.hws")
		{
			LauncherSettings retval = null;
			if (string.IsNullOrEmpty(FromFile))
			{
				return retval;
			}
			string fullPath = Path.Combine(UserCachedDataPath, FromFile);
			if (!File.Exists(fullPath))
			{
				return retval;
			}
			try
			{
				using (FileStream fs = new FileStream(fullPath, FileMode.Open))
				{
					XmlSerializer Serializer = new XmlSerializer(typeof(LauncherSettings));
					retval = (LauncherSettings)Serializer.Deserialize(fs);
					retval.LoadedFromPath = fullPath;
					return retval;
				}
			}
			catch (Exception)
			{
				return retval;
			}
		}

		private LauncherSettings()
		{
		}

		public LauncherSettings(string FromFile = "LauncherSettings.hws")
			: this()
		{
			LoadedFromPath = Path.Combine(UserCachedDataPath, FromFile);
		}

		public void Save()
		{
			try
			{
				if (!Directory.Exists(Path.GetDirectoryName(LoadedFromPath)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(LoadedFromPath));
				}
				using (FileStream fs = new FileStream(LoadedFromPath, FileMode.Create))
				{
					XmlSerializer Serializer = new XmlSerializer(typeof(LauncherSettings));
					Serializer.Serialize(fs, this);
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
