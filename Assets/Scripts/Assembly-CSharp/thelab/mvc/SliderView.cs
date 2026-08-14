using UnityEngine.UI;

namespace thelab.mvc
{
	public class SliderView : UIElementView
	{
		public Slider slider;

		public Text field;

		public bool updateLabel = true;

		public float scrollStep = 1f;

		public string unit;

		public string format = "0.00";

		protected void Awake()
		{
			if (!field)
			{
				field = GetComponentInChildren<Text>();
			}
			slider = GetComponent<Slider>();
			if ((bool)slider)
			{
				slider.onValueChanged.AddListener(OnChange);
				UpdateField();
			}
		}

		protected virtual void OnChange(float v)
		{
			UpdateField();
			if ((bool)slider && slider.enabled)
			{
				Notify(notification + "@change");
			}
		}

		protected override void OnState(string p_state)
		{
			if (p_state != null && p_state == "scroll")
			{
				int num = (int)scroll.y;
				slider.value += (float)num * scrollStep;
			}
		}

		public void UpdateField()
		{
			if (updateLabel && (bool)field)
			{
				field.text = slider.value.ToString(format) + unit;
			}
		}
	}
}
