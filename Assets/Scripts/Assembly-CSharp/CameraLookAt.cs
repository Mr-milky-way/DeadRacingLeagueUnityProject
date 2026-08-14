using UnityEngine;

public class CameraLookAt : MonoBehaviour
{
	public Transform target;

	private void LateUpdate()
	{
		if ((bool)target)
		{
			base.transform.LookAt(target);
		}
	}
}
