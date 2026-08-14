using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UICardButtonMap : UICardButtonLarge
	{
		public VideoPlayer video;

		public UIFlareProgressGroup stars;

		public Text trackGroupNameField;

		public Text trackNameField;

		public Text trackLeftInfoField;

		public Text trackRightInfoField;

		public RectTransform trackInfoContainer;

		public new DRLMap data;

		public DRLMapTrack track;

		public MapData customData;

		private MonoActivity m_score_timer;

		private AsyncRequest m_thumbnail_loader;

		public override UICardType type => UICardType.ButtonMap;

		public int difficulty
		{
			set
			{
				if ((bool)trackRightInfoField)
				{
					trackRightInfoField.enabled = true;
					switch (value)
					{
					case 0:
						trackRightInfoField.text = base.app.model.storage.locale.Get("map.map-track-cards.difficulty.basic", "BASIC");
						break;
					case 1:
						trackRightInfoField.text = base.app.model.storage.locale.Get("map.map-track-cards.difficulty.easy", "EASY");
						break;
					case 2:
						trackRightInfoField.text = base.app.model.storage.locale.Get("map.map-track-cards.difficulty.medium", "MEDIUM");
						break;
					case 3:
						trackRightInfoField.text = base.app.model.storage.locale.Get("map.map-track-cards.difficulty.hard", "HARD");
						break;
					default:
						trackRightInfoField.text = "";
						trackRightInfoField.enabled = false;
						break;
					}
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
				if ((bool)trackLeftInfoField)
				{
					trackLeftInfoField.text = base.app.model.storage.locale.Get("map.map-track-card.laps-count.label", "LAP LENGTH") + ": " + text2 + text;
				}
				if ((bool)trackInfoContainer)
				{
					trackInfoContainer.gameObject.SetActive(num > 0f);
				}
			}
		}

		public int collectableCount
		{
			set
			{
				if ((bool)trackLeftInfoField)
				{
					trackLeftInfoField.text = string.Format("{0} {1}", value, base.app.model.storage.locale.Get("map.map-track-card.collectable-count.label", "BALLOONS"));
				}
				if ((bool)trackInfoContainer)
				{
					trackInfoContainer.gameObject.SetActive((float)value > 0f);
				}
			}
		}

		public override void Build()
		{
			base.Build();
		}

		public void Clear()
		{
			data = null;
			base.label = "";
			base.image = null;
			base.preview = null;
		}

		public void Set(DRLMap p_map, bool p_showTitle = true)
		{
			if (!p_map)
			{
				Clear();
				return;
			}
			data = p_map;
			string text = p_map.title.ToUpper();
			if (trackGroupNameField != null)
			{
				text = text.Replace("\n", " ");
			}
			base.label = (p_showTitle ? text : "");
			base.image = p_map.background;
			base.preview = p_map.preview;
			if ((bool)video)
			{
				base.imageField.enabled = true;
				video.clip = p_map.video;
				RenderTexture targetTexture = ((p_map.image is RenderTexture) ? (p_map.image as RenderTexture) : null);
				video.targetTexture = targetTexture;
				base.image = p_map.image;
			}
			else
			{
				base.imageField.enabled = true;
				base.image = p_map.preview;
			}
		}

		public void SetTrack(DRLMapTrack p_track)
		{
			track = p_track;
			if ((bool)trackNameField)
			{
				trackNameField.gameObject.SetActive(track != null);
			}
			if ((bool)trackInfoContainer)
			{
				trackInfoContainer.gameObject.SetActive(track != null);
			}
			if ((bool)track)
			{
				trackNameField.text = track.title.ToUpper();
				distance = track.length;
				difficulty = track.difficulty;
			}
		}

		public void SetTrack(DRLCommunityMapData p_data, DRLMap p_baseMap)
		{
			if (p_data != null)
			{
				MapData mapData = new MapData();
				mapData.Load(p_data.ToJson());
				SetTrack(mapData, p_baseMap);
			}
		}

		public void SetTrack(MapData p_data, DRLMap p_baseMap)
		{
			customData = p_data;
			if ((bool)trackNameField)
			{
				trackNameField.gameObject.SetActive(!string.IsNullOrEmpty(customData.mapTitle));
			}
			if ((bool)trackInfoContainer)
			{
				trackInfoContainer.gameObject.SetActive(value: true);
			}
			base.label = customData.mapTitle.ToUpper();
			if ((bool)trackNameField)
			{
				trackNameField.text = (p_baseMap ? p_baseMap.label.ToUpper() : "");
			}
			base.image = null;
			base.preview = null;
			if ((bool)base.previewFade)
			{
				base.previewFade.FadeOut(0.01f);
			}
			if ((bool)base.imageFade)
			{
				base.imageFade.FadeOut(0.01f);
			}
			string value = customData.mapThumbURL;
			switch (p_data.mapCategoryFlag)
			{
			case GameFlag.MapDRL:
			case GameFlag.MapMultiGP:
			case GameFlag.MapSimple:
				value = "";
				break;
			}
			if ((bool)trackLeftInfoField)
			{
				trackLeftInfoField.text = "";
			}
			switch (p_data.mode.typeFlag)
			{
			case GameFlag.Race:
				distance = p_data.mode.race.distance;
				break;
			case GameFlag.Collectable:
				collectableCount = p_data.mode.collectable.collectableCount;
				break;
			}
			if (string.IsNullOrEmpty(value) || DRLApp.offline)
			{
				base.image = (p_baseMap ? p_baseMap.preview : null);
				base.preview = (p_baseMap ? p_baseMap.preview : null);
				if ((bool)base.previewFade)
				{
					base.previewFade.FadeIn();
				}
			}
			else
			{
				m_thumbnail_loader = Web.Get(customData.GetThumbURL(MapData.ThumbSize.Medium), delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
				{
					if (base.validContext && !(p_progress < 1f))
					{
						base.preview = p_result;
						base.image = p_result;
						if ((bool)base.previewFade)
						{
							base.previewFade.FadeIn();
						}
					}
				});
			}
			difficulty = customData.mapDifficulty;
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
	}
}
