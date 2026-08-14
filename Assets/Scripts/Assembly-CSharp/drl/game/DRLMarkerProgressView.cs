using System.Collections.Generic;
using UnityEngine.UI;
using thelab.mvc;

namespace drl.game
{
	public class DRLMarkerProgressView : UIElementView
	{
		public Text leftField;

		public Text rightField;

		public List<Image> barFields;

		public List<Image> fillFields;

		public string left
		{
			set
			{
				leftField.text = value;
			}
		}

		public string right
		{
			set
			{
				rightField.text = value;
			}
		}

		public void SetCount(int p_value)
		{
			int count = barFields.Count;
			for (int i = 0; i < count; i++)
			{
				fillFields[i].enabled = i < p_value;
			}
		}

		public void SetTotal(int p_value)
		{
			int count = barFields.Count;
			for (int i = 0; i < count; i++)
			{
				barFields[i].gameObject.SetActive(i < p_value);
				fillFields[i].gameObject.SetActive(i < p_value);
			}
		}
	}
}
