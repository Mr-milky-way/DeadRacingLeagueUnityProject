using UnityEngine;
using UnityEngine.UI;

namespace thelab.mvc
{
	public class InputFieldView : UIElementView
	{
		[SerializeField]
		protected InputField m_field;

		[SerializeField]
		protected Text m_placeholderText;

		public new bool enabled
		{
			get
			{
				return base.enabled;
			}
			set
			{
				base.enabled = value;
				if ((bool)field)
				{
					field.interactable = value;
				}
			}
		}

		public InputField field
		{
			get
			{
				if (m_field == null)
				{
					m_field = GetComponentInChildren<InputField>();
				}
				return m_field;
			}
		}

		public Text placeholderText
		{
			get
			{
				if (m_placeholderText == null && field.placeholder != null)
				{
					m_placeholderText = field.placeholder.GetComponent<Text>();
				}
				return m_placeholderText;
			}
		}

		public string text
		{
			get
			{
				if (!field)
				{
					Awake();
				}
				if (!(field != null))
				{
					return null;
				}
				return field.text;
			}
			set
			{
				if ((bool)field)
				{
					field.text = value;
				}
			}
		}

		public string placeholder
		{
			get
			{
				if (!(placeholderText != null))
				{
					return null;
				}
				return placeholderText.text;
			}
			set
			{
				if ((bool)placeholderText)
				{
					placeholderText.text = value;
				}
			}
		}

		protected virtual void Awake()
		{
			m_field = GetComponentInChildren<InputField>();
			if ((bool)field)
			{
				field.onValueChanged.AddListener(OnChange);
				field.onEndEdit.AddListener(OnEndEdit);
				m_placeholderText = ((field.placeholder != null) ? field.placeholder.GetComponent<Text>() : null);
			}
		}

		protected virtual void OnChange(string v)
		{
			if (base.isActiveAndEnabled)
			{
				Notify(notification + "@change");
			}
		}

		protected void OnEndEdit(string v)
		{
			if (!base.isActiveAndEnabled || !base.gameObject.activeInHierarchy)
			{
				OnDeselect(null);
				return;
			}
			RunOnce(1f / 60f, delegate
			{
				if (!base.isActiveAndEnabled || !base.gameObject.activeInHierarchy)
				{
					OnDeselect(null);
				}
				else
				{
					OnChangeEnd(v);
				}
			});
		}

		protected virtual void OnChangeEnd(string v)
		{
			Notify(notification + "@end-edit", v);
		}
	}
}
