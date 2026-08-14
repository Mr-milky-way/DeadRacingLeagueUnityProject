namespace drl.sim
{
	public class DronePropTypePrefix
	{
		public const string PointNose = "PN";

		public const string BullNose = "BN";

		public const string HybridBullNose = "HN";

		public const string ButterCutter = "BC";

		public const string Special0 = "S0";

		public const string Blade2 = "2B";

		public const string Blade3 = "3B";

		public const string Blade4 = "4B";

		public const string Blade5 = "5B";

		public const string Blade6 = "6B";

		public static string FromEnum(DronePropType p_type)
		{
			return p_type switch
			{
				DronePropType.BullNose => "BN", 
				DronePropType.PointNose => "PN", 
				DronePropType.HybridBullNose => "HN", 
				DronePropType.ButterCutter => "BC", 
				DronePropType.Special0 => "S0", 
				_ => "", 
			};
		}

		public static DronePropType ToEnum(string p_type)
		{
			return p_type switch
			{
				"BN" => DronePropType.BullNose, 
				"PN" => DronePropType.PointNose, 
				"HN" => DronePropType.HybridBullNose, 
				"BC" => DronePropType.ButterCutter, 
				"S0" => DronePropType.Special0, 
				_ => (DronePropType)(-1), 
			};
		}

		public static string FromBladeCount(int p_count)
		{
			return p_count switch
			{
				2 => "2B", 
				3 => "3B", 
				4 => "4B", 
				5 => "5B", 
				6 => "6B", 
				_ => "", 
			};
		}

		public static int ToBladeCount(string p_count)
		{
			return p_count switch
			{
				"2B" => 2, 
				"3B" => 3, 
				"4B" => 4, 
				"5B" => 5, 
				"6B" => 6, 
				_ => 0, 
			};
		}
	}
}
