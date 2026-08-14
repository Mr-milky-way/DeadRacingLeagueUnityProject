using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIHUDCounterUAV : MonoBehaviour
	{
		public FadeComponent fade;

		public FadeComponent instructions;

		public Text uavSpeedText;

		public Text netHeightText;

		public Text netWidthText;

		public Text netShotsText;

		public Text cameraModeText;

		public Text nightVisionText;

		public Text simulationModeText;

		public Text gunAngleText;

		public Text pipCameraText;

		public GameObject osdTarget;

		public GameObject osdTargetYaw;

		public GameObject pipCameraImage;

		public void SetOSDTargetVisible(bool p_visible)
		{
			if (!(osdTarget == null) && !(osdTargetYaw == null))
			{
				osdTarget.SetActive(p_visible);
				osdTargetYaw.SetActive(p_visible);
			}
		}

		public void SetPIPOverlayVisible(bool p_visible)
		{
			pipCameraImage.SetActive(p_visible);
		}

		public void Refresh(float p_uavSpeed, Vector2 p_netSize, int p_netShots, string p_camMode, bool p_nightVision, bool p_gunMode, float p_gunAngle)
		{
			uavSpeedText.text = ((int)p_uavSpeed).ToString();
			netHeightText.text = p_netSize.y.ToString("#.##");
			netWidthText.text = p_netSize.x.ToString("#.##");
			cameraModeText.text = p_camMode;
			netShotsText.text = p_netShots.ToString();
			nightVisionText.text = (p_nightVision ? "ON" : "OFF");
			simulationModeText.text = (p_gunMode ? "GUN" : "SWEEP");
			gunAngleText.text = ((p_gunAngle > 180f) ? ((int)(360f - p_gunAngle)).ToString() : ((int)(0f - p_gunAngle)).ToString());
			pipCameraText.text = (pipCameraImage.activeInHierarchy ? "ON" : "OFF");
		}
	}
}
