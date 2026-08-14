using UnityEngine.UI;

namespace thelab.mvc
{
	public class ToggleView : UIElementView
	{
		public Toggle toggle;

		public Graphic[] ticks;

		public bool isOn
		{
			get
			{
				return toggle.isOn;
			}
			set
			{
				toggle.isOn = value;
			}
		}

		protected virtual void Awake()
		{
			toggle = GetComponent<Toggle>();
			if (!toggle)
			{
				toggle = GetComponentInChildren<Toggle>();
			}
			if ((bool)toggle)
			{
				toggle.onValueChanged.AddListener(OnChange);
				SetState(toggle.isOn);
			}
		}

		protected virtual void OnChange(bool v)
		{
			if (base.enabled)
			{
				Graphic[] array = ticks;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].gameObject.SetActive(v);
				}
				Notify(notification + "@change");
			}
		}

		public virtual void SetState(bool p_flag)
		{
			Graphic[] array = ticks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(p_flag);
			}
		}
	}
}
