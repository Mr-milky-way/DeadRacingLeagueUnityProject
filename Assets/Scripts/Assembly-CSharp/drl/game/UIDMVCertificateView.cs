using System;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIDMVCertificateView : UIScreenView
	{
		public ImageLayout badgeImage;

		public RawImage badgeGlow;

		public void AnimateBadge(float p_period = 5f)
		{
			Tween tween = Tween.Add(badgeImage, "scale", Vector2.one, p_period, Cubic.In);
			tween.onComplete = (Action<Tween>)Delegate.Combine(tween.onComplete, (Action<Tween>)delegate
			{
				Tween.Add(badgeGlow, "color", Color.white, 3f, Cubic.Out);
			});
		}
	}
}
