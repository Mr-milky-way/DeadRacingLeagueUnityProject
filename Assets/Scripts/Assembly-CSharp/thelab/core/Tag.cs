using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class Tag<T> : Tag
	{
		public List<T> tags;

		public Type type => typeof(T);

		public T this[int p]
		{
			get
			{
				return tags[p];
			}
			set
			{
				tags[p] = value;
			}
		}

		public int Count
		{
			get
			{
				if (tags != null)
				{
					return tags.Count;
				}
				return 0;
			}
		}

		public bool Contains(T p_tag)
		{
			if (tags != null)
			{
				return tags.IndexOf(p_tag) >= 0;
			}
			return false;
		}

		public bool Match(params T[] p_tags)
		{
			return Match(p_precise: false, p_tags);
		}

		public bool Match(bool p_precise, params T[] p_tags)
		{
			if (p_precise && p_tags.Length != tags.Count)
			{
				return false;
			}
			List<T> list = ((tags == null) ? new List<T>() : tags);
			for (int i = 0; i < p_tags.Length; i++)
			{
				bool flag = false;
				for (int j = 0; j < list.Count; j++)
				{
					if (EqualityComparer<T>.Default.Equals(list[j], p_tags[i]))
					{
						flag = true;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			return true;
		}
	}
	public class Tag : MonoBehaviour
	{
		[HideInInspector]
		public string label;

		public static List<T> FindAll<T>(string p_label) where T : Tag
		{
			List<T> list = new List<T>();
			T[] array = UnityEngine.Object.FindObjectsOfType<T>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].label == p_label)
				{
					list.Add(array[i]);
				}
			}
			return list;
		}

		public static List<Tag> FindAll(string p_label)
		{
			return FindAll<Tag>(p_label);
		}
	}
}
