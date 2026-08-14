using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLTournamentTableView : View<DRLApp>
	{
		public TableComponent tableField;

		public DRLStandingsView standings;

		protected void Awake()
		{
			Resize(-1, 0);
		}

		public void SetCount(int p_count, bool p_visible = true)
		{
			int count = standings.listField.Count;
			for (int i = 0; i < count; i++)
			{
				DRLStandingsItemView dRLStandingsItemView = standings.Get(i);
				if ((bool)dRLStandingsItemView)
				{
					if (i >= p_count)
					{
						dRLStandingsItemView.SetVisible(p_flag: false);
						dRLStandingsItemView.gameObject.SetActive(value: false);
						continue;
					}
					dRLStandingsItemView.position = i;
					dRLStandingsItemView.hasPosition = p_count > 1;
					dRLStandingsItemView.profileName = "";
					dRLStandingsItemView.time = 0f;
					dRLStandingsItemView.backgroundColor = DRLColor.red;
					dRLStandingsItemView.SetVisible(p_visible);
					dRLStandingsItemView.gameObject.SetActive(value: true);
				}
			}
		}

		public void Resize(int p_columns, int p_row)
		{
			tableField.Resize(p_columns + 1, p_row);
			standings.Clear();
			SetCount(p_row, p_visible: false);
		}

		public void Fade(bool p_flag, float p_duration, float p_delay_step = 0.02f)
		{
			int count = standings.listField.Count;
			for (int i = 0; i < count; i++)
			{
				DRLStandingsItemView dRLStandingsItemView = standings.Get(i);
				if ((bool)dRLStandingsItemView)
				{
					float num = i;
					num *= p_delay_step;
					dRLStandingsItemView.Fade(p_flag, p_duration, num);
				}
			}
		}

		public DRLStandingsItemView Set(int p_id, Color p_color, Texture p_photo, string p_name, bool p_bold)
		{
			DRLStandingsItemView dRLStandingsItemView = standings.Get(p_id);
			if (!dRLStandingsItemView)
			{
				return dRLStandingsItemView;
			}
			p_color *= (((p_id & 1) == 0) ? 1f : 0.8f);
			p_color.a = 1f;
			dRLStandingsItemView.backgroundColor = p_color;
			dRLStandingsItemView.profilePhoto = p_photo;
			dRLStandingsItemView.profileName = p_name;
			dRLStandingsItemView.time = 0f;
			dRLStandingsItemView.bold = p_bold;
			return dRLStandingsItemView;
		}
	}
}
