using System.Runtime.InteropServices;

namespace drl.sim.thread
{
	public class FlightControllerInterface
	{
		protected short[] m_signals = new short[4];

		protected float[] m_gyro = new float[3];

		protected short[] m_accelerometer = new short[3];

		protected float[] m_motors = new float[4];

		protected int[] m_pidRoll = new int[4];

		protected int[] m_pidPitch = new int[4];

		protected int[] m_pidYaw = new int[4];

		protected int[] m_pidLevel = new int[4];

		protected float[] f_pidRoll = new float[4];

		protected float[] f_pidPitch = new float[4];

		protected float[] f_pidYaw = new float[4];

		protected float[] f_pidLevel = new float[4];

		protected int[] m_superRates = new int[4];

		protected int[] m_rcRates = new int[3];

		protected int[] m_expoRates = new int[4];

		protected byte[] b_superRates = new byte[4];

		protected byte[] b_rcRates = new byte[3];

		protected byte[] b_expoRates = new byte[4];

		public virtual string Version => "unknown";

		public virtual bool Airmode
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public virtual bool Antigravity
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public virtual bool DynamicFilter
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public virtual byte LevelAngleLimit
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public virtual ushort MinThrottle
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public virtual byte ItermRotation
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public virtual byte SmartFeedforward
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public virtual byte FeedForwardTransition
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public virtual byte ItermRelax
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public virtual byte ItermRelaxCutoff
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public virtual byte ItermRelaxType
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public virtual byte AntiGravityMode
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public virtual ushort ItermAcceleratorGain
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public static void CopyArray(float[] source, float[] target)
		{
			for (int i = 0; i < source.Length && i < target.Length; i++)
			{
				target[i] = source[i];
			}
		}

		public static void CopyArray(short[] source, short[] target)
		{
			for (int i = 0; i < source.Length && i < target.Length; i++)
			{
				target[i] = source[i];
			}
		}

		public static void CopyArray(byte[] source, byte[] target)
		{
			for (int i = 0; i < source.Length && i < target.Length; i++)
			{
				target[i] = source[i];
			}
		}

		public static void CopyArray(int[] source, int[] target)
		{
			for (int i = 0; i < source.Length && i < target.Length; i++)
			{
				target[i] = source[i];
			}
		}

		public static void CopyArray(int[] source, float[] target)
		{
			for (int i = 0; i < source.Length && i < target.Length; i++)
			{
				target[i] = source[i];
			}
		}

		public virtual void enableFlightMode(FlightMode flightMode)
		{
		}

		public void setPidConstants(PIDVector roll, PIDVector pitch, PIDVector yaw, PIDVector level)
		{
			m_pidRoll[0] = roll.P;
			m_pidRoll[1] = roll.I;
			m_pidRoll[2] = roll.D;
			m_pidRoll[3] = roll.F;
			m_pidPitch[0] = pitch.P;
			m_pidPitch[1] = pitch.I;
			m_pidPitch[2] = pitch.D;
			m_pidPitch[3] = pitch.F;
			m_pidYaw[0] = yaw.P;
			m_pidYaw[1] = yaw.I;
			m_pidYaw[2] = yaw.D;
			m_pidYaw[3] = yaw.F;
			m_pidLevel[0] = level.P;
			m_pidLevel[1] = level.I;
			m_pidLevel[2] = level.D;
			m_pidLevel[3] = level.F;
			pushAllPids();
		}

		protected virtual void pushAllPids()
		{
		}

		protected virtual void pullAllPids()
		{
		}

		public int[] getPid(int axis)
		{
			pullPid(axis);
			return axis switch
			{
				0 => m_pidRoll, 
				1 => m_pidPitch, 
				2 => m_pidYaw, 
				3 => m_pidLevel, 
				_ => m_pidLevel, 
			};
		}

		public void getPid(int axis, [In][Out] float[] pid)
		{
			pullPid(axis);
			switch (axis)
			{
			case 0:
				CopyArray(m_pidRoll, pid);
				break;
			case 1:
				CopyArray(m_pidPitch, pid);
				break;
			case 2:
				CopyArray(m_pidYaw, pid);
				break;
			case 3:
				CopyArray(m_pidLevel, pid);
				break;
			}
		}

		protected virtual void pullPid(int axis)
		{
		}

		protected virtual void pushPid(int axis)
		{
		}

		public void setPid(int axis, int[] pid)
		{
			switch (axis)
			{
			case 0:
				CopyArray(pid, m_pidRoll);
				break;
			case 1:
				CopyArray(pid, m_pidPitch);
				break;
			case 2:
				CopyArray(pid, m_pidYaw);
				break;
			case 3:
				CopyArray(pid, m_pidLevel);
				break;
			}
			pushPid(axis);
		}

		public void setRates(Rates superRates, Rates expoRates, Rates rcRates)
		{
			b_superRates[0] = superRates.Roll;
			b_superRates[1] = superRates.Pitch;
			b_superRates[2] = superRates.Yaw;
			b_superRates[3] = superRates.Throttle;
			b_expoRates[0] = expoRates.Roll;
			b_expoRates[1] = expoRates.Pitch;
			b_expoRates[2] = expoRates.Yaw;
			b_expoRates[3] = expoRates.Throttle;
			b_rcRates[0] = rcRates.Roll;
			b_rcRates[1] = rcRates.Pitch;
			b_rcRates[2] = rcRates.Yaw;
			pushAllRates();
		}

		protected void pushAllRates()
		{
			pushSuperRates();
			pushExpoRates();
			pushRcRates();
		}

		protected virtual void pushSuperRates()
		{
		}

		protected virtual void pushExpoRates()
		{
		}

		protected virtual void pushRcRates()
		{
		}

		public void setSuperRates(Rates superRates)
		{
			b_superRates[0] = superRates.Roll;
			b_superRates[1] = superRates.Pitch;
			b_superRates[2] = superRates.Yaw;
			b_superRates[3] = superRates.Throttle;
			pushSuperRates();
		}

		public void setSuperRates(byte[] superRates)
		{
			CopyArray(superRates, b_superRates);
			pushSuperRates();
		}

		public void setExpoRates(Rates expoRates)
		{
			b_expoRates[0] = expoRates.Roll;
			b_expoRates[1] = expoRates.Pitch;
			b_expoRates[2] = expoRates.Yaw;
			b_expoRates[3] = expoRates.Throttle;
			pushExpoRates();
		}

		public void setExpoRates(byte[] expoRates)
		{
			CopyArray(expoRates, b_expoRates);
			pushExpoRates();
		}

		public void setRcRates(Rates rcRates)
		{
			b_rcRates[0] = rcRates.Roll;
			b_rcRates[1] = rcRates.Pitch;
			b_rcRates[2] = rcRates.Yaw;
			pushRcRates();
		}

		public void setRcRates(byte[] rcRates)
		{
			CopyArray(rcRates, b_rcRates);
			pushRcRates();
		}

		protected void pullAllRates()
		{
			pullSuperRates();
			pullExpoRates();
			pullRcRates();
		}

		protected virtual void pullSuperRates()
		{
		}

		protected virtual void pullExpoRates()
		{
		}

		protected virtual void pullRcRates()
		{
		}

		public void getSuperRates([In][Out] int[] rates)
		{
			pullSuperRates();
			CopyArray(m_superRates, rates);
		}

		public byte[] getSuperRates()
		{
			pullSuperRates();
			return b_superRates;
		}

		public void getExpoRates([In][Out] int[] rates)
		{
			pullExpoRates();
			CopyArray(m_expoRates, rates);
		}

		public byte[] getExpoRates()
		{
			pullExpoRates();
			return b_expoRates;
		}

		public void getRcRates([In][Out] int[] rates)
		{
			pullRcRates();
			CopyArray(m_rcRates, rates);
		}

		public byte[] getRcRates()
		{
			pullRcRates();
			return b_rcRates;
		}

		public void setSignals(short[] signals)
		{
			CopyArray(signals, m_signals);
			pushSignals();
		}

		public void setAccelerometer(short[] acc)
		{
			CopyArray(acc, m_accelerometer);
			pushAccelerometer();
		}

		public void setGyro(float[] gyroSignals)
		{
			CopyArray(gyroSignals, m_gyro);
			pushGyro();
		}

		public void setSignals(short roll, short pitch, short yaw, short throttle)
		{
			m_signals[0] = roll;
			m_signals[1] = pitch;
			m_signals[2] = yaw;
			m_signals[3] = throttle;
			pushSignals();
		}

		public void setAccelerometer(short roll, short pitch, short yaw)
		{
			m_accelerometer[0] = roll;
			m_accelerometer[1] = pitch;
			m_accelerometer[2] = yaw;
			pushAccelerometer();
		}

		public void setGyro(float roll, float pitch, float yaw)
		{
			m_gyro[0] = roll;
			m_gyro[1] = pitch;
			m_gyro[2] = yaw;
			pushGyro();
		}

		protected virtual void pushSignals()
		{
		}

		protected virtual void pushAccelerometer()
		{
		}

		protected virtual void pushGyro()
		{
		}

		public void getMotors([In][Out] float[] motors)
		{
			pullMotors();
			CopyArray(m_motors, motors);
		}

		public float[] getMotors()
		{
			pullMotors();
			return m_motors;
		}

		protected virtual void pullMotors()
		{
		}

		public virtual void setConfiguration(PidProfile pidProfile, ControlRate controlRate, MotorConfig motorConfig)
		{
		}

		public virtual void initializeFlightController()
		{
		}

		public virtual void doPidLoop(float deltaTime)
		{
		}

		public virtual void getDebugValues([In][Out] float[] pid, [In][Out] float[] motorsMix, [In][Out] float[] setpoint, [In][Out] int[] constants, [In][Out] float[] gyroscope)
		{
		}

		public virtual void getConstants([In][Out] int[] constants, int axis)
		{
		}
	}
}
