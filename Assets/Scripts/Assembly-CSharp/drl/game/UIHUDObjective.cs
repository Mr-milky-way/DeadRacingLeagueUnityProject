using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIHUDObjective : MonoBehaviour
	{
		public Text objectiveLabel;

		public string[] activeObjectiveLabels;

		private int activeObjective;

		public FadeComponent fade => GetComponent<FadeComponent>();

		public bool objectivesRequired { get; set; }

		public int totalObjectives { get; set; }

		public void SetLabels(string[] p_labels)
		{
			if (p_labels.Length != 0)
			{
				activeObjectiveLabels = new string[p_labels.Length];
				activeObjectiveLabels = p_labels;
				objectivesRequired = true;
				totalObjectives = p_labels.Length;
				objectiveLabel.text = activeObjectiveLabels[0];
			}
			else
			{
				objectivesRequired = false;
			}
		}

		public int[] NextObjective()
		{
			objectiveLabel.gameObject.SetActive(value: true);
			activeObjective++;
			if (activeObjective < activeObjectiveLabels.Length)
			{
				objectiveLabel.text = activeObjectiveLabels[activeObjective];
				return new int[2] { activeObjective, totalObjectives };
			}
			objectiveLabel.text = "";
			objectiveLabel.gameObject.SetActive(value: false);
			return new int[2] { totalObjectives, totalObjectives };
		}

		public void ShowObjectives()
		{
			objectiveLabel.gameObject.SetActive(value: true);
			fade.FadeIn(0f);
		}

		public void ClearObjectives()
		{
			objectiveLabel.text = "";
			activeObjectiveLabels = null;
			objectiveLabel.gameObject.SetActive(value: false);
			fade.FadeOut(0f);
		}
	}
}
