using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class UINPCOverlay : MonoBehaviour
	{
		public ControllerStateType controller = ControllerStateType.Taranis;

		public Transform containerLeft;

		public Transform containerRight;

		public Transform GetContainer(bool p_left)
		{
			if (!p_left)
			{
				return containerRight;
			}
			return containerLeft;
		}

		public void SetState(NPCStateType p_type, bool p_is_left)
		{
			if ((bool)containerLeft)
			{
				containerLeft.gameObject.SetActive(value: false);
			}
			if ((bool)containerRight)
			{
				containerRight.gameObject.SetActive(value: false);
			}
			if (p_type == NPCStateType.Hide)
			{
				return;
			}
			Transform container = GetContainer(p_is_left);
			if ((bool)container)
			{
				container.gameObject.SetActive(value: true);
			}
			List<NPCStateTypeTag> list = (container ? Hierarchy.FindAll<NPCStateTypeTag>(container) : new List<NPCStateTypeTag>());
			for (int i = 0; i < list.Count; i++)
			{
				NPCStateTypeTag nPCStateTypeTag = list[i];
				nPCStateTypeTag.gameObject.SetActive(value: false);
				if (nPCStateTypeTag.tags.Count <= 0)
				{
					list.RemoveAt(i--);
					continue;
				}
				if (nPCStateTypeTag.tags[0] != p_type)
				{
					list.RemoveAt(i--);
					continue;
				}
				string text = nPCStateTypeTag.transform.parent.name;
				switch ((text == "left" || text == "right") ? "default" : text)
				{
				case "xb":
					if (controller != ControllerStateType.XBox)
					{
						list.RemoveAt(i--);
					}
					break;
				case "ps":
					if (controller != ControllerStateType.PS4)
					{
						list.RemoveAt(i--);
					}
					break;
				case "rc":
					if (controller != ControllerStateType.Taranis)
					{
						list.RemoveAt(i--);
					}
					break;
				case "nk":
					if (controller != ControllerStateType.Nikko)
					{
						list.RemoveAt(i--);
					}
					break;
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				list[j].gameObject.SetActive(value: true);
			}
		}

		public void SetState(NPCStateType p_type)
		{
			SetState(p_type, p_is_left: false);
		}
	}
}
