using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using thelab.core;

namespace drl.level
{
	public class DRLMapLightingPreset : MonoBehaviour
	{
		[Serializable]
		public class SunShafts
		{
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

		public string label;

		public Material skybox;

		public Material fog;

		public Color ambientColor;

		public Color fogColor;

		public float fogDensity;

		public Light sunLight;

		public SunShafts sunshaft;

		[Header("Shadowmap Occlusion")]
		public List<GameObject> shadowOccludees;

		public List<GameObject> ignoreShadowOccludees;

		public List<MeshRenderer> shadowOccludeesRenderer;

		public List<ShadowCastingMode> shadowOccludeesState;

		public float shadowBoundsMargin = 1f;

		public static bool IsBoundShadowOverlaped(Light p_light, MeshRenderer p_renderer, float p_margin)
		{
			if (p_light.type != LightType.Directional)
			{
				return false;
			}
			_ = p_renderer.bounds;
			Transform transform = p_renderer.transform;
			Vector3[] worldBoundCorners = GetWorldBoundCorners(p_renderer, p_margin);
			bool flag = false;
			for (int i = 0; i < worldBoundCorners.Length; i++)
			{
				Vector3 origin = transform.TransformPoint(worldBoundCorners[i]);
				Vector3 direction = -p_light.transform.forward;
				if (!Physics.Raycast(origin, direction, out var _, float.PositiveInfinity, -1, QueryTriggerInteraction.Ignore))
				{
					flag = true;
				}
			}
			return !flag;
		}

		public static Vector3[] GetWorldBoundCorners(MeshRenderer p_renderer, float p_margin)
		{
			MeshFilter component = p_renderer.GetComponent<MeshFilter>();
			if (!component)
			{
				return new Vector3[0];
			}
			Mesh sharedMesh = component.sharedMesh;
			if (!sharedMesh)
			{
				return new Vector3[0];
			}
			Bounds bounds = sharedMesh.bounds;
			bounds.Expand(p_margin);
			Vector3 extents = bounds.extents;
			Vector3 center = bounds.center;
			return new Vector3[8]
			{
				center + new Vector3(extents.x, extents.y, extents.z),
				center + new Vector3(0f - extents.x, extents.y, extents.z),
				center + new Vector3(0f - extents.x, extents.y, 0f - extents.z),
				center + new Vector3(extents.x, extents.y, 0f - extents.z),
				center + new Vector3(extents.x, 0f - extents.y, extents.z),
				center + new Vector3(0f - extents.x, 0f - extents.y, extents.z),
				center + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z),
				center + new Vector3(extents.x, 0f - extents.y, 0f - extents.z)
			};
		}

		public static void DebugShadowOverlap(Light p_light, GameObject p_target, float p_margin = 0.5f)
		{
			List<MeshRenderer> p_targets = Hierarchy.FindAll<MeshRenderer>(p_target.transform);
			DebugShadowOverlap(p_light, p_targets);
		}

		public static void DebugShadowOverlap(Light p_light, List<MeshRenderer> p_targets, float p_margin = 0.5f)
		{
			if (!p_light)
			{
				return;
			}
			for (int i = 0; i < p_targets.Count; i++)
			{
				MeshRenderer meshRenderer = p_targets[i];
				if (meshRenderer.shadowCastingMode != ShadowCastingMode.Off)
				{
					if (IsBoundShadowOverlaped(p_light, meshRenderer, p_margin))
					{
						DrawBoundingBox(meshRenderer, Color.red, p_margin);
					}
					else
					{
						DrawBoundingBox(meshRenderer, Color.green, p_margin);
					}
				}
			}
		}

		private static void DrawBoundingBox(MeshRenderer p_renderer, Color p_color, float p_margin)
		{
			Vector3[] worldBoundCorners = GetWorldBoundCorners(p_renderer, p_margin);
			int[] array = new int[24]
			{
				0, 1, 1, 2, 2, 3, 3, 0, 4, 5,
				5, 6, 6, 7, 7, 4, 0, 4, 1, 5,
				2, 6, 3, 7
			};
			for (int i = 1; i < array.Length; i += 2)
			{
				Vector3 start = p_renderer.transform.TransformPoint(worldBoundCorners[array[i - 1]]);
				Vector3 end = p_renderer.transform.TransformPoint(worldBoundCorners[array[i]]);
				Debug.DrawLine(start, end, p_color, 10f);
			}
		}

		public void GenerateOccludeesData()
		{
			shadowOccludeesRenderer = CollectOccludeesRenderers();
			shadowOccludeesState = new List<ShadowCastingMode>();
			for (int i = 0; i < shadowOccludeesRenderer.Count; i++)
			{
				shadowOccludeesState.Add(shadowOccludeesRenderer[i].shadowCastingMode);
			}
		}

		public List<MeshRenderer> CollectOccludeesRenderers()
		{
			List<MeshRenderer> list = new List<MeshRenderer>();
			for (int i = 0; i < shadowOccludees.Count; i++)
			{
				List<MeshRenderer> collection = CollectShadowOccludeesRenderers(shadowOccludees[i]);
				list.AddRange(collection);
			}
			return list;
		}

		public void DebugOccludeesRenderers()
		{
			for (int i = 0; i < shadowOccludees.Count; i++)
			{
				DebugShadowOverlap(sunLight, shadowOccludees[i], shadowBoundsMargin);
			}
		}

		public void CheckNullOcludeesRenderer()
		{
			Debug.Log("Checking for OcludeeRenderer Null References");
			bool flag = true;
			for (int i = 0; i < shadowOccludeesRenderer.Count; i++)
			{
				if (shadowOccludeesRenderer[i] == null)
				{
					Debug.Log($"Null Reference - Index: {i}");
					flag = false;
				}
			}
			if (flag)
			{
				Debug.Log("No Null References found.");
			}
			else
			{
				Debug.Log("Null References found!");
			}
		}

		private List<MeshRenderer> CollectShadowOccludeesRenderers(GameObject p_container)
		{
			if (!p_container)
			{
				return new List<MeshRenderer>();
			}
			if (!sunLight)
			{
				return new List<MeshRenderer>();
			}
			List<MeshRenderer> list = Hierarchy.FindAll<MeshRenderer>(p_container.transform);
			List<MeshRenderer> list2 = new List<MeshRenderer>();
			for (int i = 0; i < list.Count; i++)
			{
				MeshRenderer meshRenderer = list[i];
				if (ignoreShadowOccludees.Contains(meshRenderer.gameObject))
				{
					continue;
				}
				bool flag = false;
				for (int j = 0; j < ignoreShadowOccludees.Count; j++)
				{
					if (meshRenderer.transform.IsChildOf(ignoreShadowOccludees[j].transform))
					{
						flag = true;
						break;
					}
				}
				if (!flag && IsBoundShadowOverlaped(sunLight, meshRenderer, shadowBoundsMargin) && !list2.Contains(meshRenderer))
				{
					list2.Add(meshRenderer);
				}
			}
			return list2;
		}

		public void ApplyShadowOcclusion()
		{
			for (int i = 0; i < shadowOccludeesRenderer.Count; i++)
			{
				if ((bool)shadowOccludeesRenderer[i])
				{
					if (shadowOccludeesRenderer[i].shadowCastingMode == ShadowCastingMode.ShadowsOnly)
					{
						shadowOccludeesRenderer[i].enabled = false;
					}
					else
					{
						shadowOccludeesRenderer[i].shadowCastingMode = ShadowCastingMode.Off;
					}
				}
			}
		}

		public void RevertShadowOcclusion()
		{
			if (shadowOccludeesRenderer.Count != shadowOccludeesState.Count)
			{
				Debug.LogWarning("DRLMapLightingPreset> RevertShadowOcclusion / Renderers and State List Size Mismatch!");
				return;
			}
			for (int i = 0; i < shadowOccludeesRenderer.Count; i++)
			{
				if ((bool)shadowOccludeesRenderer[i])
				{
					shadowOccludeesRenderer[i].enabled = true;
					shadowOccludeesRenderer[i].shadowCastingMode = shadowOccludeesState[i];
				}
			}
		}
	}
}
