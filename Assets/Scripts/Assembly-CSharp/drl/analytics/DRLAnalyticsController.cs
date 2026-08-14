using UnityEngine;
using drl.game;
using thelab.core;
using thelab.mvc;

namespace drl.analytics
{
	public class DRLAnalyticsController : Controller<DRLApp>
	{
		private readonly GAService service = new GAService();

		protected override void Start()
		{
			base.Start();
			Debug.Log("DRLAnalyticsController > Start");
			service.Initialize(DRLApp.GetVersionString());
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (this == null)
			{
				return;
			}
			switch (p_event)
			{
			case "analytics.ui.menu.opened":
			{
				UIScreen uIScreen = (UIScreen)p_data[0];
				string text2 = (string)p_data[1];
				if (base.app == null || !base.app.level.IsLevelLoaded("main") || uIScreen == null || uIScreen.title == "Home" || string.IsNullOrEmpty(text2))
				{
					break;
				}
				text2 = text2.ToLower().Replace('/', ':');
				string[] array = text2.Split(':');
				text2 = string.Empty;
				int num = Mathf.Min(array.Length, 3);
				for (int i = 0; i < num; i++)
				{
					text2 += array[i];
					if (i < num - 1)
					{
						text2 += ":";
					}
				}
				service.Design.UI.ClickMenuCard(text2);
				break;
			}
			case "home.sandbox@click":
				service.Design.UI.ClickMenuCard("home:physics");
				break;
			case "analytics.gameplay.loadgame":
			{
				DRLAppArguments dRLAppArguments = (DRLAppArguments)p_data[0];
				if (!(dRLAppArguments == null))
				{
					GameFlag mode = dRLAppArguments.game.mode;
					GameFlag type = dRLAppArguments.game.type;
					string text3 = ((dRLAppArguments.game.map == null) ? "" : dRLAppArguments.game.map.label);
					string text4 = (dRLAppArguments.game.isCustomMap ? "custom" : (dRLAppArguments.game.track ? dRLAppArguments.game.track.label : ""));
					if (!string.IsNullOrEmpty(text3) && !string.IsNullOrEmpty(text4))
					{
						service.Design.Gameplay.LoadGame(mode.ToString(), type.ToString(), text3, text4);
					}
				}
				break;
			}
			case "analytics.controller.connected":
				if (p_data[0] != null)
				{
					service.Design.Controllers.ConnectedNew((string)p_data[0]);
				}
				break;
			case "analytics.tryouts.registered":
				service.Design.Tryouts.Registered();
				break;
			case "analytics.tryouts.completed-step":
			{
				string text = (string)p_data[0];
				if (!string.IsNullOrEmpty(text))
				{
					service.Design.Tryouts.CompletedStep(text);
				}
				break;
			}
			}
		}

		public void OnPersistency()
		{
			base.app.controller.analytics = this;
			Debug.Log("DRLAnalyticsController> OnPersistency / Service Initialize");
		}
	}
}
