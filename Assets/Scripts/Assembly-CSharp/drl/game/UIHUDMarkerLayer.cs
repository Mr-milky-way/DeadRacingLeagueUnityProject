using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIHUDMarkerLayer : MonoBehaviour
	{
		public UIHUDMarker[] templates;

		public FadeComponent fade;

		[SerializeField]
		private List<Component> m_targets;

		public List<UIHUDMarker> markers;

		public Color markerColor;

		public RectOffset margins;

		[SerializeField]
		private int m_selection = -1;

		protected int m_markerUpdateCount = 15;

		protected int m_markerUpdateIndex;

		public List<Component> targets => Reflection<object>.Assert(ref m_targets);

		public int selection
		{
			get
			{
				return m_selection;
			}
			set
			{
				m_selection = Mathf.Clamp(value, -1, targets.Count - 1);
				RefreshSelection();
			}
		}

		protected void Awake()
		{
		}

		protected UIHUDMarker GetInstance(int p_template_id = 0)
		{
			if (p_template_id < 0)
			{
				return null;
			}
			if (p_template_id >= templates.Length)
			{
				return null;
			}
			UIHUDMarker uIHUDMarker = Object.Instantiate(templates[p_template_id]);
			uIHUDMarker.name = "marker";
			uIHUDMarker.transform.SetParent(base.transform, worldPositionStays: false);
			return uIHUDMarker;
		}

		public UIHUDMarker Add(Component p_target, int p_template_id = 0)
		{
			if (!Contains(p_target))
			{
				targets.Add(p_target);
			}
			UIHUDMarker marker = GetMarker(p_target);
			if ((bool)marker)
			{
				return marker;
			}
			marker = GetInstance(p_template_id);
			marker.name = "marker-" + p_target.name;
			marker.target = p_target;
			marker.margins = new RectOffset(margins.left, margins.right, margins.top, margins.bottom);
			markers.Add(marker);
			RefreshSelection();
			SetMarkerColor(marker);
			return marker;
		}

		public void SetMarkerColor(UIHUDMarker p_uiHUDMarker)
		{
			p_uiHUDMarker.color = markerColor;
		}

		public void Remove(Component p_target)
		{
			if (Contains(p_target))
			{
				UIHUDMarker marker = GetMarker(p_target);
				targets.Remove(p_target);
				if ((bool)marker)
				{
					markers.Remove(marker);
					Object.Destroy(marker.gameObject);
					RefreshSelection();
				}
			}
		}

		public void Clear()
		{
			while (markers.Count > 0)
			{
				UIHUDMarker uIHUDMarker = markers[0];
				markers.RemoveAt(0);
				Object.Destroy(uIHUDMarker.gameObject);
			}
			targets.Clear();
		}

		public UIHUDMarker GetMarker(Component p_target)
		{
			if (!Contains(p_target))
			{
				return null;
			}
			for (int i = 0; i < markers.Count; i++)
			{
				UIHUDMarker uIHUDMarker = markers[i];
				if (!(uIHUDMarker.target != p_target))
				{
					return uIHUDMarker;
				}
			}
			return null;
		}

		public bool Contains(Component p_target)
		{
			return targets.Contains(p_target);
		}

		public void SetSelection(Component p_target)
		{
			if (Contains(p_target))
			{
				selection = targets.IndexOf(p_target);
			}
		}

		protected void RefreshSelection()
		{
			int num = m_selection;
			for (int i = 0; i < markers.Count; i++)
			{
				UIHUDMarker uIHUDMarker = markers[i];
				uIHUDMarker.visible = num < 0 || num == i;
				uIHUDMarker.alpha = ((num < 0) ? 1f : ((num == i) ? 1f : 0f));
				bool bidirectional = uIHUDMarker.bidirectional;
				Graphic graphicsField = uIHUDMarker.graphicsField;
				if ((bool)graphicsField)
				{
					graphicsField.color = uIHUDMarker.color;
					graphicsField.enabled = num == i;
				}
				graphicsField = uIHUDMarker.innerGraphicsField;
				if ((bool)graphicsField)
				{
					graphicsField.enabled = num == i && !bidirectional;
				}
			}
		}

		protected void LateUpdate()
		{
			if (markers.Count <= 0)
			{
				return;
			}
			int markerUpdateIndex;
			for (int i = 0; i < m_markerUpdateCount; i++)
			{
				markerUpdateIndex = m_markerUpdateIndex;
				if (markerUpdateIndex < 0 || markerUpdateIndex >= markers.Count || markers.Count <= 0)
				{
					break;
				}
				m_markerUpdateIndex = (m_markerUpdateIndex + 1) % markers.Count;
				UIHUDMarker uIHUDMarker = markers[markerUpdateIndex];
				if (!uIHUDMarker || selection < 0 || markerUpdateIndex == selection)
				{
					continue;
				}
				if (uIHUDMarker.visible)
				{
					uIHUDMarker.visible = false;
				}
				if (!uIHUDMarker.visible && uIHUDMarker.alpha > 0f)
				{
					uIHUDMarker.alpha = 0f;
					uIHUDMarker.graphicsField.color = uIHUDMarker.color;
					uIHUDMarker.graphicsField.enabled = false;
					if ((bool)uIHUDMarker.innerGraphicsField)
					{
						uIHUDMarker.graphicsField.enabled = false;
					}
				}
			}
			markerUpdateIndex = selection;
			if (markerUpdateIndex >= 0 && markerUpdateIndex < markers.Count)
			{
				UIHUDMarker uIHUDMarker = markers[markerUpdateIndex];
				if ((bool)uIHUDMarker && !uIHUDMarker.selfUpdate && (uIHUDMarker.visible || uIHUDMarker.alpha > 0f))
				{
					uIHUDMarker.UpdateMarker();
				}
			}
		}
	}
}
