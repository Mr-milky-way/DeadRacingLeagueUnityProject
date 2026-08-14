using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIMapEditorView : UIScreenView
	{
		[Header("Layers")]
		public MEInputLayer input;

		public MEOverlayLayer overlay;

		public MEControlsLayer controls;

		[Header("Header")]
		public FadeComponent undoFade;

		public FadeComponent redoFade;

		public List<DRLToggleExpandView> inputStateToggles;

		public List<DRLToggleExpandView> actionStateToggles;

		public FadeComponent actionStateFade;

		public List<DRLToggleExpandView> renderStateToggles;

		public List<FadeComponent> gameTestButtons;

		public Text gameTestReplayCountField;

		public FadeComponent gameTestSaveFade;

		public FadeComponent gameTestNeedRaceFade;

		public FadeComponent statsScenePerfOutlineWarning;

		public FadeComponent statsScenePerfBackgroundWarning;

		public TextMetric statsVertexCountField;

		public SwitcherComponent statsVertexCountStateSwitcher;

		public TextMetric statsObjectCountField;

		public int statsVertexCountLimit = 500000;

		public DRLToggleView pivotStateToggle;

		public SwitcherComponent pivotStateToggleSwitcher;

		public DRLToggleView physicsDropStateToggle;

		public FadeComponent physicsDropStateFade;

		[Header("Right Panel")]
		public DRLTabGroup tabGroupRight;

		[Header("Library Panel")]
		public DRLInputFieldView assetQueryField;

		public Text assetQueryCountField;

		public DRLStepperView category0Stepper;

		public SwitcherComponent category1Switcher;

		public MapAssetType[] category0Flags = new MapAssetType[3]
		{
			MapAssetType.Prop,
			MapAssetType.RaceProp,
			MapAssetType.Tool
		};

		public MapAssetType[][] category1Flags = new MapAssetType[4][]
		{
			new MapAssetType[6]
			{
				MapAssetType.None,
				MapAssetType.Misc,
				MapAssetType.Vehicles,
				MapAssetType.Nature,
				MapAssetType.Rocks,
				MapAssetType.Primitives
			},
			new MapAssetType[8]
			{
				MapAssetType.None,
				MapAssetType.DRL,
				MapAssetType.Inflatables,
				MapAssetType.Markers,
				MapAssetType.Missions,
				MapAssetType.MultiGP,
				MapAssetType.Regional,
				MapAssetType.Neon
			},
			new MapAssetType[1],
			new MapAssetType[1]
		};

		public DRLMapEditorLibraryView assetLibraryPanel;

		[Header("Properties Panel")]
		public MEInspectorPanelView inspector;

		[Header("Info")]
		public MEInfoLayer info;

		[Header("Metrics")]
		public DRLStepperView gizmoGridStepper;

		public DRLToggleView metricRulersToggle;

		public DRLToggleView metricSnapMoveToggle;

		public Image metricSnapMoveLockOutline;

		public Image metricSnapMoveLockBackground;

		public DRLNumberFieldView metricSnapMoveField;

		public DRLToggleView metricSnapRotateToggle;

		public Image metricSnapRotateLockOutline;

		public Image metricSnapRotateLockBackground;

		public DRLNumberFieldView metricSnapRotateField;

		public DRLToggleView metricModeToggle;

		[Header("Mode Race")]
		public TextMetric trackDistanceField;

		public FadeComponent trackDistanceFade;

		[Header("Mode Collectable")]
		public Text collectableCountField;

		public FadeComponent collectableCountFade;

		[Header("Misc")]
		public CanvasGroup content;

		private FadeComponent m_content_fade;

		public FadeComponent rightContentFade;

		private bool m_is_saving;

		public UIMapEditorController controller => AssertLocal<UIMapEditorController>("controller");

		public MapEditorView editor
		{
			get
			{
				if (!controller)
				{
					return null;
				}
				if (!controller.editor)
				{
					return null;
				}
				return controller.editor.view;
			}
		}

		public DRLStepperView category1Stepper => category1Switcher.GetCurrent<DRLStepperView>();

		public string assetQuery => assetQueryField.text;

		public int assetQueryCount
		{
			set
			{
				assetQueryCountField.text = value.ToString();
			}
		}

		public MapAssetType category0Flag
		{
			get
			{
				int index = category0Stepper.index;
				return category0Flags[index];
			}
		}

		public MapAssetType category1Flag
		{
			get
			{
				int index = category1Switcher.index;
				MapAssetType[] obj = category1Flags[index];
				index = category1Stepper.index;
				return obj[index];
			}
		}

		public FadeComponent contentFade
		{
			get
			{
				if (!m_content_fade)
				{
					return m_content_fade = content.GetComponent<FadeComponent>();
				}
				return m_content_fade;
			}
		}

		public bool contentInputEnabled
		{
			set
			{
				if (content.blocksRaycasts != value)
				{
					content.blocksRaycasts = value;
				}
				if (content.interactable != value)
				{
					content.interactable = value;
				}
				if (controls.inputEnabled != value)
				{
					controls.inputEnabled = value;
				}
			}
		}

		public MapEditorGridStateType gizmoGridState
		{
			get
			{
				return (MapEditorGridStateType)gizmoGridStepper.index;
			}
			set
			{
				gizmoGridStepper.index = (int)value;
				gizmoGridStepper.Refresh();
			}
		}

		public bool isMapSaving
		{
			get
			{
				return m_is_saving;
			}
			set
			{
				m_is_saving = value;
				gameTestSaveFade.Fade(value ? 1f : 0f);
			}
		}

		public void BlinkTestReplayWarning()
		{
			gameTestNeedRaceFade.Kill();
			gameTestNeedRaceFade.alpha = 1f;
			gameTestNeedRaceFade.Fade(0f, 1f, 2f);
			base.app.view.audio.PlayUIGenericError();
		}

		public void SetReplayCacheCount(int p_count)
		{
			gameTestReplayCountField.text = "(" + p_count + ")";
			gameTestButtons[3].Fade((p_count <= 0) ? 0.2f : 1f);
		}

		public void SetUndoEnabled(bool p_flag)
		{
			FadeComponent fadeComponent = undoFade;
			fadeComponent.Fade(p_flag ? 1f : 0.2f, 0.2f);
			fadeComponent.allowMouseInput = p_flag;
		}

		public void SetRedoEnabled(bool p_flag)
		{
			FadeComponent fadeComponent = redoFade;
			fadeComponent.Fade(p_flag ? 1f : 0.2f, 0.2f);
			fadeComponent.allowMouseInput = p_flag;
		}

		public void SetMapDistance(float p_distance)
		{
			float p_alpha = ((p_distance > 0f) ? 1f : 0.2f);
			trackDistanceField.value = p_distance;
			collectableCountFade.Fade(p_alpha, 0.2f, 0.05f, Cubic.Out);
		}

		public void SetCollectableCount(int p_count)
		{
			float p_alpha = ((p_count > 0) ? 1f : 0.2f);
			collectableCountField.text = p_count.ToString();
			collectableCountFade.Fade(p_alpha, 0.2f, 0.05f, Cubic.Out);
		}

		public void SetMapModeInfo(MapData p_data)
		{
			GameFlag typeFlag = p_data.mode.typeFlag;
			if ((uint)(typeFlag - 13) > 1u)
			{
				_ = 24;
				return;
			}
			trackDistanceField.gameObject.SetActive(value: true);
			SetMapDistance(p_data.mode.race.distance);
		}

		public void SetInputState(MEInputStateType p_type)
		{
			List<DRLToggleExpandView> list = inputStateToggles;
			int num = -1;
			switch (p_type)
			{
			case MEInputStateType.Action:
				num = 0;
				break;
			case MEInputStateType.Navigate:
				num = 1;
				break;
			case MEInputStateType.Orbit:
				num = 2;
				break;
			case MEInputStateType.Pan:
				num = 3;
				break;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if ((bool)list[i].toggle)
				{
					list[i].toggle.isOn = num == i;
				}
			}
		}

		public void SetPivotModeState(MEHandlePivotType p_type)
		{
			pivotStateToggle.SetState(p_type == MEHandlePivotType.Global);
			pivotStateToggleSwitcher.index = (int)p_type;
		}

		public void SetPhysicsDropState(bool p_flag)
		{
			physicsDropStateToggle.SetState(p_flag);
		}

		public void SetPhysicsDropEnabled(bool p_flag)
		{
			float p_alpha = (p_flag ? 1f : 0.2f);
			physicsDropStateFade.Fade(p_alpha, 0.2f);
		}

		public void SetRenderstate(MERenderStateType p_type)
		{
			List<DRLToggleExpandView> list = renderStateToggles;
			int num = -1;
			switch (p_type)
			{
			case MERenderStateType.Scene:
				num = 0;
				break;
			case MERenderStateType.Race:
				num = 1;
				break;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if ((bool)list[i].toggle)
				{
					list[i].toggle.interactable = true;
					list[i].toggle.isOn = num == i;
					Timer.Set(list[i].toggle, "interactable", 1f / 60f, !list[i].toggle.isOn);
				}
			}
			switch (p_type)
			{
			case MERenderStateType.Scene:
				rightContentFade.FadeIn(0.25f);
				break;
			case MERenderStateType.Race:
				rightContentFade.FadeOut(0.25f);
				break;
			}
		}

		public void SetActionState(MEActionStateType p_type)
		{
			List<DRLToggleExpandView> list = actionStateToggles;
			int num = -1;
			switch (p_type)
			{
			case MEActionStateType.None:
				num = -1;
				break;
			case MEActionStateType.Select:
				num = 0;
				break;
			case MEActionStateType.Move:
				num = 1;
				break;
			case MEActionStateType.Rotate:
				num = 2;
				break;
			case MEActionStateType.Scale:
				num = 3;
				break;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if ((bool)list[i].toggle)
				{
					list[i].toggle.isOn = num == i;
				}
			}
			float p_alpha = ((num < 0) ? 0.2f : 1f);
			actionStateFade.Fade(p_alpha, 0.2f);
		}

		public void SetScreenVisible(bool p_flag)
		{
			controls.fade.Fade(p_flag ? 1f : (-0.1f), 0.3f);
			contentFade.Fade(p_flag ? 1f : (-0.1f), 0.3f);
		}

		public bool IsScreenVisible()
		{
			return contentFade.alpha >= 1f;
		}

		public bool SwitchScreenVisible()
		{
			float num = contentFade.alpha;
			SetScreenVisible((num < 1f) ? true : false);
			return num >= 1f;
		}

		public void SetMetricSnapMoveEnabled(bool p_flag)
		{
			DRLToggleView dRLToggleView = metricSnapMoveToggle;
			DRLNumberFieldView dRLNumberFieldView = metricSnapMoveField;
			FadeComponent component = Hierarchy.GetComponent<FadeComponent>(dRLNumberFieldView.gameObject);
			dRLNumberFieldView.enabled = p_flag;
			dRLNumberFieldView.input.enabled = p_flag;
			dRLToggleView.toggle.isOn = p_flag;
			component.Fade(p_flag ? 1f : 0.2f, 0.25f);
		}

		public void SetMetricSnapMoveLock(bool p_flag)
		{
			Hierarchy.GetComponent<FadeComponent>(metricSnapMoveField.gameObject).allowMouseInput = !p_flag;
			metricSnapMoveLockBackground.enabled = p_flag;
			metricSnapMoveLockOutline.enabled = p_flag;
		}

		public void SetMetricSnapMove(float p_unit)
		{
			float value = metricSnapMoveField.value;
			if (Mathf.Abs(p_unit - value) > 0f)
			{
				metricSnapMoveField.value = p_unit;
			}
		}

		public void SetMetricSnapRotateEnabled(bool p_flag)
		{
			DRLToggleView dRLToggleView = metricSnapRotateToggle;
			DRLNumberFieldView dRLNumberFieldView = metricSnapRotateField;
			FadeComponent component = Hierarchy.GetComponent<FadeComponent>(dRLNumberFieldView.gameObject);
			dRLNumberFieldView.enabled = p_flag;
			dRLNumberFieldView.input.enabled = p_flag;
			dRLToggleView.toggle.isOn = p_flag;
			component.Fade(p_flag ? 1f : 0.2f, 0.25f);
		}

		public void SetMetricSnapRotateLock(bool p_flag)
		{
			Hierarchy.GetComponent<FadeComponent>(metricSnapRotateField.gameObject).allowMouseInput = !p_flag;
			metricSnapRotateLockBackground.enabled = p_flag;
			metricSnapRotateLockOutline.enabled = p_flag;
		}

		public void SetMetricSnapRotate(float p_unit)
		{
			float value = metricSnapRotateField.value;
			if (Mathf.Abs(p_unit - value) > 0f)
			{
				metricSnapRotateField.value = p_unit;
			}
		}

		public void SetRendererStats(int p_vertex_count, int p_object_count)
		{
			statsVertexCountField.value = p_vertex_count;
			statsObjectCountField.value = p_object_count;
			bool flag = p_vertex_count >= statsVertexCountLimit;
			statsVertexCountField.GetComponent<Text>().color = (flag ? Color.yellow : Color.white);
			statsVertexCountStateSwitcher.index = (flag ? 1 : 0);
			statsScenePerfOutlineWarning.pulse = flag;
			statsScenePerfBackgroundWarning.pulse = flag;
			if (!flag)
			{
				statsScenePerfOutlineWarning.FadeOut(0.5f);
			}
			if (!flag)
			{
				statsScenePerfBackgroundWarning.FadeOut(0.5f);
			}
		}
	}
}
