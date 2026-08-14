namespace drl.game
{
	public class DRLVersion
	{
		public static bool hideMode;

		public static bool hidePlatform;

		public static string major => "4";

		public static string minor => "2";

		public static string patch => "ee16";

		public static string mode => "rls";

		public static string platform => "win";

		public static string server => "2102";

		public static string full => major + "." + minor + "." + patch + (hideMode ? "" : ("." + mode)) + (hidePlatform ? "" : ("-" + platform));

		public static string small => major + "." + minor + "." + patch + (hidePlatform ? "" : ("-" + platform));

		public static string minimum => major + "." + minor + (hidePlatform ? "" : ("-" + platform));

		public static string value => patch;
	}
}
