using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class MEOverlayLayer : View<DRLApp>
	{
		public RectTransform selectionBox;

		public Vector2 size => (base.transform as RectTransform).rect.size;

		public void ClearSelectionBox()
		{
			selectionBox.gameObject.SetActive(value: false);
		}

		public void SelectionBox(Rect p_area)
		{
			RectTransform rectTransform = selectionBox;
			rectTransform.sizeDelta = p_area.size;
			rectTransform.anchoredPosition = new Vector2(p_area.xMin, p_area.yMin);
			selectionBox.gameObject.SetActive(value: true);
		}
	}
}
