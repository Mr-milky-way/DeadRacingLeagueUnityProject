namespace drl.game
{
	public class ReplayChannelIds
	{
		public const string Time = "time";

		public const string DronePosX = "drone-px";

		public const string DronePosY = "drone-py";

		public const string DronePosZ = "drone-pz";

		public static string[] DronePos = new string[3] { "drone-px", "drone-py", "drone-pz" };

		public const string DronePart = "drone-part";

		public const string DronePartPosX = "drone-part-px";

		public const string DronePartPosY = "drone-part-py";

		public const string DronePartPosZ = "drone-part-pz";

		public static string[] DronePartPos = new string[3] { "drone-part-px", "drone-part-py", "drone-part-pz" };

		public const string DroneQuatX = "drone-qx";

		public const string DroneQuatY = "drone-qy";

		public const string DroneQuatZ = "drone-qz";

		public const string DroneQuatW = "drone-qw";

		public static string[] DroneQuat = new string[4] { "drone-qx", "drone-qy", "drone-qz", "drone-qw" };

		public const string DronePartQuatX = "drone-part-qx";

		public const string DronePartQuatY = "drone-part-qy";

		public const string DronePartQuatZ = "drone-part-qz";

		public const string DronePartQuatW = "drone-part-qw";

		public static string[] DronePartQuat = new string[4] { "drone-part-qx", "drone-part-qy", "drone-part-qz", "drone-part-qw" };

		public static string[] DroneParts = new string[7] { "drone-part-px", "drone-part-py", "drone-part-pz", "drone-part-qx", "drone-part-qy", "drone-part-qz", "drone-part-qw" };

		public const string InputThrottle = "input-t";

		public const string InputYaw = "input-y";

		public const string InputPitch = "input-p";

		public const string InputRoll = "input-r";

		public static string[] Input = new string[4] { "input-y", "input-t", "input-r", "input-p" };

		public const string DroneVelX = "drone-vx";

		public const string DroneVelY = "drone-vy";

		public const string DroneVelZ = "drone-vz";

		public static string[] DroneVel = new string[3] { "drone-vx", "drone-vy", "drone-vz" };

		public const string DroneRPM0 = "drone-rpm0";

		public const string DroneRPM1 = "drone-rpm1";

		public const string DroneRPM2 = "drone-rpm2";

		public const string DroneRPM3 = "drone-rpm3";

		public static string[] Drone4RPM = new string[4] { "drone-rpm0", "drone-rpm1", "drone-rpm2", "drone-rpm3" };

		public const string DronePIDYaw = "drone-pid-y";

		public const string DronePIDPitch = "drone-pid-p";

		public const string DronePIDRoll = "drone-pid-r";

		public static string[] DronePID = new string[3] { "drone-pid-y", "drone-pid-p", "drone-pid-r" };

		public const string DroneDragX = "drone-drag-x";

		public const string DroneDragY = "drone-drag-y";

		public const string DroneDragZ = "drone-drag-z";

		public static string[] DroneDrag = new string[3] { "drone-drag-x", "drone-drag-y", "drone-drag-z" };

		public const string DroneDragForceX = "drone-drag-fx";

		public const string DroneDragForceY = "drone-drag-fy";

		public const string DroneDragForceZ = "drone-drag-fz";

		public static string[] DroneDragForce = new string[3] { "drone-drag-fx", "drone-drag-fy", "drone-drag-fz" };

		public const string DroneThrust0 = "drone-thrust0";

		public const string DroneThrust1 = "drone-thrust1";

		public const string DroneThrust2 = "drone-thrust2";

		public const string DroneThrust3 = "drone-thrust3";

		public static string[] Drone4Thrust = new string[4] { "drone-thrust0", "drone-thrust1", "drone-thrust2", "drone-thrust3" };

		public const string DroneTorque = "drone-torque";

		public static string[] ChannelAll = new string[27]
		{
			"time", "drone-px", "drone-py", "drone-pz", "drone-qx", "drone-qy", "drone-qz", "drone-qw", "input-t", "input-y",
			"input-p", "input-r", "drone-vx", "drone-vy", "drone-vz", "drone-rpm0", "drone-rpm1", "drone-rpm2", "drone-rpm3", "drone-pid-y",
			"drone-pid-p", "drone-pid-r", "drone-thrust0", "drone-thrust1", "drone-thrust2", "drone-thrust3", "drone-torque"
		};

		public static string[] ChannelBasic = new string[19]
		{
			"time", "drone-px", "drone-py", "drone-pz", "drone-qx", "drone-qy", "drone-qz", "drone-qw", "input-t", "input-y",
			"input-p", "input-r", "drone-vx", "drone-vy", "drone-vz", "drone-rpm0", "drone-rpm1", "drone-rpm2", "drone-rpm3"
		};
	}
}
