using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class UIGameViewerView : UIScreenView
	{
		public UIGameViewerInformationLayer info;

		public UIGameViewerControlsLayer controls;

		public GameObject tabContainer;

		public List<GameObject> directorLayoutList;

		public bool directorModeAllowed;

		public ViewerModeType mode;

		internal bool notificationLock;

		private void Start()
		{
		}

		public void RefreshTargets(List<GamePlayerData> p_players)
		{
			UIGameViewerControlsPlaybackPanel playback = controls.playback;
			List<string> list = new List<string>();
			for (int i = 0; i < p_players.Count; i++)
			{
				GamePlayerData gamePlayerData = p_players[i];
				if (gamePlayerData != null)
				{
					list.Add(gamePlayerData.name.ToUpper());
				}
			}
			playback.SetTargets(list);
		}

		public virtual void SetMode(ViewerModeType p_mode)
		{
		}

		public void EnableControls(bool p_focus)
		{
			controls.fade.FadeIn(0.2f);
			base.app.view.ui.navigation.enabled = true;
			if (p_focus)
			{
				UINavigation.Focus(controls.playback.targetStepper);
			}
		}

		public void DisableControls(float p_duration = 0f)
		{
			base.app.view.ui.navigation.enabled = false;
			if (p_duration <= 0f)
			{
				controls.fade.alpha = -0.1f;
			}
			else
			{
				controls.fade.FadeOut(p_duration);
			}
		}

		public void SetDirectorModeAllowed(bool p_flag)
		{
			tabContainer.SetActive(p_flag);
			for (int i = 0; i < directorLayoutList.Count; i++)
			{
				directorLayoutList[i].SetActive(p_flag);
			}
			directorModeAllowed = p_flag;
		}

		public bool ControlsEnabled()
		{
			return controls.fade.alpha >= 1f;
		}
	}
}
