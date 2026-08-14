using UnityEngine;

namespace drl
{
	public class FPSGUI : MonoBehaviour
	{
		private int m_last_frame;

		private float m_elapsed;

		private int m_fps;

		private bool m_show_fps = true;

		protected void Start()
		{
			m_last_frame = Time.frameCount;
			m_elapsed = 0f;
			m_fps = 0;
		}

		protected void Update()
		{
			m_elapsed += Time.deltaTime;
			if (m_elapsed >= 0.5f)
			{
				m_fps = (Time.frameCount - m_last_frame) * 2;
				m_last_frame = Time.frameCount;
				m_elapsed = 0f;
			}
		}

		protected void OnGUI()
		{
			if (m_show_fps)
			{
				GUIStyle label = GUI.skin.label;
				GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
				gUIStyle.fontSize = 20;
				gUIStyle.alignment = TextAnchor.UpperCenter;
				gUIStyle.normal.textColor = Color.yellow;
				gUIStyle.fontStyle = FontStyle.Bold;
				GUI.skin.label = gUIStyle;
				GUI.Label(new Rect(((float)Screen.width - 100f) * 0.5f, 5f, 100f, 30f), m_fps + " FPS");
				GUI.skin.label = label;
			}
		}
	}
}
