using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MECameraMetricWidget : NotificationView<DRLApp>
	{
		public enum Mode
		{
			Position = 0,
			Raycast = 1,
			RaycastRelative = 2
		}

		private UIMapEditorView m_screen;

		public Mode mode;

		[SerializeField]
		private MECamera m_camera;

		public float rate = 0.05f;

		public List<TextMetric> fields;

		public SwitcherComponent iconModes;

		private Vector3 m_last_p0;

		private float m_refresh_elapsed;

		private Transform m_anchor;

		public UIMapEditorView screen
		{
			get
			{
				if (!m_screen && (bool)this && (bool)base.transform)
				{
					m_screen = Hierarchy.FindReverse<UIMapEditorView>(base.transform);
				}
				return m_screen;
			}
		}

		public MECamera camera
		{
			get
			{
				if ((bool)m_camera)
				{
					return m_camera;
				}
				if ((bool)screen && (bool)screen.editor)
				{
					m_camera = screen.editor.camera;
				}
				return m_camera;
			}
			set
			{
				m_camera = value;
			}
		}

		protected void Awake()
		{
			m_anchor = new GameObject("camera-metric-anchor").transform;
			m_anchor.gameObject.hideFlags = HideFlags.HideAndDontSave;
			m_last_p0 = Vector3.one * float.PositiveInfinity;
			Activity.Run(OnUpdate, 0f, false);
		}

		public void SetMetricFormat(MEMetricMode p_mode)
		{
			TextMetric.ValueFormat outputFormat = ((p_mode == MEMetricMode.Metric) ? TextMetric.ValueFormat.MetricDistance : TextMetric.ValueFormat.ImperialDistance);
			for (int i = 0; i < fields.Count; i++)
			{
				fields[i].outputFormat = outputFormat;
				fields[i].Refresh();
			}
		}

		public void SetField(int p_id, float p_value, bool p_force)
		{
			if (p_id >= 0 && p_id < fields.Count)
			{
				if (p_force)
				{
					fields[p_id].value = float.PositiveInfinity;
				}
				fields[p_id].value = p_value;
			}
		}

		public void SetField(int p_id, float p_value)
		{
			SetField(p_id, p_value, p_force: false);
		}

		public void SetField(int p_id, string p_value)
		{
			if (p_id >= 0 && p_id < fields.Count)
			{
				fields[p_id].SetText(p_value);
			}
		}

		public void SetFields(string p_value)
		{
			for (int i = 0; i < fields.Count; i++)
			{
				fields[i].SetText(p_value);
			}
		}

		public void SetFields(Vector3 p_value, bool p_force)
		{
			SetField(0, p_value.x, p_force);
			SetField(1, p_value.y, p_force);
			SetField(2, p_value.z, p_force);
		}

		public void SetFields(Vector3 p_value)
		{
			SetFields(p_value, p_force: false);
		}

		public void Refresh(bool p_force = false)
		{
			if (!camera)
			{
				SetFields("-");
				return;
			}
			Vector3 position = camera.transform.position;
			switch (mode)
			{
			case Mode.Position:
				if (p_force || !((m_last_p0 - position).sqrMagnitude < 0.005f))
				{
					m_last_p0 = position;
					SetFields(position, p_force: true);
				}
				break;
			case Mode.Raycast:
			case Mode.RaycastRelative:
			{
				if (Physics.Raycast(camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo, 1000f, MESceneView.RaycastFlags, QueryTriggerInteraction.Collide))
				{
					Vector3 vector = hitInfo.point;
					if (mode == Mode.RaycastRelative)
					{
						Transform anchor = m_anchor;
						Vector3 upwards = Vector3.up;
						Vector3 forward = camera.transform.forward;
						forward.y = 0f;
						forward.Normalize();
						if (forward.magnitude < 0.05f)
						{
							upwards = Vector3.forward;
							forward = camera.transform.up;
							forward.y = 0f;
						}
						anchor.position = camera.transform.position;
						anchor.localRotation = Quaternion.LookRotation(forward, upwards);
						vector = anchor.InverseTransformPoint(vector);
					}
					SetFields(vector, p_force: true);
					SetField(3, hitInfo.distance, p_force: true);
				}
				else
				{
					SetFields("-");
				}
				break;
			}
			}
		}

		public bool OnUpdate()
		{
			if (!this)
			{
				return false;
			}
			if ((bool)camera)
			{
				Mode mode = this.mode;
				if ((uint)(mode - 1) <= 1u)
				{
					bool keyDown = Input.GetKeyDown(KeyCode.LeftShift);
					bool flag = screen.editor.model.state.input == MEInputStateType.Action;
					if (keyDown && flag)
					{
						this.mode = ((this.mode != Mode.Raycast) ? Mode.Raycast : Mode.RaycastRelative);
						iconModes.index = ((this.mode != Mode.Raycast) ? 1 : 0);
					}
				}
			}
			m_refresh_elapsed += Time.unscaledDeltaTime;
			if (m_refresh_elapsed < rate)
			{
				return true;
			}
			m_refresh_elapsed = 0f;
			Refresh();
			return true;
		}
	}
}
