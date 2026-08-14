using System.Collections.Generic;
using drl.sim;

namespace drl.game
{
	public class DRLStoreAsset : DRLLibraryAsset
	{
		public List<DRLStoreFilter> filters;

		public bool ConvertPartsToGUIDs()
		{
			bool result = false;
			foreach (DRLStoreFilter filter in filters)
			{
				if (filter.mode == DRLStoreFilter.Mode.Asset)
				{
					result = true;
					List<string> collection = filter.parts.ConvertAll((DronePart dpit) => dpit.guid);
					filter.guids = new List<string>(collection);
					filter.mode = DRLStoreFilter.Mode.GUID;
					filter.parts.Clear();
				}
			}
			return result;
		}
	}
}
