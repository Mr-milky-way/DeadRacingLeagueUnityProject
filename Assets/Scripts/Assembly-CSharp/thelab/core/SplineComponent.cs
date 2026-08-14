using System;
using UnityEngine;

namespace thelab.core
{
	[ExecuteInEditMode]
	public class SplineComponent : MonoBehaviour
	{
		public SplineType type = SplineType.Catmull;

		private Vector3Spline m_positions;

		private Vector3Spline m_rotations;

		private Vector3Spline m_ups;

		private Vector3Spline m_forwards;

		private Vector3Spline m_scales;

		[SerializeField]
		[HideInInspector]
		private SplineTangentMode[] m_modes;

		internal int m_rev;

		public Vector3Spline positions
		{
			get
			{
				bool flag = m_positions == null;
				m_positions = (flag ? (m_positions = new Vector3Spline(type, 0)) : m_positions);
				if (flag)
				{
					Refresh();
				}
				return m_positions;
			}
		}

		public Vector3Spline rotations
		{
			get
			{
				bool flag = m_rotations == null;
				m_rotations = (flag ? (m_rotations = new Vector3Spline(type, 0)) : m_rotations);
				if (flag)
				{
					Refresh();
				}
				return m_rotations;
			}
		}

		public Vector3Spline ups
		{
			get
			{
				bool flag = m_ups == null;
				m_ups = (flag ? (m_ups = new Vector3Spline(type, 0)) : m_ups);
				if (flag)
				{
					Refresh();
				}
				return m_ups;
			}
		}

		public Vector3Spline forwads
		{
			get
			{
				bool flag = m_forwards == null;
				m_forwards = (flag ? (m_forwards = new Vector3Spline(type, 0)) : m_forwards);
				if (flag)
				{
					Refresh();
				}
				return m_forwards;
			}
		}

		public Vector3Spline scales
		{
			get
			{
				bool flag = m_scales == null;
				m_scales = (flag ? (m_scales = new Vector3Spline(type, 0)) : m_scales);
				if (flag)
				{
					Refresh();
				}
				return m_scales;
			}
		}

		public SplineTangentMode[] modes
		{
			get
			{
				if (m_modes == null)
				{
					m_modes = new SplineTangentMode[0];
				}
				if (m_modes.Length != positions.values.Length)
				{
					Array.Resize(ref m_modes, positions.values.Length);
				}
				return m_modes;
			}
		}

		public bool hasChanged
		{
			get
			{
				for (int i = 0; i < base.transform.childCount; i++)
				{
					if (base.transform.GetChild(i).hasChanged)
					{
						return true;
					}
				}
				return base.transform.hasChanged;
			}
			set
			{
				for (int i = 0; i < base.transform.childCount; i++)
				{
					base.transform.GetChild(i).hasChanged = value;
				}
				base.transform.hasChanged = value;
			}
		}

		protected void Awake()
		{
			Refresh();
		}

		public void SetType(SplineType p_type)
		{
			type = p_type;
			positions.type = p_type;
			rotations.type = p_type;
			ups.type = p_type;
			forwads.type = p_type;
			scales.type = p_type;
		}

		public void Refresh()
		{
			Transform[] array = new Transform[base.transform.childCount];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = base.transform.GetChild(i);
			}
			Vector3[] array2 = new Vector3[array.Length];
			for (int j = 0; j < array.Length; j++)
			{
				array2[j] = array[j].position;
			}
			positions.values = array2;
			Vector3[] array3 = new Vector3[array.Length];
			for (int k = 0; k < array.Length; k++)
			{
				array3[k] = array[k].localEulerAngles;
			}
			rotations.values = array3;
			Vector3[] array4 = new Vector3[array.Length];
			for (int l = 0; l < array.Length; l++)
			{
				array4[l] = array[l].localScale;
			}
			scales.values = array4;
			Vector3[] array5 = new Vector3[array.Length];
			for (int m = 0; m < array.Length; m++)
			{
				array5[m] = array[m].up;
			}
			ups.values = array5;
			Vector3[] array6 = new Vector3[array.Length];
			for (int n = 0; n < array.Length; n++)
			{
				array6[n] = array[n].forward;
			}
			forwads.values = array6;
			m_rev++;
		}
	}
}
