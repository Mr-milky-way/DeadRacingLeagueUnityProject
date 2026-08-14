using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	[ExecuteInEditMode]
	public class TextPosition : TextNumber<float>
	{
		public Transform anchor;

		public bool x = true;

		public bool y = true;

		public bool z = true;

		private List<float> m_values;

		public override string GetStringValue()
		{
			if (m_values == null)
			{
				m_values = new List<float>();
			}
			m_values.Clear();
			Transform transform = (anchor ? anchor : base.transform);
			if (x)
			{
				m_values.Add(transform.position.x);
			}
			if (y)
			{
				m_values.Add(transform.position.y);
			}
			if (z)
			{
				m_values.Add(transform.position.z);
			}
			string text = "";
			if (m_values.Count >= 1)
			{
				text = text + m_values[0].ToString(format).Replace(",", ".") + postfix;
			}
			if (m_values.Count >= 2)
			{
				text = text + "," + m_values[1].ToString(format).Replace(",", ".") + postfix;
			}
			if (m_values.Count >= 3)
			{
				text = text + "," + m_values[2].ToString(format).Replace(",", ".") + postfix;
			}
			return text;
		}

		protected virtual void OnEnable()
		{
			SetText(GetStringValue());
		}

		protected virtual void OnWillRenderObject()
		{
			if (base.transform.hasChanged)
			{
				SetText(GetStringValue());
				base.transform.hasChanged = false;
			}
		}
	}
}
