using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.game;
using thelab.core;

public class MissingLocalizationTool : MonoBehaviour
{
	[Serializable]
	public class MissingLocaleData
	{
		public string screenName;

		public List<Text> missingFields;

		public int lblsAmount;

		public List<DRLStepperView> missingSteppers;

		public int steppersAmount;

		public MissingLocaleData()
		{
			missingFields = new List<Text>();
			missingSteppers = new List<DRLStepperView>();
		}
	}

	[SerializeField]
	private List<MissingLocaleData> missingResults;

	[ContextMenu("FindMissingLocales")]
	public void FindMissingLocales()
	{
		missingResults = new List<MissingLocaleData>();
		UIScreen[] array = UnityEngine.Object.FindObjectsOfType<UIScreen>();
		foreach (UIScreen uIScreen in array)
		{
			MissingLocaleData missingLocaleData = new MissingLocaleData();
			missingLocaleData.screenName = uIScreen.gameObject.name;
			Text[] componentsInChildren = uIScreen.GetComponentsInChildren<Text>();
			foreach (Text text in componentsInChildren)
			{
				if (IsLocalizable(text.text) && !(text.GetComponent<LocaleText>() != null))
				{
					missingLocaleData.missingFields.Add(text);
				}
			}
			DRLStepperView[] componentsInChildren2 = uIScreen.GetComponentsInChildren<DRLStepperView>();
			foreach (DRLStepperView dRLStepperView in componentsInChildren2)
			{
				if (!(dRLStepperView.GetComponent<LocaleProperty>() != null))
				{
					missingLocaleData.missingSteppers.Add(dRLStepperView);
				}
			}
			missingLocaleData.lblsAmount = missingLocaleData.missingFields.Count;
			missingLocaleData.steppersAmount = missingLocaleData.missingSteppers.Count;
			if (missingLocaleData.lblsAmount > 0 || missingLocaleData.steppersAmount > 0)
			{
				missingLocaleData.screenName = missingLocaleData.screenName + " (" + (missingLocaleData.lblsAmount + missingLocaleData.steppersAmount) + ")";
				missingResults.Add(missingLocaleData);
			}
		}
	}

	private bool IsLocalizable(string s)
	{
		if (string.IsNullOrEmpty(s.Trim()))
		{
			return false;
		}
		for (int i = 0; i < s.Length; i++)
		{
			if (char.IsDigit(s[i]))
			{
				return false;
			}
		}
		return true;
	}
}
