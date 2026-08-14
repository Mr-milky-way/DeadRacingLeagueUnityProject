using System;
using System.Runtime.InteropServices;

namespace drl.sim.thread
{
	public class Betaflight_3_4 : FlightControllerInterface
	{
		private const string libraryName = "bf_3_4";

		public override string Version => Marshal.PtrToStringAnsi(getVersion());

		public override bool Airmode
		{
			get
			{
				return FeatureIsEnabled(4194304u);
			}
			set
			{
				if (value)
				{
					FeatureEnable(4194304u);
				}
				else
				{
					FeatureDisable(4194304u);
				}
			}
		}

		public override bool Antigravity
		{
			get
			{
				return FeatureIsEnabled(268435456u);
			}
			set
			{
				if (value)
				{
					FeatureEnable(268435456u);
				}
				else
				{
					FeatureDisable(268435456u);
				}
			}
		}

		public override bool DynamicFilter
		{
			get
			{
				return FeatureIsEnabled(536870912u);
			}
			set
			{
				if (value)
				{
					FeatureEnable(536870912u);
				}
				else
				{
					FeatureDisable(536870912u);
				}
			}
		}

		public override byte LevelAngleLimit
		{
			get
			{
				return GetLevelAngleLimit();
			}
			set
			{
				SetLevelAngleLimit(value);
			}
		}

		public override ushort MinThrottle
		{
			get
			{
				return GetMinThrottle();
			}
			set
			{
				SetMinThrottle(value);
			}
		}

		public override byte ItermRotation
		{
			get
			{
				return GetItermRotation();
			}
			set
			{
				SetItermRotation(value);
			}
		}

		public override byte SmartFeedforward
		{
			get
			{
				return GetSmartFeedforward();
			}
			set
			{
				SetSmartFeedforward(value);
			}
		}

		public override byte ItermRelax
		{
			get
			{
				return GetItermRelax();
			}
			set
			{
				SetItermRelax(value);
			}
		}

		public override byte ItermRelaxCutoff
		{
			get
			{
				return GetItermRelaxCutoff();
			}
			set
			{
				SetItermRelaxCutoff(value);
			}
		}

		public override byte ItermRelaxType
		{
			get
			{
				return GetItermRelaxType();
			}
			set
			{
				SetItermRelaxType(value);
			}
		}

		public override ushort ItermAcceleratorGain
		{
			get
			{
				return GetItermAcceleratorGain();
			}
			set
			{
				SetItermAcceleratorGain(value);
			}
		}

		[DllImport("bf_3_4")]
		public static extern void InitializeFlightController();

		[DllImport("bf_3_4")]
		public static extern void DoPidLoop(float deltaTime);

		[DllImport("bf_3_4")]
		public static extern void SetArmed(bool armed);

		[DllImport("bf_3_4")]
		public static extern void SetFlightMode(int mode);

		[DllImport("bf_3_4")]
		public static extern void SetPidConstants(int[] roll, int[] pitch, int[] yaw, int[] level);

		[DllImport("bf_3_4")]
		public static extern void SetPidConstant(int axis, [In][Out] int[] constants);

		[DllImport("bf_3_4")]
		public static extern void SetPidSumLimit(int pidSumLimit, int pidSumYawLimit);

		[DllImport("bf_3_4")]
		public static extern void SetSignals(short[] signals);

		[DllImport("bf_3_4")]
		public static extern void SetAccelerometer(short roll, short pitch, short yaw);

		[DllImport("bf_3_4")]
		public static extern void SetGyro(float[] gyroSignals);

		[DllImport("bf_3_4")]
		public static extern void SetSuperRates(byte[] superRates);

		[DllImport("bf_3_4")]
		public static extern void SetRcExpoRates(byte[] expoRates);

		[DllImport("bf_3_4")]
		public static extern void SetRcRates(byte[] rcRates);

		[DllImport("bf_3_4")]
		public static extern void SetMinThrottle(short minThrottle);

		[DllImport("bf_3_4")]
		public static extern void UpdateConfiguration(byte[] profiles);

		[DllImport("bf_3_4")]
		public static extern void GetPidProfile(byte[] profile);

		[DllImport("bf_3_4")]
		public static extern void GetPid(int axis, [In][Out] float[] pid);

		[DllImport("bf_3_4")]
		public static extern void GetMotors([In][Out] float[] motors);

		[DllImport("bf_3_4")]
		public static extern void GetDebugValues([In][Out] float[] pid, [In][Out] float[] motorsMix, [In][Out] float[] setpoint, [In][Out] int[] constants, [In][Out] float[] gyroscope);

		[DllImport("bf_3_4")]
		public static extern void SetPreviousGyroRateDterm([In][Out] float[] pGyro);

		[DllImport("bf_3_4")]
		public static extern void GetPreviousGyroRateDterm([In][Out] float[] pGyro);

		[DllImport("bf_3_4")]
		public static extern void GetSuperRates([In][Out] int[] superRates);

		[DllImport("bf_3_4")]
		public static extern void GetRcExpoRates([In][Out] int[] expoRates);

		[DllImport("bf_3_4")]
		public static extern void GetRcRates([In][Out] int[] rcRates);

		[DllImport("bf_3_4")]
		public static extern void GetConstants([In][Out] int[] constants, int axis);

		[DllImport("bf_3_4", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		public static extern IntPtr getVersion();

		[DllImport("bf_3_4")]
		public static extern byte GetAbsControlErrorLimit();

		[DllImport("bf_3_4")]
		public static extern byte GetAbsControlGain();

		[DllImport("bf_3_4")]
		public static extern byte GetAbsControlLimit();

		[DllImport("bf_3_4")]
		public static extern byte GetAcroTrainerAngleLimit();

		[DllImport("bf_3_4")]
		public static extern byte GetAcroTrainerDebugAxis();

		[DllImport("bf_3_4")]
		public static extern byte GetAcroTrainerGain();

		[DllImport("bf_3_4")]
		public static extern ushort GetAcroTrainerLookaheadMs();

		[DllImport("bf_3_4")]
		public static extern ushort GetCrashDelay();

		[DllImport("bf_3_4")]
		public static extern ushort GetCrashDthreshold();

		[DllImport("bf_3_4")]
		public static extern ushort GetCrashGthreshold();

		[DllImport("bf_3_4")]
		public static extern ushort GetCrashLimitYaw();

		[DllImport("bf_3_4")]
		public static extern byte GetCrashRecovery();

		[DllImport("bf_3_4")]
		public static extern byte GetCrashRecoveryAngle();

		[DllImport("bf_3_4")]
		public static extern byte GetCrashRecoveryRate();

		[DllImport("bf_3_4")]
		public static extern ushort GetCrashSetpointThreshold();

		[DllImport("bf_3_4")]
		public static extern ushort GetCrashTime();

		[DllImport("bf_3_4")]
		public static extern byte GetDtermFilterType();

		[DllImport("bf_3_4")]
		public static extern ushort GetDtermLowpass2Hz();

		[DllImport("bf_3_4")]
		public static extern ushort GetDtermLowpassHz();

		[DllImport("bf_3_4")]
		public static extern ushort GetDtermNotchCutoff();

		[DllImport("bf_3_4")]
		public static extern ushort GetDtermNotchHz();

		[DllImport("bf_3_4")]
		public static extern ushort GetDtermSetpointWeight();

		[DllImport("bf_3_4")]
		public static extern byte GetSetpointRelaxRatio();

		[DllImport("bf_3_4")]
		public static extern byte GetHorizonTiltEffect();

		[DllImport("bf_3_4")]
		public static extern byte GetHorizonTiltExpertMode();

		[DllImport("bf_3_4")]
		public static extern ushort GetItermAcceleratorGain();

		[DllImport("bf_3_4")]
		public static extern ushort GetItermLimit();

		[DllImport("bf_3_4")]
		public static extern ushort GetItermThrottleThreshold();

		[DllImport("bf_3_4")]
		public static extern byte GetItermWindupPointPercent();

		[DllImport("bf_3_4")]
		public static extern byte GetItermRelax();

		[DllImport("bf_3_4")]
		public static extern byte GetItermRelaxCutoff();

		[DllImport("bf_3_4")]
		public static extern byte GetItermRelaxType();

		[DllImport("bf_3_4")]
		public static extern byte GetItermRotation();

		[DllImport("bf_3_4")]
		public static extern byte GetLevelAngleLimit();

		[DllImport("bf_3_4")]
		public static extern byte GetPidAtMinThrottle();

		[DllImport("bf_3_4")]
		public static extern ushort GetPidSumLimit();

		[DllImport("bf_3_4")]
		public static extern ushort GetPidSumLimitYaw();

		[DllImport("bf_3_4")]
		public static extern ushort GetRateAccelLimit();

		[DllImport("bf_3_4")]
		public static extern byte GetSmartFeedforward();

		[DllImport("bf_3_4")]
		public static extern byte GetThrottleBoost();

		[DllImport("bf_3_4")]
		public static extern byte GetThrottleBoostCutoff();

		[DllImport("bf_3_4")]
		public static extern byte GetVbatPidCompensation();

		[DllImport("bf_3_4")]
		public static extern ushort GetYawRateAccelLimit();

		[DllImport("bf_3_4")]
		public static extern ushort GetYawLowpassHz();

		[DllImport("bf_3_4")]
		public static extern byte GetThrMid8();

		[DllImport("bf_3_4")]
		public static extern byte GetThrExpo8();

		[DllImport("bf_3_4")]
		public static extern byte GetDynThrPID();

		[DllImport("bf_3_4")]
		public static extern byte GetTpaBreakpoint();

		[DllImport("bf_3_4")]
		public static extern byte GetRatesType();

		[DllImport("bf_3_4")]
		public static extern byte GetThrottleLimitType();

		[DllImport("bf_3_4")]
		public static extern byte GetThrottleLimitPercent();

		[DllImport("bf_3_4")]
		public static extern ushort GetMinThrottle();

		[DllImport("bf_3_4")]
		public static extern ushort GetMaxThrottle();

		[DllImport("bf_3_4")]
		public static extern ushort GetMinCommand();

		[DllImport("bf_3_4")]
		public static extern bool IsAirmodeActive();

		[DllImport("bf_3_4")]
		public static extern void SetAbsControlErrorLimit(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetAbsControlGain(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetAbsControlLimit(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetAcroTrainerAngleLimit(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetAcroTrainerDebugAxis(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetAcroTrainerGain(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetAcroTrainerLookaheadMs(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetCrashDelay(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetCrashDthreshold(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetCrashGthreshold(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetCrashLimitYaw(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetCrashRecovery(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetCrashRecoveryAngle(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetCrashRecoveryRate(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetCrashSetpointThreshold(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetCrashTime(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetDtermFilterType(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetDtermLowpass2Hz(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetDtermLowpassHz(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetDtermNotchCutoff(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetDtermNotchHz(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetDtermSetpointWeight(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetSetpointRelaxRatio(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetHorizonTiltEffect(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetHorizonTiltExpertMode(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetItermAcceleratorGain(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetItermLimit(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetItermThrottleThreshold(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetItermWindupPointPercent(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetItermRelax(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetItermRelaxCutoff(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetItermRelaxType(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetItermRotation(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetLevelAngleLimit(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetPidAtMinThrottle(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetPidSumLimit(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetPidSumLimitYaw(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetRateAccelLimit(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetSmartFeedforward(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetThrottleBoost(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetThrottleBoostCutoff(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetVbatPidCompensation(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetYawRateAccelLimit(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetYawLowpassHz(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetThrMid8(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetThrExpo8(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetDynThrPID(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetTpaBreakpoint(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetRatesType(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetThrottleLimitType(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetThrottleLimitPercent(byte value);

		[DllImport("bf_3_4")]
		public static extern void SetMinThrottle(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetMaxThrottle(ushort value);

		[DllImport("bf_3_4")]
		public static extern void SetMinCommand(ushort value);

		[DllImport("bf_3_4")]
		public static extern bool FeatureIsEnabled(uint value);

		[DllImport("bf_3_4")]
		public static extern void FeatureEnable(uint value);

		[DllImport("bf_3_4")]
		public static extern void FeatureDisable(uint value);

		[DllImport("bf_3_4")]
		public static extern void SetDroneLayout(byte value);

		public override void enableFlightMode(FlightMode flightMode)
		{
			SetFlightMode((int)flightMode);
		}

		protected override void pushAllPids()
		{
			SetPidConstants(m_pidRoll, m_pidPitch, m_pidYaw, m_pidLevel);
		}

		protected override void pullAllPids()
		{
			GetPid(0, f_pidRoll);
			m_pidRoll[0] = (int)f_pidRoll[0];
			m_pidRoll[1] = (int)f_pidRoll[1];
			m_pidRoll[2] = (int)f_pidRoll[2];
			m_pidRoll[3] = (int)f_pidRoll[3];
			GetPid(1, f_pidPitch);
			m_pidPitch[0] = (int)f_pidPitch[0];
			m_pidPitch[1] = (int)f_pidPitch[1];
			m_pidPitch[2] = (int)f_pidPitch[2];
			m_pidPitch[3] = (int)f_pidPitch[3];
			GetPid(2, f_pidYaw);
			m_pidYaw[0] = (int)f_pidYaw[0];
			m_pidYaw[1] = (int)f_pidYaw[1];
			m_pidYaw[2] = (int)f_pidYaw[2];
			m_pidYaw[3] = (int)f_pidYaw[3];
			GetPid(3, f_pidLevel);
			m_pidLevel[0] = (int)f_pidLevel[0];
			m_pidLevel[1] = (int)f_pidLevel[1];
			m_pidLevel[2] = (int)f_pidLevel[2];
			m_pidLevel[3] = (int)f_pidLevel[3];
		}

		protected override void pullPid(int axis)
		{
			switch (axis)
			{
			case 0:
				GetPid(0, f_pidRoll);
				m_pidRoll[0] = (int)f_pidRoll[0];
				m_pidRoll[1] = (int)f_pidRoll[1];
				m_pidRoll[2] = (int)f_pidRoll[2];
				m_pidRoll[3] = (int)f_pidRoll[3];
				break;
			case 1:
				GetPid(1, f_pidPitch);
				m_pidPitch[0] = (int)f_pidPitch[0];
				m_pidPitch[1] = (int)f_pidPitch[1];
				m_pidPitch[2] = (int)f_pidPitch[2];
				m_pidPitch[3] = (int)f_pidPitch[3];
				break;
			case 2:
				GetPid(2, f_pidYaw);
				m_pidYaw[0] = (int)f_pidYaw[0];
				m_pidYaw[1] = (int)f_pidYaw[1];
				m_pidYaw[2] = (int)f_pidYaw[2];
				m_pidYaw[3] = (int)f_pidYaw[3];
				break;
			case 3:
				GetPid(3, f_pidLevel);
				m_pidLevel[0] = (int)f_pidLevel[0];
				m_pidLevel[1] = (int)f_pidLevel[1];
				m_pidLevel[2] = (int)f_pidLevel[2];
				m_pidLevel[3] = (int)f_pidLevel[3];
				break;
			}
		}

		protected override void pushPid(int axis)
		{
			switch (axis)
			{
			case 0:
				SetPidConstant(0, m_pidRoll);
				break;
			case 1:
				SetPidConstant(1, m_pidPitch);
				break;
			case 2:
				SetPidConstant(2, m_pidYaw);
				break;
			case 3:
				SetPidConstant(3, m_pidLevel);
				break;
			}
		}

		protected override void pushSuperRates()
		{
			SetSuperRates(b_superRates);
		}

		protected override void pushExpoRates()
		{
			SetRcExpoRates(b_expoRates);
		}

		protected override void pushRcRates()
		{
			SetRcRates(b_rcRates);
		}

		protected override void pullSuperRates()
		{
			GetSuperRates(m_superRates);
			b_superRates[0] = (byte)m_superRates[0];
			b_superRates[1] = (byte)m_superRates[1];
			b_superRates[2] = (byte)m_superRates[2];
			b_superRates[3] = (byte)m_superRates[3];
		}

		protected override void pullExpoRates()
		{
			GetRcExpoRates(m_expoRates);
			b_expoRates[0] = (byte)m_expoRates[0];
			b_expoRates[1] = (byte)m_expoRates[1];
			b_expoRates[2] = (byte)m_expoRates[2];
			b_expoRates[3] = (byte)m_expoRates[3];
		}

		protected override void pullRcRates()
		{
			GetRcRates(m_superRates);
			b_rcRates[0] = (byte)m_superRates[0];
			b_rcRates[1] = (byte)m_superRates[1];
			b_rcRates[2] = (byte)m_superRates[2];
		}

		protected override void pushSignals()
		{
			SetSignals(m_signals);
		}

		protected override void pushAccelerometer()
		{
			SetAccelerometer(m_accelerometer[0], m_accelerometer[1], m_accelerometer[2]);
		}

		protected override void pushGyro()
		{
			SetGyro(m_gyro);
		}

		protected override void pullMotors()
		{
			GetMotors(m_motors);
		}

		public override void setConfiguration(PidProfile pidProfile, ControlRate controlRate, MotorConfig motorConfig)
		{
			UpdateConfiguration(CommsUtil.PrepareUpdateConfiguration(pidProfile, controlRate, motorConfig));
		}

		public override void initializeFlightController()
		{
			InitializeFlightController();
		}

		public override void doPidLoop(float deltaTime)
		{
			DoPidLoop(deltaTime);
		}

		public override void getDebugValues([In][Out] float[] pid, [In][Out] float[] motorsMix, [In][Out] float[] setpoint, [In][Out] int[] constants, [In][Out] float[] gyroscope)
		{
			GetDebugValues(pid, motorsMix, setpoint, constants, gyroscope);
		}

		public override void getConstants([In][Out] int[] constants, int axis)
		{
			GetConstants(constants, axis);
		}
	}
}
