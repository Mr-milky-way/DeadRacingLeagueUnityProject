using UnityEngine;

namespace TTTSamples
{
	public class CircleMoverSample : MonoBehaviour
	{
		private float moveSpeed = 150f;

		private float torque;

		private Rigidbody2D rb;

		private void Start()
		{
			rb = GetComponent<Rigidbody2D>();
		}

		private void Update()
		{
			torque = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
		}

		private void FixedUpdate()
		{
			rb.AddTorque(0f - torque);
		}
	}
}
