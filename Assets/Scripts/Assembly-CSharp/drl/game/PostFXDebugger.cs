using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace drl.game
{
	public class PostFXDebugger : MonoBehaviour
	{
		public Font font;

		public List<Behaviour> components;

		private bool m_initialized;

		private List<object> m_items;

		private int m_current;

		private int m_total;

		private float m_pressTime;

		private bool m_isPressing;

		private bool m_willRemove;

		private float m_timeToRemove = 2f;

		public void Initialize()
		{
			m_initialized = true;
			m_items = new List<object>();
			for (int i = 0; i < components.Count; i++)
			{
				Component component = components[i];
				m_items.Add(component);
				Debug.Log(component.GetType());
				if (component.GetType() == typeof(PostProcessingBehaviour))
				{
					PostProcessingBehaviour postProcessingBehaviour = component as PostProcessingBehaviour;
					List<object> list = new List<object>();
					list.Add(postProcessingBehaviour.profile.fog);
					list.Add(postProcessingBehaviour.profile.antialiasing);
					list.Add(postProcessingBehaviour.profile.ambientOcclusion);
					list.Add(postProcessingBehaviour.profile.screenSpaceReflection);
					list.Add(postProcessingBehaviour.profile.depthOfField);
					list.Add(postProcessingBehaviour.profile.motionBlur);
					list.Add(postProcessingBehaviour.profile.eyeAdaptation);
					list.Add(postProcessingBehaviour.profile.bloom);
					list.Add(postProcessingBehaviour.profile.colorGrading);
					list.Add(postProcessingBehaviour.profile.userLut);
					list.Add(postProcessingBehaviour.profile.chromaticAberration);
					list.Add(postProcessingBehaviour.profile.grain);
					list.Add(postProcessingBehaviour.profile.vignette);
					list.Add(postProcessingBehaviour.profile.dithering);
					m_items.AddRange(list);
				}
			}
			m_total = m_items.Count;
		}

		private void Update()
		{
			if (m_initialized)
			{
				m_willRemove = ((m_isPressing && Time.time - m_pressTime > m_timeToRemove) ? true : false);
			}
		}

		private void OnGUI()
		{
			if (!m_initialized || !font)
			{
				return;
			}
			GUI.contentColor = Color.white;
			GUI.skin.font = font;
			float num = 10f;
			int num2 = 0;
			GUI.Label(new Rect(10f, num, 400f, 20f), "POST FX DEBUGGER");
			if (m_items != null && m_items.Count > 0)
			{
				for (int i = 0; i < m_items.Count; i++)
				{
					object obj = m_items[i];
					if (obj is PostProcessingModel)
					{
						PostProcessingModel postProcessingModel = obj as PostProcessingModel;
						num = DrawOptionItem(i, postProcessingModel.enabled, postProcessingModel.GetType().Name, num, p_show_id_group: false);
					}
					else
					{
						Behaviour behaviour = obj as Behaviour;
						num = DrawOptionItem(i, behaviour.enabled, obj.GetType().Name, num, p_show_id_group: true, num2);
						num2++;
					}
				}
			}
			else
			{
				GUI.contentColor = Color.red;
				GUI.Label(new Rect(10f, num += 20f, 1910f, 20f), "NO COMPONENTS AVAILABLE");
			}
			GUI.contentColor = Color.white;
			GUI.Label(new Rect(10f, num += 20f, 400f, 20f), "SELECT:Y/X | ACTION:VIEW");
		}

		private float DrawOptionItem(int p_id, bool p_state, string p_name, float p_line_y = 0f, bool p_show_id_group = true, int p_id_group = 0)
		{
			float num = (p_line_y += 20f);
			string arg = (p_show_id_group ? ("[" + p_id_group + "]") : "   ");
			string arg2 = (p_state ? "[X]" : "[ ]");
			string arg3 = "[" + p_name + "]";
			string text = $"{arg}{arg2}{arg3}";
			Color color = (m_willRemove ? Color.red : Color.gray);
			GUI.contentColor = ((m_current == p_id) ? color : Color.white);
			GUI.Label(new Rect(10f, num, 1910f, 20f), text);
			GUI.contentColor = Color.white;
			return num;
		}
	}
}
