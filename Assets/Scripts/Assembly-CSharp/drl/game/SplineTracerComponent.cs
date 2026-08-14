using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class SplineTracerComponent : MonoBehaviour
	{
		[Header("Spline")]
		public SplineComponent spline;

		public List<TransformVector> samples;

		public float samplesDistance = 0.6f;

		public LineRenderer renderer;

		public int rendererSegments = 70;

		public float traceLength = 15f;

		public float traceOffset = 3f;

		public float traceSmoothness = 0.2f;

		public float traceIndex;

		public Vector3[] rendererPositions;

		[Header("Sections")]
		public SplineTracerSectionComponent sectionTemplate;

		public List<SplineTracerSectionComponent> sections;

		public float sectionMargin = 20f;

		private static bool m_debug_tracing;

		public bool rendererEnabled
		{
			get
			{
				if (!renderer)
				{
					return false;
				}
				return renderer.enabled;
			}
			set
			{
				if ((bool)renderer)
				{
					renderer.enabled = value;
				}
			}
		}

		public void Initialize(SplineComponent p_spline, IList<Vector3> p_sections, int p_laps)
		{
			if ((bool)spline)
			{
				Debug.LogWarning("SplineTracerComponent> Initialize / Already Setup!");
				return;
			}
			spline = p_spline;
			if (!spline)
			{
				Debug.LogWarning("SplineTracerComponent> Initialize / Spline is <null>");
				return;
			}
			if (spline.positions == null)
			{
				Debug.LogWarning("SplineTracerComponent> Initialize / Spline Positions are <null>!", spline);
				return;
			}
			spline.Refresh();
			Vector3Spline positions = spline.positions;
			positions.Refresh();
			if (positions.values.Length == 0)
			{
				Debug.LogWarning("SplineTracerComponent> Initialize / Spline has no Positions!", spline);
				return;
			}
			samples = new List<TransformVector>();
			float length = positions.length;
			float num = Mathf.Max(samplesDistance, 0.05f);
			int a = Mathf.RoundToInt(length / num);
			int num2 = Mathf.Max(1, p_laps);
			TransformVector item = default(TransformVector);
			int num3 = Mathf.Max(a, 2);
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num3; j++)
				{
					float p_ratio = (float)j / (float)(num3 - 1);
					Vector3 normalized = positions.GetNormalized(p_ratio);
					Quaternion identity = Quaternion.identity;
					item.position = normalized;
					item.rotation = identity;
					samples.Add(item);
				}
			}
			if (p_sections.Count > 0)
			{
				item.position = p_sections[p_sections.Count - 1];
				samples.Add(item);
			}
			for (int k = 1; k < samples.Count; k++)
			{
				TransformVector value = samples[k - 1];
				Vector3 forward = samples[k].position - value.position;
				forward.Normalize();
				Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
				value.rotation = rotation;
				samples[k - 1] = value;
			}
			if (samples.Count >= 2)
			{
				item = samples[samples.Count - 1];
				item.rotation = samples[samples.Count - 2].rotation;
				samples[samples.Count - 1] = item;
			}
			for (int l = 0; l < p_sections.Count; l++)
			{
				SplineTracerSectionComponent splineTracerSectionComponent = (sectionTemplate ? Object.Instantiate(sectionTemplate) : null);
				if (!splineTracerSectionComponent)
				{
					splineTracerSectionComponent = new GameObject().AddComponent<SplineTracerSectionComponent>();
				}
				Vector3 position = p_sections[l];
				splineTracerSectionComponent.tracer = this;
				splineTracerSectionComponent.name = l.ToString();
				splineTracerSectionComponent.index = ((l > 0) ? (-1) : 0);
				splineTracerSectionComponent.transform.parent = base.transform;
				splineTracerSectionComponent.transform.position = position;
				sections.Add(splineTracerSectionComponent);
			}
			List<int> list = new List<int>();
			int num4 = 0;
			for (int m = 1; m < sections.Count; m++)
			{
				SplineTracerSectionComponent splineTracerSectionComponent2 = sections[m];
				Vector3 position2 = splineTracerSectionComponent2.transform.position;
				bool flag = false;
				list.Clear();
				for (int n = num4; n < samples.Count; n++)
				{
					bool flag2 = Vector3.Distance(samples[n].position, position2) <= 15f;
					if (flag2)
					{
						list.Add(n);
					}
					if (flag && !flag2)
					{
						break;
					}
					flag = flag2;
				}
				float num5 = 1E+09f;
				for (int num6 = 0; num6 < list.Count; num6++)
				{
					int index = list[num6];
					float num7 = Vector3.Distance(position2, samples[index].position);
					if (!(num7 >= num5))
					{
						num5 = num7;
						splineTracerSectionComponent2.index = index;
						splineTracerSectionComponent2.transform.localRotation = samples[index].rotation;
					}
				}
				num4 = splineTracerSectionComponent2.index + 1;
			}
			if (sections.Count >= 2)
			{
				for (int num8 = 1; num8 < sections.Count; num8++)
				{
					bool flag3 = num8 <= 1;
					_ = sections.Count;
					SplineTracerSectionComponent splineTracerSectionComponent3 = sections[num8 - 1];
					SplineTracerSectionComponent splineTracerSectionComponent4 = sections[num8];
					SplineTracerSectionComponent splineTracerSectionComponent5 = splineTracerSectionComponent3;
					int start = ((!flag3) ? splineTracerSectionComponent5.GetNextSampleIndex(0f - sectionMargin) : 0);
					int nextSampleIndex = splineTracerSectionComponent4.GetNextSampleIndex(sectionMargin);
					splineTracerSectionComponent3.start = start;
					splineTracerSectionComponent3.end = nextSampleIndex;
					if (num8 >= sections.Count - 1)
					{
						int a2 = samples.Count - 1;
						splineTracerSectionComponent4.start = Mathf.Min(a2, splineTracerSectionComponent4.index - 15);
						splineTracerSectionComponent4.end = Mathf.Min(a2, splineTracerSectionComponent4.index + 15);
					}
				}
			}
			Debug.Log(string.Format("SplineTracerComponent> Initialize / spline[{0}m] samples[{1}] sections[{2}] laps[{3}] line-points[{4}]", length.ToString("0.0"), num3, sections.Count, p_laps, rendererSegments), this);
			if ((bool)renderer)
			{
				renderer.positionCount = rendererSegments;
				rendererPositions = new Vector3[rendererSegments];
			}
			ResetTrace();
		}

		public void Initialize<T>(SplineComponent p_spline, IList<T> p_sections, int p_laps) where T : Object
		{
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < p_sections.Count; i++)
			{
				Object obj = p_sections[i];
				if (obj == null)
				{
					continue;
				}
				Vector3 item = Vector3.zero;
				if (obj is GameObject)
				{
					item = (obj as GameObject).transform.position;
				}
				if (obj is Component)
				{
					item = (obj as Component).transform.position;
				}
				if (obj is Collider)
				{
					if (obj is BoxCollider)
					{
						BoxCollider boxCollider = (BoxCollider)obj;
						item = boxCollider.transform.TransformPoint(boxCollider.center);
					}
					else if (obj is SphereCollider)
					{
						SphereCollider sphereCollider = (SphereCollider)obj;
						item = sphereCollider.transform.TransformPoint(sphereCollider.center);
					}
					else if (obj is MeshCollider)
					{
						item = ((MeshCollider)obj).bounds.center;
					}
				}
				list.Add(item);
			}
			Initialize(p_spline, list, p_laps);
		}

		public void SetRendererColor(Color p_color)
		{
			renderer.startColor = p_color;
			renderer.endColor = p_color;
		}

		public Color GetRendererColor()
		{
			return renderer.startColor;
		}

		public int GetClosestSampleIndex(Vector3 p_point)
		{
			return GetClosestSampleIndex(p_point, 0, null);
		}

		public int GetClosestSampleIndex(Vector3 p_point, int p_offset)
		{
			return GetClosestSampleIndex(p_point, p_offset, null);
		}

		internal int GetClosestSampleIndex(Vector3 p_point, List<TransformVector> p_samples)
		{
			return GetClosestSampleIndex(p_point, 0, p_samples);
		}

		internal int GetClosestSampleIndex(Vector3 p_point, int p_offset, List<TransformVector> p_samples)
		{
			List<TransformVector> list = ((p_samples == null) ? samples : p_samples);
			if (list == null)
			{
				return -1;
			}
			if (list.Count <= 0)
			{
				return -1;
			}
			int num = Mathf.Clamp(p_offset, 0, list.Count - 1);
			float num2 = Vector3.Distance(list[num].position, p_point);
			for (int i = p_offset + 1; i < list.Count; i++)
			{
				float num3 = Vector3.Distance(list[i].position, p_point);
				if (num3 < num2)
				{
					num2 = num3;
					num = i;
				}
			}
			return num;
		}

		public Vector3 GetClosestSamplePosition(Vector3 p_point)
		{
			int closestSampleIndex = GetClosestSampleIndex(p_point);
			if (closestSampleIndex < 0)
			{
				return p_point;
			}
			return samples[closestSampleIndex].position;
		}

		public TransformVector GetClosestSample(Vector3 p_point)
		{
			return GetClosestSample(p_point, 0);
		}

		public TransformVector GetClosestSample(Vector3 p_point, int p_offset)
		{
			int value = GetClosestSampleIndex(p_point) + p_offset;
			value = Mathf.Clamp(value, 0, samples.Count - 1);
			if (value < 0)
			{
				return new TransformVector
				{
					position = p_point,
					rotation = Quaternion.identity
				};
			}
			return samples[value];
		}

		public SplineTracerSectionComponent GetSection(int p_index)
		{
			if (sections == null)
			{
				return null;
			}
			int num = Mathf.Clamp(p_index, 0, sections.Count - 1);
			if (num < 0)
			{
				return null;
			}
			if (num >= sections.Count)
			{
				return null;
			}
			return sections[num];
		}

		public SplineTracerSectionComponent GetSectionClamped(int p_index)
		{
			if (sections == null)
			{
				return null;
			}
			if (sections.Count <= 0)
			{
				return null;
			}
			int index = Mathf.Clamp(p_index, 0, sections.Count - 1);
			return sections[index];
		}

		public void Clear()
		{
			for (int i = 0; i < sections.Count; i++)
			{
				if ((bool)sections[i])
				{
					Object.Destroy(sections[i].gameObject);
				}
			}
			sections.Clear();
			spline = null;
			if ((bool)renderer)
			{
				renderer.positionCount = 0;
			}
		}

		public void ResetTrace()
		{
			if ((bool)spline && sections != null && samples != null && sections.Count > 0 && samples.Count > 0)
			{
				Vector3 position = samples[sections[0].index].position;
				traceIndex = sections[0].index;
				RefreshTrace(0, position, p_force: true);
			}
		}

		public void RefreshTrace(int p_section, Vector3 p_position, bool p_force = false)
		{
			if ((bool)spline && (bool)renderer && sections != null && sections.Count > 0)
			{
				int index = Mathf.Clamp(p_section - 1, 0, sections.Count - 1);
				int closestSampleIndex = sections[index].GetClosestSampleIndex(p_position, p_forward_only: true);
				int num = 0;
				traceIndex = Mathf.Lerp(traceIndex, closestSampleIndex, p_force ? 1f : ((traceSmoothness <= 0f) ? 1f : (Time.deltaTime / traceSmoothness)));
				int num2 = Mathf.RoundToInt(traceIndex);
				if (samplesDistance > 0f)
				{
					num = Mathf.RoundToInt(traceOffset / samplesDistance);
				}
				int num3 = num2 + num;
				int num4 = ((samplesDistance <= 0f) ? 1 : Mathf.RoundToInt(traceLength / samplesDistance));
				int num5 = num3 - num4;
				int num6 = num3 + num4;
				int count = samples.Count;
				if (num5 < 0)
				{
					num5 = 0;
				}
				if (num6 >= count)
				{
					num6 = count - 1;
				}
				int num7 = rendererSegments;
				for (int i = 0; i < num7; i++)
				{
					float t = ((num7 <= 1) ? 0f : ((float)i / (float)(num7 - 1)));
					float num8 = (int)Mathf.Lerp(num5, num6, t);
					int index2 = Mathf.Clamp((int)num8, 0, count - 1);
					int index3 = Mathf.Clamp((int)num8 + 1, 0, count - 1);
					float t2 = num8 - Mathf.Floor(num8);
					Vector3 position = samples[index2].position;
					Vector3 position2 = samples[index3].position;
					rendererPositions[i] = Vector3.Lerp(position, position2, t2);
				}
				renderer.SetPositions(rendererPositions);
				if (m_debug_tracing && rendererPositions.Length >= 2)
				{
					Vector3 start = rendererPositions[0];
					Vector3 end = rendererPositions[rendererPositions.Length - 1];
					Debug.DrawLine(start, end, Color.red);
				}
			}
		}

		[ContextMenu("Switch Debug Tracing")]
		private void SwitchDebugTracing()
		{
			m_debug_tracing = !m_debug_tracing;
		}

		[ContextMenu("Debug Samples Path")]
		private void DebugSamplesPath()
		{
			if (samples == null || samples.Count < 2)
			{
				return;
			}
			for (int i = 1; i < samples.Count; i++)
			{
				float t = (float)i / (float)(samples.Count - 1);
				Vector3 position = samples[i - 1].position;
				Vector3 position2 = samples[i].position;
				bool flag = (i & 1) == 0;
				bool flag2 = false;
				for (int j = 0; j < sections.Count; j++)
				{
					if (Mathf.Abs(sections[j].index - i) <= 5)
					{
						flag2 = true;
						break;
					}
				}
				Color color = (flag2 ? new Color(0.3f, 0f, 0f) : Color.black);
				Color color2 = (flag2 ? new Color(1f, 0f, 0f) : Color.green);
				position.y += Mathf.Lerp(-0.5f, 0.5f, t);
				position2.y += Mathf.Lerp(-0.5f, 0.5f, t);
				Debug.DrawLine(position, position2, flag ? color : color2, 30f);
			}
		}

		[ContextMenu("Debug Samples Axis")]
		private void DebugSamplesAxis()
		{
			if (samples != null)
			{
				for (int i = 0; i < samples.Count; i += 2)
				{
					Vector3 position = samples[i].position;
					Quaternion rotation = samples[i].rotation;
					Vector3 vector = rotation * Vector3.right;
					Vector3 vector2 = rotation * Vector3.up;
					Vector3 vector3 = rotation * Vector3.forward;
					Debug.DrawLine(position, position + vector * 0.33f, Color.red, 30f, depthTest: false);
					Debug.DrawLine(position, position + vector2 * 0.33f, Color.green, 30f, depthTest: false);
					Debug.DrawLine(position, position + vector3 * 0.33f, Color.blue, 30f, depthTest: false);
				}
			}
		}
	}
}
