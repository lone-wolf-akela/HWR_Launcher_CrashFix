using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using Steamworks;

namespace Launcher
{
	public class App : Application
	{
		public static readonly AppId_t BaseHomeworldAppId = new AppId_t(244160u);

		public static readonly AppId_t HomeworldDlcAppId = new AppId_t(255550u);

		public static readonly AppId_t Homeworld2DlcAppId = new AppId_t(255551u);

		private bool _contentLoaded;

		public static bool bUseQaEnvironment { get; private set; }

		public static bool bSkipLauncher { get; private set; }

		public static bool bSteamInitialized { get; private set; }

		public static bool bIsTest { get; private set; }

		public static int gl_required_major { get; private set; }

		public static int gl_required_minor { get; private set; }

		public static int AMD_min_major { get; private set; }

		public static int AMD_min_minor { get; private set; }

		public static ulong SteamLobbyId { get; private set; }

		public App()
		{
			bSteamInitialized = false;
			if (SteamAPI.Init())
			{
				bSteamInitialized = true;
				string lang = GetCurrentGameLanguage();
				SetLanguage(lang);
			}
		}

		public static string GetCurrentGameLanguage()
		{
			if (bSteamInitialized)
			{
				return SteamApps.GetCurrentGameLanguage();
			}
			return null;
		}

		public static void SetLanguage(string lang)
		{
			switch (lang)
			{
			case "german":
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");
				break;
			case "french":
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
				break;
			case "italian":
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("it-IT");
				break;
			case "spanish":
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-ES");
				break;
			case "russian":
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
				break;
			default:
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
				break;
			}
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			bUseQaEnvironment = false;
			gl_required_major = 3;
			gl_required_minor = 3;
			SteamLobbyId = 0uL;
			if (e.Args != null && e.Args.Length > 0)
			{
				bool bNextArgIsLanguage = false;
				bool bNextArgIsSteamHostId = false;
				StringBuilder sb = new StringBuilder();
				string[] args4 = e.Args;
				foreach (string arg in args4)
				{
					bool bShouldPassArgToGame = true;
					string argLower = arg.ToLower();
					if (bNextArgIsLanguage)
					{
						SetLanguage(argLower);
						bNextArgIsLanguage = false;
						bShouldPassArgToGame = true;
					}
					else if (bNextArgIsSteamHostId)
					{
						bNextArgIsSteamHostId = false;
						bShouldPassArgToGame = true;
						ulong value3;
						if (ulong.TryParse(argLower, out value3))
						{
							SteamLobbyId = value3;
						}
					}
					else
					{
						switch (argLower)
						{
						case "-test":
						case "test":
							bIsTest = true;
							bShouldPassArgToGame = false;
							break;
						case "-qa":
						case "qa":
							bUseQaEnvironment = true;
							bShouldPassArgToGame = false;
							break;
						case "-prod":
						case "prod":
							bUseQaEnvironment = false;
							bShouldPassArgToGame = false;
							break;
						default:
							if (argLower.StartsWith("-tmsurl="))
							{
								string[] args3 = arg.Split(new char[1] { '=' }, 2);
								base.Properties["TMSOverride"] = args3[1];
							}
							else if (argLower.StartsWith("-locale") || argLower.StartsWith("/locale"))
							{
								bNextArgIsLanguage = true;
								bShouldPassArgToGame = true;
							}
							else if (argLower == "+connect_lobby")
							{
								bSkipLauncher = true;
								bNextArgIsSteamHostId = true;
								bShouldPassArgToGame = true;
							}
							else if (argLower.StartsWith("-gl_min="))
							{
								string[] args2 = arg.Substring(8).Split(new char[1] { '.' }, 2);
								if (args2.Length > 1)
								{
									int value2;
									if (int.TryParse(args2[0], out value2))
									{
										gl_required_major = value2;
									}
									if (int.TryParse(args2[1], out value2))
									{
										gl_required_minor = value2;
									}
								}
							}
							else
							{
								if (!argLower.StartsWith("-amd_min="))
								{
									break;
								}
								string[] args = arg.Substring(9).Split(new char[1] { '.' }, 2);
								if (args.Length > 1)
								{
									int value;
									if (int.TryParse(args[0], out value))
									{
										AMD_min_major = value;
									}
									if (int.TryParse(args[1], out value))
									{
										AMD_min_minor = value;
									}
								}
							}
							break;
						}
					}
					if (bShouldPassArgToGame)
					{
						sb.Append(arg);
						sb.Append(' ');
					}
				}
				base.Properties["StartupArguments"] = sb.ToString().Trim();
			}
			base.OnStartup(e);
		}

		public static bool ApplicationIsActivated()
		{
			IntPtr activatedHandle = GetForegroundWindow();
			if (activatedHandle == IntPtr.Zero)
			{
				return false;
			}
			int procId = Process.GetCurrentProcess().Id;
			int activeProcId;
			GetWindowThreadProcessId(activatedHandle, out activeProcId);
			return activeProcId == procId;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public void InitializeComponent()
		{
			if (!_contentLoaded)
			{
				_contentLoaded = true;
				base.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
				Uri resourceLocater = new Uri("/Launcher;component/app.xaml", UriKind.Relative);
				Application.LoadComponent(this, resourceLocater);
			}
		}

		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[STAThread]
		[DebuggerNonUserCode]
		public static void Main()
		{
			App app = new App();
			app.InitializeComponent();
			app.Run();
		}
	}
}
