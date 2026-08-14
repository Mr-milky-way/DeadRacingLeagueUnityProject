using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityStandardAssets.ImageEffects;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class LevelController : Controller<DRLApp>
	{
		private float m_last_radio;

		private bool m_is_radiofx_enabled;

		private bool m_is_radiofx_range_enabled;

		private Transform m_current_root_ppb;

		private PostProcessingBehaviour[] m_root_ppb_cache;

		private List<DroneCamera> m_cameras;

		public TrackController track => AssertFind<TrackController>("track");

		public LevelFactory factory => AssertLocal<LevelFactory>("factory");

		public LevelModel model => AssertLocal<LevelModel>("model");

		public GameController game => AssertParent<GameController>("level");

		protected void Awake()
		{
			if ((bool)model.settings)
			{
				model.settings.light.ambientColor = RenderSettings.ambientSkyColor;
			}
		}

		protected void OnDestroy()
		{
			if ((bool)model.settings)
			{
				RenderSettings.ambientSkyColor = model.settings.light.ambientColor;
			}
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "scene.start":
				model.data = base.app.arguments.game.map;
				factory.library = base.app.model.storage.library;
				break;
			case "scene.game.scenes@complete":
				ApplyLevelSettings();
				m_is_radiofx_enabled = base.app.model.storage.state.player.settings.graphics.radioFx;
				m_is_radiofx_range_enabled = base.app.model.storage.state.player.settings.game.radioNoise;
				Notify(0.02f, "game.level.load@complete");
				break;
			case "game.simulation.camera@add":
			{
				DroneCamera p_camera = Reflection<object>.Get<DroneCamera>(p_data, 0);
				ApplyLevelSettings(p_camera);
				m_last_radio = -1f;
				break;
			}
			case "game.simulation.drone@ready":
			{
				Drone p_drone = Reflection<object>.Get<Drone>(p_data, 0);
				ApplyLevelSettings(p_drone);
				break;
			}
			case "settings.game.screen.apply":
				m_is_radiofx_enabled = base.app.model.storage.state.player.settings.graphics.radioFx;
				m_is_radiofx_range_enabled = base.app.model.storage.state.player.settings.game.radioNoise;
				break;
			}
		}

		public void ApplyLevelSettings(Drone p_drone)
		{
			if (!p_drone)
			{
				Debug.LogWarning("LevelController> ApplyLevelSettings - Drone is null!");
				return;
			}
			Debug.Log("LevelController> ApplyLevelSettings - Drone [" + p_drone?.ToString() + "]");
			LevelSettings settings = model.settings;
			if (!settings)
			{
				Debug.LogWarning("LevelController> Level Settings not found.");
				return;
			}
			Transform transform = p_drone.body.frame.transform.Find("colliders").Find("rig");
			List<Collider> list = new List<Collider>();
			if ((bool)transform)
			{
				list.AddRange(transform.GetComponentsInChildren<Collider>());
			}
			if (p_drone.rigidbody.levelHoleTriggers == null)
			{
				p_drone.rigidbody.levelHoleTriggers = new List<Collider>();
			}
			p_drone.rigidbody.levelHoleTriggers.Clear();
			if (settings.terrain.hasHoles)
			{
				p_drone.rigidbody.levelHoleTriggers.AddRange(settings.terrain.holesTriggers);
			}
			LayerSwitch layerSwitch = (transform ? Hierarchy.Find<LayerSwitch>(transform) : null);
			if (!layerSwitch)
			{
				return;
			}
			if (layerSwitch.colliders == null)
			{
				layerSwitch.colliders = new List<Collider>();
			}
			layerSwitch.colliders.Clear();
			layerSwitch.colliders.AddRange(settings.terrain.holesTriggers);
			if (layerSwitch.targets == null)
			{
				layerSwitch.targets = new List<GameObject>();
			}
			layerSwitch.targets.Clear();
			foreach (Collider item in list)
			{
				if (item != null && !layerSwitch.targets.Contains(item.gameObject))
				{
					layerSwitch.targets.Add(item.gameObject);
				}
			}
		}

		public void ApplyLevelSettings(List<DroneCamera> p_cameras)
		{
			for (int i = 0; i < p_cameras.Count; i++)
			{
				ApplyLevelSettings(p_cameras[i]);
			}
		}

		public void ApplyLevelSettings(DroneCamera p_camera)
		{
			if (!p_camera)
			{
				Debug.LogWarning("LevelController> ApplyLevelSettings - Camera is null!");
				return;
			}
			CameraFX component = p_camera.GetComponent<CameraFX>();
			if (!component)
			{
				Debug.LogWarning("LevelController> ApplyLevelSettings - CameraFX is null!");
				return;
			}
			LevelSettings settings = model.settings;
			if (!settings)
			{
				Debug.LogWarning("LevelController> Level Settings not found.");
				return;
			}
			bool sunShafts = base.app.model.storage.state.player.settings.graphics.sunShafts;
			bool flag = settings.sunshafts.enabled;
			SunShafts sunshafts = component.sunshafts;
			bool sunshaftsAllowed = sunShafts && flag;
			component.sunshaftsAllowed = sunshaftsAllowed;
			if ((bool)sunshafts)
			{
				sunshafts.enabled = sunshaftsAllowed;
				sunshafts.sunTransform = settings.sunshafts.caster;
				sunshafts.sunThreshold = settings.sunshafts.thresholdColor;
				sunshafts.sunColor = settings.sunshafts.shaftsColor;
				sunshafts.maxRadius = 1f - settings.sunshafts.falloff;
				sunshafts.sunShaftBlurRadius = settings.sunshafts.blurSize;
				sunshafts.radialBlurIterations = settings.sunshafts.blurIterations;
				sunshafts.sunShaftIntensity = settings.sunshafts.intensity;
			}
			if (settings.terrain.hasHoles)
			{
				p_camera.holeCollision.entranceTriggers = settings.terrain.holesTriggers;
				p_camera.holeCollision.terrainColliders = settings.terrain.terrainColliders;
				p_camera.holeCollision.Initialize();
			}
			p_camera.SetNearFarClips(settings.camera.nearPlane, settings.camera.farPlane);
			Debug.Log("LevelController> Camera - n[" + component.camera.nearClipPlane + "] f[" + component.camera.farClipPlane + "]");
			if (m_current_root_ppb != model.root.transform)
			{
				m_current_root_ppb = model.root.transform;
				List<PostProcessingBehaviour> list = Hierarchy.FindAll<PostProcessingBehaviour>(model.root.transform);
				if ((bool)component.ppb)
				{
					list.Add(component.ppb);
				}
				m_root_ppb_cache = list.ToArray();
			}
			DRLQualityGroup component2 = settings.GetComponent<DRLQualityGroup>();
			if (!component2)
			{
				Debug.LogWarning("LevelController> QualityGroup  not found in settings.");
				return;
			}
			Debug.Log("LevelController> Found [" + ((m_root_ppb_cache != null) ? m_root_ppb_cache.Length : 0) + "] PPB");
			component2.postProcessing = ((m_root_ppb_cache == null) ? new PostProcessingBehaviour[0] : m_root_ppb_cache);
			component2.Apply();
		}

		public void SetLightingPreset(int p_index)
		{
			if (model.HasLightingPresets())
			{
				if (!model.settings.light.GetPreset(p_index))
				{
					Debug.LogWarning("LevelController> SetLightingPreset / preset[" + p_index + "] not found!");
					return;
				}
				model.settings.light.ApplyPreset(p_index);
				ApplyLevelSettings();
				ApplyLevelSettings(game.model.camera);
				Notify("settings.graphics.map.lighting.apply");
			}
		}

		public void ApplyLevelSettings()
		{
			Debug.Log("LevelController> ApplyLevelSettings");
			LevelSettings settings = model.settings;
			if (!settings)
			{
				Debug.LogWarning("LevelController> ApplyLevelSettings / Level Settings not found.");
				return;
			}
			Light sun = model.sun;
			DRLQualityGroup component = settings.GetComponent<DRLQualityGroup>();
			if ((bool)component)
			{
				for (int i = 0; i < component.fxprofiles.Count; i++)
				{
					component.fxprofiles[i].screenSpaceReflection.enabled = false;
				}
			}
			bool flag = QualitySettings.shadows != ShadowQuality.Disable;
			if ((bool)sun)
			{
				_ = sun.enabled;
			}
			QualitySettings.GetQualityLevel();
			if ((bool)sun)
			{
				sun.cullingMask = model.sunLayerMask;
				if (!flag)
				{
					sun.cullingMask = DRLLayerFlag.WaterBit;
				}
				sun.cullingMask &= ~DRLLayerFlag.HUDBit;
				sun.gameObject.SetActive(value: true);
				model.ApplySunFix(flag);
			}
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			int index = Mathf.Clamp(graphics.shadow, 0, graphics.shadowQualityPresets.Count - 1);
			ShadowQualitySettings shadowQualitySettings = graphics.shadowQualityPresets[index];
			float num = 1f;
			if (shadowQualitySettings != null)
			{
				num = shadowQualitySettings.distance;
			}
			QualitySettings.shadowDistance = settings.shadow.distance * num;
			QualitySettings.shadowCascade2Split *= settings.shadow.cascadeDistanceScale;
			QualitySettings.shadowCascade4Split *= settings.shadow.cascadeDistanceScale;
			bool advancedRendering = graphics.advancedRendering;
			Color ambientColor = model.settings.light.ambientColor;
			Color color = ((advancedRendering && flag) ? Color.black : model.settings.light.ambientBrightness);
			List<RenderingProbeGroup> allRenderingProbeGroups = model.allRenderingProbeGroups;
			bool flag2 = allRenderingProbeGroups.Count > 0;
			for (int j = 0; j < allRenderingProbeGroups.Count; j++)
			{
				RenderingProbeGroup renderingProbeGroup = allRenderingProbeGroups[j];
				if ((bool)renderingProbeGroup)
				{
					renderingProbeGroup.ambientBrightness = color;
				}
			}
			if (!flag2)
			{
				RenderSettings.ambientSkyColor = ambientColor + color;
			}
			float rangeDistance = settings.radioNoise.rangeDistance;
			model.radio.receptionRangeDistance = ((rangeDistance < 0f) ? 6f : rangeDistance);
			Debug.Log("LevelController> ApplyLevelSettings");
		}

		protected void Update()
		{
			UpdateRadioQuality();
		}

		protected void UpdateRadioQuality()
		{
			if (!model.radio || !model.radio.enabled || !base.app.model.storage)
			{
				return;
			}
			bool flag = base.app.model.game.type == GameFlag.MapEditor;
			if (m_cameras == null)
			{
				m_cameras = new List<DroneCamera>();
			}
			List<DroneCamera> list = m_cameras;
			Drone drone = null;
			if (!flag)
			{
				DroneSimulation simulation = base.app.model.game.simulation;
				if (!simulation)
				{
					return;
				}
				drone = base.app.model.game.playerDrone;
				list = simulation.cameras.list;
				if (!drone)
				{
					drone = ((simulation.drones.list.Count <= 0) ? null : simulation.drones.list[0]);
				}
			}
			else
			{
				list.Clear();
				list.Add(base.app.model.game.camera);
			}
			bool is_radiofx_enabled = m_is_radiofx_enabled;
			bool is_radiofx_range_enabled = m_is_radiofx_range_enabled;
			int num = list?.Count ?? 0;
			for (int i = 0; i < num; i++)
			{
				DroneCamera droneCamera = list[i];
				if ((bool)droneCamera && (bool)droneCamera.fx)
				{
					Transform transform = (flag ? droneCamera.transform : droneCamera.follow.target);
					bool flag2 = (bool)drone && (bool)transform && transform.IsChildOf(drone.transform);
					bool flag3 = droneCamera.mode == DroneCameraModeType.FPV;
					bool flag4 = flag || flag2;
					model.radio.receptionEnabled = flag4 && is_radiofx_enabled && is_radiofx_range_enabled && !flag && flag3;
					float p_exp = (flag ? 0.3f : 1f);
					float num2 = (flag4 ? model.radio.UpdateTarget(droneCamera.fx, p_exp) : 1f);
					float num3 = 0.5f;
					float num4 = 0.01f;
					float num5 = 0.99f;
					float f = m_last_radio - num2;
					float boundsSignal = model.radio.boundsSignal;
					float receptionSignal = model.radio.receptionSignal;
					if (num2 <= num4 && m_last_radio > num4)
					{
						Notify(flag ? "map-editor.camera.signal-lost" : "game.drone.signal-lost", drone, num2, boundsSignal, receptionSignal);
					}
					if (num2 >= num5 && m_last_radio < num5)
					{
						Notify(flag ? "map-editor.camera.signal-full" : "game.drone.signal-full", drone, num2, boundsSignal, receptionSignal);
					}
					if (num2 < num3 && m_last_radio >= num3)
					{
						Notify(flag ? "map-editor.camera.signal-drop" : "game.drone.signal-drop", drone, num2, boundsSignal, receptionSignal);
					}
					if (num2 >= num3 && m_last_radio < num3)
					{
						Notify(flag ? "map-editor.camera.signal-recover" : "game.drone.signal-recover", drone, num2, boundsSignal, receptionSignal);
					}
					if (Mathf.Abs(f) > 0f)
					{
						Notify(flag ? "map-editor.camera.signal-update" : "game.drone.signal-update", drone, num2, boundsSignal, receptionSignal);
					}
					droneCamera.fx.radioEnabled = is_radiofx_enabled;
					if (!flag && !flag3)
					{
						droneCamera.fx.radioEnabled = false;
					}
					m_last_radio = num2;
				}
			}
		}
	}
}
