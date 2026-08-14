using UnityEngine;

[AddComponentMenu("Wwise/AkEnvironmentPortal")]
[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
public class AkEnvironmentPortal : MonoBehaviour
{
	public const int MAX_ENVIRONMENTS_PER_PORTAL = 2;

	public Vector3 axis = Vector3.right;

	public AkEnvironment[] environments = new AkEnvironment[2];

	private BoxCollider m_BoxCollider;

	private BoxCollider BoxCollider
	{
		get
		{
			if (!m_BoxCollider)
			{
				m_BoxCollider = GetComponent<BoxCollider>();
			}
			return m_BoxCollider;
		}
	}

	public bool EnvironmentsShareAuxBus
	{
		get
		{
			if (environments[0] == null)
			{
				return environments[1] == null;
			}
			if (environments[1] == null)
			{
				return false;
			}
			if (environments[0].data == null)
			{
				return environments[1].data == null;
			}
			if (environments[1].data == null)
			{
				return false;
			}
			return environments[0].data.Id == environments[1].data.Id;
		}
	}

	public float GetAuxSendValueForPosition(Vector3 in_position, int index)
	{
		float num = Vector3.Dot(Vector3.Scale(BoxCollider.size, base.transform.lossyScale), axis);
		Vector3 vector = Vector3.Normalize(base.transform.rotation * axis);
		float value = Vector3.Dot(in_position - (base.transform.position - num * 0.5f * vector), vector);
		value = Mathf.Clamp(value, 0f, num);
		if (index == 0)
		{
			return (num - value) * (num - value) / (num * num);
		}
		return value * value / (num * num);
	}
}
