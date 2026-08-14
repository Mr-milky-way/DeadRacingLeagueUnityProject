using System.Collections.Generic;
using UnityEngine;

namespace drl.game
{
	public class DRLQuest : DRLGameAsset
	{
		public Texture imageXbox;

		public Texture imagePS;

		[TextArea(1, 4)]
		public string title;

		[TextArea(1, 4)]
		public string levelTitle;

		[TextArea(1, 4)]
		public string description;

		public DRLMission testMission;

		public List<DRLMission> missions;

		public List<FCMode> flightModes
		{
			get
			{
				List<FCMode> list = new List<FCMode>();
				for (int i = 0; i < missions.Count; i++)
				{
					DRLMission dRLMission = missions[i];
					if (!dRLMission)
					{
						continue;
					}
					for (int j = 0; j < dRLMission.flightModes.Count; j++)
					{
						FCMode item = dRLMission.flightModes[j];
						if (!list.Contains(item))
						{
							list.Add(item);
							break;
						}
					}
				}
				if (testMission != null)
				{
					list.Add(testMission.flightModes[0]);
				}
				bool flag = list.Contains(FCMode.Intermediate);
				bool flag2 = list.Contains(FCMode.Pro);
				if (flag || flag2)
				{
					list.Remove(FCMode.Beginner);
				}
				return list;
			}
		}

		public DRLMission FindByGUID(string p_guid)
		{
			for (int i = 0; i < missions.Count; i++)
			{
				if ((bool)missions[i] && missions[i].guid == p_guid)
				{
					return missions[i];
				}
			}
			return null;
		}

		public override string GetPrefix()
		{
			return "QS";
		}
	}
}
