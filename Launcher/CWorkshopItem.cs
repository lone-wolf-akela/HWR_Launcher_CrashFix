using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;

namespace Launcher
{
	public class CWorkshopItem
	{
		private CSteamID steamId;

		public PublishedFileId_t fileId;

		private SubscribedItemDownloadDetails_t itemDetails;

		public EGameType gameType;

		public EModType modType;

		public string modFolder;

		private string gameModFolderPath;

		public string previewFilename;

		public string bigFilename;

		public string Title { get; set; }

		private static List<string> GetListOfOptions(string input)
		{
			List<string> output = new List<string>();
			string[] input_split = input.Split(' ');
			string[] array = input_split;
			foreach (string s in array)
			{
				s.Trim();
				output.Add(s);
			}
			return output;
		}

		public static string GetGamePathFolder(string gameDirectory)
		{
			string gamepath = AppDomain.CurrentDomain.BaseDirectory;
			if (App.bUseQaEnvironment)
			{
				gamepath = "C:\\Program Files (x86)\\Steam\\SteamApps\\common\\Homeworld\\HWLauncher\\";
			}
			return gamepath + "..\\" + gameDirectory;
		}

		public static string GetModPathFolder(string gameDirectory, CSteamID steamId)
		{
			string gamePath = GetGamePathFolder(gameDirectory);
			gamePath += "\\DataWorkshopMODs";
			string modPathFolder = Path.GetFullPath(gamePath);
			return modPathFolder + string.Format("\\{0}", steamId);
		}

		public CWorkshopItem(CSteamID InSteamId, PublishedFileId_t InFileId, SubscribedItemDownloadDetails_t InDetails)
		{
			steamId = InSteamId;
			fileId = InFileId;
			itemDetails = InDetails;
			Title = "";
			previewFilename = "";
			gameType = EGameType.GAMETYPE_Unknown;
			modType = EModType.MODTYPE_Mod;
		}

		private bool ParseConfigFile(string filename)
		{
			bool bAreFollowingLinesDescription = false;
			try
			{
				StreamReader sr = File.OpenText(filename);
				string input;
				while ((input = sr.ReadLine()) != null)
				{
					if (bAreFollowingLinesDescription)
					{
						continue;
					}
					string input_trim = input.Trim();
					if (input == "" || input_trim.Substring(0, 2) == "//")
					{
						continue;
					}
					string input_lower = input_trim.ToLower();
					if (input_lower.StartsWith("title:"))
					{
						string title_trim = input.Trim().Substring(6).Trim();
						Title = title_trim.Trim();
						if (Title == "")
						{
							Title = "Unknown";
						}
					}
					else if (input_lower.StartsWith("gametype:"))
					{
						gameType = EGameType.GAMETYPE_Unknown;
						string gametype_trim = input.Trim().Substring(9).ToLower()
							.Trim();
						List<string> gametypes = GetListOfOptions(gametype_trim);
						for (int i = 0; i < gametypes.Count; i = checked(i + 1))
						{
							if (gametypes[i] == "hw1classic")
							{
								gameType |= EGameType.GAMETYPE_HW1Classic;
							}
							else if (gametypes[i] == "hw2classic")
							{
								gameType |= EGameType.GAMETYPE_HW2Classic;
							}
							else if (gametypes[i] == "homeworldrm")
							{
								gameType |= EGameType.GAMETYPE_HomeworldRM;
							}
						}
					}
					else if (input_lower.StartsWith("modtype:"))
					{
						modType = EModType.MODTYPE_Mod;
						switch (input.Trim().Substring(8).ToLower()
							.Trim())
						{
						case "mod":
							modType = EModType.MODTYPE_Mod;
							break;
						case "locale":
							modType = EModType.MODTYPE_Locale;
							break;
						case "badges":
							modType = EModType.MODTYPE_Badges;
							break;
						case "cursors":
							modType = EModType.MODTYPE_Cursors;
							break;
						}
					}
					else if (input_lower.StartsWith("bigfilename:"))
					{
						if (!(bigFilename = input.Trim().Substring(12).Trim()).EndsWith(".big"))
						{
							bigFilename += ".big";
						}
					}
					else if (input_lower.StartsWith("description:"))
					{
						bAreFollowingLinesDescription = true;
					}
				}
				if (gameType == EGameType.GAMETYPE_Unknown)
				{
					gameType = EGameType.GAMETYPE_HomeworldRM;
				}
				if (gameType == EGameType.GAMETYPE_HW1Classic)
				{
					modType = EModType.MODTYPE_Mod;
				}
				sr.Close();
			}
			catch
			{
				return false;
			}
			return true;
		}

		private void CopyFileIfNewer(string srcFile, string destFile)
		{
			FileInfo srcFileInfo = new FileInfo(srcFile);
			FileInfo destFileInfo = new FileInfo(destFile);
			bool bCopyFile = true;
			if (destFileInfo.Exists && srcFileInfo.LastWriteTime <= destFileInfo.LastWriteTime)
			{
				bCopyFile = false;
			}
			if (bCopyFile)
			{
				srcFileInfo.CopyTo(destFileInfo.FullName, true);
			}
		}

		private void CopySteamWorkshopFilesToGameDirectory(string gamepath, ref List<string> modWorkshopItemFolderNames)
		{
			checked
			{
				try
				{
					if (!Directory.Exists(gamepath))
					{
						return;
					}
					string baseDirPath = string.Format("{0}\\DataWorkshopMODs", Path.GetFullPath(gamepath));
					Directory.CreateDirectory(baseDirPath);
					modFolder = string.Format("{0}", steamId);
					string dirPath = baseDirPath + string.Format("\\{0}", modFolder);
					Directory.CreateDirectory(dirPath);
					modFolder += string.Format("\\{0}", fileId);
					dirPath = baseDirPath + string.Format("\\{0}", modFolder);
					Directory.CreateDirectory(dirPath);
					gameModFolderPath = dirPath;
					for (int index2 = 0; index2 < modWorkshopItemFolderNames.Count; index2++)
					{
						if (modWorkshopItemFolderNames[index2] == gameModFolderPath)
						{
							modWorkshopItemFolderNames.RemoveAt(index2);
							break;
						}
					}
					string[] dirFiles = Directory.GetFiles(itemDetails.itemFolderName, "*.*");
					for (int index = 0; index < dirFiles.Length; index++)
					{
						string destFilename = gameModFolderPath + "\\" + Path.GetFileName(dirFiles[index]);
						CopyFileIfNewer(dirFiles[index], destFilename);
					}
					previewFilename = gameModFolderPath + "\\preview.jpg";
					if (!File.Exists(previewFilename))
					{
						previewFilename = gameModFolderPath + "\\preview.png";
						if (!File.Exists(previewFilename))
						{
							previewFilename = "";
						}
					}
				}
				catch
				{
				}
			}
		}

		public void ProcessWorkshopItem(ref List<string> modWorkshopItemFolderNames)
		{
			string config_filename = itemDetails.itemFolderName + "\\Config.txt";
			if (ParseConfigFile(config_filename))
			{
				string gamepath = "";
				if ((gameType & EGameType.GAMETYPE_HW1Classic) == EGameType.GAMETYPE_HW1Classic)
				{
					gamepath = GetGamePathFolder("Homeworld1Classic");
					CopySteamWorkshopFilesToGameDirectory(gamepath, ref modWorkshopItemFolderNames);
				}
				if ((gameType & EGameType.GAMETYPE_HW2Classic) == EGameType.GAMETYPE_HW2Classic)
				{
					gamepath = GetGamePathFolder("Homeworld2Classic");
					CopySteamWorkshopFilesToGameDirectory(gamepath, ref modWorkshopItemFolderNames);
				}
				if ((gameType & EGameType.GAMETYPE_HomeworldRM) == EGameType.GAMETYPE_HomeworldRM)
				{
					gamepath = GetGamePathFolder("HomeworldRM");
					CopySteamWorkshopFilesToGameDirectory(gamepath, ref modWorkshopItemFolderNames);
				}
			}
		}
	}
}
