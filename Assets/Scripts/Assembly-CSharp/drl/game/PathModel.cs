using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class PathModel : Model<DRLApp>
	{
		public class LookupNode : MonoBehaviour
		{
			public float ratio;

			public float distance;
		}

		public bool hasSpline;

		[Header("Spline")]
		public SplineRenderer path;

		public float length;

		[SerializeField]
		[Range(-0.1f, 1f)]
		private float m_path_alpha;

		public LookupNode closest;

		public LookupNode end;

		public List<LookupNode> lookup;

		[Header("Guide")]
		public LineRenderer guide;

		public AnimationCurve guideWidthCurve;

		public int guideSamples = 15;

		public float guideOffset;

		[Header("Debug")]
		public Transform debugTarget;

		public bool debugNodes;

		protected float m_next_r0;

		protected float m_next_r1;

		protected Vector3[] m_normalized_cache;

		private float m_progress_elapsed;

		private float m_progress_spf = 0.025f;

		public SplineComponent spline
		{
			get
			{
				if (!hasSpline)
				{
					return null;
				}
				return path.spline;
			}
			set
			{
				path.spline = value;
			}
		}

		public float pathAlpha
		{
			get
			{
				return m_path_alpha;
			}
			set
			{
				float f = Mathf.Abs(value - m_path_alpha);
				m_path_alpha = value;
				if (Mathf.Abs(f) > 0f)
				{
					SetPathAlpha(m_path_alpha);
				}
			}
		}

		protected void Awake()
		{
			GenerateLookup();
			SetPathAlpha(m_path_alpha);
		}

		public void GenerateLookup()
		{
			Transform c = base.transform.Find("lookup");
			lookup = new List<LookupNode>();
			int k = 0;
			Activity.Run((Func<bool>)delegate
			{
				if (k > 200)
				{
					UpdateLookup();
					return false;
				}
				for (int i = 0; i < 50; i++)
				{
					LookupNode lookupNode = ((k >= lookup.Count) ? null : lookup[k]);
					if (!lookupNode)
					{
						GameObject obj = new GameObject(k.ToString() ?? "");
						obj.transform.SetParent(c);
						obj.transform.localPosition = Vector3.zero;
						lookupNode = obj.AddComponent<LookupNode>();
						lookup.Add(lookupNode);
					}
					k++;
				}
				return true;
			}, 0f, false);
			m_normalized_cache = new Vector3[5000];
			for (int num = 0; num < m_normalized_cache.Length; num++)
			{
				m_normalized_cache[num].x = float.NaN;
			}
		}

		public void SetPathAlpha(float p_value)
		{
			m_path_alpha = p_value;
			LineRenderer lineRenderer = (path ? path.renderer : null);
			if ((bool)lineRenderer)
			{
				Gradient colorGradient = lineRenderer.colorGradient;
				GradientAlphaKey[] alphaKeys = colorGradient.alphaKeys;
				GradientColorKey[] colorKeys = colorGradient.colorKeys;
				for (int i = 0; i < alphaKeys.Length; i++)
				{
					GradientAlphaKey gradientAlphaKey = alphaKeys[i];
					gradientAlphaKey.alpha = Mathf.Clamp01(p_value);
					alphaKeys[i] = gradientAlphaKey;
				}
				colorGradient.SetKeys(colorKeys, alphaKeys);
				lineRenderer.colorGradient = colorGradient;
				lineRenderer.enabled = p_value >= 0f;
			}
		}

		public void UpdateLookup()
		{
			if (!spline)
			{
				Debug.LogWarning("PathModel> UpdateLookup - Spline is null!");
				return;
			}
			float num = 0f;
			float num2 = lookup.Count - 1;
			num2 = ((num2 <= 0f) ? 0f : (1f / num2));
			float num3 = (length = spline.positions.length);
			for (int i = 0; i < lookup.Count; i++)
			{
				LookupNode lookupNode = lookup[i];
				float num4 = num3 * num;
				lookupNode.ratio = num;
				lookupNode.distance = num4;
				Vector3 position = spline.positions.Get(num4);
				lookupNode.transform.position = position;
				if (i > 0)
				{
					lookup[i - 1].transform.LookAt(lookupNode.transform, Vector3.up);
				}
				num += num2;
			}
		}

		public Vector3Int GetProximity(Vector3 p_position, float p_minDistance = -1f, float p_maxDistance = -1f, float p_guide_offset = -1f, LookupNode p_closest = null, int p_hint = 0)
		{
			Vector3Int result = default(Vector3Int);
			int num = (p_closest ? lookup.IndexOf(p_closest) : p_hint);
			int num2 = 15;
			int num3 = Mathf.Max(0, num - num2);
			int num4 = Mathf.Min(num + num2, lookup.Count - 1);
			if (p_minDistance > -1f)
			{
				for (int i = 1; i < lookup.Count; i++)
				{
					if (lookup[i].distance > p_minDistance)
					{
						num3 = i - 1;
						break;
					}
				}
			}
			if (p_maxDistance > -1f)
			{
				for (int num5 = lookup.Count - 2; num5 > 0; num5--)
				{
					if (lookup[num5].distance < p_maxDistance)
					{
						num4 = num5 + 1;
						break;
					}
				}
			}
			LookupNode lookupNode = ((lookup.Count <= 0) ? null : lookup[num3]);
			float num6 = (lookupNode ? Vector3.Distance(lookupNode.transform.position, p_position) : 999999f);
			int value = num3;
			for (int j = num3 + 1; j <= num4; j++)
			{
				LookupNode lookupNode2 = lookup[j];
				_ = lookupNode2.transform.position;
				float num7 = Vector3.Distance(p_position, lookupNode2.transform.position);
				if (num7 < num6)
				{
					lookupNode = lookupNode2;
					num6 = num7;
					value = j;
				}
			}
			result.x = Mathf.Clamp(num3, 0, lookup.Count - 1);
			result.y = Mathf.Clamp(num4, 0, lookup.Count - 1);
			result.z = Mathf.Clamp(value, 0, lookup.Count - 1);
			return result;
		}

		public void UpdateProgress(Vector3 p_position, int p_hint, float p_minDistance = -1f, float p_maxDistance = -1f, float p_guide_offset = -1f)
		{
			if (!spline || lookup.Count <= 0)
			{
				return;
			}
			m_progress_elapsed += Time.unscaledDeltaTime;
			if (!(m_progress_elapsed < m_progress_spf))
			{
				m_progress_elapsed = 0f;
				Vector3Spline positions = spline.positions;
				LookupNode lookupNode = null;
				int num = 15;
				int num2 = (closest ? lookup.IndexOf(closest) : p_hint);
				Vector3Int proximity = GetProximity(p_position, p_minDistance, p_maxDistance, p_guide_offset, closest, p_hint);
				int x = proximity.x;
				int y = proximity.y;
				int z = proximity.z;
				lookupNode = lookup[z];
				if (debugNodes)
				{
					Debug.DrawLine(p_position, lookupNode.transform.position, new Color(1f, 0f, 1f, 0.5f));
				}
				z = Mathf.Clamp(z + 4, 0, lookup.Count - 1);
				closest = lookup[z];
				if (debugNodes)
				{
					Debug.DrawLine(p_position, closest.transform.position, new Color(0f, 1f, 1f, 0.5f));
				}
				Mathf.Clamp01(((closest ? Vector3.Distance(closest.transform.position, p_position) : 0f) - 20f) / 20f);
				num = guideSamples;
				x = Mathf.Max(0, num2 - num);
				y = Mathf.Min(num2 + num, lookup.Count - 1);
				float ratio = lookup[x].ratio;
				float ratio2 = lookup[y].ratio;
				float num3 = ((p_guide_offset < 0f) ? guideOffset : p_guide_offset);
				float num4 = ((length <= 0f) ? 0f : (num3 / length));
				ratio += num4;
				ratio2 += num4;
				float t = Time.deltaTime * 8f;
				m_next_r0 = Mathf.Lerp(m_next_r0, ratio, t);
				m_next_r1 = Mathf.Lerp(m_next_r1, ratio2, t);
				int positionCount = guide.positionCount;
				Vector3[] normalized_cache = m_normalized_cache;
				for (int i = 0; i < positionCount; i++)
				{
					float t2 = (float)i / (float)(positionCount - 1);
					float num5 = Mathf.Clamp01(Mathf.Lerp(m_next_r0, m_next_r1, t2));
					int num6 = Mathf.FloorToInt(num5 * ((float)normalized_cache.Length - 1f));
					Vector3 position = ((!float.IsNaN(normalized_cache[num6].x)) ? normalized_cache[num6] : (normalized_cache[num6] = positions.GetNormalized(num5)));
					guide.SetPosition(i, position);
				}
				float value = guideWidthCurve.Evaluate(lookupNode.ratio);
				guide.widthMultiplier = Mathf.Clamp01(value);
			}
		}

		public void ResetQuery()
		{
			closest = null;
		}

		protected void Update()
		{
			if ((bool)debugTarget)
			{
				UpdateProgress(debugTarget.position, 0);
			}
		}
	}
}
