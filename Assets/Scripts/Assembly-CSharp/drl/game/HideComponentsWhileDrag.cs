using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace drl.game
{
	public class HideComponentsWhileDrag : MonoBehaviour
	{
		public float dragThreshold;

		public List<GameObject> disableWhenOrbit;

		private Vector3 m_startMousePosition;

		private float m_pathLength;

		private bool m_inDrag;

		private void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				m_startMousePosition = Input.mousePosition;
			}
			if (Input.GetMouseButton(0))
			{
				m_pathLength = (Input.mousePosition - m_startMousePosition).magnitude;
				if (m_pathLength > dragThreshold)
				{
					m_inDrag = true;
					DisableUI();
				}
			}
			if (Input.GetMouseButtonUp(0) && m_inDrag)
			{
				EnableUI();
				m_inDrag = false;
			}
		}

		public void DisableUI()
		{
			for (int i = 0; i < disableWhenOrbit.Count; i++)
			{
				if (!(disableWhenOrbit[i] == null) && !(disableWhenOrbit[i].gameObject == null))
				{
					CanvasGroup component = disableWhenOrbit[i].GetComponent<CanvasGroup>();
					if (component != null)
					{
						component.blocksRaycasts = false;
					}
					DRLScrollView component2 = disableWhenOrbit[i].GetComponent<DRLScrollView>();
					if (component2 != null)
					{
						component2.enabled = false;
					}
					GraphicRaycaster component3 = disableWhenOrbit[i].GetComponent<GraphicRaycaster>();
					if (component3 != null)
					{
						component3.enabled = false;
					}
				}
			}
		}

		public void EnableUI()
		{
			for (int i = 0; i < disableWhenOrbit.Count; i++)
			{
				if (!(disableWhenOrbit[i] == null) && !(disableWhenOrbit[i].gameObject == null))
				{
					CanvasGroup component = disableWhenOrbit[i].GetComponent<CanvasGroup>();
					if (component != null)
					{
						component.blocksRaycasts = true;
					}
					DRLScrollView component2 = disableWhenOrbit[i].GetComponent<DRLScrollView>();
					if (component2 != null)
					{
						component2.enabled = true;
					}
					GraphicRaycaster component3 = disableWhenOrbit[i].GetComponent<GraphicRaycaster>();
					if (component3 != null)
					{
						component3.enabled = true;
					}
				}
			}
		}
	}
}
