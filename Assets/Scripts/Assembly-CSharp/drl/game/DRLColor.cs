using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class DRLColor
	{
		public static Color[] profileColors = new Color[12]
		{
			Colorf.RGBToColor(0u),
			Colorf.RGBToColor(16711680u),
			Colorf.RGBToColor(16750080u),
			Colorf.RGBToColor(16187141u),
			Colorf.RGBToColor(2817792u),
			Colorf.RGBToColor(1097984u),
			Colorf.RGBToColor(8978399u),
			Colorf.RGBToColor(16777215u),
			Colorf.RGBToColor(28142u),
			Colorf.RGBToColor(393456u),
			Colorf.RGBToColor(9306351u),
			Colorf.RGBToColor(16515327u)
		};

		public static Color[] profileTryoutsColors = new Color[6]
		{
			Colorf.RGBToColor(16711680u),
			Colorf.RGBToColor(16750080u),
			Colorf.RGBToColor(16187141u),
			Colorf.RGBToColor(1097984u),
			Colorf.RGBToColor(393456u),
			Colorf.RGBToColor(9306351u)
		};

		public static Color[] profileTournamentColors = new Color[6]
		{
			Colorf.RGBToColor(16711680u),
			Colorf.RGBToColor(393456u),
			Colorf.RGBToColor(16187141u),
			Colorf.RGBToColor(1097984u),
			Colorf.RGBToColor(9306351u),
			Colorf.RGBToColor(16750080u)
		};

		public static Color[] profileFontColors = new Color[12]
		{
			Colorf.RGBToColor(9145227u),
			Colorf.RGBToColor(4718592u),
			Colorf.RGBToColor(6240256u),
			Colorf.RGBToColor(5461504u),
			Colorf.RGBToColor(940032u),
			Colorf.RGBToColor(343808u),
			Colorf.RGBToColor(24133u),
			Colorf.RGBToColor(5460819u),
			Colorf.RGBToColor(11101u),
			Colorf.RGBToColor(196930u),
			Colorf.RGBToColor(2031668u),
			Colorf.RGBToColor(4784202u)
		};

		public static Color[] mapEditorColors = new Color[28]
		{
			Colorf.RGBToColor(0u),
			Colorf.RGBToColor(16711680u),
			Colorf.RGBToColor(16750080u),
			Colorf.RGBToColor(16187141u),
			Colorf.RGBToColor(2817792u),
			Colorf.RGBToColor(1097984u),
			Colorf.RGBToColor(8978399u),
			Colorf.RGBToColor(16777215u),
			Colorf.RGBToColor(39167u),
			Colorf.RGBToColor(393456u),
			Colorf.RGBToColor(9306351u),
			Colorf.RGBToColor(16515327u),
			Colorf.RGBToColor(3556687u),
			Colorf.RGBToColor(8421504u),
			Colorf.RGBToColor(13882323u),
			Colorf.RGBToColor(16116430u),
			Colorf.RGBToColor(6636321u),
			Colorf.RGBToColor(6633505u),
			Colorf.RGBToColor(11887901u),
			Colorf.RGBToColor(12236908u),
			Colorf.RGBToColor(6508871u),
			Colorf.RGBToColor(14840411u),
			Colorf.RGBToColor(9109504u),
			Colorf.RGBToColor(139u),
			Colorf.RGBToColor(13421568u),
			Colorf.RGBToColor(25600u),
			Colorf.RGBToColor(13033215u),
			Colorf.RGBToColor(12968895u)
		};

		public static Color[] classColors = new Color[8]
		{
			Colorf.RGBToColor(0u),
			Colorf.RGBToColor(16515327u),
			Colorf.RGBToColor(35238u),
			Colorf.RGBToColor(5751552u),
			Colorf.RGBToColor(14988800u),
			Colorf.RGBToColor(16739328u),
			Colorf.RGBToColor(15007744u),
			Colorf.RGBToColor(15009029u)
		};

		public static Color[] raceLineColors = new Color[11]
		{
			Colorf.RGBToColor(1097984u),
			Colorf.RGBToColor(2817792u),
			Colorf.RGBToColor(16187141u),
			Colorf.RGBToColor(16750080u),
			Colorf.RGBToColor(16711680u),
			Colorf.RGBToColor(8978399u),
			Colorf.RGBToColor(16777215u),
			Colorf.RGBToColor(28142u),
			Colorf.RGBToColor(393456u),
			Colorf.RGBToColor(9306351u),
			Colorf.RGBToColor(16515327u)
		};

		public static Color[] checkPointColors = new Color[11]
		{
			Colorf.RGBToColor(1097984u),
			Colorf.RGBToColor(2817792u),
			Colorf.RGBToColor(16187141u),
			Colorf.RGBToColor(16750080u),
			Colorf.RGBToColor(16711680u),
			Colorf.RGBToColor(8978399u),
			Colorf.RGBToColor(16777215u),
			Colorf.RGBToColor(28142u),
			Colorf.RGBToColor(393456u),
			Colorf.RGBToColor(9306351u),
			Colorf.RGBToColor(16515327u)
		};

		public static Color red = Colorf.RGBToColor(16711680u);

		public static Color green = Colorf.RGBToColor(7851041u);

		public static Color greenLight = Colorf.RGBToColor(129329u);

		public static Color yellow = Colorf.RGBToColor(16312092u);

		public static Color yellowDark = Colorf.RGBToColor(12682790u);

		public static Color gray3 = Colorf.RGBToColor(5855319u);

		public static Color gray4 = Colorf.RGBToColor(9671571u);

		public static Color randomProfileColor
		{
			get
			{
				int num = Random.Range(0, profileColors.Length);
				return profileColors[num];
			}
		}

		public static Color GetFontColorByProfileColor(Color p_c)
		{
			for (int i = 0; i < profileColors.Length; i++)
			{
				if (p_c == profileColors[i])
				{
					return profileFontColors[i];
				}
			}
			return Color.white;
		}
	}
}
