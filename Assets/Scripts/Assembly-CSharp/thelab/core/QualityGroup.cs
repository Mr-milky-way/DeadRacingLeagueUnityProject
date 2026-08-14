using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class QualityGroup : MonoBehaviour
	{
		[Serializable]
		public class Element
		{
			public UnityEngine.Object target;

			public List<bool> flags;

			public Element()
			{
				flags = new List<bool>();
			}

			public void Apply(int p_quality)
			{
				bool p_enabled = p_quality < 0 || p_quality >= flags.Count || flags[p_quality];
				Apply(p_enabled);
			}

			public void Apply(bool p_enabled)
			{
				if (!target)
				{
					return;
				}
				bool flag = p_enabled;
				if (target is Behaviour)
				{
					((Behaviour)target).enabled = flag;
					return;
				}
				GameObject gameObject = null;
				if (target is Component)
				{
					gameObject = ((Component)target).gameObject;
				}
				else if (target is GameObject)
				{
					gameObject = (GameObject)target;
				}
				if (!gameObject)
				{
					Debug.LogWarning("QualityGroup> Failed to apply [" + target.name + "]");
				}
				else
				{
					gameObject.SetActive(flag);
				}
			}
		}

		public static List<QualityGroup> list;

		private static bool m_static_dirty;

		public string id;

		[HideInInspector]
		public List<Element> targets;

		protected int m_current;

		public int currentQualityLevel => m_current;

		public static void SetQuality(string p_id, int p_quality)
		{
			if (list == null)
			{
				list = new List<QualityGroup>();
			}
			for (int i = 0; i < list.Count; i++)
			{
				QualityGroup qualityGroup = list[i];
				if (string.IsNullOrEmpty(p_id) || !(qualityGroup.id != p_id))
				{
					qualityGroup.Apply(p_quality);
				}
			}
		}

		public static void SetQuality(int p_quality)
		{
			SetQuality("", p_quality);
		}

		public static void SetQuality(string p_id, bool p_flag)
		{
			if (list == null)
			{
				list = new List<QualityGroup>();
			}
			for (int i = 0; i < list.Count; i++)
			{
				QualityGroup qualityGroup = list[i];
				if (string.IsNullOrEmpty(p_id) || !(qualityGroup.id != p_id))
				{
					qualityGroup.Apply(p_flag);
				}
			}
		}

		public static void Refresh()
		{
			m_static_dirty = true;
			if (list == null)
			{
				list = new List<QualityGroup>();
			}
			foreach (QualityGroup item in list)
			{
				item.Update();
			}
			m_static_dirty = false;
		}

		protected virtual void Awake()
		{
			if (list == null)
			{
				list = new List<QualityGroup>();
			}
			if (list.IndexOf(this) < 0)
			{
				list.Add(this);
			}
			m_current = -1;
			Invoke("Apply", 0.05f);
		}

		protected virtual void OnDestroy()
		{
			if (list == null)
			{
				list = new List<QualityGroup>();
			}
			if (list.IndexOf(this) >= 0)
			{
				list.Remove(this);
			}
		}

		public virtual void Apply(int p_quality)
		{
			if (base.enabled)
			{
				m_current = p_quality;
				for (int i = 0; i < targets.Count; i++)
				{
					targets[i].Apply(p_quality);
				}
			}
		}

		public virtual void Apply(bool p_flag)
		{
			if (base.enabled)
			{
				for (int i = 0; i < targets.Count; i++)
				{
					targets[i].Apply(p_flag);
				}
			}
		}

		public void Apply()
		{
			Apply(m_current);
		}

		protected virtual void Update()
		{
			if (m_static_dirty)
			{
				Apply();
			}
		}
	}
}
