using System;
using System.Collections;
using UnityEngine;
using drl.sim;

namespace thelab.core
{
	public class FNSkip : FlowNode
	{
		public enum SkipState
		{
			StartSkip = 0,
			EndSkip = 1
		}

		public const float IGNORE_TIME_WINDOW = 0.55f;

		public const float HOLD_TIME_TO_SKIP = 0.75f;

		public SkipState skipState;

		private float m_startTime;

		[HideInInspector]
		public float holdTime;

		private bool m_FNSkipStopAlreadyExecuted;

		private Activity mainActivity;

		private SimulationFlowModule m_module;

		internal override bool hasContent => true;

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

		public static event Action OnSkipStart;

		public static event Action OnSkipStop;

		internal override void OnInitialize()
		{
			base.OnInitialize();
			status = FlowStatus.Running;
			if (skipState != SkipState.StartSkip)
			{
				return;
			}
			this.TimerRunOnce(delegate
			{
				flow.skipHandler.Listen(flow);
				if ((bool)module)
				{
					module.ui.ShowSkip();
				}
			}, 0.5f);
		}

		internal override FlowStatus OnUpdate()
		{
			status = FlowStatus.Complete;
			FNSkip nc = flow.current as FNSkip;
			if (!nc)
			{
				return status;
			}
			if (!module)
			{
				return status;
			}
			if (skipState == SkipState.StartSkip)
			{
				mainActivity = Activity.Run((Func<bool>)delegate
				{
					if (!nc.flow.active)
					{
						return true;
					}
					if (nc.status == FlowStatus.Idle)
					{
						return true;
					}
					if (Input.anyKey)
					{
						nc.holdTime += Time.deltaTime;
						if (nc.holdTime >= 0.55f && (bool)module)
						{
							module.ui.UpdateSkip((nc.holdTime - 0.55f) / 0.75f);
						}
						if (nc.holdTime - 0.55f >= 0.75f && FNSkip.OnSkipStart != null)
						{
							OnSkipStop += FNSkip_OnSkipStop;
							if ((bool)module)
							{
								module.ui.HideSkip();
								module.ui.FadeIn(0f, 0.8f);
							}
							StartCoroutine(DoSkip(0.5f));
							return false;
						}
					}
					else
					{
						nc.holdTime = 0f;
						if ((bool)module && module.ui != null)
						{
							module.ui.UpdateSkip(0f);
						}
					}
					return true;
				}, 0f, false);
			}
			else
			{
				nc.holdTime = 0f;
				if ((bool)module)
				{
					module.ui.UpdateSkip(0f);
				}
				if (FNSkip.OnSkipStop != null)
				{
					FNSkip.OnSkipStop();
				}
				if ((bool)module)
				{
					module.ui.HideSkip();
				}
			}
			return status;
		}

		private void FNSkip_OnSkipStop()
		{
			OnSkipStop -= FNSkip_OnSkipStop;
			m_FNSkipStopAlreadyExecuted = true;
			if (module != null && module.ui != null)
			{
				module.ui.FadeOut(0f, 0.8f);
			}
		}

		private IEnumerator DoSkip(float delay)
		{
			float elapsed = 0f;
			while (elapsed < delay)
			{
				elapsed += Time.deltaTime;
				yield return null;
			}
			if (!m_FNSkipStopAlreadyExecuted)
			{
				OnSkipStop -= FNSkip_OnSkipStop;
				if (FNSkip.OnSkipStart != null)
				{
					FNSkip.OnSkipStart();
				}
				if ((bool)module)
				{
					module.ui.FadeOut(0f, 0.8f);
				}
			}
		}

		private void OnDestroy()
		{
			if (mainActivity != null)
			{
				mainActivity.Stop();
				mainActivity = null;
			}
			FNSkip.OnSkipStart = null;
			FNSkip.OnSkipStop = null;
			m_FNSkipStopAlreadyExecuted = true;
		}
	}
}
