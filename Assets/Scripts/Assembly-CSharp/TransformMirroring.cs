using UnityEngine;

public class TransformMirroring : MonoBehaviour
{
	public Transform target;

	public Vector3 offsetPosition;

	public Vector3 offsetRotation;

	public Vector3 offsetScale;

	protected void LateUpdate()
	{
		if ((bool)target)
		{
			base.transform.position = target.position + offsetPosition;
			base.transform.localEulerAngles = target.localEulerAngles + offsetRotation;
			base.transform.localScale = target.localScale + offsetScale;
		}
	}
}
