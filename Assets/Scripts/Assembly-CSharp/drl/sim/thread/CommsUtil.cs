using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace drl.sim.thread
{
	public class CommsUtil
	{
		public static object ByteArrayToObject(byte[] arrBytes)
		{
			using MemoryStream memoryStream = new MemoryStream();
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			memoryStream.Write(arrBytes, 0, arrBytes.Length);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return binaryFormatter.Deserialize(memoryStream);
		}

		public static byte[] PrepareUpdateConfiguration(PidProfile pidProfile, ControlRate controlRate, MotorConfig motorConfig)
		{
			using MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			for (int i = 0; i < 4; i++)
			{
				binaryWriter.Write(pidProfile.PIDF_roll[i]);
			}
			for (int j = 0; j < 4; j++)
			{
				binaryWriter.Write(pidProfile.PIDF_pitch[j]);
			}
			for (int k = 0; k < 4; k++)
			{
				binaryWriter.Write(pidProfile.PIDF_yaw[k]);
			}
			for (int l = 0; l < 4; l++)
			{
				binaryWriter.Write(pidProfile.PIDF_level[l]);
			}
			binaryWriter.Write(pidProfile.abs_control_error_limit);
			binaryWriter.Write(pidProfile.abs_control_gain);
			binaryWriter.Write(pidProfile.abs_control_limit);
			binaryWriter.Write(pidProfile.acro_trainer_angle_limit);
			binaryWriter.Write(pidProfile.acro_trainer_debug_axis);
			binaryWriter.Write(pidProfile.acro_trainer_gain);
			binaryWriter.Write(pidProfile.acro_trainer_lookahead_ms);
			binaryWriter.Write(pidProfile.antiGravityMode);
			binaryWriter.Write(pidProfile.crash_delay);
			binaryWriter.Write(pidProfile.crash_dthreshold);
			binaryWriter.Write(pidProfile.crash_gthreshold);
			binaryWriter.Write(pidProfile.crash_limit_yaw);
			binaryWriter.Write(pidProfile.crash_recovery);
			binaryWriter.Write(pidProfile.crash_recovery_angle);
			binaryWriter.Write(pidProfile.crash_recovery_rate);
			binaryWriter.Write(pidProfile.crash_setpoint_threshold);
			binaryWriter.Write(pidProfile.crash_time);
			binaryWriter.Write(pidProfile.dterm_filter_type);
			binaryWriter.Write(pidProfile.dterm_lowpass2_hz);
			binaryWriter.Write(pidProfile.dterm_lowpass_hz);
			binaryWriter.Write(pidProfile.dterm_notch_cutoff);
			binaryWriter.Write(pidProfile.dterm_notch_hz);
			binaryWriter.Write(pidProfile.feedForwardTransition);
			binaryWriter.Write(pidProfile.horizon_tilt_effect);
			binaryWriter.Write(pidProfile.horizon_tilt_expert_mode);
			binaryWriter.Write(pidProfile.itermAcceleratorGain);
			binaryWriter.Write(pidProfile.itermLimit);
			binaryWriter.Write(pidProfile.itermThrottleThreshold);
			binaryWriter.Write(pidProfile.itermWindupPointPercent);
			binaryWriter.Write(pidProfile.iterm_relax);
			binaryWriter.Write(pidProfile.iterm_relax_cutoff);
			binaryWriter.Write(pidProfile.iterm_relax_type);
			binaryWriter.Write(pidProfile.iterm_rotation);
			binaryWriter.Write(pidProfile.levelAngleLimit);
			binaryWriter.Write(pidProfile.pidAtMinThrottle);
			binaryWriter.Write(pidProfile.pidSumLimit);
			binaryWriter.Write(pidProfile.pidSumLimitYaw);
			binaryWriter.Write(pidProfile.rateAccelLimit);
			binaryWriter.Write(pidProfile.smart_feedforward);
			binaryWriter.Write(pidProfile.throttle_boost);
			binaryWriter.Write(pidProfile.throttle_boost_cutoff);
			binaryWriter.Write(pidProfile.vbatPidCompensation);
			binaryWriter.Write(pidProfile.yawRateAccelLimit);
			binaryWriter.Write(pidProfile.yaw_lowpass_hz);
			binaryWriter.Write(controlRate.thrMid8);
			binaryWriter.Write(controlRate.thrExpo8);
			binaryWriter.Write(controlRate.dynThrPID);
			binaryWriter.Write(controlRate.tpa_breakpoint);
			binaryWriter.Write(controlRate.rates_type);
			binaryWriter.Write(controlRate.rcRates[0]);
			binaryWriter.Write(controlRate.rcRates[1]);
			binaryWriter.Write(controlRate.rcRates[2]);
			binaryWriter.Write(controlRate.rcExpo[0]);
			binaryWriter.Write(controlRate.rcExpo[1]);
			binaryWriter.Write(controlRate.rcExpo[2]);
			binaryWriter.Write(controlRate.rates[0]);
			binaryWriter.Write(controlRate.rates[1]);
			binaryWriter.Write(controlRate.rates[2]);
			binaryWriter.Write(controlRate.throttle_limit_type);
			binaryWriter.Write(controlRate.throttle_limit_percent);
			binaryWriter.Write(motorConfig.minthrottle);
			binaryWriter.Write(motorConfig.maxthrottle);
			binaryWriter.Write(motorConfig.mincommand);
			return memoryStream.ToArray();
		}
	}
}
