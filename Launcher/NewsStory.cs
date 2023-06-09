using System.ComponentModel;
using ProtoBuf;

namespace Launcher
{
	[ProtoContract]
	public class NewsStory : INotifyPropertyChanged
	{
		private string _headline;

		private int _date;

		private string _url;

		private string _fullStory;

		[ProtoMember(1)]
		public string Headline
		{
			get
			{
				return _headline;
			}
			set
			{
				_headline = value;
				NotifyPropertyChanged("Headline");
			}
		}

		[ProtoMember(2)]
		public int Date
		{
			get
			{
				return _date;
			}
			set
			{
				_date = value;
				NotifyPropertyChanged("Date");
			}
		}

		[ProtoMember(3)]
		public string Url
		{
			get
			{
				return _url;
			}
			set
			{
				_url = value;
				NotifyPropertyChanged("Url");
			}
		}

		[ProtoMember(4)]
		public string FullStory
		{
			get
			{
				return _fullStory;
			}
			set
			{
				_fullStory = value;
				NotifyPropertyChanged("FullStory");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
