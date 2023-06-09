using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using HTMLConverter;
using ProtoBuf;
using Spark;
using Steamworks;

namespace Launcher
{
	public class MainWindow : Window, INotifyPropertyChanged, IComponentConnector, IStyleConnector
	{
		private enum eGridState
		{
			GRID_State_Invalid,
			GRID_State_Launcher,
			GRID_State_WaitingForShift,
			GRID_State_EULA,
			GRID_State_SignUp,
			GRID_State_SignUpWaiting,
			GRID_State_SignIn,
			GRID_State_SignInResetWaiting,
			GRID_State_SignInWaiting,
			GRID_State_Redeem,
			GRID_State_Rewards
		}

		private enum eSignUpModelMessage
		{
			MODAL_SignUp_InvalidDate,
			MODAL_SignUp_InvalidAge,
			MODAL_SignUp_InvalidEmail,
			MODAL_SignUp_InvalidPassword,
			MODAL_SignUp_MismatchPassword,
			MODAL_SignUp_WaitingForSignInResponse,
			MODAL_SignUp_ErrorEmailTaken,
			MODAL_SignUp_ErrorPasswordNotSet,
			MODAL_SignUp_ErrorPasswordTooShort,
			MODAL_SignUp_ErrorPasswordTooLong,
			MODAL_SignUp_ErrorPasswordInvalid,
			MODAL_SignUp_ErrorEmailNotSet,
			MODAL_SignUp_ErrorEmailNotValid,
			MODAL_SignUp_ErrorPlatformTaken,
			MODAL_SignUp_ErrorSessionTimeout,
			MODAL_SignUp_ErrorGenericFailure,
			MODAL_SignIn_WaitingForSignInResponse,
			MODAL_SignIn_ErrorSignInFailure,
			MODAL_SignIn_ErrorGenericFailure,
			MODAL_ResetPassword_ErrorFailure,
			MODAL_ResetPassword_WaitingForResponse,
			MODAL_ResetPassword_Success,
			MODAL_CodeRedeem_Failure,
			MODAL_SignUp_ConnectionFailed
		}

		private const float MusicVolume = 0.1f;

		private const string Homeworld1ClassicBasePath = "\\..\\Homeworld1Classic\\";

		private const string Homeworld2ClassicBasePath = "\\..\\Homeworld2Classic\\";

		private const string Homeworld2HDBasePath = "\\..\\HomeworldRM\\";

		public static byte[] buffer;

		public static byte[] Description_buffer;

		public static byte[] DriverAssemblyVersion_buffer;

		public static string gl_version;

		public static bool bIsGLVersionValid;

		public static int gl_version_major;

		public static int gl_version_minor;

		private static Callback<GameLobbyJoinRequested_t> m_SteamLobbyJoinRequested;

		private static Callback<LobbyDataUpdate_t> m_CallbackLobbyDataUpdated;

		protected Grid m_PreviousScreen;

		private List<IniFile> IniFiles;

		private List<IniFile> InstalledIniFiles;

		private ResourceManager resourceManager;

		private readonly MediaPlayer MusicPlayer;

		private readonly SoundEffect ButtonClickSound;

		private readonly SoundEffect MouseOverSound;

		private CSparkInterface SparkInterface;

		private CSteamInterface SteamInterface;

		private System.Threading.Timer SteamCallbackTimer;

		private CSteamID SteamID;

		private string[] HomeworldClassicInstallFiles = new string[2] { "\\..\\Homeworld1Classic\\exe\\Homeworld.exe", "\\..\\Homeworld1Classic\\Data\\homeworld.big" };

		private string[] Homeworld2BaseInstallFiles = new string[3] { "\\..\\HomeworldRM\\bin\\Release\\HomeworldRM.exe", "\\..\\HomeworldRM\\Data\\Homeworld2.big", "\\..\\HomeworldRM\\Data\\Music.big" };

		private string[] HomeworldHDInstallFiles = new string[1] { "\\..\\HomeworldRM\\Data\\HW1Campaign.big" };

		private string[] Homeworld2ClassicInstallFiles = new string[2] { "\\..\\Homeworld2Classic\\bin\\Release\\Homeworld2.exe", "\\..\\Homeworld2Classic\\Data\\Homeworld2.big" };

		private string[] Homeworld2HDInstallFiles = new string[1] { "\\..\\HomeworldRM\\Data\\HW2Campaign.big" };

		private bool bHWClassicInstalled;

		private bool bHWHDInstalled;

		private bool bHW2ClassicInstalled;

		private bool bHW2HDInstalled;

		private bool bHWMPInstalled;

		private bool bForceVideoDefaults;

		private ProcessStartInfo _processStartInfo;

		private static string ConnectLobbyServerArgs = "";

		private bool Grid_Btn_RemasteredState;

		private bool Grid_Btn_ClassicState;

		private int eula_count;

		private int eula_index;

		private bool EULA_IsInitializing;

		private string currentAgeString;

		private string emailText;

		private string passwordText;

		private string confirmPasswordText;

		private eGridState GridState;

		private eGridState ModalSwitchToState;

		private bool bHasShiftAccountButNeedsSignIn;

		private bool bWasRedeemButtonClicked;

		private bool bWasRewardsButtonClicked;

		private bool bWaitingForRedeemCodeResponse;

		private List<CWorkshopItem> WorkshopItems;

		private List<CWorkshopItem> AvailableWorkshopItems;

		private List<CWorkshopItem> SelectedWorkshopItems;

		private EGameType AvailableWorkshopItemsGameType;

		private string CmdLineArgs;

		public static MainWindow MyMainWindow;

		public static string ThreeLetterLanguageCode;

		private LauncherSettings _mySettings;

		public static IniFile ToggleOptionsLocFile;

		public static IniFile MenuNamesLocFile;

		private ObservableCollection<Setting> _configSettings;

		internal MainWindow wndMain;

		internal Grid grdAlwaysOn;

		internal System.Windows.Controls.Label ShiftUsername;

		internal ToggleButton Btn_Remastered;

		internal ToggleButton Btn_Classic;

		internal System.Windows.Controls.Label Label_QA;

		internal System.Windows.Controls.Label Label_TEST;

		internal System.Windows.Controls.Button Btn_MODS;

		internal ToggleButton Btn_Sound;

		internal System.Windows.Controls.Button Btn_Redeem;

		internal System.Windows.Controls.Button Btn_Rewards;

		internal System.Windows.Controls.Button Btn_SignIn;

		internal System.Windows.Controls.Button Btn_SignOut;

		internal System.Windows.Controls.Button Btn_SignUp;

		internal Grid grdLauncher;

		internal System.Windows.Controls.Button Btn_HWRM_1;

		internal System.Windows.Controls.Button Btn_HWRM_2;

		internal Viewbox ShiftRightColumnViewbox;

		internal Image AngelMoonLogo;

		internal System.Windows.Controls.Button Btn_Multiplayer;

		internal Grid SparkNewsGrid;

		internal TextBlock ___TextBlock___SELECTTOPLAY_Copy3;

		internal TextBlock _TextblockConnectingToShift;

		internal System.Windows.Controls.Button Btn_SHiFT_LinkBtn;

		internal System.Windows.Controls.Button Btn_HW_1;

		internal System.Windows.Controls.Button Btn_HW_2;

		internal TextBlock ___TextBlock___SELECTTOPLAY_Copy2;

		internal TextBlock ___TextBlock___CLASSIC_;

		internal TextBlock ___TextBlock___SELECTTOPLAY;

		internal TextBlock ___TextBlock___SELECTTOPLAY_Copy;

		internal TextBlock ___TextBlock___SELECTTOPLAY_Copy1;

		internal TextBlock LegalLabel_Copy;

		internal StackPanel ShiftLegalStackPanel_Copy;

		internal System.Windows.Controls.Label LegalLabel_Copy3;

		internal TextBlock HW1RMDisabledText;

		internal TextBlock HW2RMDisabledText;

		internal Grid grdWaiting;

		internal System.Windows.Controls.Label Label_Waiting;

		internal System.Windows.Controls.Button Btn_SHiFT_SignInWaiting_Back;

		internal Grid grdMODs;

		internal ToggleButton Btn_MOD_HW2C;

		internal ToggleButton Btn_MOD_Remastered;

		internal System.Windows.Controls.CheckBox HW1CampaignCheckbox;

		internal System.Windows.Controls.CheckBox HW2CampaignCheckbox;

		internal System.Windows.Controls.ListView lvDataBindingAvailableMods;

		internal Image ImagePreviewMOD;

		internal System.Windows.Controls.ListView lvDataBindingSelectedMods;

		internal System.Windows.Controls.TextBox TextBox_CmdLine;

		internal System.Windows.Controls.Button Btn_MOD_Back;

		internal System.Windows.Controls.Button Btn_MOD_Select;

		internal System.Windows.Controls.Button Btn_MOD_Launch;

		internal System.Windows.Controls.Button Btn_MOD_Remove;

		internal Grid grdModNotSelected;

		internal TextBlock TextModNotSelected;

		internal System.Windows.Controls.Button Btn_MODNotSelected_Ok;

		internal StackPanel ModalPanel;

		internal System.Windows.Controls.Label Label_ModalDateInvalidCaption;

		internal TextBlock TextBlock_ModalDateInvalid;

		internal System.Windows.Controls.Label Label_ModalAgeInvalidCaption;

		internal TextBlock TextBlock_ModalAgeInvalid;

		internal System.Windows.Controls.Label Label_ModalEmailInvalidCaption;

		internal TextBlock TextBlock_ModalEmailInvalid;

		internal System.Windows.Controls.Label Label_ModalPasswordInvalidCaption;

		internal TextBlock TextBlock_ModalPasswordInvalid;

		internal System.Windows.Controls.Label Label_ModalPasswordMismatchCaption;

		internal TextBlock TextBlock_ModalPasswordMismatch;

		internal System.Windows.Controls.Label Label_ModalErrorEmailTakenCaption;

		internal TextBlock TextBlock_ModalErrorEmailTaken;

		internal System.Windows.Controls.Label Label_ModalErrorPasswordNotSetCaption;

		internal TextBlock TextBlock_ModalErrorPasswordNotSet;

		internal System.Windows.Controls.Label Label_ModalErrorPasswordTooShortCaption;

		internal TextBlock TextBlock_ModalErrorPasswordTooShort;

		internal System.Windows.Controls.Label Label_ModalErrorPasswordTooLongCaption;

		internal TextBlock TextBlock_ModalErrorPasswordTooLong;

		internal System.Windows.Controls.Label Label_ModalErrorPasswordInvalidCaption;

		internal TextBlock TextBlock_ModalErrorPasswordInvalid;

		internal System.Windows.Controls.Label Label_ModalErrorEmailNotSetCaption;

		internal TextBlock TextBlock_ModalErrorEmailNotSet;

		internal System.Windows.Controls.Label Label_ModalErrorEmailNotValidCaption;

		internal TextBlock TextBlock_ModalErrorEmailNotValid;

		internal System.Windows.Controls.Label Label_ModalErrorPlatformTakenCaption;

		internal TextBlock TextBlock_ModalErrorPlatformTaken;

		internal System.Windows.Controls.Label Label_ModalErrorSessionTimeoutCaption;

		internal TextBlock TextBlock_ModalErrorSessionTimeout;

		internal System.Windows.Controls.Label Label_ModalErrorGenericFailureCaption;

		internal TextBlock TextBlock_ModalErrorGenericFailure;

		internal System.Windows.Controls.Label Label_ModalErrorConnectionFailedCaption;

		internal TextBlock TextBlock_ModalErrorConnectionFailed;

		internal System.Windows.Controls.Label Label_ModalErrorSignInFailureCaption;

		internal TextBlock TextBlock_ModalErrorSignInFailure;

		internal System.Windows.Controls.Label Label_ModalErrorGenericSignInFailureCaption;

		internal TextBlock TextBlock_ModalErrorGenericSignInFailure;

		internal System.Windows.Controls.Label Label_ModalResetPasswordFailureCaption;

		internal TextBlock TextBlock_ModalResetPasswordFailure;

		internal System.Windows.Controls.Label Label_ModalResetPasswordSuccessCaption;

		internal TextBlock TextBlock_ModalResetPasswordSuccess;

		internal System.Windows.Controls.Label Label_ModalCodeRedeemFailureCaption;

		internal TextBlock TextBlock_ModalCodeRedeemFailure;

		internal System.Windows.Controls.Label Label_ModalSigningUpCaption;

		internal System.Windows.Controls.Label Label_ModalSigningInCaption;

		internal System.Windows.Controls.Label Label_ModalResetPasswordWaitingCaption;

		internal System.Windows.Controls.Button Btn_Modal_OK;

		internal Grid grdShiftScreens;

		internal Image shiftLogo;

		internal Grid grdMPB_Introduction;

		internal System.Windows.Controls.Button Btn_Shift_MPBIntroduction_Back;

		internal System.Windows.Controls.Button Btn_Shift_MPBIntroduction_Continue;

		internal TextBlock TextWelcome1;

		internal TextBlock TextWelcome2;

		internal TextBlock TextWelcome3;

		internal Grid grdAgeGate;

		internal System.Windows.Controls.Button Btn_VerifyDOBBackButton;

		internal TextBlock AgeGate_Header;

		internal StackPanel MonthPanel;

		internal System.Windows.Controls.TextBox TextBox_AgeGateMonth;

		internal StackPanel DayPanel;

		internal System.Windows.Controls.TextBox TextBox_AgeGateDay;

		internal StackPanel YearPanel;

		internal System.Windows.Controls.TextBox TextBox_AgeGateYear;

		internal System.Windows.Controls.Button Btn_VerifyDOBButton;

		internal Grid grdRedeemCode;

		internal System.Windows.Controls.Label Label_EnterShiftCode;

		internal System.Windows.Controls.TextBox TextBox_EnterCode_One;

		internal System.Windows.Controls.TextBox TextBox_EnterCode_Two;

		internal System.Windows.Controls.TextBox TextBox_EnterCode_Three;

		internal System.Windows.Controls.TextBox TextBox_EnterCode_Four;

		internal System.Windows.Controls.TextBox TextBox_EnterCode_Five;

		internal System.Windows.Controls.Button Btn_RedeemCodeSubmit;

		internal System.Windows.Controls.Button Btn_RedeemCodeBack;

		internal Grid grdSignUp;

		internal System.Windows.Controls.Label Label_SHiFT_SignUp_First;

		internal System.Windows.Controls.Label Label_SHiFT_SignUp_Second;

		internal System.Windows.Controls.TextBox TextBox_SignUp_Email;

		internal PasswordBox TextBox_SignUp_Password;

		internal PasswordBox TextBox_SignUp_ConfirmPassword;

		internal System.Windows.Controls.Button Btn_SHiFT_SignUp;

		internal System.Windows.Controls.Button Btn_SHiFT_SignUp_Back;

		internal Grid grdEULA;

		internal System.Windows.Controls.Label Label_EULA;

		internal System.Windows.Controls.RichTextBox TextBox_EULA;

		internal System.Windows.Controls.Button Btn_EULA_Accept;

		internal System.Windows.Controls.Button Btn_EULA_Decline;

		internal Grid grdSignIn;

		internal System.Windows.Controls.Label Label_SHiFT_SignIn_First;

		internal System.Windows.Controls.Label Label_SHiFT_SignIn_Second;

		internal System.Windows.Controls.TextBox TextBox_SignIn_Email;

		internal PasswordBox TextBox_SignIn_Password;

		internal System.Windows.Controls.Button Btn_SHiFT_SignIn;

		internal System.Windows.Controls.Button Btn_SHiFT_SignIn_Reset_Password;

		internal System.Windows.Controls.Button Btn_SHiFT_SignIn_Back;

		internal Grid grdNeedShift;

		internal System.Windows.Controls.Button Btn_Shift_NeedShift_Back;

		internal System.Windows.Controls.Button Btn_Shift_NeedShift_Continue;

		internal Grid grdEnterBetaCode;

		internal System.Windows.Controls.Label Label_RedeemShiftCode1;

		internal System.Windows.Controls.TextBox TextBox_EnterCode_Three1;

		internal System.Windows.Controls.Button Btn_RedeemBetaCode_Submit;

		internal System.Windows.Controls.Button Btn_RedeemCodeBack1;

		internal Grid grdRedeemInvalid;

		internal TextBlock TextRedeemInvalid;

		internal System.Windows.Controls.Button Btn_RedeemInvalid_Ok;

		internal Grid grdMPB_ThanksScreen;

		internal System.Windows.Controls.Button Btn_Shift_MPBIntroduction_Back1;

		internal TextBlock TextThanks1;

		internal TextBlock TextThanks2;

		internal TextBlock TextThanksOr;

		internal System.Windows.Controls.Button Btn_Shift_MPBIntroduction_Continue1;

		internal TextBlock LegalLabel_Copy1;

		internal StackPanel ShiftLegalStackPanel;

		internal System.Windows.Controls.Label LegalLabel_Copy2;

		internal Grid grdSettings;

		internal System.Windows.Controls.Button Btn_Settings_Back;

		internal System.Windows.Controls.Button Btn_ResetProfile;

		internal System.Windows.Controls.Button Btn_Help;

		internal System.Windows.Controls.Button Btn_Minimize;

		internal System.Windows.Controls.Button Btn_Close;

		internal System.Windows.Controls.Button Btn_Settings;

		internal Grid grdWarning;

		internal System.Windows.Controls.Button Btn_Warning_Back1;

		internal System.Windows.Controls.Button Btn_Warning_Continue;

		internal TextBlock TextOGLWarning;

		internal TextBlock TextOGLRequiredVersion;

		internal TextBlock TextOGLYourWarning;

		internal TextBlock TextOGLYourVersion;

		internal TextBlock TextOGLCrashWarning;

		internal Grid grdConnectLobby;

		internal TextBlock TextConnectLobby;

		private bool _contentLoaded;

		public LauncherSettings MySettings
		{
			get
			{
				return _mySettings;
			}
			set
			{
				if (_mySettings != value)
				{
					_mySettings = value;
					NotifyPropertyChanged("MySettings");
				}
			}
		}

		public DynamicContent DynamicData { get; private set; }

		public ObservableCollection<Setting> ConfigSettings
		{
			get
			{
				return _configSettings;
			}
			set
			{
				if (_configSettings != value)
				{
					_configSettings = value;
					NotifyPropertyChanged("ConfigSettings");
				}
			}
		}

		public bool IsMusicMuted
		{
			get
			{
				if (MySettings == null)
				{
					return false;
				}
				return MySettings.bMuteAudio;
			}
		}

		public bool IsSfxMuted
		{
			get
			{
				return IsMusicMuted;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[DllImport("GL_info.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void GetGLVersion(IntPtr hWnd, [MarshalAs(UnmanagedType.LPArray)] byte[] buffer, int buffer_size);

		public void NotifyPropertyChanged(string PropertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
			}
		}

		public MainWindow()
		{
			MyMainWindow = this;
			ThreeLetterLanguageCode = GetThreeLetterLanguageCode();
			ConfigSettings = new ObservableCollection<Setting>();
			IniFiles = new List<IniFile>();
			InstalledIniFiles = new List<IniFile>();
			DynamicData = new DynamicContent(base.Dispatcher);
			MusicPlayer = new MediaPlayer();
			ButtonClickSound = new SoundEffect(this, 2, "ButtonClick", 0.2f);
			MouseOverSound = new SoundEffect(this, 4, "MouseOver", 0.05f);
			InitializeComponent();
			SteamInterface = new CSteamInterface();
			m_SteamLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnSteamLobbyJoinRequested);
			m_CallbackLobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdatedCallback);
			AutoResetEvent autoEvent = new AutoResetEvent(false);
			SteamCallbackTimer = new System.Threading.Timer(RunSteamCallbacks, autoEvent, 10, 10);
			WorkshopItems = new List<CWorkshopItem>();
			AvailableWorkshopItems = new List<CWorkshopItem>();
			SelectedWorkshopItems = new List<CWorkshopItem>();
			CmdLineArgs = "";
			AvailableWorkshopItemsGameType = EGameType.GAMETYPE_HomeworldRM;
		}

		private void RunSteamCallbacks(object stateInfo)
		{
			SteamAPI.RunCallbacks();
		}

		public void DragWindow(object sender, MouseButtonEventArgs args)
		{
			base.WindowState = WindowState.Normal;
			DragMove();
		}

		public void Generic_MouseEnter(object sender, EventArgs args)
		{
			MouseOverSound.Play();
		}

		public void Generic_ButtonClick(object sender, EventArgs args)
		{
			ButtonClickSound.Play();
		}

		private string GetResourceString(string Key)
		{
			if (resourceManager == null)
			{
				resourceManager = new ResourceManager("Launcher.Properties.Resources", GetType().Assembly);
			}
			return resourceManager.GetString(Key);
		}

		private void btnQuit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void EatMouseDown_Click(object sender, MouseButtonEventArgs e)
		{
			ButtonClickSound.Play();
			e.Handled = true;
		}

		private void btnDynamicData_Click(object sender, MouseButtonEventArgs e)
		{
			FrameworkElement frameworkSender = sender as FrameworkElement;
			object data = ((frameworkSender != null) ? frameworkSender.DataContext : null);
			if (data is NewsStory)
			{
				NewsStory dynamicData = (NewsStory)data;
				Process.Start(dynamicData.Url.ToString());
			}
		}

		private void SetMusicVolume()
		{
			MusicPlayer.Volume = (IsMusicMuted ? 0f : 0.1f);
			bool isMusicMuted = IsMusicMuted;
		}

		private string GetHomeworld2Options(bool bHDVersion)
		{
			return MySettings.GetHW2WindowedParam();
		}

		private string GetHomeworldClassicOptions(bool bHDVersion)
		{
			return MySettings.GetHWWindowedParam();
		}

		private bool Verify_HomeworldBase_Install()
		{
			if (App.bIsTest)
			{
				return true;
			}
			bool bExists = true;
			string[] homeworld2BaseInstallFiles = Homeworld2BaseInstallFiles;
			foreach (string fileName in homeworld2BaseInstallFiles)
			{
				string filePath = Environment.CurrentDirectory + fileName;
				if (!File.Exists(filePath))
				{
					bExists = false;
				}
			}
			return bExists;
		}

		private bool Verify_HomeworldClassic_Install()
		{
			if (App.bIsTest)
			{
				return true;
			}
			bool bExists = true;
			string[] homeworldClassicInstallFiles = HomeworldClassicInstallFiles;
			foreach (string fileName in homeworldClassicInstallFiles)
			{
				string filePath = Environment.CurrentDirectory + fileName;
				if (!File.Exists(filePath))
				{
					bExists = false;
				}
			}
			return bExists;
		}

		private bool Verify_HomeworldHD_Install()
		{
			if (App.bIsTest)
			{
				return true;
			}
			bool bExists = true;
			string[] homeworldHDInstallFiles = HomeworldHDInstallFiles;
			foreach (string fileName in homeworldHDInstallFiles)
			{
				string filePath = Environment.CurrentDirectory + fileName;
				if (!File.Exists(filePath))
				{
					bExists = false;
				}
			}
			return bExists;
		}

		private bool Verify_Homeworld2Classic_Install()
		{
			if (App.bIsTest)
			{
				return true;
			}
			bool bExists = true;
			string[] homeworld2ClassicInstallFiles = Homeworld2ClassicInstallFiles;
			foreach (string fileName in homeworld2ClassicInstallFiles)
			{
				string filePath = Environment.CurrentDirectory + fileName;
				if (!File.Exists(filePath))
				{
					bExists = false;
				}
			}
			return bExists;
		}

		private bool Verify_Homeworld2HD_Install()
		{
			if (App.bIsTest)
			{
				return true;
			}
			bool bExists = true;
			string[] homeworld2HDInstallFiles = Homeworld2HDInstallFiles;
			foreach (string fileName in homeworld2HDInstallFiles)
			{
				string filePath = Environment.CurrentDirectory + fileName;
				if (!File.Exists(filePath))
				{
					bExists = false;
				}
			}
			return bExists;
		}

		private void Init_SparkControls()
		{
			grdShiftScreens.Visibility = Visibility.Collapsed;
			grdNeedShift.Visibility = Visibility.Collapsed;
			grdMPB_Introduction.Visibility = Visibility.Collapsed;
			grdAgeGate.Visibility = Visibility.Collapsed;
			grdRedeemInvalid.Visibility = Visibility.Collapsed;
			Btn_SignUp.Visibility = Visibility.Collapsed;
			Btn_SignOut.Visibility = Visibility.Collapsed;
			Btn_SignIn.Visibility = Visibility.Collapsed;
			Btn_Redeem.Visibility = Visibility.Collapsed;
			if (!(CSparkInterface.GetAccountUID() == ""))
			{
				ShiftUsername.Content = CSparkInterface.GetAccountUID();
			}
		}

		private bool IsDigit(char character)
		{
			if (character >= '0' && character <= '9')
			{
				return true;
			}
			return false;
		}

		private void wndMain_Loaded(object sender, RoutedEventArgs e)
		{
			Window window = Window.GetWindow(this);
			WindowInteropHelper wih = new WindowInteropHelper(window);
			IntPtr hWnd = wih.Handle;
			buffer = new byte[256];
			buffer[0] = 0;
			try
			{
				GetGLVersion(hWnd, buffer, 256);
			}
			catch
			{
			}
			gl_version = "";
			for (int i = 0; i < 256 && buffer[i] != 0; i = checked(i + 1))
			{
				gl_version += Convert.ToChar(buffer[i]);
			}
			bIsGLVersionValid = false;
			if (gl_version != "" && IsDigit(gl_version.Substring(0, 1)[0]) && gl_version.Substring(1, 1)[0] == '.' && IsDigit(gl_version.Substring(2, 1)[0]) && gl_version.Substring(3, 1)[0] == '.' && IsDigit(gl_version.Substring(4, 1)[0]))
			{
				bool bIsValidMajor = int.TryParse(gl_version.Substring(0, 1), out gl_version_major);
				bool bIsValidMinor = int.TryParse(gl_version.Substring(2, 1), out gl_version_minor);
				if (bIsValidMajor && bIsValidMinor)
				{
					bIsGLVersionValid = true;
				}
			}
			Description_buffer = new byte[1024];
			Description_buffer[0] = 0;
			DriverAssemblyVersion_buffer = new byte[1024];
			DriverAssemblyVersion_buffer[0] = 0;
			try
			{
				bHWMPInstalled = Verify_HomeworldBase_Install();
				bHWClassicInstalled = Verify_HomeworldClassic_Install();
				bHWHDInstalled = Verify_HomeworldHD_Install();
				bHW2ClassicInstalled = Verify_Homeworld2Classic_Install();
				bHW2HDInstalled = Verify_Homeworld2HD_Install();
				if (App.bSteamInitialized)
				{
					SteamID = SteamUser.GetSteamID();
					string SteamUserName = SteamFriends.GetPersonaName();
					ShiftUsername.Content = SteamUserName;
					SparkInterface = new CSparkInterface();
					CSteamInterface.GetSubscribedItems(SubscribedItems);
					GridState = eGridState.GRID_State_Launcher;
					ModalSwitchToState = eGridState.GRID_State_Invalid;
					bHasShiftAccountButNeedsSignIn = false;
					bWasRedeemButtonClicked = false;
					bWasRewardsButtonClicked = false;
					ThreadPool.QueueUserWorkItem(CSparkInterface.ThreadPoolCallback);
					if (SteamApps.BIsSubscribedApp(App.BaseHomeworldAppId))
					{
						bool bHWDLCOwned = SteamApps.BIsSubscribedApp(App.HomeworldDlcAppId);
						bool bHW2DLCOwned = SteamApps.BIsSubscribedApp(App.Homeworld2DlcAppId);
						bHWClassicInstalled = bHWDLCOwned && bHWClassicInstalled;
						bHWHDInstalled = bHWDLCOwned && bHWMPInstalled && bHWHDInstalled;
						bHW2ClassicInstalled = bHW2DLCOwned && bHW2ClassicInstalled;
						bHW2HDInstalled = bHW2DLCOwned && bHWMPInstalled && bHW2HDInstalled;
					}
					Init_SparkControls();
				}
				LoadLauncherSettings();
				Btn_Sound.IsChecked = MySettings.bMuteAudio;
				Btn_Multiplayer.IsEnabled = true;
				StartMusic();
				InitConfigSettings();
				LoadConfigs();
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(string.Format("Exception:\n{0}", ex.Message), "Homeworld Launcher", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			InitDynamicContent();
			if (MySettings != null)
			{
				InitializeOptions();
			}
			grdLauncher.Visibility = Visibility.Visible;
			grdWaiting.Visibility = Visibility.Collapsed;
			grdEULA.Visibility = Visibility.Collapsed;
			grdSignUp.Visibility = Visibility.Collapsed;
			grdSignIn.Visibility = Visibility.Collapsed;
			grdRedeemCode.Visibility = Visibility.Collapsed;
			grdMODs.Visibility = Visibility.Collapsed;
			grdModNotSelected.Visibility = Visibility.Collapsed;
			ModalPanel.Visibility = Visibility.Collapsed;
			Label_QA.Visibility = ((!App.bUseQaEnvironment) ? Visibility.Collapsed : Visibility.Visible);
			Label_TEST.Visibility = ((!App.bIsTest) ? Visibility.Collapsed : Visibility.Visible);
			Btn_Remastered.IsChecked = true;
			Btn_Classic.IsChecked = false;
			Btn_HWRM_1.IsEnabled = bHWHDInstalled;
			Btn_HWRM_2.IsEnabled = bHW2HDInstalled;
			if (bHWHDInstalled)
			{
				HW1RMDisabledText.Visibility = Visibility.Collapsed;
			}
			if (bHW2HDInstalled)
			{
				HW2RMDisabledText.Visibility = Visibility.Collapsed;
			}
			if (App.bSkipLauncher)
			{
				if (App.SteamLobbyId != 0)
				{
					grdConnectLobby.Visibility = Visibility.Visible;
				}
				else
				{
					RunMultiplayer(false);
				}
			}
		}

		private void InitializeOptions()
		{
		}

		private void OnSteamLobbyJoinRequested(GameLobbyJoinRequested_t pCallback)
		{
			ConnectLobbyServerArgs += string.Format(" +connect_lobby {0}", pCallback.m_steamIDLobby);
			SteamMatchmaking.RequestLobbyData(pCallback.m_steamIDLobby);
		}

		private void OnLobbyDataUpdatedCallback(LobbyDataUpdate_t pCallback)
		{
			int mod_index = 0;
			bool bDone = false;
			List<string> mod_list = new List<string>();
			checked
			{
				while (!bDone)
				{
					string keyname = string.Format("modfile{0}", mod_index);
					string modname = SteamMatchmaking.GetLobbyData((CSteamID)pCallback.m_ulSteamIDLobby, keyname);
					if (modname == null || modname == "")
					{
						break;
					}
					mod_list.Add(modname);
					mod_index++;
				}
				bool bIsFirstMod = true;
				for (int index = 0; index < mod_list.Count; index++)
				{
					int slash_index = mod_list[index].IndexOf('\\');
					if (slash_index <= 0)
					{
						continue;
					}
					string SteamID_string = mod_list[index].Substring(0, slash_index);
					ulong value;
					if (ulong.TryParse(SteamID_string, out value))
					{
						int len = mod_list[index].Length;
						string mod_folder = mod_list[index].Substring(slash_index + 1, len - slash_index - 1);
						if (bIsFirstMod)
						{
							ConnectLobbyServerArgs += string.Format(" -workshopmod {0}\\{1}", SteamID, mod_folder);
						}
						else
						{
							ConnectLobbyServerArgs += string.Format(",{0}\\{1}", SteamID, mod_folder);
						}
						bIsFirstMod = false;
					}
				}
				MyMainWindow.Dispatcher.BeginInvoke((Action)delegate
				{
					RunMultiplayer(false);
				});
			}
		}

		private void wndMain_ContentRendered(object sender, EventArgs e)
		{
			if (!App.bSteamInitialized)
			{
				System.Windows.Forms.MessageBox.Show("SteamAPI.Init() failed.  Is Steam running?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private void wndMain_Closed(object sender, EventArgs e)
		{
			if (MySettings != null)
			{
				MySettings.Save();
			}
		}

		private void LoadLauncherSettings()
		{
			MySettings = LauncherSettings.Load();
			if (MySettings == null)
			{
				MySettings = new LauncherSettings();
			}
		}

		private void StartMusic()
		{
			MusicPlayer.Open(new Uri("Audio/Music.wav", UriKind.Relative));
			MusicPlayer.MediaEnded += MusicPlayer_MediaEnded;
			SetMusicVolume();
			MusicPlayer.Play();
		}

		private void MusicPlayer_MediaEnded(object sender, EventArgs e)
		{
			MusicPlayer.Position = TimeSpan.Zero;
			MusicPlayer.Play();
		}

		private void InitDynamicContent()
		{
			DynamicData.InitDynamicContent();
		}

		public static string GetThreeLetterLanguageCode(CultureInfo culture = null)
		{
			string Ue3LangExt = "INT";
			string winLangExt = ((culture != null) ? culture.ThreeLetterWindowsLanguageName.ToUpper() : Thread.CurrentThread.CurrentUICulture.ThreeLetterWindowsLanguageName.ToUpper());
			switch (winLangExt)
			{
			case "DEU":
			case "ESN":
			case "FRA":
			case "ITA":
			case "JPN":
			case "RUS":
				Ue3LangExt = winLangExt;
				break;
			}
			return Ue3LangExt;
		}

		private void LoadConfigs(bool bInstalledIniOnly = false)
		{
		}

		private void InitConfigSettings()
		{
			using (Stream fs = Assembly.GetExecutingAssembly().GetManifestResourceStream("Launcher.ConfigData.Settings.w2l"))
			{
				using (GZipStream gs = new GZipStream(fs, CompressionMode.Decompress))
				{
					ConfigSettings = Serializer.Deserialize<ObservableCollection<Setting>>((Stream)(object)gs);
				}
			}
			foreach (Setting setting in ConfigSettings)
			{
				setting.Initialize();
				if (setting.PossibleValues == null)
				{
					throw new Exception("Must set PossibleValues list for each ConfigSetting. Broken setting: " + setting.ConfigKey);
				}
			}
		}

		[Conditional("DEBUG")]
		private void DebugGenerateUncompressedProtobufForSettings<T>(T Settings, string FileName = "export.pb")
		{
			using (FileStream fs = File.Create(FileName))
			{
				Serializer.Serialize((Stream)(object)fs, Settings);
			}
		}

		[Conditional("DEBUG")]
		private void DebugGenerateCompressedProtobufForSettings<T>(T Settings, string FileName = "export.w2l")
		{
			using (FileStream fs = File.Create(FileName))
			{
				using (GZipStream gs = new GZipStream(fs, CompressionMode.Compress))
				{
					Serializer.Serialize((Stream)(object)gs, Settings);
				}
			}
		}

		public IEnumerable<string> ParseQuotedCommaSeparatedValues(string CsvString)
		{
			bool bInEntry = false;
			bool bForceNextCharInString = false;
			StringBuilder nextRetval = null;
			try
			{
				foreach (char c in CsvString)
				{
					if (bInEntry)
					{
						switch (c)
						{
						case '\\':
							bForceNextCharInString = true;
							break;
						case '"':
							if (!bForceNextCharInString)
							{
								if (c == '"')
								{
									yield return nextRetval.ToString();
									bInEntry = false;
								}
								break;
							}
							goto default;
						default:
							nextRetval.Append(c);
							bForceNextCharInString = false;
							break;
						}
					}
					else if (c == '"')
					{
						bInEntry = true;
						nextRetval = new StringBuilder();
					}
				}
			}
			finally
			{
			}
		}

		private static IEnumerable<T> FindChildrenByType<T>(DependencyObject parent) where T : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i = checked(i + 1))
			{
				DependencyObject child = VisualTreeHelper.GetChild(parent, i);
				if (child is T)
				{
					yield return child as T;
				}
				foreach (T grandchild in FindChildrenByType<T>(child))
				{
					if (grandchild != null)
					{
						yield return grandchild;
					}
				}
			}
		}

		public static T FindChildByType<T>(DependencyObject depObj) where T : DependencyObject
		{
			if (depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i = checked(i + 1))
				{
					DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
					if (child != null && child is T)
					{
						return (T)child;
					}
					T childItem = FindChildByType<T>(child);
					if (childItem != null)
					{
						return childItem;
					}
				}
			}
			return null;
		}

		public static void UpdateFocusableButtonList(List<System.Windows.Controls.Button> buttonList, object elem)
		{
			if (elem is System.Windows.Controls.Panel)
			{
				foreach (object child2 in (elem as System.Windows.Controls.Panel).Children)
				{
					if (child2 is System.Windows.Controls.Button && (child2 as System.Windows.Controls.Button).Visibility == Visibility.Visible && (child2 as System.Windows.Controls.Button).IsEnabled)
					{
						buttonList.Add(child2 as System.Windows.Controls.Button);
					}
					else
					{
						UpdateFocusableButtonList(buttonList, child2);
					}
				}
				return;
			}
			if (elem is ContentControl)
			{
				if ((elem as ContentControl).Content != null)
				{
					UpdateFocusableButtonList(buttonList, (elem as ContentControl).Content);
				}
			}
			else
			{
				if (!(elem is ItemsControl))
				{
					return;
				}
				IEnumerable<System.Windows.Controls.Button> btnList = FindChildrenByType<System.Windows.Controls.Button>(elem as ItemsControl);
				foreach (System.Windows.Controls.Button child in btnList)
				{
					if (child != null && child.Visibility == Visibility.Visible && child.IsEnabled)
					{
						buttonList.Add(child);
					}
				}
			}
		}

		private void Btn_HW_1_Click(object sender, RoutedEventArgs e)
		{
			_processStartInfo = new ProcessStartInfo();
			string workingdir = Environment.CurrentDirectory + "\\..\\Homeworld1Classic\\exe\\";
			_processStartInfo.WorkingDirectory = workingdir;
			_processStartInfo.FileName = workingdir + "Homeworld.exe";
			_processStartInfo.Arguments = GetHomeworldClassicOptions(true);
			if (System.Windows.Application.Current.Properties.Count > 0)
			{
				ProcessStartInfo processStartInfo = _processStartInfo;
				processStartInfo.Arguments = processStartInfo.Arguments + " " + System.Windows.Application.Current.Properties["StartupArguments"].ToString();
			}
			if (!File.Exists(_processStartInfo.FileName))
			{
				System.Windows.Forms.MessageBox.Show(string.Format("Executable does not exist:\n{0}", _processStartInfo.FileName), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
			_processStartInfo.CreateNoWindow = true;
			_processStartInfo.UseShellExecute = false;
			if (!ShowWarningDialog())
			{
				Process.Start(_processStartInfo);
				Close();
			}
		}

		private void Btn_HW_2_Click(object sender, RoutedEventArgs e)
		{
			_processStartInfo = new ProcessStartInfo();
			string workingdir = Environment.CurrentDirectory + "\\..\\Homeworld2Classic\\bin\\";
			workingdir = ((!App.bIsTest) ? (workingdir + "Release\\") : (workingdir + "Test\\"));
			_processStartInfo.WorkingDirectory = workingdir;
			_processStartInfo.FileName = workingdir + "Homeworld2.exe";
			_processStartInfo.Arguments = GetHomeworldClassicOptions(true);
			if (System.Windows.Application.Current.Properties.Count > 0)
			{
				ProcessStartInfo processStartInfo = _processStartInfo;
				processStartInfo.Arguments = processStartInfo.Arguments + " " + System.Windows.Application.Current.Properties["StartupArguments"].ToString();
			}
			_processStartInfo.Arguments += " -nopbuffer";
			if (!File.Exists(_processStartInfo.FileName))
			{
				System.Windows.Forms.MessageBox.Show(string.Format("Executable does not exist:\n{0}", _processStartInfo.FileName), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
			_processStartInfo.CreateNoWindow = true;
			_processStartInfo.UseShellExecute = false;
			if (!ShowWarningDialog())
			{
				Process.Start(_processStartInfo);
				Close();
			}
		}

		private void Btn_HWRM_1_Click(object sender, RoutedEventArgs e)
		{
			_processStartInfo = new ProcessStartInfo();
			string workingdir = Environment.CurrentDirectory + "\\..\\HomeworldRM\\bin\\";
			workingdir = ((!App.bIsTest) ? (workingdir + "Release\\") : (workingdir + "Test\\"));
			_processStartInfo.WorkingDirectory = workingdir;
			_processStartInfo.FileName = workingdir + "HomeworldRM.exe";
			_processStartInfo.Arguments = GetHomeworld2Options(true);
			if (System.Windows.Application.Current.Properties.Count > 0)
			{
				ProcessStartInfo processStartInfo = _processStartInfo;
				processStartInfo.Arguments = processStartInfo.Arguments + " " + System.Windows.Application.Current.Properties["StartupArguments"].ToString();
			}
			if (bForceVideoDefaults)
			{
				_processStartInfo.Arguments += " -forceVideoDefaults";
			}
			_processStartInfo.Arguments += " -dlccampaign HW1Campaign.big -campaign HomeworldClassic -moviepath DataHW1Campaign";
			if (!File.Exists(_processStartInfo.FileName))
			{
				System.Windows.Forms.MessageBox.Show(string.Format("Executable does not exist:\n{0}", _processStartInfo.FileName), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
			_processStartInfo.CreateNoWindow = true;
			_processStartInfo.UseShellExecute = false;
			if (!ShowWarningDialog())
			{
				Process.Start(_processStartInfo);
				Close();
			}
		}

		private void Btn_HWRM_2_Click(object sender, RoutedEventArgs e)
		{
			_processStartInfo = new ProcessStartInfo();
			string workingdir = Environment.CurrentDirectory + "\\..\\HomeworldRM\\bin\\";
			workingdir = ((!App.bIsTest) ? (workingdir + "Release\\") : (workingdir + "Test\\"));
			_processStartInfo.WorkingDirectory = workingdir;
			_processStartInfo.FileName = workingdir + "HomeworldRM.exe";
			_processStartInfo.Arguments = GetHomeworld2Options(true);
			if (System.Windows.Application.Current.Properties.Count > 0)
			{
				ProcessStartInfo processStartInfo = _processStartInfo;
				processStartInfo.Arguments = processStartInfo.Arguments + " " + System.Windows.Application.Current.Properties["StartupArguments"].ToString();
			}
			if (bForceVideoDefaults)
			{
				_processStartInfo.Arguments += " -forceVideoDefaults";
			}
			_processStartInfo.Arguments += " -dlccampaign HW2Campaign.big -campaign Ascension -moviepath DataHW2Campaign";
			if (!File.Exists(_processStartInfo.FileName))
			{
				System.Windows.Forms.MessageBox.Show(string.Format("Executable does not exist:\n{0}", _processStartInfo.FileName), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
			_processStartInfo.CreateNoWindow = true;
			_processStartInfo.UseShellExecute = false;
			if (!ShowWarningDialog())
			{
				Process.Start(_processStartInfo);
				Close();
			}
		}

		private void RunMultiplayer(bool bUseSettings)
		{
			_processStartInfo = new ProcessStartInfo();
			string workingdir = Environment.CurrentDirectory + "\\..\\HomeworldRM\\bin\\";
			workingdir = ((!App.bIsTest) ? (workingdir + "Release\\") : (workingdir + "Test\\"));
			_processStartInfo.WorkingDirectory = workingdir;
			_processStartInfo.FileName = workingdir + "HomeworldRM.exe";
			if (System.Windows.Application.Current.Properties.Count > 0)
			{
				ProcessStartInfo processStartInfo = _processStartInfo;
				processStartInfo.Arguments = processStartInfo.Arguments + " " + System.Windows.Application.Current.Properties["StartupArguments"].ToString();
			}
			if (bForceVideoDefaults)
			{
				_processStartInfo.Arguments += " -forceVideoDefaults";
			}
			if (bUseSettings)
			{
				ProcessStartInfo processStartInfo2 = _processStartInfo;
				processStartInfo2.Arguments = processStartInfo2.Arguments + " " + GetHomeworld2Options(true);
			}
			if (ConnectLobbyServerArgs != "")
			{
				_processStartInfo.Arguments += ConnectLobbyServerArgs;
			}
			if (!File.Exists(_processStartInfo.FileName))
			{
				System.Windows.Forms.MessageBox.Show(string.Format("Executable does not exist:\n{0}", _processStartInfo.FileName), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
			_processStartInfo.CreateNoWindow = true;
			_processStartInfo.UseShellExecute = false;
			if (!ShowWarningDialog())
			{
				Process.Start(_processStartInfo);
				Close();
			}
		}

		private void Btn_Multiplayer_Click(object sender, RoutedEventArgs e)
		{
			if (Btn_Classic.IsChecked != true)
			{
				RunMultiplayer(true);
			}
		}

		private void Btn_Remastered_Click(object sender, RoutedEventArgs e)
		{
			Btn_Remastered.IsChecked = true;
			Btn_Classic.IsChecked = false;
			Btn_HW_1.IsEnabled = bHWHDInstalled;
			Btn_HW_2.IsEnabled = bHW2HDInstalled;
			Btn_Multiplayer.IsEnabled = bHWMPInstalled;
		}

		private void Btn_Classic_Click(object sender, RoutedEventArgs e)
		{
			Btn_Classic.IsChecked = true;
			Btn_Remastered.IsChecked = false;
			Btn_HW_1.IsEnabled = bHWClassicInstalled;
			Btn_HW_2.IsEnabled = bHW2ClassicInstalled;
			Btn_Multiplayer.IsEnabled = false;
		}

		private void Btn_Help_Click(object sender, RoutedEventArgs e)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe", "https://gearboxsoftware.zendesk.com/home");
			Process.Start(startInfo);
		}

		private void Btn_SHiFT_Click(object sender, RoutedEventArgs e)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe", "https://shift.gearboxsoftware.com");
			Process.Start(startInfo);
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

		private void Btn_Sound_Click(object sender, RoutedEventArgs e)
		{
			MySettings.bMuteAudio = !MySettings.bMuteAudio;
			SetMusicVolume();
		}

		private void btnMinimize_Click(object sender, RoutedEventArgs e)
		{
			base.WindowState = WindowState.Minimized;
		}

		private void wndMain_StateChanged(object sender, EventArgs e)
		{
			if (base.WindowState == WindowState.Minimized)
			{
				MusicPlayer.Pause();
			}
			else
			{
				MusicPlayer.Play();
			}
		}

		private void wndMain_Activated(object sender, EventArgs e)
		{
			MusicPlayer.Play();
			if (grdEULA.Visibility == Visibility.Visible)
			{
				TextBox_EULA.Focusable = true;
				Keyboard.Focus(TextBox_EULA);
			}
		}

		private void wndMain_Deactivated(object sender, EventArgs e)
		{
			MusicPlayer.Pause();
		}

		private void ShowLauncher_Grid(bool bIsShown)
		{
			grdShiftScreens.Visibility = Visibility.Collapsed;
			grdAgeGate.Visibility = Visibility.Collapsed;
			grdNeedShift.Visibility = Visibility.Collapsed;
			grdWaiting.Visibility = Visibility.Collapsed;
			grdEULA.Visibility = Visibility.Collapsed;
			grdSignUp.Visibility = Visibility.Collapsed;
			grdSignIn.Visibility = Visibility.Collapsed;
			grdRedeemCode.Visibility = Visibility.Collapsed;
			grdMODs.Visibility = Visibility.Collapsed;
			grdSettings.Visibility = Visibility.Collapsed;
			if (!bIsShown && grdLauncher.Visibility != 0)
			{
				return;
			}
			grdLauncher.Visibility = ((!bIsShown) ? Visibility.Collapsed : Visibility.Visible);
			if (!bIsShown)
			{
				if (Btn_Remastered.IsChecked.HasValue)
				{
					Grid_Btn_RemasteredState = Btn_Remastered.IsChecked.Value;
				}
				Btn_Remastered.IsChecked = false;
				if (Btn_Classic.IsChecked.HasValue)
				{
					Grid_Btn_ClassicState = Btn_Classic.IsChecked.Value;
				}
				Btn_Classic.IsChecked = false;
			}
			else
			{
				Btn_Remastered.IsChecked = Grid_Btn_RemasteredState;
				Btn_Classic.IsChecked = Grid_Btn_ClassicState;
			}
			Btn_Remastered.IsEnabled = bIsShown;
			Btn_Classic.IsEnabled = bIsShown;
			Btn_MODS.IsEnabled = bIsShown;
			Btn_Redeem.IsEnabled = bIsShown;
			Btn_Rewards.IsEnabled = bIsShown;
			if (bIsShown)
			{
				GridState = eGridState.GRID_State_Launcher;
				ModalSwitchToState = eGridState.GRID_State_Invalid;
				bWasRedeemButtonClicked = false;
				bWasRewardsButtonClicked = false;
			}
		}

		private void ShowSHiFT_Waiting_Grid()
		{
			ShowLauncher_Grid(false);
			grdWaiting.Visibility = Visibility.Visible;
		}

		private void ShowEULA_Grid()
		{
			ShowLauncher_Grid(false);
			grdShiftScreens.Visibility = Visibility.Visible;
			grdEULA.Visibility = Visibility.Visible;
			grdEULA.IsEnabled = true;
			Btn_EULA_Accept.IsEnabled = true;
			Btn_EULA_Decline.IsEnabled = true;
			TextBox_EULA.Focusable = true;
			Keyboard.Focus(TextBox_EULA);
			ScrollViewer sv = FindChildByType<ScrollViewer>(TextBox_EULA);
			if (sv != null)
			{
				sv.ScrollToTop();
			}
			string eula_text = CSparkInterface.GetEULAString(eula_index);
			if (eula_text != null)
			{
				EULA_DisplayText(eula_text);
			}
		}

		private void ShowSHiFT_SignUp_Grid()
		{
			ShowLauncher_Grid(false);
			grdSignUp.Visibility = Visibility.Visible;
			grdSignUp.IsEnabled = true;
		}

		private void ShowSHiFT_SignIn_Grid()
		{
			ShowLauncher_Grid(false);
			grdSignIn.Visibility = Visibility.Visible;
			grdSignIn.IsEnabled = true;
		}

		private void ShowSHiFT_RedeemCode_Grid(bool bIsShown)
		{
			ShowLauncher_Grid(false);
			grdRedeemCode.Visibility = Visibility.Visible;
		}

		private void HandleGridState()
		{
			ICSparkInterfaceBase.eSparkStateMachine StateMachineState = CSparkInterface.GetStateMachineState();
			if (StateMachineState == ICSparkInterfaceBase.eSparkStateMachine.SPARK_STATE_FailedGiveUp)
			{
				DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_ConnectionFailed);
				return;
			}
			if (GridState == eGridState.GRID_State_Launcher)
			{
				GridState = eGridState.GRID_State_WaitingForShift;
			}
			if (GridState == eGridState.GRID_State_WaitingForShift)
			{
				if (StateMachineState != ICSparkInterfaceBase.eSparkStateMachine.SPARK_STATE_SparkVerified)
				{
					if (grdWaiting.Visibility == Visibility.Collapsed)
					{
						ShowSHiFT_Waiting_Grid();
						return;
					}
				}
				else
				{
					GridState = eGridState.GRID_State_EULA;
				}
			}
			checked
			{
				if (GridState == eGridState.GRID_State_EULA)
				{
					if (CSparkInterface.HasAgreementsToSign())
					{
						if (grdEULA.Visibility == Visibility.Collapsed)
						{
							eula_count = CSparkInterface.GetEULACount();
							eula_index = 0;
							if (eula_count > 0)
							{
								ShowEULA_Grid();
								return;
							}
							GridState = eGridState.GRID_State_SignUp;
						}
						else
						{
							eula_index++;
							if (eula_index < eula_count)
							{
								ShowEULA_Grid();
								return;
							}
							GridState = eGridState.GRID_State_SignUp;
						}
					}
					else
					{
						GridState = eGridState.GRID_State_SignUp;
					}
				}
				if (GridState == eGridState.GRID_State_SignUp)
				{
					if (CSparkInterface.GetAccountUID() == "")
					{
						if (bHasShiftAccountButNeedsSignIn)
						{
							GridState = eGridState.GRID_State_SignIn;
						}
						else if (grdSignUp.Visibility == Visibility.Collapsed)
						{
							ShowSHiFT_SignUp_Grid();
							return;
						}
					}
					else
					{
						GridState = eGridState.GRID_State_SignUpWaiting;
					}
				}
				if (GridState == eGridState.GRID_State_SignUpWaiting)
				{
					if (ModalPanel.Visibility == Visibility.Visible && CSparkInterface.WasErrorDuringSignIn())
					{
						ModalPanel.Visibility = Visibility.Collapsed;
						switch (CSparkInterface.GetErrorMessage())
						{
						case "EMAIL_TAKEN":
							DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_ErrorEmailTaken);
							ModalSwitchToState = eGridState.GRID_State_SignIn;
							return;
						case "PASSWORD_NOT_SET":
							DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_ErrorPasswordNotSet);
							break;
						case "PASSWORD_TOO_SHORT":
							DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_ErrorPasswordTooShort);
							break;
						case "PASSWORD_TOO_LONG":
							DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_ErrorPasswordTooLong);
							break;
						case "PASSWORD_INVALID":
							DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_ErrorPasswordInvalid);
							break;
						case "EMAIL_NOT_SET":
							DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_ErrorEmailNotSet);
							break;
						case "EMAIL_NOT_VALID":
							DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_ErrorEmailNotValid);
							break;
						case "PLATFORM_TAKEN_BY_OTHER":
							DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_ErrorPlatformTaken);
							break;
						case "SESSION_TIMEOUT":
							DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_ErrorSessionTimeout);
							break;
						default:
							DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_ErrorGenericFailure);
							break;
						}
						GridState = eGridState.GRID_State_SignUp;
						return;
					}
					if (CSparkInterface.HasAgreementsToSign() || CSparkInterface.GetAccountUID() == "")
					{
						if (ModalPanel.Visibility == Visibility.Collapsed)
						{
							DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_WaitingForSignInResponse);
						}
						if (StateMachineState == ICSparkInterfaceBase.eSparkStateMachine.SPARK_STATE_SparkVerified)
						{
							if (CSparkInterface.HasAgreementsToSign())
							{
								CSparkInterface.SignEULAs();
								return;
							}
							if (CSparkInterface.GetAccountUID() == "")
							{
								CSparkInterface.SignUpGearboxAccount(emailText, passwordText, confirmPasswordText, currentAgeString);
								return;
							}
						}
					}
					else
					{
						if (ModalPanel.Visibility == Visibility.Visible)
						{
							ModalPanel.Visibility = Visibility.Collapsed;
						}
						if (grdEULA.Visibility == Visibility.Visible)
						{
							grdEULA.Visibility = Visibility.Collapsed;
						}
						if (grdSignUp.Visibility == Visibility.Visible)
						{
							grdSignUp.Visibility = Visibility.Collapsed;
						}
						if (grdSignIn.Visibility == Visibility.Visible)
						{
							grdSignIn.Visibility = Visibility.Collapsed;
						}
						if (bWasRedeemButtonClicked)
						{
							GridState = eGridState.GRID_State_Redeem;
						}
						else if (bWasRewardsButtonClicked)
						{
							GridState = eGridState.GRID_State_Rewards;
						}
					}
				}
				if (GridState == eGridState.GRID_State_SignIn)
				{
					if (CSparkInterface.GetAccountUID() == "")
					{
						if (grdSignIn.Visibility == Visibility.Collapsed)
						{
							bHasShiftAccountButNeedsSignIn = true;
							TextBox_SignIn_Password.Password = "";
							TextBox_SignIn_Email.Text = emailText;
							ShowSHiFT_SignIn_Grid();
							return;
						}
					}
					else
					{
						GridState = eGridState.GRID_State_SignInWaiting;
					}
				}
				if (GridState == eGridState.GRID_State_SignInResetWaiting)
				{
					if (ModalPanel.Visibility == Visibility.Visible && CSparkInterface.WasErrorDuringSignIn())
					{
						ModalPanel.Visibility = Visibility.Collapsed;
						DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_ResetPassword_ErrorFailure);
						ModalSwitchToState = eGridState.GRID_State_SignIn;
						return;
					}
					if (ModalPanel.Visibility != Visibility.Collapsed)
					{
						DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_ResetPassword_Success);
						ModalSwitchToState = eGridState.GRID_State_SignIn;
						return;
					}
					DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_ResetPassword_WaitingForResponse);
					if (StateMachineState == ICSparkInterfaceBase.eSparkStateMachine.SPARK_STATE_SparkVerified)
					{
						CSparkInterface.SendResetPassword(emailText);
						return;
					}
				}
				if (GridState == eGridState.GRID_State_SignInWaiting)
				{
					if (ModalPanel.Visibility == Visibility.Visible && CSparkInterface.WasErrorDuringSignIn())
					{
						ModalPanel.Visibility = Visibility.Collapsed;
						string error_message = CSparkInterface.GetErrorMessage();
						if (error_message == "LOGIN_FAIL")
						{
							DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignIn_ErrorSignInFailure);
							ModalSwitchToState = eGridState.GRID_State_SignIn;
						}
						else
						{
							DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignIn_ErrorGenericFailure);
							GridState = eGridState.GRID_State_SignIn;
						}
						return;
					}
					if (CSparkInterface.GetAccountUID() == "")
					{
						if (ModalPanel.Visibility == Visibility.Collapsed)
						{
							DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignIn_WaitingForSignInResponse);
						}
						if (StateMachineState == ICSparkInterfaceBase.eSparkStateMachine.SPARK_STATE_SparkVerified && CSparkInterface.GetAccountUID() == "")
						{
							CSparkInterface.SignInGearboxAccount(emailText, passwordText);
							return;
						}
					}
					else
					{
						if (ModalPanel.Visibility == Visibility.Visible)
						{
							ModalPanel.Visibility = Visibility.Collapsed;
						}
						if (grdEULA.Visibility == Visibility.Visible)
						{
							grdEULA.Visibility = Visibility.Collapsed;
						}
						if (grdSignUp.Visibility == Visibility.Visible)
						{
							grdSignUp.Visibility = Visibility.Collapsed;
						}
						if (grdSignIn.Visibility == Visibility.Visible)
						{
							grdSignIn.Visibility = Visibility.Collapsed;
						}
						if (bWasRedeemButtonClicked)
						{
							GridState = eGridState.GRID_State_Redeem;
						}
						else if (bWasRewardsButtonClicked)
						{
							GridState = eGridState.GRID_State_Rewards;
						}
					}
				}
				if (GridState == eGridState.GRID_State_Redeem && grdRedeemCode.Visibility == Visibility.Collapsed)
				{
					ShowSHiFT_RedeemCode_Grid(true);
				}
			}
		}

		public void SparkInterfaceStateChanged()
		{
			System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
			{
				MyMainWindow.SparkInterfaceStateChangedInternal();
			}, new object[0]);
		}

		private void SparkInterfaceStateChangedInternal()
		{
			ICSparkInterfaceBase.eSparkStateMachine StateMachineState = CSparkInterface.GetStateMachineState();
			if (StateMachineState == ICSparkInterfaceBase.eSparkStateMachine.SPARK_STATE_SparkVerified)
			{
				Btn_Multiplayer.IsEnabled = true;
				_TextblockConnectingToShift.Visibility = Visibility.Collapsed;
			}
			if (bWaitingForRedeemCodeResponse)
			{
				bWaitingForRedeemCodeResponse = false;
				if (CSparkInterface.IsRedeemedCodeValid())
				{
					grdEnterBetaCode.Visibility = Visibility.Collapsed;
					grdMPB_ThanksScreen.Visibility = Visibility.Visible;
				}
				else
				{
					grdEnterBetaCode.Visibility = Visibility.Collapsed;
					grdRedeemInvalid.Visibility = Visibility.Visible;
				}
			}
			if (GridState != eGridState.GRID_State_Launcher)
			{
				HandleGridState();
			}
		}

		private void Btn_EULA_Accept_Click(object sender, RoutedEventArgs e)
		{
			CSparkInterface.SignEULAs();
			grdEULA.Visibility = Visibility.Collapsed;
			RunMultiplayer(true);
		}

		private void Btn_EULA_Decline_Click(object sender, RoutedEventArgs e)
		{
			ShowLauncher_Grid(true);
		}

		private void EULA_DisplayText(string HTML_Text)
		{
			EULA_IsInitializing = true;
			string xamlString = HtmlToXamlConverter.ConvertHtmlToXaml(HTML_Text, true);
			StringReader stringReader = new StringReader(xamlString);
			XmlReader xmlReader = XmlReader.Create(stringReader);
			FlowDocument doc = (FlowDocument)XamlReader.Load(xmlReader);
			TextBox_EULA.Document.Blocks.Clear();
			TextBox_EULA.Document = doc;
		}

		private bool IsAtBottomOfEULA()
		{
			double offset = TextBox_EULA.VerticalOffset;
			double viewport = TextBox_EULA.ViewportHeight;
			double extent = TextBox_EULA.ExtentHeight;
			if (offset + viewport >= extent)
			{
				return true;
			}
			return false;
		}

		private void EULA_RichTextBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			ScrollViewer sv = FindChildByType<ScrollViewer>(TextBox_EULA);
			if (sv != null && sv.ComputedVerticalScrollBarVisibility == Visibility.Visible && EULA_IsInitializing)
			{
				Btn_EULA_Accept.IsEnabled = false;
				EULA_IsInitializing = false;
			}
			if (IsAtBottomOfEULA())
			{
				Btn_EULA_Accept.IsEnabled = true;
			}
		}

		private void EULA_RichTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			ScrollViewer sv = FindChildByType<ScrollViewer>(TextBox_EULA);
			if (sv != null)
			{
				if (e.Key == Key.Down)
				{
					sv.LineDown();
				}
				else if (e.Key == Key.Up)
				{
					sv.LineUp();
				}
				else if (e.Key == Key.Next)
				{
					sv.PageDown();
				}
				else if (e.Key == Key.Prior)
				{
					sv.PageUp();
				}
			}
		}

		private void EULA_RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			ICollection<TextChange> changed = e.Changes;
			if (changed.Count == 0)
			{
				ScrollViewer sv = FindChildByType<ScrollViewer>(TextBox_EULA);
				if (sv != null && sv.ComputedVerticalScrollBarVisibility == Visibility.Visible && EULA_IsInitializing)
				{
					Btn_EULA_Accept.IsEnabled = false;
					EULA_IsInitializing = false;
				}
			}
		}

		private void DisplaySignUpModalDialog(eSignUpModelMessage message, Grid previousScreen = null)
		{
			grdShiftScreens.Visibility = Visibility.Hidden;
			m_PreviousScreen = previousScreen;
			Label_ModalDateInvalidCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalDateInvalid.Visibility = Visibility.Collapsed;
			Label_ModalAgeInvalidCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalAgeInvalid.Visibility = Visibility.Collapsed;
			Label_ModalEmailInvalidCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalEmailInvalid.Visibility = Visibility.Collapsed;
			Label_ModalPasswordInvalidCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalPasswordInvalid.Visibility = Visibility.Collapsed;
			Label_ModalPasswordMismatchCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalPasswordMismatch.Visibility = Visibility.Collapsed;
			Label_ModalErrorEmailTakenCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalErrorEmailTaken.Visibility = Visibility.Collapsed;
			Label_ModalErrorPasswordNotSetCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalErrorPasswordNotSet.Visibility = Visibility.Collapsed;
			Label_ModalErrorPasswordTooShortCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalErrorPasswordTooShort.Visibility = Visibility.Collapsed;
			Label_ModalErrorPasswordTooLongCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalErrorPasswordTooLong.Visibility = Visibility.Collapsed;
			Label_ModalErrorPasswordInvalidCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalErrorPasswordInvalid.Visibility = Visibility.Collapsed;
			Label_ModalErrorEmailNotSetCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalErrorEmailNotSet.Visibility = Visibility.Collapsed;
			Label_ModalErrorEmailNotValidCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalErrorEmailNotValid.Visibility = Visibility.Collapsed;
			Label_ModalErrorPlatformTakenCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalErrorPlatformTaken.Visibility = Visibility.Collapsed;
			Label_ModalErrorSessionTimeoutCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalErrorSessionTimeout.Visibility = Visibility.Collapsed;
			Label_ModalErrorGenericFailureCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalErrorGenericFailure.Visibility = Visibility.Collapsed;
			Label_ModalErrorConnectionFailedCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalErrorConnectionFailed.Visibility = Visibility.Collapsed;
			Label_ModalErrorSignInFailureCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalErrorSignInFailure.Visibility = Visibility.Collapsed;
			Label_ModalErrorGenericSignInFailureCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalErrorGenericSignInFailure.Visibility = Visibility.Collapsed;
			Label_ModalResetPasswordFailureCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalResetPasswordFailure.Visibility = Visibility.Collapsed;
			Label_ModalResetPasswordSuccessCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalResetPasswordSuccess.Visibility = Visibility.Collapsed;
			Label_ModalCodeRedeemFailureCaption.Visibility = Visibility.Collapsed;
			TextBlock_ModalCodeRedeemFailure.Visibility = Visibility.Collapsed;
			Label_ModalSigningUpCaption.Visibility = Visibility.Collapsed;
			Label_ModalSigningInCaption.Visibility = Visibility.Collapsed;
			Label_ModalResetPasswordWaitingCaption.Visibility = Visibility.Collapsed;
			Btn_Modal_OK.Visibility = Visibility.Visible;
			switch (message)
			{
			case eSignUpModelMessage.MODAL_SignUp_InvalidDate:
				Label_ModalDateInvalidCaption.Visibility = Visibility.Visible;
				TextBlock_ModalDateInvalid.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignUp_InvalidAge:
				Label_ModalAgeInvalidCaption.Visibility = Visibility.Visible;
				TextBlock_ModalAgeInvalid.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignUp_InvalidEmail:
				Label_ModalEmailInvalidCaption.Visibility = Visibility.Visible;
				TextBlock_ModalEmailInvalid.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignUp_InvalidPassword:
				Label_ModalPasswordInvalidCaption.Visibility = Visibility.Visible;
				TextBlock_ModalPasswordInvalid.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignUp_MismatchPassword:
				Label_ModalPasswordMismatchCaption.Visibility = Visibility.Visible;
				TextBlock_ModalPasswordMismatch.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignUp_WaitingForSignInResponse:
				Label_ModalSigningUpCaption.Visibility = Visibility.Visible;
				Btn_Modal_OK.Visibility = Visibility.Collapsed;
				break;
			case eSignUpModelMessage.MODAL_SignUp_ErrorEmailTaken:
				Label_ModalErrorEmailTakenCaption.Visibility = Visibility.Visible;
				TextBlock_ModalErrorEmailTaken.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignUp_ErrorPasswordNotSet:
				Label_ModalErrorPasswordNotSetCaption.Visibility = Visibility.Visible;
				TextBlock_ModalErrorPasswordNotSet.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignUp_ErrorPasswordTooShort:
				Label_ModalErrorPasswordTooShortCaption.Visibility = Visibility.Visible;
				TextBlock_ModalErrorPasswordTooShort.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignUp_ErrorPasswordTooLong:
				Label_ModalErrorPasswordTooLongCaption.Visibility = Visibility.Visible;
				TextBlock_ModalErrorPasswordTooLong.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignUp_ErrorPasswordInvalid:
				Label_ModalErrorPasswordInvalidCaption.Visibility = Visibility.Visible;
				TextBlock_ModalErrorPasswordInvalid.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignUp_ErrorEmailNotSet:
				Label_ModalErrorEmailNotSetCaption.Visibility = Visibility.Visible;
				TextBlock_ModalErrorEmailNotSet.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignUp_ErrorEmailNotValid:
				Label_ModalErrorEmailNotValidCaption.Visibility = Visibility.Visible;
				TextBlock_ModalErrorEmailNotValid.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignUp_ErrorPlatformTaken:
				Label_ModalErrorPlatformTakenCaption.Visibility = Visibility.Visible;
				TextBlock_ModalErrorPlatformTaken.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignUp_ErrorSessionTimeout:
				Label_ModalErrorSessionTimeoutCaption.Visibility = Visibility.Visible;
				TextBlock_ModalErrorSessionTimeout.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignUp_ErrorGenericFailure:
				Label_ModalErrorGenericFailureCaption.Visibility = Visibility.Visible;
				TextBlock_ModalErrorGenericFailure.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignUp_ConnectionFailed:
				Label_ModalErrorConnectionFailedCaption.Visibility = Visibility.Visible;
				TextBlock_ModalErrorConnectionFailed.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignIn_WaitingForSignInResponse:
				Label_ModalSigningInCaption.Visibility = Visibility.Visible;
				Btn_Modal_OK.Visibility = Visibility.Collapsed;
				break;
			case eSignUpModelMessage.MODAL_SignIn_ErrorSignInFailure:
				Label_ModalErrorSignInFailureCaption.Visibility = Visibility.Visible;
				TextBlock_ModalErrorSignInFailure.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_SignIn_ErrorGenericFailure:
				Label_ModalErrorGenericSignInFailureCaption.Visibility = Visibility.Visible;
				TextBlock_ModalErrorGenericSignInFailure.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_ResetPassword_ErrorFailure:
				Label_ModalResetPasswordFailureCaption.Visibility = Visibility.Visible;
				TextBlock_ModalResetPasswordFailure.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_ResetPassword_WaitingForResponse:
				Label_ModalResetPasswordWaitingCaption.Visibility = Visibility.Visible;
				Btn_Modal_OK.Visibility = Visibility.Collapsed;
				break;
			case eSignUpModelMessage.MODAL_ResetPassword_Success:
				Label_ModalResetPasswordSuccessCaption.Visibility = Visibility.Visible;
				TextBlock_ModalResetPasswordSuccess.Visibility = Visibility.Visible;
				break;
			case eSignUpModelMessage.MODAL_CodeRedeem_Failure:
				Label_ModalCodeRedeemFailureCaption.Visibility = Visibility.Visible;
				TextBlock_ModalCodeRedeemFailure.Visibility = Visibility.Visible;
				break;
			}
			ModalPanel.Visibility = Visibility.Visible;
			grdWaiting.IsEnabled = false;
			grdSignUp.IsEnabled = false;
			grdSignIn.IsEnabled = false;
			grdEULA.IsEnabled = false;
		}

		private void Btn_SHiFT_SignUp_Click(object sender, RoutedEventArgs e)
		{
			emailText = TextBox_SignUp_Email.Text;
			if (!emailText.Contains(".") || !emailText.Contains("@"))
			{
				DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_InvalidEmail);
				return;
			}
			passwordText = TextBox_SignUp_Password.Password;
			if (passwordText.Length < 8)
			{
				DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_InvalidPassword);
				return;
			}
			confirmPasswordText = TextBox_SignUp_ConfirmPassword.Password;
			if (passwordText != confirmPasswordText)
			{
				DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_MismatchPassword);
				return;
			}
			GridState = eGridState.GRID_State_SignUpWaiting;
			HandleGridState();
		}

		private void Btn_SHiFT_SignIn_Click(object sender, RoutedEventArgs e)
		{
			emailText = TextBox_SignIn_Email.Text;
			passwordText = TextBox_SignIn_Password.Password;
			if (passwordText.Length < 8)
			{
				DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_InvalidPassword);
				return;
			}
			GridState = eGridState.GRID_State_SignInWaiting;
			HandleGridState();
		}

		private void Btn_SHiFT_SignIn_Reset_Click(object sender, RoutedEventArgs e)
		{
			emailText = TextBox_SignIn_Email.Text;
			if (!emailText.Contains(".") || !emailText.Contains("@"))
			{
				DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_InvalidEmail);
				return;
			}
			GridState = eGridState.GRID_State_SignInResetWaiting;
			HandleGridState();
		}

		private void Btn_SHiFT_Back_Click(object sender, RoutedEventArgs e)
		{
			ShowLauncher_Grid(true);
		}

		private void AgeGatePreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Space)
			{
				e.Handled = true;
			}
		}

		private void AgeGatePreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			int i;
			bool bIsNumeric = int.TryParse(e.Text, out i);
			e.Handled = !bIsNumeric;
		}

		private void AgeGateTextBoxPasting(object sender, DataObjectPastingEventArgs e)
		{
			if (e.DataObject.GetDataPresent(typeof(string)))
			{
				string text = (string)e.DataObject.GetData(typeof(string));
				int i;
				if (!int.TryParse(text, out i))
				{
					e.CancelCommand();
				}
			}
			else
			{
				e.CancelCommand();
			}
		}

		private void PasswordTextBoxPasting(object sender, DataObjectPastingEventArgs e)
		{
			e.CancelCommand();
		}

		private void Btn_RedeemCodeSubmit_Click(object sender, RoutedEventArgs e)
		{
			DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_CodeRedeem_Failure);
		}

		private void Btn_RedeemCodeBack_Click(object sender, RoutedEventArgs e)
		{
			bWaitingForRedeemCodeResponse = false;
			grdEnterBetaCode.Visibility = Visibility.Collapsed;
			ShowLauncher_Grid(true);
		}

		private void EnterCode_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Space)
			{
				e.Handled = true;
			}
		}

		private void EnterCode_TextChanged(object sender, TextChangedEventArgs e)
		{
			System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)sender;
			using (tb.DeclareChangeBlock())
			{
				foreach (TextChange c in e.Changes)
				{
					if (c.AddedLength != 0)
					{
						tb.Select(c.Offset, c.AddedLength);
						string text = tb.SelectedText.ToUpper();
						if (!string.Equals(tb.SelectedText, text, StringComparison.Ordinal))
						{
							tb.SelectedText = text;
						}
						tb.Select(checked(c.Offset + c.AddedLength), 0);
					}
				}
			}
		}

		private void Modal_OnClick(object sender, RoutedEventArgs e)
		{
			ModalPanel.Visibility = Visibility.Collapsed;
			if (m_PreviousScreen != null)
			{
				m_PreviousScreen.Visibility = Visibility.Visible;
			}
			else
			{
				grdLauncher.Visibility = Visibility.Visible;
			}
			eGridState modalSwitchToState = ModalSwitchToState;
		}

		private void Btn_Redeem_Click(object sender, RoutedEventArgs e)
		{
			bWasRedeemButtonClicked = true;
			HandleGridState();
		}

		private void Btn_Rewards_Click(object sender, RoutedEventArgs e)
		{
		}

		private void ShowMODs_Grid()
		{
			ShowLauncher_Grid(false);
			grdMODs.Visibility = Visibility.Visible;
			SelectedWorkshopItems = new List<CWorkshopItem>();
			lvDataBindingSelectedMods.ItemsSource = null;
			Btn_MOD_HW2C.IsChecked = false;
			Btn_MOD_Remastered.IsChecked = true;
		}

		private void Btn_Settings_Click(object sender, RoutedEventArgs e)
		{
			grdSettings.Visibility = Visibility.Visible;
		}

		private void Btn_MODS_Click(object sender, RoutedEventArgs e)
		{
			ShowMODs_Grid();
		}

		private void Btn_MODsBack_Click(object sender, RoutedEventArgs e)
		{
			ShowLauncher_Grid(true);
		}

		private void Btn_MODsSelect_Click(object sender, RoutedEventArgs e)
		{
			int index = lvDataBindingAvailableMods.SelectedIndex;
			if (index < 0 || index >= AvailableWorkshopItems.Count)
			{
				return;
			}
			for (int selected_index = 0; selected_index < SelectedWorkshopItems.Count; selected_index = checked(selected_index + 1))
			{
				if (SelectedWorkshopItems[selected_index].fileId == AvailableWorkshopItems[index].fileId)
				{
					return;
				}
			}
			SelectedWorkshopItems.Add(AvailableWorkshopItems[index]);
			lvDataBindingSelectedMods.ItemsSource = SelectedWorkshopItems;
			ICollectionView view = CollectionViewSource.GetDefaultView(lvDataBindingSelectedMods.ItemsSource);
			view.Refresh();
		}

		private void Btn_MODsRemove_Click(object sender, RoutedEventArgs e)
		{
			int index = lvDataBindingSelectedMods.SelectedIndex;
			if (index >= 0 && index < SelectedWorkshopItems.Count)
			{
				SelectedWorkshopItems.RemoveAt(index);
				lvDataBindingSelectedMods.ItemsSource = SelectedWorkshopItems;
				ICollectionView view = CollectionViewSource.GetDefaultView(lvDataBindingSelectedMods.ItemsSource);
				view.Refresh();
			}
		}

		private void Btn_MODs_HW2C_Click(object sender, RoutedEventArgs e)
		{
			if (AvailableWorkshopItemsGameType != EGameType.GAMETYPE_HW2Classic)
			{
				AvailableWorkshopItemsGameType = EGameType.GAMETYPE_HW2Classic;
				HW1CampaignCheckbox.IsEnabled = false;
				HW2CampaignCheckbox.IsEnabled = false;
				BuildAvailableWorkshopItemsList(AvailableWorkshopItemsGameType);
				Btn_MOD_Remastered.IsChecked = false;
				Image image = ImagePreviewMOD;
				image.Source = null;
			}
			Btn_MOD_HW2C.IsChecked = true;
		}

		private void Btn_MODs_Remastered_Click(object sender, RoutedEventArgs e)
		{
			if (AvailableWorkshopItemsGameType != EGameType.GAMETYPE_HomeworldRM)
			{
				AvailableWorkshopItemsGameType = EGameType.GAMETYPE_HomeworldRM;
				HW1CampaignCheckbox.IsEnabled = true;
				HW2CampaignCheckbox.IsEnabled = true;
				BuildAvailableWorkshopItemsList(AvailableWorkshopItemsGameType);
				Btn_MOD_HW2C.IsChecked = false;
				Image image = ImagePreviewMOD;
				image.Source = null;
			}
			Btn_MOD_Remastered.IsChecked = true;
		}

		private void ChkBox_HW1Campaign_Click(object sender, RoutedEventArgs e)
		{
			HW1CampaignCheckbox.IsChecked = HW1CampaignCheckbox.IsChecked == true;
			HW2CampaignCheckbox.IsChecked = false;
			e.Handled = true;
		}

		private void ChkBox_HW2Campaign_Click(object sender, RoutedEventArgs e)
		{
			HW2CampaignCheckbox.IsChecked = HW2CampaignCheckbox.IsChecked == true;
			HW1CampaignCheckbox.IsChecked = false;
			e.Handled = true;
		}

		private void CmdLine_TextChanged(object sender, TextChangedEventArgs e)
		{
			CmdLineArgs = TextBox_CmdLine.Text;
		}

		private void Btn_MODsLaunch_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedWorkshopItems.Count < 1)
			{
				grdModNotSelected.Visibility = Visibility.Visible;
				grdMODs.Visibility = Visibility.Collapsed;
				return;
			}
			string args = "";
			string workingDir = "";
			string executableFile = "";
			if (Btn_MOD_HW2C.IsChecked == true)
			{
				workingDir = Environment.CurrentDirectory + "\\..\\Homeworld2Classic\\bin\\";
				workingDir = ((!App.bIsTest) ? (workingDir + "Release\\") : (workingDir + "Test\\"));
				executableFile = "Homeworld2.exe";
			}
			else if (Btn_MOD_Remastered.IsChecked == true)
			{
				if (bForceVideoDefaults)
				{
					args += " -forceVideoDefaults";
				}
				if (HW1CampaignCheckbox.IsChecked == true)
				{
					args += " -dlccampaign HW1Campaign.big -campaign HomeworldClassic -moviepath DataHW1Campaign";
				}
				else if (HW2CampaignCheckbox.IsChecked == true)
				{
					args += " -dlccampaign HW2Campaign.big -campaign Ascension -moviepath DataHW2Campaign";
				}
				workingDir = Environment.CurrentDirectory + "\\..\\HomeworldRM\\bin\\";
				workingDir = ((!App.bIsTest) ? (workingDir + "Release\\") : (workingDir + "Test\\"));
				executableFile = "HomeworldRM.exe";
			}
			checked
			{
				for (int j = SelectedWorkshopItems.Count - 1; j >= 0; j--)
				{
					if (SelectedWorkshopItems[j].modType == EModType.MODTYPE_Locale)
					{
						string bigfilename = SelectedWorkshopItems[j].bigFilename;
						if (bigfilename.EndsWith(".big"))
						{
							bigfilename = bigfilename.Substring(0, bigfilename.Length - 4);
						}
						args = args + " -locale " + bigfilename;
						break;
					}
				}
				bool bIsFirstMod = true;
				for (int i = 0; i < SelectedWorkshopItems.Count; i++)
				{
					string modArg = "";
					modArg = ((!bIsFirstMod) ? string.Format(",{0}\\{1}", SelectedWorkshopItems[i].modFolder, SelectedWorkshopItems[i].bigFilename) : string.Format(" -workshopmod {0}\\{1}", SelectedWorkshopItems[i].modFolder, SelectedWorkshopItems[i].bigFilename));
					bIsFirstMod = false;
					args += modArg;
				}
				_processStartInfo = new ProcessStartInfo();
				_processStartInfo.WorkingDirectory = workingDir;
				_processStartInfo.FileName = workingDir + executableFile;
				_processStartInfo.Arguments = args;
				if (System.Windows.Application.Current.Properties.Count > 0)
				{
					ProcessStartInfo processStartInfo = _processStartInfo;
					processStartInfo.Arguments = processStartInfo.Arguments + " " + System.Windows.Application.Current.Properties["StartupArguments"].ToString();
				}
				ProcessStartInfo processStartInfo2 = _processStartInfo;
				processStartInfo2.Arguments = processStartInfo2.Arguments + " " + TextBox_CmdLine.Text;
				if (!File.Exists(_processStartInfo.FileName))
				{
					System.Windows.Forms.MessageBox.Show(string.Format("Executable does not exist:\n{0}", _processStartInfo.FileName), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				_processStartInfo.CreateNoWindow = true;
				_processStartInfo.UseShellExecute = false;
				if (!ShowWarningDialog())
				{
					Process.Start(_processStartInfo);
					Close();
				}
			}
		}

		private void Btn_MODNotSelected_Ok_Click(object sender, RoutedEventArgs e)
		{
			grdMODs.Visibility = Visibility.Visible;
			grdModNotSelected.Visibility = Visibility.Collapsed;
		}

		private void SetAvailableModsPicture()
		{
			int index = lvDataBindingAvailableMods.SelectedIndex;
			if (index >= 0 && index < AvailableWorkshopItems.Count)
			{
				BitmapImage b = new BitmapImage();
				b.BeginInit();
				b.UriSource = new Uri(AvailableWorkshopItems[index].previewFilename);
				b.EndInit();
				Image image = ImagePreviewMOD;
				image.Source = b;
			}
		}

		private void SetSelectedModsPicture()
		{
			int index = lvDataBindingSelectedMods.SelectedIndex;
			if (index >= 0 && index < SelectedWorkshopItems.Count)
			{
				BitmapImage b = new BitmapImage();
				b.BeginInit();
				b.UriSource = new Uri(SelectedWorkshopItems[index].previewFilename);
				b.EndInit();
				Image image2 = ImagePreviewMOD;
				image2.Source = b;
			}
			else
			{
				Image image = ImagePreviewMOD;
				image.Source = null;
			}
		}

		private void lvAvailableMods_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SetAvailableModsPicture();
		}

		private void lvAvailableMods_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			SetAvailableModsPicture();
		}

		private void lvAvailableMods_GotFocus(object sender, RoutedEventArgs e)
		{
			SetAvailableModsPicture();
		}

		private void lvSelectedMods_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SetSelectedModsPicture();
		}

		private void lvSelectedMods_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			SetSelectedModsPicture();
		}

		private void lvSelectedMods_GotFocus(object sender, RoutedEventArgs e)
		{
			SetSelectedModsPicture();
		}

		private void BuildAvailableWorkshopItemsList(EGameType gameType)
		{
			AvailableWorkshopItems = new List<CWorkshopItem>();
			for (int index = 0; index < WorkshopItems.Count; index = checked(index + 1))
			{
				if ((WorkshopItems[index].gameType & gameType) == gameType)
				{
					AvailableWorkshopItems.Add(WorkshopItems[index]);
				}
			}
			lvDataBindingAvailableMods.ItemsSource = AvailableWorkshopItems;
			SelectedWorkshopItems = new List<CWorkshopItem>();
			lvDataBindingSelectedMods.ItemsSource = SelectedWorkshopItems;
		}

		private void SubscribedItems(EResult Result)
		{
			if (CSteamInterface.subscribedItemIndex < CSteamInterface.numSubscribedItems)
			{
				int index4 = CSteamInterface.subscribedItemIndex;
				if (Result == EResult.k_EResultOK)
				{
					WorkshopItems.Add(new CWorkshopItem(SteamID, CSteamInterface.subscribedItemDetails[index4].m_nPublishedFileId, CSteamInterface.subscribedDownloadItems[index4]));
				}
				CSteamInterface.GetNextSubscribedItem(SubscribedItems);
				return;
			}
			string modPathFolder = "";
			List<string> modWorkshopItemFolderNames = new List<string>();
			checked
			{
				for (int loop = 0; loop < 3; loop++)
				{
					switch (loop)
					{
					case 0:
						modPathFolder = CWorkshopItem.GetModPathFolder("Homeworld1Classic", SteamID);
						break;
					case 1:
						modPathFolder = CWorkshopItem.GetModPathFolder("Homeworld2Classic", SteamID);
						break;
					default:
						modPathFolder = CWorkshopItem.GetModPathFolder("HomeworldRM", SteamID);
						break;
					}
					try
					{
						string[] directories = Directory.GetDirectories(modPathFolder);
						for (int index = 0; index < directories.Length; index++)
						{
							string dirName = Path.GetFileName(directories[index].TrimEnd(Path.DirectorySeparatorChar));
							bool bIsOnlyDigits = true;
							for (int char_index = 0; char_index < dirName.Length; char_index++)
							{
								if (dirName[char_index] < '0' || dirName[char_index] > '9')
								{
									bIsOnlyDigits = false;
									break;
								}
							}
							if (bIsOnlyDigits)
							{
								modWorkshopItemFolderNames.Add(directories[index]);
							}
						}
					}
					catch
					{
					}
				}
				for (int index3 = 0; index3 < WorkshopItems.Count; index3++)
				{
					WorkshopItems[index3].ProcessWorkshopItem(ref modWorkshopItemFolderNames);
				}
				if (App.SteamLobbyId != 0)
				{
					if (SteamMatchmaking.RequestLobbyData((CSteamID)App.SteamLobbyId))
					{
						return;
					}
					grdConnectLobby.Visibility = Visibility.Collapsed;
				}
				BuildAvailableWorkshopItemsList(AvailableWorkshopItemsGameType);
				for (int index2 = 0; index2 < modWorkshopItemFolderNames.Count; index2++)
				{
					try
					{
						Directory.Delete(modWorkshopItemFolderNames[index2], true);
					}
					catch
					{
					}
				}
			}
		}

		private void Btn_VerifyDOBButton_Click(object sender, RoutedEventArgs e)
		{
			int month = 0;
			int day = 0;
			int year = 0;
			try
			{
				month = Convert.ToInt32(TextBox_AgeGateMonth.Text);
				day = Convert.ToInt32(TextBox_AgeGateDay.Text);
				year = Convert.ToInt32(TextBox_AgeGateYear.Text);
			}
			catch
			{
			}
			if (month < 1 || month > 12 || day < 1 || day > 31 || year < 1900 || year > 2030)
			{
				DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_InvalidDate, grdShiftScreens);
				return;
			}
			DateTime now = DateTime.Now;
			int CutOffYear = checked(now.Year - 13);
			if (year >= CutOffYear && (year > CutOffYear || (month >= now.Month && (month > now.Month || ((day > now.Day) ? true : false)))))
			{
				DisplaySignUpModalDialog(eSignUpModelMessage.MODAL_SignUp_InvalidAge, grdShiftScreens);
				return;
			}
			currentAgeString = year + "-";
			if (month < 10)
			{
				currentAgeString += "0";
			}
			currentAgeString = currentAgeString + month + "-";
			if (day < 10)
			{
				currentAgeString += "0";
			}
			currentAgeString += day;
			grdAgeGate.Visibility = Visibility.Collapsed;
			ShowEULA_Grid();
		}

		private void Btn_SignUp_Click(object sender, RoutedEventArgs e)
		{
			grdShiftScreens.Visibility = Visibility.Visible;
			grdAgeGate.Visibility = Visibility.Visible;
		}

		private void Btn_MPBeta_Click(object sender, RoutedEventArgs e)
		{
			if (CSparkInterface.HasShiftAccount())
			{
				if (CSparkInterface.HasAgreementsToSign())
				{
					ShowEULA_Grid();
				}
				else
				{
					RunMultiplayer(true);
				}
			}
			else
			{
				RunMultiplayer(true);
			}
		}

		private void Btn_Shift_NeedShift_Continue_Click(object sender, RoutedEventArgs e)
		{
		}

		private void Btn_Shift_NeedShift_Back_Click(object sender, RoutedEventArgs e)
		{
			ShowLauncher_Grid(true);
		}

		private void Btn_Shift_MPBIntroduction_Continue_Click(object sender, RoutedEventArgs e)
		{
			grdMPB_Introduction.Visibility = Visibility.Collapsed;
			if (!CSparkInterface.HasShiftAccount())
			{
				grdAgeGate.Visibility = Visibility.Visible;
			}
			else if (CSparkInterface.HasAgreementsToSign())
			{
				ShowEULA_Grid();
			}
			else
			{
				grdEnterBetaCode.Visibility = Visibility.Visible;
			}
		}

		private void Btn_RedeemBetaCode_Submit_Click(object sender, RoutedEventArgs e)
		{
			if (string.Equals(TextBox_EnterCode_Three1.Text, "BETA", StringComparison.OrdinalIgnoreCase) && !bWaitingForRedeemCodeResponse)
			{
				if (CSparkInterface.SendCodeRedeem())
				{
					bWaitingForRedeemCodeResponse = true;
					return;
				}
				grdEnterBetaCode.Visibility = Visibility.Collapsed;
				grdMPB_ThanksScreen.Visibility = Visibility.Visible;
			}
		}

		private void Btn_RedeemInvalid_Ok_Click(object sender, RoutedEventArgs e)
		{
			grdRedeemInvalid.Visibility = Visibility.Collapsed;
			grdEnterBetaCode.Visibility = Visibility.Visible;
		}

		private void Btn_Sound_Checked(object sender, RoutedEventArgs e)
		{
		}

		private void Btn_Shift_MPBIntroduction_Back1_Click(object sender, RoutedEventArgs e)
		{
			grdMPB_ThanksScreen.Visibility = Visibility.Collapsed;
			ShowLauncher_Grid(true);
		}

		private void Btn_Shift_MPBIntroduction_Continue1_Click(object sender, RoutedEventArgs e)
		{
			RunMultiplayer(true);
		}

		private bool ShowWarningDialog(RoutedEventHandler ButtonClickFunction = null, Grid previousGrid = null)
		{
			if (!bIsGLVersionValid)
			{
				return false;
			}
			int gl_required_major = App.gl_required_major;
			int gl_required_minor = App.gl_required_minor;
			if (gl_version_major > gl_required_major)
			{
				return false;
			}
			if (gl_version_major == gl_required_major && gl_version_minor >= gl_required_minor)
			{
				return false;
			}
			string required_version = string.Format("{0}.{1}", App.gl_required_major, App.gl_required_minor);
			TextOGLRequiredVersion.Text = required_version;
			string your_version = string.Format("{0}.{1}", gl_version_major, gl_version_minor);
			TextOGLYourVersion.Text = your_version;
			grdWarning.Visibility = Visibility.Visible;
			return true;
		}

		private void Btn_Warning_Back1_Click(object sender, RoutedEventArgs e)
		{
			grdWarning.Visibility = Visibility.Collapsed;
		}

		private void Btn_Warning_Continue_Click(object sender, RoutedEventArgs e)
		{
			Process.Start(_processStartInfo);
			Close();
		}

		private void Btn_ResetProfile_Click(object sender, RoutedEventArgs e)
		{
			bForceVideoDefaults = true;
			grdSettings.Visibility = Visibility.Collapsed;
		}

		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[DebuggerNonUserCode]
		public void InitializeComponent()
		{
			if (!_contentLoaded)
			{
				_contentLoaded = true;
				Uri resourceLocater = new Uri("/Launcher;component/mainwindow.xaml", UriKind.Relative);
				System.Windows.Application.LoadComponent(this, resourceLocater);
			}
		}

		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		internal Delegate _CreateDelegate(Type delegateType, string handler)
		{
			return Delegate.CreateDelegate(delegateType, this, handler);
		}

		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				wndMain = (MainWindow)target;
				wndMain.Loaded += wndMain_Loaded;
				wndMain.ContentRendered += wndMain_ContentRendered;
				wndMain.Closed += wndMain_Closed;
				wndMain.StateChanged += wndMain_StateChanged;
				wndMain.Activated += wndMain_Activated;
				wndMain.Deactivated += wndMain_Deactivated;
				break;
			case 2:
				((Grid)target).MouseLeftButtonDown += DragWindow;
				break;
			case 3:
				grdAlwaysOn = (Grid)target;
				break;
			case 4:
				ShiftUsername = (System.Windows.Controls.Label)target;
				break;
			case 5:
				Btn_Remastered = (ToggleButton)target;
				Btn_Remastered.Click += Btn_Remastered_Click;
				break;
			case 6:
				Btn_Classic = (ToggleButton)target;
				Btn_Classic.Click += Btn_Classic_Click;
				break;
			case 7:
				Label_QA = (System.Windows.Controls.Label)target;
				break;
			case 8:
				Label_TEST = (System.Windows.Controls.Label)target;
				break;
			case 9:
				Btn_MODS = (System.Windows.Controls.Button)target;
				Btn_MODS.Click += Btn_MODS_Click;
				break;
			case 10:
				Btn_Sound = (ToggleButton)target;
				Btn_Sound.Click += Btn_Sound_Click;
				Btn_Sound.Checked += Btn_Sound_Checked;
				break;
			case 11:
				Btn_Redeem = (System.Windows.Controls.Button)target;
				Btn_Redeem.Click += Btn_Redeem_Click;
				break;
			case 12:
				Btn_Rewards = (System.Windows.Controls.Button)target;
				Btn_Rewards.Click += Btn_Rewards_Click;
				break;
			case 13:
				Btn_SignIn = (System.Windows.Controls.Button)target;
				Btn_SignIn.Click += Btn_Redeem_Click;
				break;
			case 14:
				Btn_SignOut = (System.Windows.Controls.Button)target;
				Btn_SignOut.Click += Btn_Redeem_Click;
				break;
			case 15:
				Btn_SignUp = (System.Windows.Controls.Button)target;
				Btn_SignUp.Click += Btn_SignUp_Click;
				break;
			case 16:
				grdLauncher = (Grid)target;
				break;
			case 17:
				Btn_HWRM_1 = (System.Windows.Controls.Button)target;
				Btn_HWRM_1.Click += Btn_HWRM_1_Click;
				break;
			case 18:
				Btn_HWRM_2 = (System.Windows.Controls.Button)target;
				Btn_HWRM_2.Click += Btn_HWRM_2_Click;
				break;
			case 19:
				ShiftRightColumnViewbox = (Viewbox)target;
				break;
			case 20:
				AngelMoonLogo = (Image)target;
				break;
			case 21:
				Btn_Multiplayer = (System.Windows.Controls.Button)target;
				Btn_Multiplayer.Click += Btn_MPBeta_Click;
				break;
			case 22:
				SparkNewsGrid = (Grid)target;
				break;
			case 24:
				___TextBlock___SELECTTOPLAY_Copy3 = (TextBlock)target;
				break;
			case 25:
				_TextblockConnectingToShift = (TextBlock)target;
				break;
			case 26:
				Btn_SHiFT_LinkBtn = (System.Windows.Controls.Button)target;
				Btn_SHiFT_LinkBtn.Click += Btn_SHiFT_Click;
				break;
			case 27:
				Btn_HW_1 = (System.Windows.Controls.Button)target;
				Btn_HW_1.Click += Btn_HW_2_Click;
				break;
			case 28:
				Btn_HW_2 = (System.Windows.Controls.Button)target;
				Btn_HW_2.Click += Btn_HW_1_Click;
				break;
			case 29:
				___TextBlock___SELECTTOPLAY_Copy2 = (TextBlock)target;
				break;
			case 30:
				___TextBlock___CLASSIC_ = (TextBlock)target;
				break;
			case 31:
				___TextBlock___SELECTTOPLAY = (TextBlock)target;
				break;
			case 32:
				___TextBlock___SELECTTOPLAY_Copy = (TextBlock)target;
				break;
			case 33:
				___TextBlock___SELECTTOPLAY_Copy1 = (TextBlock)target;
				break;
			case 34:
				LegalLabel_Copy = (TextBlock)target;
				break;
			case 35:
				ShiftLegalStackPanel_Copy = (StackPanel)target;
				break;
			case 36:
				LegalLabel_Copy3 = (System.Windows.Controls.Label)target;
				break;
			case 37:
				((Hyperlink)target).RequestNavigate += Hyperlink_RequestNavigate;
				break;
			case 38:
				((Hyperlink)target).RequestNavigate += Hyperlink_RequestNavigate;
				break;
			case 39:
				((Hyperlink)target).RequestNavigate += Hyperlink_RequestNavigate;
				break;
			case 40:
				HW1RMDisabledText = (TextBlock)target;
				break;
			case 41:
				HW2RMDisabledText = (TextBlock)target;
				break;
			case 42:
				grdWaiting = (Grid)target;
				break;
			case 43:
				Label_Waiting = (System.Windows.Controls.Label)target;
				break;
			case 44:
				Btn_SHiFT_SignInWaiting_Back = (System.Windows.Controls.Button)target;
				Btn_SHiFT_SignInWaiting_Back.Click += Btn_SHiFT_Back_Click;
				break;
			case 45:
				grdMODs = (Grid)target;
				break;
			case 46:
				Btn_MOD_HW2C = (ToggleButton)target;
				Btn_MOD_HW2C.Click += Btn_MODs_HW2C_Click;
				break;
			case 47:
				Btn_MOD_Remastered = (ToggleButton)target;
				Btn_MOD_Remastered.Click += Btn_MODs_Remastered_Click;
				break;
			case 48:
				HW1CampaignCheckbox = (System.Windows.Controls.CheckBox)target;
				HW1CampaignCheckbox.Click += ChkBox_HW1Campaign_Click;
				break;
			case 49:
				HW2CampaignCheckbox = (System.Windows.Controls.CheckBox)target;
				HW2CampaignCheckbox.Click += ChkBox_HW2Campaign_Click;
				break;
			case 50:
				lvDataBindingAvailableMods = (System.Windows.Controls.ListView)target;
				lvDataBindingAvailableMods.SelectionChanged += lvAvailableMods_SelectionChanged;
				lvDataBindingAvailableMods.PreviewMouseDown += lvAvailableMods_PreviewMouseDown;
				lvDataBindingAvailableMods.GotFocus += lvAvailableMods_GotFocus;
				break;
			case 51:
				ImagePreviewMOD = (Image)target;
				break;
			case 52:
				lvDataBindingSelectedMods = (System.Windows.Controls.ListView)target;
				lvDataBindingSelectedMods.SelectionChanged += lvSelectedMods_SelectionChanged;
				lvDataBindingSelectedMods.PreviewMouseDown += lvSelectedMods_PreviewMouseDown;
				lvDataBindingSelectedMods.GotFocus += lvSelectedMods_GotFocus;
				break;
			case 53:
				TextBox_CmdLine = (System.Windows.Controls.TextBox)target;
				TextBox_CmdLine.TextChanged += CmdLine_TextChanged;
				break;
			case 54:
				Btn_MOD_Back = (System.Windows.Controls.Button)target;
				Btn_MOD_Back.Click += Btn_MODsBack_Click;
				break;
			case 55:
				Btn_MOD_Select = (System.Windows.Controls.Button)target;
				Btn_MOD_Select.Click += Btn_MODsSelect_Click;
				break;
			case 56:
				Btn_MOD_Launch = (System.Windows.Controls.Button)target;
				Btn_MOD_Launch.Click += Btn_MODsLaunch_Click;
				break;
			case 57:
				Btn_MOD_Remove = (System.Windows.Controls.Button)target;
				Btn_MOD_Remove.Click += Btn_MODsRemove_Click;
				break;
			case 58:
				grdModNotSelected = (Grid)target;
				break;
			case 59:
				TextModNotSelected = (TextBlock)target;
				break;
			case 60:
				Btn_MODNotSelected_Ok = (System.Windows.Controls.Button)target;
				Btn_MODNotSelected_Ok.Click += Btn_MODNotSelected_Ok_Click;
				break;
			case 61:
				ModalPanel = (StackPanel)target;
				break;
			case 62:
				Label_ModalDateInvalidCaption = (System.Windows.Controls.Label)target;
				break;
			case 63:
				TextBlock_ModalDateInvalid = (TextBlock)target;
				break;
			case 64:
				Label_ModalAgeInvalidCaption = (System.Windows.Controls.Label)target;
				break;
			case 65:
				TextBlock_ModalAgeInvalid = (TextBlock)target;
				break;
			case 66:
				Label_ModalEmailInvalidCaption = (System.Windows.Controls.Label)target;
				break;
			case 67:
				TextBlock_ModalEmailInvalid = (TextBlock)target;
				break;
			case 68:
				Label_ModalPasswordInvalidCaption = (System.Windows.Controls.Label)target;
				break;
			case 69:
				TextBlock_ModalPasswordInvalid = (TextBlock)target;
				break;
			case 70:
				Label_ModalPasswordMismatchCaption = (System.Windows.Controls.Label)target;
				break;
			case 71:
				TextBlock_ModalPasswordMismatch = (TextBlock)target;
				break;
			case 72:
				Label_ModalErrorEmailTakenCaption = (System.Windows.Controls.Label)target;
				break;
			case 73:
				TextBlock_ModalErrorEmailTaken = (TextBlock)target;
				break;
			case 74:
				Label_ModalErrorPasswordNotSetCaption = (System.Windows.Controls.Label)target;
				break;
			case 75:
				TextBlock_ModalErrorPasswordNotSet = (TextBlock)target;
				break;
			case 76:
				Label_ModalErrorPasswordTooShortCaption = (System.Windows.Controls.Label)target;
				break;
			case 77:
				TextBlock_ModalErrorPasswordTooShort = (TextBlock)target;
				break;
			case 78:
				Label_ModalErrorPasswordTooLongCaption = (System.Windows.Controls.Label)target;
				break;
			case 79:
				TextBlock_ModalErrorPasswordTooLong = (TextBlock)target;
				break;
			case 80:
				Label_ModalErrorPasswordInvalidCaption = (System.Windows.Controls.Label)target;
				break;
			case 81:
				TextBlock_ModalErrorPasswordInvalid = (TextBlock)target;
				break;
			case 82:
				Label_ModalErrorEmailNotSetCaption = (System.Windows.Controls.Label)target;
				break;
			case 83:
				TextBlock_ModalErrorEmailNotSet = (TextBlock)target;
				break;
			case 84:
				Label_ModalErrorEmailNotValidCaption = (System.Windows.Controls.Label)target;
				break;
			case 85:
				TextBlock_ModalErrorEmailNotValid = (TextBlock)target;
				break;
			case 86:
				Label_ModalErrorPlatformTakenCaption = (System.Windows.Controls.Label)target;
				break;
			case 87:
				TextBlock_ModalErrorPlatformTaken = (TextBlock)target;
				break;
			case 88:
				Label_ModalErrorSessionTimeoutCaption = (System.Windows.Controls.Label)target;
				break;
			case 89:
				TextBlock_ModalErrorSessionTimeout = (TextBlock)target;
				break;
			case 90:
				Label_ModalErrorGenericFailureCaption = (System.Windows.Controls.Label)target;
				break;
			case 91:
				TextBlock_ModalErrorGenericFailure = (TextBlock)target;
				break;
			case 92:
				Label_ModalErrorConnectionFailedCaption = (System.Windows.Controls.Label)target;
				break;
			case 93:
				TextBlock_ModalErrorConnectionFailed = (TextBlock)target;
				break;
			case 94:
				Label_ModalErrorSignInFailureCaption = (System.Windows.Controls.Label)target;
				break;
			case 95:
				TextBlock_ModalErrorSignInFailure = (TextBlock)target;
				break;
			case 96:
				Label_ModalErrorGenericSignInFailureCaption = (System.Windows.Controls.Label)target;
				break;
			case 97:
				TextBlock_ModalErrorGenericSignInFailure = (TextBlock)target;
				break;
			case 98:
				Label_ModalResetPasswordFailureCaption = (System.Windows.Controls.Label)target;
				break;
			case 99:
				TextBlock_ModalResetPasswordFailure = (TextBlock)target;
				break;
			case 100:
				Label_ModalResetPasswordSuccessCaption = (System.Windows.Controls.Label)target;
				break;
			case 101:
				TextBlock_ModalResetPasswordSuccess = (TextBlock)target;
				break;
			case 102:
				Label_ModalCodeRedeemFailureCaption = (System.Windows.Controls.Label)target;
				break;
			case 103:
				TextBlock_ModalCodeRedeemFailure = (TextBlock)target;
				break;
			case 104:
				Label_ModalSigningUpCaption = (System.Windows.Controls.Label)target;
				break;
			case 105:
				Label_ModalSigningInCaption = (System.Windows.Controls.Label)target;
				break;
			case 106:
				Label_ModalResetPasswordWaitingCaption = (System.Windows.Controls.Label)target;
				break;
			case 107:
				Btn_Modal_OK = (System.Windows.Controls.Button)target;
				Btn_Modal_OK.Click += Modal_OnClick;
				break;
			case 108:
				grdShiftScreens = (Grid)target;
				break;
			case 109:
				shiftLogo = (Image)target;
				break;
			case 110:
				grdMPB_Introduction = (Grid)target;
				break;
			case 111:
				Btn_Shift_MPBIntroduction_Back = (System.Windows.Controls.Button)target;
				Btn_Shift_MPBIntroduction_Back.Click += Btn_Shift_NeedShift_Back_Click;
				break;
			case 112:
				Btn_Shift_MPBIntroduction_Continue = (System.Windows.Controls.Button)target;
				Btn_Shift_MPBIntroduction_Continue.Click += Btn_Shift_MPBIntroduction_Continue_Click;
				break;
			case 113:
				TextWelcome1 = (TextBlock)target;
				break;
			case 114:
				TextWelcome2 = (TextBlock)target;
				break;
			case 115:
				TextWelcome3 = (TextBlock)target;
				break;
			case 116:
				grdAgeGate = (Grid)target;
				break;
			case 117:
				Btn_VerifyDOBBackButton = (System.Windows.Controls.Button)target;
				Btn_VerifyDOBBackButton.Click += Btn_SHiFT_Back_Click;
				break;
			case 118:
				AgeGate_Header = (TextBlock)target;
				break;
			case 119:
				MonthPanel = (StackPanel)target;
				break;
			case 120:
				TextBox_AgeGateMonth = (System.Windows.Controls.TextBox)target;
				TextBox_AgeGateMonth.PreviewKeyDown += AgeGatePreviewKeyDown;
				TextBox_AgeGateMonth.PreviewTextInput += AgeGatePreviewTextInput;
				TextBox_AgeGateMonth.AddHandler(System.Windows.DataObject.PastingEvent, new DataObjectPastingEventHandler(AgeGateTextBoxPasting));
				break;
			case 121:
				DayPanel = (StackPanel)target;
				break;
			case 122:
				TextBox_AgeGateDay = (System.Windows.Controls.TextBox)target;
				TextBox_AgeGateDay.PreviewKeyDown += AgeGatePreviewKeyDown;
				TextBox_AgeGateDay.PreviewTextInput += AgeGatePreviewTextInput;
				TextBox_AgeGateDay.AddHandler(System.Windows.DataObject.PastingEvent, new DataObjectPastingEventHandler(AgeGateTextBoxPasting));
				break;
			case 123:
				YearPanel = (StackPanel)target;
				break;
			case 124:
				TextBox_AgeGateYear = (System.Windows.Controls.TextBox)target;
				TextBox_AgeGateYear.PreviewKeyDown += AgeGatePreviewKeyDown;
				TextBox_AgeGateYear.PreviewTextInput += AgeGatePreviewTextInput;
				TextBox_AgeGateYear.AddHandler(System.Windows.DataObject.PastingEvent, new DataObjectPastingEventHandler(AgeGateTextBoxPasting));
				break;
			case 125:
				Btn_VerifyDOBButton = (System.Windows.Controls.Button)target;
				Btn_VerifyDOBButton.Click += Btn_VerifyDOBButton_Click;
				break;
			case 126:
				grdRedeemCode = (Grid)target;
				break;
			case 127:
				Label_EnterShiftCode = (System.Windows.Controls.Label)target;
				break;
			case 128:
				TextBox_EnterCode_One = (System.Windows.Controls.TextBox)target;
				TextBox_EnterCode_One.PreviewKeyDown += EnterCode_PreviewKeyDown;
				TextBox_EnterCode_One.TextChanged += EnterCode_TextChanged;
				break;
			case 129:
				TextBox_EnterCode_Two = (System.Windows.Controls.TextBox)target;
				TextBox_EnterCode_Two.PreviewKeyDown += EnterCode_PreviewKeyDown;
				TextBox_EnterCode_Two.TextChanged += EnterCode_TextChanged;
				break;
			case 130:
				TextBox_EnterCode_Three = (System.Windows.Controls.TextBox)target;
				TextBox_EnterCode_Three.PreviewKeyDown += EnterCode_PreviewKeyDown;
				TextBox_EnterCode_Three.TextChanged += EnterCode_TextChanged;
				break;
			case 131:
				TextBox_EnterCode_Four = (System.Windows.Controls.TextBox)target;
				TextBox_EnterCode_Four.PreviewKeyDown += EnterCode_PreviewKeyDown;
				TextBox_EnterCode_Four.TextChanged += EnterCode_TextChanged;
				break;
			case 132:
				TextBox_EnterCode_Five = (System.Windows.Controls.TextBox)target;
				TextBox_EnterCode_Five.PreviewKeyDown += EnterCode_PreviewKeyDown;
				TextBox_EnterCode_Five.TextChanged += EnterCode_TextChanged;
				break;
			case 133:
				Btn_RedeemCodeSubmit = (System.Windows.Controls.Button)target;
				Btn_RedeemCodeSubmit.Click += Btn_RedeemCodeSubmit_Click;
				break;
			case 134:
				Btn_RedeemCodeBack = (System.Windows.Controls.Button)target;
				Btn_RedeemCodeBack.Click += Btn_RedeemCodeBack_Click;
				break;
			case 135:
				grdSignUp = (Grid)target;
				break;
			case 136:
				Label_SHiFT_SignUp_First = (System.Windows.Controls.Label)target;
				break;
			case 137:
				Label_SHiFT_SignUp_Second = (System.Windows.Controls.Label)target;
				break;
			case 138:
				TextBox_SignUp_Email = (System.Windows.Controls.TextBox)target;
				break;
			case 139:
				TextBox_SignUp_Password = (PasswordBox)target;
				TextBox_SignUp_Password.AddHandler(System.Windows.DataObject.PastingEvent, new DataObjectPastingEventHandler(PasswordTextBoxPasting));
				break;
			case 140:
				TextBox_SignUp_ConfirmPassword = (PasswordBox)target;
				TextBox_SignUp_ConfirmPassword.AddHandler(System.Windows.DataObject.PastingEvent, new DataObjectPastingEventHandler(PasswordTextBoxPasting));
				break;
			case 141:
				Btn_SHiFT_SignUp = (System.Windows.Controls.Button)target;
				Btn_SHiFT_SignUp.Click += Btn_SHiFT_SignUp_Click;
				break;
			case 142:
				Btn_SHiFT_SignUp_Back = (System.Windows.Controls.Button)target;
				Btn_SHiFT_SignUp_Back.Click += Btn_SHiFT_Back_Click;
				break;
			case 143:
				grdEULA = (Grid)target;
				break;
			case 144:
				Label_EULA = (System.Windows.Controls.Label)target;
				break;
			case 145:
				TextBox_EULA = (System.Windows.Controls.RichTextBox)target;
				TextBox_EULA.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(EULA_RichTextBox_ScrollChanged));
				TextBox_EULA.PreviewKeyDown += EULA_RichTextBox_KeyDown;
				TextBox_EULA.TextChanged += EULA_RichTextBox_TextChanged;
				break;
			case 146:
				Btn_EULA_Accept = (System.Windows.Controls.Button)target;
				Btn_EULA_Accept.Click += Btn_EULA_Accept_Click;
				break;
			case 147:
				Btn_EULA_Decline = (System.Windows.Controls.Button)target;
				Btn_EULA_Decline.Click += Btn_EULA_Decline_Click;
				break;
			case 148:
				grdSignIn = (Grid)target;
				break;
			case 149:
				Label_SHiFT_SignIn_First = (System.Windows.Controls.Label)target;
				break;
			case 150:
				Label_SHiFT_SignIn_Second = (System.Windows.Controls.Label)target;
				break;
			case 151:
				TextBox_SignIn_Email = (System.Windows.Controls.TextBox)target;
				break;
			case 152:
				TextBox_SignIn_Password = (PasswordBox)target;
				TextBox_SignIn_Password.AddHandler(System.Windows.DataObject.PastingEvent, new DataObjectPastingEventHandler(PasswordTextBoxPasting));
				break;
			case 153:
				Btn_SHiFT_SignIn = (System.Windows.Controls.Button)target;
				Btn_SHiFT_SignIn.Click += Btn_SHiFT_SignIn_Click;
				break;
			case 154:
				Btn_SHiFT_SignIn_Reset_Password = (System.Windows.Controls.Button)target;
				Btn_SHiFT_SignIn_Reset_Password.Click += Btn_SHiFT_SignIn_Reset_Click;
				break;
			case 155:
				Btn_SHiFT_SignIn_Back = (System.Windows.Controls.Button)target;
				Btn_SHiFT_SignIn_Back.Click += Btn_SHiFT_Back_Click;
				break;
			case 156:
				grdNeedShift = (Grid)target;
				break;
			case 157:
				Btn_Shift_NeedShift_Back = (System.Windows.Controls.Button)target;
				Btn_Shift_NeedShift_Back.Click += Btn_Shift_NeedShift_Back_Click;
				break;
			case 158:
				Btn_Shift_NeedShift_Continue = (System.Windows.Controls.Button)target;
				Btn_Shift_NeedShift_Continue.Click += Btn_Shift_NeedShift_Continue_Click;
				break;
			case 159:
				grdEnterBetaCode = (Grid)target;
				break;
			case 160:
				Label_RedeemShiftCode1 = (System.Windows.Controls.Label)target;
				break;
			case 161:
				TextBox_EnterCode_Three1 = (System.Windows.Controls.TextBox)target;
				TextBox_EnterCode_Three1.PreviewKeyDown += EnterCode_PreviewKeyDown;
				TextBox_EnterCode_Three1.TextChanged += EnterCode_TextChanged;
				break;
			case 162:
				Btn_RedeemBetaCode_Submit = (System.Windows.Controls.Button)target;
				Btn_RedeemBetaCode_Submit.Click += Btn_RedeemBetaCode_Submit_Click;
				break;
			case 163:
				Btn_RedeemCodeBack1 = (System.Windows.Controls.Button)target;
				Btn_RedeemCodeBack1.Click += Btn_RedeemCodeBack_Click;
				break;
			case 164:
				grdRedeemInvalid = (Grid)target;
				break;
			case 165:
				TextRedeemInvalid = (TextBlock)target;
				break;
			case 166:
				Btn_RedeemInvalid_Ok = (System.Windows.Controls.Button)target;
				Btn_RedeemInvalid_Ok.Click += Btn_RedeemInvalid_Ok_Click;
				break;
			case 167:
				grdMPB_ThanksScreen = (Grid)target;
				break;
			case 168:
				Btn_Shift_MPBIntroduction_Back1 = (System.Windows.Controls.Button)target;
				Btn_Shift_MPBIntroduction_Back1.Click += Btn_Shift_MPBIntroduction_Back1_Click;
				break;
			case 169:
				TextThanks1 = (TextBlock)target;
				break;
			case 170:
				TextThanks2 = (TextBlock)target;
				break;
			case 171:
				TextThanksOr = (TextBlock)target;
				break;
			case 172:
				Btn_Shift_MPBIntroduction_Continue1 = (System.Windows.Controls.Button)target;
				Btn_Shift_MPBIntroduction_Continue1.Click += Btn_Shift_MPBIntroduction_Continue1_Click;
				break;
			case 173:
				LegalLabel_Copy1 = (TextBlock)target;
				break;
			case 174:
				ShiftLegalStackPanel = (StackPanel)target;
				break;
			case 175:
				LegalLabel_Copy2 = (System.Windows.Controls.Label)target;
				break;
			case 176:
				((Hyperlink)target).RequestNavigate += Hyperlink_RequestNavigate;
				break;
			case 177:
				((Hyperlink)target).RequestNavigate += Hyperlink_RequestNavigate;
				break;
			case 178:
				((Hyperlink)target).RequestNavigate += Hyperlink_RequestNavigate;
				break;
			case 179:
				grdSettings = (Grid)target;
				break;
			case 180:
				Btn_Settings_Back = (System.Windows.Controls.Button)target;
				Btn_Settings_Back.Click += Btn_MODsBack_Click;
				break;
			case 181:
				Btn_ResetProfile = (System.Windows.Controls.Button)target;
				Btn_ResetProfile.Click += Btn_ResetProfile_Click;
				break;
			case 182:
				Btn_Help = (System.Windows.Controls.Button)target;
				Btn_Help.Click += Btn_Help_Click;
				break;
			case 183:
				Btn_Minimize = (System.Windows.Controls.Button)target;
				Btn_Minimize.Click += btnMinimize_Click;
				break;
			case 184:
				Btn_Close = (System.Windows.Controls.Button)target;
				Btn_Close.Click += btnQuit_Click;
				break;
			case 185:
				Btn_Settings = (System.Windows.Controls.Button)target;
				Btn_Settings.Click += Btn_Settings_Click;
				break;
			case 186:
				grdWarning = (Grid)target;
				break;
			case 187:
				Btn_Warning_Back1 = (System.Windows.Controls.Button)target;
				Btn_Warning_Back1.Click += Btn_Warning_Back1_Click;
				break;
			case 188:
				Btn_Warning_Continue = (System.Windows.Controls.Button)target;
				Btn_Warning_Continue.Click += Btn_Warning_Continue_Click;
				break;
			case 189:
				TextOGLWarning = (TextBlock)target;
				break;
			case 190:
				TextOGLRequiredVersion = (TextBlock)target;
				break;
			case 191:
				TextOGLYourWarning = (TextBlock)target;
				break;
			case 192:
				TextOGLYourVersion = (TextBlock)target;
				break;
			case 193:
				TextOGLCrashWarning = (TextBlock)target;
				break;
			case 194:
				grdConnectLobby = (Grid)target;
				break;
			case 195:
				TextConnectLobby = (TextBlock)target;
				break;
			default:
				_contentLoaded = true;
				break;
			}
		}

		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void IStyleConnector.Connect(int connectionId, object target)
		{
			if (connectionId == 23)
			{
				((Grid)target).MouseLeftButtonUp += btnDynamicData_Click;
				((Grid)target).PreviewMouseLeftButtonDown += EatMouseDown_Click;
			}
		}
	}
}
