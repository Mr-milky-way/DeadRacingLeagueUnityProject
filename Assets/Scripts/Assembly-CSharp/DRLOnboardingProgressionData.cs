using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using drl.backend;
using thelab.core;

public class DRLOnboardingProgressionData : SerializedData
{
	public enum Mode
	{
		beginner = 0,
		intermediate = 1,
		pro = 2
	}

	private readonly List<DRLCircuitData.Tag> m_modes = new List<DRLCircuitData.Tag>();

	public string missionName
	{
		get
		{
			return Get<string>("mission-name");
		}
		set
		{
			Set("mission-name", value);
		}
	}

	public string mapGUID
	{
		get
		{
			return Get<string>("map-GUID");
		}
		set
		{
			Set("map-GUID", value);
		}
	}

	public string trackGUID
	{
		get
		{
			return Get<string>("track-GUID");
		}
		set
		{
			Set("track-GUID", value);
		}
	}

	public bool trackStatus
	{
		get
		{
			return Get("track-completed", d: false);
		}
		set
		{
			Set("track-completed", value);
		}
	}

	private string[] m_modeArray
	{
		get
		{
			object obj = Get("mode", (object)new string[0]);
			if (!(obj is JArray))
			{
				return null;
			}
			obj = (obj as JArray).ToObject<string[]>();
			return (string[])obj;
		}
	}

	public List<DRLCircuitData.Tag> modes
	{
		get
		{
			m_modes.Clear();
			if (m_modeArray == null || m_modeArray.Length == 0)
			{
				return m_modes;
			}
			for (int i = 0; i < m_modeArray.Length; i++)
			{
				string value = m_modeArray[i];
				if (!string.IsNullOrEmpty(value))
				{
					Enum.TryParse<DRLCircuitData.Tag>(value, out var result);
					if (result != DRLCircuitData.Tag.none)
					{
						m_modes.Add(result);
					}
				}
			}
			return m_modes;
		}
	}
}
