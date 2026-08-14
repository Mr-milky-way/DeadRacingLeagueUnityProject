using System;
using UnityEngine;

namespace drl.sim
{
	[Serializable]
	public struct SignalVector
	{
		public float throttle;

		public float altitude;

		public float yaw;

		public float pitch;

		public float roll;

		public float magnitude => new Vector4(throttle, yaw, pitch, roll).magnitude;

		public float maneauver => new Vector3(yaw, pitch, roll).magnitude;

		public SignalVector Set(float t, float y, float p, float r)
		{
			throttle = t;
			yaw = y;
			pitch = p;
			roll = r;
			return this;
		}

		public void Add(SignalVector p_v)
		{
			throttle += p_v.throttle;
			yaw += p_v.yaw;
			pitch += p_v.pitch;
			roll += p_v.roll;
			altitude += p_v.altitude;
		}

		public void Scale(SignalVector p_v)
		{
			throttle *= p_v.throttle;
			yaw *= p_v.yaw;
			pitch *= p_v.pitch;
			roll *= p_v.roll;
			altitude *= p_v.altitude;
		}

		public void Scale(float p_v)
		{
			throttle *= p_v;
			yaw *= p_v;
			pitch *= p_v;
			roll *= p_v;
			altitude *= p_v;
		}

		public void Noise(float p_strength)
		{
			float t = Mathf.Clamp01(p_strength);
			throttle *= Mathf.Lerp(1f, UnityEngine.Random.value, t);
			roll *= Mathf.Lerp(1f, UnityEngine.Random.value, t);
			pitch *= Mathf.Lerp(1f, UnityEngine.Random.value, t);
			yaw *= Mathf.Lerp(1f, UnityEngine.Random.value, t);
			altitude *= Mathf.Lerp(1f, UnityEngine.Random.value, t);
		}

		public static SignalVector Lerp(SignalVector v0, SignalVector v1, float r)
		{
			return new SignalVector
			{
				roll = Mathf.Lerp(v0.roll, v1.roll, r),
				pitch = Mathf.Lerp(v0.pitch, v1.pitch, r),
				yaw = Mathf.Lerp(v0.yaw, v1.yaw, r),
				throttle = Mathf.Lerp(v0.throttle, v1.throttle, r),
				altitude = Mathf.Lerp(v0.altitude, v1.altitude, r)
			};
		}

		public static SignalVector Pow(SignalVector v, SignalVector exp)
		{
			SignalVector result = new SignalVector
			{
				roll = Mathf.Pow(Mathf.Abs(v.roll), exp.roll),
				pitch = Mathf.Pow(Mathf.Abs(v.pitch), exp.pitch),
				yaw = Mathf.Pow(Mathf.Abs(v.yaw), exp.yaw),
				throttle = Mathf.Pow(Mathf.Abs(v.throttle), exp.throttle),
				altitude = Mathf.Pow(Mathf.Abs(v.altitude), exp.altitude)
			};
			if (v.roll < 0f)
			{
				result.roll = 0f - result.roll;
			}
			if (v.pitch < 0f)
			{
				result.pitch = 0f - result.pitch;
			}
			if (v.yaw < 0f)
			{
				result.yaw = 0f - result.yaw;
			}
			if (v.throttle < 0f)
			{
				result.throttle = 0f - result.throttle;
			}
			if (v.altitude < 0f)
			{
				result.altitude = 0f - result.altitude;
			}
			return result;
		}

		public static SignalVector Pow(SignalVector v, float exp)
		{
			return new SignalVector
			{
				roll = Mathf.Pow(v.roll, exp),
				pitch = Mathf.Pow(v.pitch, exp),
				yaw = Mathf.Pow(v.yaw, exp),
				throttle = Mathf.Pow(v.throttle, exp),
				altitude = Mathf.Pow(v.altitude, exp)
			};
		}

		public static SignalVector CleanFlightExpoRate(SignalVector p_sig, SignalVector p_expo, SignalVector p_scale, bool p_usingLimits)
		{
			SignalVector result = new SignalVector
			{
				roll = CleanFlightStylePitchRoll(p_sig.roll, p_expo.roll, p_scale.roll, p_usingLimits),
				pitch = CleanFlightStylePitchRoll(p_sig.pitch, p_expo.pitch, p_scale.pitch, p_usingLimits),
				yaw = CleanFlightStyleYaw(p_sig.yaw, p_expo.yaw, p_usingLimits),
				throttle = CleanFlightStyleThrottle(p_sig.throttle, p_expo.throttle, 1f, p_usingLimits),
				altitude = CleanFlightStyleThrottle(p_sig.altitude, p_expo.altitude, 1f, p_usingLimits)
			};
			if (p_sig.throttle < 0f)
			{
				result.throttle = 0f - result.throttle;
			}
			if (p_sig.altitude < 0f)
			{
				result.altitude = 0f - result.altitude;
			}
			return result;
		}

		public static float CleanFlightStylePitchRoll(float p_rawSig, float p_expo, float p_rate, bool useClamps = true)
		{
			float num = (useClamps ? Mathf.Clamp(p_expo, 0f, 1f) : p_expo);
			float num2 = (useClamps ? Mathf.Clamp(p_rate, 0f, 2.5f) : p_rate);
			float num3 = (useClamps ? Mathf.Clamp(p_rawSig, -1f, 1f) : p_rawSig);
			float num4 = num * num2 * (num3 * num3 * num3) - num * num2 * num3 + num2 * num3;
			if (float.IsNaN(num4))
			{
				return 0f;
			}
			return num4;
		}

		public static float CleanFlightStyleYaw(float p_rawSig, float p_expo, bool useClamps = true)
		{
			float num = (useClamps ? Mathf.Clamp(p_expo, 0f, 1f) : p_expo);
			float num2 = (useClamps ? Mathf.Clamp(p_rawSig, -1f, 1f) : p_rawSig);
			float num3 = num * (num2 * num2 * num2) - num * num2 + num2;
			if (float.IsNaN(num3))
			{
				return 0f;
			}
			return num3;
		}

		public static float CleanFlightStyleThrottle(float p_rawSig, float p_expo, float p_mid, bool useClamps = true)
		{
			float num = (useClamps ? Mathf.Clamp(p_expo, 0f, 1f) : p_expo);
			float num2 = 1f;
			float num3 = (useClamps ? Mathf.Clamp(p_rawSig, -1f, 1f) : p_rawSig);
			float num4 = num * num2 * (num3 * num3 * num3) - num * num2 * num3 + num2 * num3;
			if (float.IsNaN(num4))
			{
				return 0f;
			}
			return num4;
		}
	}
}
