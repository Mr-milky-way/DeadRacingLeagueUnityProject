using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	[RequireComponent(typeof(Text))]
	public class UITruncateText : ActivityBehaviour, ILateUpdateable
	{
		public enum TruncateTextWith
		{
			TargetContainer = 0,
			NumericalWidth = 1
		}

		public float marginWidth = 20f;

		public TruncateTextWith truncateWith;

		public RectTransform targetContainer;

		public float numericalWidth = 300f;

		public string appendTruncateString = "...";

		public bool dynamicString;

		public Text textField;

		public int textLastCharacterCount;

		protected void Awake()
		{
			if (!textField)
			{
				textField = GetComponent<Text>();
			}
		}

		protected void OnEnable()
		{
			TruncateText();
		}

		public void Refresh()
		{
			TruncateText();
		}

		private float GetMaxWidth()
		{
			if (!textField)
			{
				textField = GetComponent<Text>();
			}
			float width = numericalWidth;
			if (truncateWith == TruncateTextWith.TargetContainer)
			{
				if (targetContainer == null)
				{
					targetContainer = textField.rectTransform;
				}
				width = targetContainer.rect.width;
			}
			width -= marginWidth;
			if (width <= 0f)
			{
				width += marginWidth;
			}
			return width;
		}

		public void TruncateText()
		{
			float maxWidth = GetMaxWidth();
			if (textField.preferredWidth < maxWidth)
			{
				return;
			}
			string text = textField.text;
			bool flag = false;
			for (int i = 0; i < text.Length; i++)
			{
				textField.text = new StringInfo(text).SubstringByTextElements(0, i + 1);
				if (!(textField.preferredWidth >= maxWidth))
				{
					continue;
				}
				for (int j = 1; j < i; j++)
				{
					textField.text = new StringInfo(text).SubstringByTextElements(0, i - j) + appendTruncateString;
					if (textField.preferredWidth < maxWidth)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			textLastCharacterCount = textField.text.Length;
		}

		public void OnLateUpdate()
		{
			if (dynamicString && (bool)textField && textField.text.Length != textLastCharacterCount)
			{
				TruncateText();
			}
		}
	}
}
