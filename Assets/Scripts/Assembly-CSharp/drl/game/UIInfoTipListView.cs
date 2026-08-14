using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIInfoTipListView : View<DRLApp>
	{
		public List<MEInfoHelpData> content;

		public List<InfoTipListPreset> presets;

		public bool runOnAwake = true;

		public ListComponent list => AssertLocal<ListComponent>("list");

		protected void Awake()
		{
			if (runOnAwake)
			{
				Populate();
			}
		}

		public void Populate(List<MEInfoHelpData> p_list, int[] p_items)
		{
			if (p_list == null || p_items == null)
			{
				return;
			}
			p_list.Sort((MEInfoHelpData a, MEInfoHelpData b) => (a.order >= b.order) ? 1 : (-1));
			list.Clear();
			int num = p_items.Length;
			for (int num2 = 0; num2 < num; num2++)
			{
				int num3 = p_items[num2];
				if (num3 >= 0 && num3 < p_list.Count)
				{
					MEInfoHelpTagView mEInfoHelpTagView = list.Push<MEInfoHelpTagView>();
					mEInfoHelpTagView.Set(p_list[num3]);
					mEInfoHelpTagView.backgroundImage.enabled = false;
					mEInfoHelpTagView.name = p_list[num2].label;
					mEInfoHelpTagView.GetComponent<HorizontalLayoutGroup>().spacing = 2f;
					mEInfoHelpTagView.separatorVisible = true;
					mEInfoHelpTagView.spaces[0].gameObject.SetActive(value: false);
					mEInfoHelpTagView.spaces[1].gameObject.SetActive(value: false);
					mEInfoHelpTagView.spaces[2].gameObject.SetActive(value: false);
				}
			}
		}

		public void Populate(List<MEInfoHelpData> p_list)
		{
			int[] array = new int[p_list.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = i;
			}
			Populate(p_list, array);
		}

		[ContextMenu("Populate")]
		public void Populate()
		{
			Populate(content);
		}

		public void Populate(int[] p_items)
		{
			Populate(content, p_items);
		}

		public void Populate(string p_preset)
		{
			List<MEInfoHelpData> infoList = GetInfoList(p_preset);
			Populate(infoList);
		}

		public List<MEInfoHelpData> GetInfoList(InfoTipListPreset p_preset)
		{
			List<MEInfoHelpData> list = new List<MEInfoHelpData>();
			if (p_preset == null)
			{
				return list;
			}
			for (int i = 0; i < p_preset.items.Count; i++)
			{
				string p_label = p_preset.items[i];
				MEInfoHelpData info = GetInfo(p_label);
				if (info != null)
				{
					list.Add(info);
				}
			}
			return list;
		}

		public List<MEInfoHelpData> GetInfoList(string p_preset)
		{
			return GetInfoList(GetPreset(p_preset));
		}

		public MEInfoHelpData GetInfo(string p_label)
		{
			return content.Find((MEInfoHelpData it) => it.label == p_label);
		}

		public InfoTipListPreset GetPreset(string p_id)
		{
			return presets.Find((InfoTipListPreset it) => it.id == p_id);
		}
	}
}
