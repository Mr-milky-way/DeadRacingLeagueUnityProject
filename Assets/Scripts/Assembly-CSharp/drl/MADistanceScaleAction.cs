using UnityEngine;

namespace drl
{
	public class MADistanceScaleAction : MapAssetAction
	{
		public AnimationCurve scaleDistanceCurve;

		public Transform asset;

		public Vector3 minScale = Vector3.zero;

		public Vector3 maxScale = Vector3.one;

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

		protected override void OnActionTriggerStart()
		{
			if ((bool)asset)
			{
				asset.transform.localScale = minScale;
			}
		}

		protected override void OnActionTriggerComplete()
		{
			if ((bool)asset)
			{
				asset.transform.localScale = maxScale;
			}
		}

		protected override bool OnActionUpdate()
		{
			if (!asset)
			{
				return false;
			}
			Collider closestObject = GetClosestObject();
			if ((bool)closestObject)
			{
				float num = Vector3.Distance(closestObject.transform.position, trigger.transform.position);
				SphereCollider sphereCollider = trigger as SphereCollider;
				float num2 = (sphereCollider ? sphereCollider.radius : 10f);
				float time = Mathf.Clamp01(num / num2);
				float t = scaleDistanceCurve.Evaluate(time);
				asset.transform.localScale = Vector3.Lerp(minScale, maxScale, t);
			}
			return true;
		}
	}
}
