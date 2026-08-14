using UnityEngine;

namespace drl.game
{
	public class XboxDebugGUI : MonoBehaviour
	{
		public string[] labels;

		public string[] options;

		public object[] data;

		public int currentOption;

		private float m_frames;

		private float m_frame_time;

		private int m_fps;

		public void Awake()
		{
			m_frames = 0f;
			m_frame_time = Time.time;
			labels = new string[6] { "VSync On/Off", "Drone On/Off", "ImageEffects On/Off", "UI On/Off", "Game On/Off", "Game Camera On/Off" };
			options = new string[6] { "vsync-toggle", "drone-toggle", "image-effect-toggle", "ui-toggle", "game-toggle", "game-camera-toggle" };
			data = new object[6] { false, true, true, true, true, true };
		}

		public void Update()
		{
			m_frames += 1f;
			float num = Time.time - m_frame_time;
			if (num >= 0.5f)
			{
				m_fps = Mathf.RoundToInt(m_frames / num);
				m_frames = 0f;
				m_frame_time = Time.time;
			}
		}

		public void OnGUI()
		{
		}
	}
}
