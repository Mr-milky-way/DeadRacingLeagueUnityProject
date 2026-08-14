using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace drl.game
{
	public class UIMarkers : MonoBehaviour
	{
		[HideInInspector]
		public List<GameObject> stepsList;

		public GameObject stepPrefab;

		public int greyStepsCount;

		public GameObject list;

		public void SetGreyCheckerPattern(List<GameObject> image)
		{
			for (int i = 1; i < image.Count && i <= image.Count; i += 2)
			{
				Image component = image[i].GetComponent<Image>();
				Color color = component.color;
				color.a = 0.8f;
				component.color = color;
			}
		}

		public void Init(int totalSteps, int progress)
		{
			ClearLists();
			if (list == null)
			{
				list = base.gameObject;
			}
			for (int i = 0; i < progress; i++)
			{
				stepsList.Add(Object.Instantiate(stepPrefab, list.transform));
			}
			int num = totalSteps - progress;
			for (int j = 0; j < num; j++)
			{
				GameObject gameObject = Object.Instantiate(stepPrefab, list.transform);
				gameObject.GetComponent<Image>().color = Color.gray;
				stepsList.Add(gameObject);
			}
			SetGreyCheckerPattern(stepsList);
		}

		public void ClearLists()
		{
			foreach (GameObject steps in stepsList)
			{
				Object.Destroy(steps);
			}
			stepsList.Clear();
		}

		public void SetRedCurrentStep(int i)
		{
			if (!(stepsList[i] == null))
			{
				stepsList[i].GetComponent<Image>().color = Color.red;
			}
		}
	}
}
