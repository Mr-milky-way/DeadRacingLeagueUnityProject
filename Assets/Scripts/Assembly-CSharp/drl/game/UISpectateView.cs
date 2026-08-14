using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISpectateView : UIScreenView
	{
		[Header("Panel")]
		public FadeSlideComponent panelFade;

		public CanvasGroup panelCanvasGroup;

		public FadeComponent contentFade;

		public UIElementView panelTriggerField;

		[Header("Help")]
		public ListComponent helpDataList;

		public GameObject helpDataListSpace;

		public GameObject helpButton;

		[Header("Playback")]
		public Image[] playStates;

		public DRLSliderView speedSlider;

		public Text speedSliderField;

		public DRLSliderView timeSlider;

		public Text timeSliderField;

		public GameObject playbackSpace;

		public List<Text> playbackTimeMarkers;

		public List<Image> playbackEventMarkers;

		public int playbackEventMarkerCount = 50;

		public float playbackEventAreaWidth = 264f;

		[Header("Cameras")]
		public FadeComponent[] cameraModeButtons;

		public ListComponent cameraToolButtonList;

		public GridLayoutGroup cameraToolButtonGrid;

		public GameObject cameraToolListSpace;

		public RectTransform cameraToolButtonsContainer;

		public DRLToggleView leaderFocusToggle;

		[Header("Course Camers")]
		public ListComponent cameraCourseButtonList;

		public GridLayoutGroup cameraCourseButtonGrid;

		public GameObject cameraCourseListSpace;

		public RectTransform cameraCourseButtonsContainer;

		[Header("Targets")]
		public ListComponent targetButtonList;

		public GridLayoutGroup targetButtonGrid;

		public GameObject targetButtonContainer;

		public GameObject targetButtonListSpace;

		public DRLStepperView targetStepper;

		public GameObject targetStepperSpace;

		[Header("Stats")]
		public FadeComponent[] trailModeButtons;

		public GameObject trailWidthContainer;

		public FadeComponent[] trailWidthModeButtons;

		public DRLToggleView nameToggle;

		public DRLToggleView raceStatsToggle;

		public DRLToggleView controllerToggle;

		[Header("Race")]
		public GameObject timeContainer;

		public Text timeMinField;

		public Text timeSecField;

		public Text timeMsField;

		public GameObject lapContainer;

		public Text lapTotalField;

		public Text lapCountField;

		public FadeComponent timeFade;

		public FadeComponent lapFade;

		[Header("Profile")]
		public GameObject userContainer;

		public Image userBackground;

		public RawImage userPhoto;

		public Text userNameField;

		public FadeComponent userFade;

		[Header("Controller")]
		public UIControllerOverlay controller;

		[Header("Video")]
		public GameObject videoCaptureWatermark;

		public GameObject videoCaptureSpace;

		public RectTransform videoCaptureButton;

		public RectTransform videoCaptureContent;

		public RawImage videoCaptureEncodeProgress;

		public float videoCaptureEncodeProgressWidth;

		public FadeComponent[] videoSizeMode;

		public FadeComponent[] videoAspectMode;

		public FadeComponent[] videoFPSMode;

		public FadeComponent[] videoQualityMode;

		public RectTransform[] videoProcessStates;

		public FadeComponent videoProcessFade;

		public DRLNumberFieldView videoRecordRangeStartField;

		public DRLNumberFieldView videoRecordRangeEndField;

		public DRLToggleView videoCaptureCropToggle;

		public DRLInputFieldView videoCaptureFolderField;

		public Text videoCaptureTempSpaceField;

		public Text maxPowerField;

		public GameObject maxPowerContainer;

		private bool m_tournamentContext;

		private bool m_isControlEnabled;

		private bool m_help_data_visible;

		private bool m_can_video_capture;

		private Activity m_encode_progress_animation;

		private bool m_race_stats_visible;

		private bool m_race_stats_allowed;

		private bool m_lap_count_allowed;

		private int m_c0_nms = -1;

		private int m_c0_ns;

		private int m_c0_nm;

		private string[] nsc0;

		public bool tournamentContext
		{
			get
			{
				if (!base.app.inTournament)
				{
					return m_tournamentContext;
				}
				return true;
			}
			set
			{
				m_tournamentContext = value;
			}
		}

		public bool isHelpDataVisible
		{
			get
			{
				return m_help_data_visible;
			}
			set
			{
				helpDataList.gameObject.SetActive(m_help_data_visible = value);
				helpDataListSpace.gameObject.SetActive(m_help_data_visible);
			}
		}

		public bool canVideoCapture
		{
			get
			{
				if (base.app.model.storage.state.player.profile.isDeveloper)
				{
					return m_can_video_capture;
				}
				return false;
			}
		}

		public float speed
		{
			get
			{
				return speedSlider.slider.value;
			}
			set
			{
				speedSlider.slider.enabled = false;
				speedSlider.slider.value = value;
				speedSlider.slider.enabled = true;
			}
		}

		public float time
		{
			get
			{
				return timeSlider.slider.value;
			}
			set
			{
				timeSlider.slider.enabled = false;
				timeSlider.slider.value = value;
				timeSlider.slider.enabled = true;
			}
		}

		public float duration
		{
			get
			{
				return timeSlider.slider.maxValue;
			}
			set
			{
				timeSlider.slider.maxValue = value;
				timeSlider.slider.minValue = 0f;
				RefreshTimeMarkers();
			}
		}

		public bool videoCaptureCropEnabled
		{
			get
			{
				return videoCaptureCropToggle.toggle.isOn;
			}
			set
			{
				videoCaptureCropToggle.toggle.isOn = value;
			}
		}

		public string videoCaptureFolderPath
		{
			get
			{
				return videoCaptureFolderField.text;
			}
			set
			{
				videoCaptureFolderField.text = value;
			}
		}

		public bool leaderFocusEnabled
		{
			get
			{
				if (leaderFocusToggle != null && leaderFocusToggle.toggle != null)
				{
					return leaderFocusToggle.toggle.isOn;
				}
				return false;
			}
			set
			{
				if (leaderFocusToggle != null && leaderFocusToggle.toggle != null)
				{
					leaderFocusToggle.toggle.isOn = value;
				}
			}
		}

		public int targetIndex
		{
			get
			{
				return targetStepper.index;
			}
			set
			{
				targetStepper.index = value;
				targetStepper.Refresh();
			}
		}

		public bool playerNameVisible
		{
			get
			{
				return nameToggle.toggle.isOn;
			}
			set
			{
				nameToggle.toggle.isOn = value;
			}
		}

		public bool raceStatsVisible
		{
			get
			{
				return m_race_stats_visible;
			}
			set
			{
				m_race_stats_visible = value;
				raceStatsToggle.toggle.isOn = m_race_stats_visible && m_race_stats_allowed;
			}
		}

		public bool raceStatsAllowed
		{
			get
			{
				return m_race_stats_allowed;
			}
			set
			{
				m_race_stats_allowed = value;
				raceStatsToggle.toggle.isOn = m_race_stats_visible && m_race_stats_allowed;
				raceStatsToggle.gameObject.SetActive(m_race_stats_allowed);
			}
		}

		public bool lapCountAllowed
		{
			get
			{
				return m_lap_count_allowed;
			}
			set
			{
				m_lap_count_allowed = value;
				lapContainer.SetActive(m_lap_count_allowed && m_race_stats_allowed);
			}
		}

		public bool controllerVisible
		{
			get
			{
				return controllerToggle.toggle.isOn;
			}
			set
			{
				controllerToggle.toggle.isOn = value;
			}
		}

		public float raceTime
		{
			set
			{
				if (value <= 0f)
				{
					m_c0_nms = -1;
					m_c0_ns = -1;
					m_c0_nm = -1;
				}
				int num = Mathf.FloorToInt(value * 1000f) % 1000;
				int num2 = Mathf.FloorToInt(value) % 60;
				int num3 = Mathf.FloorToInt(value / 60f) % 60;
				if (nsc0 == null)
				{
					nsc0 = new string[1000];
					for (int i = 0; i < nsc0.Length; i++)
					{
						nsc0[i] = i.ToString("000");
					}
				}
				if (num != m_c0_nms)
				{
					m_c0_nms = num;
					timeMsField.text = nsc0[num];
				}
				if (num2 != m_c0_ns)
				{
					m_c0_ns = num2;
					timeSecField.text = nsc0[num2];
					timeSecField.text = timeSecField.text.Substring(1, 2);
				}
				if (num3 != m_c0_nm)
				{
					m_c0_nm = num3;
					timeMinField.text = nsc0[num3];
					timeMinField.text = timeMinField.text.Substring(1, 2);
				}
			}
		}

		protected void Start()
		{
			RectTransform rectTransform = (RectTransform)videoCaptureEncodeProgress.rectTransform.parent;
			videoCaptureEncodeProgressWidth = rectTransform.sizeDelta.x;
			speedSlider.slider.onValueChanged.AddListener(RefreshSpeedField);
			timeSlider.slider.onValueChanged.AddListener(RefreshTimeField);
			DisableControls(0f, p_disable_visually: true);
		}

		public void SetHelpData(List<MEInfoHelpData> p_list, int p_max_count)
		{
			helpDataList.Clear();
			int num = Mathf.Min(p_list.Count, p_max_count);
			for (int i = 0; i < num; i++)
			{
				MEInfoHelpTagView mEInfoHelpTagView = helpDataList.Push<MEInfoHelpTagView>();
				mEInfoHelpTagView.Set(p_list[i]);
				mEInfoHelpTagView.backgroundImage.enabled = false;
				mEInfoHelpTagView.name = p_list[i].label;
				mEInfoHelpTagView.spaces[0].gameObject.SetActive(value: false);
				mEInfoHelpTagView.spaces[1].gameObject.SetActive(value: false);
				mEInfoHelpTagView.spaces[2].gameObject.SetActive(value: true);
			}
		}

		protected void RefreshTimeMarkers()
		{
			int count = playbackTimeMarkers.Count;
			float num = Mathf.Max(count - 1, 1);
			float num2 = duration / num;
			float num3 = 0f;
			for (int i = 0; i < count; i++)
			{
				Text text = playbackTimeMarkers[i];
				num3 = Mathf.Round(num2 * (float)i);
				text.text = Format.SecondsToTime(num3).Substring(1);
			}
		}

		public void ClearEvents()
		{
			if (playbackEventMarkerCount != playbackEventMarkers.Count)
			{
				for (int i = 1; i < playbackEventMarkers.Count; i++)
				{
					UnityEngine.Object.Destroy(playbackEventMarkers[i].transform.parent.gameObject);
				}
				GameObject gameObject = playbackEventMarkers[0].transform.parent.gameObject;
				Transform parent = gameObject.transform.parent;
				for (int j = 0; j < playbackEventMarkerCount - 1; j++)
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, parent);
					RectTransform rectTransform = (RectTransform)gameObject2.transform;
					Vector2 anchoredPosition = ((RectTransform)gameObject.transform).anchoredPosition;
					anchoredPosition.x = 0f;
					rectTransform.anchoredPosition = anchoredPosition;
					playbackEventMarkers.Add(gameObject2.GetComponentInChildren<Image>());
				}
				for (int k = 0; k < playbackEventMarkers.Count; k++)
				{
					playbackEventMarkers[k].transform.parent.name = $"{k}";
				}
			}
			for (int l = 0; l < playbackEventMarkers.Count; l++)
			{
				playbackEventMarkers[l].transform.parent.gameObject.SetActive(value: false);
			}
		}

		public void SetEvents(List<ReplayEvent> p_events)
		{
			ClearEvents();
			List<ReplayEvent> list = new List<ReplayEvent>(p_events);
			list.Sort(delegate(ReplayEvent a, ReplayEvent replayEvent2)
			{
				if (a.typeFlag == replayEvent2.typeFlag)
				{
					if (!(a.time < replayEvent2.time))
					{
						return 1;
					}
					return -1;
				}
				if (a.typeFlag == ReplayEventType.Lap)
				{
					return 1;
				}
				if (replayEvent2.typeFlag == ReplayEventType.Lap)
				{
					return -1;
				}
				if (a.typeFlag == ReplayEventType.Hit)
				{
					return 1;
				}
				if (replayEvent2.typeFlag == ReplayEventType.Hit)
				{
					return -1;
				}
				return (!(a.time < replayEvent2.time)) ? 1 : (-1);
			});
			float b = playbackEventAreaWidth;
			int num = Mathf.Min(list.Count, playbackEventMarkers.Count);
			int num2 = -1;
			for (int num3 = list.Count - 1; num3 >= 0; num3--)
			{
				if (list[num3].typeFlag == ReplayEventType.Gate)
				{
					num2 = num3;
					break;
				}
			}
			for (int num4 = 0; num4 < num; num4++)
			{
				ReplayEvent replayEvent = list[num4];
				if (replayEvent.typeFlag == ReplayEventType.Reset)
				{
					continue;
				}
				Image image = playbackEventMarkers[num4];
				RectTransform rectTransform = (RectTransform)image.transform.parent;
				Vector2 sizeDelta = image.rectTransform.sizeDelta;
				sizeDelta.x = 2f;
				switch (replayEvent.typeFlag)
				{
				case ReplayEventType.Gate:
				case ReplayEventType.Collect:
					image.color = DRLColor.green;
					if (num4 == num2)
					{
						image.color = Color.blue;
						sizeDelta.x = 3f;
					}
					break;
				case ReplayEventType.Lap:
					image.color = DRLColor.yellow;
					sizeDelta.x = 3f;
					break;
				case ReplayEventType.Crash:
					image.color = DRLColor.red;
					sizeDelta.x = 3f;
					break;
				case ReplayEventType.Hit:
					image.color = Colorf.RGBToColor(12255419u);
					break;
				}
				image.rectTransform.sizeDelta = sizeDelta;
				float t = Mathf.Clamp01(replayEvent.time / duration);
				float x = Mathf.Lerp(0f, b, t);
				Vector2 anchoredPosition = rectTransform.anchoredPosition;
				anchoredPosition.x = x;
				rectTransform.anchoredPosition = anchoredPosition;
				rectTransform.gameObject.SetActive(value: true);
			}
		}

		public void SetPlaybackPause(bool p_flag)
		{
			Image image = Reflection<object>.Get(playStates, 0);
			if ((bool)image)
			{
				image.enabled = !p_flag;
			}
			image = Reflection<object>.Get(playStates, 1);
			if ((bool)image)
			{
				image.enabled = p_flag;
			}
		}

		public void SetPlaybackEnabled(bool p_flag)
		{
			timeSlider.transform.parent.gameObject.SetActive(p_flag);
			speedSlider.transform.parent.gameObject.SetActive(p_flag);
			playbackSpace.SetActive(p_flag);
			m_can_video_capture = p_flag;
			bool active = canVideoCapture && p_flag;
			videoCaptureSpace.SetActive(active);
			videoCaptureButton.gameObject.SetActive(active);
			videoCaptureContent.gameObject.SetActive(value: false);
		}

		private void RefreshSpeedField(float v)
		{
			int num = Mathf.FloorToInt(v * 100f);
			string text = ((num <= 0) ? "" : "+");
			speedSliderField.text = text + num + "%";
		}

		private void RefreshTimeField(float v)
		{
			if (v <= 0f)
			{
				m_c0_nms = -1;
				m_c0_ns = -1;
				m_c0_nm = -1;
			}
			int num = Mathf.FloorToInt(v * 1000f) % 1000;
			int num2 = Mathf.FloorToInt(v) % 60;
			int num3 = Mathf.FloorToInt(v / 60f) % 60;
			if (nsc0 == null)
			{
				nsc0 = new string[1000];
				for (int i = 0; i < nsc0.Length; i++)
				{
					nsc0[i] = i.ToString("000");
				}
			}
			timeSliderField.text = $"{num3:00}:{num2:00}.{nsc0[num]}";
		}

		public void SetTempDiskSpace(ulong p_byte_count)
		{
			double a = (double)p_byte_count / 1024.0 / 1024.0 / 1024.0;
			a = Math.Ceiling(a);
			videoCaptureTempSpaceField.text = a.ToString("0") + " GB";
		}

		public void SetVideoRecordRange(float p_start, float p_end, float p_min, float p_max)
		{
			float num = Mathf.Clamp(p_start, p_min, p_max);
			float num2 = Mathf.Clamp(p_end, p_min, p_max);
			if (num2 < num)
			{
				num = num2;
			}
			if (num > num2)
			{
				num2 = num;
			}
			DRLNumberFieldView dRLNumberFieldView = videoRecordRangeStartField;
			dRLNumberFieldView.minValue = p_min;
			dRLNumberFieldView.maxValue = p_max;
			dRLNumberFieldView.value = num;
			DRLNumberFieldView dRLNumberFieldView2 = videoRecordRangeEndField;
			dRLNumberFieldView2.minValue = p_min;
			dRLNumberFieldView2.maxValue = p_max;
			dRLNumberFieldView2.value = num2;
		}

		public void SetVideoWatermark(bool p_flag)
		{
			videoCaptureWatermark.SetActive(p_flag);
		}

		public void SetVideoRecordEnabled(bool p_flag)
		{
			bool flag = canVideoCapture;
			videoCaptureSpace.SetActive(flag && !p_flag);
			videoCaptureButton.gameObject.SetActive(flag && !p_flag);
			videoCaptureContent.gameObject.SetActive(flag && p_flag);
		}

		public void SetVideoEncodingState(bool p_encoding)
		{
			videoProcessStates[0].gameObject.SetActive(!p_encoding);
			videoProcessStates[1].gameObject.SetActive(p_encoding);
			videoProcessFade.group.interactable = !p_encoding;
			videoProcessFade.group.blocksRaycasts = !p_encoding;
			if (!p_encoding)
			{
				SetVideoEncodingProgress(0f);
			}
		}

		public void SetVideoEncodingProgress(float p_progress)
		{
			if (!videoCaptureEncodeProgress)
			{
				return;
			}
			RectTransform obj = (RectTransform)videoCaptureEncodeProgress.transform.parent;
			Vector2 sizeDelta = obj.sizeDelta;
			sizeDelta.x = Mathf.Clamp01(p_progress) * videoCaptureEncodeProgressWidth;
			obj.sizeDelta = sizeDelta;
			videoCaptureEncodeProgress.transform.parent.gameObject.SetActive(p_progress > 0f);
			if (p_progress <= 0f)
			{
				if (m_encode_progress_animation != null)
				{
					m_encode_progress_animation.Stop();
				}
				m_encode_progress_animation = null;
				return;
			}
			m_encode_progress_animation = Activity.Run((Func<bool>)delegate
			{
				if (!base.validContext)
				{
					return false;
				}
				RawImage rawImage = videoCaptureEncodeProgress;
				Rect uvRect = rawImage.uvRect;
				uvRect.x -= uvRect.width * Time.unscaledDeltaTime * 0.05f;
				rawImage.uvRect = uvRect;
				return true;
			}, 0f, false);
		}

		public void SetVideoSizeMode(UISpectateVideoFlags p_mode)
		{
			int num = -1;
			switch (p_mode)
			{
			case UISpectateVideoFlags.Size2160:
				num = 0;
				break;
			case UISpectateVideoFlags.Size1080:
				num = 1;
				break;
			case UISpectateVideoFlags.Size720:
				num = 2;
				break;
			case UISpectateVideoFlags.Size540:
				num = 3;
				break;
			case UISpectateVideoFlags.Size480:
				num = 4;
				break;
			case UISpectateVideoFlags.Size240:
				num = 5;
				break;
			}
			for (int i = 0; i < videoSizeMode.Length; i++)
			{
				videoSizeMode[i].Fade((num == i) ? 1f : 0.2f);
			}
		}

		public void SetVideoAspectMode(UISpectateVideoFlags p_mode)
		{
			int num = -1;
			switch (p_mode)
			{
			case UISpectateVideoFlags.AspectWH:
				num = 0;
				break;
			case UISpectateVideoFlags.AspectHW:
				num = 1;
				break;
			case UISpectateVideoFlags.Aspect21_9:
				num = 2;
				break;
			case UISpectateVideoFlags.Aspect16_10:
				num = 3;
				break;
			case UISpectateVideoFlags.Aspect16_9:
				num = 4;
				break;
			case UISpectateVideoFlags.Aspect4_3:
				num = 5;
				break;
			case UISpectateVideoFlags.Aspect1_1:
				num = 6;
				break;
			}
			for (int i = 0; i < videoAspectMode.Length; i++)
			{
				videoAspectMode[i].Fade((num == i) ? 1f : 0.2f);
			}
		}

		public void SetVideoFPSMode(UISpectateVideoFlags p_mode)
		{
			int num = -1;
			switch (p_mode)
			{
			case UISpectateVideoFlags.FPS240:
				num = 0;
				break;
			case UISpectateVideoFlags.FPS120:
				num = 1;
				break;
			case UISpectateVideoFlags.FPS60:
				num = 2;
				break;
			case UISpectateVideoFlags.FPS30:
				num = 3;
				break;
			case UISpectateVideoFlags.FPS24:
				num = 4;
				break;
			}
			for (int i = 0; i < videoFPSMode.Length; i++)
			{
				videoFPSMode[i].Fade((num == i) ? 1f : 0.2f);
			}
		}

		public void SetVideoQualityMode(UISpectateVideoFlags p_mode)
		{
			int num = (int)(p_mode - 50);
			if (p_mode == UISpectateVideoFlags.QualityMax)
			{
				num = videoQualityMode.Length - 1;
			}
			for (int i = 0; i < videoQualityMode.Length; i++)
			{
				videoQualityMode[i].Fade((num == i) ? 1f : 0.2f);
			}
		}

		public void InitCameraToolList()
		{
			SetCameraTools(20, p_has_hints: false);
			SetCameraTools(0, p_has_hints: false);
			SetCourseCameras(20);
			SetCourseCameras(0);
		}

		public void SetCamerasList(string p_item_id, GridLayoutGroup p_grid, ListComponent p_list, RectTransform p_container, GameObject p_space, Component p_up, Component p_down, int p_count, bool p_has_hints)
		{
			p_space.SetActive(p_count > 0);
			p_container.gameObject.SetActive(p_count > 0);
			int num = ((p_count > 0) ? ((p_count > 10) ? 20 : 10) : 0);
			num = 20;
			p_list.Clear();
			for (int i = 0; i < num; i++)
			{
				bool flag = i < p_count;
				UISpectateCTButton uISpectateCTButton = p_list.Push<UISpectateCTButton>();
				uISpectateCTButton.name = p_item_id;
				uISpectateCTButton.index = i;
				uISpectateCTButton.SetLabel((i + 1).ToString());
				uISpectateCTButton.SetEnabled(flag);
				uISpectateCTButton.SetActive(flag);
				uISpectateCTButton.SetHintList(flag && p_has_hints);
				uISpectateCTButton.SetHint(p_flag: false);
			}
			this.TimerRunOnce(delegate
			{
				UINavigation.Link(p_grid, null, null, p_up, p_down, allow_disabled: true);
			}, 0.75f);
		}

		public void SetCourseCameras(int p_count)
		{
			SetCamerasList("course-camera-item", cameraCourseButtonGrid, cameraCourseButtonList, cameraCourseButtonsContainer, cameraCourseListSpace, null, targetStepper, p_count, p_has_hints: false);
			for (int i = 0; i < cameraCourseButtonList.Count; i++)
			{
				cameraCourseButtonList.Get<UISpectateCTButton>(i).outlineActiveField.color = DRLColor.yellow;
			}
		}

		public void SetCameraTools(int p_count, bool p_has_hints)
		{
			cameraModeButtons[3].gameObject.SetActive(p_count > 0);
			cameraModeButtons[4].gameObject.SetActive(p_count > 0);
			SetCamerasList("camera-tool-item", cameraToolButtonGrid, cameraToolButtonList, cameraToolButtonsContainer, cameraToolListSpace, cameraModeButtons[0], targetStepper, p_count, p_has_hints);
		}

		public void SetFocusLeaderEnabled(bool p_flag)
		{
			leaderFocusToggle.transform.parent.gameObject.SetActive(p_flag);
		}

		public void SetCameraMode(SpectateCameraModeType p_mode)
		{
			for (int i = 0; i < cameraModeButtons.Length; i++)
			{
				cameraModeButtons[i].Fade((p_mode == (SpectateCameraModeType)i) ? 1f : 0.2f);
			}
		}

		public void SetCameraToolActive(int p_index, bool p_flag)
		{
			if (p_index >= 0 && p_index < cameraToolButtonList.Count)
			{
				UISpectateCTButton uISpectateCTButton = cameraToolButtonList.Get<UISpectateCTButton>(p_index);
				if (uISpectateCTButton.IsEnabled())
				{
					uISpectateCTButton.SetActive(p_flag);
				}
			}
		}

		public void ClearCameraToolActive()
		{
			int count = cameraToolButtonList.Count;
			for (int i = 0; i < count; i++)
			{
				SetCameraToolActive(i, p_flag: false);
			}
		}

		public void ClearCameraToolHints()
		{
			int count = cameraToolButtonList.Count;
			for (int i = 0; i < count; i++)
			{
				cameraToolButtonList.Get<UISpectateCTButton>(i).SetHint(p_flag: false);
			}
		}

		public void SetCameraToolHint(int p_index, int p_hint, bool p_flag, Color p_color)
		{
			if (p_index >= 0 && p_index < cameraToolButtonList.Count)
			{
				UISpectateCTButton uISpectateCTButton = cameraToolButtonList.Get<UISpectateCTButton>(p_index);
				if (uISpectateCTButton.IsEnabled())
				{
					uISpectateCTButton.SetHint(p_flag, p_hint, p_color);
				}
			}
		}

		public void SetCameraToolFocus(int p_index, bool p_flag)
		{
			if (p_index >= 0 && p_index < cameraToolButtonList.Count)
			{
				UISpectateCTButton uISpectateCTButton = cameraToolButtonList.Get<UISpectateCTButton>(p_index);
				if (uISpectateCTButton.IsEnabled())
				{
					uISpectateCTButton.SetFocus(p_flag);
				}
			}
		}

		public void ClearCameraToolFocus()
		{
			int count = cameraToolButtonList.Count;
			for (int i = 0; i < count; i++)
			{
				SetCameraToolFocus(i, p_flag: false);
			}
		}

		public void SetCourseCameraActive(int p_index, bool p_flag)
		{
			if (p_index >= 0 && p_index < cameraCourseButtonList.Count)
			{
				UISpectateCTButton uISpectateCTButton = cameraCourseButtonList.Get<UISpectateCTButton>(p_index);
				if (uISpectateCTButton.IsEnabled())
				{
					uISpectateCTButton.SetActive(p_flag);
				}
			}
		}

		public void ClearCourseCameraActive()
		{
			int count = cameraCourseButtonList.Count;
			for (int i = 0; i < count; i++)
			{
				SetCourseCameraActive(i, p_flag: false);
			}
		}

		public void SetTargets(List<string> p_targets, bool p_full_size)
		{
			List<string> list = new List<string>(p_targets);
			targetButtonListSpace.SetActive(list.Count > 0);
			targetButtonContainer.gameObject.SetActive(list.Count > 0);
			targetStepper.gameObject.SetActive(list.Count > 0);
			targetStepperSpace.SetActive(list.Count > 0);
			targetButtonList.Clear();
			p_full_size = true;
			int num = ((list.Count > 0) ? (p_full_size ? 12 : 6) : 0);
			targetButtonGrid.cellSize = new Vector2(p_full_size ? 28.3f : 32f, targetButtonGrid.cellSize.y);
			for (int i = 0; i < num; i++)
			{
				bool flag = i < list.Count;
				UISpectateTargetButton uISpectateTargetButton = targetButtonList.Push<UISpectateTargetButton>();
				uISpectateTargetButton.index = i;
				uISpectateTargetButton.SetEnabled(flag);
				uISpectateTargetButton.SetLabel((i + 1).ToString());
				uISpectateTargetButton.name = "spectate-target-item";
			}
			List<UISpectateTargetButton> list2 = targetButtonList.GetList<UISpectateTargetButton>();
			for (int j = 0; j < list2.Count; j++)
			{
				if (j < list.Count - 1)
				{
					list2[j].navigation.right = list2[j + 1].navigation;
				}
				if (j > 0)
				{
					list2[j].navigation.left = list2[j - 1].navigation;
				}
				list2[j].navigation.up = targetStepper;
				list2[j].navigation.down = trailModeButtons[0];
			}
			AssertTargetStepper(list);
		}

		public void AssertTargetStepper(List<string> p_names)
		{
			List<string> list = new List<string>(p_names);
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = list[i].ToUpper();
				if (list[i].Length > 14)
				{
					list[i] = list[i].Substring(0, 14) + "...";
				}
			}
			targetStepper.labels = ((list.Count > 0) ? list.ToArray() : new string[1] { "NO PLAYERS" });
			targetStepper.min = 0;
			targetStepper.max = list.Count - 1;
			targetStepper.index = Mathf.Clamp(targetStepper.index, targetStepper.min, targetStepper.max);
			targetStepper.Refresh();
			Debug.Log(string.Format("UISpectateView> AssertTargetStepper / index[{0}] and {1} Names\n{2}", targetStepper.index, list.Count, string.Join("\n", list)));
		}

		public void SetTargetFocus(int p_index, bool p_flag)
		{
			UISpectateTargetButton uISpectateTargetButton = targetButtonList.Get<UISpectateTargetButton>(p_index);
			if (!(uISpectateTargetButton == null))
			{
				uISpectateTargetButton.SetFocus(p_flag);
			}
		}

		public void TargetBlink(int p_index, float p_duration = 0.5f, float p_delay = 0f)
		{
			if (p_index >= 0 && !(targetButtonList == null) && targetButtonList.Count != 0 && p_index < targetButtonList.Count)
			{
				UISpectateTargetButton uISpectateTargetButton = targetButtonList.Get<UISpectateTargetButton>(p_index);
				if (!(uISpectateTargetButton == null))
				{
					uISpectateTargetButton.Blink(p_duration, p_delay);
				}
			}
		}

		public void ClearTargetFocus()
		{
			int count = targetButtonList.Count;
			for (int i = 0; i < count; i++)
			{
				SetTargetFocus(i, p_flag: false);
			}
		}

		public void EnableControls(bool p_focus, bool p_disable_visually = false)
		{
			m_isControlEnabled = true;
			panelFade.FadeIn(0.2f);
			base.app.view.ui.navigation.enabled = true;
			if (p_focus)
			{
				UINavigation.Focus(targetStepper);
			}
			if (p_disable_visually)
			{
				panelCanvasGroup.alpha = 0f;
				panelCanvasGroup.blocksRaycasts = false;
			}
			else
			{
				panelCanvasGroup.alpha = 1f;
				panelCanvasGroup.blocksRaycasts = true;
			}
		}

		public void DisableControls(float p_duration = 0f, bool p_disable_visually = false)
		{
			m_isControlEnabled = false;
			base.app.view.ui.navigation.enabled = false;
			if (p_duration <= 0f)
			{
				panelFade.transition = 1f;
				this.TimerRunOnce(delegate
				{
					UINavigation.Focus(cameraModeButtons[0]);
				}, 0.6f);
			}
			else
			{
				panelFade.FadeOut(p_duration);
			}
			if (p_disable_visually)
			{
				panelCanvasGroup.alpha = 0f;
				panelCanvasGroup.blocksRaycasts = false;
			}
			else
			{
				panelCanvasGroup.alpha = 1f;
				panelCanvasGroup.blocksRaycasts = true;
			}
		}

		public bool IsControlsEnabled()
		{
			if (!m_isControlEnabled)
			{
				return panelFade.transition <= 0f;
			}
			return true;
		}

		public void SetLapCount(int p_count, int p_total)
		{
			lapCountField.text = p_count.ToString("00");
			lapTotalField.text = p_total.ToString("00");
		}

		public void SetLapCountEnabled(bool p_flag)
		{
			if ((bool)lapContainer)
			{
				lapContainer.SetActive(p_flag && m_lap_count_allowed && m_race_stats_allowed);
			}
		}

		public void SetRaceStatsVisible(bool p_flag)
		{
			if ((bool)timeContainer)
			{
				timeContainer.SetActive(p_flag);
			}
			SetLapCountEnabled(p_flag);
			base.app.view.ui.game.hud.damage.Show(p_flag);
		}

		public void SetUser(string p_name, Texture2D p_photo, Color p_color)
		{
			userNameField.text = p_name;
			userBackground.color = p_color;
			userPhoto.enabled = p_photo != null;
			userPhoto.texture = p_photo;
		}

		public void SetUser(GamePlayerData p_data)
		{
			if (p_data != null)
			{
				p_data.RefreshPlayerPhoto(delegate
				{
					userPhoto.texture = p_data.photo;
				});
				SetUser(p_data.name.ToUpper(), p_data.photo, p_data.color);
			}
		}

		public void SetUserVisible(bool p_flag)
		{
			if ((bool)userContainer)
			{
				userContainer.SetActive(p_flag);
			}
		}

		public void SetDroneTrailMode(SpectateDroneTrailModeType p_mode)
		{
			for (int i = 0; i < cameraModeButtons.Length; i++)
			{
				trailModeButtons[i].Fade((p_mode == (SpectateDroneTrailModeType)i) ? 1f : 0.2f);
			}
		}

		public void SetDroneTrailWidthMode(SpectateDroneTrailWidthModeType p_mode)
		{
			for (int i = 0; i < cameraModeButtons.Length; i++)
			{
				trailWidthModeButtons[i].Fade((p_mode == (SpectateDroneTrailWidthModeType)i) ? 1f : 0.2f);
			}
		}

		public void SetControllerType(ControllerStateType p_controller)
		{
			controller.SetController(p_controller);
		}

		public void ToggleUserInfo()
		{
			bool p_flag = lapFade.alpha <= 0.2f;
			ShowUerInfo(p_flag);
		}

		public void ToggleUsername()
		{
			bool flag = userFade.alpha <= 0.2f;
			userFade.Fade(flag ? 1 : 0, 0f);
			playerNameVisible = flag;
		}

		public void ShowUerInfo(bool p_flag)
		{
			userFade.Fade(p_flag ? 1 : 0);
			lapFade.Fade(p_flag ? 1 : 0);
			if (p_flag)
			{
				base.app.view.ui.game.hud.Show();
			}
			else
			{
				base.app.view.ui.game.hud.Hide();
			}
		}
	}
}
