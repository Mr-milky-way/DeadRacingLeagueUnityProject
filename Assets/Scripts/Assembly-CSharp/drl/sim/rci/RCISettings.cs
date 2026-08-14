using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl.sim.rci
{
	[Serializable]
	public class RCISettings
	{
		public int Version;

		public List<RCDeviceData> Data;

		public RCISettings()
		{
			Version = 3;
			Data = new List<RCDeviceData>();
		}

		public static RCISettings AttemptToUpdateData(int version, object data)
		{
			Debug.Log($"RCISettings Version {version} detected attempting to update.");
			switch (version)
			{
			case 3:
				Debug.LogWarning("RCISettings already updated and current!");
				return (RCISettings)data;
			case 2:
			{
				RCISettings rCISettings;
				try
				{
					rCISettings = (RCISettings)data;
				}
				catch (InvalidCastException)
				{
					Debug.Log($"RCISettings Unable to update to {3} from {version}.  Returning Default settings.");
					return new RCISettings();
				}
				for (int i = 0; i < rCISettings.Data.Count; i++)
				{
					if (rCISettings.Data[i].isDefault)
					{
						RCDeviceData rCDeviceData = new RCDeviceData(rCISettings.Data[i].defaultControllerType, custom: false, rCISettings.Data[i].hardwareName, rCISettings.Data[i].guid);
						for (int j = 0; j < 4; j++)
						{
							rCDeviceData.assignedAxisData[j] = rCISettings.Data[i].assignedAxisData[j];
						}
						rCISettings.Data[i] = rCDeviceData;
					}
				}
				Debug.Log($"RCISettings Update successful version {3} from {version}.");
				rCISettings.Version = 3;
				return rCISettings;
			}
			default:
				Debug.Log($"RCISettings Unable to update to {3} from {version}.  Returning Default settings.");
				return new RCISettings();
			}
		}
	}
}
