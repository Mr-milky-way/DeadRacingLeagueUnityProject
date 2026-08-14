using System;
using System.Collections.Generic;
using UnityEngine;
using drl.game;
using drl.sim.rci;
using thelab.core;

namespace drl.sim
{
	public class FNDialog : FlowNode
	{
		[Serializable]
		public class Dialog
		{
			public enum Mode
			{
				None = 0,
				Show = 1,
				Hide = 2,
				TextProgression = 3,
				TextNPC = 4,
				NPCOverlay = 5,
				Texture = 6,
				TextureText = 7,
				HideClear = 8,
				FadeOutUI = 9,
				HideNPCOverlay = 10,
				ShowFooter = 11,
				ShowHeader = 12,
				HideFooter = 13,
				HideHeader = 14,
				SetStickIcon = 15,
				__Footer_ = 100,
				FooterLeftText = 101,
				FooterRightText = 102,
				FooterNPCState = 103,
				__Controller_ = 200,
				ControllerAnimate = 201,
				_Gauge_ = 300,
				SetRightGauge = 301,
				SetLeftGauge = 302,
				SetRightPercisionGauge = 303,
				SetLeftPercisionGauge = 304,
				HighlightGauge = 305
			}

			public Mode mode;

			public Texture texture;

			public string text = "";

			public float timeout;

			public KeyCode key;

			public FNController.StickIcon stickIcon = FNController.StickIcon.throttle;

			public FlowModuleUI.ElementType elementType = FlowModuleUI.ElementType.Dialog;

			public NPCStateType npcState;

			public UIControllerAnimationType controllerAnimation;

			public Gauge gauge;

			public float gaugeValue;

			public string[] gaugeLabels = new string[3];

			public bool gaugeLocked;
		}

		[SerializeField]
		private SimulationFlowModule m_module;

		public FlowModuleUI ui;

		[SerializeField]
		private List<Dialog> m_dialogs;

		public int current;

		public float elapsed;

		private ControllerTypeTag ct;

		private static DRLApp m_app;

		public SimulationFlowModule module
		{
			get
			{
				if (!m_module)
				{
					return Hierarchy.FindReverse<SimulationFlowModule>(base.transform);
				}
				return m_module;
			}
		}

		internal override bool hasContent => true;

		public List<Dialog> dialogs
		{
			get
			{
				if (m_dialogs != null)
				{
					return m_dialogs;
				}
				return m_dialogs = new List<Dialog>();
			}
		}

		public DRLApp app
		{
			get
			{
				if (!m_app)
				{
					return m_app = UnityEngine.Object.FindObjectOfType<DRLApp>();
				}
				return m_app;
			}
		}

		internal override void OnInitialize()
		{
			current = 0;
			DRLAppArguments dRLAppArguments = (app ? app.arguments : null);
			if ((bool)dRLAppArguments && (bool)dRLAppArguments.game.mission)
			{
				ct = dRLAppArguments.game.mission.GetComponent<ControllerTypeTag>();
			}
			Apply();
		}

		public void Set(int p_dialog)
		{
			elapsed = 0f;
			current = Mathf.Clamp(p_dialog, -1, dialogs.Count);
			if (current < dialogs.Count && current >= 0)
			{
				elapsed = 0f;
				Apply();
			}
		}

		public void Next()
		{
			elapsed = 0f;
			current = Mathf.Clamp(current + 1, -1, dialogs.Count);
			if (current < dialogs.Count)
			{
				Apply();
			}
		}

		public void Prev()
		{
			elapsed = 0f;
			current = Mathf.Clamp(current - 1, -1, dialogs.Count);
			if (current >= 0)
			{
				Apply();
			}
		}

		public void Apply()
		{
			FlowModuleUI flowModuleUI = (ui ? ui : (module ? module.ui : null));
			if (!flowModuleUI || dialogs.Count <= 0 || current >= dialogs.Count || current < 0)
			{
				return;
			}
			Dialog dialog = dialogs[current];
			if ((bool)ct && ct.Contains(ControllerStateType.Nikko) && dialog.npcState.ToString().EndsWith("0"))
			{
				dialog.npcState++;
			}
			switch (dialog.mode)
			{
			case Dialog.Mode.Show:
				flowModuleUI.Show(dialog.elementType);
				break;
			case Dialog.Mode.Hide:
				flowModuleUI.Hide(dialog.elementType);
				break;
			case Dialog.Mode.HideClear:
				flowModuleUI.Hide(FlowModuleUI.ElementType.Dialog);
				flowModuleUI.SetDialog("");
				break;
			case Dialog.Mode.TextProgression:
				flowModuleUI.SetDialog(dialog.text);
				break;
			case Dialog.Mode.TextNPC:
				flowModuleUI.SetDialogNPC(dialog.text);
				break;
			case Dialog.Mode.NPCOverlay:
				((DebugFlowModuleUI)flowModuleUI).SetTextNPCOverlay(dialog.text);
				((DebugFlowModuleUI)flowModuleUI).ShowNPCOverlay();
				break;
			case Dialog.Mode.Texture:
				flowModuleUI.SetDialog(dialog.texture);
				break;
			case Dialog.Mode.TextureText:
				flowModuleUI.SetDialog(dialog.texture, dialog.text);
				break;
			case Dialog.Mode.FadeOutUI:
				((DebugFlowModuleUI)flowModuleUI).FadeOut(float.Parse(dialog.text));
				break;
			case Dialog.Mode.HideNPCOverlay:
				((DebugFlowModuleUI)flowModuleUI).HideNPCOverlay();
				break;
			case Dialog.Mode.ShowFooter:
				flowModuleUI.ShowFooter();
				break;
			case Dialog.Mode.ShowHeader:
				flowModuleUI.ShowHeader(p_show: true);
				break;
			case Dialog.Mode.SetStickIcon:
				((DebugFlowModuleUI)flowModuleUI).ShowControllerIcon(dialog.stickIcon);
				break;
			case Dialog.Mode.HideFooter:
				flowModuleUI.HideFooter();
				break;
			case Dialog.Mode.HideHeader:
				flowModuleUI.ShowHeader(p_show: false);
				break;
			case Dialog.Mode.FooterLeftText:
				flowModuleUI.SetFooterLeftText(dialog.text);
				break;
			case Dialog.Mode.FooterRightText:
				flowModuleUI.SetFooterRightText(dialog.text);
				break;
			case Dialog.Mode.FooterNPCState:
				flowModuleUI.SetFooterNPCState(dialog.npcState);
				break;
			case Dialog.Mode.ControllerAnimate:
				if (dialog.controllerAnimation == UIControllerAnimationType.StopAll)
				{
					dialog.controllerAnimation = UIControllerAnimationType.UserInput;
				}
				flowModuleUI.SetControllerAnimation(dialog.controllerAnimation);
				break;
			case Dialog.Mode.SetRightGauge:
				if (dialog.gaugeLabels[2] != "" || dialog.gaugeLabels[1] != "" || dialog.gaugeLabels[0] != "")
				{
					flowModuleUI.SetGauge(1, dialog.gaugeLabels[2], dialog.gaugeLabels[1], dialog.gaugeLabels[0]);
				}
				flowModuleUI.SetGauge(1, dialog.gaugeValue != 0f);
				flowModuleUI.SetGauge(1, dialog.gaugeValue);
				flowModuleUI.SetGauge(dialog.gaugeLocked, 1, dialog.gaugeLocked ? 0.3f : 0f);
				break;
			case Dialog.Mode.SetLeftGauge:
				if (dialog.gaugeLabels[2] != "" || dialog.gaugeLabels[1] != "" || dialog.gaugeLabels[0] != "")
				{
					flowModuleUI.SetGauge(0, dialog.gaugeLabels[2], dialog.gaugeLabels[1], dialog.gaugeLabels[0]);
				}
				flowModuleUI.SetGauge(0, dialog.gaugeValue != 0f);
				flowModuleUI.SetGauge(0, dialog.gaugeValue);
				flowModuleUI.SetGauge(dialog.gaugeLocked, 0, dialog.gaugeLocked ? 0.3f : 0f);
				break;
			case Dialog.Mode.SetRightPercisionGauge:
				if (dialog.gaugeValue == 0f && dialog.gaugeLocked)
				{
					flowModuleUI.SetPrecisionGauge(dialog.gaugeLocked, 1, 0.2f);
					break;
				}
				if (dialog.gaugeLabels[2] != "" || dialog.gaugeLabels[1] != "" || dialog.gaugeLabels[0] != "")
				{
					flowModuleUI.SetPrecisionGauge(1, dialog.gaugeLabels[2], dialog.gaugeLabels[1], dialog.gaugeLabels[0], dialog.gaugeValue);
				}
				flowModuleUI.SetPrecisionGauge(dialog.gaugeLocked, 1, dialog.gaugeLocked ? 0.3f : 0f);
				flowModuleUI.SetPrecisionGauge(1, dialog.gaugeValue != 0f);
				flowModuleUI.SetPrecisionGauge(1, dialog.gaugeValue);
				break;
			case Dialog.Mode.SetLeftPercisionGauge:
				if (dialog.gaugeValue == 0f && dialog.gaugeLocked)
				{
					flowModuleUI.SetPrecisionGauge(dialog.gaugeLocked, 0, 0.2f);
					break;
				}
				if (dialog.gaugeLabels[2] != "" || dialog.gaugeLabels[1] != "" || dialog.gaugeLabels[0] != "")
				{
					flowModuleUI.SetPrecisionGauge(0, dialog.gaugeLabels[2], dialog.gaugeLabels[1], dialog.gaugeLabels[0], dialog.gaugeValue);
				}
				flowModuleUI.SetPrecisionGauge(dialog.gaugeLocked, 0, dialog.gaugeLocked ? 0.3f : 0f);
				flowModuleUI.SetPrecisionGauge(0, dialog.gaugeValue != 0f);
				flowModuleUI.SetPrecisionGauge(0, dialog.gaugeValue);
				break;
			case Dialog.Mode.HighlightGauge:
				switch (dialog.gauge)
				{
				case Gauge.LeftGauge:
					flowModuleUI.HighlightGauge(0);
					break;
				case Gauge.RightGauge:
					flowModuleUI.HighlightGauge(1);
					break;
				case Gauge.LeftPrecisionGauge:
					flowModuleUI.HighlightPrecisionGauge(0);
					break;
				case Gauge.RightPrecisionGauge:
					flowModuleUI.HighlightPrecisionGauge(1);
					break;
				}
				break;
			}
		}

		internal override FlowStatus OnUpdate()
		{
			FlowModuleUI flowModuleUI = (ui ? ui : (module ? module.ui : null));
			if (!flowModuleUI)
			{
				return FlowStatus.Complete;
			}
			if (dialogs.Count <= 0)
			{
				return FlowStatus.Complete;
			}
			if (current >= dialogs.Count)
			{
				flowModuleUI.HideButtonNext();
				return FlowStatus.Complete;
			}
			if (current < 0)
			{
				return FlowStatus.Complete;
			}
			Dialog dialog = dialogs[current];
			elapsed += Time.deltaTime;
			if (dialog.timeout > 0f)
			{
				float timeout = (float)dialog.text.Length * 0.05f + (float)dialog.text.Split('.').Length * 0.1f;
				if (dialog.timeout >= 1f)
				{
					dialog.timeout = timeout;
				}
				if (elapsed >= dialog.timeout)
				{
					elapsed = dialog.timeout;
					flowModuleUI.HideButtonNext();
					Next();
				}
			}
			if (dialog.timeout >= 1f)
			{
				flowModuleUI.ShowButtonNext();
			}
			bool flag = !RCI.HasNavigationController && RCI.GetAxisTrigger(RawAxis.RightStickX, isPositiveSign: true);
			if ((RCI.GetAnyButtonUp() || flag || Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Space) || Input.GetMouseButtonUp(0)) && (dialog.timeout <= 0f || dialog.timeout >= 1f))
			{
				Next();
				flowModuleUI.HideButtonNext();
				flowModuleUI.PlayButtonNextAudio();
			}
			if (dialog.timeout > 0f)
			{
				flowModuleUI.UpdateButtonNext(elapsed / dialog.timeout);
			}
			return FlowStatus.Running;
		}

		public override FlowStatus OnSkip()
		{
			FlowModuleUI flowModuleUI = (ui ? ui : (module ? module.ui : null));
			if ((bool)flowModuleUI)
			{
				foreach (Dialog dialog in dialogs)
				{
					if ((bool)ct && ct.Contains(ControllerStateType.Nikko) && dialog.npcState.ToString().EndsWith("0"))
					{
						dialog.npcState++;
					}
					switch (dialog.mode)
					{
					case Dialog.Mode.Show:
						flowModuleUI.Show(dialog.elementType);
						break;
					case Dialog.Mode.Hide:
						flowModuleUI.Hide(dialog.elementType);
						break;
					case Dialog.Mode.TextProgression:
						flowModuleUI.SetDialog(dialog.text);
						break;
					case Dialog.Mode.TextNPC:
						flowModuleUI.SetDialogNPC(dialog.text);
						break;
					case Dialog.Mode.Texture:
						flowModuleUI.SetDialog(dialog.texture);
						break;
					case Dialog.Mode.TextureText:
						flowModuleUI.SetDialog(dialog.texture, dialog.text);
						break;
					case Dialog.Mode.FooterLeftText:
						flowModuleUI.SetFooterLeftText(dialog.text);
						break;
					case Dialog.Mode.FooterRightText:
						flowModuleUI.SetFooterRightText(dialog.text);
						break;
					case Dialog.Mode.FooterNPCState:
						flowModuleUI.SetFooterNPCState(dialog.npcState);
						break;
					case Dialog.Mode.ControllerAnimate:
						if (dialog.controllerAnimation == UIControllerAnimationType.StopAll)
						{
							dialog.controllerAnimation = UIControllerAnimationType.UserInput;
						}
						flowModuleUI.SetControllerAnimation(dialog.controllerAnimation);
						break;
					}
				}
				flowModuleUI.HideButtonNext();
			}
			return FlowStatus.Complete;
		}
	}
}
