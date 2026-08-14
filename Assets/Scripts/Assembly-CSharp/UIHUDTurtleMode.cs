using UnityEngine;
using UnityEngine.UI;
using drl.game;
using thelab.core;
using thelab.mvc;

[RequireComponent(typeof(FadeComponent))]
public class UIHUDTurtleMode : View<DRLApp>
{
	private Activity m_activity;

	public Text label;

	private FadeComponent fade => AssertLocal<FadeComponent>("fade");

	private void Start()
	{
		fade.alpha = 0f;
	}

	public void SetDroneArmed(bool p_flag)
	{
		if (m_activity != null)
		{
			m_activity.Stop();
		}
		label.text = (p_flag ? base.app.model.storage.locale.Get("race-hud.drone-armed", "DRONE ARMED") : base.app.model.storage.locale.Get("race-hud.drone-disarmed", "DRONE DISARMED"));
		fade.FadeIn();
		m_activity = this.TimerRunOnce(delegate
		{
			fade.FadeOut();
		}, 1.4f);
	}

	public void SetDroneTurtle(bool p_flag)
	{
		if (m_activity != null)
		{
			m_activity.Stop();
		}
		label.text = (p_flag ? base.app.model.storage.locale.Get("race-hud.drone-turtle-on", "TURTLE MODE ON") : base.app.model.storage.locale.Get("race-hud.drone-turtle-off", "TURTLE MODE OFF"));
		fade.FadeIn();
		m_activity = this.TimerRunOnce(delegate
		{
			fade.FadeOut();
		}, 1.4f);
	}
}
