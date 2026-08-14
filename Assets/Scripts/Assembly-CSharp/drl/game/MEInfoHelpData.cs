using System;
using UnityEngine;

namespace drl.game
{
	[Serializable]
	public class MEInfoHelpData
	{
		public string label;

		public string defaultLabel;

		public bool localized = true;

		public MEInfoHelpType type;

		public Sprite icon;

		public string key;

		public bool reversed;

		public bool separator;

		public int order;

		public string iconId
		{
			get
			{
				if (!icon)
				{
					return "";
				}
				return icon.name;
			}
		}
	}
}
