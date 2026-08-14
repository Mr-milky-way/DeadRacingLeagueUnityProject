using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIHUDPhysicsView : View<DRLApp>
	{
		[Header("Containers")]
		public RectTransform containerGraphsLeft;

		public RectTransform containerGraphsRight;

		public Transform footer;

		public GameObject footerTooltip;

		public GameObject footerMessage;

		public GameObject tooltipXbox;

		public GameObject tooltipPS;

		public List<GameObject> xboxDashboardTooltips;

		public List<GameObject> psDashboardTooltips;

		public GameObject xboxToggleDashboard;

		public GameObject psToggleDashboard;

		[Header("Labels")]
		public Text labelSpeedGlobalX;

		public Text labelSpeedGlobalY;

		public Text labelSpeedGlobalZ;

		public Text labelSpeedLocalX;

		public Text labelSpeedLocalY;

		public Text labelSpeedLocalZ;

		public Text labelSpeedRotationX;

		public Text labelSpeedRotationY;

		public Text labelSpeedRotationZ;

		public Text labelSpeedAirspeed;

		public Text labelSpeedFlightMps;

		public Text labelSpeedFlightKmh;

		public Text labelSpeedTopspeedKmh;

		public Text labelSpeedGroundMps;

		public Text labelSpeedGroundKmh;

		public Text labelForceGlobalX;

		public Text labelForceGlobalY;

		public Text labelForceGlobalZ;

		public Text labelForceLocalX;

		public Text labelForceLocalY;

		public Text labelForceLocalZ;

		public Text labelForceDragX;

		public Text labelForceDragY;

		public Text labelForceDrag;

		public Text labelForceLift;

		public Text labelForceDragZ;

		public Text labelForceWindX;

		public Text labelForceWindY;

		public Text labelForceWindZ;

		public Text labelForceDamage;

		public Text labelForceLastEnergy;

		public Text labelElectricBatteryCharge;

		public Text labelElectricBatteryVoltage;

		public Text labelElectricTemperature;

		[Header("Graphs")]
		public UIGraph graphThrottle;

		public RectTransform graphPitchRoll;

		public UIGraph graphPitch;

		public UIGraph graphRoll;

		public UIGraph graphYaw;

		public UIGraph graphEfficiency;

		public UIDroneSchematic graphMotors;

		public RectTransform graphForce;

		public RectTransform graphSpeed;

		public RectTransform graphElectric;

		public Image psSelectIcon;

		public Image psBackIcon;

		public Sprite psButtonX;

		public Sprite psButtonO;

		private bool m_raceHudVisible;

		private bool m_raceStandingsVisible;

		private int m_standingsCount;

		public bool raceHudVisible
		{
			get
			{
				return m_raceHudVisible;
			}
			set
			{
				m_raceHudVisible = value;
				UpdateGraphsOffsets();
			}
		}

		public bool raceStandingsVisible
		{
			get
			{
				return m_raceStandingsVisible;
			}
			set
			{
				m_raceStandingsVisible = value;
				UpdateGraphsOffsets();
			}
		}

		public int raceStandingsCount
		{
			get
			{
				return m_standingsCount;
			}
			set
			{
				m_standingsCount = value;
				UpdateGraphsOffsets();
			}
		}

		public void ShowFooter(bool p_show)
		{
			RefreshNavigationTooltips();
			footer.gameObject.SetActive(p_show);
		}

		public void RefreshNavigationTooltips()
		{
			DefaultControllerType defaultControllerType = RCI.GetDefaultControllerType(DefaultControllerType.XBox);
			bool flag = defaultControllerType == DefaultControllerType.XBox && RCI.GetActiveJoystick() != null;
			bool flag2 = defaultControllerType == DefaultControllerType.PS && RCI.GetActiveJoystick() != null;
			bool isShowing = base.app.view.ui.game.hud.dashboard.isShowing;
			if (flag)
			{
				footerMessage.SetActive(value: false);
				tooltipXbox.SetActive(value: true);
				tooltipPS.SetActive(value: false);
				foreach (GameObject xboxDashboardTooltip in xboxDashboardTooltips)
				{
					xboxDashboardTooltip.SetActive(isShowing);
				}
				xboxToggleDashboard.SetActive(base.app.controller.game.race == null);
			}
			else if (flag2)
			{
				footerMessage.SetActive(value: false);
				tooltipXbox.SetActive(value: false);
				tooltipPS.SetActive(value: true);
				foreach (GameObject psDashboardTooltip in psDashboardTooltips)
				{
					psDashboardTooltip.SetActive(isShowing);
				}
				psToggleDashboard.SetActive(base.app.controller.game.race == null);
			}
			footerMessage.SetActive(!flag && !flag2);
		}

		public void Align()
		{
			footer.Find("message/message-edit-drone").GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		}

		public void UpdateGraphsOffsets()
		{
			float num = (raceHudVisible ? (-120f) : 0f);
			containerGraphsRight.anchoredPosition = new Vector2(0f, num);
			float y = ((m_standingsCount > 1) ? num : 0f);
			if (m_raceStandingsVisible && m_standingsCount > 0)
			{
				y = -165f - 40f * (float)m_standingsCount;
			}
			containerGraphsLeft.anchoredPosition = new Vector2(0f, y);
		}
	}
}
