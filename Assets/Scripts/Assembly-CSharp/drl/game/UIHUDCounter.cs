using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIHUDCounter : MonoBehaviour
	{
		public FadeComponent fade;

		public List<FadeComponent> lamps;

		public List<FadeComponent> glows;

		protected void Awake()
		{
			for (int i = 0; i < lamps.Count; i++)
			{
				Image component = glows[i].GetComponent<Image>();
				Material material = Object.Instantiate(component.material);
				material.name = component.material.name + "-copy";
				component.material = material;
			}
			Clear();
		}

		public void Clear()
		{
			for (int i = 0; i < lamps.Count; i++)
			{
				lamps[i].alpha = 0f;
				FadeComponent fadeComponent = glows[i];
				fadeComponent.alpha = 0f;
				fadeComponent.transform.localEulerAngles = new Vector3(0f, 0f, -30f);
				Rotator component = fadeComponent.GetComponent<Rotator>();
				component.Clear();
				component.speed = Vector3.zero;
			}
		}

		public void FadeLamp(int p_id, bool p_on)
		{
			float p_duration = (p_on ? 0.8f : 0.3f);
			FadeComponent fadeComponent = lamps[p_id];
			if (p_on)
			{
				fadeComponent.alpha = 1f;
			}
			fadeComponent.Fade(p_on ? 1f : 0f, p_duration, 0f, Cubic.Out);
			fadeComponent = glows[p_id];
			if (p_on)
			{
				fadeComponent.alpha = 1f;
			}
			fadeComponent.Fade(p_on ? 0.2f : 0f, p_duration, 0f, Cubic.Out);
			Rotator component = fadeComponent.GetComponent<Rotator>();
			if (p_on)
			{
				component.speed = new Vector3(0f, 0f, -90f);
			}
			Tween.Add(component, "speed", new Vector3(0f, 0f, p_on ? (-2f) : 0f), p_duration, Cubic.Out);
		}
	}
}
