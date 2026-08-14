namespace drl.sim.rci
{
	public static class RCHardware
	{
		public static readonly string[][] knownTypes = new string[16][]
		{
			new string[5] { "FrSky", "frsky taranis joystick", "frsky usb hid", "frsky horus joystick", "frsky x10 joystick" },
			new string[3] { "Xbox", "xbox", "x360" },
			new string[7] { "Playstation", "wireless controller", "ps2", "ps3", "ps4", "ps(r)", "playstation" },
			new string[6] { "Logitech Controller", "controller (gamepad f310)", "logitech dual action", "logitech rumblepad 2 usb", "logitech cordless rumblepad 2", "gamepad f310 (controller)" },
			new string[7] { "USB Transmitter", "ppm", "interlink elite", "interlink", "interlink-x", "rc-joystick", "ppm->" },
			new string[6] { "Donge Transmitter", "KMODEL", "vjoy device", "vjoy - virtual joystick", "usb dsmx hid", "spektrum receiver" },
			new string[15]
			{
				"Gamepad", "USB Gamepad", "generic usb joystick", "usb joystick", "controller (razer sabertooth elite)", "controller (xeox gamepad)", "controller (gpx gamepad)", "controller (coń)", "controller (madcatz gamepad)", "controller (madcatz call of duty gamepad)",
				"controller (inno gamepad..)", "controller (sl-6566)", "controller (steelseries xinput controller)", "controller (gamepad)", "deviation gamepad"
			},
			new string[3] { "I6 Style Transmitter", "fs-i6s emulator", "tgy-i6s emulator" },
			new string[3] { "Nikko Controller", "nikko air 115", "nikko air controller" },
			new string[2] { "HobbyKing Transmitter", "HK-MT6 emulator" },
			new string[2] { "SAILI", "saili" },
			new string[2] { "Gamesir", "gamesir" },
			new string[2] { "Gold Warrior", "gold warrior sim" },
			new string[2] { "Ikarus", "ikarus" },
			new string[2] { "Pengfei", "pengfei" },
			new string[2] { "Thrustmaster", "thrustmaster" }
		};

		public static bool IsKnownType(string hardwareName)
		{
			return !string.IsNullOrEmpty(CommonNameForKnownType(hardwareName));
		}

		public static string CommonNameForKnownType(string hardwareName)
		{
			if (string.IsNullOrEmpty(hardwareName))
			{
				return string.Empty;
			}
			for (int i = 0; i < knownTypes.Length; i++)
			{
				for (int j = 1; j < knownTypes[i].Length; j++)
				{
					if (hardwareName.Contains(knownTypes[i][j]))
					{
						return knownTypes[i][0];
					}
				}
			}
			return string.Empty;
		}
	}
}
