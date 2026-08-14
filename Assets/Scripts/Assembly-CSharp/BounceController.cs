using UnityEngine;
using thelab.mvc;

public class BounceController : Controller<BounceApplication>
{
	public override void OnNotification(string p_event, Object p_target, params object[] p_data)
	{
		if (p_event == null)
		{
			return;
		}
		switch (p_event)
		{
		case "scene.load":
			Log("Scene [" + p_data[0]?.ToString() + "][" + p_data[1]?.ToString() + "] loaded");
			break;
		case "ball.hit":
			if ((string)p_data[0] == "ground")
			{
				base.app.model.bounces++;
				Log("Hit " + base.app.model.bounces);
				if (base.app.model.bounces >= base.app.model.winCondition)
				{
					base.app.view.ball.enabled = false;
					base.app.view.ball.GetComponent<Rigidbody>().isKinematic = true;
					Notify("game.complete");
				}
			}
			break;
		case "game.complete":
			Log("Victory!");
			base.app.view.timer.Play();
			break;
		case "mid.trigger.enter":
			Log("Mid Fall Enter!");
			((ColliderView)p_target).collider.enabled = false;
			break;
		case "start.trigger.exit":
			Log("Start Fall Exit!");
			((ColliderView)p_target).collider.enabled = false;
			break;
		case "start.trigger.stay":
			Log("Start Fall Stay [" + Time.time + "]");
			break;
		case "ping.timer.step":
		{
			TimerView timerView = (TimerView)p_target;
			Log("Ping " + timerView.step);
			break;
		}
		case "ping.timer.complete":
			Log("Ping Complete!");
			break;
		}
	}
}
