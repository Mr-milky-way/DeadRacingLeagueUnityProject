using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace drl.sim.thread
{
	public class FlightController
	{
		public enum FlightControllerVersion
		{
			None = 0,
			Betaflight_3_4 = 1,
			Betaflight_3_5 = 2,
			Betaflight_4_0 = 3
		}

		private static FlightControllerVersion m_currentVersion = FlightControllerVersion.Betaflight_4_0;

		private static FlightControllerInterface m_current = new Betaflight_4_0();

		private static Dictionary<FlightControllerVersion, string> m_actualVersions = null;

		public static int CurrentVersionInt
		{
			get
			{
				return CurrentVersion switch
				{
					FlightControllerVersion.Betaflight_3_4 => 34, 
					FlightControllerVersion.Betaflight_3_5 => 35, 
					FlightControllerVersion.Betaflight_4_0 => 40, 
					_ => 0, 
				};
			}
			set
			{
				switch (value)
				{
				case 34:
					CurrentVersion = FlightControllerVersion.Betaflight_3_4;
					break;
				case 35:
					CurrentVersion = FlightControllerVersion.Betaflight_3_5;
					break;
				case 40:
					CurrentVersion = FlightControllerVersion.Betaflight_4_0;
					break;
				default:
					Debug.LogError("FlightController: unknown version: " + value);
					break;
				}
			}
		}

		public static FlightControllerVersion CurrentVersion
		{
			get
			{
				return m_currentVersion;
			}
			set
			{
				if (m_currentVersion != value)
				{
					switch (value)
					{
					case FlightControllerVersion.Betaflight_3_4:
						m_current = new Betaflight_3_4();
						break;
					case FlightControllerVersion.Betaflight_3_5:
						m_current = new Betaflight_3_5();
						break;
					case FlightControllerVersion.Betaflight_4_0:
						m_current = new Betaflight_4_0();
						break;
					default:
						Debug.LogError("FlightController: unknown version: " + value);
						return;
					}
					m_current.initializeFlightController();
				}
				m_currentVersion = value;
			}
		}

		public static Dictionary<FlightControllerVersion, string> ActualVersions
		{
			get
			{
				if (m_actualVersions == null)
				{
					m_actualVersions = new Dictionary<FlightControllerVersion, string>();
					m_actualVersions.Add(FlightControllerVersion.Betaflight_3_4, new Betaflight_3_4().Version);
					m_actualVersions.Add(FlightControllerVersion.Betaflight_3_5, new Betaflight_3_5().Version);
					m_actualVersions.Add(FlightControllerVersion.Betaflight_4_0, new Betaflight_4_0().Version);
				}
				return m_actualVersions;
			}
		}

		public static FlightMode flightMode { get; protected set; }

		public static string Version => m_current.Version;

		public static bool Airmode
		{
			get
			{
				return m_current.Airmode;
			}
			set
			{
				m_current.Airmode = value;
			}
		}

		public static bool Antigravity
		{
			get
			{
				return m_current.Antigravity;
			}
			set
			{
				m_current.Antigravity = value;
			}
		}

		public static bool DynamicFilter
		{
			get
			{
				return m_current.DynamicFilter;
			}
			set
			{
				m_current.DynamicFilter = value;
			}
		}

		public static byte LevelAngleLimit
		{
			get
			{
				return m_current.LevelAngleLimit;
			}
			set
			{
				m_current.LevelAngleLimit = value;
			}
		}

		public static ushort MinThrottle
		{
			get
			{
				return m_current.MinThrottle;
			}
			set
			{
				m_current.MinThrottle = value;
			}
		}

		public static byte ItermRotation
		{
			get
			{
				return m_current.ItermRotation;
			}
			set
			{
				m_current.ItermRotation = value;
			}
		}

		public static byte SmartFeedforward
		{
			get
			{
				return m_current.SmartFeedforward;
			}
			set
			{
				m_current.SmartFeedforward = value;
			}
		}

		public static byte FeedForwardTransition
		{
			get
			{
				return m_current.FeedForwardTransition;
			}
			set
			{
				m_current.FeedForwardTransition = value;
			}
		}

		public static byte ItermRelax
		{
			get
			{
				return m_current.ItermRelax;
			}
			set
			{
				m_current.ItermRelax = value;
			}
		}

		public static byte ItermRelaxCutoff
		{
			get
			{
				return m_current.ItermRelaxCutoff;
			}
			set
			{
				m_current.ItermRelaxCutoff = value;
			}
		}

		public static byte ItermRelaxType
		{
			get
			{
				return m_current.ItermRelaxType;
			}
			set
			{
				m_current.ItermRelaxType = value;
			}
		}

		public static byte AntiGravityMode
		{
			get
			{
				return m_current.AntiGravityMode;
			}
			set
			{
				m_current.AntiGravityMode = value;
			}
		}

		public static ushort ItermAcceleratorGain
		{
			get
			{
				return m_current.ItermAcceleratorGain;
			}
			set
			{
				m_current.ItermAcceleratorGain = value;
			}
		}

		public static string VersionString(int p_version)
		{
			return p_version switch
			{
				34 => VersionString(FlightControllerVersion.Betaflight_3_4), 
				35 => VersionString(FlightControllerVersion.Betaflight_3_5), 
				40 => VersionString(FlightControllerVersion.Betaflight_4_0), 
				_ => "UNKNOWN", 
			};
		}

		public static string VersionString(FlightControllerVersion p_version)
		{
			if (ActualVersions.ContainsKey(p_version))
			{
				return ActualVersions[p_version];
			}
			return "UNKNOWN";
		}

		public static void EnableFlightMode(FlightMode p_flightMode)
		{
			flightMode = p_flightMode;
			m_current.enableFlightMode(p_flightMode);
		}

		public static void SetPidConstants(PIDVector roll, PIDVector pitch, PIDVector yaw, PIDVector level)
		{
			m_current.setPidConstants(roll, pitch, yaw, level);
		}

		public static void SetRates(Rates superRates, Rates expoRates, Rates rcRates)
		{
			m_current.setRates(superRates, expoRates, rcRates);
		}

		public static void SetConfiguration(PidProfile pidProfile, ControlRate controlRate, MotorConfig motorConfig)
		{
			m_current.setConfiguration(pidProfile, controlRate, motorConfig);
		}

		public static void InitializeFlightController()
		{
			m_current.initializeFlightController();
		}

		public static void SetSignals(short[] signals)
		{
			m_current.setSignals(signals);
		}

		public static void SetAccelerometer(short roll, short pitch, short yaw)
		{
			m_current.setAccelerometer(roll, pitch, yaw);
		}

		public static void SetGyro(float[] gyroSignals)
		{
			m_current.setGyro(gyroSignals);
		}

		public static void DoPidLoop(float deltaTime)
		{
			m_current.doPidLoop(deltaTime);
		}

		public static void GetPid(int axis, [In][Out] float[] pid)
		{
			m_current.getPid(axis, pid);
		}

		public static void GetSuperRates([In][Out] int[] superRates)
		{
			m_current.getSuperRates(superRates);
		}

		public static void GetRcExpoRates([In][Out] int[] expoRates)
		{
			m_current.getExpoRates(expoRates);
		}

		public static void GetRcRates([In][Out] int[] rcRates)
		{
			m_current.getRcRates(rcRates);
		}

		public static void GetMotors([In][Out] float[] motors)
		{
			m_current.getMotors(motors);
		}

		public static void GetDebugValues([In][Out] float[] pid, [In][Out] float[] motorsMix, [In][Out] float[] setpoint, [In][Out] int[] constants, [In][Out] float[] gyroscope)
		{
			m_current.getDebugValues(pid, motorsMix, setpoint, constants, gyroscope);
		}

		public static void SetSuperRates(byte[] superRates)
		{
			m_current.setSuperRates(superRates);
		}

		public static void SetRcExpoRates(byte[] expoRates)
		{
			m_current.setExpoRates(expoRates);
		}

		public static void SetRcRates(byte[] rcRates)
		{
			m_current.setRcRates(rcRates);
		}

		public static void SetMinThrottle(ushort minThrottle)
		{
			m_current.MinThrottle = minThrottle;
		}

		public static void GetConstants([In][Out] int[] constants, int axis)
		{
			m_current.getConstants(constants, axis);
		}
	}
}
