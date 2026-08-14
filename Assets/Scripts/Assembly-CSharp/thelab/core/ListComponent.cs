using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	[ExecuteInEditMode]
	public class ListComponent : ListComponent<Component>
	{
	}
	[ExecuteInEditMode]
	public class ListComponent<T> : BaseListComponent where T : Component
	{
		public T template;

		public List<T> list;

		public List<T> filter;

		public List<Transform> ignore;

		public int siblingIndexOffset;

		public bool indexNameItems = true;

		protected Predicate<T> m_last_filter;

		protected bool m_need_layout;

		public T this[int i] => list[i];

		public override int Count
		{
			get
			{
				if (list != null)
				{
					return list.Count;
				}
				return 0;
			}
		}

		protected virtual void Awake()
		{
			Prune();
		}

		public T Insert(int p_index, T p_item)
		{
			int index = Mathf.Clamp(p_index, 0, list.Count);
			T val = (p_item ? p_item : GetInstance());
			if ((bool)p_item)
			{
				p_item.transform.SetParent(base.transform, worldPositionStays: false);
			}
			if (!val)
			{
				Debug.LogWarning("ListComponent> [" + base.name + "] failed to create instance.");
				return val;
			}
			list.Insert(index, val);
			RefreshHierarchy();
			if (m_last_filter != null)
			{
				ApplyFilter(val, m_last_filter(val));
			}
			m_need_layout = true;
			InvokeEvent(ListEvent.Type.Added, val.gameObject);
			return val;
		}

		public T Insert(int p_index)
		{
			return Insert(p_index, null);
		}

		public T Push(T p_item)
		{
			return Insert(list.Count, p_item);
		}

		public T Push()
		{
			return Insert(list.Count, null);
		}

		public U Push<U>() where U : Component
		{
			return Push().GetComponent<U>();
		}

		public T Unshift(T p_item)
		{
			return Insert(0, p_item);
		}

		public T Unshift()
		{
			return Insert(0, null);
		}

		public U Unshift<U>() where U : Component
		{
			return Unshift().GetComponent<U>();
		}

		public void Remove(T p_item, bool p_destroy = false)
		{
			if (p_destroy)
			{
				UnityEngine.Object.Destroy(p_item.gameObject, 1f / 30f);
			}
			p_item.gameObject.SetActive(value: false);
			list.Remove(p_item);
			RefreshHierarchy();
			if (filter.IndexOf(p_item) >= 0)
			{
				filter.Remove(p_item);
			}
			m_need_layout = true;
			InvokeEvent(ListEvent.Type.Removed, p_item.gameObject);
		}

		public void Remove(int p_index, bool p_destroy = false)
		{
			if (p_index >= 0 && p_index < list.Count)
			{
				T p_item = list[p_index];
				Remove(p_item, p_destroy);
			}
		}

		public void Pop(bool p_destroy = false)
		{
			Remove(list.Count - 1, p_destroy);
		}

		public void Shift(bool p_destroy = false)
		{
			Remove(0, p_destroy);
		}

		public void Remove(Predicate<T> p_callback, bool p_destroy = false)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (p_callback(list[i]))
				{
					Remove(list[i], p_destroy);
					break;
				}
			}
		}

		public void RemoveAll(Predicate<T> p_callback, bool p_destroy = false)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (p_callback(list[i]))
				{
					Remove(list[i], p_destroy);
				}
			}
		}

		public void Clear(bool p_destroy)
		{
			for (int i = 0; i < list.Count; i++)
			{
				T val = list[i];
				if (p_destroy && (bool)val)
				{
					UnityEngine.Object.Destroy(val.gameObject, 1f / 30f);
				}
				if ((bool)val)
				{
					val.gameObject.SetActive(value: false);
				}
			}
			list.Clear();
			m_need_layout = true;
		}

		public void Clear()
		{
			Clear(p_destroy: false);
		}

		public List<U> GetList<U>() where U : Component
		{
			List<U> list = new List<U>();
			for (int i = 0; i < Count; i++)
			{
				T val = this[i];
				if (val == null)
				{
					continue;
				}
				if (val is U)
				{
					list.Add((U)(Component)val);
					continue;
				}
				U component = val.GetComponent<U>();
				if ((bool)component)
				{
					list.Add(component);
				}
			}
			return list;
		}

		public U Get<U>(int p_id) where U : Component
		{
			if (p_id < 0)
			{
				return null;
			}
			if (p_id >= Count)
			{
				return null;
			}
			if (list[p_id] == null)
			{
				return null;
			}
			return this[p_id].GetComponent<U>();
		}

		public void Filter(Predicate<T> p_query)
		{
			ClearFilter();
			m_last_filter = p_query;
			if (m_last_filter != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					T val = list[i];
					bool p_result = p_query(val);
					ApplyFilter(val, p_result);
				}
				m_need_layout = true;
				InvokeEvent(ListEvent.Type.Filter);
			}
		}

		public void ClearFilter()
		{
			filter.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				if ((bool)list[i])
				{
					list[i].gameObject.SetActive(value: true);
				}
			}
			m_last_filter = null;
			m_need_layout = true;
		}

		protected virtual void ApplyFilter(T p_target, bool p_result)
		{
			p_target.gameObject.SetActive(p_result);
		}

		public T Find(Predicate<T> p_callback)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (p_callback(list[i]))
				{
					return list[i];
				}
			}
			return null;
		}

		public T[] FindAll(Predicate<T> p_callback)
		{
			List<T> list = new List<T>();
			for (int i = 0; i < this.list.Count; i++)
			{
				if (p_callback(this.list[i]))
				{
					list.Add(this.list[i]);
				}
			}
			return list.ToArray();
		}

		public void Sort(Comparison<T> p_callback)
		{
			if (p_callback == null)
			{
				return;
			}
			this.list.Sort(p_callback);
			List<int> list = new List<int>();
			for (int i = 0; i < ignore.Count; i++)
			{
				list.Add(ignore[i].GetSiblingIndex());
			}
			for (int j = 0; j < this.list.Count; j++)
			{
				T val = this.list[j];
				if ((bool)val)
				{
					val.transform.SetSiblingIndex(j + 1);
				}
			}
			RefreshHierarchy();
			for (int k = 0; k < ignore.Count; k++)
			{
				ignore[k].SetSiblingIndex(list[k]);
			}
			m_need_layout = true;
			InvokeEvent(ListEvent.Type.Sort);
		}

		private void RefreshHierarchy()
		{
			int num = Mathf.Max(0, this.list.Count - 1);
			int num2 = Mathf.Max(0, (int)Mathf.Log10(num)) + 1;
			List<int> list = new List<int>();
			for (int i = 0; i < ignore.Count; i++)
			{
				list.Add(ignore[i].GetSiblingIndex());
			}
			for (int j = 0; j < this.list.Count; j++)
			{
				if (this.list[j] != null && this.list[j].transform != null)
				{
					this.list[j].transform.SetSiblingIndex(j + siblingIndexOffset);
					if (indexNameItems)
					{
						this.list[j].name = j.ToString("D" + num2);
					}
				}
			}
			for (int k = 0; k < ignore.Count; k++)
			{
				ignore[k].SetSiblingIndex(list[k]);
			}
		}

		protected T GetInstance()
		{
			if (!this)
			{
				return null;
			}
			if (!base.transform)
			{
				return null;
			}
			T val = null;
			int childCount = base.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				if (!ignore.Contains(child))
				{
					T component = child.GetComponent<T>();
					if (!(component == template) && list.IndexOf(component) < 0)
					{
						val = component;
						break;
					}
				}
			}
			if (!val && (bool)template)
			{
				val = UnityEngine.Object.Instantiate(template);
				val.transform.SetParent(base.transform, worldPositionStays: false);
				val.name = "new-item";
			}
			if ((bool)val)
			{
				val.gameObject.SetActive(value: true);
				val.transform.SetSiblingIndex(childCount);
			}
			return val;
		}

		private void LateUpdate()
		{
			if (list == null)
			{
				m_need_layout = false;
				return;
			}
			if (filter == null)
			{
				m_need_layout = false;
				return;
			}
			if (base.transform.hasChanged)
			{
				m_need_layout = true;
				base.transform.hasChanged = false;
				Prune();
			}
			if (m_need_layout)
			{
				OnLayout();
				InvokeEvent(ListEvent.Type.Layout);
				m_need_layout = false;
			}
		}

		internal void Prune()
		{
			if (list == null)
			{
				list = new List<T>();
			}
			if (filter == null)
			{
				filter = new List<T>();
			}
			if (ignore == null)
			{
				ignore = new List<Transform>();
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i])
				{
					list.RemoveAt(i--);
				}
			}
			for (int j = 0; j < filter.Count; j++)
			{
				if (!filter[j])
				{
					filter.RemoveAt(j--);
				}
			}
			if (Application.isPlaying)
			{
				return;
			}
			list.Clear();
			for (int k = 0; k < base.transform.childCount; k++)
			{
				T component = base.transform.GetChild(k).GetComponent<T>();
				if ((bool)component && !ignore.Contains(component.transform) && !(component == template) && component.gameObject.activeInHierarchy)
				{
					list.Add(component);
				}
			}
			RefreshHierarchy();
		}
	}
}
