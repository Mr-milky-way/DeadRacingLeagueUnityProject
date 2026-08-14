using System;
using System.Runtime.Serialization;
using UnityEngine;
using thelab.core;

namespace drl.backend
{
	[Serializable]
	public class DRLServiceResult
	{
		[NonSerialized]
		public WebAsyncRequest request;

		[OptionalField]
		public string id;

		public bool success;

		public bool encoded = true;

		[OptionalField]
		public string message;

		[OptionalField]
		public string token;

		[OptionalField]
		public string webtoken;

		[OptionalField]
		public object data;

		public T GetData<T>()
		{
			T result = default(T);
			try
			{
				string text = ((data == null) ? "{}" : data.ToString());
				result = ((!encoded) ? Serialize.FromJson<T>(text) : ParseBase64Json<T>(text));
				if (data == null)
				{
					Debug.LogWarning("DRLService> Tried to parse null data - id[" + id + "]");
				}
			}
			catch (Exception ex)
			{
				string text2 = (encoded ? (data as string) : "");
				string text3 = (encoded ? Serialize.FromBase64<string>(data as string) : (data as string));
				Debug.LogError("DRLServiceResult> Failed to Parse Data [" + id + "]\nencoded[" + encoded + "]\nBase64 [" + text2 + "]\nJSON[" + text3 + "]\nType[" + typeof(T).Name + "]\nMessage:\n" + ex.Message);
			}
			return result;
		}

		[Obsolete("Authentication v1 logic, no longer in use.")]
		public DRLToken GetToken()
		{
			return ParseBase64Json<DRLToken>(token);
		}

		protected T ParseBase64Json<T>(string v)
		{
			if (string.IsNullOrEmpty(v))
			{
				return default(T);
			}
			return Serialize.FromJson<T>(Serialize.FromBase64<string>(v));
		}
	}
}
