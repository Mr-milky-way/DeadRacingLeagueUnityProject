using UnityEngine;

public class PhotonDebugInput : MonoBehaviour
{
	private void Update()
	{
		float x = Input.GetAxis("Horizontal") * Time.deltaTime * 5f;
		float z = Input.GetAxis("Vertical") * Time.deltaTime * 5f;
		base.transform.Translate(x, 0f, z);
	}
}
