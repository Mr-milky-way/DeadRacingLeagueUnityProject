using System.Collections.Generic;
using UnityEngine;

namespace drl.sim
{
	[ExecuteInEditMode]
	public class DroneSimulationManager : MonoBehaviour
	{
		[SerializeField]
		[HideInInspector]
		private DroneSimulation m_simulation;

		[SerializeField]
		private Transform m_container;

		public DroneSimulation simulation
		{
			get
			{
				if (!m_simulation)
				{
					return m_simulation = GetComponent<DroneSimulation>();
				}
				return m_simulation;
			}
		}

		public Transform container
		{
			get
			{
				if ((bool)m_container)
				{
					return m_container;
				}
				m_container = new GameObject(GetContainerName()).transform;
				m_container.parent = base.transform;
				m_container.localPosition = Vector3.zero;
				m_container.localEulerAngles = Vector3.zero;
				return m_container;
			}
		}

		public virtual string GetContainerName()
		{
			return "manager";
		}

		private void OnEnable()
		{
			m_container = container;
		}

		protected virtual void Start()
		{
		}
	}
	public class DroneSimulationManager<T> : DroneSimulationManager where T : Component
	{
		[SerializeField]
		private List<T> m_list;

		public List<T> list
		{
			get
			{
				if (m_list != null)
				{
					return m_list;
				}
				return m_list = new List<T>();
			}
		}

		public T Any
		{
			get
			{
				if (list == null)
				{
					return null;
				}
				if (list.Count == 0)
				{
					return null;
				}
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] != null)
					{
						return list[i];
					}
				}
				return null;
			}
		}

		public T Get(int p_index)
		{
			if (p_index < 0)
			{
				return null;
			}
			if (p_index >= list.Count)
			{
				return null;
			}
			return list[p_index];
		}

		public void Clear()
		{
			for (int i = 0; i < list.Count; i++)
			{
				Object.Destroy(list[i].gameObject);
			}
			list.Clear();
		}

		public virtual void Add(T p_item)
		{
			if (!list.Contains(p_item))
			{
				list.Add(p_item);
				IndexRename();
			}
		}

		public virtual void Remove(T p_item)
		{
			if (list.Contains(p_item))
			{
				list.Remove(p_item);
				IndexRename();
			}
		}

		public virtual void Insert(T p_item, int p_index = -1)
		{
			if (!list.Contains(p_item))
			{
				if (p_index < 0)
				{
					p_index = list.Count;
				}
				if (p_index >= list.Count)
				{
					p_index = list.Count;
				}
				list.Insert(p_index, p_item);
				IndexRename();
			}
		}

		public virtual T Instantiate(T p_template, int p_index)
		{
			if (!p_template)
			{
				Debug.LogWarning("DroneSimulationManager> Push - Invalid template!");
				return null;
			}
			T val = Object.Instantiate(p_template);
			val.transform.SetParent(base.container, worldPositionStays: true);
			Insert(val, p_index);
			return val;
		}

		public T Instantiate(T p_template)
		{
			return Instantiate(p_template, -1);
		}

		protected void IndexRename()
		{
			for (int i = 0; i < list.Count; i++)
			{
				if ((bool)list[i])
				{
					list[i].name = i.ToString() ?? "";
				}
			}
		}
	}
}
