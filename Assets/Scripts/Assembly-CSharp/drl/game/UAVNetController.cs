using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UAVNetController : Controller<DRLApp>
	{
		public enum Mode
		{
			net = 0,
			gun = 1
		}

		public GameObject netPrefab;

		public GameObject netGunPrefab;

		public GameObject gunAnchor;

		private Drone drone;

		public SimulationFlowModule sim;

		private Vector3 startPos;

		public List<GameObject> hits = new List<GameObject>();

		private Dictionary<GameObject, Vector3> contactPoints = new Dictionary<GameObject, Vector3>();

		private GameObject netgun;

		private GameObject osd;

		private GameObject osdYaw;

		private RectTransform osdRect;

		private RectTransform osdYawRect;

		private float duration;

		public Vector2 minMaxUAVSpeed;

		public Vector2 minMaxNetSize;

		public int netgunShotsAmount = 3;

		public float netShotForce = 7f;

		public Vector3 inheritedVelocity;

		private bool gameended;

		private DroneCamera m_camera;

		public Transform pipCamera;

		private string m_cameraMode = "FPV";

		private UIHUDTrainingLayer hud;

		public Mode mode;

		public DeferredNightVisionEffect nightVision;

		public SplineActor splineActor;

		public GameObject pipUI;

		private new void Start()
		{
			hud = base.app.view.ui.game.hud.training;
			osd = hud.counterUAVHUD.osdTarget;
			osdYaw = hud.counterUAVHUD.osdTargetYaw;
			osdRect = osd.GetComponent<RectTransform>();
			osdYawRect = osdYaw.GetComponent<RectTransform>();
			RefreshUI();
		}

		private void Update()
		{
			if (drone == null)
			{
				if (sim.simulation.drones.Get(0) != null)
				{
					drone = sim.simulation.drones.Get(0);
					netPrefab.transform.SetParent(drone.body.centerOfMassMarker, worldPositionStays: false);
					netPrefab.transform.localPosition = new Vector3(0f, -0.75f, 0f);
					netPrefab.transform.GetChild(0).SetParent(drone.body.centerOfMassMarker, worldPositionStays: true);
					netPrefab.SetActive(value: true);
					gunAnchor.transform.SetParent(drone.body.centerOfMassMarker);
					gunAnchor.transform.localPosition = new Vector3(0f, -0.75f, 0f);
					gunAnchor.transform.localRotation = Quaternion.identity;
					m_camera = sim.simulation.cameras.Get(0);
				}
			}
			else
			{
				if (mode == Mode.gun && (bool)osdRect && (bool)osdYawRect)
				{
					osdYawRect.eulerAngles = new Vector3(0f, 0f, drone.transform.rotation.eulerAngles.y);
					osdRect.eulerAngles = new Vector3(0f, 0f, 0f - drone.transform.rotation.eulerAngles.z);
				}
				ProcessInput();
			}
		}

		private void ProcessInput()
		{
			if (Input.GetKeyDown(KeyCode.Space) || RCI.GetButtonDown(ConsoleButtons.ActionBottomRow1))
			{
				if (gameended || mode == Mode.net || netgun != null || netgunShotsAmount < 0)
				{
					return;
				}
				contactPoints.Clear();
				netgun = Object.Instantiate(netGunPrefab, gunAnchor.transform);
				netgun.transform.localPosition = new Vector3(0f, 0f, 0.5f);
				netgun.SetActive(value: true);
				Rigidbody component = netgun.transform.GetChild(0).GetComponent<Rigidbody>();
				Cloth component2 = netgun.transform.GetChild(0).GetComponent<Cloth>();
				component2.enabled = true;
				component.angularVelocity = Vector3.zero;
				netgun.transform.SetParent(null);
				component.velocity = new Vector3(drone.rigidbody.rb.velocity.x * inheritedVelocity.x, drone.rigidbody.rb.velocity.y * inheritedVelocity.y, drone.rigidbody.rb.velocity.z * inheritedVelocity.z);
				component.AddForce(netgun.transform.forward * netShotForce, ForceMode.Impulse);
				component.useGravity = true;
				Object.Destroy(netgun, 3f);
				Object.Destroy(component2.gameObject, 3f);
				duration = sim.simulation.elapsed;
				this.TimerRunOnce(delegate
				{
					SubmitContactData();
					if (netgunShotsAmount == 0)
					{
						gameended = true;
						Notify("fn.mission@complete", (float)hits.Count / 10f);
						sim.main.Message("fn.mission@complete");
					}
				}, 2f);
				netgunShotsAmount--;
				RefreshUI();
			}
			if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
			{
				if (mode == Mode.net)
				{
					return;
				}
				mode = Mode.net;
				netPrefab.SetActive(value: true);
				hud.counterUAVHUD.SetOSDTargetVisible(p_visible: false);
				hud.counterUAVHUD.SetPIPOverlayVisible(p_visible: false);
				pipCamera.gameObject.SetActive(value: false);
				CameraFPV();
				RefreshUI();
			}
			if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
			{
				if (mode == Mode.gun)
				{
					return;
				}
				mode = Mode.gun;
				netPrefab.SetActive(value: false);
				hud.counterUAVHUD.SetPIPOverlayVisible(p_visible: true);
				CameraFPV();
				pipCamera.gameObject.SetActive(value: true);
				RefreshUI();
			}
			if (Input.GetKeyDown(KeyCode.I))
			{
				hud.ToggleCUAVInstructions();
			}
			if (Input.GetKeyDown(KeyCode.P))
			{
				if (mode == Mode.net)
				{
					return;
				}
				bool flag = !pipCamera.gameObject.activeInHierarchy;
				pipCamera.gameObject.SetActive(flag);
				hud.counterUAVHUD.SetPIPOverlayVisible(flag);
				RefreshUI();
			}
			if (Input.GetKeyDown(KeyCode.N))
			{
				nightVision.enabled = !nightVision.enabled;
				RefreshUI();
			}
			if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
			{
				if (splineActor.speed > minMaxUAVSpeed.x)
				{
					splineActor.speed -= 1f;
					splineActor.angularSpeed -= 1f;
				}
				RefreshUI();
			}
			if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
			{
				if (splineActor.speed < minMaxUAVSpeed.y)
				{
					splineActor.speed += 1f;
					splineActor.angularSpeed += 1f;
				}
				RefreshUI();
			}
			if (Input.GetKeyDown(KeyCode.C))
			{
				ToggleCameraMode();
			}
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				ScaleNet(p_up: true, p_width: false);
				RefreshUI();
			}
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				ScaleNet(p_up: false, p_width: false);
				RefreshUI();
			}
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				ScaleNet();
				RefreshUI();
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				ScaleNet(p_up: false);
				RefreshUI();
			}
			if (Input.GetKeyDown(KeyCode.D))
			{
				netgunShotsAmount++;
				netgunShotsAmount = Mathf.Clamp(netgunShotsAmount, 1, 10);
				RefreshUI();
			}
			if (Input.GetKeyDown(KeyCode.A))
			{
				netgunShotsAmount--;
				netgunShotsAmount = Mathf.Clamp(netgunShotsAmount, 1, 10);
				RefreshUI();
			}
			if (Input.GetKeyDown(KeyCode.S) && mode == Mode.gun)
			{
				float angle = gunAnchor.transform.eulerAngles.x + 5f;
				gunAnchor.transform.eulerAngles = new Vector3(ClampAngle(angle, -35f, 35f), gunAnchor.transform.eulerAngles.y, gunAnchor.transform.eulerAngles.z);
				RefreshUI();
			}
			if (Input.GetKeyDown(KeyCode.W) && mode == Mode.gun)
			{
				float angle2 = gunAnchor.transform.eulerAngles.x - 5f;
				gunAnchor.transform.eulerAngles = new Vector3(ClampAngle(angle2, -35f, 35f), gunAnchor.transform.eulerAngles.y, gunAnchor.transform.eulerAngles.z);
				RefreshUI();
			}
		}

		private void ToggleCameraMode()
		{
			if (mode != Mode.net)
			{
				if (m_cameraMode == "FPV")
				{
					CameraNetGun();
				}
				else
				{
					CameraFPV();
				}
			}
		}

		public void HitTrigger(GameObject g)
		{
			if (hits.Contains(g))
			{
				return;
			}
			hits.Add(g);
			if (mode != Mode.net)
			{
				return;
			}
			g.transform.parent.SetParent(netPrefab.transform);
			if (!gameended)
			{
				gameended = true;
				duration = sim.simulation.elapsed;
				this.TimerRunOnce(delegate
				{
					SubmitContactData();
					Notify("fn.mission@complete", (float)hits.Count / 10f);
					sim.main.Message("fn.mission@complete");
				}, 2f);
			}
		}

		public void RegisterContact(GameObject p_go, Vector3 p_cp)
		{
			if (!contactPoints.ContainsKey(p_go))
			{
				contactPoints.Add(p_go, p_cp);
			}
		}

		private void SubmitContactData()
		{
			ServiceModel service = base.app.model.service;
			if (contactPoints.Count == 0)
			{
				service.SendCounterUAVData(-2f, -2f, (mode == Mode.net) ? "net" : "gun", duration, delegate
				{
				});
				return;
			}
			int num = 0;
			Vector3 zero = Vector3.zero;
			foreach (KeyValuePair<GameObject, Vector3> contactPoint in contactPoints)
			{
				zero += contactPoint.Value;
				num++;
			}
			zero.x *= ((mode == Mode.net) ? (-1f) : 1f);
			zero /= (float)num * 5f;
			zero.x = Mathf.Clamp(zero.x, -1f, 1f);
			zero.z = Mathf.Clamp(zero.z, -1f, 1f);
			service.SendCounterUAVData(zero.z, zero.x, (mode == Mode.net) ? "net" : "gun", duration, delegate
			{
			});
			contactPoints.Clear();
		}

		private void ScaleNet(bool p_up = true, bool p_width = true)
		{
			float num = (p_up ? 0.01f : (-0.01f));
			float num2 = (p_up ? (-0.05f) : 0.05f);
			if (mode == Mode.net)
			{
				if (!p_width)
				{
					netPrefab.transform.localScale = new Vector3(Mathf.Clamp(netPrefab.transform.localScale.x + num, minMaxNetSize.x, minMaxNetSize.y), netPrefab.transform.localScale.y, netPrefab.transform.localScale.z);
				}
				else
				{
					netPrefab.transform.localScale = new Vector3(netPrefab.transform.localScale.x, netPrefab.transform.localScale.y, Mathf.Clamp(netPrefab.transform.localScale.z + num, minMaxNetSize.x, minMaxNetSize.y));
				}
				netPrefab.GetComponent<Cloth>().enabled = false;
				netPrefab.SetActive(value: false);
				netPrefab.SetActive(value: true);
				netPrefab.GetComponent<Cloth>().enabled = true;
				if (!p_width && (!p_up || !(netPrefab.transform.localScale.x + num >= minMaxNetSize.y)) && (p_up || !(netPrefab.transform.localScale.x + num <= minMaxNetSize.x)))
				{
					netPrefab.transform.localPosition = new Vector3(netPrefab.transform.localPosition.x, netPrefab.transform.localPosition.y + num2, netPrefab.transform.localPosition.z);
				}
				return;
			}
			Transform child = netGunPrefab.transform.GetChild(0);
			if (!(child.localScale.x <= minMaxNetSize.x))
			{
				if (p_width)
				{
					child.transform.localScale = new Vector3(Mathf.Clamp(child.transform.localScale.x + num, minMaxNetSize.x, minMaxNetSize.y), child.transform.localScale.y, child.transform.localScale.z);
				}
				else
				{
					child.transform.localScale = new Vector3(child.transform.localScale.x, child.transform.localScale.y, Mathf.Clamp(child.transform.localScale.z + num, minMaxNetSize.x, minMaxNetSize.y));
				}
			}
		}

		private float ClampAngle(float angle, float min, float max)
		{
			if (min < 0f && max > 0f && (angle > max || angle < min))
			{
				angle -= 360f;
				if (angle > max || angle < min)
				{
					if (Mathf.Abs(Mathf.DeltaAngle(angle, min)) < Mathf.Abs(Mathf.DeltaAngle(angle, max)))
					{
						return min;
					}
					return max;
				}
			}
			else if (min > 0f && (angle > max || angle < min))
			{
				angle += 360f;
				if (angle > max || angle < min)
				{
					if (Mathf.Abs(Mathf.DeltaAngle(angle, min)) < Mathf.Abs(Mathf.DeltaAngle(angle, max)))
					{
						return min;
					}
					return max;
				}
			}
			if (angle < min)
			{
				return min;
			}
			if (angle > max)
			{
				return max;
			}
			return angle;
		}

		public void CameraFPV()
		{
			m_camera.transform.SetParent(sim.simulation.cameras.container);
			m_camera.follow.target = drone.body.frame.camera.pivot;
			m_camera.follow.enabled = true;
			m_camera.SetFPV(drone);
			pipCamera.SetParent(gunAnchor.transform);
			pipCamera.localPosition = Vector3.zero;
			pipCamera.localRotation = Quaternion.identity;
			m_cameraMode = "FPV";
			hud.counterUAVHUD.SetOSDTargetVisible(p_visible: false);
			pipUI.SetActive(value: true);
			RefreshUI();
		}

		public void CameraNetGun()
		{
			if (mode == Mode.gun)
			{
				m_camera.SetNone();
				m_camera.follow.enabled = false;
				m_camera.follow.target = null;
				m_camera.transform.SetParent(gunAnchor.transform);
				m_camera.transform.localPosition = Vector3.zero;
				m_camera.transform.localRotation = Quaternion.identity;
				pipCamera.SetParent(drone.body.frame.camera.pivot);
				pipCamera.localPosition = Vector3.zero;
				pipCamera.localRotation = Quaternion.identity;
				m_cameraMode = "DOWN";
				hud.counterUAVHUD.SetOSDTargetVisible(p_visible: true);
				pipUI.SetActive(value: false);
				RefreshUI();
			}
		}

		private void RefreshUI()
		{
			Transform transform = ((mode == Mode.gun) ? netGunPrefab.transform.GetChild(0) : netPrefab.transform);
			hud.RefreshCounterUAVUI(splineActor.speed, new Vector2(transform.localScale.z * 10f, transform.localScale.x * 10f), netgunShotsAmount, m_cameraMode, nightVision.enabled, mode == Mode.gun, gunAnchor.transform.localEulerAngles.x);
		}
	}
}
