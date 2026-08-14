using System;
using System.Collections;
using UnityEngine;

namespace drl.sim.thread
{
	public class DroneThreadedMixer : MonoBehaviour
	{
		public enum DroneAxis
		{
			roll = 0,
			pitch = 1,
			yaw = 2
		}

		[Serializable]
		public struct BetaFlightPIDConstants
		{
			public DroneAxis DroneAxis;

			public int P;

			public int I;

			public int D;

			public int F;

			public void Copy(BetaFlightPIDConstants betaFlightPIDConstants)
			{
				DroneAxis = betaFlightPIDConstants.DroneAxis;
				P = betaFlightPIDConstants.P;
				I = betaFlightPIDConstants.I;
				D = betaFlightPIDConstants.D;
				F = betaFlightPIDConstants.F;
			}

			public bool HasChanged(BetaFlightPIDConstants betaFlightPIDConstants)
			{
				if (DroneAxis != betaFlightPIDConstants.DroneAxis)
				{
					return true;
				}
				if (P != betaFlightPIDConstants.P)
				{
					return true;
				}
				if (I != betaFlightPIDConstants.I)
				{
					return true;
				}
				if (D != betaFlightPIDConstants.D)
				{
					return true;
				}
				if (F != betaFlightPIDConstants.F)
				{
					return true;
				}
				return false;
			}
		}

		[Serializable]
		public struct InputJoystickData
		{
			[Range(-500f, 500f)]
			public float Roll;

			[Range(-500f, 500f)]
			public float Pitch;

			[Range(-500f, 500f)]
			public float Yaw;

			[Range(1000f, 2000f)]
			public float Throttle;
		}

		[Serializable]
		public struct NameConvention
		{
			public float RearRight;

			public float FrontRight;

			public float RearLeft;

			public float FrontLeft;
		}

		[Serializable]
		public struct FrameEsc
		{
			public DroneESC RearRight;

			public DroneESC FrontRight;

			public DroneESC RearLeft;

			public DroneESC FrontLeft;
		}

		[Serializable]
		public struct ESCNormalized
		{
			public NameConvention Esc;
		}

		[Serializable]
		public struct MotorReading
		{
			public NameConvention Motors;
		}

		[Serializable]
		public struct SetPointReading
		{
			public float Roll;

			public float Pitch;

			public float Yaw;
		}

		[Serializable]
		public class PIDReading
		{
			public float P;

			public float I;

			public float D;

			public float Control;

			public PIDReading()
			{
				P = 0f;
				I = 0f;
				D = 0f;
				Control = 0f;
			}

			public PIDReading(float[] pids)
			{
				P = pids[0];
				I = pids[1];
				D = pids[2];
				Control = pids[3];
			}

			public void Set(float[] pids)
			{
				P = pids[0];
				I = pids[1];
				D = pids[2];
				Control = pids[3];
			}
		}

		[Serializable]
		public struct RatesStructure
		{
			public int Roll;

			public int Pitch;

			public int Yaw;

			public bool Equals(RatesStructure other)
			{
				if (Roll == other.Roll && Pitch == other.Pitch)
				{
					return Yaw == other.Yaw;
				}
				return false;
			}

			public bool Equals(DroneProfileData.RatesStructure other)
			{
				if (Roll == other.Roll && Pitch == other.Pitch)
				{
					return Yaw == other.Yaw;
				}
				return false;
			}

			public void Set(DroneProfileData.RatesStructure other)
			{
				Roll = other.Roll;
				Pitch = other.Pitch;
				Yaw = other.Yaw;
			}

			public void Set(RatesStructure other)
			{
				Roll = other.Roll;
				Pitch = other.Pitch;
				Yaw = other.Yaw;
			}
		}

		private Rigidbody Drone;

		private Drone droneComponent;

		public DroneReceiver Receiver;

		[Header("________________________________________")]
		public GyroscopeSensor Gyroscope;

		public DroneIntertial DroneIntertial;

		[Header("________________________________________")]
		public RatesStructure SuperRates;

		public RatesStructure RcExpoRates;

		public RatesStructure RcRates;

		[Header("________________________________________")]
		public BetaFlightPIDConstants RollConst;

		public BetaFlightPIDConstants PitchConst;

		public BetaFlightPIDConstants YawConst;

		public BetaFlightPIDConstants LevelConst;

		private BetaFlightPIDConstants RollConst_previousFrame;

		private BetaFlightPIDConstants PitchConst_previousFrame;

		private BetaFlightPIDConstants YawConst_previousFrame;

		private BetaFlightPIDConstants LevelConst_previousFrame;

		public BetaFlightPIDConstants RollConst_Reading;

		public BetaFlightPIDConstants PitchConst_Reading;

		public BetaFlightPIDConstants YawConst_Reading;

		public BetaFlightPIDConstants LevelConst_Reading;

		[Space(5f)]
		public InputJoystickData JoystickData;

		[Header("________________________________________")]
		public PIDReading RollReading;

		public PIDReading PitchReading;

		public PIDReading YawReading;

		[Space(5f)]
		public SetPointReading SetPoint;

		public FrameEsc Frame_ESC;

		public ESCNormalized ESCs;

		public MotorReading Motors;

		private int[] debugRollReading = new int[3];

		private float[] gyroFromPid = new float[3];

		private float[] debugFromPid = new float[3];

		private short[] _signals = new short[4];

		private float[] _gyro = new float[3];

		private float[] rollReading = new float[4];

		private float[] pitchReading = new float[4];

		private float[] yawReading = new float[4];

		private int[] superRates = new int[3];

		private int[] rcExpoRates = new int[3];

		private int[] rcRates = new int[3];

		private byte[] setSuperRates = new byte[3];

		private byte[] setRcExpoRates = new byte[3];

		private byte[] setRcRates = new byte[3];

		private float[] motors = new float[4];

		private float[] setPoint = new float[3];

		private int[] rollPidValue = new int[4];

		private int[] pitchPidValue = new int[4];

		private int[] yawPidValue = new int[4];

		private int[] levelPidValue = new int[4];

		private PIDVector rc;

		private PIDVector pc;

		private PIDVector yc;

		private PIDVector lc;

		private ushort lastMinSignal;

		public static bool CompareRatesStructures(DroneProfileData.RatesStructure p_a, RatesStructure p_b)
		{
			if (p_a.Roll != p_b.Roll)
			{
				return false;
			}
			if (p_a.Pitch != p_b.Pitch)
			{
				return false;
			}
			if (p_a.Yaw != p_b.Yaw)
			{
				return false;
			}
			return true;
		}

		private void Start()
		{
			debugRollReading = new int[3];
			Drone = base.transform.parent.parent.GetComponent<Rigidbody>();
			droneComponent = Drone.GetComponent<Drone>();
			CreateSensors();
			SuperRates = default(RatesStructure);
			RcExpoRates = default(RatesStructure);
			RcRates = default(RatesStructure);
			RollReading = new PIDReading();
			PitchReading = new PIDReading();
			YawReading = new PIDReading();
			Frame_ESC = default(FrameEsc);
			ESCs = default(ESCNormalized);
			Motors = default(MotorReading);
			SetPoint = default(SetPointReading);
			FlightController.InitializeFlightController();
			StartCoroutine(FindFrameESCs());
		}

		private IEnumerator FindFrameESCs()
		{
			while (droneComponent == null)
			{
				droneComponent = Drone.GetComponent<Drone>();
				yield return null;
			}
			while (!droneComponent.hasBody || !droneComponent.body.hasFrame || droneComponent.body.frame.escs == null || droneComponent.body.frame.escs.Count == 0)
			{
				yield return null;
			}
			Frame_ESC.RearRight = droneComponent.body.frame.escs[2];
			Frame_ESC.FrontRight = droneComponent.body.frame.escs[1];
			Frame_ESC.RearLeft = droneComponent.body.frame.escs[3];
			Frame_ESC.FrontLeft = droneComponent.body.frame.escs[0];
			Receiver = Drone.transform.GetComponentInChildren<DroneReceiver>();
		}

		public void Loop(float deltaTime)
		{
			if ((bool)Frame_ESC.RearRight && (bool)Frame_ESC.FrontRight && (bool)Frame_ESC.RearLeft && (bool)Frame_ESC.FrontLeft)
			{
				PID_Logic(deltaTime);
				Frame_ESC.RearRight.input = ESCs.Esc.RearRight;
				Frame_ESC.FrontRight.input = ESCs.Esc.FrontRight;
				Frame_ESC.RearLeft.input = ESCs.Esc.RearLeft;
				Frame_ESC.FrontLeft.input = ESCs.Esc.FrontLeft;
			}
		}

		private void PID_Logic(float deltaTime)
		{
			JoystickData.Throttle = Receiver.signal.throttle * 1000f + 1000f;
			JoystickData.Roll = Receiver.signal.roll * 500f;
			JoystickData.Pitch = Receiver.signal.pitch * 500f;
			JoystickData.Yaw = Receiver.signal.yaw * 500f;
			_signals[0] = (short)JoystickData.Roll;
			_signals[1] = (short)JoystickData.Pitch;
			_signals[2] = (short)JoystickData.Yaw;
			_signals[3] = (short)JoystickData.Throttle;
			FlightController.SetSignals(_signals);
			_gyro[0] = 0f - Gyroscope.Velocity.z;
			_gyro[1] = Gyroscope.Velocity.x;
			_gyro[2] = Gyroscope.Velocity.y;
			FlightController.SetGyro(_gyro);
			FlightController.SetAccelerometer((short)((0f - Gyroscope.Acceleration.z) * 100f), (short)(Gyroscope.Acceleration.x * 100f), (short)(Gyroscope.Acceleration.y * 100f));
			FlightController.DoPidLoop(deltaTime);
			FlightController.GetPid(0, rollReading);
			FlightController.GetPid(1, pitchReading);
			FlightController.GetPid(2, yawReading);
			RollReading.Set(rollReading);
			PitchReading.Set(pitchReading);
			YawReading.Set(yawReading);
			FlightController.GetSuperRates(superRates);
			SuperRates.Roll = superRates[0];
			SuperRates.Pitch = superRates[1];
			SuperRates.Yaw = superRates[2];
			FlightController.GetRcExpoRates(rcExpoRates);
			RcExpoRates.Roll = rcExpoRates[0];
			RcExpoRates.Pitch = rcExpoRates[1];
			RcExpoRates.Yaw = rcExpoRates[2];
			FlightController.GetRcRates(rcRates);
			RcRates.Roll = rcRates[0];
			RcRates.Pitch = rcRates[1];
			RcRates.Yaw = rcRates[2];
			FlightController.GetMotors(motors);
			Motors.Motors.RearRight = motors[0];
			Motors.Motors.FrontRight = motors[1];
			Motors.Motors.RearLeft = motors[2];
			Motors.Motors.FrontLeft = motors[3];
			FlightController.GetDebugValues(debugFromPid, debugFromPid, setPoint, debugRollReading, gyroFromPid);
			SetPoint.Roll = setPoint[0];
			SetPoint.Pitch = setPoint[1];
			SetPoint.Yaw = setPoint[2];
			ESCs.Esc.RearRight = (Motors.Motors.RearRight - 1000f) / 1000f;
			ESCs.Esc.FrontRight = (Motors.Motors.FrontRight - 1000f) / 1000f;
			ESCs.Esc.RearLeft = (Motors.Motors.RearLeft - 1000f) / 1000f;
			ESCs.Esc.FrontLeft = (Motors.Motors.FrontLeft - 1000f) / 1000f;
		}

		private void CreateSensors()
		{
			Gyroscope = Drone.GetComponent<DroneThreaded>().Gyroscope;
			DroneIntertial = Drone.GetComponent<DroneThreaded>().Intertial;
		}

		private void Update()
		{
			if ((bool)droneComponent.physics)
			{
				DroneProfileData profile = droneComponent.profile;
				bool flag = false;
				if (!flag && !CompareRatesStructures(profile.SuperRates, SuperRates))
				{
					flag = true;
				}
				if (!flag && !CompareRatesStructures(profile.RcExpoRates, RcExpoRates))
				{
					flag = true;
				}
				if (!flag && !CompareRatesStructures(profile.RcRates, RcRates))
				{
					flag = true;
				}
				if (flag)
				{
					SuperRates.Set(droneComponent.profile.SuperRates);
					RcExpoRates.Set(droneComponent.profile.RcExpoRates);
					RcRates.Set(droneComponent.profile.RcRates);
					setSuperRates[0] = (byte)SuperRates.Roll;
					setSuperRates[1] = (byte)SuperRates.Pitch;
					setSuperRates[2] = (byte)SuperRates.Yaw;
					FlightController.SetSuperRates(setSuperRates);
					setRcExpoRates[0] = (byte)RcExpoRates.Roll;
					setRcExpoRates[1] = (byte)RcExpoRates.Pitch;
					setRcExpoRates[2] = (byte)RcExpoRates.Yaw;
					FlightController.SetRcExpoRates(setRcExpoRates);
					setRcRates[0] = (byte)RcRates.Roll;
					setRcRates[1] = (byte)RcRates.Pitch;
					setRcRates[2] = (byte)RcRates.Yaw;
					FlightController.SetRcRates(setRcRates);
				}
				if (lastMinSignal != (ushort)(droneComponent.profile.minSignal * 1000f + 1000f))
				{
					lastMinSignal = (ushort)(droneComponent.profile.minSignal * 1000f + 1000f);
					FlightController.MinThrottle = lastMinSignal;
				}
				PitchConst.P = (int)droneComponent.profile.pitchPID.p;
				PitchConst.I = (int)droneComponent.profile.pitchPID.i;
				PitchConst.D = (int)droneComponent.profile.pitchPID.d;
				PitchConst.F = (int)droneComponent.profile.pitchFF;
				RollConst.P = (int)droneComponent.profile.rollPID.p;
				RollConst.I = (int)droneComponent.profile.rollPID.i;
				RollConst.D = (int)droneComponent.profile.rollPID.d;
				RollConst.F = (int)droneComponent.profile.rollFF;
				YawConst.P = (int)droneComponent.profile.yawPID.p;
				YawConst.I = (int)droneComponent.profile.yawPID.i;
				YawConst.D = (int)droneComponent.profile.yawPID.d;
				YawConst.F = (int)droneComponent.profile.yawFF;
				LevelConst.P = (int)droneComponent.profile.levelPID.p;
				LevelConst.I = (int)droneComponent.profile.levelPID.i;
				LevelConst.D = (int)droneComponent.profile.levelPID.d;
				rc.Set((byte)RollConst.P, (byte)RollConst.I, (byte)RollConst.D, (byte)RollConst.F);
				pc.Set((byte)PitchConst.P, (byte)PitchConst.I, (byte)PitchConst.D, (byte)PitchConst.F);
				yc.Set((byte)YawConst.P, (byte)YawConst.I, (byte)YawConst.D, (byte)YawConst.F);
				lc.Set((byte)LevelConst.P, (byte)LevelConst.I, (byte)LevelConst.D, 0);
				if (RollConst_previousFrame.HasChanged(RollConst))
				{
					FlightController.SetPidConstants(rc, pc, yc, lc);
					FlightController.GetConstants(rollPidValue, 0);
					RollConst_Reading.P = rollPidValue[0];
					RollConst_Reading.I = rollPidValue[1];
					RollConst_Reading.D = rollPidValue[2];
					RollConst_Reading.F = rollPidValue[3];
				}
				if (PitchConst_previousFrame.HasChanged(PitchConst))
				{
					FlightController.SetPidConstants(rc, pc, yc, lc);
					FlightController.GetConstants(pitchPidValue, 1);
					PitchConst_Reading.P = pitchPidValue[0];
					PitchConst_Reading.I = pitchPidValue[1];
					PitchConst_Reading.D = pitchPidValue[2];
					PitchConst_Reading.F = pitchPidValue[3];
				}
				if (YawConst_previousFrame.HasChanged(YawConst))
				{
					FlightController.SetPidConstants(rc, pc, yc, lc);
					FlightController.GetConstants(yawPidValue, 2);
					YawConst_Reading.P = yawPidValue[0];
					YawConst_Reading.I = yawPidValue[1];
					YawConst_Reading.D = yawPidValue[2];
					YawConst_Reading.F = yawPidValue[3];
				}
				if (LevelConst_previousFrame.HasChanged(LevelConst))
				{
					FlightController.SetPidConstants(rc, pc, yc, lc);
					FlightController.GetConstants(levelPidValue, 3);
					LevelConst_Reading.P = levelPidValue[0];
					LevelConst_Reading.I = levelPidValue[1];
					LevelConst_Reading.D = levelPidValue[2];
					LevelConst_Reading.F = levelPidValue[3];
				}
				RollConst_previousFrame.Copy(RollConst);
				PitchConst_previousFrame.Copy(PitchConst);
				YawConst_previousFrame.Copy(YawConst);
				LevelConst_previousFrame.Copy(LevelConst);
			}
		}
	}
}
