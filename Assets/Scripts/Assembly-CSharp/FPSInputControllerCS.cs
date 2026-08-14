using UnityEngine;

[RequireComponent(typeof(CharacterMotorCS))]
public class FPSInputControllerCS : MonoBehaviour
{
	private CharacterMotorCS motor;

	private void Awake()
	{
		motor = GetComponent<CharacterMotorCS>();
	}

	private void Update()
	{
		Vector3 vector = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		if (vector != Vector3.zero)
		{
			float magnitude = vector.magnitude;
			vector /= magnitude;
			magnitude = Mathf.Min(1f, magnitude);
			magnitude *= magnitude;
			vector *= magnitude;
		}
		motor.inputMoveDirection = base.transform.rotation * vector;
		motor.inputJump = Input.GetButton("Jump");
	}
}
