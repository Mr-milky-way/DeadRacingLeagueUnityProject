using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class MAModularScaleRenderer : MapAssetComponent
	{
		[SerializeField]
		internal Vector3 m_module_scale = Vector3.one;

		[SerializeField]
		private ModularScaleComponent m_modules;

		public bool asyncRefresh = true;

		[SerializeField]
		private MARenderer m_renderer;

		public Vector3 moduleScale
		{
			get
			{
				return m_module_scale;
			}
			set
			{
				modules.scale = (m_module_scale = value);
			}
		}

		public ModularScaleComponent modules
		{
			get
			{
				if (!m_modules)
				{
					return m_modules = GetComponent<ModularScaleComponent>();
				}
				return m_modules;
			}
		}

		public MARenderer renderer
		{
			get
			{
				if (!m_renderer)
				{
					return m_renderer = GetComponent<MARenderer>();
				}
				return m_renderer;
			}
		}

		protected void SetModuleScale(MDObject d, Vector3 v)
		{
			d.SetVector3("module-scale", v);
		}

		protected Vector3 GetModuleScale(MDObject d)
		{
			return d.GetVector3("module-scale", Vector3.one);
		}

		protected void Awake()
		{
			if ((bool)modules)
			{
				modules.OnVariantChange = OnModularVariantChange;
				AddtHitRenderers(modules.current);
			}
		}

		internal override void OnEvent(MapAsset p_target, MapAssetEventType p_type)
		{
			switch (p_type)
			{
			case MapAssetEventType.Refresh:
			{
				if (!modules)
				{
					break;
				}
				modules.scale = m_module_scale;
				int childCount = modules.transform.childCount;
				for (int i = 0; i < childCount; i++)
				{
					Transform child = modules.transform.GetChild(i);
					if (child.name.Contains("$variant"))
					{
						ModularScaleVariant component = child.GetComponent<ModularScaleVariant>();
						if ((bool)component && component != modules.current)
						{
							Object.Destroy(component.gameObject);
						}
					}
					else if ((bool)modules.baseAsset)
					{
						bool flag = false;
						if (child.gameObject != modules.baseAsset)
						{
							flag = true;
						}
						if ((bool)child.GetComponent<MAGuide>())
						{
							flag = false;
						}
						if (flag)
						{
							Object.Destroy(child.gameObject);
						}
					}
				}
				MAGate mAGate = p_target as MAGate;
				if ((bool)mAGate && (bool)modules.current)
				{
					RemovetHitRenderers(modules.current);
					mAGate.AssertTrigger(modules.current.transform);
					AddtHitRenderers(modules.current);
				}
				break;
			}
			case MapAssetEventType.DataRead:
			{
				MDObject data2 = renderer.m_data;
				if (data2 != null)
				{
					m_module_scale = GetModuleScale(data2);
					renderer.Refresh();
				}
				break;
			}
			case MapAssetEventType.DataWrite:
			{
				MDObject data = renderer.m_data;
				if (data != null)
				{
					SetModuleScale(data, m_module_scale = modules.scale);
				}
				break;
			}
			case MapAssetEventType.Data:
				break;
			}
		}

		protected void OnModularVariantChange(ModularScaleVariant p_from, ModularScaleVariant p_to)
		{
			RemovetHitRenderers(p_from);
			AddtHitRenderers(p_to);
			if ((bool)p_to)
			{
				if (asyncRefresh)
				{
					renderer.DelayRefresh();
				}
				else
				{
					renderer.Refresh();
				}
			}
			SetModuleScale(renderer.data, m_module_scale = modules.scale);
		}

		private void RemovetHitRenderers(ModularScaleVariant p_target)
		{
			if (!p_target)
			{
				return;
			}
			List<Collider> list = Hierarchy.FindAll<Collider>(p_target.transform);
			for (int i = 0; i < list.Count; i++)
			{
				if ((bool)list[i])
				{
					list[i].gameObject.layer = 12;
				}
			}
			MARenderer mARenderer = renderer;
			List<Renderer> list2 = Hierarchy.FindAll<Renderer>(p_target.transform);
			for (int j = 0; j < list.Count; j++)
			{
				if (mARenderer.hits.Contains(list[j]))
				{
					mARenderer.hits.Remove(list[j]);
				}
			}
			for (int k = 0; k < list2.Count; k++)
			{
				if (mARenderer.renderers.Contains(list2[k]))
				{
					mARenderer.renderers.Remove(list2[k]);
				}
			}
		}

		private void AddtHitRenderers(ModularScaleVariant p_target)
		{
			if (!p_target)
			{
				return;
			}
			List<Collider> list = Hierarchy.FindAll<Collider>(p_target.transform);
			for (int i = 0; i < list.Count; i++)
			{
				if ((bool)list[i])
				{
					list[i].gameObject.layer = 12;
				}
			}
			MARenderer mARenderer = renderer;
			List<Renderer> list2 = Hierarchy.FindAll<Renderer>(p_target.transform);
			for (int j = 0; j < list.Count; j++)
			{
				if (!mARenderer.hits.Contains(list[j]))
				{
					mARenderer.hits.Add(list[j]);
				}
			}
			for (int k = 0; k < list2.Count; k++)
			{
				if (!mARenderer.renderers.Contains(list2[k]))
				{
					mARenderer.renderers.Add(list2[k]);
				}
			}
			MAGate component = renderer.GetComponent<MAGate>();
			if ((bool)component)
			{
				component.SetTriggersLayers(28);
			}
		}

		private bool IsDifferent(Vector3 a, Vector3 b, float p_bias = 0.0001f)
		{
			if (Mathf.Abs(a.x - b.x) >= p_bias)
			{
				return true;
			}
			if (Mathf.Abs(a.y - b.y) >= p_bias)
			{
				return true;
			}
			if (Mathf.Abs(a.z - b.z) >= p_bias)
			{
				return true;
			}
			return false;
		}
	}
}
