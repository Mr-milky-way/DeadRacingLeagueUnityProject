using UnityEngine;
using UnityEngine.UI;
using drl.game;
using drl.sim.rci;

[RequireComponent(typeof(Toggle))]
public class ChannelDropdownItemComponent : MonoBehaviour
{
	public UIChannelItemView item;

	private void Start()
	{
		if (item.isButton && RCI.IsRCController())
		{
			int siblingIndex = base.transform.GetSiblingIndex();
			if (siblingIndex > 1 && siblingIndex < 6)
			{
				GetComponent<Toggle>().interactable = false;
			}
		}
		if (item.isButton && !RCI.IsRCController())
		{
			GetComponent<Toggle>().interactable = false;
		}
		if (!item.isButton && !RCI.IsRCController() && base.transform.GetSiblingIndex() > 5)
		{
			GetComponent<Toggle>().interactable = false;
		}
	}
}
