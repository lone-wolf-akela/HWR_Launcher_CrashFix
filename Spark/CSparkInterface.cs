using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Launcher;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steamworks;

namespace Spark
{
	internal sealed class CSparkInterface : ICSparkInterfaceBase
	{
		private const int AUTHSESSIONTICKET_BUFFER_SIZE = 1024;

		private static long PreviousTicks = 0L;

		private static ASCIIEncoding encoding;

		private static CGBXHTTP_Request GBXHTTP_request;

		private static bool bDisableSpark;

		private static string SparkBaseURL;

		private static string AuthSessionTicketString;

		private static string AuthToken;

		private static eSparkStateMachine StateMachineState;

		private static List<SparkEntitlement> entitlements = new List<SparkEntitlement>();

		private static int[] eula_id;

		private static string[] eula_version;

		private static string[] eula_text;

		private static int eula_count;

		private static string signup_email;

		private static string signup_password;

		private static string signup_confirm_password;

		private static string signup_current_age;

		private static string redeem_code;

		private static bool bErrorDuringSignIn;

		private static string error_message_text;

		private static string AccountUID;

		private static bool bHasShiftAccount;

		private static bool bAgreementsToSign;

		private static float BaseRetrySeconds;

		private static int MaxRetryAttempts;

		private static float RetryMultiplier;

		private static float HttpTimeout;

		private static string VerifyURL;

		private static float VerifyFirstAttemptDelay;

		private static float VerifyBaseRetrySeconds;

		private static int VerifyMaxRetryAttempts;

		private static float VerifyRetryMultiplier;

		private static int AuthDelayRemaining;

		private static int AuthRetryCount;

		private static string ArchwayURL;

		private static string LeviathanURL;

		private static string HickoryMP_BETA_SHiFT_Code;

		private static bool bIsRedeemCodeValid;

		private static void BlobToHexString(ref string OutString, ref byte[] InBuffer, uint InBufferSize)
		{
			OutString = "";
			for (int i = 0; i < InBufferSize; i = checked(i + 1))
			{
				OutString += InBuffer[i].ToString("x2");
			}
		}

		private static bool GetObjectFromJsonObject(JObject obj, string name, ref JObject output)
		{
			output = null;
			if (obj[name] != null && obj[name].Type == JTokenType.Object)
			{
				output = obj[name].Value<JObject>();
				return true;
			}
			return false;
		}

		private static bool GetBoolFromJsonObject(JObject obj, string name, ref bool output)
		{
			output = false;
			if (obj[name] != null && obj[name].Type == JTokenType.Boolean)
			{
				output = obj[name].Value<bool>();
				return true;
			}
			return false;
		}

		private static bool GetBoolFromJsonToken(JToken token, string name, ref bool output)
		{
			output = false;
			if (token[name] != null && token[name].Type == JTokenType.Boolean)
			{
				output = token[name].Value<bool>();
				return true;
			}
			return false;
		}

		private static bool GetIntFromJsonObject(JObject obj, string name, ref int output)
		{
			output = 0;
			if (obj[name] != null && obj[name].Type == JTokenType.Integer)
			{
				output = obj[name].Value<int>();
				return true;
			}
			return false;
		}

		private static bool GetIntFromJsonToken(JToken token, string name, ref int output)
		{
			output = 0;
			if (token[name] != null && token[name].Type == JTokenType.Integer)
			{
				output = token[name].Value<int>();
				return true;
			}
			return false;
		}

		private static bool GetStringFromJsonObject(JObject obj, string name, ref string output)
		{
			output = "";
			if (obj[name] != null && obj[name].Type == JTokenType.String)
			{
				output = obj[name].Value<string>();
				return true;
			}
			return false;
		}

		private static bool GetStringFromJsonToken(JToken token, string name, ref string output)
		{
			output = "";
			if (token[name] != null && token[name].Type == JTokenType.String)
			{
				output = token[name].Value<string>();
				return true;
			}
			return false;
		}

		private static void TimeoutCallback(object state, bool timedOut)
		{
			CGBXHTTP_Request GBXHTTP_request = (CGBXHTTP_Request)state;
			if (GBXHTTP_request != null && timedOut)
			{
				GBXHTTP_request.status = CGBXHTTP_Request.GBXHTTP_Status.TimedOut;
				if (GBXHTTP_request.request != null)
				{
					GBXHTTP_request.request.Abort();
				}
			}
		}

		private static void GetRequestStreamCallback(IAsyncResult asynchronousResult)
		{
			CGBXHTTP_Request GBXHTTP_request = (CGBXHTTP_Request)asynchronousResult.AsyncState;
			if (GBXHTTP_request == null || GBXHTTP_request.request == null || GBXHTTP_request.status == CGBXHTTP_Request.GBXHTTP_Status.TimedOut)
			{
				return;
			}
			try
			{
				Stream postStream = GBXHTTP_request.request.EndGetRequestStream(asynchronousResult);
				if (GBXHTTP_request.postData != null)
				{
					try
					{
						postStream.Write(GBXHTTP_request.postData, 0, GBXHTTP_request.postData.Length);
					}
					catch
					{
						postStream.Close();
						GBXHTTP_request.status = CGBXHTTP_Request.GBXHTTP_Status.Failed;
						return;
					}
				}
				postStream.Close();
				GBXHTTP_request.request.BeginGetResponse(GetResponseCallback, GBXHTTP_request);
			}
			catch (Exception)
			{
				GBXHTTP_request.status = CGBXHTTP_Request.GBXHTTP_Status.Failed;
			}
		}

		private static void GetResponseCallback(IAsyncResult asynchronousResult)
		{
			CGBXHTTP_Request GBXHTTP_request = (CGBXHTTP_Request)asynchronousResult.AsyncState;
			if (GBXHTTP_request == null || GBXHTTP_request.request == null)
			{
				return;
			}
			try
			{
				HttpWebResponse response2 = (HttpWebResponse)GBXHTTP_request.request.EndGetResponse(asynchronousResult);
				Stream streamResponse2 = response2.GetResponseStream();
				StreamReader streamRead2 = new StreamReader(streamResponse2);
				GBXHTTP_request.responseData = streamRead2.ReadToEnd();
				streamRead2.Close();
				streamResponse2.Close();
				response2.Close();
				GBXHTTP_request.status = CGBXHTTP_Request.GBXHTTP_Status.Done;
			}
			catch (WebException e)
			{
				HttpWebResponse response = (HttpWebResponse)e.Response;
				if (response != null && (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.NotFound))
				{
					Stream streamResponse = response.GetResponseStream();
					StreamReader streamRead = new StreamReader(streamResponse);
					GBXHTTP_request.responseData = streamRead.ReadToEnd();
					streamRead.Close();
					streamResponse.Close();
					response.Close();
					GBXHTTP_request.status = CGBXHTTP_Request.GBXHTTP_Status.Failed;
				}
				else
				{
					GBXHTTP_request.status = CGBXHTTP_Request.GBXHTTP_Status.Failed;
				}
			}
		}

		internal static void ThreadPoolCallback(object threadContext)
		{
			GBXHTTP_request = null;
			encoding = new ASCIIEncoding();
			bDisableSpark = false;
			SparkBaseURL = "https://account.services.gearboxsoftware.com/";
			if (App.bUseQaEnvironment)
			{
				SparkBaseURL = "https://account-qa.services.gearboxsoftware.com/";
			}
			AuthToken = "";
			StateMachineState = eSparkStateMachine.SPARK_STATE_SteamGetAuthSessionTicket;
			AccountUID = "";
			bHasShiftAccount = false;
			bAgreementsToSign = true;
			bErrorDuringSignIn = false;
			VerifyURL = "";
			ArchwayURL = "";
			LeviathanURL = "";
			HickoryMP_BETA_SHiFT_Code = "";
			BaseRetrySeconds = 30f;
			MaxRetryAttempts = 2;
			RetryMultiplier = 3f;
			HttpTimeout = 30f;
			VerifyFirstAttemptDelay = 3f;
			VerifyBaseRetrySeconds = 4f;
			VerifyMaxRetryAttempts = 20;
			VerifyRetryMultiplier = 2f;
			AuthDelayRemaining = 0;
			AuthRetryCount = 0;
			bIsRedeemCodeValid = false;
			checked
			{
				while (true)
				{
					long CurrentTicks = DateTime.Now.Ticks;
					if (!bDisableSpark && PreviousTicks > 0)
					{
						int deltaTime = (int)unchecked(checked(CurrentTicks - PreviousTicks) / 10000);
						Tick(deltaTime);
					}
					PreviousTicks = CurrentTicks;
					Thread.Sleep(10);
				}
			}
		}

		private static void Tick(int deltaTime)
		{
			if (StateMachineState != eSparkStateMachine.SPARK_STATE_SparkVerified && StateMachineState != eSparkStateMachine.SPARK_STATE_FailedGiveUp)
			{
				HandleAuthState(deltaTime);
			}
		}

		internal static eSparkStateMachine GetStateMachineState()
		{
			return StateMachineState;
		}

		internal static bool HasShiftAccount()
		{
			return bHasShiftAccount;
		}

		internal static bool HasAgreementsToSign()
		{
			return bAgreementsToSign;
		}

		internal static bool IsRedeemedCodeValid()
		{
			return bIsRedeemCodeValid;
		}

		private static List<SparkEntitlement> FindEntitlementByStringIdentifier(string identifier)
		{
			return entitlements.FindAll((SparkEntitlement entitlement) => entitlement.Identifier == identifier);
		}

		internal static int GetEULACount()
		{
			return eula_count;
		}

		internal static string GetEULAString(int index)
		{
			if (eula_text == null || eula_text.Length == 0)
			{
				return null;
			}
			return eula_text[index];
		}

		internal static string GetAccountUID()
		{
			return AccountUID;
		}

		internal static bool WasErrorDuringSignIn()
		{
			return bErrorDuringSignIn;
		}

		internal static string GetErrorMessage()
		{
			return error_message_text;
		}

		internal static void SetStateMachineStateToFailed()
		{
			StateMachineState = eSparkStateMachine.SPARK_STATE_FailedGiveUp;
			MainWindow.MyMainWindow.SparkInterfaceStateChanged();
		}

		private static void HandleAuthState(int deltaTime)
		{
			checked
			{
				if (AuthDelayRemaining > 0)
				{
					AuthDelayRemaining -= deltaTime;
					return;
				}
				if (GBXHTTP_request != null)
				{
					if (GBXHTTP_request.status == CGBXHTTP_Request.GBXHTTP_Status.TimedOut)
					{
						HandleAuthTokenHttpTimeout();
						return;
					}
					if (GBXHTTP_request.status == CGBXHTTP_Request.GBXHTTP_Status.Failed)
					{
						HandleAuthTokenHttpTimeout();
						return;
					}
					if (GBXHTTP_request.status == CGBXHTTP_Request.GBXHTTP_Status.Waiting)
					{
						return;
					}
				}
				if (StateMachineState == eSparkStateMachine.SPARK_STATE_SteamGetAuthSessionTicket)
				{
					AuthToken_SteamGetAuthSessionTicket();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkSendAuthRequest)
				{
					AuthToken_SparkSendAuthRequest();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkWaitForAuthResponse)
				{
					AuthToken_SparkWaitForAuthResponse();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkSendVerifyRequest)
				{
					AuthToken_SparkSendVerifyRequest();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkWaitForAuthVerifyToken)
				{
					AuthToken_SparkWaitForAuthVerifyToken();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkSend_EULA_Request)
				{
					AuthToken_SparkSend_EULA_Request();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkWaitFor_EULA_Response)
				{
					AuthToken_SparkWaitFor_EULA_Response();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkSendEULASignRequest)
				{
					AuthToken_SparkSignEULARequest();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkWaitForEULASignResponse)
				{
					AuthToken_SparkWaitFor_EULASign_Response();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkSendSignUpRequest)
				{
					AuthToken_SparkSendSignUpRequest();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkWaitForSignUpResponse)
				{
					AuthToken_SparkWaitForSignUpResponse();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkSendSignInRequest)
				{
					AuthToken_SparkSendSignInRequest();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkWaitForSignInResponse)
				{
					AuthToken_SparkWaitForSignInResponse();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkSendResetPasswordRequest)
				{
					AuthToken_SparkSendResetPasswordRequest();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkWaitForResetPasswordResponse)
				{
					AuthToken_SparkWaitForResetPasswordResponse();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkSendCodeRedeemRequest)
				{
					AuthToken_SparkSendCodeRedeemRequest();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkWaitForCodeRedeemResponse)
				{
					AuthToken_SparkWaitForCodeRedeemResponse();
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SteamFailedRetryLater)
				{
					if (AuthRetryCount < 3)
					{
						AuthDelayRemaining = 60000;
						StateMachineState = eSparkStateMachine.SPARK_STATE_SteamGetAuthSessionTicket;
						AuthRetryCount++;
					}
					else
					{
						SetStateMachineStateToFailed();
					}
				}
			}
		}

		private static void HandleAuthTokenHttpTimeout()
		{
			checked
			{
				if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkWaitForAuthResponse)
				{
					if (AuthRetryCount < MaxRetryAttempts)
					{
						AuthDelayRemaining = (int)((BaseRetrySeconds + BaseRetrySeconds * RetryMultiplier * (float)AuthRetryCount) * 1000f);
						StateMachineState = eSparkStateMachine.SPARK_STATE_SparkSendAuthRequest;
						AuthRetryCount++;
						return;
					}
				}
				else if (StateMachineState == eSparkStateMachine.SPARK_STATE_SparkWaitForAuthVerifyToken && AuthRetryCount < VerifyMaxRetryAttempts)
				{
					AuthDelayRemaining = (int)((VerifyBaseRetrySeconds + VerifyBaseRetrySeconds * VerifyRetryMultiplier * (float)AuthRetryCount) * 1000f);
					StateMachineState = eSparkStateMachine.SPARK_STATE_SparkSendVerifyRequest;
					AuthRetryCount++;
					return;
				}
				SetStateMachineStateToFailed();
			}
		}

		private static void AuthToken_SteamGetAuthSessionTicket()
		{
			byte[] AuthSessionTicket = null;
			AuthDelayRemaining = 0;
			AuthSessionTicket = new byte[1024];
			uint SessionTicketSize;
			HAuthTicket authResult = SteamUser.GetAuthSessionTicket(AuthSessionTicket, 1024, out SessionTicketSize);
			if (authResult != HAuthTicket.Invalid)
			{
				AuthSessionTicketString = "";
				BlobToHexString(ref AuthSessionTicketString, ref AuthSessionTicket, SessionTicketSize);
				AuthRetryCount = 0;
				StateMachineState = eSparkStateMachine.SPARK_STATE_SparkSendAuthRequest;
			}
			else
			{
				StateMachineState = eSparkStateMachine.SPARK_STATE_SteamFailedRetryLater;
			}
		}

		private static void AuthToken_SparkSendAuthRequest()
		{
			AuthDelayRemaining = 0;
			if (AuthSessionTicketString != "")
			{
				JObject AuthSessionTicketJSON = new JObject();
				AuthSessionTicketJSON.Add("data", AuthSessionTicketString);
				string SparkAuthURL = SparkBaseURL + "v1/auth/hickory/pc/steam/";
				SparkAuthURL += MainWindow.ThreeLetterLanguageCode.ToLower();
				GBXHTTP_request = new CGBXHTTP_Request(SparkAuthURL, "POST", "application/json", "application/json");
				string buffer = AuthSessionTicketJSON.ToString(Formatting.None);
				GBXHTTP_request.SetData(encoding.GetBytes(buffer));
				int TimeoutMS = checked((int)(HttpTimeout * 1000f));
				GBXHTTP_request.SetTimeout(TimeoutMS);
				IAsyncResult result;
				try
				{
					result = GBXHTTP_request.request.BeginGetRequestStream(GetRequestStreamCallback, GBXHTTP_request);
				}
				catch
				{
					SetStateMachineStateToFailed();
					return;
				}
				ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, TimeoutCallback, GBXHTTP_request, TimeoutMS, true);
				StateMachineState = eSparkStateMachine.SPARK_STATE_SparkWaitForAuthResponse;
			}
		}

		private static void AuthToken_SparkWaitForAuthResponse()
		{
			AuthDelayRemaining = 0;
			if (GBXHTTP_request.status != CGBXHTTP_Request.GBXHTTP_Status.Done)
			{
				return;
			}
			SetSparkServicesParameters(GBXHTTP_request.responseData);
			JObject obj = JObject.Parse(GBXHTTP_request.responseData);
			GBXHTTP_request = null;
			bool bAuthSuccess = true;
			checked
			{
				if (!GetBoolFromJsonObject(obj, "success", ref bAuthSuccess) || !bAuthSuccess)
				{
					if (AuthRetryCount < MaxRetryAttempts)
					{
						AuthDelayRemaining = (int)((BaseRetrySeconds + BaseRetrySeconds * RetryMultiplier * (float)AuthRetryCount) * 1000f);
						StateMachineState = eSparkStateMachine.SPARK_STATE_SparkSendAuthRequest;
						AuthRetryCount++;
					}
					else
					{
						SetStateMachineStateToFailed();
					}
					return;
				}
				JObject archway_obj = null;
				if (GetObjectFromJsonObject(obj, "archway", ref archway_obj))
				{
					GetStringFromJsonObject(archway_obj, "request_id", ref AuthToken);
					AuthDelayRemaining = (int)(VerifyFirstAttemptDelay * 1000f);
					AuthRetryCount = 0;
					StateMachineState = eSparkStateMachine.SPARK_STATE_SparkSendVerifyRequest;
				}
				else
				{
					SetStateMachineStateToFailed();
				}
			}
		}

		private static void AuthToken_SparkSendVerifyRequest()
		{
			AuthDelayRemaining = 0;
			if (VerifyURL != "" && AuthToken != "")
			{
				string url = VerifyURL;
				url += "/verify/hickory/pc/steam/";
				url += MainWindow.ThreeLetterLanguageCode.ToLower();
				url += "/";
				url += AuthToken;
				GBXHTTP_request = new CGBXHTTP_Request(url, "GET", "application/json", "application/json");
				int TimeoutMS = checked((int)(HttpTimeout * 1000f));
				GBXHTTP_request.SetTimeout(TimeoutMS);
				IAsyncResult result = GBXHTTP_request.request.BeginGetResponse(GetResponseCallback, GBXHTTP_request);
				ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, TimeoutCallback, GBXHTTP_request, TimeoutMS, true);
				StateMachineState = eSparkStateMachine.SPARK_STATE_SparkWaitForAuthVerifyToken;
			}
			else
			{
				SetStateMachineStateToFailed();
			}
		}

		private static void AuthToken_SparkWaitForAuthVerifyToken()
		{
			AuthDelayRemaining = 0;
			checked
			{
				if (GBXHTTP_request.status == CGBXHTTP_Request.GBXHTTP_Status.Done)
				{
					SetSparkServicesParameters(GBXHTTP_request.responseData);
					JObject obj = JObject.Parse(GBXHTTP_request.responseData);
					GBXHTTP_request = null;
					bool bAuthSuccess = true;
					if (!GetBoolFromJsonObject(obj, "success", ref bAuthSuccess) || !bAuthSuccess)
					{
						if (AuthRetryCount < MaxRetryAttempts)
						{
							AuthDelayRemaining = (int)((BaseRetrySeconds + BaseRetrySeconds * RetryMultiplier * (float)AuthRetryCount) * 1000f);
							StateMachineState = eSparkStateMachine.SPARK_STATE_SparkSendVerifyRequest;
							AuthRetryCount++;
						}
						else
						{
							SetStateMachineStateToFailed();
						}
						return;
					}
					string sign_agreements = "";
					if (GetStringFromJsonObject(obj, "sign_agreements", ref sign_agreements) && string.Equals(sign_agreements, "NO_AGREEMENTS_TO_SIGN", StringComparison.OrdinalIgnoreCase))
					{
						bAgreementsToSign = false;
					}
					ParseSparkOffersAndEntitlements(obj);
					JObject archway_obj = null;
					if (GetObjectFromJsonObject(obj, "archway", ref archway_obj))
					{
						bool bInProgress = false;
						if (GetBoolFromJsonObject(archway_obj, "in_progress", ref bInProgress) && bInProgress)
						{
							if (AuthRetryCount < VerifyMaxRetryAttempts)
							{
								AuthDelayRemaining = (int)((VerifyBaseRetrySeconds + VerifyBaseRetrySeconds * VerifyRetryMultiplier * (float)AuthRetryCount) * 1000f);
								StateMachineState = eSparkStateMachine.SPARK_STATE_SparkSendVerifyRequest;
								AuthRetryCount++;
							}
							else
							{
								SetStateMachineStateToFailed();
							}
							return;
						}
						if (GetBoolFromJsonObject(archway_obj, "disable_spark", ref bDisableSpark) && bDisableSpark)
						{
							SetStateMachineStateToFailed();
							return;
						}
						GetBoolFromJsonObject(archway_obj, "has_shift_account", ref bHasShiftAccount);
						GetStringFromJsonObject(archway_obj, "account_uid", ref AccountUID);
						if (bAgreementsToSign)
						{
							AuthRetryCount = 0;
							StateMachineState = eSparkStateMachine.SPARK_STATE_SparkSend_EULA_Request;
						}
						else
						{
							StateMachineState = eSparkStateMachine.SPARK_STATE_SparkVerified;
							MainWindow.MyMainWindow.SparkInterfaceStateChanged();
						}
					}
					else if (bAgreementsToSign)
					{
						AuthRetryCount = 0;
						StateMachineState = eSparkStateMachine.SPARK_STATE_SparkSend_EULA_Request;
					}
					else
					{
						StateMachineState = eSparkStateMachine.SPARK_STATE_SparkVerified;
						MainWindow.MyMainWindow.SparkInterfaceStateChanged();
					}
				}
				else
				{
					SetStateMachineStateToFailed();
				}
			}
		}

		private static void AuthToken_SparkSend_EULA_Request()
		{
			AuthDelayRemaining = 0;
			if (AuthToken != "")
			{
				string url = SparkBaseURL;
				url += "v1/eulas/hickory/steam/required?request_id=";
				url += AuthToken;
				url += "&language=";
				url += MainWindow.ThreeLetterLanguageCode.ToLower();
				url += "&country=all";
				GBXHTTP_request = new CGBXHTTP_Request(url, "GET", "application/json", "application/json");
				int TimeoutMS = checked((int)(HttpTimeout * 1000f));
				GBXHTTP_request.SetTimeout(TimeoutMS);
				IAsyncResult result = GBXHTTP_request.request.BeginGetResponse(GetResponseCallback, GBXHTTP_request);
				ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, TimeoutCallback, GBXHTTP_request, TimeoutMS, true);
				StateMachineState = eSparkStateMachine.SPARK_STATE_SparkWaitFor_EULA_Response;
			}
			else
			{
				SetStateMachineStateToFailed();
			}
		}

		private static void AuthToken_SparkWaitFor_EULA_Response()
		{
			AuthDelayRemaining = 0;
			if (GBXHTTP_request.status == CGBXHTTP_Request.GBXHTTP_Status.Done)
			{
				JObject obj = JObject.Parse(GBXHTTP_request.responseData);
				GBXHTTP_request = null;
				if (obj["eulas"] != null && obj["eulas"].Type == JTokenType.Array)
				{
					int index = 0;
					JArray items = (JArray)obj["eulas"];
					eula_count = items.Count;
					eula_id = new int[eula_count];
					eula_version = new string[eula_count];
					eula_text = new string[eula_count];
					foreach (JToken eula_obj in obj["eulas"].Children())
					{
						eula_id[index] = 0;
						GetIntFromJsonToken(eula_obj, "eula_id", ref eula_id[index]);
						eula_version[index] = "";
						GetStringFromJsonToken(eula_obj, "version", ref eula_version[index]);
						string eula_temp_text = "";
						GetStringFromJsonToken(eula_obj, "agreement_text_local", ref eula_temp_text);
						string converted_eula_text = "";
						string text = eula_temp_text;
						foreach (char c in text)
						{
							if (c == '\r')
							{
								continue;
							}
							if (c == '\n')
							{
								converted_eula_text += "<br>";
							}
							else if (c < ' ')
							{
								if (c == '\b')
								{
									converted_eula_text += "&nbsp;";
								}
							}
							else
							{
								converted_eula_text += c;
							}
						}
						eula_text[index] = converted_eula_text;
						index = checked(index + 1);
					}
					StateMachineState = eSparkStateMachine.SPARK_STATE_SparkVerified;
					MainWindow.MyMainWindow.SparkInterfaceStateChanged();
				}
				else
				{
					SetStateMachineStateToFailed();
				}
			}
			else
			{
				SetStateMachineStateToFailed();
			}
		}

		private static void SignEULAsSend()
		{
			AuthDelayRemaining = 0;
			JObject SignEULAsJSON = new JObject();
			JArray EULAsArray = new JArray();
			checked
			{
				for (int index = 0; index < eula_count; index++)
				{
					JObject EULA_JSON = new JObject();
					EULA_JSON.Add("id", eula_id[index]);
					EULA_JSON.Add("version", eula_version[index]);
					EULA_JSON.Add("state", "signed");
					EULAsArray.Add(EULA_JSON);
				}
				SignEULAsJSON.Add("country", SteamUtils.GetIPCountry());
				SignEULAsJSON.Add("request_id", AuthToken);
				SignEULAsJSON.Add("eulas", EULAsArray);
				string SparkAgreementsURL = SparkBaseURL + "v1/users/agreements";
				GBXHTTP_request = new CGBXHTTP_Request(SparkAgreementsURL, "POST", "application/json", "application/json");
				string buffer = SignEULAsJSON.ToString(Formatting.None);
				GBXHTTP_request.SetData(encoding.GetBytes(buffer));
				int TimeoutMS = (int)(HttpTimeout * 1000f);
				GBXHTTP_request.SetTimeout(TimeoutMS);
				IAsyncResult result;
				try
				{
					result = GBXHTTP_request.request.BeginGetRequestStream(GetRequestStreamCallback, GBXHTTP_request);
				}
				catch
				{
					SetStateMachineStateToFailed();
					return;
				}
				ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, TimeoutCallback, GBXHTTP_request, TimeoutMS, true);
				StateMachineState = eSparkStateMachine.SPARK_STATE_SparkWaitForEULASignResponse;
			}
		}

		private static void AuthToken_SparkSignEULARequest()
		{
			SignEULAsSend();
		}

		internal static void SignEULAs()
		{
			AuthRetryCount = 0;
			SignEULAsSend();
		}

		private static void AuthToken_SparkWaitFor_EULASign_Response()
		{
			AuthDelayRemaining = 0;
			checked
			{
				if (GBXHTTP_request.status == CGBXHTTP_Request.GBXHTTP_Status.Done)
				{
					JObject obj = JObject.Parse(GBXHTTP_request.responseData);
					GBXHTTP_request = null;
					bool bAuthSuccess = true;
					if (!GetBoolFromJsonObject(obj, "success", ref bAuthSuccess) && !bAuthSuccess)
					{
						if (AuthRetryCount < MaxRetryAttempts)
						{
							AuthDelayRemaining = (int)((BaseRetrySeconds + BaseRetrySeconds * RetryMultiplier * (float)AuthRetryCount) * 1000f);
							StateMachineState = eSparkStateMachine.SPARK_STATE_SparkSendEULASignRequest;
							AuthRetryCount++;
						}
						else
						{
							SetStateMachineStateToFailed();
						}
						return;
					}
					string sign_agreements = "";
					if (GetStringFromJsonObject(obj, "sign_agreements", ref sign_agreements) && string.Equals(sign_agreements, "NO_AGREEMENTS_TO_SIGN", StringComparison.OrdinalIgnoreCase))
					{
						bAgreementsToSign = false;
					}
					StateMachineState = eSparkStateMachine.SPARK_STATE_SparkVerified;
					MainWindow.MyMainWindow.SparkInterfaceStateChanged();
				}
				else
				{
					SetStateMachineStateToFailed();
				}
			}
		}

		private static void SignUpGearboxAccountSend()
		{
			AuthDelayRemaining = 0;
			bErrorDuringSignIn = false;
			JObject SignUpJSON = new JObject();
			JObject UserJSON = new JObject();
			UserJSON.Add("email", signup_email);
			UserJSON.Add("password", signup_password);
			UserJSON.Add("password_confirmation", signup_confirm_password);
			UserJSON.Add("date_of_birth", signup_current_age);
			UserJSON.Add("account_source", "hickory");
			JObject PlatformConnectionJSON = new JObject();
			PlatformConnectionJSON.Add("platform", "steam");
			SignUpJSON.Add("user", UserJSON);
			SignUpJSON.Add("platform_connection", PlatformConnectionJSON);
			SignUpJSON.Add("language", MainWindow.ThreeLetterLanguageCode.ToLower());
			SignUpJSON.Add("request_id", AuthToken);
			string SparkUsersURL = SparkBaseURL + "v1/users";
			GBXHTTP_request = new CGBXHTTP_Request(SparkUsersURL, "POST", "application/json", "application/json");
			string buffer = SignUpJSON.ToString(Formatting.None);
			GBXHTTP_request.SetData(encoding.GetBytes(buffer));
			int TimeoutMS = checked((int)(HttpTimeout * 1000f));
			GBXHTTP_request.SetTimeout(TimeoutMS);
			IAsyncResult result;
			try
			{
				result = GBXHTTP_request.request.BeginGetRequestStream(GetRequestStreamCallback, GBXHTTP_request);
			}
			catch
			{
				SetStateMachineStateToFailed();
				return;
			}
			ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, TimeoutCallback, GBXHTTP_request, TimeoutMS, true);
			StateMachineState = eSparkStateMachine.SPARK_STATE_SparkWaitForSignUpResponse;
		}

		internal static void SignUpGearboxAccount(string email, string password, string confirm_password, string current_age)
		{
			signup_email = email;
			signup_password = password;
			signup_confirm_password = confirm_password;
			signup_current_age = current_age;
			SignUpGearboxAccountSend();
		}

		private static void AuthToken_SparkSendSignUpRequest()
		{
			SignUpGearboxAccountSend();
		}

		private static void AuthToken_SparkWaitForSignUpResponse()
		{
			AuthDelayRemaining = 0;
			checked
			{
				if (GBXHTTP_request.status == CGBXHTTP_Request.GBXHTTP_Status.Done)
				{
					JObject obj = JObject.Parse(GBXHTTP_request.responseData);
					GBXHTTP_request = null;
					bool bAuthSuccess = true;
					if (!GetBoolFromJsonObject(obj, "success", ref bAuthSuccess) || !bAuthSuccess)
					{
						if (obj["messages"] != null && obj["messages"].Type == JTokenType.Array)
						{
							using (IEnumerator<JToken> enumerator = obj["messages"].Children().GetEnumerator())
							{
								if (enumerator.MoveNext())
								{
									JToken messages_obj = enumerator.Current;
									bErrorDuringSignIn = true;
									error_message_text = "";
									GetStringFromJsonToken(messages_obj, "message", ref error_message_text);
									StateMachineState = eSparkStateMachine.SPARK_STATE_SparkVerified;
									MainWindow.MyMainWindow.SparkInterfaceStateChanged();
									return;
								}
							}
						}
						if (AuthRetryCount < MaxRetryAttempts)
						{
							AuthDelayRemaining = (int)((BaseRetrySeconds + BaseRetrySeconds * RetryMultiplier * (float)AuthRetryCount) * 1000f);
							StateMachineState = eSparkStateMachine.SPARK_STATE_SparkSendSignUpRequest;
							AuthRetryCount++;
						}
						else
						{
							SetStateMachineStateToFailed();
						}
						return;
					}
					JObject archway_obj = null;
					if (GetObjectFromJsonObject(obj, "archway", ref archway_obj))
					{
						bool bInProgress = false;
						if (GetBoolFromJsonObject(archway_obj, "in_progress", ref bInProgress) && bInProgress)
						{
							if (AuthRetryCount < VerifyMaxRetryAttempts)
							{
								AuthDelayRemaining = (int)((VerifyBaseRetrySeconds + VerifyBaseRetrySeconds * VerifyRetryMultiplier * (float)AuthRetryCount) * 1000f);
								StateMachineState = eSparkStateMachine.SPARK_STATE_SparkSendSignUpRequest;
								AuthRetryCount++;
							}
							else
							{
								SetStateMachineStateToFailed();
							}
						}
						else if (GetBoolFromJsonObject(archway_obj, "disable_spark", ref bDisableSpark) && bDisableSpark)
						{
							SetStateMachineStateToFailed();
						}
						else
						{
							ParseSparkOffersAndEntitlements(obj);
							GetStringFromJsonObject(archway_obj, "account_uid", ref AccountUID);
							StateMachineState = eSparkStateMachine.SPARK_STATE_SparkVerified;
							MainWindow.MyMainWindow.SparkInterfaceStateChanged();
						}
					}
					else
					{
						SetStateMachineStateToFailed();
					}
				}
				else
				{
					SetStateMachineStateToFailed();
				}
			}
		}

		public static void SignInGearboxAccountSend()
		{
			AuthDelayRemaining = 0;
			bErrorDuringSignIn = false;
			JObject SignUpJSON = new JObject();
			JObject UserJSON = new JObject();
			UserJSON.Add("login", signup_email);
			UserJSON.Add("password", signup_password);
			JObject PlatformConnectionJSON = new JObject();
			PlatformConnectionJSON.Add("platform", "steam");
			SignUpJSON.Add("user", UserJSON);
			SignUpJSON.Add("platform_connection", PlatformConnectionJSON);
			SignUpJSON.Add("language", MainWindow.ThreeLetterLanguageCode.ToLower());
			SignUpJSON.Add("request_id", AuthToken);
			string SparkUsersURL = SparkBaseURL + "v1/users/sign_in";
			GBXHTTP_request = new CGBXHTTP_Request(SparkUsersURL, "POST", "application/json", "application/json");
			string buffer = SignUpJSON.ToString(Formatting.None);
			GBXHTTP_request.SetData(encoding.GetBytes(buffer));
			int TimeoutMS = checked((int)(HttpTimeout * 1000f));
			GBXHTTP_request.SetTimeout(TimeoutMS);
			IAsyncResult result;
			try
			{
				result = GBXHTTP_request.request.BeginGetRequestStream(GetRequestStreamCallback, GBXHTTP_request);
			}
			catch
			{
				SetStateMachineStateToFailed();
				return;
			}
			ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, TimeoutCallback, GBXHTTP_request, TimeoutMS, true);
			StateMachineState = eSparkStateMachine.SPARK_STATE_SparkWaitForSignInResponse;
		}

		internal static void SignInGearboxAccount(string email, string password)
		{
			signup_email = email;
			signup_password = password;
			SignInGearboxAccountSend();
		}

		private static void AuthToken_SparkSendSignInRequest()
		{
			SignInGearboxAccountSend();
		}

		private static void AuthToken_SparkWaitForSignInResponse()
		{
			AuthDelayRemaining = 0;
			checked
			{
				if (GBXHTTP_request.status == CGBXHTTP_Request.GBXHTTP_Status.Done)
				{
					JObject obj = JObject.Parse(GBXHTTP_request.responseData);
					GBXHTTP_request = null;
					bool bAuthSuccess = true;
					if (!GetBoolFromJsonObject(obj, "success", ref bAuthSuccess) || !bAuthSuccess)
					{
						if (obj["messages"] != null && obj["messages"].Type == JTokenType.Array)
						{
							using (IEnumerator<JToken> enumerator = obj["messages"].Children().GetEnumerator())
							{
								if (enumerator.MoveNext())
								{
									JToken messages_obj = enumerator.Current;
									bErrorDuringSignIn = true;
									error_message_text = "";
									GetStringFromJsonToken(messages_obj, "message", ref error_message_text);
									StateMachineState = eSparkStateMachine.SPARK_STATE_SparkVerified;
									MainWindow.MyMainWindow.SparkInterfaceStateChanged();
									return;
								}
							}
						}
						if (AuthRetryCount < MaxRetryAttempts)
						{
							AuthDelayRemaining = (int)((BaseRetrySeconds + BaseRetrySeconds * RetryMultiplier * (float)AuthRetryCount) * 1000f);
							StateMachineState = eSparkStateMachine.SPARK_STATE_SparkSendSignInRequest;
							AuthRetryCount++;
						}
						else
						{
							SetStateMachineStateToFailed();
						}
						return;
					}
					JObject archway_obj = null;
					if (GetObjectFromJsonObject(obj, "archway", ref archway_obj))
					{
						bool bInProgress = false;
						if (GetBoolFromJsonObject(archway_obj, "in_progress", ref bInProgress) && bInProgress)
						{
							if (AuthRetryCount < VerifyMaxRetryAttempts)
							{
								AuthDelayRemaining = (int)((VerifyBaseRetrySeconds + VerifyBaseRetrySeconds * VerifyRetryMultiplier * (float)AuthRetryCount) * 1000f);
								StateMachineState = eSparkStateMachine.SPARK_STATE_SparkSendSignInRequest;
								AuthRetryCount++;
							}
							else
							{
								SetStateMachineStateToFailed();
							}
						}
						else if (GetBoolFromJsonObject(archway_obj, "disable_spark", ref bDisableSpark) && bDisableSpark)
						{
							SetStateMachineStateToFailed();
						}
						else
						{
							ParseSparkOffersAndEntitlements(obj);
							GetStringFromJsonObject(archway_obj, "account_uid", ref AccountUID);
							StateMachineState = eSparkStateMachine.SPARK_STATE_SparkVerified;
							MainWindow.MyMainWindow.SparkInterfaceStateChanged();
						}
					}
					else
					{
						SetStateMachineStateToFailed();
					}
				}
				else
				{
					SetStateMachineStateToFailed();
				}
			}
		}

		private static void SendResetPasswordSend()
		{
			AuthDelayRemaining = 0;
			bErrorDuringSignIn = false;
			JObject SignUpJSON = new JObject();
			JObject UserJSON = new JObject();
			UserJSON.Add("email", signup_email);
			JObject PlatformConnectionJSON = new JObject();
			PlatformConnectionJSON.Add("platform", "steam");
			SignUpJSON.Add("user", UserJSON);
			SignUpJSON.Add("language", MainWindow.ThreeLetterLanguageCode.ToLower());
			SignUpJSON.Add("request_id", AuthToken);
			string SparkUsersURL = SparkBaseURL + "v1/users/reset_password";
			GBXHTTP_request = new CGBXHTTP_Request(SparkUsersURL, "POST", "application/json", "application/json");
			string buffer = SignUpJSON.ToString(Formatting.None);
			GBXHTTP_request.SetData(encoding.GetBytes(buffer));
			int TimeoutMS = checked((int)(HttpTimeout * 1000f));
			GBXHTTP_request.SetTimeout(TimeoutMS);
			IAsyncResult result;
			try
			{
				result = GBXHTTP_request.request.BeginGetRequestStream(GetRequestStreamCallback, GBXHTTP_request);
			}
			catch
			{
				SetStateMachineStateToFailed();
				return;
			}
			ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, TimeoutCallback, GBXHTTP_request, TimeoutMS, true);
			StateMachineState = eSparkStateMachine.SPARK_STATE_SparkWaitForResetPasswordResponse;
		}

		internal static void SendResetPassword(string email)
		{
			signup_email = email;
			SendResetPasswordSend();
		}

		private static void AuthToken_SparkSendResetPasswordRequest()
		{
			SendResetPasswordSend();
		}

		private static void AuthToken_SparkWaitForResetPasswordResponse()
		{
			AuthDelayRemaining = 0;
			checked
			{
				if (GBXHTTP_request.status == CGBXHTTP_Request.GBXHTTP_Status.Done)
				{
					JObject obj = JObject.Parse(GBXHTTP_request.responseData);
					GBXHTTP_request = null;
					bool bAuthSuccess = true;
					if (!GetBoolFromJsonObject(obj, "success", ref bAuthSuccess) || !bAuthSuccess)
					{
						if (obj["messages"] != null && obj["messages"].Type == JTokenType.Array)
						{
							using (IEnumerator<JToken> enumerator = obj["messages"].Children().GetEnumerator())
							{
								if (enumerator.MoveNext())
								{
									JToken messages_obj = enumerator.Current;
									bErrorDuringSignIn = true;
									error_message_text = "";
									GetStringFromJsonToken(messages_obj, "message", ref error_message_text);
									StateMachineState = eSparkStateMachine.SPARK_STATE_SparkVerified;
									MainWindow.MyMainWindow.SparkInterfaceStateChanged();
									return;
								}
							}
						}
						if (AuthRetryCount < MaxRetryAttempts)
						{
							AuthDelayRemaining = (int)((BaseRetrySeconds + BaseRetrySeconds * RetryMultiplier * (float)AuthRetryCount) * 1000f);
							StateMachineState = eSparkStateMachine.SPARK_STATE_SparkSendResetPasswordRequest;
							AuthRetryCount++;
						}
						else
						{
							SetStateMachineStateToFailed();
						}
					}
					else
					{
						StateMachineState = eSparkStateMachine.SPARK_STATE_SparkVerified;
						MainWindow.MyMainWindow.SparkInterfaceStateChanged();
					}
				}
				else
				{
					SetStateMachineStateToFailed();
				}
			}
		}

		private static void SendCodeRedeemSend()
		{
			AuthDelayRemaining = 0;
			bIsRedeemCodeValid = false;
			JObject UnlockJSON = new JObject();
			UnlockJSON.Add("request_id", AuthToken);
			UnlockJSON.Add("code", redeem_code);
			UnlockJSON.Add("hardware", "pc");
			UnlockJSON.Add("game", "hickory");
			UnlockJSON.Add("language", MainWindow.ThreeLetterLanguageCode.ToLower());
			string SparkUnlockURL = SparkBaseURL + "v1/users/offers/unlock";
			GBXHTTP_request = new CGBXHTTP_Request(SparkUnlockURL, "PUT", "application/json", "application/json");
			string buffer = UnlockJSON.ToString(Formatting.None);
			GBXHTTP_request.SetData(encoding.GetBytes(buffer));
			int TimeoutMS = checked((int)(HttpTimeout * 1000f));
			GBXHTTP_request.SetTimeout(TimeoutMS);
			IAsyncResult result;
			try
			{
				result = GBXHTTP_request.request.BeginGetRequestStream(GetRequestStreamCallback, GBXHTTP_request);
			}
			catch
			{
				SetStateMachineStateToFailed();
				return;
			}
			ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, TimeoutCallback, GBXHTTP_request, TimeoutMS, true);
			StateMachineState = eSparkStateMachine.SPARK_STATE_SparkWaitForCodeRedeemResponse;
		}

		internal static bool SendCodeRedeem()
		{
			if (HickoryMP_BETA_SHiFT_Code == "")
			{
				return false;
			}
			redeem_code = HickoryMP_BETA_SHiFT_Code;
			SendCodeRedeemSend();
			return true;
		}

		private static void AuthToken_SparkSendCodeRedeemRequest()
		{
			SendCodeRedeemSend();
		}

		private static void AuthToken_SparkWaitForCodeRedeemResponse()
		{
			AuthDelayRemaining = 0;
			checked
			{
				if (GBXHTTP_request.status == CGBXHTTP_Request.GBXHTTP_Status.Done)
				{
					JObject obj = JObject.Parse(GBXHTTP_request.responseData);
					GBXHTTP_request = null;
					ParseSparkOffersAndEntitlements(obj);
					bool bRedeemSuccess = false;
					GetBoolFromJsonObject(obj, "success", ref bRedeemSuccess);
					if (bRedeemSuccess)
					{
						if (obj["messages"] != null && obj["messages"].Type == JTokenType.Array)
						{
							using (IEnumerator<JToken> enumerator = obj["messages"].Children().GetEnumerator())
							{
								if (enumerator.MoveNext())
								{
									JToken messages_obj = enumerator.Current;
									string message_text = "";
									GetStringFromJsonToken(messages_obj, "message", ref message_text);
									if (string.Equals(message_text, "CODE_REDEEMED", StringComparison.OrdinalIgnoreCase))
									{
										bIsRedeemCodeValid = true;
									}
									StateMachineState = eSparkStateMachine.SPARK_STATE_SparkVerified;
									MainWindow.MyMainWindow.SparkInterfaceStateChanged();
									return;
								}
							}
						}
						if (AuthRetryCount < MaxRetryAttempts)
						{
							AuthDelayRemaining = (int)((BaseRetrySeconds + BaseRetrySeconds * RetryMultiplier * (float)AuthRetryCount) * 1000f);
							StateMachineState = eSparkStateMachine.SPARK_STATE_SparkSendCodeRedeemRequest;
							AuthRetryCount++;
						}
						else
						{
							SetStateMachineStateToFailed();
						}
					}
					else
					{
						StateMachineState = eSparkStateMachine.SPARK_STATE_SparkVerified;
						MainWindow.MyMainWindow.SparkInterfaceStateChanged();
					}
				}
				else
				{
					SetStateMachineStateToFailed();
				}
			}
		}

		private static void ParseSparkOffersAndEntitlements(JObject obj)
		{
			ParseSparkEntitlements(obj);
		}

		private static void ParseSparkEntitlements(JObject obj)
		{
			entitlements = new List<SparkEntitlement>();
			if (obj["entitlements"] == null || obj["entitlements"].Type != JTokenType.Array)
			{
				return;
			}
			foreach (JToken entitlement in obj["entitlements"].Children())
			{
				int consumable_amount = 0;
				int consumed = 0;
				int user_entitlement_id = 0;
				string string_identifier = "";
				bool consumable = false;
				string payload = "";
				string title_localized = "";
				string description_localized = "";
				GetIntFromJsonToken(entitlement, "consumable_amount", ref consumable_amount);
				GetIntFromJsonToken(entitlement, "consumed", ref consumed);
				GetIntFromJsonToken(entitlement, "user_entitlement_id", ref user_entitlement_id);
				GetStringFromJsonToken(entitlement, "string_identifier", ref string_identifier);
				GetBoolFromJsonToken(entitlement, "consumable", ref consumable);
				GetStringFromJsonToken(entitlement, "payload", ref payload);
				GetStringFromJsonToken(entitlement, "title_localized", ref title_localized);
				GetStringFromJsonToken(entitlement, "description_localized", ref description_localized);
				SparkEntitlement entitlement_data = new SparkEntitlement();
				entitlement_data.ConsumableAmount = consumable_amount;
				entitlement_data.Consumed = consumed;
				entitlement_data.UserEntitlementId = user_entitlement_id;
				entitlement_data.Identifier = string_identifier;
				entitlement_data.Consumable = consumable;
				entitlement_data.Payload = payload;
				entitlement_data.TitleLocalized = title_localized;
				entitlement_data.DescriptionLocalized = description_localized;
				entitlements.Add(entitlement_data);
			}
		}

		private static void SetSparkServicesParameters(string JsonData)
		{
			JObject obj = JObject.Parse(JsonData);
			if (obj["services"] == null || obj["services"].Type != JTokenType.Array)
			{
				return;
			}
			foreach (JToken service in obj["services"].Children())
			{
				string service_name = "";
				if (!GetStringFromJsonToken(service, "service_name", ref service_name))
				{
					continue;
				}
				if (string.Equals(service_name, "Verify", StringComparison.OrdinalIgnoreCase))
				{
					if (service["parameters"] == null || service["parameters"].Type != JTokenType.Array)
					{
						continue;
					}
					string key4 = "";
					string value4 = "";
					foreach (JToken parameter4 in service["parameters"].Children())
					{
						if (GetStringFromJsonToken(parameter4, "key", ref key4) && GetStringFromJsonToken(parameter4, "value", ref value4))
						{
							if (string.Equals(key4, "EndpointUrl", StringComparison.OrdinalIgnoreCase))
							{
								VerifyURL = value4;
							}
							else if (string.Equals(key4, "BaseRetrySeconds", StringComparison.OrdinalIgnoreCase))
							{
								BaseRetrySeconds = Convert.ToSingle(value4);
							}
							else if (string.Equals(key4, "MaxRetryAttempts", StringComparison.OrdinalIgnoreCase))
							{
								MaxRetryAttempts = Convert.ToInt32(value4);
							}
							else if (string.Equals(key4, "RetryMultiplier", StringComparison.OrdinalIgnoreCase))
							{
								RetryMultiplier = Convert.ToSingle(value4);
							}
							else if (string.Equals(key4, "HttpTimeout", StringComparison.OrdinalIgnoreCase))
							{
								HttpTimeout = Convert.ToSingle(value4);
							}
							else if (string.Equals(key4, "Verify.FirstAttemptDelay", StringComparison.OrdinalIgnoreCase))
							{
								VerifyFirstAttemptDelay = Convert.ToSingle(value4);
							}
							else if (string.Equals(key4, "Verify.BaseRetrySeconds", StringComparison.OrdinalIgnoreCase))
							{
								VerifyBaseRetrySeconds = Convert.ToSingle(value4);
							}
							else if (string.Equals(key4, "Verify.MaxRetryAttempts", StringComparison.OrdinalIgnoreCase))
							{
								VerifyMaxRetryAttempts = Convert.ToInt32(value4);
							}
							else if (string.Equals(key4, "Verify.RetryMultiplier", StringComparison.OrdinalIgnoreCase))
							{
								VerifyRetryMultiplier = Convert.ToSingle(value4);
							}
						}
					}
				}
				else if (string.Equals(service_name, "Archway", StringComparison.OrdinalIgnoreCase))
				{
					if (service["parameters"] == null || service["parameters"].Type != JTokenType.Array)
					{
						continue;
					}
					string key3 = "";
					string value3 = "";
					foreach (JToken parameter3 in service["parameters"].Children())
					{
						if (GetStringFromJsonToken(parameter3, "key", ref key3) && GetStringFromJsonToken(parameter3, "value", ref value3) && string.Equals(key3, "EndpointUrl", StringComparison.OrdinalIgnoreCase))
						{
							ArchwayURL = value3;
						}
					}
				}
				else if (string.Equals(service_name, "Leviathan", StringComparison.OrdinalIgnoreCase))
				{
					if (service["parameters"] == null || service["parameters"].Type != JTokenType.Array)
					{
						continue;
					}
					string key2 = "";
					string value2 = "";
					foreach (JToken parameter2 in service["parameters"].Children())
					{
						if (GetStringFromJsonToken(parameter2, "key", ref key2) && GetStringFromJsonToken(parameter2, "value", ref value2) && string.Equals(key2, "EndpointUrl", StringComparison.OrdinalIgnoreCase))
						{
							LeviathanURL = value2;
						}
					}
				}
				else
				{
					if (!string.Equals(service_name, "HickoryMP", StringComparison.OrdinalIgnoreCase) || service["parameters"] == null || service["parameters"].Type != JTokenType.Array)
					{
						continue;
					}
					string key = "";
					string value = "";
					foreach (JToken parameter in service["parameters"].Children())
					{
						if (GetStringFromJsonToken(parameter, "key", ref key) && GetStringFromJsonToken(parameter, "value", ref value) && string.Equals(key, "BetaEnrollSHiFTCode", StringComparison.OrdinalIgnoreCase))
						{
							HickoryMP_BETA_SHiFT_Code = value;
						}
					}
				}
			}
		}
	}
}
