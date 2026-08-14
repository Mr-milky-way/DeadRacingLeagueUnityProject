using System.Collections.Generic;
using UnityEngine;
using drl.level;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class LevelModel : Model<DRLApp>
	{
		public DRLMap data;

		[SerializeField]
		private GameObject m_root;

		[SerializeField]
		private LevelSettings m_settings;

		[SerializeField]
		private Light m_sun;

		private DRLMapLightingPreset m_active_preset;

		private Transform m_root_env_lights;

		private bool m_sunfix_dirty;

		protected Light m_sun_fix;

		public LayerMask sunLayerMask;

		[SerializeField]
		private RadioQuality m_radio;

		public List<RenderingProbeGroup> renderingProbeGroups;

		private Activity m_fps_loop;

		public GameObject root
		{
			get
			{
				if ((bool)m_root)
				{
					return m_root;
				}
				m_root = base.app.scene.GetMapRoot();
				if ((bool)m_root)
				{
					OnRootLoaded();
				}
				return m_root;
			}
		}

		public TrackModel track => AssertFind<TrackModel>("track");

		public LevelSettings settings => Reflection<object>.Assert(ref m_settings, root, p_add: false);

		public Light sun
		{
			get
			{
				if (!root)
				{
					return null;
				}
				if (!m_root_env_lights)
				{
					m_root_env_lights = Hierarchy.Find<Transform>(root.transform, "environment.lights");
				}
				bool num = !m_root_env_lights && settings.light.presets != null;
				DRLMapLightingPreset dRLMapLightingPreset = null;
				if (num)
				{
					dRLMapLightingPreset = settings.light.GetActivePreset();
				}
				if (dRLMapLightingPreset == m_active_preset)
				{
					return m_sun;
				}
				m_active_preset = dRLMapLightingPreset;
				Transform transform = (m_active_preset ? m_active_preset.transform.Find("lights") : null);
				if (!transform)
				{
					return null;
				}
				Transform transform2 = transform;
				for (int i = 0; i < transform2.childCount; i++)
				{
					Transform child = transform2.GetChild(i);
					string text = child.name;
					if (text.IndexOf("sun-light") >= 0)
					{
						m_sun = child.GetComponent<Light>();
					}
					if (text.IndexOf("moon-light") >= 0)
					{
						m_sun = child.GetComponent<Light>();
					}
					if ((bool)m_sun)
					{
						break;
					}
				}
				if ((bool)m_sun)
				{
					sunLayerMask = m_sun.cullingMask;
				}
				return m_sun;
			}
		}

		public RadioQuality radio
		{
			get
			{
				if (!root)
				{
					return null;
				}
				if ((bool)m_radio)
				{
					return m_radio;
				}
				Transform transform = root.transform.Find("radio");
				if (!transform)
				{
					transform = new GameObject("radio").transform;
					transform.SetParent(root.transform, worldPositionStays: true);
				}
				m_radio = transform.gameObject.GetComponent<RadioQuality>();
				if (!m_radio)
				{
					m_radio = transform.gameObject.AddComponent<RadioQuality>();
				}
				return m_radio;
			}
		}

		public List<RenderingProbeGroup> allRenderingProbeGroups
		{
			get
			{
				List<RenderingProbeGroup> list = new List<RenderingProbeGroup>();
				list.AddRange(renderingProbeGroups);
				list.AddRange(track.renderingProbeGroups);
				return list;
			}
		}

		public void ApplySunFix(bool p_has_shadows)
		{
			Light light = sun;
			Transform transform = (light ? light.transform.parent : null);
			if ((bool)transform)
			{
				Transform transform2 = transform.Find("fix-terrain-lighting");
				if ((bool)transform2)
				{
					Object.DestroyImmediate(transform2.gameObject, allowDestroyingAssets: true);
				}
			}
			if (!light)
			{
				return;
			}
			GameObject gameObject = Object.Instantiate(light.gameObject, transform);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localEulerAngles = Vector3.zero;
			gameObject.name = "fix-terrain-lighting";
			m_sun_fix = gameObject.GetComponent<Light>();
			m_sun_fix.intensity = 0.001f;
			m_sun_fix.cullingMask = -1;
			m_sun_fix.cullingMask &= ~DRLLayerFlag.HUDBit;
			m_sun_fix.cullingMask &= ~DRLLayerFlag.WaterBit;
			m_sun_fix.gameObject.SetActive(!p_has_shadows);
			m_sun_fix.enabled = true;
			Transform transform3 = gameObject.transform;
			for (int num = transform3.childCount; num > 0; num--)
			{
				Transform child = transform3.GetChild(0);
				if ((bool)child)
				{
					Object.Destroy(child.gameObject);
				}
			}
		}

		public bool HasBaseAssets()
		{
			if (settings.scene.baseAssets != null && settings.scene.baseAssets.Count > 0)
			{
				return true;
			}
			if (track.settings.scene.baseAssets != null && track.settings.scene.baseAssets.Count > 0)
			{
				return true;
			}
			return false;
		}

		public void SetBaseAssetsEnabled(bool p_flag)
		{
			Debug.Log("LevelModel> SetBaseAssetsEnabled / f[" + p_flag + "]");
			if ((bool)settings)
			{
				SetBaseAssetsEnabled(settings.scene.baseAssets, p_flag);
			}
			else
			{
				Debug.LogWarning("LevelModel> SetBaseAssetsEnabled / Level Settings not Found - f[" + p_flag + "]");
			}
			if ((bool)track.settings)
			{
				SetBaseAssetsEnabled(track.settings.scene.baseAssets, p_flag);
			}
			else
			{
				Debug.LogWarning("LevelModel> SetBaseAssetsEnabled / Track Settings not Found - f[" + p_flag + "]");
			}
		}

		protected void SetBaseAssetsEnabled(IList<GameObject> p_list, bool p_flag)
		{
			if (p_list == null)
			{
				Debug.LogWarning("LevelModel> SetBaseAssetsEnabled / Invalid List - f[" + p_flag + "]");
				return;
			}
			for (int i = 0; i < p_list.Count; i++)
			{
				GameObject gameObject = p_list[i];
				if ((bool)gameObject && gameObject.activeSelf != p_flag)
				{
					gameObject.SetActive(p_flag);
				}
			}
		}

		public bool HasAssetLayers()
		{
			return settings.scene.GetAssetLayerCount() > 0;
		}

		public LevelSettings.Scene.AssetLayer GetAssetLayer(int p_index)
		{
			return settings.scene.GetAssetLayer(p_index);
		}

		public int GetAssetLayerCount()
		{
			return settings.scene.GetAssetLayerCount();
		}

		public int GetAssetLayerObjectCount(int p_layer)
		{
			return settings.scene.GetAssetLayerObjectCount(p_layer);
		}

		public void SetAssetLayerIndex(int p_layer, int p_index)
		{
			Debug.Log("LevelModel> SetAssetLayerIndex / layer[" + p_layer + "] index[" + p_index + "]");
			settings.scene.SetAssetLayer(p_layer, p_index);
		}

		public bool HasLightingPresets()
		{
			if (settings.light.presets == null)
			{
				return false;
			}
			return settings.light.presets.Count > 1;
		}

		protected virtual void OnRootLoaded()
		{
			renderingProbeGroups = Hierarchy.FindAll<RenderingProbeGroup>(root.transform);
		}
	}
}
