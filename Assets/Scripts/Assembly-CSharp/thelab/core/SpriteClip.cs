using UnityEngine;

namespace thelab.core
{
	public class SpriteClip : MovieClip<Sprite, SpriteRenderer>
	{
		protected override void Awake()
		{
			if (!base.target)
			{
				base.target = GetComponent<SpriteRenderer>();
			}
			base.Awake();
		}

		protected override void OnFrame(Sprite p_frame)
		{
			if ((bool)base.target)
			{
				base.target.sprite = p_frame;
			}
		}
	}
}
