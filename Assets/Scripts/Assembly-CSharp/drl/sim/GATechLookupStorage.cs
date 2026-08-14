namespace drl.sim
{
	public class GATechLookupStorage
	{
		public static GATechLookupData[] DragData;

		public static GATechLookupData GetData(string p_name)
		{
			if (DragData == null || DragData.Length == 0)
			{
				return null;
			}
			if (string.IsNullOrEmpty(p_name))
			{
				return null;
			}
			for (int i = 0; i < DragData.Length; i++)
			{
				if (DragData[i].name.ToLowerInvariant().StartsWith(p_name.ToLowerInvariant()))
				{
					return DragData[i];
				}
			}
			return null;
		}

		public static bool HasData(string p_name)
		{
			if (DragData == null || DragData.Length == 0)
			{
				return false;
			}
			if (string.IsNullOrEmpty(p_name))
			{
				return false;
			}
			for (int i = 0; i < DragData.Length; i++)
			{
				if (DragData[i].name.ToLowerInvariant().StartsWith(p_name.ToLowerInvariant()))
				{
					return true;
				}
			}
			return false;
		}
	}
}
