using System;
using System.Collections;
using UnityEngine;

namespace drl.sim.thread
{
	public class AutoTune : MonoBehaviour
	{
		[Serializable]
		public struct PIDProfileAutoTune
		{
			public float P;

			public float I;

			public float D;

			public PIDProfileAutoTune(float p, float i, float d)
			{
				P = p;
				I = i;
				D = d;
			}
		}

		[Serializable]
		public class AutoTuneFinetune
		{
			public float PrecisionPoint = 3f;

			public float TimeOutsideLimit = 0.5f;

			public float TimeInside = 0.1f;

			public int SuccessRows = 3;

			public PIDProfileAutoTune InitialPID = new PIDProfileAutoTune(20f, 0f, 2f);
		}

		public bool TuneInProgress;

		[Space(20f)]
		[SerializeField]
		[Range(0f, 500f)]
		private float _joystickValue = 500f;

		[SerializeField]
		private float _targetRotation = 666f;

		[SerializeField]
		private AutoTuneFinetune RollTune;

		[SerializeField]
		private AutoTuneFinetune PitchTune;

		[SerializeField]
		private AutoTuneFinetune YawTune;

		private PIDProfileAutoTune PitchProfile;

		private PIDProfileAutoTune RollProfile;

		private PIDProfileAutoTune YawProfile;

		private PIDProfileAutoTune old_pitch;

		private PIDProfileAutoTune old_roll;

		private PIDProfileAutoTune old_yaw;

		public GyroscopeSensor Gyroscope;

		public DroneThreaded Forces;

		private int inputCounter;

		private int inputHoldTime = 200;

		private int minRotation = 15;

		private float currentFrameRot;

		private bool lookForIncrease;

		private bool autotune_pitching;

		private bool autotune_yawing;

		private bool autotune_rolling;

		private bool finishedRoll;

		private bool finishedPitch;

		private bool finishedYaw;

		private int SuccessRolls;

		private int SuccessPitch;

		private int SuccessYaws;

		private Coroutine c_autoTune;

		private float timeOutside;

		private float timeInside;

		private float originalRefresRate = -1f;

		public bool autotuneFailed;

		private float autotuneTimeRoll;

		private float autotuneTimePitch;

		private float autotuneTimeYaw;

		private int resetCount;

		private bool resetting;

		public bool FinishedRoll => finishedRoll;

		public bool FinishedPitch => finishedPitch;

		public bool FinishedYaw => finishedYaw;

		private void Start()
		{
			PitchProfile = default(PIDProfileAutoTune);
			RollProfile = default(PIDProfileAutoTune);
			YawProfile = default(PIDProfileAutoTune);
			if (Forces == null)
			{
				Forces = GetComponent<DroneThreaded>();
			}
			if (Gyroscope == null)
			{
				Gyroscope = Forces.Gyroscope;
			}
		}

		public void AutoTune_Roll_Down()
		{
			inputCounter++;
			Forces.mixer.JoystickData.Roll = 0f - _joystickValue;
			Forces.HandCalculatedAngularVelocity = new Vector3(0f, 0f, _targetRotation * ((float)Math.PI / 180f));
			if (inputCounter > inputHoldTime)
			{
				autotune_rolling = false;
				inputCounter = 0;
			}
		}

		public void AutoTune_Pitch_Down()
		{
			inputCounter++;
			Forces.mixer.JoystickData.Pitch = _joystickValue;
			Forces.HandCalculatedAngularVelocity = new Vector3(_targetRotation * ((float)Math.PI / 180f), 0f, 0f);
			if (inputCounter > inputHoldTime)
			{
				autotune_pitching = false;
				inputCounter = 0;
			}
		}

		public void AutoTune_Yaw_Down()
		{
			inputCounter++;
			Forces.mixer.JoystickData.Yaw = _joystickValue;
			Forces.HandCalculatedAngularVelocity = new Vector3(0f, _targetRotation * ((float)Math.PI / 180f), 0f);
			if (inputCounter > inputHoldTime)
			{
				autotune_yawing = false;
				inputCounter = 0;
			}
		}

		public void StartAutoTune()
		{
			if (c_autoTune != null)
			{
				StopCoroutine(c_autoTune);
			}
			StartCoroutine(AutotuneSafeguard());
		}

		private IEnumerator AutotuneSafeguard()
		{
			TuneInProgress = true;
			yield return new WaitForSeconds(1f);
			finishedRoll = true;
			yield return new WaitForSeconds(1f);
			finishedPitch = true;
			yield return new WaitForSeconds(1f);
			finishedYaw = true;
			float b = Forces.Drone.body.frame.propLimit * 10f + 20f + (float)UnityEngine.Random.Range(-5, 5);
			float num = Forces.Drone.body.frame.propLimit * 10f + 10f + (float)UnityEngine.Random.Range(-5, 5);
			float num2 = Forces.Drone.body.frame.propLimit * 10f + 10f + (float)UnityEngine.Random.Range(-5, 5);
			RollProfile.P = Mathf.Floor(Mathf.Lerp(Forces.Drone.profile.rollPID.p, num2, 0.5f));
			RollProfile.I = Mathf.Floor(Mathf.Clamp(UnityEngine.Random.Range(-8, 2), 0f, 2.1f));
			RollProfile.D = Mathf.Floor(Mathf.Lerp(Forces.Drone.profile.rollPID.d, num2 + (float)UnityEngine.Random.Range(-5, 5), 0.5f));
			PitchProfile.P = Mathf.Floor(Mathf.Lerp(Forces.Drone.profile.pitchPID.p, num, 0.5f));
			PitchProfile.I = RollProfile.I;
			PitchProfile.D = Mathf.Floor(Mathf.Lerp(Forces.Drone.profile.pitchPID.d, num + (float)UnityEngine.Random.Range(-5, 5), 0.5f));
			YawProfile.P = Mathf.Floor(Mathf.Lerp(Forces.Drone.profile.yawPID.p, b, 0.5f));
			YawProfile.I = 0f;
			YawProfile.D = 0f;
			autotuneFailed = false;
			TuneInProgress = false;
			Forces.Drone.profile.rollPID.p = RollProfile.P;
			Forces.Drone.profile.rollPID.i = RollProfile.I;
			Forces.Drone.profile.rollPID.d = RollProfile.D;
			Forces.Drone.profile.pitchPID.p = PitchProfile.P;
			Forces.Drone.profile.pitchPID.i = PitchProfile.I;
			Forces.Drone.profile.pitchPID.d = PitchProfile.D;
			Forces.Drone.profile.yawPID.p = YawProfile.P;
			Forces.Drone.profile.yawPID.i = YawProfile.I;
			Forces.Drone.profile.yawPID.d = YawProfile.D;
		}

		private IEnumerator WatchForAutoTune_Coroutine()
		{
			bool autoTuneRunning_previousFrame = false;
			while (true)
			{
				yield return null;
				if (autoTuneRunning_previousFrame != TuneInProgress)
				{
					if (TuneInProgress)
					{
						Debug.Log("Tunning started!");
					}
					else if (!autotuneFailed)
					{
						Debug.Log("Auto-tune done. Total: " + (autotuneTimeYaw + autotuneTimeRoll + autotuneTimePitch).ToString("F0") + "s.");
						originalRefresRate = -1f;
						yield return new WaitForSeconds(0.5f);
						Forces.Reset();
						Forces.Drone.profile.rollPID.p = RollProfile.P;
						Forces.Drone.profile.rollPID.i = RollProfile.I;
						Forces.Drone.profile.rollPID.d = RollProfile.D;
						Forces.Drone.profile.pitchPID.p = PitchProfile.P;
						Forces.Drone.profile.pitchPID.i = PitchProfile.I;
						Forces.Drone.profile.pitchPID.d = PitchProfile.D;
						Forces.Drone.profile.yawPID.p = YawProfile.P;
						Forces.Drone.profile.yawPID.i = YawProfile.I;
						Forces.Drone.profile.yawPID.d = YawProfile.D;
					}
					else
					{
						Debug.LogWarning("Tunning failed!");
						originalRefresRate = -1f;
						if (c_autoTune != null)
						{
							StopCoroutine(c_autoTune);
						}
						autotuneFailed = false;
						autotune_rolling = false;
						autotune_pitching = false;
						autotune_yawing = false;
						Forces.Reset();
						Forces.Drone.profile.rollPID.p = old_roll.P;
						Forces.Drone.profile.rollPID.i = old_roll.I;
						Forces.Drone.profile.rollPID.d = old_roll.D;
						Forces.Drone.profile.pitchPID.p = old_pitch.P;
						Forces.Drone.profile.pitchPID.i = old_pitch.I;
						Forces.Drone.profile.pitchPID.d = old_pitch.D;
						Forces.Drone.profile.yawPID.p = old_yaw.P;
						Forces.Drone.profile.yawPID.i = old_yaw.I;
						Forces.Drone.profile.yawPID.d = old_yaw.D;
					}
				}
				autoTuneRunning_previousFrame = TuneInProgress;
			}
		}

		private IEnumerator AutoTune_Coroutine()
		{
			TuneInProgress = true;
			old_pitch = new PIDProfileAutoTune(Forces.Drone.profile.pitchPID.p, Forces.Drone.profile.pitchPID.i, Forces.Drone.profile.pitchPID.d);
			old_roll = new PIDProfileAutoTune(Forces.Drone.profile.rollPID.p, Forces.Drone.profile.rollPID.i, Forces.Drone.profile.rollPID.d);
			old_yaw = new PIDProfileAutoTune(Forces.Drone.profile.yawPID.p, Forces.Drone.profile.yawPID.i, Forces.Drone.profile.yawPID.d);
			Vector3 pos = Forces.VirtualPoint;
			Quaternion rot = Forces.VirtualRotation;
			Vector3 spd = Forces.HandCalculatedVelocity;
			Vector3 ang = Forces.HandCalculatedAngularVelocity;
			Forces.Reset();
			finishedRoll = false;
			finishedPitch = false;
			finishedYaw = false;
			SuccessPitch = 0;
			SuccessRolls = 0;
			SuccessYaws = 0;
			Forces.Drone.profile.rollPID.p = 0f;
			Forces.Drone.profile.rollPID.i = 0f;
			Forces.Drone.profile.rollPID.d = 0f;
			Forces.Drone.profile.pitchPID.p = 0f;
			Forces.Drone.profile.pitchPID.i = 0f;
			Forces.Drone.profile.pitchPID.d = 0f;
			Forces.Drone.profile.yawPID.p = 0f;
			Forces.Drone.profile.yawPID.i = 0f;
			Forces.Drone.profile.yawPID.d = 0f;
			autotuneTimeRoll = 0f;
			autotuneTimePitch = 0f;
			autotuneTimeYaw = 0f;
			autotune_rolling = true;
			yield return null;
			Debug.Log("Starting autotune process...");
			while (!finishedRoll)
			{
				yield return null;
				autotuneTimeRoll += Time.deltaTime;
			}
			autotune_pitching = true;
			yield return null;
			while (!finishedPitch)
			{
				yield return null;
				autotuneTimePitch += Time.deltaTime;
			}
			autotune_yawing = true;
			yield return null;
			while (!finishedYaw)
			{
				yield return null;
				autotuneTimeYaw += Time.deltaTime;
			}
			Forces.VirtualPoint = pos;
			Forces.VirtualRotation = rot;
			Forces.HandCalculatedVelocity = spd;
			Forces.HandCalculatedAngularVelocity = ang;
			TuneInProgress = false;
			Debug.Log("Autotune process complete.");
		}

		public void Refresh_Roll(float t_dt)
		{
			if (resetting)
			{
				Autotune_Reset();
			}
			else
			{
				if (finishedRoll)
				{
					return;
				}
				if (originalRefresRate == -1f)
				{
					originalRefresRate = t_dt;
				}
				if (autotune_rolling)
				{
					AutoTune_Roll_Down();
				}
				if (Forces.Drone.profile.rollPID.p == 0f)
				{
					Forces.Drone.profile.rollPID.p = RollTune.InitialPID.P;
					Forces.Drone.profile.rollPID.i = RollTune.InitialPID.I;
					Forces.Drone.profile.rollPID.d = RollTune.InitialPID.D;
				}
				currentFrameRot = Gyroscope.Velocity.z;
				if (currentFrameRot > (float)minRotation)
				{
					timeOutside = 0f;
					timeInside = 0f;
					lookForIncrease = true;
				}
				if (!(currentFrameRot <= 10f) || !lookForIncrease)
				{
					return;
				}
				timeOutside += t_dt;
				if (timeOutside > RollTune.TimeOutsideLimit)
				{
					Forces.Drone.profile.rollPID.p += 1f;
					SuccessRolls = 0;
					resetting = true;
					lookForIncrease = false;
					return;
				}
				if ((double)currentFrameRot < -0.01)
				{
					if (SuccessRolls > 0)
					{
						SuccessRolls = 0;
						resetting = true;
						lookForIncrease = false;
					}
					else
					{
						Forces.Drone.profile.rollPID.d += 1f;
						SuccessRolls = 0;
						resetting = true;
						lookForIncrease = false;
					}
					return;
				}
				if (currentFrameRot < RollTune.PrecisionPoint)
				{
					timeInside += t_dt;
				}
				if (timeInside > RollTune.TimeInside)
				{
					resetting = true;
					lookForIncrease = false;
					SuccessRolls++;
					if (SuccessRolls >= 3)
					{
						finishedRoll = true;
						RollProfile.P = Forces.Drone.profile.rollPID.p;
						RollProfile.I = Forces.Drone.profile.rollPID.i;
						RollProfile.D = Forces.Drone.profile.rollPID.d;
						Forces.Drone.profile.rollPID.p = 0f;
						Forces.Drone.profile.rollPID.i = 0f;
						Forces.Drone.profile.rollPID.d = 0f;
					}
				}
			}
		}

		public void Refresh_Pitch(float t_dt)
		{
			if (resetting)
			{
				Autotune_Reset();
			}
			else
			{
				if (finishedPitch || !finishedRoll)
				{
					return;
				}
				if (originalRefresRate == -1f)
				{
					originalRefresRate = t_dt;
				}
				if (autotune_pitching)
				{
					AutoTune_Pitch_Down();
				}
				if (Forces.Drone.profile.pitchPID.p == 0f)
				{
					Forces.Drone.profile.pitchPID.p = PitchTune.InitialPID.P;
					Forces.Drone.profile.pitchPID.i = PitchTune.InitialPID.I;
					Forces.Drone.profile.pitchPID.d = PitchTune.InitialPID.D;
				}
				currentFrameRot = Gyroscope.Velocity.x;
				if (currentFrameRot > (float)minRotation)
				{
					timeOutside = 0f;
					timeInside = 0f;
					lookForIncrease = true;
				}
				if (!(currentFrameRot <= 10f) || !lookForIncrease)
				{
					return;
				}
				timeOutside += t_dt;
				if (timeOutside > PitchTune.TimeOutsideLimit)
				{
					Forces.Drone.profile.pitchPID.p += 1f;
					SuccessPitch = 0;
					resetting = true;
					lookForIncrease = false;
					return;
				}
				if ((double)currentFrameRot < -0.01)
				{
					if (SuccessPitch > 0)
					{
						SuccessPitch = 0;
						resetting = true;
						lookForIncrease = false;
					}
					else
					{
						Forces.Drone.profile.pitchPID.d += 1f;
						SuccessPitch = 0;
						resetting = true;
						lookForIncrease = false;
					}
					return;
				}
				if (currentFrameRot < PitchTune.PrecisionPoint)
				{
					timeInside += t_dt;
				}
				if (timeInside > PitchTune.TimeInside)
				{
					resetting = true;
					lookForIncrease = false;
					SuccessPitch++;
					if (SuccessPitch >= 3)
					{
						finishedPitch = true;
						PitchProfile.P = Forces.Drone.profile.pitchPID.p;
						PitchProfile.I = Forces.Drone.profile.pitchPID.i;
						PitchProfile.D = Forces.Drone.profile.pitchPID.d;
						Forces.Drone.profile.pitchPID.p = 0f;
						Forces.Drone.profile.pitchPID.i = 0f;
						Forces.Drone.profile.pitchPID.d = 0f;
					}
				}
			}
		}

		public void Refresh_Yaw(float t_dt)
		{
			if (resetting)
			{
				Autotune_Reset();
			}
			else
			{
				if (finishedYaw || !finishedPitch || !finishedRoll)
				{
					return;
				}
				if (originalRefresRate == -1f)
				{
					originalRefresRate = t_dt;
				}
				if (autotune_yawing)
				{
					AutoTune_Yaw_Down();
				}
				if (Forces.Drone.profile.yawPID.p == 0f)
				{
					Forces.Drone.profile.yawPID.p = YawTune.InitialPID.P;
					Forces.Drone.profile.yawPID.i = YawTune.InitialPID.I;
					Forces.Drone.profile.yawPID.d = YawTune.InitialPID.D;
				}
				currentFrameRot = Gyroscope.Velocity.y;
				if (currentFrameRot > (float)minRotation)
				{
					timeOutside = 0f;
					timeInside = 0f;
					lookForIncrease = true;
				}
				if (!(currentFrameRot <= 10f) || !lookForIncrease)
				{
					return;
				}
				timeOutside += t_dt;
				if (timeOutside > YawTune.TimeOutsideLimit)
				{
					Forces.Drone.profile.yawPID.p += 1f;
					SuccessPitch = 0;
					resetting = true;
					lookForIncrease = false;
					return;
				}
				if ((double)currentFrameRot < -0.01)
				{
					if (SuccessYaws > 0)
					{
						SuccessYaws = 0;
						resetting = true;
						lookForIncrease = false;
					}
					else
					{
						Forces.Drone.profile.yawPID.d += 1f;
						SuccessYaws = 0;
						resetting = true;
						lookForIncrease = false;
					}
					return;
				}
				if (currentFrameRot < YawTune.PrecisionPoint)
				{
					timeInside += t_dt;
				}
				if (timeInside > YawTune.TimeInside)
				{
					resetting = true;
					lookForIncrease = false;
					SuccessYaws++;
					if (SuccessYaws >= 3)
					{
						finishedYaw = true;
						YawProfile.P = Forces.Drone.profile.yawPID.p;
						YawProfile.I = Forces.Drone.profile.yawPID.i;
						YawProfile.D = Forces.Drone.profile.yawPID.d;
						Forces.Drone.profile.yawPID.p = 0f;
						Forces.Drone.profile.yawPID.i = 0f;
						Forces.Drone.profile.yawPID.d = 0f;
					}
				}
			}
		}

		private void AutoTuneFailed()
		{
			autotuneFailed = true;
			TuneInProgress = false;
		}

		public void Autotune_Reset()
		{
			resetCount++;
			if (resetCount > 10)
			{
				resetting = false;
				resetCount = 0;
				if (!finishedRoll)
				{
					autotune_rolling = true;
				}
				else if (!finishedPitch)
				{
					autotune_pitching = true;
				}
				else if (!finishedYaw)
				{
					autotune_yawing = true;
				}
			}
		}
	}
}
