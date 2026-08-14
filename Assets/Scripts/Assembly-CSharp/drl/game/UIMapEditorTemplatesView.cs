using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class UIMapEditorTemplatesView : UIScreenView
	{
		public ListComponent listField;

		public GameFlag gameMode = GameFlag.Race;

		public void Clear()
		{
			listField.Clear();
		}

		public void Add(DRLMap p_map)
		{
			if (!p_map)
			{
				Debug.LogWarning("UIMapsView> Add - Invalid Map");
				return;
			}
			UICardButtonMap uICardButtonMap = listField.Push<UICardButtonMap>();
			uICardButtonMap.notification = "map-editor.templates-card";
			uICardButtonMap.Set(p_map);
		}

		public void Set(List<DRLMap> p_list)
		{
			Clear();
			for (int i = 0; i < p_list.Count; i++)
			{
				Add(p_list[i]);
			}
		}
	}
}
