using System;
using System.Collections.Generic;
using drl.sim;

namespace drl.game
{
	[Serializable]
	public class DRLStoreFilter
	{
		public enum Mode
		{
			Tag = 0,
			Asset = 1,
			GUID = 2
		}

		public enum Rule
		{
			Accept = 0,
			Ignore = 1
		}

		public enum Category
		{
			All = 0,
			None = 1,
			Antenna = 2,
			Battery = 3,
			CameraRF = 4,
			Frame = 5,
			Prop = 6,
			Motor = 7,
			SkinFrame = 8,
			Attachment = 9
		}

		public DroneAssetTagType category;

		public Mode mode;

		public Rule rule;

		public List<DronePart> parts;

		public List<string> guids;

		public List<DroneAssetTagType> tags;

		private bool isBuilt;

		private List<string> partGuids;

		public DRLStoreFilter()
		{
			category = DroneAssetTagType.None;
			mode = Mode.Tag;
			rule = Rule.Accept;
			parts = new List<DronePart>();
			guids = new List<string>();
			tags = new List<DroneAssetTagType>();
		}

		public bool HasGuid(string p_guid)
		{
			if (!isBuilt)
			{
				isBuilt = true;
				partGuids = new List<string>(parts.Count);
				for (int i = 0; i < parts.Count; i++)
				{
					partGuids.Add(parts[i].guid);
				}
			}
			if (!partGuids.Contains(p_guid))
			{
				return guids.Contains(p_guid);
			}
			return true;
		}

		public bool HasPart(DronePart p_part)
		{
			return HasGuid(p_part.guid);
		}

		public bool HasTag(DroneAssetTagType p_tag)
		{
			return tags.Contains(p_tag);
		}

		public static bool Filter(DronePart p_part, DronePart p_check)
		{
			if (p_part == null || p_check == null)
			{
				return true;
			}
			DRLStoreAsset component = p_check.GetComponent<DRLStoreAsset>();
			DRLStoreAsset component2 = p_part.GetComponent<DRLStoreAsset>();
			bool flag = true;
			if (component != null)
			{
				flag &= Filter(p_part, component);
			}
			if (component2 != null)
			{
				flag &= Filter(p_check, component2);
			}
			return flag;
		}

		public static bool Filter(DronePart p_part, DRLStoreAsset p_check)
		{
			if (p_check.filters == null || p_check.filters.Count == 0)
			{
				return true;
			}
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < p_check.filters.Count; i++)
			{
				DRLStoreFilter dRLStoreFilter = p_check.filters[i];
				if (dRLStoreFilter.rule != Rule.Ignore || (dRLStoreFilter.category != p_part.category && dRLStoreFilter.category != DroneAssetTagType.CategoryAll))
				{
					continue;
				}
				flag = true;
				if (dRLStoreFilter.mode == Mode.Asset || dRLStoreFilter.mode == Mode.GUID)
				{
					if (dRLStoreFilter.HasPart(p_part))
					{
						return false;
					}
					continue;
				}
				foreach (DroneAssetTagType tag in p_part.tags)
				{
					if (dRLStoreFilter.HasTag(tag))
					{
						return false;
					}
				}
			}
			for (int j = 0; j < p_check.filters.Count; j++)
			{
				DRLStoreFilter dRLStoreFilter2 = p_check.filters[j];
				if (dRLStoreFilter2.rule != Rule.Accept || (dRLStoreFilter2.category != p_part.category && dRLStoreFilter2.category != DroneAssetTagType.CategoryAll))
				{
					continue;
				}
				flag2 = true;
				if (dRLStoreFilter2.mode == Mode.Asset || dRLStoreFilter2.mode == Mode.GUID)
				{
					if (dRLStoreFilter2.HasPart(p_part))
					{
						return true;
					}
					continue;
				}
				foreach (DroneAssetTagType tag2 in p_part.tags)
				{
					if (dRLStoreFilter2.HasTag(tag2))
					{
						return true;
					}
				}
			}
			if (!flag)
			{
				return !flag2;
			}
			return true;
		}

		public static List<DronePart> Filter(List<DronePart> p_parts, DRLStoreAsset p_check)
		{
			if (p_check.filters == null || p_check.filters.Count == 0)
			{
				return p_parts;
			}
			for (int i = 0; i < p_parts.Count; i++)
			{
				if (!Filter(p_parts[i], p_check))
				{
					p_parts.RemoveAt(i);
					i--;
				}
			}
			return p_parts;
		}

		public static DroneAssetTagType TagTypeFromCategory(Category p_category)
		{
			return p_category switch
			{
				Category.All => DroneAssetTagType.CategoryAll, 
				Category.Antenna => DroneAssetTagType.Antenna, 
				Category.Battery => DroneAssetTagType.Battery, 
				Category.CameraRF => DroneAssetTagType.CameraRF, 
				Category.Frame => DroneAssetTagType.Frame, 
				Category.Prop => DroneAssetTagType.Prop, 
				Category.Motor => DroneAssetTagType.Motor, 
				Category.SkinFrame => DroneAssetTagType.SkinFrame, 
				Category.Attachment => DroneAssetTagType.Attachment0, 
				_ => DroneAssetTagType.None, 
			};
		}

		public static Category CategoryFromTagType(DroneAssetTagType p_tag)
		{
			return p_tag switch
			{
				DroneAssetTagType.CategoryAll => Category.All, 
				DroneAssetTagType.Antenna => Category.Antenna, 
				DroneAssetTagType.Battery => Category.Battery, 
				DroneAssetTagType.CameraRF => Category.CameraRF, 
				DroneAssetTagType.Frame => Category.Frame, 
				DroneAssetTagType.Prop => Category.Prop, 
				DroneAssetTagType.Motor => Category.Motor, 
				DroneAssetTagType.SkinFrame => Category.SkinFrame, 
				DroneAssetTagType.Attachment0 => Category.Attachment, 
				DroneAssetTagType.Attachment1 => Category.Attachment, 
				DroneAssetTagType.Attachment2 => Category.Attachment, 
				DroneAssetTagType.Attachment3 => Category.Attachment, 
				_ => Category.None, 
			};
		}
	}
}
