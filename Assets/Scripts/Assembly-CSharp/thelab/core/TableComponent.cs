using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	[ExecuteInEditMode]
	public class TableComponent : TableComponent<Component>
	{
	}
	[ExecuteInEditMode]
	public class TableComponent<T> : ListComponent<T> where T : Component
	{
		[SerializeField]
		private GridLayoutGroup m_grid;

		public T headerTemplate;

		public GridLayoutGroup grid
		{
			get
			{
				return m_grid ?? (m_grid = GetComponent<GridLayoutGroup>());
			}
			set
			{
				m_grid = value;
			}
		}

		protected override void Awake()
		{
		}

		public void Resize(int p_rows, int p_columns, float p_width = -1f, float p_height = -1f)
		{
			if ((bool)grid)
			{
				Clear(p_destroy: true);
				float x = ((p_width < 0f) ? grid.cellSize.x : p_width);
				float y = ((p_height < 0f) ? grid.cellSize.y : p_height);
				grid.cellSize = new Vector2(x, y);
				grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
				grid.constraintCount = p_columns;
				int num = p_rows * p_columns;
				for (int i = 0; i < num; i++)
				{
					_ = i % p_columns;
					T original = ((i / p_columns <= 0) ? headerTemplate : template);
					original = Object.Instantiate(original);
					Insert(i, original);
				}
			}
		}

		public U Get<U>(int p_row, int p_col) where U : Component
		{
			if (!grid)
			{
				return null;
			}
			int p_id = grid.constraintCount * p_row + p_col;
			return Get<U>(p_id);
		}

		public Text Set(int p_row, int p_col, string p_value)
		{
			Text text = Get<Text>(p_row, p_col);
			if ((bool)text)
			{
				text.text = p_value;
			}
			return text;
		}

		public Text Set(int p_row, int p_col, string p_value, Color p_color, TextAnchor p_alignment = TextAnchor.MiddleRight)
		{
			Text text = Set(p_row, p_col, p_value);
			if ((bool)text)
			{
				text.color = p_color;
				text.alignment = p_alignment;
			}
			return text;
		}

		public List<Text> SetRange(int p_row_from, int p_col_from, int p_row_to, int p_col_to, string p_value)
		{
			List<Text> list = new List<Text>();
			for (int i = p_row_from; i <= p_row_to; i++)
			{
				for (int j = p_col_from; j <= p_col_to; j++)
				{
					Text text = Get<Text>(i, j);
					list.Add(text);
					if ((bool)text)
					{
						text.text = p_value;
					}
				}
			}
			return list;
		}

		public List<Text> SetRange(int p_row_from, int p_col_from, int p_row_to, int p_col_to, string p_value, Color p_color)
		{
			List<Text> list = SetRange(p_row_from, p_col_from, p_row_to, p_col_to, p_value);
			for (int i = 0; i < list.Count; i++)
			{
				if ((bool)list[i])
				{
					list[i].color = p_color;
				}
			}
			return list;
		}

		public void SetRange<U>(int p_row_from, int p_col_from, int p_row_to, int p_col_to, string p_value, ref IList<U> p_list) where U : Component
		{
			for (int i = p_row_from; i <= p_row_to; i++)
			{
				for (int j = p_col_from; j <= p_col_to; j++)
				{
					U item = Get<U>(i, j);
					p_list.Add(item);
				}
			}
		}
	}
}
