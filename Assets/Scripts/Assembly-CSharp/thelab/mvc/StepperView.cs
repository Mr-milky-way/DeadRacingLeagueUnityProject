using UnityEngine;
using UnityEngine.UI;

namespace thelab.mvc
{
	public class StepperView : UIElementView
	{
		public enum Mode
		{
			Clamp = 0,
			Loop = 1
		}

		public Mode mode;

		public int index;

		public string format = "0.0";

		public int min;

		public int max = 10;

		public string[] labels;

		public Text labelField;

		public Button prevButton;

		public Button nextButton;

		public string label
		{
			get
			{
				if (labels.Length == 0)
				{
					return "";
				}
				if (index < 0)
				{
					return "";
				}
				if (index >= labels.Length)
				{
					return "";
				}
				return labels[index];
			}
		}

		protected virtual void Awake()
		{
			if ((bool)nextButton)
			{
				nextButton.onClick.AddListener(Next);
			}
			if ((bool)prevButton)
			{
				prevButton.onClick.AddListener(Prev);
			}
			Refresh();
		}

		public void Next()
		{
			int num = index;
			index++;
			OnState("change");
			Notify(notification + "@change", num);
		}

		public void Prev()
		{
			int num = index;
			index--;
			OnState("change");
			Notify(notification + "@change", num);
		}

		public void Set(int p_index)
		{
			index = p_index;
			OnState("change");
			Notify(notification + "@change", index);
		}

		protected virtual void OnChange()
		{
		}

		public void Refresh()
		{
			switch (mode)
			{
			case Mode.Clamp:
				index = Mathf.Clamp(index, min, max);
				break;
			case Mode.Loop:
				if (index < min)
				{
					index = max;
				}
				else if (index > max)
				{
					index = min;
				}
				break;
			}
			if ((bool)labelField)
			{
				labelField.text = GetLabelText();
			}
			OnChange();
		}

		public virtual string GetLabelText()
		{
			if (labels.Length == 0)
			{
				return index.ToString(format);
			}
			return label;
		}

		protected override void OnState(string p_state)
		{
			int num = index;
			switch (p_state)
			{
			case "lclick":
				if (!nextButton && !prevButton)
				{
					Next();
				}
				break;
			case "rclick":
				if (!nextButton && !prevButton)
				{
					Prev();
				}
				break;
			case "scroll":
			{
				int num2 = (int)scroll.y;
				index -= num2;
				OnState("change");
				Notify(notification + "@change", num);
				break;
			}
			case "change":
				Refresh();
				break;
			}
		}

		protected virtual bool OnInputDown()
		{
			return false;
		}

		protected virtual bool OnInputUp()
		{
			return false;
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
		}
	}
	public class StepperView<T> : StepperView
	{
		public T value;

		public bool showValue;

		protected override void OnChange()
		{
			value = GetValue(index);
			if (showValue && (bool)labelField)
			{
				labelField.text = GetValueString();
			}
		}

		private void OnEnable()
		{
			Refresh();
		}

		protected virtual T GetValue(int p_count)
		{
			return default(T);
		}

		protected virtual string GetValueString()
		{
			if (value != null)
			{
				return value.ToString();
			}
			return "";
		}

		public override string GetLabelText()
		{
			if (!showValue)
			{
				return base.GetLabelText();
			}
			return GetValueString();
		}
	}
}
