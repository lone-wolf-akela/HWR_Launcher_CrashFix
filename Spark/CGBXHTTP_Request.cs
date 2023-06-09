using System.Net;

namespace Spark
{
	internal class CGBXHTTP_Request
	{
		internal enum GBXHTTP_Status
		{
			NotActive,
			Waiting,
			Done,
			TimedOut,
			Failed
		}

		internal HttpWebRequest request;

		internal HttpWebResponse response;

		internal byte[] postData;

		internal string responseData;

		internal GBXHTTP_Status status;

		internal CGBXHTTP_Request()
		{
			request = null;
			response = null;
			status = GBXHTTP_Status.NotActive;
		}

		internal static IPEndPoint BindIPEndPointCallback(ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount)
		{
			return new IPEndPoint(IPAddress.Any, 5000);
		}

		internal CGBXHTTP_Request(string URL, string method, string ContentType, string accept)
		{
			request = (HttpWebRequest)WebRequest.Create(URL);
			request.ServicePoint.BindIPEndPointDelegate = BindIPEndPointCallback;
			request.Method = method;
			request.ContentType = ContentType;
			request.Accept = accept;
			response = null;
			postData = null;
			responseData = "";
			status = GBXHTTP_Status.Waiting;
		}

		internal void SetData(byte[] Data)
		{
			postData = Data;
			request.ContentLength = postData.Length;
		}

		internal void SetTimeout(int Timeout)
		{
			request.Timeout = Timeout;
			request.ReadWriteTimeout = Timeout;
		}
	}
}
