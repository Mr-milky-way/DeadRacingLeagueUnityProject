using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl.levels
{
	public class FloatSkinLibrary : MonoBehaviour
	{
		[Serializable]
		public class Skin
		{
			public Texture2D skin;

			public Material fringe;
		}

		public List<Skin> skins;

		public Material GetFringe(Texture2D p_skin)
		{
			for (int i = 0; i < skins.Count; i++)
			{
				Skin skin = skins[i];
				if (skin != null && !(skin.skin != p_skin))
				{
					return skin.fringe;
				}
			}
			return null;
		}
	}
}
