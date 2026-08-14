using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;
using thelab.core;

namespace drl.game
{
	public class LevelFactory : MonoBehaviour
	{
		public AssetLibrary library;

		public bool inGame;

		private Dictionary<string, bool> m_entity_init_lut;

		public MAEntity Build(MDEntity p_data, Transform p_container, Action<MAEntity, float, MAEntity> p_callback = null, bool p_async = false)
		{
			if (!library)
			{
				UnityEngine.Debug.LogWarning("LevelFactory> No Library Found!");
				return null;
			}
			if (m_entity_init_lut == null)
			{
				m_entity_init_lut = new Dictionary<string, bool>();
			}
			m_entity_init_lut.Clear();
			Dictionary<string, MAEntity> lut = new Dictionary<string, MAEntity>();
			Transform c = p_container;
			MAEntity root = null;
			p_data.RebuildHierarchy();
			List<MDEntity> build_pl = new List<MDEntity>();
			List<MDEntity> build_nl = new List<MDEntity>();
			Traverse(null, p_data, delegate(MDEntity p, MDEntity n)
			{
				build_pl.Add(p);
				build_nl.Add(n);
			});
			float build_length = build_pl.Count;
			Func<bool> on_build_step = delegate
			{
				if (build_pl.Count <= 0)
				{
					Hierarchy.Traverse(c, delegate(MAEntity it)
					{
						if ((bool)it && it.data != null)
						{
							it.data.ClearChildren();
						}
					});
					if (p_callback != null)
					{
						p_callback(root, 1f, null);
					}
					return false;
				}
				float arg = ((build_length <= 0f) ? 1f : (1f - (float)build_pl.Count / build_length)) * 0.99f;
				MDEntity mDEntity = build_pl[0];
				MDEntity mDEntity2 = build_nl[0];
				build_pl.RemoveAt(0);
				build_nl.RemoveAt(0);
				if (mDEntity2.name == "$root")
				{
					mDEntity2.guid = "DMA-NULL";
				}
				Component component = ((mDEntity == null) ? c.transform : (lut.ContainsKey(mDEntity.id) ? lut[mDEntity.id].transform : c.transform));
				MAEntity mAEntity = Instantiate(mDEntity2, component.transform);
				if (mAEntity.transform.localScale.magnitude > 500f)
				{
					mAEntity.transform.localScale = Vector3.one;
				}
				if (mAEntity is MAGate)
				{
					(mAEntity as MAGate).SetTriggerRendererEnabled(p_flag: false);
				}
				if (p_callback != null)
				{
					p_callback(null, arg, mAEntity);
				}
				lut[mDEntity2.id] = mAEntity;
				if (!root)
				{
					root = mAEntity;
				}
				return true;
			};
			Stopwatch build_clk = new Stopwatch();
			build_clk.Start();
			if (p_async)
			{
				bool async_fps_mode = true;
				int step_ms_cap = 120;
				int num = 30;
				int build_step_count = (async_fps_mode ? 1 : Mathf.Max(1, (int)build_length / num));
				int build_mem_step = 0;
				int build_mem_step_max = 10;
				if (!async_fps_mode)
				{
					build_step_count = 12;
					build_mem_step_max = 0;
				}
				UnityEngine.Debug.Log($"LevelFactory> Build / build-length[{(int)build_length}] fps-mode[{async_fps_mode}] ms-cap[{step_ms_cap}] frac-cap[{num}] build-step-count[{build_step_count}]");
				Stopwatch step_clk = new Stopwatch();
				Activity.Run((Func<bool>)delegate
				{
					int num3 = 0;
					step_clk.Restart();
					while ((!async_fps_mode || num3++ <= 500) && (!async_fps_mode || step_clk.ElapsedMilliseconds < step_ms_cap))
					{
						if (!async_fps_mode && build_mem_step > 0)
						{
							build_mem_step--;
							break;
						}
						for (int i = 0; i < build_step_count; i++)
						{
							if (!on_build_step())
							{
								UnityEngine.Debug.Log($"LevelFactory> Async Build / Complete in [{build_clk.ElapsedMilliseconds / 1000}s]");
								build_clk.Stop();
								return false;
							}
						}
						if (!async_fps_mode)
						{
							build_mem_step = build_mem_step_max;
							break;
						}
					}
					return true;
				}, 0f, false);
			}
			else
			{
				int num2 = 100000;
				while (num2 > 0 && on_build_step())
				{
					num2--;
				}
				build_clk.Stop();
			}
			return root;
		}

		public MAEntity Instantiate(MDEntity p_node, Transform p_parent)
		{
			MAEntity mAEntity = null;
			if (p_node == null)
			{
				GameObject obj = new GameObject();
				obj.name = "null";
				obj.transform.SetParent(p_parent);
				return obj.AddComponent<MAEntity>();
			}
			_ = library;
			string guid = p_node.guid;
			mAEntity = FetchEntityFromLibrary(guid);
			bool flag = Profiler.enabled;
			if ((bool)mAEntity)
			{
				mAEntity = UnityEngine.Object.Instantiate(mAEntity);
			}
			else
			{
				mAEntity = new GameObject().AddComponent<MAEntity>();
				UnityEngine.Debug.LogWarning("LevelFactory> Failed to find asset or it isnt a MapEditor Asset / guid[" + guid + "] name[" + p_node.name + "] category[" + p_node.category.ToString() + "]");
			}
			mAEntity.transform.SetParent(p_parent);
			mAEntity.data = p_node;
			mAEntity.Read();
			mAEntity.SetHitsLayer(12);
			if (mAEntity is MAGate)
			{
				(mAEntity as MAGate).SetTriggersLayers(28);
			}
			return mAEntity;
		}

		protected MAEntity FetchEntityFromLibrary(string p_guid)
		{
			MAEntity mAEntity = library.FindByGUID<MAEntity>(p_guid);
			if (!mAEntity)
			{
				return null;
			}
			if (m_entity_init_lut.ContainsKey(p_guid) && m_entity_init_lut[p_guid])
			{
				return mAEntity;
			}
			m_entity_init_lut[p_guid] = true;
			_ = mAEntity?.components;
			MAModularScaleRenderer mAModularScaleRenderer = ((mAEntity is MARenderer) ? mAEntity.GetComponent<MAModularScaleRenderer>() : null);
			ModularScaleComponent modularScaleComponent = null;
			if ((bool)mAModularScaleRenderer)
			{
				mAModularScaleRenderer.asyncRefresh = !inGame;
				modularScaleComponent = mAModularScaleRenderer.modules;
			}
			modularScaleComponent = (modularScaleComponent ? modularScaleComponent : mAEntity.GetComponent<ModularScaleComponent>());
			if ((bool)modularScaleComponent)
			{
				modularScaleComponent.enabled = !inGame;
			}
			MAGuide mAGuide = mAEntity as MAGuide;
			if ((bool)mAGuide)
			{
				mAGuide.enabled = !inGame;
				mAGuide.SetEnabled(!inGame);
				if (mAGuide.data.type == MapAssetType.SplineControlPoint)
				{
					mAGuide.SetAssetMode(!inGame);
				}
				else
				{
					mAGuide.SetIconMode(!inGame);
				}
			}
			return mAEntity;
		}

		public MapAsset Instantiate(MDEntity p_node)
		{
			return Instantiate(p_node, null);
		}

		public void Traverse(MDEntity p_node, Action<MDEntity, MDEntity> p_callback)
		{
			Traverse(null, p_node, p_callback);
		}

		protected void Traverse(MDEntity p_parent, MDEntity p_node, Action<MDEntity, MDEntity> p_callback)
		{
			if (p_node != null)
			{
				int childCount = p_node.childCount;
				p_callback?.Invoke(p_parent, p_node);
				for (int i = 0; i < childCount; i++)
				{
					MDEntity child = p_node.GetChild(i);
					Traverse(p_node, child, p_callback);
				}
			}
		}
	}
}
