using UnityEngine;
using UnityEngine.EventSystems;

namespace drl.game
{
	public class DropDownItemIndex : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler
	{
		public DRLDropdownView dropdownView;

		public int index;

		public void OnPointerEnter(PointerEventData eventData)
		{
		}
	}
}
