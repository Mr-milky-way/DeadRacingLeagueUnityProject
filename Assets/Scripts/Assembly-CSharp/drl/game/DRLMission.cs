using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class DRLMission : DRLGameAsset
	{
		public DRLMap map;

		public DRLMapTrack track;

		public string scene;

		public string title;

		[TextArea(1, 4)]
		public string description;

		[TextArea(1, 4)]
		public string shortDescription;

		public Texture descriptionImage;

		public List<DRLDroneRig> drone;

		public List<FCMode> flightModes;

		public string[] objectives;

		public float Evaluate(DataFlow p_data)
		{
			DRLMissionScore component = GetComponent<DRLMissionScore>();
			if (!component)
			{
				return 1f;
			}
			return component.Evaluate(p_data);
		}

		public DRLDroneRig GetDrone(int p_index)
		{
			if (drone == null)
			{
				drone = new List<DRLDroneRig>();
			}
			if (p_index < 0)
			{
				return null;
			}
			if (p_index >= drone.Count)
			{
				return null;
			}
			return drone[p_index];
		}

		public override string GetPrefix()
		{
			return "MS";
		}
	}
}
