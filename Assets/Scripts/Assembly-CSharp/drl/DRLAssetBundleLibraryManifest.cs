using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using thelab.core;

namespace drl
{
	[Serializable]
	public class DRLAssetBundleLibraryManifest : Dictionary<string, object>
	{
		[SerializeField]
		private string m_name;

		[SerializeField]
		private List<string> m_guids;

		[SerializeField]
		private List<string> m_assets;

		[SerializeField]
		private List<string> m_groups;

		[SerializeField]
		private List<string> m_dependencies;

		[SerializeField]
		private List<List<int>> m_references;

		public string name
		{
			get
			{
				return m_name = (ContainsKey("name") ? ((string)base["name"]) : "");
			}
			set
			{
				base["name"] = (m_name = ((value == null) ? "" : value));
			}
		}

		public List<string> guids
		{
			get
			{
				return m_guids = Assert<List<string>>("guids");
			}
			set
			{
				base["guids"] = (m_guids = ((value == null) ? new List<string>() : value));
			}
		}

		public List<string> assets
		{
			get
			{
				return m_assets = Assert<List<string>>("assets");
			}
			set
			{
				base["assets"] = (m_assets = ((value == null) ? new List<string>() : value));
			}
		}

		public List<string> groups
		{
			get
			{
				return m_groups = Assert<List<string>>("groups");
			}
			set
			{
				base["groups"] = (m_groups = ((value == null) ? new List<string>() : value));
			}
		}

		public List<string> dependencies
		{
			get
			{
				return m_dependencies = Assert<List<string>>("dependencies");
			}
			set
			{
				base["dependencies"] = ((value == null) ? new List<string>() : value);
			}
		}

		public List<List<int>> references
		{
			get
			{
				return m_references = Assert<List<List<int>>>("references");
			}
			set
			{
				base["references"] = (m_references = ((value == null) ? new List<List<int>>() : value));
			}
		}

		public bool ContainsGUID(string p_v)
		{
			return guids.Contains(p_v);
		}

		public bool ContainsAsset(string p_v)
		{
			return assets.Contains(p_v);
		}

		public bool RegisterAsset(string p_guid, string p_name, string p_groups = "")
		{
			if (string.IsNullOrEmpty(p_guid) && string.IsNullOrEmpty(p_name))
			{
				Debug.LogWarning("DRLAssetBundleLibraryManifest> RegisterAsset / invalid params");
				return false;
			}
			string item = (string.IsNullOrEmpty(p_name) ? p_guid : p_name);
			string item2 = (string.IsNullOrEmpty(p_guid) ? p_name : p_guid);
			if (assets.Contains(item))
			{
				return false;
			}
			if (guids.Contains(item2))
			{
				return false;
			}
			assets.Add(item);
			guids.Add(item2);
			groups.Add(p_groups);
			AssertReferenceList();
			return true;
		}

		public void RegisterDependency(string p_guid, string p_dependency)
		{
			if (!guids.Contains(p_guid))
			{
				Debug.LogWarning("DRLAssetBundleLibraryManifest> RegisterDependency / GUID [" + p_guid + "] doesn't exists");
			}
			else if (!string.IsNullOrEmpty(p_dependency))
			{
				if (!dependencies.Contains(p_dependency))
				{
					dependencies.Add(p_dependency);
				}
				AssertReferenceList();
				int item = dependencies.IndexOf(p_dependency);
				int num = guids.IndexOf(p_guid);
				List<int> referenceList = GetReferenceList(num);
				if (!referenceList.Contains(item))
				{
					referenceList.Add(item);
				}
				referenceList.Sort();
				references[num] = referenceList;
			}
		}

		public void RegisterDependency(string p_guid, IList<string> p_dependencies)
		{
			for (int i = 0; i < p_dependencies.Count; i++)
			{
				RegisterDependency(p_guid, p_dependencies[i]);
			}
		}

		public string GetAssetName(string p_guid)
		{
			int num = guids.IndexOf(p_guid);
			if (num < 0)
			{
				return "";
			}
			return assets[num];
		}

		public string GetAssetGUID(string p_name)
		{
			int num = assets.IndexOf(p_name);
			if (num < 0)
			{
				return "";
			}
			return guids[num];
		}

		public string GetGroups(string p_guid)
		{
			int num = guids.IndexOf(p_guid);
			if (num < 0)
			{
				return "";
			}
			if (num >= groups.Count)
			{
				return "";
			}
			return groups[num];
		}

		public List<string> GetGroupList(string p_guids)
		{
			string[] array = GetGroups(p_guids).Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Trim();
			}
			return new List<string>(array);
		}

		public List<string> SearchBundleFilesFromPaths(IList<string> p_paths, IList<string> p_guids)
		{
			List<string> list = new List<string>();
			foreach (string p_path in p_paths)
			{
				list.AddRange(Directory.GetFiles(p_path, "*$*", SearchOption.TopDirectoryOnly));
			}
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = list[i].Replace("\\", "/");
			}
			return SearchBundleFiles(list, p_guids);
		}

		public List<string> SearchBundleFiles(IList<string> p_files, IList<string> p_guids)
		{
			List<string> list = new List<string>();
			bool[] array = new bool[Mathf.Max(guids.Count, dependencies.Count)];
			List<string> list2 = new List<string>(p_files);
			List<string> list3 = new List<string>(p_files);
			list3.RemoveAll((string v) => !v.Contains("$"));
			for (int num = 0; num < array.Length; num++)
			{
				array[num] = false;
			}
			for (int num2 = 0; num2 < p_guids.Count; num2++)
			{
				string text = p_guids[num2];
				int num3 = guids.IndexOf(text);
				if (num3 < 0 || array[num3])
				{
					continue;
				}
				array[num3] = true;
				for (int num4 = 0; num4 < list3.Count; num4++)
				{
					string text2 = list3[num4];
					if (text2.Contains(text))
					{
						list.Add(text2);
						break;
					}
				}
			}
			for (int num5 = 0; num5 < array.Length; num5++)
			{
				array[num5] = false;
			}
			for (int num6 = 0; num6 < p_guids.Count; num6++)
			{
				string item = p_guids[num6];
				int num3 = guids.IndexOf(item);
				List<int> referenceList = GetReferenceList(num3);
				for (int num7 = 0; num7 < referenceList.Count; num7++)
				{
					int num8 = referenceList[num7];
					if (array[num8])
					{
						continue;
					}
					array[num8] = true;
					string text3 = GetDependency(num8);
					if (text3.Contains("dependency-"))
					{
						text3 = text3.ToLower();
					}
					string text4 = "";
					foreach (string item2 in list2)
					{
						if (item2.Contains(text3))
						{
							text4 = item2;
							break;
						}
					}
					if (!string.IsNullOrEmpty(text4) && !list.Contains(text4))
					{
						list.Add(text4);
					}
				}
			}
			return list;
		}

		public List<string> GetBundleFiles(string p_path, IList<string> p_guids)
		{
			return SearchBundleFilesFromPaths(new string[1] { p_path }, p_guids);
		}

		public List<string> GetBundleFiles(string p_path, string p_guid)
		{
			return SearchBundleFilesFromPaths(new string[1] { p_path }, new string[1] { p_guid });
		}

		public List<string> GetBundleFiles(IList<string> p_paths, string p_guid)
		{
			return SearchBundleFilesFromPaths(p_paths, new string[1] { p_guid });
		}

		public void Load(Dictionary<string, object> p_data)
		{
			Clear();
			foreach (KeyValuePair<string, object> p_datum in p_data)
			{
				base[p_datum.Key] = p_datum.Value;
			}
		}

		public void Load(DRLAssetBundleLibraryManifest p_data)
		{
			Load((Dictionary<string, object>)p_data);
		}

		public void Load(byte[] p_data)
		{
			Dictionary<string, object> dictionary = null;
			try
			{
				dictionary = Serialize.FromBytes<Dictionary<string, object>>(p_data);
			}
			catch (Exception)
			{
			}
			if (dictionary == null)
			{
				try
				{
					dictionary = Serialize.FromBytes<DRLAssetBundleLibraryManifest>(p_data);
				}
				catch (Exception)
				{
				}
			}
			if (dictionary != null)
			{
				Load(dictionary);
			}
		}

		public void LoadFromFile(string p_path)
		{
			if (File.Exists(p_path))
			{
				byte[] p_data = File.ReadAllBytes(p_path);
				Load(p_data);
			}
		}

		public byte[] ToBytes()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, object> current = enumerator.Current;
					dictionary[current.Key] = current.Value;
				}
			}
			return Serialize.ToBytes(dictionary);
		}

		public string ToJson()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, object> current = enumerator.Current;
					dictionary[current.Key] = current.Value;
				}
			}
			return Serialize.ToJson(dictionary, p_indented: true);
		}

		public void Save(string p_path, bool p_ovewrite, bool p_json = false)
		{
			if (File.Exists(p_path) && !p_ovewrite)
			{
				Debug.LogWarning("DRLAssetBundleLibraryManifest> File [" + p_path + "] already exists.");
				return;
			}
			if (File.Exists(p_path) && p_ovewrite)
			{
				File.Delete(p_path);
			}
			if (p_json)
			{
				File.WriteAllText(p_path, ToJson());
			}
			else
			{
				File.WriteAllBytes(p_path, ToBytes());
			}
		}

		protected T Assert<T>(string k) where T : new()
		{
			if (ContainsKey(k))
			{
				return (T)base[k];
			}
			T val = new T();
			base[k] = val;
			return val;
		}

		public string GetDependency(int p_index)
		{
			if (p_index < 0)
			{
				return "";
			}
			if (p_index >= dependencies.Count)
			{
				return "";
			}
			return dependencies[p_index];
		}

		public List<int> GetReferenceList(int p_index)
		{
			if (p_index < 0)
			{
				return new List<int>();
			}
			if (p_index >= references.Count)
			{
				return new List<int>();
			}
			return references[p_index];
		}

		protected void AssertReferenceList()
		{
			List<List<int>> list = references;
			while (list.Count < guids.Count)
			{
				list.Add(new List<int>());
			}
		}
	}
}
