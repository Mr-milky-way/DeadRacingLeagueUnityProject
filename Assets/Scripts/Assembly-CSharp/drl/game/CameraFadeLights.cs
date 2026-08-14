using System.Collections.Generic;
using UnityEngine;

namespace drl.game
{
	public class CameraFadeLights : MonoBehaviour
	{
		public Camera gameCamera;

		public GameObject targetLevel;

		public GameObject targetTracks;

		public float rangeStartScale = 5f;

		public float distanceDecay = 75f;

		public float distancePow = 2.5f;

		private bool m_initialized;

		private List<Light> m_lightsStatic;

		private float[] m_lightsStaticIntensity;

		private bool[] m_lightsStaticEnabled;

		private List<Light> m_lightsOscilator;

		private Vector3 m_lastCameraPosition;

		private bool m_updateAll;

		private int m_chunks = 6;

		private int m_iterator;

		private void OnDisable()
		{
			RestoreOriginalLightSettings();
		}

		public void Destroy()
		{
			m_initialized = false;
			RestoreOriginalLightSettings();
			m_lightsStatic = null;
			m_lightsStaticIntensity = null;
			m_lightsStaticEnabled = null;
			m_lightsOscilator = null;
			m_iterator = 0;
		}

		public void RestoreOriginalLightSettings()
		{
			if (m_lightsStatic == null)
			{
				return;
			}
			for (int i = 0; i < m_lightsStatic.Count; i++)
			{
				Light light = m_lightsStatic[i];
				if ((bool)light)
				{
					light.intensity = m_lightsStaticIntensity[i];
					light.enabled = m_lightsStaticEnabled[i];
				}
			}
		}

		public void Initialize()
		{
			m_initialized = true;
			m_lightsStatic = new List<Light>();
			m_lightsStatic.AddRange(targetLevel.GetComponentsInChildren<Light>());
			if (targetTracks != null)
			{
				m_lightsStatic.AddRange(targetTracks.GetComponentsInChildren<Light>());
			}
			for (int i = 0; i < m_lightsStatic.Count; i++)
			{
				Light light = m_lightsStatic[i];
				if (light.type == LightType.Directional)
				{
					m_lightsStatic[i] = null;
				}
			}
			m_lightsOscilator = new List<Light>();
			List<OscilatorLight> list = new List<OscilatorLight>();
			list.AddRange(targetLevel.GetComponentsInChildren<OscilatorLight>());
			if (targetTracks != null)
			{
				list.AddRange(targetTracks.GetComponentsInChildren<OscilatorLight>());
			}
			for (int j = 0; j < list.Count; j++)
			{
				OscilatorLight oscilatorLight = list[j];
				m_lightsOscilator.AddRange(oscilatorLight.targets);
			}
			for (int k = 0; k < m_lightsStatic.Count; k++)
			{
				Light light = m_lightsStatic[k];
				for (int l = 0; l < m_lightsOscilator.Count; l++)
				{
					Light light2 = m_lightsOscilator[l];
					if (light == light2)
					{
						m_lightsStatic[k] = null;
					}
				}
			}
			m_lightsStatic.RemoveAll((Light item) => item == null);
			m_lightsStaticIntensity = new float[m_lightsStatic.Count];
			m_lightsStaticEnabled = new bool[m_lightsStatic.Count];
			for (int num = 0; num < m_lightsStatic.Count; num++)
			{
				m_lightsStaticIntensity[num] = m_lightsStatic[num].intensity;
				m_lightsStaticEnabled[num] = m_lightsStatic[num].enabled;
			}
		}

		private void LateUpdate()
		{
			if (!m_initialized)
			{
				return;
			}
			Vector3 position = gameCamera.transform.position;
			if (m_lightsStatic != null && m_lightsStatic.Count > 0)
			{
				int num = (m_updateAll ? 1 : m_chunks);
				int num2 = Mathf.Max(m_lightsStatic.Count / num, 1);
				int num3 = 0;
				for (int i = 0; i < num2; i++)
				{
					num3 = m_iterator % m_lightsStatic.Count;
					Light light = m_lightsStatic[num3];
					if ((bool)light)
					{
						Vector3 position2 = light.transform.position;
						float range = light.range;
						float lightIntensity = GetLightIntensity(position, position2, range);
						float num4 = m_lightsStaticIntensity[num3] * lightIntensity;
						if (Mathf.Abs(light.intensity - num4) > 0.01f)
						{
							light.intensity = num4;
						}
						bool flag = num4 > 0f;
						if (flag != light.enabled)
						{
							light.enabled = flag;
						}
						m_iterator++;
					}
				}
			}
			if (m_lightsOscilator != null)
			{
				for (int j = 0; j < m_lightsOscilator.Count; j++)
				{
					Light light = m_lightsOscilator[j];
					Vector3 position3 = light.transform.position;
					float range2 = light.range;
					float lightIntensity2 = GetLightIntensity(position, position3, range2);
					float num5 = light.intensity * lightIntensity2;
					if (Mathf.Abs(light.intensity - num5) > 0.01f)
					{
						light.intensity = num5;
					}
					bool flag2 = num5 > 0f;
					if (flag2 != light.enabled)
					{
						light.enabled = flag2;
					}
				}
			}
			_ = m_lastCameraPosition;
			if ((double)Vector3.Distance(m_lastCameraPosition, position) > (double)distanceDecay * 0.5)
			{
				m_updateAll = true;
				m_iterator = 0;
			}
			else
			{
				m_updateAll = false;
			}
			m_lastCameraPosition = position;
		}

		protected float GetLightIntensity(Vector3 p_camera_pos, Vector3 p_light_pos, float p_light_range)
		{
			float num = Vector3.Distance(p_camera_pos, p_light_pos);
			float num2 = Mathf.Max(p_light_range, 1f);
			float num3 = rangeStartScale * num2;
			float num4 = num3 + distanceDecay * (num2 * 0.1f);
			return 1f - Mathf.Pow(Mathf.Clamp01((num - num3) / (num4 - num3)), distancePow);
		}
	}
}
