using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace thelab.mvc
{
	public class DropdownView : UIElementView
	{
		protected List<GameObject> optionItems;

		public List<Dropdown.OptionData> options;

		private Transform m_dropdownList;

		public Dropdown dropdown => Assert<Dropdown>("dropdown");

		public int index => dropdown.value;

		protected virtual void Awake()
		{
			dropdown.onValueChanged.AddListener(OnChange);
			options = dropdown.options;
		}

		protected virtual void Start()
		{
			dropdown.Show();
			dropdown.Hide();
			dropdown.interactable = false;
		}

		protected virtual void OnChange(int v)
		{
			Notify(notification + "@change");
			OnState("change");
		}

		protected override void OnState(string s)
		{
			base.OnState(s);
			if (!(s == "up"))
			{
				return;
			}
			this.TimerRunOnce(delegate
			{
				if (dropdown.transform.Find("Dropdown List") == null)
				{
					OnClose();
				}
				else
				{
					OnOpen();
				}
			}, 0.05f);
		}

		protected virtual bool OnInputDown()
		{
			return false;
		}

		protected virtual bool OnInputUp()
		{
			return false;
		}

		protected virtual void OnOpen()
		{
			Transform transform = dropdown.transform.Find("Dropdown List");
			if (transform == null)
			{
				return;
			}
			RectTransform content = transform.GetComponent<ScrollRect>().content;
			optionItems = new List<GameObject>();
			foreach (Transform item in content)
			{
				if (item.gameObject.activeSelf)
				{
					optionItems.Add(item.gameObject);
				}
			}
		}

		protected virtual void OnClose()
		{
			dropdown.Hide();
		}

		protected virtual void Update()
		{
			if (OnInputUp())
			{
				OnState("lclick");
			}
			if (OnInputDown())
			{
				OnState("rclick");
			}
			Transform transform = dropdown.transform.Find("Dropdown List");
			if (transform != m_dropdownList)
			{
				m_dropdownList = transform;
				if (transform != null)
				{
					OnOpen();
				}
				else
				{
					OnClose();
				}
			}
		}

		public virtual void Set(List<Dropdown.OptionData> p_options)
		{
			dropdown.options.Clear();
			foreach (Dropdown.OptionData p_option in p_options)
			{
				dropdown.options.Add(p_option);
			}
			dropdown.value = 0;
			Refresh();
		}

		public virtual void Set(List<string> p_options)
		{
			dropdown.options.Clear();
			foreach (string p_option in p_options)
			{
				dropdown.options.Add(new Dropdown.OptionData(p_option));
			}
			dropdown.value = 0;
			Refresh();
		}

		public virtual void Clear()
		{
			dropdown.options.Clear();
			Refresh();
		}

		public virtual Dropdown.OptionData Value()
		{
			int value = dropdown.value;
			Dropdown.OptionData result = new Dropdown.OptionData("");
			if (value < 0)
			{
				return result;
			}
			if (value >= options.Count)
			{
				return result;
			}
			return options[dropdown.value];
		}

		public virtual void Select(int p_option)
		{
			if (p_option <= dropdown.options.Count - 1 && p_option >= 0)
			{
				dropdown.value = p_option;
				Refresh();
			}
		}

		public virtual void Select(string p_option)
		{
			for (int i = 0; i < options.Count; i++)
			{
				if (options[i].text == p_option)
				{
					dropdown.value = i;
					Refresh();
					break;
				}
			}
		}

		public virtual void Add(Dropdown.OptionData p_option)
		{
			dropdown.options.Add(p_option);
		}

		public virtual void Remove(Dropdown.OptionData p_option)
		{
			if (dropdown.options.Contains(p_option))
			{
				dropdown.options.Remove(p_option);
				Refresh();
			}
		}

		public virtual void Refresh()
		{
			dropdown.RefreshShownValue();
		}
	}
}
