using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	public class ImageClip : MovieClip<Sprite, MaskableGraphic>
	{
		protected override void Awake()
		{
			base.Awake();
		}

		protected override void OnFrame(Sprite p_frame)
		{
			if (base.target is Image)
			{
				((Image)base.target).sprite = p_frame;
			}
			if (base.target is RawImage)
			{
				((RawImage)base.target).texture = p_frame.texture;
			}
		}
	}
}
