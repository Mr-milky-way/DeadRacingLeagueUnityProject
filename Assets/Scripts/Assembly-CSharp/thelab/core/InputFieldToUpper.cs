using System;
using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	public class InputFieldToUpper : MonoBehaviour
	{
		protected void Awake()
		{
			InputField component = GetComponent<InputField>();
			if ((bool)component)
			{
				component.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(component.onValidateInput, new InputField.OnValidateInput(Validate));
			}
		}

		private char Validate(string p_text, int p_index, char p_char)
		{
			return p_char.ToString().ToUpper()[0];
		}
	}
}
