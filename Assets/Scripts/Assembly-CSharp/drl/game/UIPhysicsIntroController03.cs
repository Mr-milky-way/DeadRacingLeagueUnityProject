using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIPhysicsIntroController03 : Controller<DRLApp>
	{
		private int fps;

		private float m_frame_elapsed;

		private int m_frames_rendered;

		private float m_timer;

		public UIPhysicsIntroView03 view => AssertLocal<UIPhysicsIntroView03>("view");

		protected void Awake()
		{
			fps = 0;
			m_frame_elapsed = 0f;
			m_frames_rendered = 0;
			Activity.Run(FPSWatch, 0f, false);
		}

		public void Restart()
		{
			fps = 0;
			m_frame_elapsed = 0f;
			m_frames_rendered = 0;
		}

		private bool FPSWatch()
		{
			if (!this)
			{
				return false;
			}
			if (!base.enabled)
			{
				return true;
			}
			m_frame_elapsed += Time.unscaledDeltaTime;
			if (m_timer < 1.2f)
			{
				m_timer += Time.deltaTime;
			}
			if (m_frame_elapsed >= 1f)
			{
				fps = Time.renderedFrameCount - m_frames_rendered;
				m_frames_rendered = Time.renderedFrameCount;
				m_frame_elapsed = 0f;
				if (m_timer >= 1.2f)
				{
					if (!view.fpsContent.activeInHierarchy)
					{
						view.fpsContent.SetActive(value: true);
					}
					view.fpsField.text = fps.ToString();
					RefreshPerformanceColor();
				}
			}
			return true;
		}

		private void RefreshPerformanceColor(int p_low = 40, int p_high = 60)
		{
			view.fpsBackground.color = ((fps <= p_low) ? view.performanceColors[0] : ((fps < p_high) ? view.performanceColors[1] : view.performanceColors[2]));
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!view.current)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				base.app.view.ui.SetDark(p_flag: false);
				RunOnce(1f / 30f, delegate
				{
					base.app.view.ui.footer.Hide(0f);
					base.app.controller.game.FadeBlur(0f, 0f);
				});
				UINavigation.focus = view.rightNavigation;
				break;
			case "intro.screens.close":
				base.app.view.ui.screens.CloseAllScreens();
				break;
			case "intro.graphics@open":
				base.app.view.ui.screens.Open<UISettingsSystemView>("settings-system-screen");
				base.app.view.ui.SetDark(p_flag: false);
				RunOnce(1f / 30f, delegate
				{
					base.app.view.ui.footer.Hide(0f);
					base.app.controller.game.FadeBlur(0f, 0f);
				});
				Notify("game.pause");
				base.app.model.game.simulation.cameras.Get(0).orbit.enabled = false;
				if (base.app.model.game != null)
				{
					RunOnce(0.2f, delegate
					{
						base.app.model.game.simulation.cameras.Get(0).follow.enabled = false;
						base.app.model.game.simulation.cameras.Get(0).orbit.enabled = false;
					});
				}
				break;
			}
		}
	}
}
