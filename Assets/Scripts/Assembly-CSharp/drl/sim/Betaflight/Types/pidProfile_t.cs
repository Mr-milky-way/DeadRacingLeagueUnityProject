namespace drl.sim.Betaflight.Types
{
	public class pidProfile_t
	{
		public pid8_t[] pid = new pid8_t[10];

		public int yaw_lpf_hz;

		public int dterm_lpf_hz;

		public int dterm_notch_hz;

		public int dterm_notch_cutoff;

		public int dterm_filter_type;

		public int itermWindupPointPercent;

		public int pidSumLimit;

		public int pidSumLimitYaw;

		public int vbatPidCompensation;

		public int pidAtMinThrottle;

		public int levelAngleLimit;

		public int horizon_tilt_effect;

		public int horizon_tilt_expert_mode;

		public int itermThrottleThreshold;

		public int itermAcceleratorGain;

		public int setpointRelaxRatio;

		public int dtermSetpointWeight;

		public int yawRateAccelLimit;

		public int rateAccelLimit;

		public int crash_dthreshold;

		public int crash_gthreshold;

		public int crash_setpoint_threshold;

		public int crash_time;

		public int crash_delay;

		public int crash_recovery_angle;

		public int crash_recovery_rate;

		public pidCrashRecovery_e crash_recovery;

		public int crash_limit_yaw;

		public int itermLimit;
	}
}
