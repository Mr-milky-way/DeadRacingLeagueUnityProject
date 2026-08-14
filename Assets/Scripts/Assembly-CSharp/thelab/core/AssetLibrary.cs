using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class AssetLibrary : UniqueAsset
	{
		[Serializable]
		public class External
		{
			public AssetBundle bundle;

			public string resource;

			[SerializeField]
			private List<AssetLibrary> m_list;

			public bool isResource => !string.IsNullOrEmpty(resource);

			public bool isBundle => bundle != null;

			public List<AssetLibrary> list
			{
				get
				{
					if (m_list != null)
					{
						return m_list;
					}
					return m_list = new List<AssetLibrary>();
				}
			}
		}

		private static Transform m_instance_container;

		[SerializeField]
		private List<GameObject> m_assets;

		[SerializeField]
		private List<External> m_externals;

		protected static Transform instance_container
		{
			get
			{
				if ((bool)m_instance_container)
				{
					return m_instance_container;
				}
				m_instance_container = new GameObject("$asset-library").transform;
				return m_instance_container;
			}
		}

		public List<GameObject> assets
		{
			get
			{
				if (m_assets != null)
				{
					return m_assets;
				}
				return m_assets = new List<GameObject>();
			}
		}

		public List<string> assetGUIDs => assets.FindAll((GameObject it) => it.GetComponent<UniqueAsset>() != null).ConvertAll((GameObject it) => it.GetComponent<UniqueAsset>().guid);

		public List<External> externals
		{
			get
			{
				if (m_externals != null)
				{
					return m_externals;
				}
				return m_externals = new List<External>();
			}
		}

		public static List<T> FindAll<T>(AssetLibrary p_library, Predicate<T> p_callback, int p_max = 0) where T : Component
		{
			List<T> list = new List<T>();
			if (!p_library)
			{
				return list;
			}
			List<GameObject> list2 = p_library.assets;
			if (p_callback == null)
			{
				return list;
			}
			for (int i = 0; i < list2.Count; i++)
			{
				if (p_max > 0 && list.Count >= p_max)
				{
					break;
				}
				GameObject gameObject = list2[i];
				if (!gameObject)
				{
					Debug.LogWarning("AssetLibrary> Null Element at [" + i + "] library[" + p_library?.ToString() + "]");
					continue;
				}
				T[] components = gameObject.GetComponents<T>();
				for (int j = 0; j < components.Length; j++)
				{
					if (p_max > 0 && list.Count >= p_max)
					{
						break;
					}
					if (p_callback(components[j]))
					{
						list.Add(components[j]);
					}
				}
				AssetLibrary[] components2 = gameObject.GetComponents<AssetLibrary>();
				for (int k = 0; k < components2.Length; k++)
				{
					if (p_max > 0 && list.Count >= p_max)
					{
						break;
					}
					int p_max2 = Mathf.Max(0, p_max - list.Count);
					List<T> list3 = FindAll(components2[k], p_callback, p_max2);
					for (int l = 0; l < list3.Count; l++)
					{
						if (p_max > 0 && list.Count >= p_max)
						{
							break;
						}
						list.Add(list3[l]);
					}
				}
			}
			List<External> list4 = p_library.externals;
			List<T> list5 = new List<T>();
			for (int m = 0; m < list4.Count; m++)
			{
				if (p_max > 0 && list.Count >= p_max)
				{
					break;
				}
				External external = list4[m];
				for (int n = 0; n < external.list.Count; n++)
				{
					if (p_max > 0 && list.Count >= p_max)
					{
						break;
					}
					int p_max3 = Mathf.Max(0, p_max - list.Count);
					list5 = FindAll(external.list[n], p_callback, p_max3);
					list.AddRange(list5);
				}
			}
			return list;
		}

		public bool Contains(string p_guid)
		{
			return Find((UniqueAsset it) => it.guid == p_guid) != null;
		}

		public bool Contains(IList<string> p_guids)
		{
			List<string> list = assetGUIDs;
			for (int i = 0; i < p_guids.Count; i++)
			{
				string item = p_guids[i];
				if (!list.Contains(item))
				{
					return false;
				}
			}
			return true;
		}

		public List<T> FindAll<T>(int p_max = 0) where T : Component
		{
			return FindAll((T it) => true, p_max);
		}

		public List<T> FindAll<T>(Predicate<T> p_callback, int p_max = 0) where T : Component
		{
			return FindAll(this, p_callback, p_max);
		}

		public T Find<T>(Predicate<T> p_callback) where T : Component
		{
			List<T> list = FindAll(p_callback, 1);
			if (list.Count > 0)
			{
				return list[0];
			}
			return null;
		}

		public T FindByGUID<T>(string p_guid) where T : Component
		{
			UniqueAsset uniqueAsset = Find((UniqueAsset it) => it.guid == p_guid);
			if (!uniqueAsset)
			{
				return null;
			}
			if (uniqueAsset.Is<T>())
			{
				return (T)(Component)uniqueAsset;
			}
			return uniqueAsset.GetComponent<T>();
		}

		public List<T> FindByGUID<T>(List<string> p_guids) where T : Component
		{
			List<T> res = new List<T>();
			FindAll(delegate(UniqueAsset it)
			{
				if (!p_guids.Contains(it.guid))
				{
					return false;
				}
				if (!(it is T item))
				{
					return false;
				}
				if (res.Contains(item))
				{
					return false;
				}
				for (int i = 0; i < res.Count; i++)
				{
					UniqueAsset uniqueAsset = res[i] as UniqueAsset;
					if ((bool)uniqueAsset && uniqueAsset.guid == it.guid)
					{
						return false;
					}
				}
				res.Add(item);
				return false;
			});
			return res;
		}

		public List<T> FindByTags<T>(bool p_exact, params string[] p_tags) where T : Component
		{
			List<StringTag> list = FindAll((StringTag it) => it.Match(p_exact, p_tags));
			List<T> list2 = new List<T>();
			for (int num = 0; num < list.Count; num++)
			{
				T component = list[num].GetComponent<T>();
				if ((bool)component)
				{
					list2.Add(component);
				}
			}
			return list2;
		}

		public List<T> FindByTags<T>(params string[] p_tags) where T : Component
		{
			return FindByTags<T>(p_exact: false, p_tags);
		}

		public T Find<T>(string p_id = "") where T : Component
		{
			T val = Find((T it) => string.IsNullOrEmpty(p_id) || it.name == p_id);
			if (!val)
			{
				return null;
			}
			if (Reflection<object>.InheritFrom<T>(val.GetType()))
			{
				return (T)val;
			}
			return val.GetComponent<T>();
		}

		public void Add(GameObject p_item)
		{
			if ((bool)p_item && !assets.Contains(p_item))
			{
				assets.Add(p_item);
			}
		}

		public void Add(Component p_item)
		{
			Add(p_item ? p_item.gameObject : null);
		}

		public void Remove(GameObject p_item)
		{
			if ((bool)p_item && assets.Contains(p_item))
			{
				assets.Remove(p_item);
			}
		}

		public void Remove(Component p_item)
		{
			Remove(p_item ? p_item.gameObject : null);
		}

		public T InstantiateByGUID<T>(string p_guid, Transform p_parent) where T : Component
		{
			T val = FindByGUID<T>(p_guid);
			if (!val)
			{
				return null;
			}
			return UnityEngine.Object.Instantiate(val, p_parent);
		}

		public T InstantiateByGUID<T>(string p_guid) where T : Component
		{
			T val = FindByGUID<T>(p_guid);
			if (!val)
			{
				return null;
			}
			return UnityEngine.Object.Instantiate(val);
		}

		public T Instantiate<T>(string p_id, Transform p_parent) where T : Component
		{
			T val = Find<T>(p_id);
			if (!val)
			{
				return null;
			}
			return UnityEngine.Object.Instantiate(val, p_parent);
		}

		public T Instantiate<T>(string p_id) where T : Component
		{
			T val = Find<T>(p_id);
			if (!val)
			{
				return null;
			}
			return UnityEngine.Object.Instantiate(val);
		}

		protected External GetExternalLibrary(AssetBundle b, string r)
		{
			if (!b && string.IsNullOrEmpty(r))
			{
				return null;
			}
			for (int i = 0; i < externals.Count; i++)
			{
				External external = externals[i];
				if (external.bundle == b)
				{
					return external;
				}
				if (external.resource == r)
				{
					return external;
				}
			}
			return null;
		}

		public External GetExternalLibrary(AssetBundle p_bundle)
		{
			return GetExternalLibrary(p_bundle, "");
		}

		public External GetExternalLibrary(string p_resource)
		{
			return GetExternalLibrary(null, p_resource);
		}

		protected void UnloadExternal(External p_external)
		{
			if (p_external == null)
			{
				return;
			}
			externals.Remove(p_external);
			int k = 0;
			Activity.Run((Func<bool>)delegate
			{
				UnityEngine.Object.Destroy(p_external.list[k++].gameObject);
				if (k >= p_external.list.Count)
				{
					p_external.list.Clear();
					if (p_external.isResource)
					{
						Resources.UnloadUnusedAssets();
					}
					if (p_external.isBundle)
					{
						p_external.bundle.Unload(unloadAllLoadedObjects: true);
					}
					return false;
				}
				return true;
			}, 0f, false);
		}

		public void UnloadExternal(AssetBundle p_bundle)
		{
			UnloadExternal(GetExternalLibrary(p_bundle));
		}

		public void UnloadExternal(string p_resource)
		{
			UnloadExternal(GetExternalLibrary(p_resource));
		}

		public bool ContainsExternal(AssetBundle p_bundle)
		{
			return GetExternalLibrary(p_bundle) != null;
		}

		public bool ContainsExternal(string p_resource)
		{
			return GetExternalLibrary(p_resource) != null;
		}

		public External LoadResource(string p_resource, bool p_async = false, Action p_on_complete = null)
		{
			if (string.IsNullOrEmpty(p_resource))
			{
				return null;
			}
			External bl = GetExternalLibrary(p_resource);
			if (bl != null)
			{
				return bl;
			}
			bl = new External();
			bl.resource = p_resource;
			AssetLibrary lib = null;
			if (p_async)
			{
				ResourceRequest req = Resources.LoadAsync<AssetLibrary>(p_resource);
				Activity.Run((Func<bool>)delegate
				{
					if (!req.isDone)
					{
						return true;
					}
					lib = (AssetLibrary)req.asset;
					if (!lib)
					{
						return false;
					}
					AssetLibrary assetLibrary2 = UnityEngine.Object.Instantiate(lib, instance_container);
					assetLibrary2.name = assetLibrary2.name.Replace("(Clone)", "") + "-instance";
					bl.list.Add(assetLibrary2);
					Resources.UnloadUnusedAssets();
					return false;
				}, 0f, false);
			}
			else
			{
				lib = Resources.Load<AssetLibrary>(p_resource);
				AssetLibrary assetLibrary = UnityEngine.Object.Instantiate(lib, instance_container);
				assetLibrary.name = assetLibrary.name.Replace("(Clone)", "") + "-instance";
				bl.list.Add(assetLibrary);
				Resources.UnloadUnusedAssets();
			}
			externals.Add(bl);
			return bl;
		}
	}
}
