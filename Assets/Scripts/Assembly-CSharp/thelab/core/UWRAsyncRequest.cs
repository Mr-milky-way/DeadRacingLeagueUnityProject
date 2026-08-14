using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace thelab.core
{
	public class UWRAsyncRequest : AsyncRequest<UnityWebRequest>
	{
		public object data;

		public bool linearTextureColor;

		public bool useMipmap;

		public string encrypt;

		public Dictionary<string, string> requestHeaders;

		private long m_length = -1L;

		public Type responseType;

		private bool m_exception_complete;

		public long length
		{
			get
			{
				if (m_length >= 0)
				{
					return m_length;
				}
				if (loader == null)
				{
					return m_length;
				}
				string responseHeader = loader.GetResponseHeader("content-length");
				if (string.IsNullOrEmpty(responseHeader))
				{
					return m_length;
				}
				if (!long.TryParse(responseHeader, out m_length))
				{
					m_length = 0L;
				}
				return m_length;
			}
		}

		protected UnityWebRequest Post(string p_uri, Dictionary<string, string> p_data)
		{
			UnityWebRequest unityWebRequest = new UnityWebRequest(p_uri, "POST");
			byte[] array = null;
			if (p_data != null)
			{
				array = SerializeForm(p_data);
			}
			unityWebRequest.uploadHandler = new UploadHandlerRaw(array)
			{
				contentType = "application/x-www-form-urlencoded"
			};
			unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
			return unityWebRequest;
		}

		protected byte[] SerializeForm(Dictionary<string, string> p_form)
		{
			if (p_form == null)
			{
				return new byte[0];
			}
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			foreach (KeyValuePair<string, string> item in p_form)
			{
				list.Add(item.Key);
				list2.Add(item.Value);
			}
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = SafeUri(list[i]);
			}
			for (int j = 0; j < list2.Count; j++)
			{
				list2[j] = SafeUri(list2[j]);
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int k = 0; k < list.Count; k++)
			{
				if (k > 0)
				{
					stringBuilder.Append("&");
				}
				stringBuilder.Append(list[k]);
				stringBuilder.Append("=");
				stringBuilder.Append(list2[k]);
			}
			return Encoding.UTF8.GetBytes(stringBuilder.ToString());
		}

		protected string SafeUri(string v)
		{
			if (v.Length < 10000)
			{
				return Uri.EscapeDataString(v);
			}
			int i = 0;
			StringBuilder stringBuilder = new StringBuilder();
			int num;
			for (; i < v.Length; i += num)
			{
				num = Mathf.Max(1, Mathf.Min(2000, v.Length - i));
				int num2 = num;
				string stringToEscape = v.Substring(i, num2);
				stringToEscape = Uri.EscapeDataString(stringToEscape);
				stringBuilder.Append(stringToEscape);
			}
			return stringBuilder.ToString();
		}

		public override void Build(object p_data, Dictionary<string, string> p_headers, Type p_response_type)
		{
			string text = (string.IsNullOrEmpty(path) ? " " : path);
			Dictionary<string, string> dictionary = ((p_headers == null) ? new Dictionary<string, string>() : p_headers);
			object obj = ((p_data == null) ? "" : p_data);
			byte[] array = null;
			string method = "GET";
			requestHeaders = new Dictionary<string, string>(dictionary);
			switch (type)
			{
			case AsyncRequestType.BundleLoad:
				method = "GET";
				break;
			case AsyncRequestType.HttpGet:
				method = "GET";
				break;
			case AsyncRequestType.HttpPost:
				method = "POST";
				break;
			case AsyncRequestType.HttpPut:
				method = "PUT";
				break;
			case AsyncRequestType.HttpDelete:
				method = "DELETE";
				break;
			case AsyncRequestType.HttpCreate:
				method = "CREATE";
				break;
			case AsyncRequestType.HttpHead:
				method = "HEAD";
				break;
			}
			data = p_data;
			if (obj is Dictionary<string, object>)
			{
				Dictionary<string, object> obj2 = obj as Dictionary<string, object>;
				Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
				foreach (KeyValuePair<string, object> item in obj2)
				{
					string key = item.Key;
					object value = item.Value;
					if (value != null)
					{
						dictionary2[key] = ((value is bool) ? value.ToString().ToLower() : value.ToString());
					}
				}
				obj = dictionary2;
			}
			if (obj is SerializedData)
			{
				obj = (obj as SerializedData).ToHashTable();
			}
			if (obj is WWWForm)
			{
				array = ((WWWForm)obj).data;
			}
			else if (obj is Dictionary<string, string>)
			{
				array = SerializeForm(obj as Dictionary<string, string>);
			}
			else if (obj is List<IMultipartFormSection>)
			{
				array = UnityWebRequest.SerializeFormSections(obj as List<IMultipartFormSection>, UnityWebRequest.GenerateBoundary());
			}
			else if (obj is string)
			{
				array = Serialize.ToBytes(obj.ToString(), Encoding.UTF8);
			}
			UnityWebRequest unityWebRequest = null;
			if (p_response_type == typeof(AudioClip))
			{
				unityWebRequest = (string.IsNullOrEmpty(text) ? null : UnityWebRequestMultimedia.GetAudioClip(text, AudioType.UNKNOWN));
			}
			if (p_response_type == typeof(AssetBundle))
			{
				unityWebRequest = (string.IsNullOrEmpty(text) ? null : UnityWebRequestAssetBundle.GetAssetBundle(text));
			}
			responseType = p_response_type;
			if (unityWebRequest == null)
			{
				switch (type)
				{
				case AsyncRequestType.HttpPost:
					if (obj is WWWForm)
					{
						unityWebRequest = UnityWebRequest.Post(text, obj as WWWForm);
					}
					else if (obj is Dictionary<string, string>)
					{
						unityWebRequest = Post(text, obj as Dictionary<string, string>);
					}
					else if (obj is List<IMultipartFormSection>)
					{
						unityWebRequest = UnityWebRequest.Post(text, obj as List<IMultipartFormSection>);
					}
					else if (obj is string)
					{
						unityWebRequest = UnityWebRequest.Post(text, obj.ToString());
					}
					break;
				case AsyncRequestType.HttpPut:
					unityWebRequest = UnityWebRequest.Put(text, array);
					break;
				case AsyncRequestType.HttpGet:
					unityWebRequest = UnityWebRequest.Get(text);
					break;
				case AsyncRequestType.HttpDelete:
					unityWebRequest = UnityWebRequest.Delete(text);
					break;
				case AsyncRequestType.HttpHead:
					unityWebRequest = UnityWebRequest.Head(text);
					break;
				}
				if (unityWebRequest == null)
				{
					unityWebRequest = UnityWebRequest.Get(text);
				}
			}
			unityWebRequest.method = method;
			if (unityWebRequest.downloadHandler == null)
			{
				unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
			}
			AsyncRequestType asyncRequestType = type;
			if ((uint)(asyncRequestType - 2) <= 1u && array != null && array.Length != 0 && unityWebRequest.uploadHandler == null)
			{
				string contentType = ((obj is WWWForm) ? "multipart/form-data" : "application/x-www-form-urlencoded");
				UploadHandlerRaw uploadHandlerRaw = new UploadHandlerRaw(array);
				uploadHandlerRaw.contentType = contentType;
				unityWebRequest.uploadHandler = uploadHandlerRaw;
			}
			if (!string.IsNullOrEmpty(encrypt) && unityWebRequest.uploadHandler != null)
			{
				array = unityWebRequest.uploadHandler.data;
				if (array != null)
				{
					for (int i = 0; i < array.Length; i++)
					{
						byte b = array[i];
						b = (byte)(~b);
						array[i] = b;
					}
					UploadHandlerRaw uploadHandlerRaw2 = new UploadHandlerRaw(array);
					uploadHandlerRaw2.contentType = unityWebRequest.uploadHandler.contentType;
					unityWebRequest.uploadHandler = uploadHandlerRaw2;
				}
			}
			foreach (KeyValuePair<string, string> item2 in dictionary)
			{
				unityWebRequest.SetRequestHeader(item2.Key, item2.Value);
			}
			loader = unityWebRequest;
		}

		protected override void OnApply()
		{
			if (IsBundle())
			{
				AssetBundle assetBundle = Get<AssetBundle>();
				if ((bool)assetBundle)
				{
					assetBundle.GetAllScenePaths().ToString();
				}
			}
		}

		protected override bool IsValid()
		{
			return loader != null;
		}

		protected override T OnGet<T>()
		{
			Type typeFromHandle = typeof(T);
			if (loader == null)
			{
				return default(T);
			}
			if (loader.downloadHandler is DownloadHandlerFile)
			{
				return default(T);
			}
			DownloadHandler downloadHandler = null;
			try
			{
				downloadHandler = loader.downloadHandler;
			}
			catch (Exception ex)
			{
				Debug.LogError("WebAsyncRequest> Error!\n" + ex.Message);
				downloadHandler = null;
			}
			if (downloadHandler == null)
			{
				return default(T);
			}
			bool flag = downloadHandler.isDone;
			if (!flag)
			{
				switch (loader.responseCode / 100)
				{
				case 4L:
					flag = true;
					break;
				case 5L:
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return default(T);
			}
			if (typeFromHandle == typeof(byte[]))
			{
				return (T)(object)downloadHandler.data;
			}
			if (typeFromHandle == typeof(string))
			{
				return (T)(object)downloadHandler.text;
			}
			if (typeFromHandle == typeof(Texture2D))
			{
				Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false);
				texture2D.LoadImage(downloadHandler.data, markNonReadable: false);
				texture2D.name = id + "-" + texture2D.graphicsFormat.ToString().ToLower() + "-" + texture2D.format.ToString().ToLower();
				if ((bool)texture2D && useMipmap && texture2D.mipmapCount <= 1)
				{
					texture2D.Apply(updateMipmaps: true);
				}
				return (T)(object)texture2D;
			}
			if (typeFromHandle == typeof(AssetBundle) && downloadHandler is DownloadHandlerAssetBundle)
			{
				return (T)(object)((DownloadHandlerAssetBundle)downloadHandler).assetBundle;
			}
			if (typeFromHandle == typeof(AudioClip) && downloadHandler is DownloadHandlerAudioClip)
			{
				return (T)(object)((DownloadHandlerAudioClip)downloadHandler).audioClip;
			}
			return default(T);
		}

		protected override float GetProgress()
		{
			if (loader == null)
			{
				return 1f;
			}
			long downloadedBytes = (long)loader.downloadedBytes;
			long num = length;
			if (loader.uploadHandler != null && loader.uploadProgress < 1f)
			{
				return 0f - (1f - loader.uploadProgress);
			}
			float num2 = loader.downloadProgress;
			if (num2 <= 0f)
			{
				float num3 = downloadedBytes;
				float num4 = num;
				if (num4 > 0f)
				{
					num2 = num3 / num4;
				}
			}
			return num2;
		}

		protected override bool IsComplete()
		{
			if (loader == null)
			{
				return true;
			}
			if (m_exception_complete)
			{
				return true;
			}
			bool flag = true;
			try
			{
				flag = loader.isDone;
			}
			catch (Exception)
			{
				flag = (m_exception_complete = true);
			}
			return flag;
		}

		protected override string GetError()
		{
			if (loader == null)
			{
				return "UWRAsyncRequest> Invalid Loader.";
			}
			string text = "";
			try
			{
				return loader.isNetworkError ? loader.error : "";
			}
			catch (Exception)
			{
				return "UWRAsyncRequest> Invalid Loader.";
			}
		}

		protected override void OnCancel()
		{
			if (loader != null)
			{
				loader.Abort();
				loader.Dispose();
				loader = null;
			}
		}
	}
}
