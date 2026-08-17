using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

namespace thelab.core
{
	public class AsyncManager : MonoBehaviour
	{
		protected static AsyncManager m_instance;

		[SerializeField]
		protected List<AsyncRequest> m_requests;

		public bool profilerEnabled;

		[SerializeField]
		protected AsyncRequestCallback m_OnEvent;

		public static AsyncManager instance
		{
			get
			{
				if (Application.isPlaying && (bool)m_instance && (m_instance.gameObject.hideFlags & HideFlags.DontSave) != HideFlags.None)
				{
					UnityEngine.Object.Destroy(m_instance.gameObject);
					m_instance = null;
				}
				if ((bool)m_instance)
				{
					return m_instance;
				}
				m_instance = UnityEngine.Object.FindObjectOfType<AsyncManager>();
				if (!m_instance)
				{
					m_instance = new GameObject("async-manager").AddComponent<AsyncManager>();
					if (Application.isPlaying)
					{
						UnityEngine.Object.DontDestroyOnLoad(m_instance.gameObject);
					}
					else
					{
						m_instance.gameObject.hideFlags = HideFlags.DontSave;
					}
				}
				return m_instance;
			}
		}

		public List<AsyncRequest> requests
		{
			get
			{
				if (m_requests != null)
				{
					return m_requests;
				}
				return m_requests = new List<AsyncRequest>();
			}
		}

		public AsyncRequestCallback OnEvent
		{
			get
			{
				if (m_OnEvent == null)
				{
					m_OnEvent = new AsyncRequestCallback();
				}
				return m_OnEvent;
			}
			set
			{
				m_OnEvent = value;
			}
		}

		static AsyncManager()
		{
		}

		public List<AsyncRequest> FindAll(string p_query, bool p_exact = true)
		{
			return requests.FindAll(delegate(AsyncRequest it)
			{
				if (p_query == "")
				{
					return true;
				}
				return (!p_exact) ? p_query.Contains(it.id) : (it.id == p_query);
			});
		}

		public List<AsyncRequest> FindAll(string p_query, bool p_exact = true, AsyncRequestStatus p_status = AsyncRequestStatus.Active)
		{
			return FindAll(p_query, p_exact).FindAll((AsyncRequest it) => it.status == p_status);
		}

		public AsyncRequest Find(string p_query, bool p_exact = true)
		{
			return requests.Find((AsyncRequest it) => (!p_exact) ? p_query.Contains(it.id) : (it.id == p_query));
		}

		public void RemoveAll(string p_query, bool p_exact = true)
		{
			requests.RemoveAll(delegate(AsyncRequest it)
			{
				if (p_query == "")
				{
					return true;
				}
				return (!p_exact) ? p_query.Contains(it.id) : (it.id == p_query);
			});
		}

		public float GetProgress(string p_query, bool p_exact = true)
		{
			List<AsyncRequest> list = FindAll(p_query, p_exact, AsyncRequestStatus.Active);
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < list.Count; i++)
			{
				AsyncRequest asyncRequest = list[i];
				num += asyncRequest.progress;
				num2 += 1f;
			}
			if (!(num2 <= 0f))
			{
				return num / num2;
			}
			return 1f;
		}

		public bool MatchAllStatus(AsyncRequestStatus p_status, string p_query, bool p_exact = true)
		{
			List<AsyncRequest> list = FindAll(p_query, p_exact, p_status);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].status != p_status)
				{
					return false;
				}
			}
			return true;
		}

		public bool MatchAnyStatus(AsyncRequestStatus p_status, string p_query, bool p_exact = true)
		{
			List<AsyncRequest> list = FindAll(p_query, p_exact, p_status);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].status == p_status)
				{
					return true;
				}
			}
			return false;
		}

		public T Get<T>(string p_query)
		{
			AsyncRequest asyncRequest = Find(p_query);
			if (asyncRequest == null)
			{
				return default(T);
			}
			return asyncRequest.Get<T>();
		}

		public bool Exists(string p_id)
		{
			return Find(p_id) != null;
		}

		public void Cancel(string p_query, bool p_exact = true)
		{
			List<AsyncRequest> list = FindAll(p_query, p_exact);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null)
				{
					list[i].Cancel();
				}
			}
		}

		public AsyncRequest Load<T>(string p_id, string p_method, string p_url, object p_data, Dictionary<string, string> p_headers)
		{
			AsyncRequestType p_type = AsyncRequestType.HttpGet;
			switch (p_method.ToUpper())
			{
			case "GET":
				p_type = AsyncRequestType.HttpGet;
				break;
			case "POST":
				p_type = AsyncRequestType.HttpPost;
				break;
			case "HEAD":
				p_type = AsyncRequestType.HttpHead;
				break;
			case "DELETE":
				p_type = AsyncRequestType.HttpDelete;
				break;
			case "PUT":
				p_type = AsyncRequestType.HttpPut;
				break;
			}
			return (WebAsyncRequest)Create(p_type, p_id, p_url, p_data, p_headers, typeof(T));
		}

		public AsyncRequest ReadBundle(string p_id, byte[] p_data, bool p_active_scenes = false)
		{
			MemoryStream stream = new MemoryStream(p_data);
			AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromStreamAsync(stream);
			assetBundleCreateRequest.allowSceneActivation = p_active_scenes;
			BundleAsyncRequest bundleAsyncRequest = (BundleAsyncRequest)Create(AsyncRequestType.BundleRead, p_id, "@memory", assetBundleCreateRequest, null, null);
			bundleAsyncRequest.stream = stream;
			Dispatch(AsyncRequestEventType.Create, bundleAsyncRequest);
			return bundleAsyncRequest;
		}

		public AsyncRequest ReadBundle(string p_id, string p_path, bool p_active_scenes = false)
		{
			AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(p_path);
			assetBundleCreateRequest.allowSceneActivation = p_active_scenes;
			AsyncRequest asyncRequest = Create(AsyncRequestType.BundleRead, p_id, p_path, assetBundleCreateRequest, null, null);
			Dispatch(AsyncRequestEventType.Create, asyncRequest);
			return asyncRequest;
		}

		protected void Update()
		{
			if (Application.isPlaying)
			{
				OnUpdate();
			}
		}

		protected void OnUpdate()
		{
			bool flag = false;
			bool flag2 = profilerEnabled && Profiler.enabled;
			for (int i = 0; i < requests.Count; i++)
			{
				AsyncRequest asyncRequest = requests[i];
				float progress = asyncRequest.progress;
				flag = false;
				if (flag2)
				{
					_ = "async." + asyncRequest.type.ToString().ToLower();
					if (asyncRequest is WebAsyncRequest)
					{
						_ = "awr." + ((WebAsyncRequest)asyncRequest).id;
					}
				}
				switch (asyncRequest.status)
				{
				case AsyncRequestStatus.Created:
					if (!asyncRequest.started)
					{
						asyncRequest.started = true;
						asyncRequest.status = AsyncRequestStatus.Active;
						if (asyncRequest is WebAsyncRequest)
						{
							((WebAsyncRequest)asyncRequest).Send();
						}
						Dispatch(AsyncRequestEventType.Start, asyncRequest);
					}
					break;
				case AsyncRequestStatus.Active:
					if (asyncRequest.cancelled)
					{
						asyncRequest.status = AsyncRequestStatus.Cancelled;
						Dispatch(AsyncRequestEventType.Cancel, asyncRequest);
						flag = true;
						break;
					}
					if (asyncRequest.hasError)
					{
						asyncRequest.status = AsyncRequestStatus.Error;
						Dispatch(AsyncRequestEventType.Error, asyncRequest);
						flag = true;
						break;
					}
					Dispatch((progress < 0f) ? AsyncRequestEventType.UploadProgress : AsyncRequestEventType.Progress, asyncRequest);
					if (asyncRequest.completed)
					{
						asyncRequest.status = AsyncRequestStatus.Pending;
						Dispatch(AsyncRequestEventType.Pending, asyncRequest);
					}
					break;
				case AsyncRequestStatus.Pending:
					if (asyncRequest.cancelled)
					{
						asyncRequest.status = AsyncRequestStatus.Cancelled;
						Dispatch(AsyncRequestEventType.Cancel, asyncRequest);
						flag = true;
					}
					else if (asyncRequest.hasError)
					{
						asyncRequest.status = AsyncRequestStatus.Error;
						Dispatch(AsyncRequestEventType.Error, asyncRequest);
						flag = true;
					}
					else
					{
						asyncRequest.Apply();
						asyncRequest.status = AsyncRequestStatus.Complete;
						Dispatch(AsyncRequestEventType.Complete, asyncRequest);
						flag = true;
					}
					break;
				}
				if (!flag)
				{
					continue;
				}
				if (asyncRequest is WebAsyncRequest)
				{
					WebAsyncRequest webAsyncRequest = asyncRequest as WebAsyncRequest;
					if (webAsyncRequest.data is byte[] && webAsyncRequest.loader != null)
					{
						webAsyncRequest.loader.Dispose();
					}
					if (webAsyncRequest.data is string && webAsyncRequest.loader != null)
					{
						webAsyncRequest.loader.Dispose();
					}
				}
				if (!asyncRequest.persistent)
				{
					requests.Remove(asyncRequest);
				}
			}
		}

		protected AsyncRequest Create(AsyncRequestType p_type, string p_id, string p_path, object p_data, Dictionary<string, string> p_headers, Type p_response_type)
		{
			AsyncRequest asyncRequest = null;
			asyncRequest = ((p_type != AsyncRequestType.BundleRead) ? ((AsyncRequest)new WebAsyncRequest()) : ((AsyncRequest)new BundleAsyncRequest()));
			if (asyncRequest == null)
			{
				return asyncRequest;
			}
			asyncRequest.id = p_id;
			asyncRequest.type = p_type;
			asyncRequest.status = AsyncRequestStatus.Created;
			asyncRequest.manager = this;
			asyncRequest.path = p_path;
			asyncRequest.Build(p_data, p_headers, p_response_type);
			requests.Add(asyncRequest);
			return asyncRequest;
		}

		protected void Dispatch(AsyncRequestEventType p_type, AsyncRequest p_req)
		{
			if (OnEvent != null)
			{
				AsyncRequestEvent asyncRequestEvent = new AsyncRequestEvent();
				asyncRequestEvent.type = p_type;
				asyncRequestEvent.target = p_req;
				OnEvent.Invoke(asyncRequestEvent);
			}
		}
	}
}
