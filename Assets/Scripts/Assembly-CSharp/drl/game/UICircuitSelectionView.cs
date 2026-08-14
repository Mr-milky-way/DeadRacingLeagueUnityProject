using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UICircuitSelectionView : UIScreenView
	{
		public ListComponent listField;

		public UINavigation backButton;

		public GameObject exitButton;

		public void Set(DRLCircuitData[] p_circuits)
		{
			if (p_circuits == null || p_circuits.Length == 0)
			{
				return;
			}
			exitButton.SetActive(base.app.inGame);
			Clear();
			for (int i = 0; i < p_circuits.Length; i++)
			{
				if (!p_circuits[i].ContainsTag(DRLCircuitData.Tag.hidden))
				{
					listField.Push<UICircuitItemView>().Set(p_circuits[i]);
				}
			}
			UINavigation.Link(listField.GetComponent<HorizontalLayoutGroup>(), backButton);
		}

		public void Clear()
		{
			listField.Clear();
		}
	}
}
