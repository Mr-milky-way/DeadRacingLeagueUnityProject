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

		private bool m_editor_offline;

		private const string editorOfflineError = "DRL online service is unavailable in the editor.";

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
			if (m_editor_offline)
			{
				m_is_sent = true;
			}
			else if (loader == null)
			{
				Debug.LogWarning("AsyncRequest> Send / Loader is <null> - " + id);
			}
			else if (!m_is_sent)
			{
				m_is_sent = true;
				loader.SendWebRequest();
			}
		}

		public override void Build(object p_data, Dictionary<string, string> p_headers, Type p_response_type)
		{
			m_editor_offline = Application.isEditor && IsDRLServiceUrl(path);
			if (m_editor_offline)
			{
				data = p_data;
				requestHeaders = p_headers ?? new Dictionary<string, string>();
				responseType = p_response_type;
				return;
			}
			base.Build(p_data, p_headers, p_response_type);
		}

		private static bool IsDRLServiceUrl(string p_url)
		{
			Uri uri;
			if (!Uri.TryCreate(p_url, UriKind.Absolute, out uri))
			{
				return false;
			}
			string host = uri.Host.ToLowerInvariant();
			return host == "api.drlgame.com" || host == "status.drlgame.com" || host == "drl-game-api.s3.amazonaws.com" || host == "drl-sim-virtual-season.s3.amazonaws.com";
		}

		protected override bool IsValid()
		{
			return m_editor_offline || base.IsValid();
		}

		protected override bool IsComplete()
		{
			return m_editor_offline || base.IsComplete();
		}

		protected override float GetProgress()
		{
			return m_editor_offline ? 1f : base.GetProgress();
		}

		protected override string GetError()
		{
			return m_editor_offline ? editorOfflineError : base.GetError();
		}
	}
}
