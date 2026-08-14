using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.sim
{
	public class UIDroneOverlay : MonoBehaviour
	{
		[SerializeField]
		private FadeComponent m_fade;

		[SerializeField]
		private RawImage m_image;

		public Transform render;

		public Camera camera;

		private bool m_rtDynamic;

		public DroneSimulation simulation;

		private DroneFactory m_factory;

		private Drone m_drone;

		private bool m_droneRunning = true;

		private Vector3 m_cameraOffset;

		[NonSerialized]
		public DroneRigData rig;

		public TextAsset defaultRig;

		private bool m_enableDroneControl;

		public GameObject renderFixPrefab;

		private Transform m_lighting;

		public Text angularVelocityField;

		public Text speedField;

		public bool autoShow = true;

		public bool cameraFollow = true;

		public bool kinematicDrone;

		public Canvas renderCanvas;

		private Quaternion m_startRotation = Quaternion.identity;

		private float lastPitch;

		private float lastRoll;

		private float lastYaw;

		private int waitCount = 30;

		private Coroutine UpdatingProfile;

		private FCProfileData m_lastProfileData;

		public FadeComponent fade
		{
			get
			{
				if (!m_fade)
				{
					return m_fade = GetComponent<FadeComponent>();
				}
				return m_fade;
			}
		}

		public RawImage image
		{
			get
			{
				if (!m_image)
				{
					return m_image = GetComponent<RawImage>();
				}
				return m_image;
			}
		}

		public DroneFactory factory
		{
			get
			{
				if (m_factory == null)
				{
					m_factory = UnityEngine.Object.FindObjectOfType<DroneFactory>();
				}
				return m_factory;
			}
		}

		public Drone drone => m_drone;

		public DroneFlightController.Sensor sensor
		{
			get
			{
				if (drone == null)
				{
					return null;
				}
				if (drone.fc == null)
				{
					return null;
				}
				return drone.fc.sensor;
			}
		}

		public bool droneRunning
		{
			get
			{
				if (drone == null)
				{
					return false;
				}
				return m_droneRunning;
			}
			set
			{
				m_droneRunning = value;
				if (drone != null && drone.fc != null)
				{
					drone.fc.armed = value;
					SpinProps(value);
				}
			}
		}

		public bool enableDroneControl
		{
			get
			{
				return m_enableDroneControl;
			}
			set
			{
				m_enableDroneControl = value;
				if (drone != null && drone.fc != null)
				{
					drone.fc.allowPitch = value;
					drone.fc.allowRoll = value;
					drone.fc.allowYaw = value;
					drone.fc.allowThrottle = value;
				}
			}
		}

		public Transform lighting
		{
			get
			{
				if (m_lighting == null && renderFixPrefab != null)
				{
					m_lighting = UnityEngine.Object.Instantiate(renderFixPrefab).transform;
					m_lighting.name = "lighting";
					m_lighting.parent = render;
					m_lighting.localScale = Vector3.one;
					m_lighting.localPosition = Vector3.zero;
				}
				return m_lighting;
			}
		}

		private void Start()
		{
			if (render == null)
			{
				render = base.transform.Find("render");
			}
			if (render == null)
			{
				Debug.LogError("UIDroneOverlay:: no render node found");
				base.gameObject.SetActive(value: false);
				return;
			}
			if (camera == null)
			{
				Transform transform = render.Find("camera");
				if ((bool)transform)
				{
					camera = transform.GetComponent<Camera>();
				}
			}
			if (camera == null)
			{
				Debug.LogError("UIDroneOverlay:: no camera found");
				base.gameObject.SetActive(value: false);
				return;
			}
			if (simulation == null)
			{
				Transform transform2 = render.Find("simulation");
				if ((bool)transform2)
				{
					simulation = transform2.GetComponent<DroneSimulation>();
				}
			}
			if (simulation == null)
			{
				Debug.LogError("UIDroneOverlay:: no simulation found");
				base.gameObject.SetActive(value: false);
				return;
			}
			if (factory == null)
			{
				Debug.LogError("UIDroneOverlay:: no factory found");
				base.gameObject.SetActive(value: false);
				return;
			}
			m_rtDynamic = false;
			camera.enabled = false;
			Activity.Run((Func<bool>)delegate
			{
				if (image.rectTransform.rect.width <= 0f)
				{
					return true;
				}
				RenderTexture targetTexture = camera.targetTexture;
				m_rtDynamic = true;
				RenderTexture renderTexture = new RenderTexture((int)image.rectTransform.rect.width, (int)image.rectTransform.rect.height, targetTexture.depth, RenderTextureFormat.ARGBFloat);
				renderTexture.anisoLevel = targetTexture.anisoLevel;
				renderTexture.antiAliasing = targetTexture.antiAliasing;
				renderTexture.filterMode = targetTexture.filterMode;
				renderTexture.isPowerOfTwo = targetTexture.isPowerOfTwo;
				renderTexture.useMipMap = targetTexture.useMipMap;
				renderTexture.mipMapBias = targetTexture.mipMapBias;
				renderTexture.autoGenerateMips = targetTexture.autoGenerateMips;
				renderTexture.wrapMode = targetTexture.wrapMode;
				renderTexture.Create();
				camera.targetTexture = renderTexture;
				camera.enabled = true;
				image.texture = renderTexture;
				return false;
			}, 0f, false);
			render.parent = null;
			render.localScale = Vector3.one;
			render.name = "ui-drone-renderer";
			simulation.podiums.Build();
			simulation.Initialize();
			simulation.transmitters.Add<DroneRCTransmitter>().channel = 0;
			fade.alpha = 0f;
			if (rig == null)
			{
				rig = ScriptableObject.CreateInstance<DroneRigData>();
				if (defaultRig != null && defaultRig.bytes != null)
				{
					rig.Set(defaultRig.bytes);
				}
			}
			StartCoroutine(WaitForInitAndCreateRig(rig));
		}

		private void LateUpdate()
		{
			if (render != null && renderCanvas != null)
			{
				render.gameObject.SetActive(renderCanvas.enabled);
			}
			AdjustCamera();
			if (drone != null && drone.fc != null && drone.fc.sensor != null)
			{
				if (angularVelocityField != null && drone.fc.sensor.gyro != null)
				{
					angularVelocityField.text = drone.fc.sensor.gyro.averageVelocity.ToString("0.00") + " deg/s";
				}
				if (drone.body != null && drone.body.frame != null && drone.body.frame.escs != null)
				{
					for (int i = 0; i < drone.body.frame.escs.Count; i++)
					{
						DroneESC droneESC = drone.body.frame.escs[i];
						if (droneESC != null && droneESC.motor != null && droneESC.motor.animation != null)
						{
							droneESC.motor.animation.ForceShader();
						}
					}
				}
				drone.rigidbody.isKinematic = kinematicDrone;
				if (!kinematicDrone && drone.hasPhysics)
				{
					drone.physics.threaded = false;
					drone.physics.legacyDrag = true;
				}
				if (drone.receiver != null)
				{
					SignalVector signalVector = drone.fc.TransformSignal(drone.receiver.signal, null);
					drone.rigidbody.rb.angularVelocity = drone.transform.rotation * new Vector3(signalVector.pitch * ((float)Math.PI / 180f), signalVector.yaw * ((float)Math.PI / 180f), signalVector.roll * ((float)Math.PI / 180f));
					if (drone.rigidbody.rb.angularVelocity.sqrMagnitude < 0.1f)
					{
						waitCount--;
						if (waitCount < 0)
						{
							drone.transform.rotation = Quaternion.Lerp(drone.transform.rotation, m_startRotation, Time.deltaTime * 10f);
						}
					}
					else
					{
						waitCount = 30;
					}
				}
			}
			if (rig != null && m_drone != null && rig.guid != m_drone.rig.guid)
			{
				StartCoroutine(WaitForInitAndCreateRig(rig));
			}
		}

		private void AdjustCamera()
		{
			if (cameraFollow && (bool)drone)
			{
				if (drone.localPosition.y < -1000f)
				{
					Vector3 localPosition = drone.localPosition;
					localPosition.y = 0f;
					drone.localPosition = localPosition;
					drone.rigidbody.ClearForces();
				}
				camera.transform.position = drone.position + m_cameraOffset;
			}
		}

		private void OnDisable()
		{
			if ((bool)render)
			{
				render.gameObject.SetActive(value: false);
			}
		}

		private void OnEnable()
		{
			if ((bool)render)
			{
				render.gameObject.SetActive(value: true);
			}
		}

		private void OnDestroy()
		{
			if (m_rtDynamic && camera != null && camera.targetTexture != null)
			{
				UnityEngine.Object.Destroy(camera.targetTexture);
			}
			if ((bool)render)
			{
				UnityEngine.Object.Destroy(render.gameObject);
			}
		}

		private IEnumerator WaitForInitAndCreateRig(DroneRigData p_rig)
		{
			rig = p_rig;
			while (simulation == null)
			{
				yield return null;
			}
			while (factory == null)
			{
				yield return null;
			}
			bool flag = true;
			if (simulation.drones.list.Count > 0)
			{
				Drone any = simulation.drones.Any;
				if (any != null)
				{
					simulation.drones.list.Remove(any);
					if ((bool)lighting)
					{
						lighting.parent = base.transform;
					}
					any.gameObject.SetActive(value: false);
					UnityEngine.Object.Destroy(any.gameObject, 0.1f);
					flag = false;
				}
			}
			m_drone = factory.Instantiate(p_rig, base.transform);
			if (!(m_drone != null))
			{
				yield break;
			}
			if (!simulation.drones.list.Contains(m_drone))
			{
				simulation.drones.list.Add(m_drone);
			}
			m_drone.OnEvent.AddListener(delegate(DroneEvent p_event)
			{
				if (p_event.type == DroneEventType.Ready)
				{
					Transform[] componentsInChildren = m_drone.GetComponentsInChildren<Transform>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].gameObject.layer = render.gameObject.layer;
					}
					m_drone.receiver.channel = 0;
					m_drone.SetEnabled(p_flag: true);
					m_drone.fc.armed = m_droneRunning;
					m_drone.fc.SetProcess(FlightControllerProcess.Level, p_flag: true);
					m_drone.fc.process.level.rate = 1f;
					m_drone.fc.process.level.delay = 2f;
					m_drone.transform.localScale = Vector3.one;
					m_startRotation = m_drone.transform.rotation;
					m_drone.renderer.SetTrailsEnabled(p_flag: false);
					m_drone.rigidbody.rb.useGravity = false;
					m_drone.rigidbody.ClearForces();
					SpinProps(m_droneRunning);
					if (speedField != null && drone.fc.sensor.inertial != null)
					{
						speedField.text = drone.rig.topSpeed + " km/h";
					}
					enableDroneControl = enableDroneControl;
					simulation.Run();
					if (kinematicDrone)
					{
						this.TimerRunOnce(delegate
						{
							m_drone.fc.armed = false;
							m_drone.fc.enabled = false;
							SpinProps(p_spin: true);
							m_drone.SetMotorRPM(2000f);
							cameraFollow = false;
							m_drone.enabled = false;
						}, 0.2f);
					}
					Activity.Run((Func<bool>)delegate
					{
						if (!camera.enabled)
						{
							return true;
						}
						if (autoShow)
						{
							fade.FadeIn(0.2f);
						}
						return false;
					}, 0f, false);
				}
			});
			if (m_drone.transform.parent != simulation.drones.container)
			{
				m_drone.transform.SetParent(simulation.drones.container, worldPositionStays: true);
			}
			m_drone.localPosition = Vector3.zero;
			m_drone.transform.localRotation = Quaternion.identity;
			if (flag)
			{
				m_cameraOffset = camera.transform.position - m_drone.position;
			}
			if ((bool)lighting)
			{
				lighting.parent = m_drone.transform;
				lighting.localPosition = Vector3.zero;
				lighting.localRotation = Quaternion.identity;
				lighting.localScale = Vector3.one;
			}
			m_drone.gameObject.layer = render.gameObject.layer;
		}

		private IEnumerator WaitForFCAndUpdateProfile()
		{
			while (drone == null)
			{
				yield return null;
			}
			while (drone.fc == null)
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.1f);
			while (drone == null)
			{
				yield return null;
			}
			while (drone.fc == null)
			{
				yield return null;
			}
			drone.fc.profile.SetData(m_lastProfileData);
			UpdatingProfile = null;
		}

		public void RefreshProfile(FCProfileData p_profile)
		{
			m_lastProfileData = p_profile;
			if (UpdatingProfile == null)
			{
				UpdatingProfile = StartCoroutine(WaitForFCAndUpdateProfile());
			}
		}

		private void SpinProps(bool p_spin)
		{
			if (!(drone == null) && !(drone.body == null) && !(drone.body.frame == null))
			{
				drone.body.frame.SpinProps(p_spin);
			}
		}

		public void Show(float p_duration = 0.2f)
		{
			fade.FadeIn(p_duration);
		}

		public void Hide(float p_duration = 0.2f)
		{
			fade.FadeOut(p_duration);
		}
	}
}
