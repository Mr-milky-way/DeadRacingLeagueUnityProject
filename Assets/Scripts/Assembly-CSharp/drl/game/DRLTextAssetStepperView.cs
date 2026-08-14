using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class DRLTextAssetStepperView : DRLListStepperView<string>
	{
		public new TextAsset data;

		public bool upperCaseLabels = true;

		public bool allowSearch = true;

		public bool searchLetterOnly = true;

		public string searchString = "";

		public float searchTimeout = 0.8f;

		private Activity m_search_timeout_timer;

		private int m_search_index;

		public List<int> m_search_indexes;

		public override void OnFocus()
		{
			base.OnFocus();
		}

		public override void OnUnfocus()
		{
			base.OnUnfocus();
		}

		protected override void Awake()
		{
			string text = (data ? data.text : "");
			List<string> list = new List<string>(data ? text.Split('\n') : ((values == null) ? new string[0] : values.ToArray()));
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			for (int i = 0; i < list.Count; i++)
			{
				string item = (upperCaseLabels ? list[i].ToUpper() : list[i]);
				list2.Add(item);
				list3.Add(list[i]);
			}
			labels = list2.ToArray();
			values.Clear();
			values.AddRange(list3);
			min = 0;
			max = list.Count - 1;
			base.Awake();
		}

		protected override void Update()
		{
			base.Update();
			if (!m_focused)
			{
				return;
			}
			if (m_search_indexes == null)
			{
				m_search_indexes = new List<int>();
			}
			if (string.IsNullOrEmpty(Input.inputString))
			{
				return;
			}
			string text = (Input.inputString[0].ToString() ?? "").ToLower();
			string text2 = searchString;
			text2 = ((!(text == "\b")) ? (text2 + text) : ((text2.Length > 0) ? text2.Substring(0, text2.Length - 1) : ""));
			if (searchLetterOnly)
			{
				text2 = ((text2.Length <= 0) ? "" : (text2[0].ToString() ?? ""));
			}
			if (searchString != text2)
			{
				m_search_index = 0;
				m_search_indexes.Clear();
				List<string> list = values;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].ToLower().IndexOf(text2) == 0)
					{
						m_search_indexes.Add(i);
					}
				}
			}
			searchString = text2;
			int num = ((m_search_index < 0) ? (-1) : ((m_search_index >= m_search_indexes.Count) ? (-1) : m_search_indexes[m_search_index]));
			if (num >= 0)
			{
				index = num;
				RefreshPreview("lclick");
				OnState("change");
				Notify(notification + "@change");
			}
			if (searchLetterOnly && m_search_indexes.Count > 0)
			{
				m_search_index = (m_search_index + 1) % m_search_indexes.Count;
			}
			if (m_search_timeout_timer != null)
			{
				m_search_timeout_timer.Stop();
			}
			m_search_timeout_timer = Activity.RunOnce(delegate
			{
				searchString = "";
			}, searchTimeout);
		}
	}
}
