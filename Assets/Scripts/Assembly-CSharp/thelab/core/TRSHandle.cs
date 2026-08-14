using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class TRSHandle : ComponentHandle<Transform>
	{
		public GizmoHandle current;

		public List<Transform> transforms;

		public bool global;

		public float snap;

		public Vector3 handlePivotPosition;

		public Quaternion handlePivotRotation;

		public bool adjustHierarchy = true;

		public bool flatHierarchy;

		protected Transform m_anchor;

		protected Vector3 m_delta;

		private List<Transform> m_parents;

		private List<int> m_indexes;

		public List<Vector3> hierarchyPositions;

		public List<Vector3> hierarchyScales;

		public int currentIndex
		{
			get
			{
				if (!current)
				{
					return -1;
				}
				return handles.IndexOf(current);
			}
		}

		public static Vector3 GetAnchorPosition(IList p_targets)
		{
			Vector3 zero = Vector3.zero;
			int num = 0;
			for (int i = 0; i < p_targets.Count; i++)
			{
				Transform transformByIndex = GetTransformByIndex(p_targets, i);
				if ((bool)transformByIndex)
				{
					Vector3 position = transformByIndex.position;
					zero += position;
					num++;
				}
			}
			float num2 = ((num <= 0) ? 0f : (1f / (float)num));
			return zero * num2;
		}

		public static Transform GetAnchor(IList p_targets, string p_name = "anchor-transform", Transform p_anchor = null, bool p_global = false)
		{
			IList list;
			if (p_targets != null)
			{
				list = p_targets;
			}
			else
			{
				IList list2 = new Transform[0];
				list = list2;
			}
			IList list3 = list;
			if (list3.Count <= 0)
			{
				return null;
			}
			Transform transformByIndex = GetTransformByIndex(list3, 0);
			bool num = list3.Count > 1 || p_global;
			Vector3 position = (num ? GetAnchorPosition(list3) : (transformByIndex ? transformByIndex.position : Vector3.zero));
			Quaternion localRotation = (num ? Quaternion.identity : (transformByIndex ? transformByIndex.localRotation : Quaternion.identity));
			Vector3 localScale = (num ? Vector3.one : (transformByIndex ? transformByIndex.localScale : Vector3.one));
			Transform obj = (p_anchor ? p_anchor : new GameObject(p_name).transform);
			obj.position = position;
			obj.localRotation = localRotation;
			obj.localScale = localScale;
			return obj;
		}

		protected static Transform GetTransformByIndex(IList p_list, int p_index)
		{
			if (p_list == null)
			{
				return null;
			}
			if (p_index < 0)
			{
				return null;
			}
			if (p_index >= p_list.Count)
			{
				return null;
			}
			GameObject gameObject = p_list[p_index] as GameObject;
			Component component = p_list[p_index] as Component;
			if (!gameObject)
			{
				if (!component)
				{
					return null;
				}
				return component.transform;
			}
			return gameObject.transform;
		}

		protected override void OnTargetsAdd(List<Transform> p_list)
		{
			if (transforms == null)
			{
				transforms = new List<Transform>();
			}
			List<Transform> list = new List<Transform>(p_list);
			if (adjustHierarchy)
			{
				for (int i = 0; i < list.Count; i++)
				{
					Transform parent = list[i];
					for (int j = i + 1; j < list.Count; j++)
					{
						if (list[j].IsChildOf(parent))
						{
							list.RemoveAt(j--);
						}
					}
				}
			}
			if (flatHierarchy)
			{
				List<Transform> list2 = new List<Transform>();
				for (int k = 0; k < list.Count; k++)
				{
					List<Transform> collection = Hierarchy.FindAll<Transform>(list[k]);
					list2.AddRange(collection);
				}
				list = list2;
				for (int l = 0; l < list.Count; l++)
				{
					Transform transform = list[l];
					for (int m = l + 1; m < list.Count; m++)
					{
						Transform transform2 = list[m];
						if (transform == transform2)
						{
							list.RemoveAt(m--);
						}
					}
				}
			}
			transforms = list;
			list.Sort(SortBySiblingIndex);
			RefreshTransform();
		}

		protected Vector3 GetTransformsPivot()
		{
			return GetAnchorPosition(transforms);
		}

		protected virtual void RefreshTransform()
		{
			bool flag = transforms.Count <= 0;
			handlePivotPosition = (flag ? base.transform.position : GetTransformsPivot());
			flag = global || transforms.Count >= 2;
			handlePivotRotation = (flag ? Quaternion.identity : transforms[0].rotation);
			base.transform.position = handlePivotPosition;
			base.transform.rotation = handlePivotRotation;
		}

		public void Refresh()
		{
			RefreshTransform();
		}

		protected void StoreTransformData(bool p_use_anchor)
		{
			if (hierarchyPositions == null)
			{
				hierarchyPositions = new List<Vector3>();
			}
			if (hierarchyScales == null)
			{
				hierarchyScales = new List<Vector3>();
			}
			Transform parent = (m_anchor ? m_anchor : base.transform);
			hierarchyPositions.Clear();
			hierarchyScales.Clear();
			List<Transform> list = transforms;
			for (int i = 0; i < list.Count; i++)
			{
				Transform transform = list[i];
				Transform parent2 = transform.parent;
				Vector3 localScale = transform.localScale;
				int siblingIndex = transform.GetSiblingIndex();
				if (p_use_anchor)
				{
					transform.SetParent(parent, worldPositionStays: true);
				}
				hierarchyPositions.Add(transform.transform.localPosition);
				hierarchyScales.Add(transform.transform.localScale);
				if (p_use_anchor)
				{
					transform.SetParent(parent2, worldPositionStays: true);
					transform.localScale = localScale;
				}
				transform.SetSiblingIndex(siblingIndex);
			}
		}

		protected virtual void ApplyHandleValue(Vector3 p_delta)
		{
		}

		protected virtual void InitHandleValue(Vector3 p_delta)
		{
		}

		protected virtual void UpdateHandle(Vector3 p_delta)
		{
			if (currentIndex < 0)
			{
				return;
			}
			List<Transform> list = transforms;
			List<Transform> list2 = ((m_parents == null) ? (m_parents = new List<Transform>()) : m_parents);
			List<int> list3 = ((m_indexes == null) ? (m_indexes = new List<int>()) : m_indexes);
			Transform parent = (m_anchor ? m_anchor : base.transform);
			list3.Clear();
			list2.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				if ((bool)list[i])
				{
					list3.Add(list[i].GetSiblingIndex());
					list2.Add(list[i].parent);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				if ((bool)list[j])
				{
					list[j].SetParent(parent, worldPositionStays: true);
				}
			}
			InitHandleValue(p_delta);
			ApplyHandleValue(p_delta);
			for (int k = 0; k < list.Count; k++)
			{
				if ((bool)list[k])
				{
					list[k].SetParent(list2[k], worldPositionStays: true);
				}
			}
			for (int l = 0; l < list.Count; l++)
			{
				if ((bool)list[l])
				{
					list[l].SetSiblingIndex(list3[l]);
				}
			}
		}

		protected int SortBySiblingIndex(Transform a, Transform b)
		{
			if (a.GetSiblingIndex() >= b.GetSiblingIndex())
			{
				return 1;
			}
			return -1;
		}

		protected virtual void SetFocus(GizmoHandle p_handle)
		{
			current = p_handle;
			for (int i = 0; i < handles.Count; i++)
			{
				handles[i].enabled = !p_handle || handles[i] == p_handle;
			}
		}

		protected int GetHandleKeyboardActiveCount()
		{
			int num = 0;
			for (int i = 0; i < handles.Count; i++)
			{
				if (handles[i].keyboard.active)
				{
					num++;
				}
			}
			return num;
		}

		public void SetHandleEnabled(int p_index, bool p_flag)
		{
			if (p_index >= 0 && p_index < handles.Count)
			{
				handles[p_index].gameObject.SetActive(p_flag);
			}
		}
	}
}
