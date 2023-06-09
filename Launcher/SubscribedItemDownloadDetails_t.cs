namespace Launcher
{
	public struct SubscribedItemDownloadDetails_t
	{
		public bool bNeedsUpdate;

		public bool bIsDownloading;

		public ulong bytesDownloaded;

		public ulong bytesTotalSize;

		public ulong sizeOnDisk;

		public string itemFolderName;
	}
}
