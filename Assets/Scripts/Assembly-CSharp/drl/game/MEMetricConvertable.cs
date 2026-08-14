using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MEMetricConvertable : View<DRLApp>
	{
		[SerializeField]
		private MapEditorView m_editor;

		[SerializeField]
		private UIMapEditorView m_screen;

		[SerializeField]
		private Component m_target;

		[SerializeField]
		private MEMetricMode m_mode;

		public bool isSpeed;

		public MapEditorView editor
		{
			get
			{
				if ((bool)m_editor)
				{
					return m_editor;
				}
				GetContainers();
				return m_editor;
			}
		}

		public UIMapEditorView screen
		{
			get
			{
				if ((bool)m_screen)
				{
					return m_screen;
				}
				GetContainers();
				return m_screen;
			}
		}

		public Component target
		{
			get
			{
				if ((bool)m_target)
				{
					return m_target;
				}
				m_target = Hierarchy.GetComponent<TextMetric>(base.gameObject);
				if ((bool)m_target)
				{
					return m_target;
				}
				m_target = Hierarchy.GetComponent<DRLNumberFieldView>(base.gameObject);
				if ((bool)m_target)
				{
					return m_target;
				}
				m_target = Hierarchy.GetComponent<DRLVectorFieldView>(base.gameObject);
				if ((bool)m_target)
				{
					return m_target;
				}
				m_target = Hierarchy.GetComponent<GridComponent>(base.gameObject);
				_ = (bool)m_target;
				return m_target;
			}
		}

		public MEMetricMode mode
		{
			get
			{
				return m_mode;
			}
			set
			{
				m_mode = value;
				Refresh();
			}
		}

		protected void Awake()
		{
			Init();
		}

		private void Init()
		{
			if ((bool)editor)
			{
				mode = editor.model.state.metric.mode;
				editor.model.state.metric.AddMetricConvertable(this);
			}
		}

		private void GetContainers()
		{
			if (!base.validContext || ((bool)m_editor && (bool)m_screen))
			{
				return;
			}
			if (!m_editor)
			{
				MapEditorController mapEditorController = (base.app ? base.app.controller.game.GetMode<MapEditorController>() : null);
				if ((bool)mapEditorController)
				{
					m_editor = mapEditorController.view;
					if ((bool)m_editor && !m_screen)
					{
						m_screen = m_editor.ui;
					}
				}
			}
			if (!m_screen)
			{
				m_screen = AssertFindReverse<UIMapEditorView>();
				if ((bool)m_screen && !m_editor)
				{
					m_editor = m_screen.editor;
				}
			}
		}

		public void Refresh()
		{
			if (base.enabled)
			{
				Component component = target;
				if (component is GridComponent)
				{
					Refresh(component as GridComponent);
				}
				if (component is TextMetric)
				{
					Refresh(component as TextMetric);
				}
				if (component is DRLNumberFieldView)
				{
					Refresh(component as DRLNumberFieldView);
				}
				if (component is DRLVectorFieldView)
				{
					Refresh(component as DRLVectorFieldView);
				}
			}
		}

		protected void Refresh(TextMetric p_field)
		{
			if (!p_field)
			{
				Debug.LogWarning("MEMetricConvertable> Invalid Field / Refresh / " + base.name);
				return;
			}
			p_field.outputFormat = ((mode != MEMetricMode.Imperial) ? TextMetric.ValueFormat.MetricDistance : TextMetric.ValueFormat.ImperialDistance);
			p_field.Refresh();
		}

		protected void Refresh(GridComponent p_field)
		{
		}

		protected void Refresh(DRLNumberFieldView p_field)
		{
			if (!p_field)
			{
				Debug.LogWarning("MEMetricConvertable> Invalid Field / Refresh / " + base.name);
				return;
			}
			p_field.convertFactor = ((mode == MEMetricMode.Imperial) ? 3.28084f : 1f);
			string text = ((mode == MEMetricMode.Imperial) ? "ft" : "m");
			if (isSpeed)
			{
				text += "/s";
			}
			p_field.suffix = text;
			p_field.Refresh();
		}

		protected void Refresh(DRLVectorFieldView p_field)
		{
			if (!p_field)
			{
				Debug.LogWarning("MEMetricConvertable> Invalid Field / Refresh / " + base.name);
				return;
			}
			for (int i = 0; i < p_field.fields.Count; i++)
			{
				Refresh(p_field.fields[i]);
			}
		}
	}
}
