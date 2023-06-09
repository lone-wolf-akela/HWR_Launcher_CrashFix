namespace Spark
{
	internal class ICSparkInterfaceBase
	{
		internal class SparkEntitlement
		{
			internal string Identifier;

			internal string Payload;

			internal int UserEntitlementId;

			internal bool Consumable;

			internal int Consumed;

			internal int ConsumableAmount;

			internal string TitleLocalized;

			internal string DescriptionLocalized;
		}

		internal enum eSparkStateMachine
		{
			SPARK_STATE_SteamGetAuthSessionTicket,
			SPARK_STATE_SparkSendAuthRequest,
			SPARK_STATE_SparkWaitForAuthResponse,
			SPARK_STATE_SparkSendVerifyRequest,
			SPARK_STATE_SparkWaitForAuthVerifyToken,
			SPARK_STATE_SparkSend_EULA_Request,
			SPARK_STATE_SparkWaitFor_EULA_Response,
			SPARK_STATE_SparkSendEULASignRequest,
			SPARK_STATE_SparkWaitForEULASignResponse,
			SPARK_STATE_SparkSendSignUpRequest,
			SPARK_STATE_SparkWaitForSignUpResponse,
			SPARK_STATE_SparkSendSignInRequest,
			SPARK_STATE_SparkWaitForSignInResponse,
			SPARK_STATE_SparkSendResetPasswordRequest,
			SPARK_STATE_SparkWaitForResetPasswordResponse,
			SPARK_STATE_SparkSendCodeRedeemRequest,
			SPARK_STATE_SparkWaitForCodeRedeemResponse,
			SPARK_STATE_SparkVerified,
			SPARK_STATE_SteamFailedRetryLater,
			SPARK_STATE_FailedGiveUp
		}
	}
}
