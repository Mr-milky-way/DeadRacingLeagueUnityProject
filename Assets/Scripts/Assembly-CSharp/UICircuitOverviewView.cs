using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.game;
using thelab.core;

public class UICircuitOverviewView : UIScreenView
{
	[Header("UI")]
	public Text circuitTotalTimeText;

	public Text resetText;

	public Text trackDifficultyLabel;

	public Text trackDifficultySelectionLabel;

	public Text racesCountText;

	public ListComponent mapCardsListField;

	public ListComponent raceStripesListField;

	public GameObject exitButton;

	public GameObject circuitSelectionButton;

	public UINavigation settingsNav;

	public DRLStepperView opponentModeStepper;

	public DRLStepperView opponentDifficultyStepper;

	public UILeaderboardCardView leaderCard;

	public GameObject leaderCardPreview;

	public GameObject leaderCardContent;

	public UIStatusView status;

	private int attempts;

	private int racesComplete;

	private int totalRaces;

	[Header("Circuit Information")]
	public DRLCircuitData circuitData;

	private float circuitTotalTime;

	public CircuitsOpponentMode opponentMode
	{
		get
		{
			return (CircuitsOpponentMode)opponentModeStepper.index;
		}
		set
		{
			opponentModeStepper.index = (int)value;
			opponentModeStepper.Refresh();
		}
	}

	public CircuitsOpponentDifficulty opponentDifficulty
	{
		get
		{
			return (CircuitsOpponentDifficulty)opponentDifficultyStepper.index;
		}
		set
		{
			opponentDifficultyStepper.index = (int)value;
			opponentDifficultyStepper.Refresh();
		}
	}

	public int Attempts
	{
		get
		{
			return attempts;
		}
		set
		{
			attempts = value;
			resetText.text = value.ToString();
		}
	}

	public int RacesComplete
	{
		get
		{
			return racesComplete;
		}
		set
		{
			racesComplete = value;
			racesCountText.text = racesComplete + "/" + totalRaces;
		}
	}

	public int TotalRaces
	{
		get
		{
			return totalRaces;
		}
		set
		{
			totalRaces = value;
			racesCountText.text = racesComplete + "/" + totalRaces;
		}
	}

	public float CircuitTotalTime
	{
		get
		{
			return circuitTotalTime;
		}
		set
		{
			circuitTotalTime = value;
			circuitTotalTimeText.text = Format.SecondsToMMSSFFF(value);
		}
	}

	public void Set()
	{
		if (circuitData == null)
		{
			Debug.LogWarning("UICircuitOverviewView> Set - no circuit data defined!");
			status.SetWarning("CIRCUIT INVALID!");
			this.TimerRunOnce(delegate
			{
				status.fade.FadeOut(0f);
				Notify("ui.screen@return");
			}, 2f);
			return;
		}
		Clear();
		CircuitStateModel circuits = base.app.model.storage.state.player.circuits;
		base.screen.title = circuitData.name;
		CircuitStateModel.CircuitsProgressData circuitProgress = circuits.GetCircuitProgress(circuitData.guid);
		if (circuitProgress != null)
		{
			CircuitTotalTime = circuitProgress.time;
			RacesComplete = circuitProgress.progress;
			Attempts = circuitProgress.attempts;
		}
		TotalRaces = circuitData.maps.Length;
		for (int num = 0; num < TotalRaces; num++)
		{
			UICircuitMapItemView uICircuitMapItemView = mapCardsListField.Push<UICircuitMapItemView>();
			uICircuitMapItemView.Set(circuitData.maps[num]);
			uICircuitMapItemView.HideFooter();
			if (num <= RacesComplete)
			{
				uICircuitMapItemView.UnlockMap();
			}
			else
			{
				uICircuitMapItemView.LockMap();
			}
			if (circuitProgress != null && circuitProgress.times.Count > num)
			{
				uICircuitMapItemView.SetTrackComplete(circuitProgress.times[num]);
			}
			else
			{
				uICircuitMapItemView.ResetTrackComplete();
			}
		}
		Localization locale = base.app.model.storage.locale;
		switch (circuitData.difficulty)
		{
		case 0:
			trackDifficultyLabel.text = locale.Get("map.map-track-cards.difficulty.easy", "EASY");
			break;
		case 1:
			trackDifficultyLabel.text = locale.Get("map.map-track-cards.difficulty.medium", "MEDIUM");
			break;
		case 2:
			trackDifficultyLabel.text = locale.Get("map.map-track-cards.difficulty.hard", "HARD");
			break;
		}
		opponentMode = circuits.opponentMode;
		opponentDifficulty = circuits.opponentDifficulty;
		if (DRLApp.offline || circuitData.ContainsTag(DRLCircuitData.Tag.tryouts))
		{
			opponentMode = CircuitsOpponentMode.Off;
			opponentModeStepper.interactable = false;
		}
		SetOpponentDifficultyFlag(opponentMode == CircuitsOpponentMode.On);
		SetNavigation();
		SetRaceStripes();
	}

	private void SetNavigation()
	{
		List<UICircuitMapItemView> list = mapCardsListField.GetList<UICircuitMapItemView>();
		for (int i = 0; i < list.Count; i++)
		{
			UINavigation component = list[i].GetComponent<UINavigation>();
			if (i == 0)
			{
				component.left = settingsNav;
				settingsNav.right = component;
			}
			else
			{
				component.left = list[i - 1].GetComponent<UINavigation>();
			}
			if (i == list.Count)
			{
				component.right = null;
			}
			else if (i < list.Count && i + 1 < list.Count)
			{
				component.right = list[i + 1].GetComponent<UINavigation>();
			}
		}
	}

	public void SetRaceStripes()
	{
		for (int i = 0; i < totalRaces; i++)
		{
			UICircuitMapItemStripeView uICircuitMapItemStripeView = raceStripesListField.Push<UICircuitMapItemStripeView>();
			if (i < RacesComplete)
			{
				uICircuitMapItemStripeView.SetComplete();
			}
			else
			{
				uICircuitMapItemStripeView.Reset();
			}
		}
	}

	public void SetOpponentDifficultyFlag(bool p_flag)
	{
		CanvasGroup component = opponentDifficultyStepper.GetComponent<CanvasGroup>();
		component.alpha = (p_flag ? 1f : 0.2f);
		bool blocksRaycasts = (component.interactable = p_flag);
		component.blocksRaycasts = blocksRaycasts;
		opponentDifficultyStepper.interactable = p_flag;
	}

	public void SetLeader(DRLCircuitLeaderboardData p_data)
	{
		CanvasGroup component = leaderCard.GetComponent<CanvasGroup>();
		if (p_data == null)
		{
			component.alpha = 0.15f;
			component.interactable = false;
			component.blocksRaycasts = false;
			if ((bool)leaderCardPreview)
			{
				leaderCardPreview.gameObject.SetActive(value: false);
			}
			if ((bool)leaderCardContent)
			{
				leaderCardContent.gameObject.SetActive(value: false);
			}
			return;
		}
		component.alpha = 1f;
		component.interactable = true;
		component.blocksRaycasts = true;
		if ((bool)leaderCardPreview)
		{
			leaderCardPreview.gameObject.SetActive(value: true);
		}
		if ((bool)leaderCardContent)
		{
			leaderCardContent.gameObject.SetActive(value: true);
		}
		leaderCard.Set(p_data);
	}

	public void Clear()
	{
		if ((bool)mapCardsListField)
		{
			mapCardsListField.Clear();
			raceStripesListField.Clear();
			CircuitTotalTime = 0f;
			RacesComplete = 0;
			Attempts = 0;
		}
	}
}
