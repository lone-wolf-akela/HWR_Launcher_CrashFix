using System;
using Steamworks;

namespace Launcher
{
	internal class CSteamInterface
	{
		public static PublishedFileId_t[] subscribedItems;

		public static SteamUGCDetails_t[] subscribedItemDetails;

		public static SubscribedItemDownloadDetails_t[] subscribedDownloadItems;

		public static uint numSubscribedItems;

		public static int subscribedItemIndex;

		private static CallResult<SteamUGCRequestUGCDetailsResult_t> m_SteamUGCRequestUGCDetailsResultCallback;

		private static GetSubscribedItemsDelegate SubscribedItems_delegate;

		public CSteamInterface()
		{
			subscribedItems = null;
			subscribedItemDetails = null;
			subscribedDownloadItems = null;
			m_SteamUGCRequestUGCDetailsResultCallback = CallResult<SteamUGCRequestUGCDetailsResult_t>.Create(OnSubscribedItemResult);
		}

		public static void GetSubscribedItems(GetSubscribedItemsDelegate InSubscribedItemsDelegate)
		{
			SubscribedItems_delegate = InSubscribedItemsDelegate;
			numSubscribedItems = SteamUGC.GetNumSubscribedItems();
			if (numSubscribedItems != 0)
			{
				subscribedItems = new PublishedFileId_t[numSubscribedItems];
				subscribedItemDetails = new SteamUGCDetails_t[numSubscribedItems];
				subscribedDownloadItems = new SubscribedItemDownloadDetails_t[numSubscribedItems];
				uint numPopulatedSubscribedItems = SteamUGC.GetSubscribedItems(subscribedItems, numSubscribedItems);
				if (numPopulatedSubscribedItems == numSubscribedItems)
				{
					subscribedItemIndex = 0;
					bool bLegacy;
					if (SteamUGC.GetItemUpdateInfo(subscribedItems[subscribedItemIndex], out subscribedDownloadItems[subscribedItemIndex].bNeedsUpdate, out subscribedDownloadItems[subscribedItemIndex].bIsDownloading, out subscribedDownloadItems[subscribedItemIndex].bytesDownloaded, out subscribedDownloadItems[subscribedItemIndex].bytesTotalSize) && SteamUGC.GetItemInstallInfo(subscribedItems[subscribedItemIndex], out subscribedDownloadItems[subscribedItemIndex].sizeOnDisk, out subscribedDownloadItems[subscribedItemIndex].itemFolderName, 1024u, out bLegacy))
					{
						SteamAPICall_t hSteamAPICall = SteamUGC.RequestUGCDetails(subscribedItems[subscribedItemIndex], 60u);
						if (hSteamAPICall != SteamAPICall_t.Invalid)
						{
							m_SteamUGCRequestUGCDetailsResultCallback.Set(hSteamAPICall);
							return;
						}
					}
				}
				MainWindow.MyMainWindow.Dispatcher.BeginInvoke((Action)delegate
				{
					SubscribedItems_delegate(EResult.k_EResultFail);
				});
			}
			else
			{
				MainWindow.MyMainWindow.Dispatcher.BeginInvoke((Action)delegate
				{
					SubscribedItems_delegate(EResult.k_EResultOK);
				});
			}
		}

		public static void GetNextSubscribedItem(GetSubscribedItemsDelegate InSubscribedItemsDelegate)
		{
			SubscribedItems_delegate = InSubscribedItemsDelegate;
			checked
			{
				subscribedItemIndex++;
				bool bLegacy;
				if (subscribedItemIndex < numSubscribedItems && SteamUGC.GetItemUpdateInfo(subscribedItems[subscribedItemIndex], out subscribedDownloadItems[subscribedItemIndex].bNeedsUpdate, out subscribedDownloadItems[subscribedItemIndex].bIsDownloading, out subscribedDownloadItems[subscribedItemIndex].bytesDownloaded, out subscribedDownloadItems[subscribedItemIndex].bytesTotalSize) && SteamUGC.GetItemInstallInfo(subscribedItems[subscribedItemIndex], out subscribedDownloadItems[subscribedItemIndex].sizeOnDisk, out subscribedDownloadItems[subscribedItemIndex].itemFolderName, 1024u, out bLegacy))
				{
					SteamAPICall_t hSteamAPICall = SteamUGC.RequestUGCDetails(subscribedItems[subscribedItemIndex], 60u);
					if (hSteamAPICall != SteamAPICall_t.Invalid)
					{
						m_SteamUGCRequestUGCDetailsResultCallback.Set(hSteamAPICall);
						return;
					}
				}
				SubscribedItems_delegate(EResult.k_EResultOK);
			}
		}

		private static void OnSubscribedItemResult(SteamUGCRequestUGCDetailsResult_t pCallback, bool bIOFailure)
		{
			subscribedItemDetails[subscribedItemIndex] = pCallback.m_details;
			MainWindow.MyMainWindow.Dispatcher.BeginInvoke((Action)delegate
			{
				SubscribedItems_delegate(EResult.k_EResultOK);
			});
		}
	}
}
