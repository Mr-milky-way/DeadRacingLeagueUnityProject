using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class AssetBundleViewer : MonoBehaviour
	{
		public TextAsset file;

		public AssetBundle bundle;

		public List<string> scenes;

		public string selected;
	}
}
