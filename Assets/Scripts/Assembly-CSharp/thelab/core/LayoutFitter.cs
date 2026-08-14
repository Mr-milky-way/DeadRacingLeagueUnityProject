using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	[ExecuteInEditMode]
	public class LayoutFitter : MonoBehaviour
	{
		internal enum State
		{
			Idle = 0,
			Collect = 1,
			Process = 2,
			Apply = 3
		}

		public List<RectTransform> targets;

		public bool includeChildren;

		public float marginLeft;

		public float marginRight;

		public float marginTop;

		public float marginBottom;

		public Vector2 offset;

		public int maxDepth = 10;

		private RectTransform _rt;

		private int m_dirty_frames;

		private Vector3[] m_corners;

		private Vector4 m_last_margin;

		private Vector2 m_last_offset;

		private Dictionary<int, Graphic> m_component_cache;

		internal State state;

		internal Bounds bounds;

		internal Bounds backBounds;

		internal List<RectTransform> hierarchy;

		internal int process_iterator;

		protected RectTransform m_rt
		{
			get
			{
				if (!_rt)
				{
					return _rt = GetComponent<RectTransform>();
				}
				return _rt;
			}
		}

		public void Refresh()
		{
			for (int i = 0; i < targets.Count; i++)
			{
				if ((bool)targets[i])
				{
					targets[i].hasChanged = true;
				}
			}
		}

		protected void LateUpdate()
		{
			if (targets == null)
			{
				targets = new List<RectTransform>();
			}
			if (targets.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < targets.Count; i++)
			{
				if ((bool)targets[i] && targets[i].hasChanged)
				{
					targets[i].hasChanged = false;
					m_dirty_frames = 4;
				}
			}
			Vector4 vector = new Vector4(marginLeft, marginRight, marginTop, marginBottom);
			if (Vector4.Distance(m_last_margin, vector) > 0f)
			{
				m_dirty_frames = 4;
			}
			if (Vector2.Distance(m_last_offset, offset) > 0f)
			{
				m_dirty_frames = 4;
			}
			m_last_margin = vector;
			m_last_offset = offset;
			RectTransform rt = m_rt;
			switch (state)
			{
			case State.Idle:
				if (m_dirty_frames > 0)
				{
					m_dirty_frames--;
					state = State.Collect;
				}
				break;
			case State.Collect:
			{
				this.bounds = default(Bounds);
				if (hierarchy == null)
				{
					hierarchy = new List<RectTransform>();
				}
				hierarchy.Clear();
				for (int k = 0; k < targets.Count; k++)
				{
					RectTransform rectTransform2 = targets[k];
					if ((bool)rectTransform2 && includeChildren)
					{
						CollectChildren(hierarchy, rectTransform2, 0, (maxDepth <= 0) ? 1000000 : maxDepth);
					}
				}
				process_iterator = 0;
				state = State.Process;
				break;
			}
			case State.Process:
			{
				int num = 0;
				List<RectTransform> list = (includeChildren ? hierarchy : targets);
				num = list.Count;
				int num2 = num / 2;
				for (int j = 0; j < num2; j++)
				{
					int num3 = process_iterator;
					process_iterator++;
					if (num3 >= num)
					{
						break;
					}
					RectTransform rectTransform = list[num3];
					if ((bool)rectTransform)
					{
						Encapsulate(ref this.bounds, rectTransform);
					}
				}
				if (process_iterator >= num)
				{
					state = State.Apply;
				}
				break;
			}
			case State.Apply:
				backBounds = this.bounds;
				state = State.Idle;
				break;
			}
			Bounds bounds = backBounds;
			bounds.min -= new Vector3(vector.x, vector.w);
			bounds.max += new Vector3(vector.y, vector.z);
			rt.anchoredPosition = bounds.center;
			rt.anchoredPosition += offset;
			rt.sizeDelta = bounds.size;
		}

		protected void CollectChildren(List<RectTransform> p_list, Transform p_target, int p_depth, int p_max)
		{
			if (p_depth >= p_max)
			{
				return;
			}
			int num = (p_target ? p_target.childCount : 0);
			if (m_component_cache == null)
			{
				m_component_cache = new Dictionary<int, Graphic>();
			}
			for (int i = 0; i < num; i++)
			{
				Transform child = p_target.GetChild(i);
				if ((bool)child && child.gameObject.activeInHierarchy)
				{
					int instanceID = child.GetInstanceID();
					Graphic graphic = null;
					if (m_component_cache.ContainsKey(instanceID))
					{
						graphic = m_component_cache[instanceID];
					}
					else
					{
						graphic = child.GetComponent<Graphic>();
						m_component_cache[instanceID] = graphic;
					}
					if ((bool)graphic)
					{
						p_list.Add(child as RectTransform);
					}
					CollectChildren(p_list, child, p_depth + 1, p_max);
				}
			}
		}

		protected void EncapsulateChildren(ref Bounds p_bounds, Transform p_target, int p_depth, int p_max)
		{
			if (p_depth >= p_max)
			{
				return;
			}
			int num = (p_target ? p_target.childCount : 0);
			if (m_component_cache == null)
			{
				m_component_cache = new Dictionary<int, Graphic>();
			}
			for (int i = 0; i < num; i++)
			{
				Transform child = p_target.GetChild(i);
				if ((bool)child && child.gameObject.activeInHierarchy)
				{
					int instanceID = child.GetInstanceID();
					Graphic graphic = null;
					if (m_component_cache.ContainsKey(instanceID))
					{
						graphic = m_component_cache[instanceID];
					}
					else
					{
						graphic = child.GetComponent<Graphic>();
						m_component_cache[instanceID] = graphic;
					}
					if ((bool)graphic)
					{
						Encapsulate(ref p_bounds, (RectTransform)child);
					}
					EncapsulateChildren(ref p_bounds, child, p_depth + 1, p_max);
				}
			}
		}

		protected void Encapsulate(ref Bounds p_bounds, RectTransform p_target)
		{
			if (!p_target)
			{
				return;
			}
			Bounds bounds = p_bounds;
			if (m_corners == null)
			{
				m_corners = new Vector3[4]
				{
					Vector3.zero,
					Vector3.zero,
					Vector3.zero,
					Vector3.zero
				};
			}
			if (m_corners != null && m_corners.Length == 4)
			{
				p_target.GetWorldCorners(m_corners);
			}
			Transform parent = base.transform.parent;
			for (int i = 0; i < m_corners.Length; i++)
			{
				Vector3 vector = m_corners[i];
				vector = (parent ? parent.InverseTransformPoint(vector) : vector);
				if (!bounds.Contains(vector))
				{
					bounds.Encapsulate(vector);
				}
			}
			p_bounds = bounds;
		}
	}
}
