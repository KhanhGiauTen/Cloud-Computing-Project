using System;
using System.Net;
using System.IO;

namespace CloudContactManager.Services.API
{
	// SpeedSMS API client implemented giống mẫu gốc từ SpeedSMS
	public class Speedsmsapi
	{
		public const int TYPE_QC = 1;
		public const int TYPE_CSKH = 2;
		public const int TYPE_BRANDNAME = 3;
		public const int TYPE_BRANDNAME_NOTIFY = 4; // Gửi sms sử dụng brandname Notify
		public const int TYPE_GATEWAY = 5; // Gửi sms sử dụng app android từ số di động cá nhân

		private const string rootURL = "https://api.speedsms.vn/index.php";
		private string accessToken = "sqzsGRMSfm3bKdNDO22sBYL_ofsfBnWw";

		public Speedsmsapi()
		{
		}

		public Speedsmsapi(string token)
		{
			this.accessToken = token;
		}

		private string EncodeNonAsciiCharacters(string value)
		{
			var sb = new System.Text.StringBuilder();
			foreach (char c in value)
			{
				if (c > 127)
				{
					string encodedValue = "\\u" + ((int)c).ToString("x4");
					sb.Append(encodedValue);
				}
				else
				{
					sb.Append(c);
				}
			}
			return sb.ToString();
		}

		public string getUserInfo()
		{
			string url = rootURL + "/user/info";
			NetworkCredential myCreds = new NetworkCredential(accessToken, ":x");
			using var client = new WebClient();
			client.Credentials = myCreds;
			using Stream data = client.OpenRead(url);
			using var reader = new StreamReader(data);
			return reader.ReadToEnd();
		}

		public string sendSMS(string[] phones, string content, int type, string sender)
		{
			string url = rootURL + "/sms/send";
			if (phones == null || phones.Length <= 0)
				return string.Empty;
			if (string.IsNullOrEmpty(content))
				return string.Empty;

			if (type == TYPE_BRANDNAME && string.IsNullOrEmpty(sender))
				return string.Empty;

			NetworkCredential myCreds = new NetworkCredential(accessToken, ":x");
			using var client = new WebClient();
			client.Credentials = myCreds;
			client.Headers[HttpRequestHeader.ContentType] = "application/json";

			string builder = "{\"to\":[";
			for (int i = 0; i < phones.Length; i++)
			{
				builder += "\"" + phones[i] + "\"";
				if (i < phones.Length - 1)
				{
					builder += ",";
				}
			}
			builder += "], \"content\": \"" + Uri.EscapeDataString(content) + "\", \"type\":" + type + ", \"sender\": \"" + sender + "\"}";

			string json = builder;
			return client.UploadString(url, json);
		}

		public string sendMMS(string[] phones, string content, string link, string sender)
		{
			string url = rootURL + "/mms/send";
			if (phones == null || phones.Length <= 0)
				return string.Empty;
			if (string.IsNullOrEmpty(content))
				return string.Empty;

			NetworkCredential myCreds = new NetworkCredential(accessToken, ":x");
			using var client = new WebClient();
			client.Credentials = myCreds;
			client.Headers[HttpRequestHeader.ContentType] = "application/json";

			string builder = "{\"to\":[";
			for (int i = 0; i < phones.Length; i++)
			{
				builder += "\"" + phones[i] + "\"";
				if (i < phones.Length - 1)
				{
					builder += ",";
				}
			}
			builder += "], \"content\": \"" + Uri.EscapeDataString(content) + "\", \"link\": \"" + link + "\", \"sender\": \"" + sender + "\"}";

			string json = builder;
			return client.UploadString(url, json);
		}
	}
}
