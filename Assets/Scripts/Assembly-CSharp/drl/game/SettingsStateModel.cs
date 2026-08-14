using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class SettingsStateModel : Model<DRLApp>
	{
		private GraphicsStateModel m_graphics_state_model;

		public PlayerStateModel parent => AssertParent<PlayerStateModel>("parent");

		public DataFlow data => parent.data;

		public AudioStateModel audio => AssertFind<AudioStateModel>("audio");

		public GraphicsStateModel graphics
		{
			get
			{
				if ((bool)m_graphics_state_model)
				{
					return m_graphics_state_model;
				}
				if (!m_graphics_state_model)
				{
					m_graphics_state_model = AssertFind<GraphicsStateModel>("graphics");
				}
				string text = "graphics-standalone";
				m_graphics_state_model = AssertFind<GraphicsStateModel>(text);
				List<GraphicsStateModel> list = Hierarchy.FindAll<GraphicsStateModel>(base.transform);
				for (int i = 0; i < list.Count; i++)
				{
					if ((bool)list[i] && list[i].name != text)
					{
						Object.Destroy(list[i].gameObject);
					}
				}
				return m_graphics_state_model;
			}
		}

		public ControllerProfileStateModel controller => AssertFind<ControllerProfileStateModel>("controller");

		public TuningStateModel tuning => AssertFind<TuningStateModel>("tuning");

		public GameStateModel game => AssertFind<GameStateModel>("game");

		public void Refresh()
		{
			if ((bool)parent)
			{
				parent.Refresh();
			}
		}
	}
}
