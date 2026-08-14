using UnityEngine;

public class TrainingElementChild : MonoBehaviour
{
	[SerializeField]
	private TrainingElement m_trainingElement;

	public TrainingElement TrainingElement => m_trainingElement;
}
