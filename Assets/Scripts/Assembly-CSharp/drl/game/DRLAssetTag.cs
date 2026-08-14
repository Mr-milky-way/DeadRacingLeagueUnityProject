using System.Collections.Generic;
using thelab.core;

namespace drl.game
{
	public class DRLAssetTag<T> : Tag<T>
	{
		public List<T> FindAll(T p_start, T p_end)
		{
			List<T> list = new List<T>();
			int num = (int)(object)p_start;
			int num2 = (int)(object)p_end;
			if (tags == null)
			{
				tags = new List<T>();
			}
			for (int i = 0; i < tags.Count; i++)
			{
				int num3 = (int)(object)tags[i];
				if (num3 > num && num3 < num2)
				{
					list.Add(tags[i]);
				}
			}
			return list;
		}

		public T Find(T p_start, T p_end)
		{
			int num = (int)(object)p_start;
			int num2 = (int)(object)p_end;
			if (tags == null)
			{
				tags = new List<T>();
			}
			for (int i = 0; i < tags.Count; i++)
			{
				int num3 = (int)(object)tags[i];
				if (num3 > num && num3 < num2)
				{
					return tags[i];
				}
			}
			return default(T);
		}
	}
}
