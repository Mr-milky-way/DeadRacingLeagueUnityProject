using System;
using drl.sim.Betaflight.Types;

namespace drl.sim.Betaflight
{
	public class FC_RC
	{
		private static controlRateConfig_t currentControlRateProfile = new controlRateConfig_t();

		private static rxConfig_t rxConfig = new rxConfig_t();

		private static pidProfile_t currentPidProfile = new pidProfile_t();

		public const int PWM_RANGE_MIN = 1000;

		public const int PWM_RANGE_MAX = 2000;

		private static float[] rcCommand = new float[4];

		private bool isRXDataNew;

		private rxRuntimeConfig_t rxRuntimeConfig;

		private int[] rcData = new int[18];

		private rcControlsConfig_t rcControlsConfig = new rcControlsConfig_t();

		private static lowVoltageCutoff_t lowVoltageCutoff;

		private static failsafeState_t failsafeState;

		private int headFreeModeHold;

		private attitudeEulerAngles_t attitude = new attitudeEulerAngles_t();

		private static float[] setpointRate = new float[3];

		private static float[] rcDeflection = new float[3];

		private static float[] rcDeflectionAbs = new float[3];

		private static float throttlePIDAttenuation;

		public const int THROTTLE_LOOKUP_LENGTH = 12;

		private static int[] lookupThrottleRC = new int[12];

		public const float SETPOINT_RATE_LIMIT = 1998f;

		public const float RC_RATE_INCREMENTAL = 14.54f;

		private static int lastFpvCamAngleDegrees = 0;

		private static float cosFactor = 1f;

		private static float sinFactor = 0f;

		public const int THROTTLE_BUFFER_MAX = 20;

		public const int THROTTLE_DELTA_MS = 100;

		private static int index;

		private static int[] rcCommandThrottlePrevious = new int[20];

		private static float[] rcCommandInterp = new float[4];

		private static float[] rcStepSize = new float[4];

		private static int rcInterpolationStepCount;

		private static int currentRxRefreshRate;

		private float getSetpointRate(int axis)
		{
			return setpointRate[axis];
		}

		private float getRcDeflection(int axis)
		{
			return rcDeflection[axis];
		}

		private float getRcDeflectionAbs(int axis)
		{
			return rcDeflectionAbs[axis];
		}

		private float getThrottlePIDAttenuation()
		{
			return throttlePIDAttenuation;
		}

		private void generateThrottleCurve()
		{
			for (int i = 0; i < 12; i++)
			{
				int num = 10 * i - currentControlRateProfile.thrMid8;
				int num2 = 1;
				if (num > 0)
				{
					num2 = 100 - currentControlRateProfile.thrMid8;
				}
				if (num < 0)
				{
					num2 = currentControlRateProfile.thrMid8;
				}
				lookupThrottleRC[i] = 10 * currentControlRateProfile.thrMid8 + num * (100 - currentControlRateProfile.thrExpo8 + currentControlRateProfile.thrExpo8 * (num * num) / (num2 * num2)) / 10;
				lookupThrottleRC[i] = 1000 + 1000 * lookupThrottleRC[i] / 1000;
			}
		}

		private static int rcLookupThrottle(int tmp)
		{
			int num = tmp / 100;
			return lookupThrottleRC[num] + (tmp - num * 100) * (lookupThrottleRC[num + 1] - lookupThrottleRC[num]) / 100;
		}

		private static void calculateSetpointRate(int axis)
		{
			int num;
			float num2;
			if (axis != 2)
			{
				num = currentControlRateProfile.rcExpo8;
				num2 = (float)currentControlRateProfile.rcRate8 / 100f;
			}
			else
			{
				num = currentControlRateProfile.rcYawExpo8;
				num2 = (float)currentControlRateProfile.rcYawRate8 / 100f;
			}
			if (num2 > 2f)
			{
				num2 += 14.54f * (num2 - 2f);
			}
			float num3 = rcCommand[axis] / 500f;
			rcDeflection[axis] = num3;
			float num4 = BF.ABS(num3);
			rcDeflectionAbs[axis] = num4;
			if (num != 0)
			{
				float num5 = (float)num / 100f;
				num3 = num3 * BF.power3(num4) * num5 + num3 * (1f - num5);
			}
			float num6 = 200f * num2 * num3;
			if (currentControlRateProfile.rates[axis] != 0)
			{
				float num7 = 1f / BF.constrainf(1f - num4 * ((float)currentControlRateProfile.rates[axis] / 100f), 0.01f, 1f);
				num6 *= num7;
			}
			setpointRate[axis] = BF.constrainf(num6, -1998f, 1998f);
		}

		private static void scaleRcCommandToFpvCamAngle()
		{
			if (lastFpvCamAngleDegrees != rxConfig.fpvCamAngleDegrees)
			{
				lastFpvCamAngleDegrees = rxConfig.fpvCamAngleDegrees;
				cosFactor = BF.cos_approx((float)rxConfig.fpvCamAngleDegrees * ((float)Math.PI / 180f));
				sinFactor = BF.sin_approx((float)rxConfig.fpvCamAngleDegrees * ((float)Math.PI / 180f));
			}
			float num = setpointRate[0];
			float num2 = setpointRate[2];
			setpointRate[0] = BF.constrainf(num * cosFactor - num2 * sinFactor, -1998f, 1998f);
			setpointRate[2] = BF.constrainf(num2 * cosFactor + num * sinFactor, -1998f, 1998f);
		}

		private static void checkForThrottleErrorResetState(int rxRefreshRate)
		{
			int num = rxRefreshRate / 1000;
			int num2 = BF.constrain(100 / num, 1, 20);
			int num3 = (BF.feature(features_e.FEATURE_3D) ? (currentPidProfile.itermThrottleThreshold / 2) : currentPidProfile.itermThrottleThreshold);
			rcCommandThrottlePrevious[index++] = (int)rcCommand[3];
			if (index >= num2)
			{
				index = 0;
			}
			if (BF.ABS((int)rcCommand[3] - rcCommandThrottlePrevious[index]) > num3)
			{
				PID.pidSetItermAccelerator(BF.CONVERT_PARAMETER_TO_FLOAT(currentPidProfile.itermAcceleratorGain));
			}
			else
			{
				PID.pidSetItermAccelerator(1f);
			}
		}

		private void processRcCommand()
		{
			if (isRXDataNew)
			{
				currentRxRefreshRate = BF.constrain(BF.getTaskDeltaTime(), 1000, 20000);
				if (BF.isAntiGravityModeActive())
				{
					checkForThrottleErrorResetState(currentRxRefreshRate);
				}
			}
			int num = rxConfig.rcInterpolationChannels + 2;
			bool flag = false;
			int num2 = 0;
			if (rxConfig.rcInterpolation != 0)
			{
				int num3 = rxConfig.rcInterpolation switch
				{
					2 => currentRxRefreshRate + 1000, 
					3 => 1000 * rxConfig.rcInterpolationInterval, 
					_ => rxRuntimeConfig.rxRefreshRate, 
				};
				if (isRXDataNew && num3 > 0)
				{
					rcInterpolationStepCount = num3 / PID.targetPidLooptime;
					for (int i = 0; i < num; i++)
					{
						rcStepSize[i] = (rcCommand[i] - rcCommandInterp[i]) / (float)rcInterpolationStepCount;
					}
				}
				else
				{
					rcInterpolationStepCount--;
				}
				if (rcInterpolationStepCount > 0)
				{
					for (int j = 0; j < num; j++)
					{
						rcCommandInterp[j] += rcStepSize[j];
						rcCommand[j] = rcCommandInterp[j];
						num2 = BF.MAX(j, 2);
					}
					flag = true;
				}
			}
			else
			{
				rcInterpolationStepCount = 0;
			}
			if (flag || isRXDataNew)
			{
				if (isRXDataNew)
				{
					num2 = 2;
				}
				for (int k = 0; k <= num2; k++)
				{
					calculateSetpointRate(k);
				}
				if (rxConfig.fpvCamAngleDegrees != 0 && BF.IS_RC_MODE_ACTIVE(boxId_e.BOXFPVANGLEMIX) && !BF.FLIGHT_MODE(flightModeFlags_e.HEADFREE_MODE))
				{
					scaleRcCommandToFpvCamAngle();
				}
				isRXDataNew = false;
			}
		}

		private void updateRcCommands()
		{
			if (rcData[3] < currentControlRateProfile.tpa_breakpoint)
			{
				int num = 100;
				throttlePIDAttenuation = 1f;
			}
			else
			{
				int num = ((rcData[3] >= 2000) ? (100 - currentControlRateProfile.dynThrPID) : (100 - currentControlRateProfile.dynThrPID * (rcData[3] - currentControlRateProfile.tpa_breakpoint) / (2000 - currentControlRateProfile.tpa_breakpoint)));
				throttlePIDAttenuation = (float)num / 100f;
			}
			int num2;
			for (int i = 0; i < 3; i++)
			{
				num2 = BF.MIN(BF.ABS(rcData[i] - rxConfig.midrc), 500);
				if (i == 0 || i == 1)
				{
					num2 = ((num2 > rcControlsConfig.deadband) ? (num2 - rcControlsConfig.deadband) : 0);
					rcCommand[i] = num2;
				}
				else
				{
					num2 = ((num2 > rcControlsConfig.yaw_deadband) ? (num2 - rcControlsConfig.yaw_deadband) : 0);
					rcCommand[i] = num2 * -BF.GET_DIRECTION(rcControlsConfig.yaw_control_reversed);
				}
				if (rcData[i] < rxConfig.midrc)
				{
					rcCommand[i] = 0f - rcCommand[i];
				}
			}
			if (BF.feature(features_e.FEATURE_3D))
			{
				num2 = BF.constrain(rcData[3], 1000, 2000);
				num2 -= 1000;
				if (lowVoltageCutoff.enabled)
				{
					num2 = num2 * lowVoltageCutoff.percentage / 100;
				}
			}
			else
			{
				num2 = BF.constrain(rcData[3], rxConfig.mincheck, 2000);
				num2 = (num2 - rxConfig.mincheck) * 1000 / (2000 - rxConfig.mincheck);
				if (lowVoltageCutoff.enabled)
				{
					num2 = num2 * lowVoltageCutoff.percentage / 100;
				}
			}
			rcCommand[3] = rcLookupThrottle(num2);
			if (BF.feature(features_e.FEATURE_3D) && BF.IS_RC_MODE_ACTIVE(boxId_e.BOX3DDISABLE) && !failsafeState.active)
			{
				int q = BF.qConstruct((int)rcCommand[3] - 1000, 1000);
				rcCommand[3] = rxConfig.midrc + BF.qMultiply(q, 2000 - rxConfig.midrc);
			}
			if (BF.FLIGHT_MODE(flightModeFlags_e.HEADFREE_MODE))
			{
				float angle = BF.degreesToRadians(BF.DECIDEGREES_TO_DEGREES(attitude.values.yaw) - headFreeModeHold);
				float num3 = BF.cos_approx(angle);
				float num4 = BF.sin_approx(angle);
				float num5 = rcCommand[1] * num3 + rcCommand[0] * num4;
				rcCommand[0] = rcCommand[0] * num3 - rcCommand[1] * num4;
				rcCommand[1] = num5;
			}
		}

		private void resetYawAxis()
		{
			rcCommand[2] = 0f;
			setpointRate[2] = 0f;
		}
	}
}
