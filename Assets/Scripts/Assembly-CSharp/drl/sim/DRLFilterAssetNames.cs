using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl.sim
{
	public class DRLFilterAssetNames : MonoBehaviour, ISerializationCallbackReceiver
	{
		public List<DroneAssetTagType> _keys = new List<DroneAssetTagType>();

		public List<string> _values = new List<string>();

		public Dictionary<DroneAssetTagType, string> filterNames = new Dictionary<DroneAssetTagType, string>();

		public Vector2 motor0 = new Vector2(2700f, 2709f);

		public Vector2 motor1 = new Vector2(2400f, 2413f);

		public Vector2 prop0 = new Vector2(2200f, 2216f);

		public Vector2 prop1 = new Vector2(2290f, 2294f);

		public Vector2 battery0 = new Vector2(2100f, 2107f);

		public Vector2 battery1 = new Vector2(2150f, 2156f);

		public Vector2 skins0 = new Vector2(7000f, 7007f);

		public Vector2 frame0 = new Vector2(8000f, 8006f);

		public void InitNames()
		{
			List<DroneAssetTagType> list = new List<DroneAssetTagType>(filterNames.Keys);
			for (int i = 0; i < list.Count; i++)
			{
				if (!CheckNameAgainstRange((int)list[i]))
				{
					filterNames.Remove(list[i]);
				}
			}
			Vector2[] array = new Vector2[8] { motor0, motor1, prop0, prop1, battery0, battery1, skins0, frame0 };
			for (int j = 0; j < array.Length; j++)
			{
				for (int k = (int)array[j].x; k < (int)array[j].y; k++)
				{
					if (!filterNames.ContainsKey((DroneAssetTagType)k))
					{
						Dictionary<DroneAssetTagType, string> dictionary = filterNames;
						int key = k;
						DroneAssetTagType droneAssetTagType = (DroneAssetTagType)k;
						dictionary.Add((DroneAssetTagType)key, droneAssetTagType.ToString());
					}
				}
			}
		}

		public bool CheckNameAgainstRange(int p_name)
		{
			if ((!((float)p_name >= motor0.x) || !((float)p_name < motor0.y)) && (!((float)p_name >= motor1.x) || !((float)p_name < motor1.y)) && (!((float)p_name >= prop0.x) || !((float)p_name < prop0.y)) && (!((float)p_name >= prop1.x) || !((float)p_name < prop1.y)) && (!((float)p_name >= battery0.x) || !((float)p_name < battery0.y)) && (!((float)p_name >= battery1.x) || !((float)p_name < battery1.y)) && (!((float)p_name >= skins0.x) || !((float)p_name < skins0.y)) && (!((float)p_name >= frame0.x) || !((float)p_name < frame0.y)))
			{
				return false;
			}
			return true;
		}

		public void OnBeforeSerialize()
		{
			_keys.Clear();
			_values.Clear();
			foreach (KeyValuePair<DroneAssetTagType, string> filterName in filterNames)
			{
				_keys.Add(filterName.Key);
				_values.Add(filterName.Value);
			}
		}

		public void OnAfterDeserialize()
		{
			filterNames = new Dictionary<DroneAssetTagType, string>();
			for (int i = 0; i != Math.Min(_keys.Count, _values.Count); i++)
			{
				if (!filterNames.ContainsKey(_keys[i]))
				{
					filterNames.Add(_keys[i], _values[i]);
				}
			}
		}
	}
}
