using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class ObjectTag : Tag<Object>
	{
		public List<T> GetList<T>()
		{
			List<T> list = new List<T>();
			for (int i = 0; i < tags.Count; i++)
			{
				Object obj = tags[i];
				if (obj is T)
				{
					list.Add((T)(object)obj);
				}
				else if (obj is Component)
				{
					Component component = obj as Component;
					list.Add((T)(object)component.GetComponent(typeof(T)));
				}
			}
			return list;
		}
	}
}
