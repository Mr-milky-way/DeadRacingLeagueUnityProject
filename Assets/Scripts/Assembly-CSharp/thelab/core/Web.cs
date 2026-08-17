using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace thelab.core
{
	public class Web
	{
		public static AsyncManager manager => AsyncManager.instance;

		public static string GenerateId(string p_method)
		{
			int num = Mathf.FloorToInt((Time.unscaledTime + UnityEngine.Random.value / 100f) * 1000f);
			return "request-" + p_method.ToLower() + "-" + num.ToString("00000000");
		}

		public static string GenerateQueryStringURL(string p_url, object p_data)
		{
			string text = "";
			if (p_data is IList)
			{
				text = Serialize.ToQueryString(Serialize.ToObjectList(p_data as IList).ToArray());
			}
			if (p_data is IDictionary)
			{
				text = Serialize.ToQueryString(p_data as IDictionary);
			}
			if (!(text == ""))
			{
				return p_url + "?" + text;
			}
			return p_url;
		}

		public static float Progress(string p_query, bool p_exact = true)
		{
			return manager.GetProgress(p_query, p_exact);
		}

		protected static WebAsyncRequest Create<T>(string p_id, string p_url, string p_method, WebCallback<T> p_callback, object p_data, Dictionary<string, string> p_headers = null, int p_timeout = -1)
		{
			string p_id2 = (string.IsNullOrEmpty(p_id) ? GenerateId(p_method) : p_id);
			WebAsyncRequest req = (WebAsyncRequest)manager.Load<T>(p_id2, p_method, p_url, p_data, p_headers);
			float t = Time.unscaledTime;
			bool finished = false;
			UnityAction<AsyncRequestEvent> cb = null;
			cb = delegate(AsyncRequestEvent e)
			{
				if (e.target == req && !finished)
				{
					if (p_callback != null)
					{
						switch (e.type)
						{
						case AsyncRequestEventType.Start:
							p_callback(default(T), 0f, req);
							break;
						case AsyncRequestEventType.Progress:
							p_callback(default(T), req.progress * 0.999f, req);
							break;
						case AsyncRequestEventType.UploadProgress:
							p_callback(default(T), req.progress, req);
							break;
						case AsyncRequestEventType.Error:
							p_callback(req.Get<T>(), 1f, req);
							break;
						case AsyncRequestEventType.Cancel:
							p_callback(default(T), 1f, req);
							break;
						case AsyncRequestEventType.Complete:
							p_callback(req.Get<T>(), 1f, req);
							break;
						}
					}
					AsyncRequestEventType type = e.type;
					if ((uint)(type - 5) <= 2u)
					{
						finished = true;
						manager.OnEvent.RemoveListener(cb);
					}
				}
			};
			manager.OnEvent.AddListener(cb);
			Activity.Run((Func<bool>)delegate
			{
				if (p_timeout < 0 || finished)
				{
					return false;
				}
				if (!manager)
				{
					return false;
				}
				if (Time.unscaledTime - t < (float)p_timeout)
				{
					return true;
				}
				finished = true;
				manager.OnEvent.RemoveListener(cb);
				if (req != null)
				{
					req.Timeout();
				}
				if (p_callback != null)
				{
					p_callback(default(T), 1f, req);
				}
				return false;
			}, 0f, false);
			return req;
		}

		public static WebAsyncRequest Load<T>(string p_id, string p_url, string p_method, WebCallback<T> p_callback, object p_data = null, Dictionary<string, string> p_headers = null, int p_timeout = -1)
		{
			return Create(p_id, p_url, p_method, p_callback, p_data, p_headers, p_timeout);
		}

		public static WebAsyncRequest Load<T>(string p_url, string p_method, WebCallback<T> p_callback, object p_data = null, Dictionary<string, string> p_headers = null, int p_timeout = -1)
		{
			return Load("", p_url, p_method, p_callback, p_data, p_headers, p_timeout);
		}

		public static WebAsyncRequest Get<T>(string p_id, string p_url, WebCallback<T> p_callback, object p_data = null, Dictionary<string, string> p_headers = null, int p_timeout = -1)
		{
			return Load(p_id, GenerateQueryStringURL(p_url, p_data), "GET", p_callback, null, p_headers, p_timeout);
		}

		public static WebAsyncRequest Get<T>(string p_url, WebCallback<T> p_callback, object p_data = null, Dictionary<string, string> p_headers = null, int p_timeout = -1)
		{
			return Get("", p_url, p_callback, p_data, p_headers, p_timeout);
		}

		public static WebAsyncRequest Post<T>(string p_id, string p_url, WebCallback<T> p_callback, object p_data = null, Dictionary<string, string> p_headers = null, int p_timeout = -1)
		{
			return Load(p_id, p_url, "POST", p_callback, p_data, p_headers, p_timeout);
		}

		public static WebAsyncRequest Post<T>(string p_url, WebCallback<T> p_callback, object p_data = null, Dictionary<string, string> p_headers = null, int p_timeout = -1)
		{
			return Post("", p_url, p_callback, p_data, p_headers, p_timeout);
		}
	}
}
