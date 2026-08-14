using UnityEngine;

namespace drl
{
	public class MADistanceAnimationAction : MADistanceRatioAction
	{
		[Space(5f)]
		[Header("Animation")]
		public Animator animation;

		public AnimationClip clip;

		protected void Awake()
		{
			if ((bool)animation)
			{
				animation.speed = 0f;
			}
		}

		protected override void OnRatioChange(float p_ratio)
		{
			animation.Play(clip.name ?? "", 0, p_ratio);
			animation.speed = 0f;
		}
	}
}
