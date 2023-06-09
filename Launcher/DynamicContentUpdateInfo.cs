using ProtoBuf;

namespace Launcher
{
	[ProtoContract]
	public class DynamicContentUpdateInfo
	{
		[ProtoMember(1)]
		public int LastUpdated { get; set; }

		[ProtoMember(2)]
		public string DataUrl { get; set; }
	}
}
