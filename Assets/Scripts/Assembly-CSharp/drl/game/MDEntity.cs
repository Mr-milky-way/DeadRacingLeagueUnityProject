using System.Collections.Generic;
using UnityEngine;

namespace drl.game
{
	public class MDEntity : MDObject
	{
		private MDEntity m_parent;

		private List<MDEntity> m_children;

		public MDEntity parent
		{
			get
			{
				return m_parent;
			}
			set
			{
				parentId = "";
				if (m_parent != null)
				{
					m_parent.RemoveChild(this);
				}
				m_parent = value;
				if (m_parent != null)
				{
					parentId = m_parent.id;
					m_parent.AddChild(this);
				}
			}
		}

		public string parentId
		{
			get
			{
				return Get("parent-id", "");
			}
			set
			{
				Set("parent-id", value);
			}
		}

		protected List<MDEntity> children
		{
			get
			{
				return m_children = GetCast<List<MDEntity>>("children", null, p_add: true);
			}
			set
			{
				Set("children", m_children = value);
			}
		}

		public int childCount
		{
			get
			{
				if (m_children != null)
				{
					return m_children.Count;
				}
				return 0;
			}
		}

		public Vector3 localPosition
		{
			get
			{
				return GetVector3("local-position", Vector3.zero);
			}
			set
			{
				SetVector3("local-position", value);
			}
		}

		public Quaternion localRotation
		{
			get
			{
				return GetQuaternion("local-rotation", Quaternion.identity);
			}
			set
			{
				SetQuaternion("local-rotation", value);
			}
		}

		public Vector3 localEuler
		{
			get
			{
				return localRotation.eulerAngles;
			}
			set
			{
				localRotation = Quaternion.Euler(value);
			}
		}

		public Vector3 localScale
		{
			get
			{
				return GetVector3("local-scale", Vector3.one);
			}
			set
			{
				Vector3 v = value;
				v.x = Mathf.Clamp(v.x, -10000f, 10000f);
				v.y = Mathf.Clamp(v.y, -10000f, 10000f);
				v.z = Mathf.Clamp(v.z, -10000f, 10000f);
				SetVector3("local-scale", v);
			}
		}

		public List<string> dependencies
		{
			get
			{
				List<string> list = new List<string>();
				List<MDEntity> list2 = children;
				for (int i = 0; i < list2.Count; i++)
				{
					MDEntity mDEntity = list2[i];
					if (!list.Contains(mDEntity.guid))
					{
						list.Add(mDEntity.guid);
					}
				}
				return list;
			}
		}

		public MDEntityAttribFlag attribs
		{
			get
			{
				return (MDEntityAttribFlag)Get("attribs", 0);
			}
			set
			{
				Set("attribs", (int)value);
			}
		}

		public void ClearChildren()
		{
			children = new List<MDEntity>();
		}

		public MDEntity GetChild(int p_index)
		{
			if (p_index >= 0)
			{
				if (p_index < childCount)
				{
					return children[p_index];
				}
				return null;
			}
			return null;
		}

		public int GetChildIndex(MDEntity p_child)
		{
			MDEntity mDEntity = ((p_child == null) ? null : GetChildByGUID(p_child.id));
			if (mDEntity != null)
			{
				return children.IndexOf(mDEntity);
			}
			return -1;
		}

		public void SetChildIndex(int p_index, MDEntity p_child)
		{
			if (GetChildIndex(p_child) >= 0)
			{
				List<MDEntity> list = children;
				int index = Mathf.Clamp(p_index, 0, list.Count - 1);
				list.Remove(p_child);
				list.Insert(index, p_child);
			}
		}

		public bool ContainsChild(MDEntity p_child)
		{
			return ((p_child == null) ? null : GetChildByGUID(p_child.id)) != null;
		}

		public void SetSiblingIndex(int p_index)
		{
			if (parent != null)
			{
				parent.SetChildIndex(p_index, this);
			}
		}

		public int GetSiblingIndex()
		{
			if (parent != null)
			{
				return parent.GetChildIndex(this);
			}
			return -1;
		}

		public MDEntity GetChildByGUID(string p_guid)
		{
			List<MDEntity> list = children;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null && list[i].id == p_guid)
				{
					return list[i];
				}
			}
			return null;
		}

		protected void AddChild(MDEntity c)
		{
			if (!ContainsChild(c))
			{
				children.Add(c);
			}
		}

		protected void RemoveChild(MDEntity c)
		{
			if (c != null)
			{
				c = GetChildByGUID(c.id);
			}
			if (c != null)
			{
				children.Remove(c);
			}
		}

		protected void SwapChildren(MDEntity a, MDEntity b)
		{
			int childIndex = GetChildIndex(a);
			int childIndex2 = GetChildIndex(b);
			if (childIndex >= 0 || childIndex2 >= 0)
			{
				List<MDEntity> list = children;
				list[childIndex] = b;
				list[childIndex2] = a;
			}
		}

		internal void RefreshParenting(bool p_assert_instance = false)
		{
			List<MDEntity> list = children;
			for (int i = 0; i < list.Count; i++)
			{
				MDEntity mDEntity = list[i];
				mDEntity.m_parent = this;
				mDEntity.parentId = base.id;
				mDEntity.RefreshParenting();
			}
		}

		public List<MDEntity> RebuildHierarchy()
		{
			List<MDEntity> result = children;
			RefreshParenting();
			return result;
		}

		public void SetTransform(Vector3 p, Vector3 r, Vector3 s)
		{
			localPosition = p;
			localEuler = r;
			localScale = s;
		}

		public void SetTransform(Vector3 p, Quaternion r, Vector3 s)
		{
			localPosition = p;
			localRotation = r;
			localScale = s;
		}

		public MDEntity()
		{
			m_children = children;
			if (m_children == null)
			{
				children = new List<MDEntity>();
			}
			base.type = MapAssetType.Entity;
		}

		public override string ToJsonProperties(bool p_indented = false)
		{
			object v = Get<object>("children", null);
			Remove("children");
			string result = ToJson(p_indented);
			Set("children", v);
			return result;
		}
	}
}
