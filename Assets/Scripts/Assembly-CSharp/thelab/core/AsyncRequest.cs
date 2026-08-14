using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class AsyncRequest
	{
		public string id;

		public bool persistent;

		public AsyncRequestType type;

		public AsyncRequestStatus status;

		public AsyncManager manager;

		public string path = "";

		[SerializeField]
		[TextArea(5, 20)]
		protected string m_error;

		public bool cancelled;

		public bool started;

		public bool valid => IsValid();

		public float progress
		{
			get
			{
				if (completed)
				{
					return 1f;
				}
				if (valid)
				{
					return GetProgress() * 0.9999f;
				}
				return 1f;
			}
		}

		public bool completed
		{
			get
			{
				if (valid)
				{
					return IsComplete();
				}
				return true;
			}
		}

		public string error
		{
			get
			{
				if (!valid)
				{
					return "AsyncRequest - Invalid Request.";
				}
				if (!completed)
				{
					return "";
				}
				return GetError();
			}
			internal set
			{
				m_error = value;
			}
		}

		public bool hasError => !string.IsNullOrEmpty(error);

		public virtual void Build(object p_data, Dictionary<string, string> p_headers, Type p_response_type)
		{
		}

		public bool IsBundle()
		{
			if (type != AsyncRequestType.BundleLoad)
			{
				return type == AsyncRequestType.BundleRead;
			}
			return true;
		}

		public bool IsWeb()
		{
			return (type & AsyncRequestType.Web) != 0;
		}

		public void Cancel()
		{
			if (!cancelled)
			{
				cancelled = true;
				OnCancel();
			}
		}

		public void Timeout()
		{
			Cancel();
			m_error = "AsyncRequest - Timeout";
		}

		public void Apply()
		{
			if (valid && completed && !cancelled && !hasError)
			{
				OnApply();
			}
		}

		protected virtual void OnApply()
		{
		}

		protected virtual string GetError()
		{
			return "Invalid Request - Must extend this class.";
		}

		protected virtual bool IsComplete()
		{
			return false;
		}

		protected virtual bool IsValid()
		{
			return false;
		}

		protected virtual float GetProgress()
		{
			return 0f;
		}

		protected virtual void OnCancel()
		{
		}

		protected virtual T OnGet<T>()
		{
			return default(T);
		}

		public T Get<T>()
		{
			T result = default(T);
			try
			{
				result = OnGet<T>();
				return result;
			}
			catch (Exception ex)
			{
				Debug.LogError("AsyncRequest> Failed to Get [" + typeof(T).Name + "] from [" + id + "]\n" + ex.Message);
			}
			return result;
		}
	}
	public class AsyncRequest<T> : AsyncRequest
	{
		public T loader;
	}
}
