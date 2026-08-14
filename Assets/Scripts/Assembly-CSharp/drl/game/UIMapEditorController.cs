using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMapEditorController : Controller<DRLApp>
	{
		private MapEditorController m_editor;

		[Header("Inspector Panel")]
		public MEInspectorPanelController inspector;

		public MapEditorController editor
		{
			get
			{
				return m_editor;
			}
			set
			{
				m_editor = value;
				inspector.editor = m_editor;
				view.controls.editor = m_editor;
			}
		}

		public UIMapEditorView view => AssertLocal<UIMapEditorView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event != null)
			{
				_ = p_event == "ui.screen@close";
			}
			if (!(base.app.view.ui.screens.current != view.screen) && p_event != null && p_event == "ui.screen@open" && !(p_data[0] as UIScreen != view.screen))
			{
				base.app.view.ui.SetDark(p_flag: false);
			}
		}
	}
}
