using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using ProtoBuf;

namespace Launcher
{
	[ProtoContract]
	public class Setting : INotifyPropertyChanged
	{
		[ProtoContract]
		public class ModifiableKeyValuePair<T1, T2>
		{
			[ProtoMember(1)]
			public T1 Key { get; set; }

			[ProtoMember(2)]
			public T2 Value { get; set; }

			public ModifiableKeyValuePair()
			{
			}

			public ModifiableKeyValuePair(T1 k, T2 v)
				: this()
			{
				Key = k;
				Value = v;
			}
		}

		internal const int FullscreenSettingIdx = 0;

		internal const int WindowedSettingIdx = 1;

		internal const int FullscreenWindowedSettingIdx = 2;

		internal const int TrueSettingIdx = 1;

		internal const int FalseSettingIdx = 0;

		internal const string FullscreenConfigKey = "Fullscreen";

		internal const string WindowedFullscreenConfigKey = "WindowedFullscreen";

		private int _selectedValue;

		[ProtoMember(1)]
		public string ConfigFileSuffix { get; set; }

		[ProtoMember(2)]
		public string ConfigSection { get; set; }

		[ProtoMember(3)]
		public string ConfigKey { get; set; }

		public int SelectedValue
		{
			get
			{
				return _selectedValue;
			}
			set
			{
				if (_selectedValue != value)
				{
					_selectedValue = value;
					NotifyPropertyChanged("SelectedValue");
					NotifyPropertyChanged("DisplayValue");
				}
			}
		}

		public string ConfigValue
		{
			get
			{
				return PossibleValues[SelectedValue].Key;
			}
		}

		public string DisplayValue
		{
			get
			{
				return PossibleValues[SelectedValue].Value;
			}
		}

		[ProtoMember(4)]
		public List<ModifiableKeyValuePair<string, string>> PossibleValues { get; set; }

		[ProtoMember(5)]
		public int DefaultValue { get; set; }

		[ProtoMember(6)]
		public string LocNameKey { get; set; }

		[ProtoMember(7)]
		public int LocValuesIndex { get; set; }

		[ProtoMember(8)]
		public ESettingType SettingType { get; set; }

		[ProtoMember(9)]
		public bool bIsAutoSetSetting { get; set; }

		[ProtoMember(10)]
		public bool bKeepExistingCapitalization { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public void NextSettingValue(bool bCanWrap)
		{
			int NextIdx = checked(SelectedValue + 1);
			if (NextIdx >= PossibleValues.Count)
			{
				NextIdx = ((!bCanWrap) ? SelectedValue : 0);
			}
			SelectedValue = NextIdx;
		}

		public void PreviousSettingValue(bool bCanWrap)
		{
			checked
			{
				int NextIdx = SelectedValue - 1;
				if (NextIdx < 0)
				{
					NextIdx = ((!bCanWrap) ? SelectedValue : (PossibleValues.Count - 1));
				}
				SelectedValue = NextIdx;
			}
		}

		public void NotifyPropertyChanged(string PropertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
			}
		}

		internal void Initialize()
		{
			ESettingType settingType = SettingType;
			if (settingType != ESettingType.Resolution)
			{
				return;
			}
			PossibleValues = new List<ModifiableKeyValuePair<string, string>>();
			string CurrentScreenRes = string.Format("{0}x{1}", SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
			foreach (ResolutionHelper.SupportedResolution res in ResolutionHelper.SupportedResolutions)
			{
				string resStr = string.Format("{0}x{1}", res.Width, res.Height);
				if (resStr.Equals(CurrentScreenRes))
				{
					DefaultValue = PossibleValues.Count;
				}
				PossibleValues.Add(new ModifiableKeyValuePair<string, string>(resStr, resStr));
			}
		}

		internal void SaveToConfig(IniFile iniFile)
		{
			Section section = iniFile.GetSection(ConfigSection);
			switch (SettingType)
			{
			case ESettingType.Basic:
			case ESettingType.AutoDetect:
				section.GetParameter(ConfigKey).SetValue(ConfigValue);
				break;
			case ESettingType.Resolution:
			{
				string[] wh = PossibleValues[SelectedValue].Key.Split('x');
				section.GetParameter("ResX").SetValue(wh[0]);
				section.GetParameter("ResY").SetValue(wh[1]);
				break;
			}
			case ESettingType.WindowMode:
				section.GetParameter("Fullscreen").SetValue((SelectedValue == 0) ? PossibleValues[0].Key : PossibleValues[1].Key);
				section.GetParameter("WindowedFullscreen").SetValue((SelectedValue == 2) ? PossibleValues[0].Key : PossibleValues[1].Key);
				break;
			}
		}

		internal void SetDefaultValueFromIni(IniFile ini)
		{
			switch (SettingType)
			{
			case ESettingType.Basic:
			case ESettingType.AutoDetect:
				SetBasicSettingDefaultValueFromIni(ini);
				break;
			case ESettingType.Resolution:
				SetResolutionSettingDefaultValueFromIni(ini);
				break;
			case ESettingType.WindowMode:
				SetWindowModeSettingDefaultValueFromIni(ini);
				break;
			}
		}

		private void SetWindowModeSettingDefaultValueFromIni(IniFile ini)
		{
			Section section = ini.GetSection(ConfigSection);
			Parameter parmFullscreen = ((section != null) ? section.GetParameter("Fullscreen") : null);
			Parameter parmWindowedFullscreen = ((section != null) ? section.GetParameter("WindowedFullscreen") : null);
			int existingIndex = ((parmFullscreen == null || !parmFullscreen.Value.Equals(PossibleValues[0].Key, StringComparison.InvariantCultureIgnoreCase)) ? ((parmWindowedFullscreen != null && parmWindowedFullscreen.Value.Equals(PossibleValues[0].Key, StringComparison.InvariantCultureIgnoreCase)) ? 2 : ((parmFullscreen != null && parmWindowedFullscreen != null) ? 1 : (-1))) : 0);
			if (existingIndex >= 0)
			{
				SelectedValue = existingIndex;
				return;
			}
			SelectedValue = DefaultValue;
			if (section == null)
			{
				ini.AddSection(ConfigSection);
			}
			if (parmFullscreen == null)
			{
				ini[ConfigSection].AddParameter(new Parameter("Fullscreen", PossibleValues[0].Key));
			}
			if (parmWindowedFullscreen == null)
			{
				ini[ConfigSection].AddParameter(new Parameter("WindowedFullscreen", PossibleValues[1].Key));
			}
		}

		private void SetResolutionSettingDefaultValueFromIni(IniFile ini)
		{
			Section section = ini.GetSection(ConfigSection);
			Parameter parmX = ((section != null) ? section.GetParameter("ResX") : null);
			Parameter parmY = ((section != null) ? section.GetParameter("ResY") : null);
			int existingIndex = ((parmX != null && parmY != null) ? PossibleValues.FindIndex((ModifiableKeyValuePair<string, string> x) => x.Key == string.Format("{0}x{1}", parmX.Value, parmY.Value)) : (-1));
			if (existingIndex >= 0)
			{
				SelectedValue = existingIndex;
				return;
			}
			SelectedValue = DefaultValue;
			if (section == null)
			{
				ini.AddSection(ConfigSection);
			}
			if (parmX == null)
			{
				ini[ConfigSection].AddParameter(new Parameter("ResX", SystemParameters.PrimaryScreenWidth.ToString()));
			}
			if (parmY == null)
			{
				ini[ConfigSection].AddParameter(new Parameter("ResY", SystemParameters.PrimaryScreenHeight.ToString()));
			}
		}

		private void SetBasicSettingDefaultValueFromIni(IniFile ini)
		{
			Section section = ini.GetSection(ConfigSection);
			Parameter parm = ((section != null) ? section.GetParameter(ConfigKey) : null);
			int existingValue = ((parm != null) ? PossibleValues.FindIndex((ModifiableKeyValuePair<string, string> x) => x.Key.ToLowerInvariant() == parm.Value.ToLowerInvariant()) : (-1));
			double parmValue;
			if (existingValue == -1 && parm != null && double.TryParse(parm.Value, out parmValue))
			{
				for (int i = 0; i < PossibleValues.Count; i = checked(i + 1))
				{
					double possibleValue;
					if (double.TryParse(PossibleValues[i].Key, out possibleValue) && possibleValue.Equals(parmValue))
					{
						existingValue = i;
						break;
					}
				}
			}
			if (existingValue >= 0)
			{
				SelectedValue = existingValue;
				return;
			}
			SelectedValue = DefaultValue;
			if (section == null)
			{
				ini.AddSection(ConfigSection);
			}
			if (parm == null)
			{
				ini[ConfigSection].AddParameter(new Parameter(ConfigKey, ConfigValue));
			}
		}

		internal bool IsDifferentFromSavedValue(IniFile iniFile)
		{
			switch (SettingType)
			{
			case ESettingType.Basic:
			case ESettingType.AutoDetect:
				return IsBasicSettingDifferentFromSavedValue(iniFile);
			case ESettingType.Resolution:
				return IsResolutionSettingDifferentFromSavedValue(iniFile);
			case ESettingType.WindowMode:
				return IsWindowModeSettingDifferentFromSavedValue(iniFile);
			default:
				throw new Exception("Forgot to set up IsDifferentFromSavedValue() for your new setting type.");
			}
		}

		private bool IsWindowModeSettingDifferentFromSavedValue(IniFile iniFile)
		{
			if (iniFile == null)
			{
				return false;
			}
			Section section = iniFile.GetSection(ConfigSection);
			bool fullEquals = section.GetParameter("Fullscreen").Value.Equals(PossibleValues[(SelectedValue != 0) ? 1 : 0].Key, StringComparison.InvariantCultureIgnoreCase);
			bool windowedFullEquals = section.GetParameter("WindowedFullscreen").Value.Equals(PossibleValues[(SelectedValue != 2) ? 1 : 0].Key, StringComparison.InvariantCultureIgnoreCase);
			if (fullEquals)
			{
				return !windowedFullEquals;
			}
			return true;
		}

		private bool IsResolutionSettingDifferentFromSavedValue(IniFile iniFile)
		{
			if (iniFile == null)
			{
				return false;
			}
			Section section = iniFile.GetSection(ConfigSection);
			return !string.Format("{0}x{1}", section.GetParameter("ResX").Value, section.GetParameter("ResY").Value).Equals(PossibleValues[SelectedValue].Key);
		}

		private bool IsBasicSettingDifferentFromSavedValue(IniFile iniFile)
		{
			if (iniFile == null)
			{
				return false;
			}
			string savedSettingValue = iniFile.GetSection(ConfigSection).GetParameter(ConfigKey).Value;
			if (!savedSettingValue.ToLower().Equals(ConfigValue.ToLower()))
			{
				double dblSaved;
				double dblCurrent;
				if (double.TryParse(savedSettingValue, out dblSaved) && double.TryParse(ConfigValue, out dblCurrent))
				{
					return !dblSaved.Equals(dblCurrent);
				}
				return true;
			}
			return false;
		}
	}
}
