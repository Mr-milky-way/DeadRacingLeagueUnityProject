using System;
using thelab.core;

namespace drl.backend
{
	public class DRLProgressionPrizeData : SerializedData
	{
		public enum PrizeType
		{
			Invalid = 0,
			Number = 1,
			Multiplier = 2
		}

		public int value => Get("value", 0);

		public string prizeType
		{
			get
			{
				return Get("prize-type", "");
			}
			set
			{
				Set("prize-type", value);
			}
		}

		public PrizeType prizeTypeFlag
		{
			get
			{
				if (!string.IsNullOrEmpty(prizeType))
				{
					return (PrizeType)Enum.Parse(typeof(PrizeType), prizeType);
				}
				return PrizeType.Invalid;
			}
		}

		public string description => Get("description", "");
	}
}
