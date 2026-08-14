using System;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UICardButtonMapTrack : UICardView
	{
		[Header("Title")]
		public Text title0Field;

		public Text title1Field;

		[Header("XP")]
		public RectTransform progressionXPContainer;

		public RectTransform progressionXPSpace;

		public Text progressionXPField;

		[Header("Distance")]
		public GameObject distanceContainer;

		public GameObject distanceSpace;

		public Text distanceField;

		[Header("Laps")]
		public GameObject lapsContainer;

		public GameObject lapsSpace;

		public Text lapsField;

		[Header("Collectable")]
		public GameObject collectableContainer;

		public GameObject collectableSpace;

		public Text collectableField;

		[Header("Difficulty")]
		public GameObject difficultyContainer;

		public GameObject difficultySpace;

		public Text difficultyField;

		[Header("Others")]
		public RawImage previewField;

		public RawImage imageField;

		public UIFlareProgressGroup stars;

		public Image difficultyBar;

		public Color difficultyNeutral;

		public Color difficultyBasic;

		public Color difficultyEasy;

		public Color difficultyMedium;

		public Color difficultyHard;

		[Header("Favorite")]
		[SerializeField]
		private RectTransform favoriteContainer;

		[SerializeField]
		private FadeComponent heartIconFade;

		[SerializeField]
		private FadeComponent favoriteHotkeyFade;

		[SerializeField]
		private DRLToggleView favoriteToggleView;

		[SerializeField]
		private DRLGamepadHotkey favoriteGamepadHotkey;

		[Space]
		public new DRLMapTrack data;

		[SerializeField]
		private DRLCommunityMapData communityData;

		public MapData customData;

		private MonoActivity m_score_timer;

		private int m_difficulty;

		public override UICardType type => UICardType.ButtonMapTrack;

		public GameCollectableModel model => AssertLocal<GameCollectableModel>("model");

		public DRLCommunityMapData CommunityData
		{
			get
			{
				return communityData;
			}
			set
			{
				communityData = value;
			}
		}

		public string title0
		{
			set
			{
				UIReflection.Set(title0Field, value);
			}
		}

		public string title1
		{
			set
			{
				UIReflection.Set(title1Field, value);
			}
		}

		public int difficulty
		{
			get
			{
				return m_difficulty;
			}
			set
			{
				difficultyContainer.SetActive(value: true);
				if (difficultySpace != null)
				{
					difficultySpace.SetActive(value: true);
				}
				m_difficulty = value;
				switch (value)
				{
				case 0:
					difficultyField.text = base.app.model.storage.locale.Get("map.map-track-cards.difficulty.basic", "BASIC");
					if (difficultyBar != null)
					{
						difficultyBar.color = difficultyBasic;
					}
					return;
				case 1:
					difficultyField.text = base.app.model.storage.locale.Get("map.map-track-cards.difficulty.easy", "EASY");
					if (difficultyBar != null)
					{
						difficultyBar.color = difficultyEasy;
					}
					return;
				case 2:
					difficultyField.text = base.app.model.storage.locale.Get("map.map-track-cards.difficulty.medium", "MEDIUM");
					if (difficultyBar != null)
					{
						difficultyBar.color = difficultyMedium;
					}
					return;
				case 3:
					difficultyField.text = base.app.model.storage.locale.Get("map.map-track-cards.difficulty.hard", "HARD");
					if (difficultyBar != null)
					{
						difficultyBar.color = difficultyHard;
					}
					return;
				}
				difficultyField.text = "";
				difficultyContainer.SetActive(value: false);
				if (difficultySpace != null)
				{
					difficultySpace.SetActive(value: false);
				}
				if (difficultyBar != null)
				{
					difficultyBar.color = difficultyNeutral;
				}
			}
		}

		public float distance
		{
			set
			{
				float num = value / 1000f;
				string text = ((num < 1f) ? "M" : "KM");
				num = ((num < 1f) ? (num * 1000f) : num);
				string text2 = ((text == "M") ? num.ToString("0") : num.ToString("0.0"));
				distanceField.text = text2 + text;
				distanceContainer.SetActive(num > 0f);
				if (distanceSpace != null)
				{
					distanceSpace.SetActive(num > 0f);
				}
			}
		}

		public int laps
		{
			get
			{
				if (lapsField != null)
				{
					return Convert.ToInt32(lapsField.text);
				}
				return 0;
			}
			set
			{
				int num = value;
				if ((bool)lapsContainer)
				{
					lapsField.text = num.ToString();
				}
			}
		}

		public int collectableCount
		{
			set
			{
				int num = value;
				string text = num.ToString("0");
				if ((bool)collectableField)
				{
					collectableField.text = text;
				}
				if ((bool)collectableContainer)
				{
					collectableContainer.SetActive(num > 0);
					if (collectableSpace != null)
					{
						collectableSpace.SetActive(num > 0);
					}
				}
				if ((bool)collectableContainer)
				{
					collectableContainer.SetActive(num > 1);
					if (collectableSpace != null)
					{
						collectableSpace.SetActive(num > 1);
					}
				}
			}
		}

		public Texture preview
		{
			set
			{
				UIReflection.Set(previewField, value);
			}
		}

		public Texture image
		{
			set
			{
				UIReflection.Set(imageField, value);
				if ((bool)imageField)
				{
					imageField.enabled = value;
				}
			}
		}

		public void Set(DRLMapTrack p_data)
		{
			if (!p_data)
			{
				return;
			}
			title0 = p_data.map.title.ToUpper().Replace("\n", " ");
			title1 = p_data.title.ToUpper();
			image = p_data.image;
			distance = p_data.length;
			collectableCount = 0;
			difficulty = (p_data.freestyleOnly ? (-1) : p_data.difficulty);
			data = p_data;
			customData = null;
			if (favoriteGamepadHotkey != null)
			{
				favoriteGamepadHotkey.enabled = false;
			}
			switch (base.app.arguments.game.type)
			{
			case GameFlag.Race:
				if (p_data.map.data != null)
				{
					distance = p_data.map.data.mode.race.distance;
					laps = p_data.map.data.mode.race.lapCount;
				}
				if (lapsContainer != null)
				{
					lapsContainer.SetActive(value: true);
				}
				if (lapsSpace != null)
				{
					lapsSpace.SetActive(value: true);
				}
				if (collectableContainer != null)
				{
					collectableContainer.SetActive(value: false);
				}
				if (collectableSpace != null)
				{
					collectableSpace.SetActive(value: false);
				}
				break;
			case GameFlag.Collectable:
				if (p_data.map.data != null)
				{
					collectableCount = p_data.map.data.mode.collectable.collectableCount;
				}
				if (collectableContainer != null)
				{
					collectableContainer.SetActive(value: true);
				}
				if (collectableSpace != null)
				{
					collectableSpace.SetActive(value: true);
				}
				if (lapsContainer != null)
				{
					lapsContainer.SetActive(value: false);
				}
				if (lapsSpace != null)
				{
					lapsSpace.SetActive(value: false);
				}
				break;
			case GameFlag.Freestyle:
				if (lapsContainer != null)
				{
					lapsContainer.SetActive(value: false);
				}
				if (lapsSpace != null)
				{
					lapsSpace.SetActive(value: false);
				}
				if (collectableContainer != null)
				{
					collectableContainer.SetActive(value: false);
				}
				if (collectableSpace != null)
				{
					collectableSpace.SetActive(value: false);
				}
				break;
			}
		}

		public void Set(DRLCommunityMapData p_data)
		{
			CommunityData = p_data;
		}

		public void Set(MapData p_data, DRLMap p_map = null)
		{
			GameFlag gameFlag = base.app.arguments.game.type;
			if (p_data == null)
			{
				return;
			}
			title0 = ((!p_map) ? "" : p_map.title.ToUpper().Replace("\n", " "));
			title1 = p_data.mapTitle.ToUpper();
			distance = 0f;
			collectableCount = 0;
			switch (gameFlag)
			{
			case GameFlag.Race:
				distance = p_data.mode.race.distance;
				laps = p_data.mode.race.lapCount;
				if (lapsContainer != null)
				{
					lapsContainer.SetActive(value: true);
				}
				if (lapsSpace != null)
				{
					lapsSpace.SetActive(value: true);
				}
				if (collectableContainer != null)
				{
					collectableContainer.SetActive(value: false);
				}
				if (collectableSpace != null)
				{
					collectableSpace.SetActive(value: false);
				}
				break;
			case GameFlag.Collectable:
				collectableCount = p_data.mode.collectable.collectableCount;
				if (lapsContainer != null)
				{
					lapsContainer.SetActive(value: false);
				}
				if (lapsSpace != null)
				{
					lapsSpace.SetActive(value: false);
				}
				if (collectableContainer != null)
				{
					collectableContainer.SetActive(value: true);
				}
				if (collectableSpace != null)
				{
					collectableSpace.SetActive(value: true);
				}
				break;
			case GameFlag.Freestyle:
				if (lapsContainer != null)
				{
					lapsContainer.SetActive(value: false);
				}
				if (lapsSpace != null)
				{
					lapsSpace.SetActive(value: false);
				}
				if (collectableContainer != null)
				{
					collectableContainer.SetActive(value: false);
				}
				if (collectableSpace != null)
				{
					collectableSpace.SetActive(value: false);
				}
				break;
			}
			difficulty = p_data.mapDifficulty;
			if (p_data.mode.typeFlag == GameFlag.Collectable && favoriteContainer != null)
			{
				favoriteContainer.gameObject.SetActive(value: false);
			}
			data = null;
			customData = p_data;
			if (favoriteGamepadHotkey != null)
			{
				favoriteGamepadHotkey.enabled = false;
			}
		}

		public void SetRating(float p_rating, float p_delay = 0f, float p_item_delay = 0.25f)
		{
			if ((bool)stars)
			{
				if (m_score_timer != null)
				{
					m_score_timer.Stop();
				}
				m_score_timer = RunOnce(delegate
				{
					float p_progress = p_rating * (float)stars.list.Count;
					stars.FadeProgress(p_progress, p_item_delay);
				}, p_delay);
			}
		}

		public void SetProgression(int p_xp_total)
		{
			if ((bool)progressionXPContainer)
			{
				progressionXPContainer.gameObject.SetActive(p_xp_total > 0);
				if (progressionXPSpace != null)
				{
					progressionXPSpace.gameObject.SetActive(p_xp_total > 0);
				}
			}
			if ((bool)progressionXPField)
			{
				progressionXPField.text = p_xp_total + "XP";
			}
		}

		public override void Build()
		{
			base.Build();
		}

		public override void OnFocus()
		{
			base.OnFocus();
			SetFavoriteFocus(p_focus: true);
		}

		public override void OnUnfocus()
		{
			base.OnUnfocus();
			SetFavoriteFocus(p_focus: false);
		}

		public void SetFavoriteToggleOn(bool p_on)
		{
			favoriteToggleView.isOn = p_on;
		}

		private void SetFavoriteFocus(bool p_focus)
		{
			if (!favoriteContainer.gameObject.activeSelf)
			{
				return;
			}
			favoriteGamepadHotkey.enabled = p_focus;
			if (p_focus)
			{
				favoriteHotkeyFade.FadeIn();
				heartIconFade.FadeIn();
				return;
			}
			favoriteHotkeyFade.FadeOut();
			if (favoriteToggleView.isOn)
			{
				heartIconFade.FadeIn();
			}
			else
			{
				heartIconFade.Fade(0.2f);
			}
		}

		public void SetFavoriteActive(bool p_active)
		{
			favoriteContainer.gameObject.SetActive(p_active);
			SetFavoriteFocus(p_focus: false);
		}
	}
}
