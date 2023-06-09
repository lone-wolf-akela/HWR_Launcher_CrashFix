using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ProtoBuf;

namespace Launcher
{
	public class DynamicContent : INotifyPropertyChanged
	{
		private const string CachedNewsInfoFile = "NewsMetadata.wpb";

		private const string CachedNewsFile = "News.wpb";

		private const string CachedBackgroundInfoFile = "BackgroundMetadata.wpb";

		private const string CachedBackgroundFile = "Background.wid";

		private const string LauncherDataLangKey = "lang";

		public readonly string LauncherBaseUrl = string.Format("http://cdn{0}.services.gearboxsoftware.com/sparktms/hickory/pc/steam/launcher/LauncherContent.{1}.wpb", App.bUseQaEnvironment ? "-qa" : "", MainWindow.ThreeLetterLanguageCode.ToLower());

		private readonly string UserCachedDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "Homeworld Remastered", "LauncherData");

		private readonly string LauncherNewsUrl = "{0}" + string.Format("?{0}={1}", "lang", MainWindow.ThreeLetterLanguageCode);

		private ObservableCollection<NewsStory> _newsStories;

		private DynamicContentUpdateInfo CachedNewsInfo;

		private Dispatcher MainWindowDispatcher;

		public ObservableCollection<NewsStory> NewsStories
		{
			get
			{
				return _newsStories;
			}
			set
			{
				_newsStories = value;
				NotifyPropertyChanged("NewsStories");
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

		public DynamicContent()
		{
			try
			{
				Uri BaseUri = new Uri(string.Format("http://cdn{0}.services.gearboxsoftware.com/sparktms/", App.bUseQaEnvironment ? "-qa" : ""));
				if (Application.Current.Properties["TMSOverride"] != null)
				{
					string TmsOverride = Application.Current.Properties["TMSOverride"].ToString();
					if (!TmsOverride.EndsWith("/"))
					{
						TmsOverride += "/";
					}
					BaseUri = new Uri(TmsOverride);
				}
				LauncherBaseUrl = new Uri(BaseUri, string.Format("hickory/pc/steam/launcher/LauncherContent.{0}.wpb", MainWindow.ThreeLetterLanguageCode.ToLower())).ToString();
			}
			catch (Exception)
			{
			}
		}

		public DynamicContent(Dispatcher InMainWindowDispatcher)
			: this()
		{
			MainWindowDispatcher = InMainWindowDispatcher;
		}

		public void InitDynamicContent()
		{
			ThreadPool.QueueUserWorkItem(BlockingDownloadAvailableData);
		}

		private void BlockingDownloadAvailableData(object StateInfo)
		{
			TryLoadCachedNewsInfo();
			Console.WriteLine("Attempting to download dynamic content metadata from {0}...", LauncherBaseUrl);
			try
			{
				HttpWebRequest client = WebRequest.Create(LauncherBaseUrl) as HttpWebRequest;
				AvailableData availableData = null;
				client.Headers.Add("Accept-Encoding", "gzip,deflate");
				using (WebResponse response = client.GetResponse())
				{
					Console.WriteLine("Dynamic content metadata response received. Parsing...");
					using (GZipStream gs = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
					{
						availableData = Serializer.Deserialize<AvailableData>((Stream)(object)gs);
					}
				}
				if (availableData != null)
				{
					bool bFoundNewInfo = false;
					Console.WriteLine("Successfully parsed dynamic content metadata. Looking for new info...");
					MainWindowDispatcher.Invoke((Action<ObservableCollection<NewsStory>>)delegate(ObservableCollection<NewsStory> stories)
					{
						try
						{
							NewsStories = stories;
							Console.WriteLine("News stories loaded. Caching...");
						}
						catch (Exception ex2)
						{
							Console.WriteLine("Failed to load news stories. Error: {0}{1}{2}", ex2.Message, Environment.NewLine, ex2.StackTrace);
						}
					}, availableData.NewsStories);
					CachedNewsInfo = availableData.NewsInfo;
					if (CachedNewsInfo == null)
					{
						CachedNewsInfo = new DynamicContentUpdateInfo
						{
							LastUpdated = UnixTimestampToTextDate.DateTimeToUnixTime(DateTime.Now)
						};
					}
					if (TryWriteNewsCache(NewsStories))
					{
						Console.WriteLine("All news downloads and caching completed successfully.");
					}
					if (!bFoundNewInfo)
					{
						Console.WriteLine("All cached dynamic data is up to date.");
					}
				}
				else
				{
					Console.WriteLine("Failed to parse dynamic content metadata.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error retrieving dynamic content metadata: {0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace);
			}
		}

		private bool ConditionalDownloadNewNews(DynamicContentUpdateInfo dynamicContentUpdateInfo)
		{
			if (dynamicContentUpdateInfo != null && (CachedNewsInfo == null || dynamicContentUpdateInfo.LastUpdated > CachedNewsInfo.LastUpdated))
			{
				Console.WriteLine("New news info found. Old date: {0}, New date: {1}.", (CachedNewsInfo != null) ? UnixTimestampToTextDate.UnixTimeToDateTime(CachedNewsInfo.LastUpdated) : DateTime.MinValue, UnixTimestampToTextDate.UnixTimeToDateTime(dynamicContentUpdateInfo.LastUpdated));
				ThreadPool.QueueUserWorkItem(BlockingDownloadNewsStories, dynamicContentUpdateInfo);
				CachedNewsInfo = dynamicContentUpdateInfo;
				return true;
			}
			return false;
		}

		private void BlockingDownloadNewsStories(object StateInfo)
		{
			DynamicContentUpdateInfo contentInfo = StateInfo as DynamicContentUpdateInfo;
			Console.WriteLine("Retrieving news stories from {0}...", contentInfo.DataUrl);
			HttpWebRequest client = WebRequest.Create(string.Format(LauncherNewsUrl, contentInfo.DataUrl)) as HttpWebRequest;
			try
			{
				using (WebResponse response = client.GetResponse())
				{
					Console.WriteLine("News stories received. Loading...");
					MainWindowDispatcher.Invoke((Action<Stream>)delegate(Stream stream)
					{
						try
						{
							NewsStories = Serializer.Deserialize<ObservableCollection<NewsStory>>((Stream)(object)stream);
							Console.WriteLine("News stories loaded. Caching...");
						}
						catch (Exception ex2)
						{
							Console.WriteLine("Failed to load news stories. Error: {0}{1}{2}", ex2.Message, Environment.NewLine, ex2.StackTrace);
						}
					}, response.GetResponseStream());
					if (TryWriteNewsCache(NewsStories))
					{
						Console.WriteLine("All news downloads and caching completed successfully.");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to retrieve replacement background. Error: {0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace);
			}
		}

		private void TryLoadCachedNewsInfo()
		{
			Console.WriteLine("Loading cached news data from '{0}'...", "NewsMetadata.wpb");
			TryLoadCachedInfo("NewsMetadata.wpb", out CachedNewsInfo);
			if (CachedNewsInfo != null)
			{
				Console.WriteLine("Cached news info loaded.");
			}
			else
			{
				Console.WriteLine("Failed to load cached news info.");
			}
			try
			{
				string FullNewsCachePath = Path.Combine(UserCachedDataPath, "News.wpb");
				if (MainWindowDispatcher == null || !File.Exists(FullNewsCachePath))
				{
					return;
				}
				using (FileStream fs = new FileStream(FullNewsCachePath, FileMode.Open))
				{
					ObservableCollection<NewsStory> cachedNews = Serializer.Deserialize<ObservableCollection<NewsStory>>((Stream)(object)fs);
					MainWindowDispatcher.BeginInvoke((Action<ObservableCollection<NewsStory>>)delegate(ObservableCollection<NewsStory> news)
					{
						NewsStories = news;
					}, cachedNews);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to load cached news. Error: {0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace);
			}
		}

		private void TryLoadCachedInfo(string Filename, out DynamicContentUpdateInfo info)
		{
			info = null;
			try
			{
				string FullCachedPath = Path.Combine(UserCachedDataPath, Filename);
				if (File.Exists(FullCachedPath))
				{
					using (FileStream fs = new FileStream(FullCachedPath, FileMode.Open))
					{
						info = Serializer.Deserialize<DynamicContentUpdateInfo>((Stream)(object)fs);
						return;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to retrieve dynamic data for file '{0}'. Error: {1}", Filename, ex.Message);
			}
		}

		private bool TryWriteMetadataCache(string Filename, DynamicContentUpdateInfo info)
		{
			try
			{
				string FullCachedPath = Path.Combine(UserCachedDataPath, Filename);
				using (FileStream fs = new FileStream(FullCachedPath, FileMode.Create))
				{
					info.DataUrl = null;
					Serializer.Serialize((Stream)(object)fs, info);
				}
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to write dynamic data to file '{0}'. Error: {1}{2}{3}", Filename, ex.Message, Environment.NewLine, ex.StackTrace);
				return false;
			}
		}

		private bool TryWriteNewsCache(ObservableCollection<NewsStory> news)
		{
			try
			{
				if (!Directory.Exists(UserCachedDataPath))
				{
					Directory.CreateDirectory(UserCachedDataPath);
				}
				using (FileStream fs = new FileStream(Path.Combine(UserCachedDataPath, "News.wpb"), FileMode.Create))
				{
					Serializer.Serialize((Stream)(object)fs, news);
					Console.WriteLine("Cached actual news data. Caching metadata...");
				}
				if (TryWriteMetadataCache("NewsMetadata.wpb", CachedNewsInfo))
				{
					return true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to cache news stories. Error: {0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace);
			}
			return false;
		}
	}
}
