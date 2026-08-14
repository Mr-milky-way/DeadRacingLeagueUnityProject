using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class DRLGameAsset : UniqueAsset
	{
		private Tag<GameFlag> m_tags;

		public List<GameObject> content;

		public int order;

		public Texture preview;

		public Texture image;

		public Tag<GameFlag> tags
		{
			get
			{
				if ((bool)m_tags)
				{
					return m_tags;
				}
				m_tags = GetComponent<Tag<GameFlag>>();
				if ((bool)m_tags)
				{
					return m_tags;
				}
				return m_tags = base.gameObject.AddComponent<GameFlagTag>();
			}
		}

		public virtual string GetPrefix()
		{
			return "GA";
		}

		protected override string GetGUID()
		{
			return GetPrefix() + "-" + GUID.Create(1, "", 500, 0, 4095, "x3");
		}
	}
}
