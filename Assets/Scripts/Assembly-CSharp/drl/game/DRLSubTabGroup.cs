using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.mvc;

namespace drl.game
{
	public class DRLSubTabGroup : UIElementView
	{
		public GameObject[] subTabs;

		public Text[] subTabsTexts;

		public Image[] subTabsImages;

		public Button nextTab;

		public Button previousTab;

		public Text nextTabText;

		public Text previousTabText;

		public Image nextTabImage;

		public Image previousTabImage;

		private int m_currentTab;

		private Dictionary<string, int> subTabNameToIndex = new Dictionary<string, int>();

		public string currentTab => subTabs[m_currentTab].name;

		protected void Awake()
		{
			for (int i = 0; i < subTabs.Length; i++)
			{
				subTabNameToIndex.Add(subTabs[i].name, i);
			}
			m_currentTab = 0;
			nextTabText.text = subTabsTexts[1].text;
			nextTabImage.sprite = subTabsImages[1].sprite;
			previousTabText.text = subTabsTexts[2].text;
			previousTabImage.sprite = subTabsImages[2].sprite;
		}

		public void SetTab(string p_currentTab)
		{
			if (subTabNameToIndex.ContainsKey(p_currentTab))
			{
				m_currentTab = subTabNameToIndex[p_currentTab];
				int num = m_currentTab - 1;
				if (num < 0)
				{
					num = subTabs.Length - 1;
				}
				int num2 = (m_currentTab + 1) % subTabs.Length;
				previousTabText.text = subTabsTexts[num].text;
				previousTabImage.sprite = subTabsImages[num].sprite;
				nextTabText.text = subTabsTexts[num2].text;
				nextTabImage.sprite = subTabsImages[num2].sprite;
			}
		}

		public void NextTabClick()
		{
			previousTabText.text = subTabsTexts[m_currentTab].text;
			previousTabImage.sprite = subTabsImages[m_currentTab].sprite;
			m_currentTab = (m_currentTab + 1) % subTabs.Length;
			int num = (m_currentTab + 1) % subTabs.Length;
			nextTabText.text = subTabsTexts[num].text;
			nextTabImage.sprite = subTabsImages[num].sprite;
		}

		public void PreviousTabClick()
		{
			nextTabText.text = subTabsTexts[m_currentTab].text;
			nextTabImage.sprite = subTabsImages[m_currentTab].sprite;
			m_currentTab--;
			if (m_currentTab < 0)
			{
				m_currentTab = subTabs.Length - 1;
			}
			int num = m_currentTab - 1;
			if (num < 0)
			{
				num = subTabs.Length - 1;
			}
			previousTabText.text = subTabsTexts[num].text;
			previousTabImage.sprite = subTabsImages[num].sprite;
		}
	}
}
