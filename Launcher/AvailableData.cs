using System.Collections.ObjectModel;
using ProtoBuf;

namespace Launcher
{
	[ProtoContract]
	public class AvailableData
	{
		[ProtoMember(1)]
		public DynamicContentUpdateInfo NewsInfo { get; set; }

		[ProtoMember(2)]
		public DynamicContentUpdateInfo BackgroundInfo { get; set; }

		[ProtoMember(3)]
		public ObservableCollection<NewsStory> NewsStories { get; set; }
	}
}
