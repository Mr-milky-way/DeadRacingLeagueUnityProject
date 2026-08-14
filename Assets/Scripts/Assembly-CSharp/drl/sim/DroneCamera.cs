using System;
using System.Collections;
using System.Collections.Generic;
using GPUInstancer;
using UnityEngine;
using drl.game;
using thelab.core;

namespace drl.sim
{
	[RequireComponent(typeof(OrbitTransform))]
	[RequireComponent(typeof(OrbitWASDInput))]
	[RequireComponent(typeof(OrbitFollowInput))]
	public class DroneCamera : MonoBehaviour
	{
		public const OrbitTransform.Transition OrbitSnapFlag = (OrbitTransform.Transition)4369;

		public List<Camera> cameras;

		private CameraFX m_fx;

		private DynamicResolutionFX m_drfx;

		private VideoCapture m_video;

		private CameraNearPlaneSnap m_npsnap;

		[SerializeField]
		private DroneCameraModeType m_mode;

		[SerializeField]
		private Drone m_drone;

		private OrbitTransform m_orbit;

		private OrbitWASDInput m_wasd;

		private OrbitFollowInput m_follow;

		public DRLResolveHoleCollision holeCollision;

		public bool unscaledTime;

		public bool lensDistortionAllowed;

		private Activity m_camera_updater;

		private Activity m_fpv_updater;

		public bool inGarage;

		public Action<DroneCameraModeType> CameraModeChanged;

		public RenderTexture captureRT;

		public Camera main
		{
			get
			{
				if (cameras.Count > 0)
				{
					return cameras[0];
				}
				return null;
			}
		}

		public Camera background
		{
			get
			{
				if (cameras.Count > 1)
				{
					return cameras[1];
				}
				return null;
			}
		}

		public CameraFX fx
		{
			get
			{
				if (!base.gameObject)
				{
					return null;
				}
				return Reflection<object>.Assert(ref m_fx, base.gameObject, p_add: false);
			}
		}

		public DynamicResolutionFX drfx
		{
			get
			{
				if (!main)
				{
					return null;
				}
				return Reflection<object>.Assert(ref m_drfx, main.gameObject, p_add: false);
			}
		}

		public VideoCapture video => Reflection<object>.Assert(ref m_video, base.gameObject);

		public CameraNearPlaneSnap npsnap
		{
			get
			{
				if (!m_npsnap)
				{
					return m_npsnap = GetComponentInChildren<CameraNearPlaneSnap>();
				}
				return m_npsnap;
			}
		}

		public DroneCameraModeType mode
		{
			get
			{
				return m_mode;
			}
			internal set
			{
				if (m_mode != value)
				{
					OnModeChange(m_mode, value);
				}
				m_mode = value;
			}
		}

		public Drone drone
		{
			get
			{
				return m_drone;
			}
			set
			{
				if (m_drone != value)
				{
					OnDroneChange(value);
				}
				m_drone = value;
			}
		}

		public OrbitTransform orbit
		{
			get
			{
				if (!m_orbit)
				{
					return m_orbit = GetComponent<OrbitTransform>();
				}
				return m_orbit;
			}
		}

		public OrbitWASDInput wasd
		{
			get
			{
				if (!m_wasd)
				{
					return m_wasd = GetComponent<OrbitWASDInput>();
				}
				return m_wasd;
			}
		}

		public OrbitFollowInput follow
		{
			get
			{
				if (!m_follow)
				{
					return m_follow = GetComponent<OrbitFollowInput>();
				}
				return m_follow;
			}
		}

		public CameraLens lens
		{
			get
			{
				if (cameras.Count <= 0)
				{
					return null;
				}
				return cameras[0].GetComponent<CameraLens>();
			}
		}

		public float hfov
		{
			get
			{
				if (!lens)
				{
					return 0f;
				}
				return lens.hfov;
			}
			set
			{
				if ((bool)lens)
				{
					lens.hfov = value;
				}
			}
		}

		public float fov
		{
			get
			{
				float num = fovOffset;
				if (cameras.Count > 0)
				{
					return cameras[0].fieldOfView - num;
				}
				return 0f;
			}
			set
			{
				RefreshFOV(value);
			}
		}

		public float cameraFOV
		{
			get
			{
				if (cameras.Count > 0)
				{
					return cameras[0].fieldOfView;
				}
				return 0f;
			}
		}

		public float fovOffset
		{
			get
			{
				if (mode == DroneCameraModeType.FPV)
				{
					if (!lensDistortionAllowed)
					{
						return 0f;
					}
					return FCProfileData.lensDistortionFOVOffset;
				}
				return 0f;
			}
		}

		public void RefreshFOV(float v)
		{
			for (int i = 0; i < cameras.Count; i++)
			{
				if ((bool)cameras[i])
				{
					cameras[i].fieldOfView = v + fovOffset;
				}
			}
			GPUInstancerAPI.SetCamera(main);
		}

		public void SetGameCameraEnabled(bool p_flag)
		{
			if (cameras.Count >= 1 && (bool)cameras[0])
			{
				cameras[0].enabled = p_flag;
			}
			if (cameras.Count >= 2 && (bool)cameras[1])
			{
				cameras[1].enabled = p_flag;
			}
			if (cameras.Count >= 3 && (bool)cameras[2])
			{
				cameras[2].enabled = p_flag;
			}
		}

		public void SetNearFarClips(float p_near, float p_far)
		{
			if (cameras.Count >= 1 && (bool)cameras[0])
			{
				cameras[0].nearClipPlane = 0.04f;
			}
			if (cameras.Count >= 1 && (bool)cameras[0])
			{
				cameras[0].farClipPlane = p_far;
			}
			if (cameras.Count >= 2 && (bool)cameras[1])
			{
				cameras[1].nearClipPlane = 0.04f;
			}
			if (cameras.Count >= 2 && (bool)cameras[1])
			{
				cameras[1].farClipPlane = p_far;
			}
			if (cameras.Count >= 3 && (bool)cameras[2])
			{
				cameras[2].nearClipPlane = 0.04f;
			}
			if (cameras.Count >= 3 && (bool)cameras[2])
			{
				cameras[2].farClipPlane = p_far;
			}
		}

		public void RefreshFOV()
		{
			RefreshFOV(fov);
		}

		protected virtual void Awake()
		{
			if (cameras.Count <= 0)
			{
				cameras = Hierarchy.FindAll<Camera>(base.transform);
			}
			holeCollision = GetComponent<DRLResolveHoleCollision>();
			if ((bool)holeCollision)
			{
				Timer.Set(holeCollision, "enabled", 1f / 30f, false);
			}
			OnModeChange(DroneCameraModeType.None, mode);
			wasd.enabled = false;
			wasd.usePhysics = false;
			wasd.useJoystick = false;
		}

		public void SetBackgroundEnabled(bool p_flag)
		{
			if ((bool)background)
			{
				background.enabled = p_flag;
			}
		}

		public void SetFPV(Drone p_drone)
		{
			drone = p_drone;
			if ((bool)drone && drone.ready && (bool)follow)
			{
				follow.flags = OrbitFollowInput.Flag.All;
				follow.target = drone.body.frame.camera.pivot;
				orbit.distance = -0.02f;
				orbit.angle = Vector2.zero;
				orbit.SetTransitionMask(OrbitTransform.TransitionMask.Snap);
				mode = DroneCameraModeType.FPV;
				wasd.enabled = false;
				drone.renderer.shadowsOnly = true;
				if (CameraModeChanged != null)
				{
					CameraModeChanged(mode);
				}
			}
		}

		public void SetFPVSmooth(Drone p_drone, float p_transition_time)
		{
			drone = p_drone;
			orbit.constraint.Clear();
			orbit.SetTransitionMask(OrbitTransform.TransitionMask.Snap);
			Tween tween = Tween.Add(orbit, "distance", 0.1f, p_transition_time, 0f, Cubic.Out);
			tween.onComplete = (Action<Tween>)Delegate.Combine(tween.onComplete, (Action<Tween>)delegate
			{
				SetFPV(drone);
				drone.renderer.shadowsOnly = true;
			});
			mode = DroneCameraModeType.FPVSmooth;
			if (CameraModeChanged != null)
			{
				CameraModeChanged(mode);
			}
		}

		public void SetTPVBack(Drone p_drone, float p_distance = 0.4f, bool p_rotate_x = false, bool p_rotate_y = false)
		{
			drone = p_drone;
			SetTPV(DroneCameraModeType.TPVBack, p_distance);
			orbit.constraint.Clear();
			follow.target = drone.transform;
			orbit.constraint.angleMin = new Vector2(p_rotate_x ? float.NegativeInfinity : 0f, p_rotate_y ? float.NegativeInfinity : 0f);
			orbit.constraint.angleMax = new Vector2(p_rotate_x ? float.PositiveInfinity : 0f, p_rotate_y ? float.PositiveInfinity : 0f);
			orbit.constraint.distanceMin = (orbit.constraint.distanceMax = p_distance);
			drone.renderer.shadowsOnly = false;
			if (CameraModeChanged != null)
			{
				CameraModeChanged(DroneCameraModeType.TPVBack);
			}
		}

		public void SetTPVSmooth(Drone p_drone, float p_distance = 0.4f, bool p_rotate_x = false, bool p_rotate_y = false)
		{
			drone = p_drone;
			mode = DroneCameraModeType.TPVSmooth;
			drone.renderer.shadowsOnly = false;
			if (CameraModeChanged != null)
			{
				CameraModeChanged(mode);
			}
		}

		public void SetTPVFree(Drone p_drone, float p_distance = 0.4f, float p_min_distance = 0.5f, float p_max_distance = 2.5f, bool p_smooth = false)
		{
			drone = p_drone;
			SetTPV(DroneCameraModeType.TPVFree, p_distance, p_smooth);
			orbit.constraint.Clear();
			orbit.constraint.distanceMin = p_min_distance;
			orbit.constraint.distanceMax = p_max_distance;
			drone.renderer.shadowsOnly = false;
			if (CameraModeChanged != null)
			{
				CameraModeChanged(DroneCameraModeType.TPVFree);
			}
		}

		public void SetTPVMissions(Drone p_drone, float p_distance = 0.4f, float p_min_distance = 0f, float p_max_distance = 10f)
		{
			drone = p_drone;
			drone.renderer.shadowsOnly = false;
			SetTPV(DroneCameraModeType.TPVMissions, p_distance);
			orbit.constraint.Clear();
			orbit.constraint.distanceMin = p_min_distance;
			orbit.constraint.distanceMax = p_max_distance;
			orbit.angle = new Vector2(0f, 12f);
			follow.offset = new Vector3(0f, 0.1f, 0f);
			follow.target = drone.transform;
			follow.flags = (OrbitFollowInput.Flag)23;
			orbit.SetTransitionMask(OrbitTransform.TransitionMask.SmoothTPV);
			drone.renderer.shadowsOnly = false;
			if (CameraModeChanged != null)
			{
				CameraModeChanged(DroneCameraModeType.TPVMissions);
			}
		}

		protected void SetTPV(DroneCameraModeType p_mode, float p_distance, bool smooth = false)
		{
			if (!smooth)
			{
				orbit.distance = p_distance;
			}
			else
			{
				Tween tween = Tween.Add(orbit, "distance", p_distance, 3f, 0f, Cubic.Out);
				tween.onComplete = (Action<Tween>)Delegate.Combine(tween.onComplete, (Action<Tween>)delegate
				{
					mode = p_mode;
				});
			}
			if (p_mode == DroneCameraModeType.TPVFree)
			{
				if (!smooth)
				{
					orbit.anchorRotation = Quaternion.identity;
				}
				else
				{
					Tween.Add(orbit, "anchorRotation", Quaternion.identity, 1f, 0f, Cubic.Out);
				}
				mode = p_mode;
			}
			else
			{
				mode = (smooth ? DroneCameraModeType.TPVSmooth : p_mode);
			}
			if (CameraModeChanged != null)
			{
				CameraModeChanged(mode);
			}
		}

		public void SetGameExternal(Drone p_drone)
		{
			StopUpdater();
			if ((bool)p_drone)
			{
				Vector2 angle = new Vector2(0f, 12f);
				SetTPVBack(p_drone, 0.5f);
				follow.flags = OrbitFollowInput.Flag.All;
				follow.offset = new Vector3(0f, 0.1f, 0f);
				orbit.angle = angle;
				fov = 45f;
				p_drone.renderer.shadowsOnly = false;
			}
		}

		public void SetFreeCamera(bool p_reset_y = false)
		{
			if (drone != null)
			{
				drone.renderer.shadowsOnly = false;
			}
			drone = null;
			StopUpdater();
			mode = DroneCameraModeType.Free;
			if (p_reset_y)
			{
				Vector3 forward = orbit.transform.forward;
				forward.y = 0f;
				forward.Normalize();
				Transform obj = orbit.transform;
				Quaternion rotation = (orbit.anchorRotation = Quaternion.LookRotation(forward, Vector3.up));
				obj.rotation = rotation;
				orbit.Snap();
			}
			if (CameraModeChanged != null)
			{
				CameraModeChanged(mode);
			}
		}

		public void SetNone()
		{
			if (drone != null)
			{
				drone.renderer.shadowsOnly = false;
			}
			drone = null;
			StopUpdater();
			mode = DroneCameraModeType.None;
			orbit.SetTransitionMask(OrbitTransform.TransitionMask.Snap);
			orbit.constraint.Clear();
			if (CameraModeChanged != null)
			{
				CameraModeChanged(mode);
			}
		}

		public void SetIntro(DroneSimulation p_simulation, float p_duration, float p_amplitude, float p_angle_y, float p_distance)
		{
			if (!p_simulation)
			{
				return;
			}
			mode = DroneCameraModeType.None;
			orbit.transition = (OrbitTransform.Transition)4369;
			orbit.constraint.Clear();
			StopUpdater();
			int off = Mathf.FloorToInt(UnityEngine.Random.value * 6f);
			if ((bool)fx)
			{
				fx.distortEnabled = false;
			}
			float animation_elapsed = 0f;
			m_camera_updater = Activity.Run((Predicate<float>)delegate
			{
				if (!p_simulation.drones.ready)
				{
					return true;
				}
				List<Drone> list = p_simulation.drones.list;
				float num = animation_elapsed;
				int index = (Mathf.FloorToInt(num) + off) % list.Count;
				float num2 = num - Mathf.Floor(num);
				float a = (0f - p_amplitude) * 0.5f;
				float b = p_amplitude * 0.5f;
				Vector2 angle = new Vector2
				{
					x = (((Mathf.FloorToInt(num) & 1) == 0) ? Mathf.Lerp(a, b, num2) : Mathf.Lerp(a, b, 1f - num2)),
					y = 15f + p_angle_y
				};
				Vector3 position = list[index].transform.position;
				orbit.anchor = position;
				orbit.angle = angle;
				orbit.distance = p_distance;
				orbit.anchorRotation = Quaternion.LookRotation(-list[index].transform.forward, Vector3.up);
				if (Vector3.Distance(orbit.anchor, position) <= 0.2f)
				{
					float num3 = (unscaledTime ? Time.fixedUnscaledDeltaTime : Time.deltaTime);
					animation_elapsed += ((p_duration <= 0f) ? 0f : (num3 / p_duration));
				}
				return true;
			}, 0f, false);
		}

		public void SetPodiumAnimation(int p_defaultPodium, DroneSimulation p_simulation, Transform p_clip_container, float p_speed, List<GamePlayerData> p_racers, Action<int> p_callback = null)
		{
			if (!p_clip_container)
			{
				return;
			}
			DroneSimulation sim = p_simulation;
			if (!sim)
			{
				return;
			}
			mode = DroneCameraModeType.None;
			orbit.transition = (OrbitTransform.Transition)4369;
			orbit.constraint.Clear();
			follow.flags = OrbitFollowInput.Flag.All;
			if ((bool)fx)
			{
				fx.distortEnabled = false;
			}
			StopUpdater();
			List<Animator> anims = Hierarchy.FindAll<Animator>(p_clip_container);
			if (anims.Count <= 0)
			{
				return;
			}
			List<Transform> anchors = new List<Transform>();
			for (int i = 0; i < anims.Count; i++)
			{
				Animator animator = anims[i];
				if (animator.transform.childCount <= 0)
				{
					anims.RemoveAt(i--);
					continue;
				}
				Transform transform = Hierarchy.Find(animator.transform, "transform.camera");
				if (!transform)
				{
					anims.RemoveAt(i--);
				}
				else
				{
					anchors.Add(transform);
				}
			}
			if (anchors.Count <= 0)
			{
				return;
			}
			fov = 50f;
			Debug.Log("DroneCamera> SetPodiumAnimation - speed[" + p_speed + "] anchors[" + anchors.Count + "] container[" + p_clip_container?.ToString() + "]");
			List<int> next_idx = new List<int>();
			for (int j = 0; j < anims.Count; j++)
			{
				next_idx.Add(j);
			}
			List<int> next_podium_idx = new List<int>();
			for (int k = 0; k < sim.drones.list.Count; k++)
			{
				next_podium_idx.Add(k);
			}
			List<int> list = next_idx;
			for (int l = 0; l < list.Count; l++)
			{
				list.Sort((int a, int b) => (!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
			}
			list = next_podium_idx;
			for (int num = 0; num < list.Count; num++)
			{
				list.Sort((int a, int b) => (!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
			}
			int next = 0;
			int state = 0;
			int next_podium = 0;
			Transform container = p_clip_container;
			List<GamePlayerData> ready_players = new List<GamePlayerData>();
			m_camera_updater = ((Component)this).ActivityRun((Predicate<float>)delegate
			{
				if (this == null)
				{
					return true;
				}
				if (sim.podiums.list == null || sim.podiums.list.Count == 0)
				{
					return true;
				}
				ready_players.Clear();
				for (int m = 0; m < p_racers.Count; m++)
				{
					if (p_racers[m].drone != null && p_racers[m].drone.ready)
					{
						ready_players.Add(p_racers[m]);
					}
				}
				int count = ready_players.Count;
				if (count != next_podium_idx.Count)
				{
					next_podium_idx.Clear();
					for (int n = 0; n < ready_players.Count; n++)
					{
						next_podium_idx.Add(ready_players[n].order);
					}
					for (int num2 = 0; num2 < next_podium_idx.Count; num2++)
					{
						next_podium_idx.Sort((int ia, int ib) => (!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
					}
				}
				if (next_podium_idx.Count <= 0)
				{
					next_podium_idx.Add(0);
				}
				int value = ((next_idx.Count > 0) ? next_idx[Mathf.Clamp(next, 0, next_idx.Count - 1)] : 0);
				next_podium = Mathf.Clamp(next_podium, 0, next_podium_idx.Count - 1);
				int num3 = ((next_podium_idx.Count > 0) ? next_podium_idx[next_podium] : 0);
				value = Mathf.Clamp(value, 0, Mathf.Min(anims.Count, anchors.Count) - 1);
				num3 = ((count == 1) ? p_defaultPodium : num3);
				Animator animator2 = ((value < 0) ? null : ((value >= anims.Count) ? null : anims[value]));
				Transform transform2 = ((value < 0) ? null : ((value >= anchors.Count) ? null : anchors[value]));
				DronePodium dronePodium = ((num3 < 0) ? null : ((num3 >= sim.podiums.list.Count) ? null : sim.podiums.list[num3]));
				switch (state)
				{
				case 0:
					if ((bool)animator2)
					{
						animator2.StartPlayback();
						animator2.speed = p_speed;
						animator2.gameObject.SetActive(value: true);
					}
					if ((bool)transform2)
					{
						follow.target = transform2;
						base.transform.position = transform2.position;
						base.transform.rotation = transform2.rotation;
					}
					if ((bool)dronePodium)
					{
						container.position = dronePodium.spawn.position;
						container.rotation = dronePodium.spawn.rotation;
					}
					state = 1;
					if (p_callback != null)
					{
						p_callback(num3);
					}
					if ((bool)fx)
					{
						fx.aoIntensity = 3f;
						fx.aoRadius = 0.015f;
						fx.SetDOF(container, 9f, 35f);
					}
					break;
				case 1:
					if (!animator2)
					{
						state = 2;
					}
					else if (animator2.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.95f)
					{
						state = 2;
					}
					break;
				case 2:
					next = (next + 1) % next_idx.Count;
					next_podium = (next_podium + 1) % next_podium_idx.Count;
					if ((bool)animator2)
					{
						animator2.StopPlayback();
						animator2.gameObject.SetActive(value: false);
					}
					state = 0;
					break;
				}
				return true;
			}, 0f);
			m_camera_updater.late = true;
		}

		public void SetPodiumAnimation(DroneSimulation p_simulation, Transform p_clip_container, List<GamePlayerData> p_racers, Action<int> p_callback = null)
		{
			SetPodiumAnimation(0, p_simulation, p_clip_container, 1f, p_racers, p_callback);
		}

		public void StopPodiumAnimation(Transform p_clip_container)
		{
			if ((bool)fx)
			{
				fx.ClearDOF();
			}
			if ((bool)p_clip_container)
			{
				List<Animator> list = Hierarchy.FindAll<Animator>(p_clip_container);
				for (int i = 0; i < list.Count; i++)
				{
					Animator animator = list[i];
					animator.StopPlayback();
					animator.gameObject.SetActive(value: false);
				}
			}
			StopUpdater();
		}

		public void SetFollow(Transform p_target, float p_distance)
		{
			StopUpdater();
			follow.target = p_target;
			follow.flags = OrbitFollowInput.Flag.All;
			orbit.constraint.Clear();
			orbit.constraint.angleMin = Vector2.zero;
			orbit.constraint.angleMax = Vector2.zero;
			orbit.constraint.distanceMin = (orbit.constraint.distanceMax = p_distance);
			mode = DroneCameraModeType.Follow;
		}

		public void SetFollow(Camera p_target, float p_distance)
		{
			StopUpdater();
			follow.target = p_target.transform;
			follow.flags = OrbitFollowInput.Flag.All;
			orbit.constraint.Clear();
			orbit.constraint.angleMin = Vector2.zero;
			orbit.constraint.angleMax = Vector2.zero;
			orbit.constraint.distanceMin = (orbit.constraint.distanceMax = p_distance);
			mode = DroneCameraModeType.Follow;
			m_camera_updater = Activity.Run((Func<bool>)delegate
			{
				if (!p_target)
				{
					return false;
				}
				float fieldOfView = p_target.fieldOfView;
				fov = fieldOfView;
				return true;
			}, 0f, false);
			m_camera_updater.late = true;
		}

		public void SetLineCamera(Drone p_drone, LineTransform p_line, float p_speed = 3f, bool betweenAnchors = false)
		{
			drone = p_drone;
			mode = DroneCameraModeType.LineCamera;
			Drone current_drone = p_drone;
			StopUpdater();
			orbit.transition = (OrbitTransform.Transition)4352;
			if (!p_line)
			{
				return;
			}
			p_line.Clamp = betweenAnchors;
			m_camera_updater = Activity.Run((Func<bool>)delegate
			{
				if (current_drone != drone)
				{
					return false;
				}
				if (mode != DroneCameraModeType.LineCamera)
				{
					return false;
				}
				if (!p_line)
				{
					return false;
				}
				Vector3 p_position = drone.position;
				Quaternion p_rotation = drone.transform.localRotation;
				p_line.Evaluate(drone.position, ref p_position, ref p_rotation, Vector3.up);
				UpdateOrbitLook(orbit, drone.position, p_position, p_rotation, p_speed);
				return true;
			}, 0f, false);
			m_camera_updater.late = true;
		}

		public void SetLineCamera(Drone p_drone, Transform p_anchor_0, Transform p_anchor_1, float p_speed = 3f)
		{
			drone = p_drone;
			mode = DroneCameraModeType.LineCamera;
			Drone current_drone = p_drone;
			StopUpdater();
			orbit.transition = (OrbitTransform.Transition)4352;
			m_camera_updater = Activity.Run((Func<bool>)delegate
			{
				if (current_drone != drone)
				{
					return false;
				}
				if (mode != DroneCameraModeType.LineCamera)
				{
					return false;
				}
				Vector3 p_position = drone.position;
				Quaternion p_rotation = drone.transform.localRotation;
				LineTransform.Evaluate(drone.position, p_anchor_0.position, p_anchor_1.position, 0f, ref p_position, ref p_rotation, Vector3.up);
				UpdateOrbitLook(orbit, drone.position, p_position, p_rotation, p_speed);
				return true;
			}, 0f, false);
			m_camera_updater.late = true;
		}

		public void SetLineCamera(Drone p_drone, Vector3 p_anchor_0, Vector3 p_anchor_1, float p_speed = 3f)
		{
			drone = p_drone;
			mode = DroneCameraModeType.LineCamera;
			Drone current_drone = p_drone;
			StopUpdater();
			orbit.transition = (OrbitTransform.Transition)4352;
			m_camera_updater = Activity.Run((Func<bool>)delegate
			{
				if (current_drone != drone)
				{
					return false;
				}
				if (mode != DroneCameraModeType.LineCamera)
				{
					return false;
				}
				Vector3 p_position = drone.position;
				Quaternion p_rotation = drone.transform.localRotation;
				LineTransform.Evaluate(drone.position, p_anchor_0, p_anchor_1, 0f, ref p_position, ref p_rotation, Vector3.up);
				UpdateOrbitLook(orbit, drone.position, p_position, p_rotation, p_speed);
				return true;
			}, 0f, false);
			m_camera_updater.late = true;
		}

		public void SetLOS(Drone p_drone, Transform p_anchor, float p_speed = 3f)
		{
			drone = p_drone;
			mode = DroneCameraModeType.LOS;
			Drone current_drone = p_drone;
			StopUpdater();
			m_camera_updater = Activity.Run((Func<bool>)delegate
			{
				if (current_drone != drone)
				{
					return false;
				}
				if (mode != DroneCameraModeType.LOS)
				{
					return false;
				}
				if (!drone)
				{
					return false;
				}
				if (!p_anchor)
				{
					return false;
				}
				Quaternion p_rotation = Quaternion.LookRotation(drone.position - p_anchor.position, Vector3.up);
				UpdateOrbitLook(orbit, drone.position, p_anchor.position, p_rotation, p_speed);
				return true;
			}, 0f, false);
			m_camera_updater.late = true;
		}

		public void SetLOS(Drone p_drone, Vector3 p_anchor, float p_speed = 3f)
		{
			drone = p_drone;
			mode = DroneCameraModeType.LOS;
			Drone current_drone = p_drone;
			StopUpdater();
			m_camera_updater = Activity.Run((Func<bool>)delegate
			{
				if (current_drone != drone)
				{
					return false;
				}
				if (mode != DroneCameraModeType.LOS)
				{
					return false;
				}
				Quaternion p_rotation = Quaternion.LookRotation(drone.position - p_anchor, Vector3.up);
				UpdateOrbitLook(orbit, drone.position, p_anchor, p_rotation, p_speed);
				return true;
			}, 0f, false);
		}

		public void SetLOSFast(Drone p_drone, Vector3 p_anchor)
		{
			drone = p_drone;
			mode = DroneCameraModeType.LOS;
			Drone current_drone = p_drone;
			StopUpdater();
			base.transform.position = p_anchor;
			m_camera_updater = Activity.Run((Func<bool>)delegate
			{
				if (current_drone != drone)
				{
					return false;
				}
				if (mode != DroneCameraModeType.LOS)
				{
					return false;
				}
				base.transform.LookAt(p_drone.transform);
				return true;
			}, 0f, false);
		}

		protected void OnDroneChange(Drone p_drone)
		{
			switch (m_mode)
			{
			case DroneCameraModeType.TPVBack:
			case DroneCameraModeType.TPVFree:
			case DroneCameraModeType.TPVSmooth:
			case DroneCameraModeType.TPVMissions:
				if ((bool)(follow.target = (p_drone ? p_drone.transform : null)))
				{
					follow.target = p_drone.transform;
				}
				if (p_drone != null)
				{
					p_drone.renderer.shadowsOnly = false;
				}
				break;
			case DroneCameraModeType.FPV:
				Activity.Run(delegate(Activity a)
				{
					if ((bool)p_drone && p_drone.ready)
					{
						follow.target = p_drone.body.frame.camera.pivot;
						p_drone.body.frame.camera.target = this;
						if (!inGarage)
						{
							p_drone.renderer.shadowsOnly = true;
						}
						a.Stop();
					}
				});
				break;
			case DroneCameraModeType.LOS:
			case DroneCameraModeType.LineCamera:
			case DroneCameraModeType.Follow:
			case DroneCameraModeType.FPVSmooth:
				break;
			}
		}

		protected void OnModeChange(DroneCameraModeType p_from, DroneCameraModeType p_to)
		{
			StopUpdater();
			orbit.transition = (OrbitTransform.Transition)4369;
			orbit.Refresh();
			if ((bool)fx)
			{
				fx.distortEnabled = false;
			}
			if ((bool)holeCollision)
			{
				holeCollision.enabled = false;
			}
			switch (p_from)
			{
			case DroneCameraModeType.FPV:
				follow.target = null;
				wasd.enabled = false;
				if ((bool)drone)
				{
					drone.body.frame.camera.target = null;
				}
				if (m_fpv_updater != null)
				{
					m_fpv_updater.Stop();
					m_fpv_updater = null;
				}
				break;
			case DroneCameraModeType.FPVSmooth:
				wasd.enabled = false;
				npsnap.enabled = true;
				break;
			case DroneCameraModeType.TPVBack:
			case DroneCameraModeType.TPVSmooth:
			case DroneCameraModeType.TPVMissions:
				follow.target = null;
				follow.offset = Vector3.zero;
				wasd.useJoystick = false;
				break;
			case DroneCameraModeType.TPVFree:
				follow.target = null;
				follow.offset = Vector3.zero;
				wasd.enabled = false;
				wasd.usePhysics = false;
				wasd.useJoystick = false;
				break;
			case DroneCameraModeType.Free:
				wasd.enabled = false;
				wasd.usePhysics = false;
				wasd.useJoystick = false;
				break;
			case DroneCameraModeType.Follow:
				follow.target = null;
				break;
			}
			switch (p_to)
			{
			case DroneCameraModeType.None:
			{
				wasd.enabled = false;
				wasd.usePhysics = false;
				wasd.useJoystick = false;
				follow.target = null;
				follow.flags = OrbitFollowInput.Flag.All;
				if ((bool)drone)
				{
					drone.body.frame.camera.target = null;
				}
				Rigidbody component = GetComponent<Rigidbody>();
				if (component != null)
				{
					component.constraints = RigidbodyConstraints.None;
					component.collisionDetectionMode = CollisionDetectionMode.Discrete;
					component.isKinematic = true;
				}
				if ((bool)npsnap)
				{
					npsnap.enabled = true;
				}
				break;
			}
			case DroneCameraModeType.FPV:
				orbit.constraint.Clear();
				orbit.transition = (OrbitTransform.Transition)4369;
				orbit.distance = -0.02f;
				orbit.angle = Vector3.zero;
				follow.target = null;
				follow.flags = OrbitFollowInput.Flag.All;
				wasd.enabled = false;
				wasd.usePhysics = false;
				if ((bool)fx)
				{
					fx.distortEnabled = lensDistortionAllowed;
				}
				_ = drone.fc.profile;
				if (m_fpv_updater != null)
				{
					m_fpv_updater.Stop();
				}
				m_fpv_updater = Activity.Run(delegate(Activity a)
				{
					if ((bool)drone && drone.hasBody && drone.body.hasFrame && (bool)drone.body.frame.camera)
					{
						follow.target = drone.body.frame.camera.pivot;
						drone.body.frame.camera.target = this;
						drone.renderer.shadowsOnly = true;
						a.Stop();
					}
				});
				if ((bool)npsnap)
				{
					npsnap.enabled = true;
				}
				break;
			case DroneCameraModeType.FPVSmooth:
				follow.target = drone.body.frame.camera.pivot;
				follow.flags = OrbitFollowInput.Flag.All;
				npsnap.enabled = false;
				main.nearClipPlane = 0.01f;
				wasd.enabled = false;
				wasd.usePhysics = false;
				wasd.useJoystick = false;
				orbit.transition = (OrbitTransform.Transition)8738;
				if (drone != null)
				{
					drone.renderer.shadowsOnly = true;
				}
				if ((bool)npsnap)
				{
					npsnap.enabled = true;
				}
				break;
			case DroneCameraModeType.TPVMissions:
				if ((bool)drone)
				{
					follow.target = drone.transform;
				}
				follow.offset = new Vector3(0f, 0.1f, 0f);
				follow.flags = (OrbitFollowInput.Flag)23;
				wasd.enabled = false;
				wasd.usePhysics = false;
				wasd.useJoystick = false;
				orbit.SetTransitionMask(OrbitTransform.TransitionMask.SmoothTPV);
				if (drone != null)
				{
					drone.renderer.shadowsOnly = false;
				}
				if ((bool)npsnap)
				{
					npsnap.enabled = false;
				}
				break;
			case DroneCameraModeType.TPVBack:
				if ((bool)drone)
				{
					follow.target = drone.transform;
				}
				follow.flags = (OrbitFollowInput.Flag)23;
				wasd.enabled = false;
				wasd.usePhysics = false;
				wasd.useJoystick = false;
				orbit.SetTransitionMask(OrbitTransform.TransitionMask.SmoothTPV);
				if (drone != null)
				{
					drone.renderer.shadowsOnly = false;
				}
				if ((bool)npsnap)
				{
					npsnap.enabled = false;
				}
				break;
			case DroneCameraModeType.TPVSmooth:
				follow.target = drone.transform;
				follow.flags = (OrbitFollowInput.Flag)23;
				wasd.enabled = false;
				wasd.usePhysics = false;
				wasd.useJoystick = false;
				orbit.transition = (OrbitTransform.Transition)8738;
				if (drone != null)
				{
					drone.renderer.shadowsOnly = false;
				}
				if ((bool)npsnap)
				{
					npsnap.enabled = false;
				}
				break;
			case DroneCameraModeType.TPVFree:
				wasd.enabled = true;
				wasd.usePhysics = false;
				wasd.useJoystick = false;
				if ((bool)drone)
				{
					follow.target = drone.transform;
				}
				follow.offset = new Vector3(0f, 0.1f, 0f);
				follow.flags = OrbitFollowInput.Flag.PositionXYZ;
				orbit.transition = (OrbitTransform.Transition)4386;
				if (drone != null)
				{
					drone.renderer.shadowsOnly = false;
				}
				if ((bool)npsnap)
				{
					npsnap.enabled = false;
				}
				break;
			case DroneCameraModeType.Free:
			{
				wasd.enabled = true;
				wasd.usePhysics = true;
				wasd.useJoystick = true;
				if ((bool)holeCollision)
				{
					holeCollision.enabled = true;
				}
				Rigidbody component2 = GetComponent<Rigidbody>();
				if (component2 != null)
				{
					component2.constraints = RigidbodyConstraints.FreezeRotation;
					component2.isKinematic = false;
					component2.collisionDetectionMode = CollisionDetectionMode.Continuous;
				}
				orbit.transition = (OrbitTransform.Transition)4386;
				orbit.speed.angle = 15f;
				wasd.sensitivity = 0.1f;
				if (drone != null)
				{
					drone.renderer.shadowsOnly = false;
				}
				if ((bool)npsnap)
				{
					npsnap.enabled = false;
				}
				break;
			}
			case DroneCameraModeType.LOS:
				orbit.constraint.Clear();
				orbit.distance = -0.1f;
				orbit.angle = Vector3.zero;
				wasd.enabled = false;
				wasd.usePhysics = false;
				orbit.transition = (OrbitTransform.Transition)8738;
				if (drone != null)
				{
					drone.renderer.shadowsOnly = false;
				}
				if ((bool)npsnap)
				{
					npsnap.enabled = false;
				}
				break;
			case DroneCameraModeType.Follow:
				follow.enabled = true;
				wasd.enabled = false;
				wasd.usePhysics = false;
				wasd.useJoystick = false;
				orbit.transition = (OrbitTransform.Transition)4386;
				if (drone != null)
				{
					drone.renderer.shadowsOnly = false;
				}
				if ((bool)npsnap)
				{
					npsnap.enabled = false;
				}
				break;
			}
			RefreshFOV();
		}

		private void StopUpdater()
		{
			if (m_fpv_updater != null)
			{
				m_fpv_updater.Stop();
				m_fpv_updater = null;
			}
			if (m_camera_updater != null)
			{
				m_camera_updater.Stop();
				m_camera_updater = null;
			}
		}

		private void UpdateOrbitLook(OrbitTransform p_orbit, Vector3 p_target, Vector3 p_position, Quaternion p_rotation, float p_speed)
		{
			float num = (unscaledTime ? Time.fixedUnscaledDeltaTime : Time.deltaTime);
			num = ((p_speed <= 0f) ? 1f : Mathf.Clamp01(num * p_speed));
			p_orbit.anchor = Vector3.Lerp(p_orbit.anchor, p_position, num);
			p_orbit.anchorRotation = Quaternion.Lerp(p_orbit.anchorRotation, p_rotation, num);
		}

		private void RefreshCaptureRT()
		{
			Camera camera = main;
			int num = Mathf.Max(Screen.width, 2);
			int num2 = Mathf.Max(Screen.height, 2);
			bool flag = true;
			bool num3 = (bool)camera && camera.allowHDR;
			RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGBFloat;
			RenderTextureFormat renderTextureFormat2 = (num3 ? renderTextureFormat : RenderTextureFormat.ARGB32);
			if ((bool)captureRT && captureRT.width == num && captureRT.height == num2 && captureRT.format == renderTextureFormat2)
			{
				flag = false;
			}
			if (flag)
			{
				if ((bool)captureRT)
				{
					captureRT.Release();
					UnityEngine.Object.DestroyImmediate(captureRT, allowDestroyingAssets: true);
					captureRT = null;
				}
				captureRT = new RenderTexture(num, num2, 24, renderTextureFormat2);
				captureRT.useMipMap = false;
			}
			captureRT.name = "StaticBackgroundRT";
		}

		public RenderTexture Capture()
		{
			RefreshCaptureRT();
			Camera camera = main;
			if (!camera)
			{
				return captureRT;
			}
			RenderTexture targetTexture = camera.targetTexture;
			camera.targetTexture = captureRT;
			camera.Render();
			camera.targetTexture = targetTexture;
			return captureRT;
		}

		public void CaptureAsync(Action<RenderTexture> p_callback)
		{
			RefreshCaptureRT();
			if (!main)
			{
				p_callback?.Invoke(captureRT);
			}
			else
			{
				StartCoroutine(Render(captureRT, p_callback));
			}
		}

		private IEnumerator Render(RenderTexture p_rt, Action<RenderTexture> p_callback)
		{
			yield return new WaitForEndOfFrame();
			Camera camera = ((cameras.Count >= 3) ? cameras[2] : null);
			if (camera != null)
			{
				RenderTexture targetTexture = camera.targetTexture;
				camera.targetTexture = p_rt;
				camera.Render();
				camera.targetTexture = targetTexture;
			}
			camera = ((cameras.Count >= 1) ? cameras[0] : null);
			if (camera != null)
			{
				RenderTexture targetTexture = camera.targetTexture;
				camera.targetTexture = p_rt;
				camera.Render();
				camera.targetTexture = targetTexture;
			}
			yield return new WaitForEndOfFrame();
			p_callback?.Invoke(p_rt);
		}

		protected void Update()
		{
		}

		private void OnDisable()
		{
			StopUpdater();
		}

		private void OnDestroy()
		{
			if ((bool)captureRT)
			{
				captureRT.Release();
				UnityEngine.Object.DestroyImmediate(captureRT, allowDestroyingAssets: true);
				captureRT = null;
			}
		}
	}
}
