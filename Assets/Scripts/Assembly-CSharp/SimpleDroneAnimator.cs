using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using drl.sim;
using drl.sim.rci;

[RequireComponent(typeof(UIDroneOverlay))]
public class SimpleDroneAnimator : MonoBehaviour
{
	private Transform m_drone;

	private UIDroneOverlay m_droneOverlay;

	private float droneStartHeight;

	private Vector3 startingRotation;

	private readonly Dictionary<RawAxis, bool> inverts = new Dictionary<RawAxis, bool>
	{
		{
			RawAxis.LeftStickX,
			false
		},
		{
			RawAxis.LeftStickY,
			false
		},
		{
			RawAxis.RightStickX,
			false
		},
		{
			RawAxis.RightStickY,
			false
		}
	};

	private readonly Dictionary<RawAxis, float> min = new Dictionary<RawAxis, float>
	{
		{
			RawAxis.LeftStickX,
			-1f
		},
		{
			RawAxis.LeftStickY,
			-1f
		},
		{
			RawAxis.RightStickX,
			-1f
		},
		{
			RawAxis.RightStickY,
			-1f
		}
	};

	private readonly Dictionary<RawAxis, float> max = new Dictionary<RawAxis, float>
	{
		{
			RawAxis.LeftStickX,
			1f
		},
		{
			RawAxis.LeftStickY,
			1f
		},
		{
			RawAxis.RightStickX,
			1f
		},
		{
			RawAxis.RightStickY,
			1f
		}
	};

	private readonly Dictionary<RawAxis, float> center = new Dictionary<RawAxis, float>
	{
		{
			RawAxis.LeftStickX,
			0f
		},
		{
			RawAxis.LeftStickY,
			0f
		},
		{
			RawAxis.RightStickX,
			0f
		},
		{
			RawAxis.RightStickY,
			0f
		}
	};

	private readonly Dictionary<RawAxis, float> deadzone = new Dictionary<RawAxis, float>
	{
		{
			RawAxis.LeftStickX,
			0f
		},
		{
			RawAxis.LeftStickY,
			0f
		},
		{
			RawAxis.RightStickX,
			0f
		},
		{
			RawAxis.RightStickY,
			0f
		}
	};

	private readonly Dictionary<RawAxis, int> channels = new Dictionary<RawAxis, int>
	{
		{
			RawAxis.LeftStickX,
			-1
		},
		{
			RawAxis.LeftStickY,
			-1
		},
		{
			RawAxis.RightStickX,
			-1
		},
		{
			RawAxis.RightStickY,
			-1
		}
	};

	private readonly Dictionary<RawAxis, float> zeroThrottle = new Dictionary<RawAxis, float>
	{
		{
			RawAxis.LeftStickX,
			-2f
		},
		{
			RawAxis.LeftStickY,
			-2f
		},
		{
			RawAxis.RightStickX,
			-2f
		},
		{
			RawAxis.RightStickY,
			-2f
		}
	};

	private bool useChannels;

	private void Start()
	{
	}

	public void Init()
	{
		m_droneOverlay = GetComponent<UIDroneOverlay>();
		if ((bool)m_droneOverlay)
		{
			m_drone = m_droneOverlay.drone.transform;
			droneStartHeight = m_drone.position.y;
			startingRotation = m_drone.rotation.eulerAngles;
		}
	}

	private void Update()
	{
		if ((bool)m_drone)
		{
			Animate();
		}
	}

	private void Animate()
	{
		float num = (useChannels ? GetAssignedValue(RawAxis.LeftStickX) : RCI.GetRawAxis(RawAxis.LeftStickX));
		float num2 = (useChannels ? GetAssignedValue(RawAxis.LeftStickY) : RCI.GetRawAxis(RawAxis.LeftStickY));
		float num3 = (useChannels ? GetAssignedValue(RawAxis.RightStickX) : RCI.GetRawAxis(RawAxis.RightStickX));
		float num4 = (useChannels ? GetAssignedValue(RawAxis.RightStickY) : RCI.GetRawAxis(RawAxis.RightStickY));
		num = (float.IsNaN(num) ? 0f : num);
		num2 = (float.IsNaN(num2) ? 0f : num2);
		num3 = (float.IsNaN(num3) ? 0f : num3);
		num4 = (float.IsNaN(num4) ? 0f : num4);
		num = Mathf.Clamp(num, -1f, 1f);
		num2 = Mathf.Clamp(num2, -1f, 1f);
		num3 = Mathf.Clamp(num3, -1f, 1f);
		num4 = Mathf.Clamp(num4, -1f, 1f);
		m_drone.position = Vector3.Lerp(m_drone.position, new Vector3(m_drone.position.x, droneStartHeight + num2 / 15f, m_drone.position.z), Time.deltaTime * 10f);
		Vector3 euler = new Vector3(num4, num, 0f - num3) * 45f;
		euler += startingRotation;
		m_drone.rotation = Quaternion.Lerp(m_drone.rotation, Quaternion.Euler(euler), Time.deltaTime * 10f);
	}

	private float GetAssignedValue(RawAxis axis)
	{
		if (channels[axis] < 0 || channels[axis] >= RCI.GetAxisCount())
		{
			return 0f;
		}
		return RCI.GetAssignedAxisValueFromIndex(channels[axis], min[axis], max[axis], center[axis], deadzone[axis], zeroThrottle[axis], inverts[axis]);
	}

	public void UpdateChannelData(RawAxis axis, CalibrationData data)
	{
		channels[axis] = data.ElementIDs[axis];
		useChannels = true;
		if (axis != RawAxis.ToggleA && axis != RawAxis.ToggleB)
		{
			center[axis] = data.Centers[data.ElementIDs[axis]];
			if (data.Invert.ContainsKey(axis))
			{
				inverts[axis] = data.Invert[axis];
			}
			if (data.RangeMin.ContainsKey(axis))
			{
				min[axis] = data.RangeMin[axis];
			}
			if (data.RangeMax.ContainsKey(axis))
			{
				max[axis] = data.RangeMax[axis];
			}
			if (data.Deadzone.ContainsKey(axis))
			{
				deadzone[axis] = data.Deadzone[axis];
			}
			if (axis == RawAxis.LeftStickY)
			{
				zeroThrottle[axis] = data.ZeroThrottle;
			}
			else
			{
				zeroThrottle[axis] = -2f;
			}
		}
	}

	public void UpdateChannelData(CalibrationData data)
	{
		useChannels = true;
		foreach (RawAxis key in data.ElementIDs.Keys)
		{
			if (key != RawAxis.ToggleA && key != RawAxis.ToggleB)
			{
				channels[key] = data.ElementIDs[key];
				if (data.ElementIDs[key] >= 0 && data.ElementIDs[key] < data.Centers.Length)
				{
					center[key] = data.Centers[data.ElementIDs[key]];
				}
				if (data.Invert.ContainsKey(key))
				{
					inverts[key] = data.Invert[key];
				}
				if (data.RangeMin.ContainsKey(key))
				{
					min[key] = data.RangeMin[key];
				}
				if (data.RangeMax.ContainsKey(key))
				{
					max[key] = data.RangeMax[key];
				}
				if (data.Deadzone.ContainsKey(key))
				{
					deadzone[key] = data.Deadzone[key];
				}
				if (key == RawAxis.LeftStickY)
				{
					zeroThrottle[key] = data.ZeroThrottle;
				}
				else
				{
					zeroThrottle[key] = -2f;
				}
			}
		}
	}

	public void UpdateInvert(RawAxis axis, bool p_invert)
	{
		inverts[axis] = p_invert;
	}

	public void ResetChannelData()
	{
		inverts[RawAxis.LeftStickX] = false;
		inverts[RawAxis.LeftStickY] = false;
		inverts[RawAxis.RightStickX] = false;
		inverts[RawAxis.RightStickY] = false;
		min[RawAxis.LeftStickX] = -1f;
		min[RawAxis.LeftStickY] = -1f;
		min[RawAxis.RightStickX] = -1f;
		min[RawAxis.RightStickY] = -1f;
		max[RawAxis.LeftStickX] = 1f;
		max[RawAxis.LeftStickY] = 1f;
		max[RawAxis.RightStickX] = 1f;
		max[RawAxis.RightStickY] = 1f;
		center[RawAxis.LeftStickX] = 0f;
		center[RawAxis.LeftStickY] = 0f;
		center[RawAxis.RightStickX] = 0f;
		center[RawAxis.RightStickY] = 0f;
		deadzone[RawAxis.LeftStickX] = 0f;
		deadzone[RawAxis.LeftStickY] = 0f;
		deadzone[RawAxis.RightStickX] = 0f;
		deadzone[RawAxis.RightStickY] = 0f;
		zeroThrottle[RawAxis.LeftStickX] = -2f;
		zeroThrottle[RawAxis.LeftStickY] = -2f;
		zeroThrottle[RawAxis.RightStickX] = -2f;
		zeroThrottle[RawAxis.RightStickY] = -2f;
	}

	public void UseRCChannels(Dictionary<RawAxis, int> p_channels)
	{
		useChannels = true;
		if (p_channels == null)
		{
			return;
		}
		foreach (KeyValuePair<RawAxis, int> p_channel in p_channels)
		{
			if (channels.ContainsKey(p_channel.Key))
			{
				channels[p_channel.Key] = p_channel.Value;
			}
		}
	}

	public void UseRCChannels()
	{
		if (!RCI.HasSavedProfile())
		{
			return;
		}
		useChannels = true;
		RCDeviceData savedProfile = RCI.GetSavedProfile();
		foreach (RawAxis item in channels.Keys.ToList())
		{
			AssignedAxisData aAD = savedProfile.GetAAD(item);
			channels[item] = aAD.ElementID;
			center[item] = aAD.center;
			inverts[item] = aAD.inverted;
			min[item] = aAD.min;
			max[item] = aAD.max;
			deadzone[item] = aAD.deadzone;
			if (item == RawAxis.LeftStickY)
			{
				zeroThrottle[item] = aAD.zeroThrottle;
			}
			else
			{
				zeroThrottle[item] = -2f;
			}
		}
	}

	public void UseRawAxis()
	{
		useChannels = false;
	}
}
