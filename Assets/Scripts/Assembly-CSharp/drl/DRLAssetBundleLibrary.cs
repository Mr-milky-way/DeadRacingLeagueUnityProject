using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using thelab.core;

namespace drl
{
	public class DRLAssetBundleLibrary : AssetLibrary
	{
		public List<DRLAssetBundleLibraryManifest> manifests;

		public List<AssetBundle> dependencies;

		public List<string> bundlePaths;

		public List<string> bundleFiles;

		public uint batchLoadSize = 2u;

		public bool batchLoadAsync = true;

		public List<string> dependencyFilter;

		public List<string> guidFilter;

		public Dictionary<string, List<string>> dependencyCache;

		private Activity m_bundle_req_loop;

		public List<string> GetGUIDs()
		{
			List<string> list = new List<string>();
			for (int i = 0; i < manifests.Count; i++)
			{
				list.AddRange(manifests[i].guids);
			}
			return list;
		}

		public List<string> GetGroups(string p_guid)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < manifests.Count; i++)
			{
				list.AddRange(manifests[i].GetGroupList(p_guid));
			}
			return list;
		}

		public List<string> GetDependencies()
		{
			List<string> list = new List<string>();
			for (int i = 0; i < manifests.Count; i++)
			{
				list.AddRange(manifests[i].dependencies);
			}
			return list;
		}

		public List<string> GetBundleFiles(IList<string> p_guids, bool p_use_cache = true)
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>(p_guids);
			if (p_use_cache)
			{
				for (int i = 0; i < list2.Count; i++)
				{
					string key = list2[i];
					if (!dependencyCache.ContainsKey(key))
					{
						continue;
					}
					List<string> list3 = dependencyCache[key];
					for (int j = 0; j < list3.Count; j++)
					{
						if (!list.Contains(list3[j]))
						{
							list.Add(list3[j]);
						}
					}
					list2.RemoveAt(i--);
				}
			}
			for (int k = 0; k < manifests.Count; k++)
			{
				list.AddRange(manifests[k].SearchBundleFilesFromPaths(bundlePaths, list2));
				list.AddRange(manifests[k].SearchBundleFiles(bundleFiles, list2));
			}
			return list;
		}

		public List<string> GetBundleFiles(string p_guid)
		{
			return GetBundleFiles(new string[1] { p_guid });
		}

		public void LoadManifests(IList<string> p_paths, bool p_cache = false)
		{
			for (int i = 0; i < p_paths.Count; i++)
			{
				string text = p_paths[i];
				bool flag = Directory.Exists(text);
				FileInfo fileInfo = (flag ? null : new FileInfo(text));
				if (!flag)
				{
					_ = fileInfo.Directory.FullName;
				}
				List<string> list = new List<string>();
				list.AddRange(flag ? Directory.GetFiles(text ?? "", "manifest*.bytes") : new string[1] { text });
				list.AddRange(flag ? Directory.GetFiles(text ?? "", "manifest*.ablm") : new string[1] { text });
				for (int j = 0; j < list.Count; j++)
				{
					for (int k = j + 1; k < list.Count; k++)
					{
						if (list[j] == list[k])
						{
							list.RemoveAt(k--);
						}
					}
				}
				for (int l = 0; l < list.Count; l++)
				{
					string text2 = list[l];
					if (!File.Exists(text2))
					{
						Debug.LogWarning("DRLAssetBundleLibrary> Manifest [" + text2 + "] not found!");
						continue;
					}
					DRLAssetBundleLibraryManifest dRLAssetBundleLibraryManifest = null;
					FileInfo fileInfo2 = new FileInfo(text2);
					switch (fileInfo2.Extension.ToLower())
					{
					case ".bytes":
						dRLAssetBundleLibraryManifest = RegisterAssetBundleManifest(text2);
						break;
					case ".ablm":
						dRLAssetBundleLibraryManifest = RegisterABLM(text2);
						break;
					}
					if (dRLAssetBundleLibraryManifest != null)
					{
						dRLAssetBundleLibraryManifest.name = fileInfo2.Name.Replace(fileInfo2.Extension, "");
					}
				}
			}
			dependencyCache = new Dictionary<string, List<string>>();
			if (p_cache)
			{
				RefreshGUIDDependencyCache();
			}
			Activity.RunOnce(delegate
			{
				Debug.Log($"DRLAssetBundleLibrary> [{base.name}] Fetch Manifests / count[{manifests.Count}]");
			});
		}

		public void LoadManifests(string p_path)
		{
			LoadManifests(new string[1] { p_path });
		}

		public void LoadCache(Dictionary<string, List<string>> p_cache)
		{
			dependencyCache = new Dictionary<string, List<string>>();
			foreach (string item in new List<string>(p_cache.Keys))
			{
				dependencyCache[item] = new List<string>(p_cache[item]);
			}
		}

		protected DRLAssetBundleLibraryManifest RegisterAssetBundleManifest(string p_file_path)
		{
			AssetBundle assetBundle = AssetBundle.LoadFromFile(p_file_path);
			AssetBundleManifest assetBundleManifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
			if (!assetBundleManifest)
			{
				return null;
			}
			FileInfo fileInfo = new FileInfo(p_file_path);
			assetBundleManifest = UnityEngine.Object.Instantiate(assetBundleManifest);
			assetBundleManifest.name = fileInfo.Name.Replace(fileInfo.Extension, "");
			assetBundle.Unload(unloadAllLoadedObjects: true);
			List<string> list = new List<string>(assetBundleManifest.GetAllAssetBundles());
			DRLAssetBundleLibraryManifest dRLAssetBundleLibraryManifest = new DRLAssetBundleLibraryManifest();
			for (int i = 0; i < list.Count; i++)
			{
				string p_target = list[i];
				string p_guid = "";
				string p_name = "";
				GetGUIDName(p_target, out p_guid, out p_name);
				dRLAssetBundleLibraryManifest.RegisterAsset(p_guid, p_name);
				string[] allDependencies = assetBundleManifest.GetAllDependencies(p_name);
				dRLAssetBundleLibraryManifest.RegisterDependency(p_guid, allDependencies);
			}
			manifests.Add(dRLAssetBundleLibraryManifest);
			return dRLAssetBundleLibraryManifest;
		}

		protected DRLAssetBundleLibraryManifest RegisterABLM(string p_file_path)
		{
			DRLAssetBundleLibraryManifest dRLAssetBundleLibraryManifest = new DRLAssetBundleLibraryManifest();
			dRLAssetBundleLibraryManifest.LoadFromFile(p_file_path);
			manifests.Add(dRLAssetBundleLibraryManifest);
			return dRLAssetBundleLibraryManifest;
		}

		protected void RefreshGUIDDependencyCache()
		{
			if (dependencyCache != null)
			{
				dependencyCache.Clear();
			}
			dependencyCache = new Dictionary<string, List<string>>();
			List<string> list = new List<string>();
			foreach (string bundlePath in bundlePaths)
			{
				if (Directory.Exists(bundlePath))
				{
					list.AddRange(Directory.GetFiles(bundlePath, "*$*", SearchOption.TopDirectoryOnly));
				}
			}
			foreach (string bundleFile in bundleFiles)
			{
				if (bundleFile.Contains("$"))
				{
					list.Add(bundleFile);
				}
			}
			for (int i = 0; i < manifests.Count; i++)
			{
				List<string> guids = manifests[i].guids;
				for (int j = 0; j < guids.Count; j++)
				{
					string text = guids[j];
					List<int> referenceList = manifests[i].GetReferenceList(j);
					List<string> list2 = (dependencyCache.ContainsKey(text) ? dependencyCache[text] : new List<string>());
					for (int k = 0; k < list.Count; k++)
					{
						string text2 = list[k];
						if (text2.Contains(text))
						{
							list2.Add(text2);
							break;
						}
					}
					for (int l = 0; l < referenceList.Count; l++)
					{
						int p_index = referenceList[l];
						string text3 = manifests[i].GetDependency(p_index);
						if (text3.Contains("dependency-"))
						{
							text3 = text3.ToLower();
						}
						string text4 = "";
						foreach (string bundlePath2 in bundlePaths)
						{
							string text5 = bundlePath2 + text3 + ".bytes";
							if (File.Exists(text5))
							{
								text4 = text5;
								break;
							}
						}
						if (string.IsNullOrEmpty(text4))
						{
							foreach (string bundleFile2 in bundleFiles)
							{
								if (bundleFile2.Contains(text3) && File.Exists(bundleFile2))
								{
									text4 = bundleFile2;
									break;
								}
							}
						}
						if (!string.IsNullOrEmpty(text4) && !list2.Contains(text4))
						{
							list2.Add(text4);
						}
					}
					dependencyCache[text] = list2;
				}
			}
		}

		public void LoadAssetsAsync(IList<string> p_guids, bool p_include_dependency, Action<float> p_on_status = null)
		{
			List<string> list = new List<string>(p_guids);
			if (guidFilter.Count > 0)
			{
				list.RemoveAll(delegate(string it)
				{
					for (int i = 0; i < guidFilter.Count; i++)
					{
						string value2 = guidFilter[i];
						if (!string.IsNullOrEmpty(value2) && it.Contains(value2))
						{
							return false;
						}
					}
					return true;
				});
			}
			for (int num = 0; num < list.Count; num++)
			{
				string text = list[num];
				if (string.IsNullOrEmpty(text))
				{
					list.RemoveAt(num--);
				}
				else if (Contains(text) && !p_include_dependency)
				{
					Debug.Log("DRLAssetBundleLibrary> LoadAssetsAsync / Asset [" + text + "] already exists!");
					list.RemoveAt(num--);
				}
			}
			List<string> list2 = GetBundleFiles(list);
			List<string> b_dependencies = GetDependencies();
			if (!p_include_dependency)
			{
				list2.RemoveAll(delegate(string it)
				{
					for (int i = 0; i < b_dependencies.Count; i++)
					{
						if (it.Contains(b_dependencies[i]))
						{
							return true;
						}
					}
					return false;
				});
			}
			if (dependencyFilter.Count > 0)
			{
				list2.RemoveAll(delegate(string it)
				{
					for (int i = 0; i < dependencyFilter.Count; i++)
					{
						string value2 = dependencyFilter[i];
						if (!string.IsNullOrEmpty(value2) && it.Contains(value2))
						{
							return false;
						}
					}
					return true;
				});
			}
			b_dependencies.Clear();
			list2.Sort(delegate(string a, string b)
			{
				bool flag = a.Contains("dependency-");
				bool flag2 = b.Contains("dependency-");
				long length = new FileInfo(a).Length;
				long length2 = new FileInfo(b).Length;
				if (flag && flag2)
				{
					if (length <= length2)
					{
						return 1;
					}
					return -1;
				}
				if (!flag && !flag2)
				{
					if (length <= length2)
					{
						return 1;
					}
					return -1;
				}
				if (flag)
				{
					return -1;
				}
				if (flag2)
				{
					return 1;
				}
				return (length <= length2) ? 1 : (-1);
			});
			if (p_on_status != null)
			{
				p_on_status(0f);
			}
			int batch_length = (int)batchLoadSize;
			float batch_count = 0f;
			float batch_index = 0f;
			int batch_state = 0;
			int frames_cooldown = 0;
			List<List<string>> batch_contexts = new List<List<string>>();
			string[] array = new string[5] { "-tex", "-mat", "-mdl", "-msv", "" };
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				List<string> list3 = new List<string>();
				string value = array[num2];
				for (int num3 = 0; num3 < list2.Count; num3++)
				{
					string text2 = list2[num3];
					if (string.IsNullOrEmpty(value) || text2.Contains(value))
					{
						list2.RemoveAt(num3--);
						list3.Add(text2);
					}
				}
				batch_count += (float)(list3.Count / batch_length) + 1f;
				batch_contexts.Add(list3);
			}
			List<string> batch_buffer = new List<string>();
			List<string> batch_dependencies = null;
			Activity.Run((Func<bool>)delegate
			{
				switch (batch_state)
				{
				case 0:
					while (batch_contexts.Count > 0)
					{
						batch_dependencies = batch_contexts[0];
						if (batch_dependencies.Count > 0)
						{
							break;
						}
						batch_contexts.RemoveAt(0);
					}
					if (batch_contexts.Count <= 0)
					{
						batch_state = 5;
					}
					else
					{
						batch_state = 1;
					}
					break;
				case 1:
					if (batch_dependencies.Count <= 0)
					{
						batch_state = 0;
					}
					else
					{
						int count = Mathf.Min(batch_dependencies.Count, batch_length);
						batch_buffer.Clear();
						batch_buffer.AddRange(batch_dependencies.GetRange(0, count));
						batch_dependencies.RemoveRange(0, count);
						batch_state = 2;
					}
					break;
				case 2:
					batch_state = 3;
					LoadAssetsAsyncBatch(batch_buffer, delegate(float p_batch_progress)
					{
						float num4 = Mathf.Clamp01((batch_index + p_batch_progress) / batch_count);
						if (p_on_status != null)
						{
							p_on_status(num4 * 0.99f);
						}
						if (p_batch_progress >= 1f)
						{
							batch_index += 1f;
							batch_state = 4;
							int num5 = ((batch_dependencies != null) ? batch_dependencies.Count : 0);
							frames_cooldown = ((num5 > 0) ? 1 : 3);
						}
					});
					break;
				case 4:
					frames_cooldown--;
					if (frames_cooldown > 0)
					{
						return true;
					}
					batch_state = 0;
					break;
				case 5:
					if (p_on_status != null)
					{
						p_on_status(1f);
					}
					return false;
				}
				return true;
			}, 0f, false);
		}

		public void LoadAssetsAsync(IList<string> p_guids, Action<float> p_on_status = null)
		{
			LoadAssetsAsync(p_guids, p_include_dependency: true, p_on_status);
		}

		protected void LoadAssetsAsyncBatch(IList<string> p_dependency_files, Action<float> p_on_status = null)
		{
			List<string> list = new List<string>(p_dependency_files);
			List<string> b_dependencies_names = new List<string>();
			List<AssetBundle> b_dependencies = new List<AssetBundle>();
			List<AssetBundleCreateRequest> b_req_bundle_list = new List<AssetBundleCreateRequest>();
			List<AssetBundleRequest> b_req_asset_list = new List<AssetBundleRequest>();
			string log = "";
			List<AssetBundle> list2 = new List<AssetBundle>(AssetBundle.GetAllLoadedAssetBundles());
			list2.ConvertAll((AssetBundle it) => (!it) ? "<null>" : it.name);
			log += " Manifest\n";
			for (int num = 0; num < list.Count; num++)
			{
				string text = list[num];
				FileInfo fileInfo = new FileInfo(text);
				string bn = fileInfo.Name.Replace(fileInfo.Extension, "");
				bool num2 = File.Exists(text);
				AssetBundleCreateRequest item = null;
				AssetBundle assetBundle = (num2 ? GetDependency(text) : null);
				AssetBundle assetBundle2 = (num2 ? list2.Find((AssetBundle itb) => (bool)itb && itb.name == bn) : null);
				string arg = ((!num2) ? "E" : (assetBundle ? "D" : (assetBundle2 ? "B" : "N")));
				bool flag = (bool)assetBundle || (bool)assetBundle2;
				if (!flag)
				{
					item = AssetBundle.LoadFromFileAsync(text);
				}
				b_req_bundle_list.Add(item);
				b_dependencies.Add(null);
				b_dependencies_names.Add(flag ? "" : bn);
				log += $"  [{num}] [{arg}] {text}\n";
			}
			if (p_on_status != null)
			{
				p_on_status(0f);
			}
			float previous_progress = 0f;
			int load_state = 0;
			float progress_offset = 0f;
			float progress_fraction = 0.333f;
			IList current_request_list;
			Activity.Run((Func<bool>)delegate
			{
				IList list4;
				if (load_state != 0)
				{
					IList list3 = b_req_asset_list;
					list4 = list3;
				}
				else
				{
					IList list3 = b_req_bundle_list;
					list4 = list3;
				}
				current_request_list = list4;
				float progression = GetProgression(b_req_bundle_list);
				float num3 = progress_offset + Mathf.Clamp(progression * 0.99f, 0.01f, 0.99f) * progress_fraction;
				if (Mathf.Abs(num3 - previous_progress) > 0f)
				{
					previous_progress = num3;
					if (p_on_status != null)
					{
						p_on_status(num3 * 0.999f);
					}
				}
				bool flag2 = IsComplete(current_request_list);
				switch (load_state)
				{
				case 0:
				{
					if (!flag2)
					{
						return true;
					}
					List<AssetBundleCreateRequest> list5 = (List<AssetBundleCreateRequest>)current_request_list;
					log = "";
					for (int i = 0; i < list5.Count; i++)
					{
						AssetBundleCreateRequest assetBundleCreateRequest = list5[i];
						AssetBundle assetBundle3 = ((assetBundleCreateRequest == null) ? b_dependencies[i] : assetBundleCreateRequest.assetBundle);
						if (!assetBundle3)
						{
							b_req_asset_list.Add(null);
						}
						else
						{
							assetBundle3.name = b_dependencies_names[i];
							AssetBundleRequest item2 = (batchLoadAsync ? assetBundle3.LoadAllAssetsAsync() : null);
							b_req_asset_list.Add(item2);
							if (!dependencies.Contains(assetBundle3))
							{
								dependencies.Add(assetBundle3);
							}
							b_dependencies[i] = assetBundle3;
						}
					}
					progress_offset += progress_fraction;
					load_state = 1;
					break;
				}
				case 1:
				{
					if (!flag2)
					{
						return true;
					}
					List<AssetBundleRequest> list6 = (List<AssetBundleRequest>)current_request_list;
					log = "";
					for (int j = 0; j < list6.Count; j++)
					{
						AssetBundleRequest assetBundleRequest = list6[j];
						_ = b_dependencies_names[j];
						AssetBundle assetBundle4 = b_dependencies[j];
						_ = assetBundle4 == null;
						string text2 = (assetBundle4 ? assetBundle4.name : "");
						UnityEngine.Object[] array = ((assetBundleRequest != null) ? assetBundleRequest.allAssets : (assetBundle4 ? assetBundle4.LoadAllAssets() : null));
						log += string.Format("  [{0}][{1}][{2}][{3}]\n", j, assetBundle4 ? " " : "*", (array == null) ? "-" : array.Length.ToString(), text2);
						if (array == null)
						{
							array = new UnityEngine.Object[0];
						}
						if (text2.Contains("$"))
						{
							foreach (UnityEngine.Object obj in array)
							{
								if (obj is GameObject)
								{
									UniqueAsset component = (obj as GameObject).GetComponent<UniqueAsset>();
									if ((bool)component && !base.assets.Contains(component.gameObject))
									{
										base.assets.Add(component.gameObject);
									}
								}
							}
						}
					}
					progress_offset += progress_fraction;
					load_state = 2;
					break;
				}
				case 2:
					if (p_on_status != null)
					{
						p_on_status(1f);
					}
					return false;
				}
				return true;
			}, 0f, false);
		}

		public void UnloadLibrary(bool p_forceImmediate = false)
		{
			List<UnityEngine.Object> list = new List<UnityEngine.Object>();
			list.AddRange(base.assets);
			for (int i = 0; i < list.Count; i++)
			{
				if (p_forceImmediate)
				{
					UnityEngine.Object.DestroyImmediate((GameObject)list[i], allowDestroyingAssets: true);
				}
				else
				{
					UnityEngine.Object.Destroy((GameObject)list[i]);
				}
			}
			base.assets.Clear();
			UnloadDependencies(p_all: true);
		}

		public void UnloadDependencies(bool p_all, bool p_forceImmediate = false)
		{
			List<UnityEngine.Object> list = new List<UnityEngine.Object>();
			list.AddRange(dependencies);
			for (int i = 0; i < list.Count; i++)
			{
				AssetBundle assetBundle = list[i] as AssetBundle;
				if (!assetBundle)
				{
					continue;
				}
				assetBundle.Unload(p_all);
				if (p_all)
				{
					if (p_forceImmediate)
					{
						UnityEngine.Object.DestroyImmediate(assetBundle, allowDestroyingAssets: true);
					}
					else
					{
						UnityEngine.Object.Destroy(assetBundle);
					}
				}
			}
			list.Clear();
			dependencies.Clear();
		}

		public bool ContainsDependency(string p_file_path)
		{
			return GetDependency(p_file_path) != null;
		}

		public AssetBundle GetDependency(string p_file_path)
		{
			FileInfo fileInfo = new FileInfo(p_file_path);
			string text = fileInfo.Name.Replace(fileInfo.Extension, "").ToLower();
			for (int i = 0; i < dependencies.Count; i++)
			{
				AssetBundle assetBundle = dependencies[i];
				if ((bool)assetBundle && text == assetBundle.name.ToLower())
				{
					return assetBundle;
				}
			}
			return null;
		}

		public AssetBundle GetAssetBundleByName(string p_name)
		{
			return dependencies.Find((AssetBundle it) => (bool)it && it.name == p_name);
		}

		private float GetProgression(IList p_requests, int p_count = -1)
		{
			float num = ((p_requests == null) ? 0f : ((float)((p_count < 0) ? p_requests.Count : p_count)));
			float num2 = 0f;
			for (int i = 0; i < p_requests.Count; i++)
			{
				object obj = p_requests[i];
				if (obj is AssetBundleCreateRequest)
				{
					AssetBundleCreateRequest assetBundleCreateRequest = obj as AssetBundleCreateRequest;
					num2 += ((obj == null) ? 1f : assetBundleCreateRequest.progress);
				}
				if (obj is AssetBundleRequest)
				{
					AssetBundleRequest assetBundleRequest = obj as AssetBundleRequest;
					num2 += ((obj == null) ? 1f : assetBundleRequest.progress);
				}
			}
			if (!(num <= 0f))
			{
				return num2 / num;
			}
			return 1f;
		}

		private bool IsComplete(IList p_requests, int p_count = -1)
		{
			int num = 0;
			for (int i = 0; i < p_requests.Count; i++)
			{
				object obj = p_requests[i];
				if (obj == null)
				{
					num++;
					continue;
				}
				if (obj is AssetBundleCreateRequest)
				{
					if (!(obj as AssetBundleCreateRequest).isDone)
					{
						return false;
					}
					num++;
				}
				if (obj is AssetBundleRequest)
				{
					AssetBundleRequest assetBundleRequest = obj as AssetBundleRequest;
					if (!assetBundleRequest.isDone)
					{
						return false;
					}
					_ = assetBundleRequest.allAssets;
					num++;
				}
			}
			if (p_count >= 0)
			{
				return num >= p_count;
			}
			return true;
		}

		private string AssertGUID(string p_guid)
		{
			if (!p_guid.Contains("-"))
			{
				return p_guid;
			}
			string[] array = p_guid.Split('-');
			array[0] = array[0].ToUpper();
			return string.Join("-", array);
		}

		private void GetGUIDName(string p_target, out string p_guid, out string p_name)
		{
			string[] array = p_target.Split('$');
			p_guid = AssertGUID((array.Length <= 1) ? "" : array[1]);
			p_name = array[0];
		}

		private string AssertAssetName(string p_name)
		{
			GetGUIDName(p_name, out var p_guid, out var p_name2);
			return p_name2 + "$" + p_guid;
		}
	}
}
