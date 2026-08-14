using UnityEngine;

namespace drl
{
	public class MADistanceRatioAction : MapAssetAction
	{
		[Space(5f)]
		[Header("Distance / Ratio")]
		public AnimationCurve ratioDistanceCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		public float ratio;

		public bool oriented;

		public new MapAssetActionMode mode
		{
			get
			{
				return MapAssetActionMode.Auto;
			}
			set
			{
			}
		}

		protected void SetRatio(float p_ratio)
		{
			float num = ratioDistanceCurve.Evaluate(p_ratio);
			ratio = num;
			OnRatioChange(ratio);
		}

		protected override void OnActionTriggerStart()
		{
			SetRatio(oriented ? 1f : 1f);
		}

		protected override void OnActionTriggerComplete()
		{
			SetRatio(oriented ? (-1f) : 1f);
		}

		protected virtual void OnRatioChange(float p_ratio)
		{
		}

		protected override bool OnActionUpdate()
		{
			Collider closestObject = GetClosestObject();
			if ((bool)closestObject)
			{
				Vector3 position = trigger.transform.position;
				Vector3 position2 = closestObject.transform.position;
				float num = Vector3.Distance(position2, position);
				SphereCollider sphereCollider = trigger as SphereCollider;
				float num2 = (sphereCollider ? sphereCollider.radius : 10f);
				float num3 = Mathf.Clamp01(num / num2);
				if (oriented && Vector3.Dot(position2 - position, trigger.transform.forward) < 0f)
				{
					num3 = 0f - num3;
				}
				float num4 = ratioDistanceCurve.Evaluate(num3);
				if (Mathf.Abs(num4 - ratio) > 0f)
				{
					ratio = num4;
					OnRatioChange(ratio);
				}
			}
			return true;
		}

		protected override void OnActionRestore()
		{
			SetRatio(oriented ? 1f : 1f);
		}
	}
}
