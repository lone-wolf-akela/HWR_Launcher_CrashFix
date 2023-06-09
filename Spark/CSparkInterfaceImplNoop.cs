namespace Spark
{
	internal class CSparkInterfaceImplNoop : ICSparkInterfaceBase
	{
		internal static eSparkStateMachine GetStateMachineState()
		{
			return eSparkStateMachine.SPARK_STATE_FailedGiveUp;
		}

		internal static string GetAccountUID()
		{
			return "";
		}

		internal static void SignUpGearboxAccount(string email, string password, string confirm_password, string current_age)
		{
		}

		internal static void ThreadPoolCallback(object threadContext)
		{
		}

		internal static int GetEULACount()
		{
			return 0;
		}

		internal static string GetEULAString(int index)
		{
			return "";
		}

		internal static void SignEULAs()
		{
		}

		internal static bool HasAgreementsToSign()
		{
			return false;
		}

		internal static bool WasErrorDuringSignIn()
		{
			return false;
		}

		internal static string GetErrorMessage()
		{
			return "";
		}

		internal static void SendResetPassword(string email)
		{
		}

		internal static void SignInGearboxAccount(string email, string password)
		{
		}

		internal static bool IsRedeemedCodeValid()
		{
			return false;
		}

		internal static bool HasShiftAccount()
		{
			return false;
		}

		internal static bool SendCodeRedeem()
		{
			return false;
		}
	}
}
