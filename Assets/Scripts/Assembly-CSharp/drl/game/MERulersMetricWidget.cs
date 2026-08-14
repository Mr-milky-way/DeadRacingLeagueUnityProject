using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class MERulersMetricWidget : MEControlsWidget
	{
		public List<TextMetric> fields;

		public List<Image> positionModeIcons;

		private static Dictionary<MAEntity, bool> m_absolute_flags;

		public MAEntity anchor;

		public List<Transform> targets;

		private Vector3 m_last_p0;

		private Vector3 m_last_p1;

		private Activity m_update_loop;

		public bool useAbsolutePosition
		{
			get
			{
				return GetAbsoluteFlag();
			}
			set
			{
				SetAbsoluteFlag(value);
				RefreshAbsoluteFlag();
			}
		}

		private bool GetAbsoluteFlag()
		{
			if (!anchor)
			{
				return false;
			}
			if (!m_absolute_flags.ContainsKey(anchor))
			{
				return false;
			}
			return m_absolute_flags[anchor];
		}

		private void SetAbsoluteFlag(bool f)
		{
			if ((bool)anchor)
			{
				m_absolute_flags[anchor] = f;
			}
		}

		private void RefreshAbsoluteFlag()
		{
			bool absoluteFlag = GetAbsoluteFlag();
			positionModeIcons[0].gameObject.SetActive(absoluteFlag);
			positionModeIcons[1].gameObject.SetActive(!absoluteFlag);
		}

		protected override void Awake()
		{
			if (m_absolute_flags == null)
			{
				m_absolute_flags = new Dictionary<MAEntity, bool>();
			}
			m_last_p0 = Vector3.one * 10000f;
			m_last_p1 = Vector3.one * 10000f;
			base.Awake();
		}

		public void Set(MAEntity p_anchor, List<Transform> p_targets)
		{
			anchor = p_anchor;
			targets = new List<Transform>(p_targets);
			RefreshAbsoluteFlag();
			if (m_update_loop != null)
			{
				m_update_loop.Stop();
			}
			m_update_loop = Activity.Run(OnUpdate, 0f, false);
			Refresh(p_force: true);
		}

		public void Clear()
		{
			anchor = null;
			targets.Clear();
			useAbsolutePosition = false;
			SetFields("");
			if (m_update_loop != null)
			{
				m_update_loop.Stop();
			}
		}

		public void SetMetricFormat(MEMetricMode p_mode)
		{
			TextMetric.ValueFormat outputFormat = ((p_mode == MEMetricMode.Metric) ? TextMetric.ValueFormat.MetricDistance : TextMetric.ValueFormat.ImperialDistance);
			for (int i = 0; i < fields.Count; i++)
			{
				fields[i].outputFormat = outputFormat;
				fields[i].Refresh();
			}
		}

		public void SetField(int p_id, float p_value, bool p_force)
		{
			if (p_id >= 0 && p_id < fields.Count)
			{
				if (p_force)
				{
					fields[p_id].value = float.PositiveInfinity;
				}
				fields[p_id].value = p_value;
			}
		}

		public void SetField(int p_id, float p_value)
		{
			SetField(p_id, p_value);
		}

		public void SetField(int p_id, string p_value)
		{
			if (p_id >= 0 && p_id < fields.Count)
			{
				fields[p_id].SetText(p_value);
			}
		}

		public void SetFields(string p_value)
		{
			for (int i = 0; i < fields.Count; i++)
			{
				fields[i].SetText(p_value);
			}
		}

		public void SetFields(Vector3 p_value, bool p_force)
		{
			SetField(1, p_value.x, p_force);
			SetField(2, p_value.y, p_force);
			SetField(3, p_value.z, p_force);
		}

		public void SetFields(Vector3 p_value)
		{
			SetFields(p_value, p_force: false);
		}

		public void Refresh(bool p_force = false)
		{
			if (!anchor)
			{
				return;
			}
			Vector3 position = anchor.transform.position;
			Vector3 averagePosition = Hierarchy.GetAveragePosition(targets);
			if (p_force || !((m_last_p0 - position).sqrMagnitude < 0.005f) || !((m_last_p1 - averagePosition).sqrMagnitude < 0.005f))
			{
				m_last_p0 = position;
				m_last_p1 = averagePosition;
				if (useAbsolutePosition)
				{
					SetFields(position, p_force);
					SetField(0, Vector3.Distance(position, averagePosition), p_force);
				}
				else if (targets.Count > 0)
				{
					SetFields(anchor.transform.InverseTransformPoint(averagePosition), p_force);
					SetField(0, Vector3.Distance(position, averagePosition), p_force);
				}
				else
				{
					SetFields("-");
				}
			}
		}

		public void Snap(List<Transform> p_list)
		{
			if (!anchor)
			{
				return;
			}
			Vector3 position = anchor.transform.position;
			Quaternion localRotation = anchor.transform.localRotation;
			if (p_list.Count > 0)
			{
				Vector3 averagePosition = Hierarchy.GetAveragePosition(p_list);
				Quaternion localRotation2 = ((p_list.Count <= 1) ? p_list[0].localRotation : localRotation);
				Transform transform = new GameObject("pivot").transform;
				for (int i = 0; i < p_list.Count; i++)
				{
					Transform obj = p_list[i];
					Transform parent = obj.transform.parent;
					transform.position = averagePosition;
					transform.localRotation = localRotation2;
					obj.SetParent(transform, worldPositionStays: true);
					transform.position = position;
					transform.localRotation = localRotation;
					obj.SetParent(parent, worldPositionStays: true);
				}
				Object.Destroy(transform.gameObject);
			}
		}

		public void Snap()
		{
			Snap(targets);
		}

		public bool OnUpdate()
		{
			if (!anchor)
			{
				return false;
			}
			Refresh();
			return true;
		}
	}
}
