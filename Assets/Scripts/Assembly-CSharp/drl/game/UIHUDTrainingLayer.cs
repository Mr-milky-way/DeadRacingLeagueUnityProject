using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class UIHUDTrainingLayer : MonoBehaviour
	{
		public HorizontalLayoutGroup headerLayout;

		public LayoutGroup contentLayout;

		public LayoutGroup clock0Layout;

		public LayoutGroup clock0FieldLayout;

		public LayoutGroup clock1Layout;

		public LayoutGroup clock1FieldLayout;

		public GameObject clockContainer0;

		public ParticleSystem clockParticle0;

		public Text clockLabelField0;

		public Text clockMinField0;

		public Text clockSecField0;

		public Text clockMsField0;

		public GameObject clockContainer1;

		public ParticleSystem clockParticle1;

		public Text clockLabelField1;

		public Text clockMinField1;

		public Text clockSecField1;

		public Text clockMsField1;

		public GameObject stepContainer;

		public RawImage stepIconImage;

		public ParticleSystem stepParticle;

		public Text stepCountField;

		public Text stepTotalField;

		public GameObject stepSlashContainer;

		public UIHUDPrecisionGauge leftPrecisionGauge;

		public UIHUDGauge leftGauge;

		public UIHUDPrecisionGauge rightPrecisionGauge;

		public UIHUDGauge rightGauge;

		public UIHUDIndicator indicators;

		public Text footerLeftField;

		public Text footerRightField;

		public UIControllerOverlay controller;

		public UINPCOverlay npc;

		public FadeComponent fade;

		public FadeComponent bodyFade;

		public FadeComponent headerFade;

		public FadeComponent headerButtonNextFade;

		public FadeComponent footerFade;

		public FadeComponent footerLeftFade;

		public FadeComponent footerRightFade;

		public RectTransform footerLeftProgress;

		public ParticleSystem footerLeftProgressHilight;

		public RectTransform footerProgressStepBar;

		public RectTransform footerProgressBg;

		public Color footerBarSuccessColor = Color.green;

		public Color footerBarFailedColor = Color.red;

		private int footerProgressStepBarCount;

		private RectTransform[] footerProgressStepBarArray;

		public FadeComponent skipAvailableWarning;

		public FadeComponent dmvIntroSkipWarning;

		public FadeComponent content;

		public UIHUDObjective objectives;

		public UIHUDRequirements requirements;

		public UIHUDCounter counter;

		private bool hudAnimsInitialized;

		public float headerYOnScreen;

		public float headerYOffScreen = 140f;

		public float footerYOnScreen;

		public float footerYOffScreen = -1500f;

		public float npcYOnScreen;

		public float npcYOffScreen = -600f;

		public float controllerYOnScreen;

		public float controllerYOffScreen = -400f;

		private bool isHeaderOnScreen;

		private bool isFooterOnScreen;

		public UIHUDCounterUAV counterUAVHUD;

		[HideInInspector]
		public float stepTime0;

		[HideInInspector]
		public float stepTime1;

		public DRLApp app;

		private int m_c0_nms = -1;

		private int m_c0_ns;

		private int m_c0_nm;

		private string[] nsc0;

		private int m_c1_nms = -1;

		private int m_c1_ns;

		private int m_c1_nm;

		private string[] nsc1;

		public float headerY
		{
			get
			{
				return ((RectTransform)headerLayout.transform).anchoredPosition.y;
			}
			set
			{
				RectTransform obj = (RectTransform)headerLayout.transform;
				Vector2 anchoredPosition = obj.anchoredPosition;
				anchoredPosition.y = value;
				obj.anchoredPosition = anchoredPosition;
			}
		}

		public float footerY
		{
			get
			{
				return ((RectTransform)footerFade.transform).anchoredPosition.y;
			}
			set
			{
				RectTransform obj = (RectTransform)footerFade.transform;
				Vector2 anchoredPosition = obj.anchoredPosition;
				anchoredPosition.y = value;
				obj.anchoredPosition = anchoredPosition;
			}
		}

		public float npcY
		{
			get
			{
				return ((RectTransform)npc.transform).anchoredPosition.y;
			}
			set
			{
				RectTransform obj = (RectTransform)npc.transform;
				Vector2 anchoredPosition = obj.anchoredPosition;
				anchoredPosition.y = value;
				obj.anchoredPosition = anchoredPosition;
			}
		}

		public float controllerY
		{
			get
			{
				return ((RectTransform)controller.transform).anchoredPosition.y;
			}
			set
			{
				RectTransform obj = (RectTransform)controller.transform;
				Vector2 anchoredPosition = obj.anchoredPosition;
				anchoredPosition.y = value;
				obj.anchoredPosition = anchoredPosition;
			}
		}

		public float time0
		{
			get
			{
				return GetTimerValue(0);
			}
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
					clockMsField0.text = nsc0[num];
				}
				if (num2 != m_c0_ns)
				{
					m_c0_ns = num2;
					clockSecField0.text = nsc0[num2];
					clockSecField0.text = clockSecField0.text.Substring(1, 2);
				}
				if (num3 != m_c0_nm)
				{
					m_c0_nm = num3;
					clockMinField0.text = nsc0[num3];
					clockMinField0.text = clockMinField0.text.Substring(1, 2);
				}
			}
		}

		public string clockLabel0
		{
			set
			{
				clockLabelField0.text = value;
			}
		}

		public float time1
		{
			get
			{
				return GetTimerValue(1);
			}
			set
			{
				if (value <= 0f)
				{
					m_c1_nms = -1;
					m_c1_ns = -1;
					m_c1_nm = -1;
				}
				int num = Mathf.FloorToInt(value * 1000f) % 1000;
				int num2 = Mathf.FloorToInt(value) % 60;
				int num3 = Mathf.FloorToInt(value / 60f) % 60;
				if (nsc1 == null)
				{
					nsc1 = new string[1000];
					for (int i = 0; i < nsc1.Length; i++)
					{
						nsc1[i] = i.ToString("000");
					}
				}
				if (num != m_c1_nms)
				{
					m_c1_nms = num;
					clockMsField1.text = nsc1[num];
				}
				if (num2 != m_c1_ns)
				{
					m_c1_ns = num2;
					clockSecField1.text = nsc1[num2];
					clockSecField1.text = clockSecField1.text.Substring(1, 2);
				}
				if (num3 != m_c1_nm)
				{
					m_c1_nm = num3;
					clockMinField1.text = nsc1[num3];
					clockMinField1.text = clockMinField1.text.Substring(1, 2);
				}
			}
		}

		public string clockLabel1
		{
			set
			{
				clockLabelField1.text = value;
			}
		}

		private void Start()
		{
			headerLayout.enabled = false;
			contentLayout.enabled = false;
			clock0Layout.enabled = false;
			clock1Layout.enabled = false;
			clock0FieldLayout.enabled = false;
			clock1FieldLayout.enabled = false;
		}

		private void InitAnims()
		{
			headerYOnScreen = headerY;
			footerYOnScreen = footerY;
			npcYOnScreen = npcY;
			controllerYOnScreen = controllerY;
			hudAnimsInitialized = true;
		}

		private void EnableContentVerticalLayoutGroup(bool p_enable)
		{
			VerticalLayoutGroup component = content.GetComponent<VerticalLayoutGroup>();
			if ((bool)component)
			{
				component.enabled = p_enable;
			}
		}

		public void ShowHeader(bool p_show, float p_time = 0.7f)
		{
			if (!hudAnimsInitialized)
			{
				InitAnims();
			}
			if ((p_show && isHeaderOnScreen) || (!p_show && !isHeaderOnScreen))
			{
				return;
			}
			EnableContentVerticalLayoutGroup(p_enable: false);
			if (p_show)
			{
				Activity.RunOnce(delegate
				{
					EnableContentVerticalLayoutGroup(p_enable: true);
				}, p_time * 1.1f);
				if (requirements != null)
				{
					clockLabel0 = requirements.GetLocalisedTimerLabel1(app.model.storage.locale);
					clockLabel1 = requirements.GetLocalisedTimerLabel2(app.model.storage.locale);
					SetTimerVisible(0, requirements.timerRequired);
					SetTimerVisible(1, requirements.timer2Required && requirements.timer2VisibleAtStart);
					if (requirements.stepsCounterRequired)
					{
						SetStepsVisible(p_flag: true);
						SetStepIconImage(requirements.stepsIconImageTexture);
						SetStep(0, requirements.stepsCounterTotal);
					}
					SetProgressBar(0, requirements.progressBarTotal);
				}
				if (objectives.objectivesRequired)
				{
					objectives.gameObject.SetActive(value: true);
					objectives.ShowObjectives();
				}
				HeaderMoveIn(0f, p_time);
				isHeaderOnScreen = true;
			}
			else
			{
				HeaderMoveOut(0f, p_time);
				isHeaderOnScreen = false;
			}
		}

		public void ShowFooter(bool p_show, float p_time = 0.7f)
		{
			if (!hudAnimsInitialized)
			{
				InitAnims();
			}
			if ((p_show && isFooterOnScreen) || (!p_show && !isFooterOnScreen))
			{
				return;
			}
			EnableContentVerticalLayoutGroup(p_enable: false);
			if (p_show)
			{
				Activity.RunOnce(delegate
				{
					EnableContentVerticalLayoutGroup(p_enable: true);
				}, p_time * 1.1f);
				ShowContent();
				if ((bool)footerLeftFade)
				{
					footerLeftFade.Fade(1f, 0f, 0f, Cubic.Out);
					if (requirements.controllerVisibleAtStart)
					{
						controller.fade.Fade(1f, 0f, 0f, Cubic.Out);
					}
				}
				FooterMoveIn(0f, p_time);
				Activity.RunOnce(delegate
				{
					isFooterOnScreen = true;
				}, p_time);
			}
			else
			{
				FooterMoveOut(0f, p_time);
				Activity.RunOnce(delegate
				{
					isFooterOnScreen = false;
				}, p_time);
			}
		}

		private void HeaderMoveY(float p_y, float p_delay, float p_time = 0.7f)
		{
			Tween.Kill(this, "headerY");
			Tween.Add(this, "headerY", p_y, p_time, p_delay, Cubic.Out);
		}

		private void HeaderMoveIn(float p_delay, float p_time = 0.7f)
		{
			headerY = headerYOffScreen;
			HeaderMoveY(headerYOnScreen, p_delay, p_time);
		}

		private void HeaderMoveOut(float p_delay, float p_time = 0.7f)
		{
			headerY = headerYOnScreen;
			HeaderMoveY(headerYOffScreen, p_delay, p_time);
		}

		private void FooterMoveY(float p_y, float p_delay, float p_time = 0.7f)
		{
			Tween.Kill(this, "footerY");
			Tween.Add(this, "footerY", p_y, p_time, p_delay, Cubic.Out);
		}

		private void NpcMoveY(float p_y, float p_delay, float p_time = 0.7f)
		{
			Tween.Kill(this, "npcY");
			Tween.Add(this, "npcY", p_y, p_time, p_delay, Cubic.Out);
		}

		private void ControllerMoveY(float p_y, float p_delay, float p_time = 0.7f)
		{
			Tween.Kill(this, "controllerY");
			Tween.Add(this, "controllerY", p_y, p_time, p_delay, Cubic.Out);
		}

		private void FooterMoveIn(float p_delay, float p_time = 0.7f)
		{
			footerY = footerYOffScreen;
			FooterMoveY(footerYOnScreen, p_delay, p_time);
			npcY = npcYOffScreen;
			NpcMoveY(npcYOnScreen, p_delay, p_time * 1.1f);
			controllerY = controllerYOffScreen;
			ControllerMoveY(controllerYOnScreen, p_delay, p_time * 1.1f);
		}

		private void FooterMoveOut(float p_delay, float p_time = 0.7f)
		{
			footerY = footerYOnScreen;
			FooterMoveY(footerYOffScreen, p_delay, p_time);
			npcY = npcYOnScreen;
			NpcMoveY(npcYOffScreen, p_delay, p_time * 0.9f);
			controllerY = controllerYOnScreen;
			ControllerMoveY(controllerYOffScreen, p_delay, p_time * 0.9f);
		}

		public void SetLeftGauges(bool p_precision, bool p_gauge)
		{
			leftPrecisionGauge.gameObject.SetActive(p_precision);
			leftGauge.gameObject.SetActive(p_gauge);
		}

		public void SetRightGauges(bool p_precision, bool p_gauge)
		{
			rightPrecisionGauge.gameObject.SetActive(p_precision);
			rightGauge.gameObject.SetActive(p_gauge);
		}

		public void SetStepIconImage(Texture p_img)
		{
			if ((bool)stepIconImage && p_img != null)
			{
				stepIconImage.texture = p_img;
			}
			else
			{
				stepIconImage.gameObject.SetActive(value: false);
			}
		}

		public void SetStep(int p_step, int p_count)
		{
			if (p_step != -1)
			{
				SetStepsVisible(requirements.stepsCounterRequired);
			}
			if (p_count == -1 && (bool)requirements)
			{
				p_count = requirements.stepsCounterTotal;
				p_step = 0;
				ClearProgressOnAllBars();
			}
			stepCountField.text = p_step.ToString("00");
			stepTotalField.text = p_count.ToString("00");
			stepTotalField.gameObject.SetActive(value: true);
			stepSlashContainer.gameObject.SetActive(value: true);
		}

		public int GetSteps()
		{
			int result = 0;
			int.TryParse(stepCountField.text, out result);
			return result;
		}

		public void SetProgressBar(int p_step, int p_count)
		{
			if (p_count <= 0 && (bool)requirements)
			{
				if (requirements.progressBarTotal <= 0)
				{
					requirements.progressBarTotal = requirements.stepsCounterTotal;
				}
				p_count = requirements.progressBarTotal;
				p_step = 0;
				ClearProgressOnAllBars();
			}
			CreateProgressBarSteps(p_count);
			if (p_step > 0)
			{
				SetStepBarProgress(p_step - 1, 1f, p_success: true, p_animated: true);
			}
			else
			{
				ClearProgressOnAllBars();
			}
		}

		private void ClearProgressOnAllBars()
		{
			if (footerProgressStepBarArray != null)
			{
				for (int i = 0; i < footerProgressStepBarArray.Length; i++)
				{
					SetStepBarProgress(i, 0f);
				}
			}
		}

		private void SetStepBarProgress(int p_step, float progress, bool p_success = true, bool p_animated = false)
		{
			if (footerProgressStepBarArray != null && footerProgressStepBarArray.Length != 0 && p_step <= footerProgressStepBarArray.Length)
			{
				RectTransform component = footerProgressStepBarArray[p_step].GetChild(0).GetComponent<RectTransform>();
				component.GetComponent<Image>().color = (p_success ? footerBarSuccessColor : footerBarFailedColor);
				Vector3 localScale = component.localScale;
				localScale.x = Mathf.Clamp01(progress);
				if (p_animated)
				{
					Tween.Add(component, "localScale", localScale, 0.5f, Cubic.Out);
				}
				else
				{
					component.localScale = localScale;
				}
			}
		}

		private void CreateProgressBarSteps(int p_totalSteps)
		{
			if (p_totalSteps == footerProgressStepBarCount)
			{
				return;
			}
			if (footerProgressStepBarArray != null)
			{
				for (int i = 1; i < footerProgressStepBarArray.Length; i++)
				{
					if (footerProgressStepBarArray[i] != null)
					{
						Object.Destroy(footerProgressStepBarArray[i].gameObject);
						footerProgressStepBarArray[i] = null;
					}
				}
				footerProgressStepBarArray[0].gameObject.SetActive(value: false);
			}
			if (p_totalSteps >= 1)
			{
				float spacing = footerProgressBg.GetComponent<HorizontalLayoutGroup>().spacing;
				footerProgressStepBarArray = new RectTransform[p_totalSteps];
				float x = (footerProgressBg.rect.width - (float)(p_totalSteps - 2) * spacing) / (float)p_totalSteps;
				Vector2 sizeDelta = new Vector2(x, footerProgressBg.rect.height);
				footerProgressStepBar.gameObject.SetActive(value: true);
				footerProgressStepBarArray[0] = footerProgressStepBar;
				footerProgressStepBarArray[0].sizeDelta = sizeDelta;
				for (int j = 1; j < p_totalSteps; j++)
				{
					RectTransform rectTransform = Object.Instantiate(footerProgressStepBar, footerProgressStepBar.transform.parent);
					rectTransform.name = "bar-step" + j;
					rectTransform.sizeDelta = sizeDelta;
					footerProgressStepBarArray[j] = rectTransform;
				}
			}
			footerProgressStepBarCount = p_totalSteps;
		}

		public void SetCount(int p_step)
		{
			stepCountField.text = p_step.ToString("00");
			stepTotalField.gameObject.SetActive(value: false);
			stepSlashContainer.gameObject.SetActive(value: false);
		}

		public void HilightStepProgress()
		{
			if ((bool)footerLeftProgressHilight)
			{
				footerLeftProgressHilight.Play();
			}
		}

		public void SetTimerVisible(int p_position, bool p_flag)
		{
			switch (p_position)
			{
			case 0:
				if (clockContainer0.activeInHierarchy == p_flag)
				{
					return;
				}
				clockContainer0.SetActive(p_flag);
				break;
			case 1:
				if (clockContainer1.activeInHierarchy == p_flag)
				{
					return;
				}
				clockContainer1.SetActive(p_flag);
				break;
			}
			RefreshHeaderLayouts();
		}

		public float GetTimerValue(int p_position)
		{
			if (p_position == 0)
			{
				return float.Parse(clockMsField0.text) * 0.001f + float.Parse(clockSecField0.text) + float.Parse(clockMinField0.text) * 60f;
			}
			return float.Parse(clockMsField1.text) * 0.001f + float.Parse(clockSecField1.text) + float.Parse(clockMinField1.text) * 60f;
		}

		public void SetStepsVisible(bool p_flag)
		{
			stepContainer.SetActive(p_flag);
			RefreshHeaderLayouts();
		}

		private void RefreshHeaderLayouts()
		{
			headerLayout.enabled = true;
			contentLayout.enabled = true;
			clock0Layout.enabled = true;
			clock1Layout.enabled = true;
			clock0FieldLayout.enabled = true;
			clock1FieldLayout.enabled = true;
			this.TimerRunOnce(delegate
			{
				headerLayout.enabled = false;
				contentLayout.enabled = false;
				clock0Layout.enabled = false;
				clock1Layout.enabled = false;
				clock0FieldLayout.enabled = false;
				clock1FieldLayout.enabled = false;
			}, 3f);
		}

		public void ShowNPCOverlay(NPCStateType p_npc_state, string p_text)
		{
			content.FadeOut(0.3f);
			controller.fade.FadeOut(0.3f);
			Activity.RunOnce(delegate
			{
				content.gameObject.SetActive(value: false);
			}, 0.4f);
		}

		public void ShowContent()
		{
			content.gameObject.SetActive(value: true);
			content.FadeIn(0.3f);
			if (requirements.controllerVisibleAtStart)
			{
				controller.fade.FadeIn(0.3f);
			}
		}

		public void HilightTimer(int p_position)
		{
			switch (p_position)
			{
			case 0:
				if ((bool)clockParticle0)
				{
					clockParticle0.Play(withChildren: true);
				}
				break;
			case 1:
				if ((bool)clockParticle1)
				{
					clockParticle1.Play(withChildren: true);
				}
				break;
			}
		}

		public void HilightStep()
		{
			if ((bool)stepParticle)
			{
				stepParticle.Play(withChildren: true);
			}
		}

		public void ShowButtonNext()
		{
			if ((bool)headerButtonNextFade)
			{
				headerButtonNextFade.FadeIn(0.5f);
			}
		}

		public void UpdateButtonNext(float progress)
		{
			Slider componentInChildren = headerButtonNextFade.GetComponentInChildren<Slider>();
			if ((bool)componentInChildren)
			{
				componentInChildren.value = progress;
			}
		}

		public void HideButtonNext()
		{
			if ((bool)headerButtonNextFade)
			{
				headerButtonNextFade.FadeOut(0.5f);
			}
		}

		public void ShowDMVIntroSkip()
		{
			if (!(app.model.game == null) && !(app.arguments.game.quest == null) && app.arguments.game.quest.tags.Contains(GameFlag.DMVQuest) && (bool)dmvIntroSkipWarning)
			{
				dmvIntroSkipWarning.gameObject.SetActive(value: true);
				dmvIntroSkipWarning.FadeIn(0f);
				dmvIntroSkipWarning.Pulse();
			}
		}

		public void HideDMVIntroSkip()
		{
			if (!(app.model.game == null) && !(app.arguments.game.quest == null) && app.arguments.game.quest.tags.Contains(GameFlag.DMVQuest) && (bool)dmvIntroSkipWarning)
			{
				dmvIntroSkipWarning.FadeOut(0f);
				dmvIntroSkipWarning.gameObject.SetActive(value: false);
			}
		}

		public void ShowSkipWarning()
		{
			if ((bool)skipAvailableWarning)
			{
				skipAvailableWarning.gameObject.SetActive(value: true);
				skipAvailableWarning.FadeIn(0f);
				skipAvailableWarning.Pulse();
			}
		}

		public void UpdateSkipValue(float val)
		{
			if ((bool)skipAvailableWarning)
			{
				Slider componentInChildren = skipAvailableWarning.GetComponentInChildren<Slider>();
				if ((bool)componentInChildren)
				{
					componentInChildren.transform.GetChild(0).gameObject.SetActive(val > 0f);
					componentInChildren.value = val;
				}
			}
		}

		public void HideSkipWarning()
		{
			if ((bool)skipAvailableWarning)
			{
				skipAvailableWarning.FadeOut(0f);
				skipAvailableWarning.gameObject.SetActive(value: false);
			}
		}

		public void SetFooterLeftText(string p_text)
		{
			footerLeftField.text = p_text;
			bool flag = ((!(p_text == "")) ? true : false);
			if ((flag && !isFooterOnScreen) || (!flag && isFooterOnScreen))
			{
				ShowFooter(flag);
			}
			if (!isHeaderOnScreen)
			{
				ShowHeader(p_show: true);
			}
		}

		public void SetFooterNPCState(NPCStateType p_state)
		{
			npc.SetState(p_state);
		}

		public void ShowIndicator(Indicator p_indicator)
		{
			indicators.Show(p_indicator);
		}

		public void HideIndicator(Indicator p_indicator)
		{
			indicators.Hide(p_indicator);
		}

		public void ClearIndicators()
		{
			indicators.Clear();
		}

		public void Clear()
		{
			if ((bool)headerButtonNextFade)
			{
				headerButtonNextFade.FadeOut(0f);
			}
			SetTimerVisible(0, p_flag: false);
			SetTimerVisible(1, p_flag: false);
			SetLeftGauges(p_precision: false, p_gauge: false);
			SetRightGauges(p_precision: false, p_gauge: false);
			indicators.Clear();
			SetStepsVisible(p_flag: false);
			controller.SetAnimation(UIControllerAnimationType.StopAll);
			footerLeftField.text = "";
			footerRightField.text = "";
			npc.SetState(NPCStateType.Neutral0);
		}

		public void SaveStepTimes()
		{
			stepTime0 = time0;
			stepTime1 = time1;
		}

		public void ResetStepTimes()
		{
			time0 = stepTime0;
			time1 = stepTime1;
		}

		public void SetStepTimes(Vector2 p_times)
		{
			time0 = p_times.x;
			time1 = p_times.y;
		}

		public float GetStepTimes(int p_position)
		{
			if (p_position != 0)
			{
				return stepTime1;
			}
			return stepTime0;
		}

		public void ShowObjectives()
		{
			objectives.ShowObjectives();
			RefreshHeaderLayouts();
		}

		public void SetObjectives(string[] p_labels)
		{
			objectives.SetLabels(p_labels);
		}

		public void ClearObjectives()
		{
			objectives.ClearObjectives();
		}

		public void NextObjective()
		{
			int[] array = objectives.NextObjective();
			SetStep(array[0], array[1]);
		}

		public void ShowController()
		{
			controller.fade.FadeIn(0.3f);
		}

		public void HideController()
		{
			controller.fade.FadeOut(0.3f);
		}

		public void FadeInCounterLamp(int c)
		{
			if (counter.fade.alpha == 0f)
			{
				counter.fade.FadeIn();
				Activity.RunOnce(delegate
				{
					counter.FadeLamp(c, p_on: true);
				}, 0.4f);
				return;
			}
			counter.FadeLamp(c, p_on: true);
			if (c == 2 && counter.fade.alpha > 0f)
			{
				Activity.RunOnce(delegate
				{
					counter.fade.alpha = -0.1f;
				}, 0.4f);
			}
		}

		public void RefreshCounterUAVUI(float p_uavSpeed, Vector2 p_netSize, int p_netShots, string p_camMode, bool p_nightVision, bool p_gunMode, float p_gunAngle)
		{
			counterUAVHUD.Refresh(p_uavSpeed, p_netSize, p_netShots, p_camMode, p_nightVision, p_gunMode, p_gunAngle);
		}

		public void ToggleCUAVInstructions()
		{
			if (counterUAVHUD.instructions.alpha < 0.1f)
			{
				counterUAVHUD.instructions.FadeIn();
			}
			else
			{
				counterUAVHUD.instructions.FadeOut();
			}
		}
	}
}
