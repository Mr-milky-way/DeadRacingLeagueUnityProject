using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using thelab.core;

namespace drl.backend
{
	public class DRLCircuitData : SerializedData
	{
		public enum Tag
		{
			none = 0,
			tryouts = 1,
			hidden = 2
		}

		private readonly List<Tag> m_tags = new List<Tag>();

		private DRLCircuitMapData[] m_maps;

		public string guid => Get("id", "");

		public string name => Get("name", "");

		public string description => Get("description", "");

		public string imageURL => Get("image-url", "");

		public int difficulty => Get("difficulty", 0);

		public int trackCount
		{
			get
			{
				DRLCircuitMapData[] array = maps;
				if (array == null)
				{
					return 0;
				}
				return array.Length;
			}
		}

		private string[] m_tagsArray
		{
			get
			{
				object obj = Get("tags", (object)new string[0]);
				if (!(obj is JArray))
				{
					return null;
				}
				obj = (obj as JArray).ToObject<string[]>();
				return (string[])obj;
			}
		}

		public List<Tag> tags
		{
			get
			{
				m_tags.Clear();
				if (m_tagsArray == null || m_tagsArray.Length == 0)
				{
					return m_tags;
				}
				for (int i = 0; i < m_tagsArray.Length; i++)
				{
					string value = m_tagsArray[i];
					if (!string.IsNullOrEmpty(value))
					{
						Enum.TryParse<Tag>(value, out var result);
						if (result != Tag.none)
						{
							m_tags.Add(result);
						}
					}
				}
				return m_tags;
			}
		}

		public DRLCircuitMapData[] maps
		{
			get
			{
				object obj = Get("maps-data", (object)new DRLCircuitMapData[0]);
				if (obj is JArray)
				{
					obj = (obj as JArray).ToObject<DRLCircuitMapData[]>();
				}
				m_maps = (DRLCircuitMapData[])obj;
				return m_maps;
			}
		}

		public bool ContainsTag(Tag p_tag)
		{
			return tags.Contains(p_tag);
		}
	}
}
