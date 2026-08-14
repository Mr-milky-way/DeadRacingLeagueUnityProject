namespace drl.sim.thread
{
	public struct PidProfile
	{
		public ushort[] PIDF_roll;

		public ushort[] PIDF_pitch;

		public ushort[] PIDF_yaw;

		public ushort[] PIDF_level;

		public ushort yaw_lowpass_hz;

		public ushort dterm_lowpass_hz;

		public ushort dterm_notch_hz;

		public ushort dterm_notch_cutoff;

		public byte dterm_filter_type;

		public byte itermWindupPointPercent;

		public ushort pidSumLimit;

		public ushort pidSumLimitYaw;

		public byte pidAtMinThrottle;

		public byte levelAngleLimit;

		public byte horizon_tilt_effect;

		public bool horizon_tilt_expert_mode;

		public byte antiGravityMode;

		public ushort itermThrottleThreshold;

		public ushort itermAcceleratorGain;

		public ushort yawRateAccelLimit;

		public ushort rateAccelLimit;

		public ushort crash_dthreshold;

		public ushort crash_gthreshold;

		public ushort crash_setpoint_threshold;

		public ushort crash_time;

		public ushort crash_delay;

		public byte crash_recovery_angle;

		public byte crash_recovery_rate;

		public byte vbatPidCompensation;

		public byte feedForwardTransition;

		public ushort crash_limit_yaw;

		public ushort itermLimit;

		public ushort dterm_lowpass2_hz;

		public byte crash_recovery;

		public byte throttle_boost;

		public byte throttle_boost_cutoff;

		public bool iterm_rotation;

		public bool smart_feedforward;

		public byte iterm_relax_type;

		public byte iterm_relax_cutoff;

		public byte iterm_relax;

		public byte acro_trainer_angle_limit;

		public byte acro_trainer_debug_axis;

		public byte acro_trainer_gain;

		public ushort acro_trainer_lookahead_ms;

		public byte abs_control_gain;

		public byte abs_control_limit;

		public byte abs_control_error_limit;
	}
}
