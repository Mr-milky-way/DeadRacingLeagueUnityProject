using System;
using System.Collections.Generic;
using GPUInstancer;
using UnityEngine;
using UnityEngine.PostProcessing;
using drl.level;
using thelab.core;

namespace drl
{
	public class LevelSettings : MonoBehaviour
	{
		[Serializable]
		public class Stats
		{
			public int trianglesCount;

			public int trianglesCountClean;
		}

		[Serializable]
		public class Scene
		{
			[Serializable]
			public class AssetLayer
			{
				public string label;

				public List<UnityEngine.Object> assets;

				public int Count
				{
					get
					{
						if (assets != null)
						{
							return assets.Count;
						}
						return 0;
					}
				}

				public void ClearAssets(string p_filter = "")
				{
					assets.RemoveAll((UnityEngine.Object it) => string.IsNullOrEmpty(p_filter) || it.name.Contains(p_filter));
				}

				public void SetEnabled(int p_index, bool p_flag)
				{
					if (assets == null)
					{
						assets = new List<UnityEngine.Object>();
					}
					for (int i = 0; i < assets.Count; i++)
					{
						UnityEngine.Object obj = assets[i];
						if ((bool)obj)
						{
							bool flag = ((p_index < 0) ? p_flag : (p_index == i && p_flag));
							if (obj is GameObject)
							{
								(obj as GameObject).SetActive(flag);
							}
							if (obj is Behaviour)
							{
								(obj as Behaviour).enabled = flag;
							}
						}
					}
				}

				public void SetEnabled(bool p_flag)
				{
					SetEnabled(-1, p_flag);
				}
			}

			[Serializable]
			public class Style
			{
				public string label;

				public List<Material> targets;

				public List<Material> styles;

				public void ClearStyles(string p_filter = "")
				{
					styles.RemoveAll((Material it) => string.IsNullOrEmpty(p_filter) || it.name.Contains(p_filter));
				}

				public List<int> GetStyleIndexes()
				{
					List<string> list = styles.ConvertAll((Material it) => it.name);
					List<int> list2 = new List<int>();
					for (int num = 0; num < list.Count; num++)
					{
						string[] array = list[num].Replace("$dev", "").Split('-');
						if (array.Length != 0)
						{
							string s = array[array.Length - 1];
							int result = -1;
							if (int.TryParse(s, out result) && !list2.Contains(result))
							{
								list2.Add(result);
							}
						}
					}
					list2.Sort();
					return list2;
				}

				public Material GetStyleMaterialByPrefix(string p_prefix, int p_index)
				{
					if (styles == null)
					{
						styles = new List<Material>();
					}
					string k = p_prefix + "-style-" + p_index;
					return styles.Find((Material it) => it.name.Contains(k));
				}

				public void SetStyle(int p_index)
				{
					for (int i = 0; i < targets.Count; i++)
					{
						Material material = targets[i];
						if ((bool)material)
						{
							Material styleMaterialByPrefix = GetStyleMaterialByPrefix(material.name, p_index);
							if ((bool)styleMaterialByPrefix)
							{
								material.CopyPropertiesFromMaterial(styleMaterialByPrefix);
							}
						}
					}
				}
			}

			[Serializable]
			public class AssetBounds
			{
				public List<Collider> colliders;

				public float ground = -500f;

				public bool IsValidAssetPosition(Vector3 p_position)
				{
					if (colliders == null)
					{
						return true;
					}
					if (colliders.Count <= 0 && p_position.y >= ground)
					{
						return true;
					}
					bool flag = false;
					for (int i = 0; i < colliders.Count; i++)
					{
						Collider collider = colliders[i];
						if (!collider)
						{
							continue;
						}
						Vector3 vector = collider.transform.InverseTransformPoint(p_position);
						if (collider is SphereCollider)
						{
							SphereCollider sphereCollider = collider as SphereCollider;
							if (vector.magnitude <= sphereCollider.radius)
							{
								flag = true;
								break;
							}
						}
						if (collider is BoxCollider)
						{
							Vector3 vector2 = (collider as BoxCollider).size * 0.5f;
							if (!(vector.x > vector2.x) && !(vector.x < 0f - vector2.x) && !(vector.y > vector2.y) && !(vector.y < 0f - vector2.y) && !(vector.z > vector2.z) && !(vector.z < 0f - vector2.z))
							{
								flag = true;
								break;
							}
						}
					}
					if (flag && p_position.y >= ground)
					{
						return true;
					}
					return false;
				}

				public Vector3 GetValidPosition(Vector3 p_position)
				{
					if (IsValidAssetPosition(p_position))
					{
						return p_position;
					}
					if (p_position.y < ground)
					{
						p_position.y = ground + 2f;
					}
					List<Vector3> list = new List<Vector3>();
					for (int i = 0; i < colliders.Count; i++)
					{
						Collider collider = colliders[i];
						if ((bool)collider)
						{
							Vector3 item = collider.ClosestPoint(p_position);
							list.Add(item);
						}
					}
					if (list.Count <= 0)
					{
						return p_position;
					}
					Vector3 vector = list[0];
					float num = Vector3.Distance(vector, p_position);
					for (int j = 1; j < list.Count; j++)
					{
						float num2 = Vector3.Distance(list[j], vector);
						if (num2 < num)
						{
							vector = list[j];
							num = num2;
						}
					}
					Vector3 vector2 = vector - p_position;
					p_position += vector2 + vector2.normalized * 2f;
					return p_position;
				}
			}

			[Serializable]
			public class Grid
			{
				public bool enabled;

				public Vector3 size = Vector3.one;

				public Vector3 angle = new Vector3(90f, 90f, 90f);
			}

			public List<GameObject> baseAssets;

			public float assetsScale = 1f;

			public AssetBounds assetBounds;

			public List<AssetLayer> assetLayers;

			public List<Style> styles;

			public Grid grid;

			public int GetBaseAssetsCount()
			{
				if (baseAssets != null)
				{
					return baseAssets.Count;
				}
				return 0;
			}

			public bool IsBaseAssetsEnabled()
			{
				if (baseAssets == null)
				{
					return false;
				}
				if (baseAssets.Count <= 0)
				{
					return true;
				}
				if (!baseAssets[0])
				{
					return false;
				}
				return baseAssets[0].activeInHierarchy;
			}

			public void SetBaseAssetsEnabled(bool p_flag)
			{
				if (baseAssets == null)
				{
					return;
				}
				for (int i = 0; i < baseAssets.Count; i++)
				{
					if ((bool)baseAssets[i])
					{
						baseAssets[i].SetActive(p_flag);
					}
				}
			}

			public int GetAssetLayerCount()
			{
				if (assetLayers != null)
				{
					return assetLayers.Count;
				}
				return 0;
			}

			public AssetLayer GetAssetLayer(int p_index)
			{
				if (assetLayers == null)
				{
					return null;
				}
				if (p_index < 0)
				{
					return null;
				}
				if (p_index >= assetLayers.Count)
				{
					return null;
				}
				return assetLayers[p_index];
			}

			public int GetAssetLayerObjectCount(int p_index)
			{
				return GetAssetLayer(p_index)?.Count ?? 0;
			}

			public void SetAssetLayer(int p_layer, int p_asset)
			{
				GetAssetLayer(p_layer)?.SetEnabled(p_asset, p_flag: true);
			}

			public void ClearAssetLayers(string p_filter)
			{
				int assetLayerCount = GetAssetLayerCount();
				for (int i = 0; i < assetLayerCount; i++)
				{
					GetAssetLayer(i).ClearAssets(p_filter);
				}
			}

			public int GetStyleCount()
			{
				if (styles != null)
				{
					return styles.Count;
				}
				return 0;
			}

			public Style GetStyle(int p_index)
			{
				if (styles == null)
				{
					return null;
				}
				if (p_index < 0)
				{
					return null;
				}
				if (p_index >= styles.Count)
				{
					return null;
				}
				return styles[p_index];
			}

			public int GetStyleMaterialCount(int p_index)
			{
				return GetStyle(p_index)?.GetStyleIndexes().Count ?? 0;
			}

			public void SetStyle(int p_style, int p_index)
			{
				GetStyle(p_style)?.SetStyle(p_index);
			}

			public void ClearStyles(string p_filter)
			{
				int styleCount = GetStyleCount();
				for (int i = 0; i < styleCount; i++)
				{
					GetStyle(i).ClearStyles(p_filter);
				}
			}
		}

		[Serializable]
		public class SunShafts
		{
			public bool enabled;

			public Transform caster;

			public Color thresholdColor;

			public Color shaftsColor;

			[Range(0.1f, 1f)]
			public float falloff;

			[Range(1f, 10f)]
			public float blurSize = 2f;

			[Range(1f, 3f)]
			public int blurIterations = 2;

			public float intensity;
		}

		[Serializable]
		public class ExposureCompensation
		{
			public float advancedRenderingOff;

			public float eyeAdaptationOff;
		}

		[Serializable]
		public class RadioNoise
		{
			public float rangeDistance = -1f;
		}

		[Serializable]
		public class Light
		{
			public bool isIndoor;

			public RenderTexture enhancedFog;

			public UnityEngine.Light sunLight;

			public List<DRLMapLightingPreset> presets;

			[HideInInspector]
			public Color ambientColor;

			public Color ambientBrightness = Color.black;

			private DRLMapLightingPreset m_lightingPreset;

			private List<UnityEngine.Light> levelLights;

			public List<string> presetLabels
			{
				get
				{
					if (presets != null)
					{
						return presets.ConvertAll((DRLMapLightingPreset it) => it.label);
					}
					return new List<string>();
				}
			}

			public UnityEngine.Camera levelFogCamera
			{
				get
				{
					UnityEngine.Camera main = UnityEngine.Camera.main;
					Transform transform = (main ? main.transform.parent : null);
					Transform transform2 = (transform ? transform.Find("fog") : null);
					if (!transform2)
					{
						return null;
					}
					return transform2.GetComponent<UnityEngine.Camera>();
				}
			}

			public Skybox levelFogSkybox
			{
				get
				{
					UnityEngine.Camera camera = levelFogCamera;
					if (!camera)
					{
						return null;
					}
					return camera.GetComponent<Skybox>();
				}
			}

			public DRLMapLightingPreset GetPreset(int p_index)
			{
				if (p_index < 0)
				{
					return null;
				}
				if (p_index >= presets.Count)
				{
					return null;
				}
				return presets[p_index];
			}

			public DRLMapLightingPreset GetActivePreset()
			{
				return presets.Find((DRLMapLightingPreset it) => it.gameObject.activeInHierarchy);
			}

			public int GetPresetCount()
			{
				if (presets != null)
				{
					return presets.Count;
				}
				return 0;
			}

			public void ApplyPreset(int p_index)
			{
				if (presets.Count <= 1)
				{
					return;
				}
				Skybox skybox = levelFogSkybox;
				for (int i = 0; i < presets.Count; i++)
				{
					DRLMapLightingPreset dRLMapLightingPreset = presets[i];
					if (!dRLMapLightingPreset)
					{
						continue;
					}
					dRLMapLightingPreset.gameObject.SetActive(p_index == i);
					if (p_index == i)
					{
						RenderSettings.skybox = dRLMapLightingPreset.skybox;
						RenderSettings.ambientSkyColor = dRLMapLightingPreset.ambientColor;
						RenderSettings.fogColor = dRLMapLightingPreset.fogColor;
						RenderSettings.fogDensity = dRLMapLightingPreset.fogDensity;
						sunLight = dRLMapLightingPreset.sunLight;
						if ((bool)skybox)
						{
							skybox.material = dRLMapLightingPreset.fog;
						}
					}
				}
				SetShadowPreset(p_index);
			}

			protected void SetShadowPreset(int p_index)
			{
				if ((bool)m_lightingPreset)
				{
					m_lightingPreset.RevertShadowOcclusion();
				}
				m_lightingPreset = GetPreset(p_index);
				if ((bool)m_lightingPreset)
				{
					m_lightingPreset.ApplyShadowOcclusion();
				}
			}

			public void ApplyPreset(int p_index, SunShafts p_sunshaft)
			{
				if (presets.Count > 1)
				{
					DRLMapLightingPreset preset = GetPreset(p_index);
					if ((bool)preset)
					{
						p_sunshaft.caster = preset.sunshaft.caster;
						p_sunshaft.thresholdColor = preset.sunshaft.thresholdColor;
						p_sunshaft.shaftsColor = preset.sunshaft.shaftsColor;
						p_sunshaft.falloff = preset.sunshaft.falloff;
						p_sunshaft.blurSize = preset.sunshaft.blurSize;
						p_sunshaft.blurIterations = preset.sunshaft.blurIterations;
						p_sunshaft.intensity = preset.sunshaft.intensity;
					}
				}
			}

			public void SetEnabled(bool p_flag)
			{
				List<UnityEngine.Light> lights = GetLights();
				for (int i = 0; i < lights.Count; i++)
				{
					lights[i].gameObject.SetActive(p_flag);
				}
			}

			private List<UnityEngine.Light> GetLights()
			{
				if (levelLights == null)
				{
					levelLights = new List<UnityEngine.Light>();
					levelLights.AddRange(UnityEngine.Object.FindObjectsOfType<UnityEngine.Light>());
					levelLights.RemoveAll((UnityEngine.Light l) => !l.gameObject.activeInHierarchy);
					levelLights.RemoveAll((UnityEngine.Light l) => l.type == LightType.Directional);
				}
				return levelLights;
			}
		}

		[Serializable]
		public class Reflection
		{
			public bool fadeReflections;

			public float defaultRangePadding = 100f;
		}

		[Serializable]
		public class Shadow
		{
			public float distance = 300f;

			[Range(0f, 3f)]
			public float cascadeDistanceScale = 1f;
		}

		[Serializable]
		public class Camera
		{
			public float nearPlane = 0.04f;

			public float farPlane = 1000f;
		}

		[Serializable]
		public class Terrain
		{
			public bool hasHoles;

			public TerrainCollider[] terrainColliders;

			public Collider[] holesTriggers;

			public float seaLevel;

			public float groundLevel;

			public GPUInstancerDetailManager gpuInstancerDetailManager;

			public GPUInstancerTreeManager gpuInstancerTreeManager;

			public void SetInstancerDetailQuality(float p_density, float p_max_distance, float p_billboard_distance)
			{
				Debug.Log($"Terrain> SetInstancerDetailQuality / Density:{p_density} Max Distance:{p_max_distance} Billboard Distance:{p_billboard_distance}.");
				if (gpuInstancerDetailManager == null)
				{
					Debug.Log("Terrain> SetInstancerDetailQuality / gpuInstancerDetailManager is null.");
					return;
				}
				UnityEngine.Terrain terrain = UnityEngine.Object.FindObjectOfType<UnityEngine.Terrain>();
				if (p_density == 0f || OS.context == "xb" || OS.context == "xbs" || OS.context == "ps4base" || OS.context == "ps4pro")
				{
					Debug.Log("Terrain> SetInstancerDetailQuality / Disable grass.");
					if ((bool)terrain)
					{
						terrain.drawTreesAndFoliage = false;
					}
					gpuInstancerDetailManager.gameObject.SetActive(value: false);
					return;
				}
				Debug.Log("Terrain> SetInstancerDetailQuality / Enable grass.");
				if ((bool)terrain)
				{
					terrain.drawTreesAndFoliage = true;
				}
				gpuInstancerDetailManager.gameObject.SetActive(value: true);
				float detailObjectDensity = gpuInstancerDetailManager.terrain.detailObjectDensity;
				gpuInstancerDetailManager.terrainSettings.maxDetailDistance = p_max_distance;
				gpuInstancerDetailManager.terrainSettings.detailDensity = p_density * detailObjectDensity;
				gpuInstancerDetailManager.GeneratePrototypes(forceNew: true);
				GPUInstancerAPI.InitializeGPUInstancer(gpuInstancerDetailManager);
			}

			public void SetInstancerTreeQuality(float p_lodBias)
			{
				Debug.Log($"Terrain> SetInstancerTreeQuality / LOD Bias:{p_lodBias}");
				if (gpuInstancerTreeManager == null)
				{
					Debug.Log("Terrain> SetInstancerTreeQuality / gpuInstancerTreeManager is null.");
					return;
				}
				bool flag = false;
				if (OS.context == "xb" || OS.context == "xbs" || OS.context == "ps4base")
				{
					flag = true;
				}
				for (int i = 0; i < gpuInstancerTreeManager.prototypeList.Count; i++)
				{
					GPUInstancerTreePrototype gPUInstancerTreePrototype = (GPUInstancerTreePrototype)gpuInstancerTreeManager.prototypeList[i];
					string name = gPUInstancerTreePrototype.prefabObject.name;
					Debug.Log($"Terrain> SetInstancerTreeQuality / gpuInstancerTreeManager name:{name} / is_low:{flag} / OS.context:{OS.context}");
					if (flag)
					{
						gPUInstancerTreePrototype.maxDistance = 500f;
					}
				}
				GPUInstancerAPI.SetLODBias(gpuInstancerTreeManager, p_lodBias);
				GPUInstancerAPI.InitializeGPUInstancer(gpuInstancerTreeManager);
			}

			public void SetCamera(UnityEngine.Camera p_camera)
			{
				if (gpuInstancerTreeManager == null)
				{
					Debug.Log("Terrain> SetInstancerDetailQuality / gpuInstancerTreeManager is null.");
				}
				else if (gpuInstancerDetailManager == null)
				{
					Debug.Log("Terrain> SetInstancerDetailQuality / gpuInstancerDetailManager is null.");
				}
				else
				{
					GPUInstancerAPI.SetCamera(p_camera);
				}
			}
		}

		[Header("Stats")]
		public Stats stats;

		[Header("Scene")]
		public Scene scene;

		[Header("Post Processing Settings")]
		public SunShafts sunshafts;

		public ExposureCompensation exposureCompensation;

		public RadioNoise radioNoise;

		[Header("Lights Settings")]
		public Light light;

		[Header("Reflections Settings")]
		public Reflection reflection;

		[Header("Shadows Settings")]
		public Shadow shadow;

		[Header("Camera Settings")]
		public Camera camera;

		[Header("Terrain Settings")]
		public Terrain terrain;

		private PostProcessingProfile m_ppp_template;

		public PostProcessingProfile pppTemplate
		{
			get
			{
				if ((bool)m_ppp_template)
				{
					return m_ppp_template;
				}
				DRLQualityGroup[] components = GetComponents<DRLQualityGroup>();
				foreach (DRLQualityGroup dRLQualityGroup in components)
				{
					if (!(dRLQualityGroup.id != "camera-fx"))
					{
						m_ppp_template = ((dRLQualityGroup.fxprofiles.Count <= 0) ? null : dRLQualityGroup.fxprofiles[0]);
						break;
					}
				}
				return m_ppp_template;
			}
		}

		protected void Awake()
		{
			m_ppp_template = pppTemplate;
		}

		protected void OnDestroy()
		{
			Debug.Log("LevelSettings.OnDestroy()");
			GPUInstancerAPI.ReleaseInstanceBuffers(terrain.gpuInstancerDetailManager);
			GPUInstancerAPI.ReleaseInstanceBuffers(terrain.gpuInstancerTreeManager);
		}
	}
}
