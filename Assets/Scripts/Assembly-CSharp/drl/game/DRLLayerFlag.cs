using System.Collections.Generic;

namespace drl.game
{
	public class DRLLayerFlag
	{
		public const int Default = 0;

		public const int Water = 4;

		public const int ReflectionOnly = 8;

		public const int Reflection = 9;

		public const int Terrain = 10;

		public const int Blocked = 11;

		public const int Collision = 12;

		public const int Player = 13;

		public const int Glow = 14;

		public const int ReflectionOnlyProbe = 15;

		public const int Pattern = 16;

		public const int LevelEffect = 17;

		public const int Gate = 21;

		public const int Minimap = 22;

		public const int DronePart = 23;

		public const int HUD = 24;

		public const int MapActions = 27;

		public const int MapAsset = 28;

		public const int EditorSelection = 29;

		public const int EditorMap = 30;

		public const int EditorRace = 31;

		private static Dictionary<int, int> m_lut_flags;

		public static int DefaultBit => 1;

		public static int WaterBit => 16;

		public static int ReflectionOnlyBit => 256;

		public static int ReflectionBit => 512;

		public static int TerrainBit => 1024;

		public static int BlockedBit => 2048;

		public static int CollisionBit => 4096;

		public static int PlayerBit => 8192;

		public static int GlowBit => 16384;

		public static int ReflectionOnlyProbeBit => 32768;

		public static int PatternBit => 65536;

		public static int LevelEffectBit => 131072;

		public static int GateBit => 2097152;

		public static int MinimapBit => 4194304;

		public static int DronePartBit => 8388608;

		public static int HUDBit => 16777216;

		public static int MapActionsBit => 134217728;

		public static int MapAssetBit => 268435456;

		public static int EditorSelectionBit => 536870912;

		public static int EditorMapBit => 1073741824;

		public static int EditorRaceBit => int.MinValue;

		public static int BitToFlag(int p_mask)
		{
			if (m_lut_flags == null)
			{
				m_lut_flags = new Dictionary<int, int>();
				m_lut_flags[0] = DefaultBit;
				m_lut_flags[4] = 4;
				m_lut_flags[8] = ReflectionOnlyBit;
				m_lut_flags[9] = ReflectionBit;
				m_lut_flags[10] = TerrainBit;
				m_lut_flags[11] = BlockedBit;
				m_lut_flags[12] = CollisionBit;
				m_lut_flags[13] = PlayerBit;
				m_lut_flags[14] = GlowBit;
				m_lut_flags[15] = ReflectionOnlyProbeBit;
				m_lut_flags[16] = PatternBit;
				m_lut_flags[17] = LevelEffectBit;
				m_lut_flags[21] = GateBit;
				m_lut_flags[22] = MinimapBit;
				m_lut_flags[23] = DronePartBit;
				m_lut_flags[24] = HUDBit;
				m_lut_flags[27] = MapActionsBit;
				m_lut_flags[28] = MapAssetBit;
				m_lut_flags[29] = EditorSelectionBit;
				m_lut_flags[30] = EditorMapBit;
				m_lut_flags[31] = EditorRaceBit;
			}
			return m_lut_flags[p_mask];
		}
	}
}
