using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class MASplineControlPoint : MAGuide
	{
		[SerializeField]
		private int m_index = -1;

		[SerializeField]
		private MASpline m_spline;

		[SerializeField]
		private BillboardComponent m_billboard;

		public int index
		{
			get
			{
				return m_index;
			}
			set
			{
				m_index = value;
				Write();
			}
		}

		public MASpline spline
		{
			get
			{
				if (!m_spline)
				{
					return m_spline = Hierarchy.FindReverse<MASpline>(base.transform);
				}
				return m_spline;
			}
		}

		public new MDSplineControlPoint data
		{
			get
			{
				return base.data as MDSplineControlPoint;
			}
			set
			{
				base.data = value;
			}
		}

		public BillboardComponent billboard
		{
			get
			{
				if (!m_billboard)
				{
					return m_billboard = GetComponent<BillboardComponent>();
				}
				return m_billboard;
			}
		}

		public override void SetEnabled(bool p_flag)
		{
			base.SetEnabled(p_flag);
			if ((bool)billboard)
			{
				billboard.enabled = p_flag;
			}
		}

		public override void Write()
		{
			base.Write();
			MDSplineControlPoint mDSplineControlPoint = data;
			if (mDSplineControlPoint != null)
			{
				mDSplineControlPoint.index = index;
			}
		}

		public override void Read()
		{
			if (m_data is MDSplineControlPoint mDSplineControlPoint)
			{
				m_index = mDSplineControlPoint.index;
			}
			base.Read();
		}

		protected override void OnRefresh()
		{
			base.OnRefresh();
			if ((bool)spline)
			{
				switch (spline.splineCategory)
				{
				case SplineCategory.Visual:
					SetColor(Color.white);
					SetAssetActive("axis", p_flag: false);
					break;
				case SplineCategory.RaceLine:
					SetColor(Color.red);
					SetAssetActive("axis", p_flag: false);
					break;
				case SplineCategory.CourseCamera:
					SetColor(DRLColor.yellowDark);
					SetAssetActive("axis", p_flag: true);
					break;
				}
			}
		}

		protected override MDObject NewData()
		{
			return new MDSplineControlPoint();
		}
	}
}
