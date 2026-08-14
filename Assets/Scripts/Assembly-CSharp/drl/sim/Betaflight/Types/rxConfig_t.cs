namespace drl.sim.Betaflight.Types
{
	public class rxConfig_t
	{
		public int[] rcmap = new int[8];

		public int serialrx_provider;

		public int sbus_inversion;

		public int halfDuplex;

		public int rx_spi_protocol;

		public int rx_spi_id;

		public int rx_spi_rf_channel_count;

		public int spektrum_bind_pin_override_ioTag;

		public int spektrum_bind_plug_ioTag;

		public int spektrum_sat_bind;

		public int spektrum_sat_bind_autoreset;

		public int rssi_channel;

		public int rssi_scale;

		public int rssi_invert;

		public int midrc;

		public int mincheck;

		public int maxcheck;

		public int rcInterpolation;

		public int rcInterpolationChannels;

		public int rcInterpolationInterval;

		public int fpvCamAngleDegrees;

		public int airModeActivateThreshold;

		public int rx_min_usec;

		public int rx_max_usec;

		public int max_aux_channel;
	}
}
