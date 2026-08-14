using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class FCProfileData : SerializedData
	{
		public class Betaflight
		{
			public class Preset
			{
				public float prRcRate;

				public float yawRcRate;

				public float prSuperRate;

				public float yawSuperRate;

				public float prExpo;

				public float yawExpo;

				public float tMid;

				public float tExpo;

				public Preset(float prRcRate, float yawRcRate, float prSuperRate, float yawSuperRate)
				{
					this.prRcRate = prRcRate;
					this.yawRcRate = yawRcRate;
					this.prSuperRate = prSuperRate;
					this.yawSuperRate = yawSuperRate;
					tMid = 0f;
					tExpo = 0f;
					prExpo = 0f;
					yawExpo = 0f;
				}

				public Preset(float prRcRate, float yawRcRate, float prSuperRate, float yawSuperRate, float tExpo, float tMid)
				{
					this.prRcRate = prRcRate;
					this.yawRcRate = yawRcRate;
					this.prSuperRate = prSuperRate;
					this.yawSuperRate = yawSuperRate;
					this.tMid = tMid;
					this.tExpo = tExpo;
					prExpo = 0f;
					yawExpo = 0f;
				}
			}

			public enum PresetType
			{
				Training = 0,
				Low = 1,
				Medium = 2,
				High = 3,
				None = 4
			}

			public static Dictionary<ControllerStateType, Preset> LowPresets = new Dictionary<ControllerStateType, Preset>
			{
				{
					ControllerStateType.XBox,
					new Preset(0.7f, 0.7f, 0.1f, 0.1f, 0f, 0.5f)
				},
				{
					ControllerStateType.PS4,
					new Preset(0.7f, 0.7f, 0.1f, 0.1f, 0f, 0.5f)
				},
				{
					ControllerStateType.Taranis,
					new Preset(0.6f, 0.6f, 0.7f, 0.7f, 0f, 0.5f)
				},
				{
					ControllerStateType.Nikko,
					new Preset(0.6f, 0.6f, 0.7f, 0.7f, 0f, 0.5f)
				}
			};

			public static Dictionary<ControllerStateType, Preset> MediumPresets = new Dictionary<ControllerStateType, Preset>
			{
				{
					ControllerStateType.XBox,
					new Preset(0.5f, 0.5f, 0.6f, 0.6f, 0f, 0.5f)
				},
				{
					ControllerStateType.PS4,
					new Preset(0.5f, 0.5f, 0.6f, 0.6f, 0f, 0.5f)
				},
				{
					ControllerStateType.Taranis,
					new Preset(1f, 1f, 0.7f, 0.7f, 0f, 0.5f)
				},
				{
					ControllerStateType.Nikko,
					new Preset(1f, 1f, 0.7f, 0.7f, 0f, 0.5f)
				}
			};

			public static Dictionary<ControllerStateType, Preset> HighPresets = new Dictionary<ControllerStateType, Preset>
			{
				{
					ControllerStateType.XBox,
					new Preset(0.65f, 0.65f, 0.65f, 0.65f, 0f, 0.5f)
				},
				{
					ControllerStateType.PS4,
					new Preset(0.65f, 0.65f, 0.65f, 0.65f, 0f, 0.5f)
				},
				{
					ControllerStateType.Taranis,
					new Preset(1.3f, 1.3f, 0.8f, 0.8f, 0f, 0.5f)
				},
				{
					ControllerStateType.Nikko,
					new Preset(1.3f, 1.3f, 0.8f, 0.8f, 0f, 0.5f)
				}
			};

			public static Dictionary<ControllerStateType, Preset> TrainingPresets = new Dictionary<ControllerStateType, Preset>
			{
				{
					ControllerStateType.XBox,
					new Preset(0.5f, 0.6f, 0.5f, 0.6f)
				},
				{
					ControllerStateType.PS4,
					new Preset(0.5f, 0.6f, 0.5f, 0.6f)
				},
				{
					ControllerStateType.Taranis,
					new Preset(0.6f, 0.7f, 0.6f, 0.7f)
				},
				{
					ControllerStateType.Nikko,
					new Preset(0.5f, 0.6f, 0.5f, 0.6f)
				}
			};

			[NonSerialized]
			private FCProfileData fcd;

			[NonSerialized]
			private string fcdThrottleKey;

			[NonSerialized]
			private string fcdYawKey;

			[NonSerialized]
			private string fcdPitchRollKey;

			[NonSerialized]
			private float defaultValue;

			[NonSerialized]
			private float defaultValueThrottle = float.MinValue;

			private float m_throttle;

			private float m_yaw;

			private float m_pitch = -1f;

			private float m_roll = -1f;

			public float throttle
			{
				get
				{
					fcd.UpdateCached();
					return m_throttle;
				}
				set
				{
					m_throttle = value;
					fcd.Set(fcdThrottleKey, value);
				}
			}

			public float yaw
			{
				get
				{
					fcd.UpdateCached();
					return m_yaw;
				}
				set
				{
					m_yaw = value;
					fcd.Set(fcdYawKey, value);
				}
			}

			public float pitchRoll
			{
				set
				{
					pitch = value;
					roll = value;
				}
			}

			public float pitch
			{
				get
				{
					fcd.UpdateCached();
					if (m_pitch < 0f)
					{
						m_pitch = fcd.Get(fcdPitchRollKey + "p", 2.1287f);
					}
					return m_pitch;
				}
				set
				{
					m_pitch = value;
					fcd.Set(fcdPitchRollKey + "p", value);
				}
			}

			public float roll
			{
				get
				{
					fcd.UpdateCached();
					if (m_roll < 0f)
					{
						m_roll = fcd.Get(fcdPitchRollKey + "r", 2.1287f);
					}
					return m_roll;
				}
				set
				{
					m_roll = value;
					fcd.Set(fcdPitchRollKey + "r", value);
				}
			}

			public Betaflight(FCProfileData d, string p, float def)
			{
				fcd = d;
				defaultValue = def;
				defaultValueThrottle = def;
				fcdThrottleKey = "fcp-" + p + "-throttle";
				fcdYawKey = "fcp-" + p + "-yaw";
				fcdPitchRollKey = "fcp-" + p + "-pitch-roll";
			}

			public Betaflight(FCProfileData d, string p, float def, float def2)
			{
				fcd = d;
				defaultValue = def;
				defaultValueThrottle = def2;
				fcdThrottleKey = "fcp-" + p + "-throttle";
				fcdYawKey = "fcp-" + p + "-yaw";
				fcdPitchRollKey = "fcp-" + p + "-pitch-roll";
			}

			public static PresetType GetPreset(ControllerStateType p_controller, float p_pitchroll_r, float p_pitchroll_sr, float p_pitchroll_e, float p_yaw_r, float p_yaw_sr, float p_yaw_e, float p_throttle_m, float p_throttle_e)
			{
				if (EqualToPreset(LowPresets[p_controller], p_pitchroll_r, p_pitchroll_sr, p_pitchroll_e, p_yaw_r, p_yaw_sr, p_yaw_e, p_throttle_m, p_throttle_e))
				{
					return PresetType.Low;
				}
				if (EqualToPreset(MediumPresets[p_controller], p_pitchroll_r, p_pitchroll_sr, p_pitchroll_e, p_yaw_r, p_yaw_sr, p_yaw_e, p_throttle_m, p_throttle_e))
				{
					return PresetType.Medium;
				}
				if (EqualToPreset(HighPresets[p_controller], p_pitchroll_r, p_pitchroll_sr, p_pitchroll_e, p_yaw_r, p_yaw_sr, p_yaw_e, p_throttle_m, p_throttle_e))
				{
					return PresetType.High;
				}
				if (EqualToPreset(TrainingPresets[p_controller], p_pitchroll_r, p_pitchroll_sr, p_pitchroll_e, p_yaw_r, p_yaw_sr, p_yaw_e, p_throttle_m, p_throttle_e))
				{
					return PresetType.Training;
				}
				return PresetType.None;
			}

			public static bool EqualToPreset(Preset p_preset, float p_pitchroll_r, float p_pitchroll_sr, float p_pitchroll_e, float p_yaw_r, float p_yaw_sr, float p_yaw_e, float p_throttle_m, float p_throttle_e)
			{
				if (Mathf.RoundToInt(p_preset.prRcRate * 100f) != Mathf.RoundToInt(p_pitchroll_r * 100f))
				{
					return false;
				}
				if (Mathf.RoundToInt(p_preset.prSuperRate * 100f) != Mathf.RoundToInt(p_pitchroll_sr * 100f))
				{
					return false;
				}
				if (Mathf.RoundToInt(p_preset.prExpo * 100f) != Mathf.RoundToInt(p_pitchroll_e * 100f))
				{
					return false;
				}
				if (Mathf.RoundToInt(p_preset.yawRcRate * 100f) != Mathf.RoundToInt(p_yaw_r * 100f))
				{
					return false;
				}
				if (Mathf.RoundToInt(p_preset.yawSuperRate * 100f) != Mathf.RoundToInt(p_yaw_sr * 100f))
				{
					return false;
				}
				if (Mathf.RoundToInt(p_preset.yawExpo * 100f) != Mathf.RoundToInt(p_yaw_e * 100f))
				{
					return false;
				}
				if (Mathf.RoundToInt(p_preset.tMid * 100f) != Mathf.RoundToInt(p_throttle_m * 100f))
				{
					return false;
				}
				if (Mathf.RoundToInt(p_preset.tExpo * 100f) != Mathf.RoundToInt(p_throttle_e * 100f))
				{
					return false;
				}
				return true;
			}
		}

		public class BetaflightRanges
		{
			[NonSerialized]
			private FCProfileData fcd;

			public float throttle => BetaflightRates.GetMax(fcd.superRate.throttle, fcd.rcRate.throttle, fcd.expo.throttle);

			public float yaw => BetaflightRates.GetMax(fcd.superRate.yaw, fcd.rcRate.yaw, fcd.expo.yaw);

			public float pitch => BetaflightRates.GetMax(fcd.superRate.pitch, fcd.rcRate.pitch, fcd.expo.pitch);

			public float roll => BetaflightRates.GetMax(fcd.superRate.roll, fcd.rcRate.roll, fcd.expo.roll);

			public float pitchRoll => (pitch + roll) * 0.5f;

			public BetaflightRanges(FCProfileData d)
			{
				fcd = d;
			}
		}

		public class PID
		{
			[NonSerialized]
			private FCProfileData fcd;

			private PIDVector m_yaw;

			private PIDVector m_pitch;

			private PIDVector m_roll;

			public static PIDVector DefaultYaw => new PIDVector(55f, 0f, 0f);

			public static PIDVector DefaultPitch => new PIDVector(40f, 0f, 50f);

			public static PIDVector DefaultRoll => new PIDVector(40f, 0f, 50f);

			public PIDVector yaw
			{
				get
				{
					fcd.UpdateCached();
					return m_yaw;
				}
				set
				{
					m_yaw = value;
					fcd.Set("fcp-pid-yaw-p", value.p);
					fcd.Set("fcp-pid-yaw-i", value.i);
					fcd.Set("fcp-pid-yaw-d", value.d);
				}
			}

			public PIDVector pitch
			{
				get
				{
					fcd.UpdateCached();
					return m_pitch;
				}
				set
				{
					m_pitch = value;
					fcd.Set("fcp-pid-pitch-p", value.p);
					fcd.Set("fcp-pid-pitch-i", value.i);
					fcd.Set("fcp-pid-pitch-d", value.d);
				}
			}

			public PIDVector roll
			{
				get
				{
					fcd.UpdateCached();
					return m_roll;
				}
				set
				{
					m_roll = value;
					fcd.Set("fcp-pid-roll-p", value.p);
					fcd.Set("fcp-pid-roll-i", value.i);
					fcd.Set("fcp-pid-roll-d", value.d);
				}
			}

			public PID(FCProfileData d)
			{
				fcd = d;
			}
		}

		private Betaflight m_superRate;

		private Betaflight m_rcRate;

		private Betaflight m_expo;

		private BetaflightRanges m_max;

		private PID m_pid;

		private float m_fov;

		public static float lensDistortionFOVOffset = 28f;

		private float m_tilt;

		private bool m_cached;

		public string guid
		{
			get
			{
				string text = Get("fcp-guid", "");
				if (string.IsNullOrEmpty(text) || text.Length < 12)
				{
					text = GUID.Create(12, "", 200, 0, 15, "x1");
					Set("fcp-guid", text);
				}
				return text;
			}
			set
			{
				string text = value;
				if (string.IsNullOrEmpty(text) || text.Length < 12)
				{
					text = GUID.Create(12, "", 200, 0, 15, "x1");
				}
				Set("fcp-guid", text);
			}
		}

		public Betaflight superRate => m_superRate ?? (m_superRate = new Betaflight(this, "super-rate", 0f));

		public Betaflight rcRate => m_rcRate ?? (m_rcRate = new Betaflight(this, "rc-rate", 2.1287f, 2.5142f));

		public Betaflight expo => m_expo ?? (m_expo = new Betaflight(this, "expo", 0f));

		public BetaflightRanges max => m_max ?? (m_max = new BetaflightRanges(this));

		public PID pid => m_pid ?? (m_pid = new PID(this));

		public float fov
		{
			get
			{
				UpdateCached();
				return m_fov;
			}
			set
			{
				m_fov = value;
				Set("fcp-drone-fov", value);
			}
		}

		public float tilt
		{
			get
			{
				UpdateCached();
				return m_tilt;
			}
			set
			{
				m_tilt = value;
				Set("fcp-drone-tilt", value);
			}
		}

		public FCProfileData()
		{
			guid = "";
			SetPreset(Betaflight.LowPresets[ControllerStateType.XBox]);
		}

		public FCProfileData(Betaflight.Preset p_ratesPreset)
		{
			guid = "";
			SetPreset(p_ratesPreset);
		}

		public void SetData(FCProfileData p_data)
		{
			if (p_data == null)
			{
				Debug.LogWarning("FCProfileData> SetData / Tried to apply null data!");
				return;
			}
			rcRate.pitch = p_data.rcRate.pitch;
			rcRate.roll = p_data.rcRate.roll;
			rcRate.yaw = p_data.rcRate.yaw;
			superRate.pitch = p_data.superRate.pitch;
			superRate.roll = p_data.superRate.roll;
			superRate.yaw = p_data.superRate.yaw;
			expo.pitch = p_data.expo.pitch;
			expo.roll = p_data.expo.roll;
			expo.yaw = p_data.expo.yaw;
			superRate.throttle = p_data.superRate.throttle;
			expo.throttle = p_data.expo.throttle;
		}

		public void SetPreset(Betaflight.Preset p_ratesPreset)
		{
			if (p_ratesPreset == null)
			{
				Debug.LogWarning("FCProfileData> SetPreset / Tried to apply null data!");
				return;
			}
			rcRate.pitchRoll = p_ratesPreset.prRcRate;
			rcRate.yaw = p_ratesPreset.yawRcRate;
			superRate.pitchRoll = p_ratesPreset.prSuperRate;
			superRate.yaw = p_ratesPreset.yawSuperRate;
			expo.pitchRoll = p_ratesPreset.prExpo;
			expo.yaw = p_ratesPreset.yawExpo;
			expo.throttle = p_ratesPreset.tExpo;
			superRate.throttle = p_ratesPreset.tMid;
		}

		public override void Set(string k, object v)
		{
			string text = v.GetType().ToString();
			if (text != null && text == "System.Double")
			{
				base[k] = (float)(double)v;
			}
			else
			{
				base[k] = v;
			}
		}

		public override void RefreshCached()
		{
		}

		public void UpdateCached()
		{
			if (!m_cached)
			{
				m_cached = true;
				m_fov = Get("fcp-drone-fov", 83f);
				m_tilt = Get("fcp-drone-tilt", 30f);
				if (m_pid == null)
				{
					m_pid = new PID(this);
				}
				m_pid.yaw = new PIDVector(Get("fcp-pid-yaw-p", PID.DefaultYaw.p), Get("fcp-pid-yaw-i", PID.DefaultYaw.i), Get("fcp-pid-yaw-d", PID.DefaultYaw.d));
				m_pid.pitch = new PIDVector(Get("fcp-pid-pitch-p", PID.DefaultPitch.p), Get("fcp-pid-pitch-i", PID.DefaultPitch.i), Get("fcp-pid-pitch-d", PID.DefaultPitch.d));
				m_pid.roll = new PIDVector(Get("fcp-pid-roll-p", PID.DefaultRoll.p), Get("fcp-pid-roll-i", PID.DefaultRoll.i), Get("fcp-pid-roll-d", PID.DefaultRoll.d));
				if (m_superRate == null)
				{
					m_superRate = new Betaflight(this, "super-rate", 0f);
				}
				if (m_rcRate == null)
				{
					m_rcRate = new Betaflight(this, "rc-rate", 2.1287f, 2.5142f);
				}
				if (m_expo == null)
				{
					m_expo = new Betaflight(this, "expo", 0f);
				}
				m_superRate.pitch = Get("fcp-super-rate-pitch-rollp", 0f);
				m_superRate.roll = Get("fcp-super-rate-pitch-rollr", 0f);
				m_superRate.yaw = Get("fcp-super-rate-yaw", 0f);
				m_superRate.throttle = Get("fcp-super-rate-throttle", 0f);
				m_rcRate.pitch = Get("fcp-rc-rate-pitch-rollp", 2.1287f);
				m_rcRate.roll = Get("fcp-rc-rate-pitch-rollr", 2.1287f);
				m_rcRate.yaw = Get("fcp-rc-rate-yaw", 2.1287f);
				m_rcRate.throttle = Get("fcp-rc-rate-throttle", 2.5142f);
				m_expo.pitch = Get("fcp-expo-pitch-rollp", 0f);
				m_expo.roll = Get("fcp-expo-pitch-rollr", 0f);
				m_expo.yaw = Get("fcp-expo-yaw", 0f);
				m_expo.throttle = Get("fcp-expo-throttle", 0f);
			}
		}

		public override void RefreshStored()
		{
		}
	}
}
