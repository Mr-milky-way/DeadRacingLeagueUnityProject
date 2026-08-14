using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace thelab.core
{
	public class BundleAsyncRequest : AsyncRequest<AssetBundleCreateRequest>
	{
		public Stream stream;

		public override void Build(object p_data, Dictionary<string, string> p_headers, Type p_response_type)
		{
			AssetBundleCreateRequest assetBundleCreateRequest = (AssetBundleCreateRequest)p_data;
			loader = assetBundleCreateRequest;
		}

		protected override void OnApply()
		{
			loader.allowSceneActivation = true;
			AssetBundle assetBundle = Get<AssetBundle>();
			if (stream != null)
			{
				stream.Close();
				stream.Dispose();
				stream = null;
			}
			if ((bool)assetBundle)
			{
				assetBundle.GetAllScenePaths().ToString();
			}
		}

		protected override T OnGet<T>()
		{
			if (typeof(T) == typeof(AssetBundle))
			{
				return (T)(object)loader.assetBundle;
			}
			return default(T);
		}

		protected override string GetError()
		{
			try
			{
				Get<AssetBundle>();
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
			return "";
		}

		protected override float GetProgress()
		{
			if (loader != null)
			{
				return loader.progress;
			}
			return 1f;
		}

		protected override bool IsComplete()
		{
			if (loader != null)
			{
				return loader.isDone;
			}
			return true;
		}

		protected override bool IsValid()
		{
			return loader != null;
		}

		protected override void OnCancel()
		{
			loader = null;
		}
	}
}
