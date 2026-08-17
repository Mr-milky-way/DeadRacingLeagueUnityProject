using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace thelab.core
{
	public class WebAsyncRequest : UWRAsyncRequest
	{
		private long m_code = -1L;

		private bool m_is_sent;

		public string method
		{
			get
			{
				if (loader != null)
				{
					return loader.method;
				}
				return "";
			}
			set
			{
				loader.method = (string.IsNullOrEmpty(value) ? "GET" : value.ToUpper());
			}
		}

		public long code
		{
			get
			{
				if (m_code >= 0)
				{
					return m_code;
				}
				try
				{
					UnityWebRequest unityWebRequest = loader;
					m_code = ((unityWebRequest == null) ? 0 : (unityWebRequest.isDone ? unityWebRequest.responseCode : 0));
				}
				catch (Exception ex)
				{
					Debug.LogWarning("AsyncRequest> " + id + " Get ResponseCode Error\n" + ex.Message);
					m_code = 0L;
				}
				return m_code;
			}
		}

		public Dictionary<string, string> responseHeaders
		{
			get
			{
				if (loader == null)
				{
					return new Dictionary<string, string>();
				}
				Dictionary<string, string> dictionary = null;
				dictionary = loader.GetResponseHeaders();
				if (dictionary != null)
				{
					return dictionary;
				}
				return new Dictionary<string, string>();
			}
		}

		public void Send()
		{
			if (loader == null)
			{
				Debug.LogWarning("AsyncRequest> Send / Loader is <null> - " + id);
			}
			else if (!m_is_sent)
			{
				m_is_sent = true;
				loader.SendWebRequest();
			}
		}
	}
}
