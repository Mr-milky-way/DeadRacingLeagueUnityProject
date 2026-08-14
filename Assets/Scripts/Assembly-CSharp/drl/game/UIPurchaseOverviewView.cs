using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace drl.game
{
	public class UIPurchaseOverviewView : UIScreenView
	{
		public List<RectTransform> eaxOfferAssets;

		public List<RectTransform> fullOfferAssets;

		public Text eaxOfferPriceField;

		public Text fullOfferPriceField;

		public UIStatusView status;

		public string price
		{
			set
			{
				Text text = eaxOfferPriceField;
				string text2 = (fullOfferPriceField.text = (string.IsNullOrEmpty(value) ? "--" : value));
				text.text = text2;
			}
		}

		public void SetMode(bool p_full_license)
		{
			for (int i = 0; i < eaxOfferAssets.Count; i++)
			{
				eaxOfferAssets[i].gameObject.SetActive(!p_full_license);
			}
			for (int j = 0; j < fullOfferAssets.Count; j++)
			{
				fullOfferAssets[j].gameObject.SetActive(p_full_license);
			}
			price = "";
		}
	}
}
