using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIGarageColorsView : View<DRLApp>
	{
		[Header("Picker Focus/Outline")]
		public FadeComponent trailColorsFocus;

		public FadeComponent propColorsFocus;

		public FadeComponent textureColorsFocus;

		public FadeComponent edgeColorsFocus;

		[Header("Navigation")]
		public UINavigation trailColorsNavigation;

		public UINavigation propColorsNavigation;

		public UINavigation textureColorsNavigation;

		public UINavigation edgeColorsNavigation;

		[Header("Swatches")]
		public List<FadeComponent> trailColorSwatches;

		public List<FadeComponent> propColorSwatches;

		public List<FadeComponent> textureColorSwatches;

		public List<FadeComponent> edgeColorSwatches;

		[Header("Color Focuses/Outlines")]
		public List<FadeComponent> trailColorFocuses;

		public List<FadeComponent> propColorFocuses;

		public List<FadeComponent> textureColorFocuses;

		public List<FadeComponent> edgeColorFocuses;

		[HideInInspector]
		public UIElementView lastUnfocusedColor;

		public int m_trailColorSelected;

		public int m_propColorSelected;

		public int m_textureColorSelected;

		public int m_edgeColorSelected;

		public Dictionary<Color, int> m_profileColorToIndex;

		public Dictionary<Color, int> m_partsColorToIndex;

		public void SetColorFocus(Component p_target, List<FadeComponent> p_list, List<FadeComponent> p_outlines, ref int p_index)
		{
			Transform transform = (p_target ? p_target.transform : null);
			for (int i = 0; i < p_list.Count; i++)
			{
				FadeComponent fadeComponent = p_list[i];
				FadeComponent fadeComponent2 = p_outlines[i];
				bool flag = (bool)transform && fadeComponent.transform == transform;
				fadeComponent.Fade((flag || p_index == i) ? 1f : 0.5f);
				fadeComponent2.Fade((flag || p_index == i) ? 1f : 0f);
			}
		}

		public void UnfocusColor(Component p_target, List<FadeComponent> p_list, List<FadeComponent> p_outlines, ref int p_index)
		{
			Transform transform = (p_target ? p_target.transform : null);
			for (int i = 0; i < p_list.Count; i++)
			{
				FadeComponent fadeComponent = p_list[i];
				FadeComponent fadeComponent2 = p_outlines[i];
				if ((bool)transform && fadeComponent.transform == transform)
				{
					fadeComponent.Fade((p_index != i) ? 0.5f : 1f);
					fadeComponent2.Fade((p_index != i) ? 0f : 1f);
				}
			}
		}

		public void ClearFocusFromPickers()
		{
			edgeColorsFocus.Fade(0f);
			textureColorsFocus.Fade(0f);
			trailColorsFocus.Fade(0f);
			propColorsFocus.Fade(0f);
		}

		public void SetEdgePickerFocus()
		{
			edgeColorsFocus.Fade(1f);
			textureColorsFocus.Fade(0f);
			trailColorsFocus.Fade(0f);
			propColorsFocus.Fade(0f);
		}

		public void SetTexturePickerFocus()
		{
			edgeColorsFocus.Fade(0f);
			textureColorsFocus.Fade(1f);
			trailColorsFocus.Fade(0f);
			propColorsFocus.Fade(0f);
		}

		public void SetTrailPickerFocus()
		{
			edgeColorsFocus.Fade(0f);
			textureColorsFocus.Fade(0f);
			trailColorsFocus.Fade(1f);
			propColorsFocus.Fade(0f);
		}

		public void SetPropPickerFocus()
		{
			edgeColorsFocus.Fade(0f);
			textureColorsFocus.Fade(0f);
			trailColorsFocus.Fade(0f);
			propColorsFocus.Fade(1f);
		}

		public void SelectColor(Component p_target, List<FadeComponent> p_list, List<FadeComponent> p_outlines, ref int p_index)
		{
			Transform transform = (p_target ? p_target.transform : null);
			for (int i = 0; i < p_list.Count; i++)
			{
				FadeComponent fadeComponent = p_list[i];
				FadeComponent fadeComponent2 = p_outlines[i];
				if ((bool)transform && fadeComponent.transform == transform)
				{
					p_outlines[p_index].Fade(0f);
					p_index = i;
					fadeComponent2.Fade(1f);
				}
			}
		}

		public void SetDRLColors()
		{
			m_profileColorToIndex = new Dictionary<Color, int>();
			m_partsColorToIndex = new Dictionary<Color, int>();
			Color[] profileColors = DRLColor.profileColors;
			int num = Mathf.Min(profileColors.Length, trailColorSwatches.Count);
			for (int i = 0; i < num; i++)
			{
				Transform transform = trailColorSwatches[i].transform.Find("image");
				if ((bool)transform)
				{
					Image component = transform.GetComponent<Image>();
					component.color = profileColors[i + 1];
					m_profileColorToIndex.Add(component.color, i);
				}
			}
			num = Mathf.Min(profileColors.Length, propColorSwatches.Count);
			for (int j = 0; j < num; j++)
			{
				Transform transform2 = propColorSwatches[j].transform.Find("image");
				if ((bool)transform2)
				{
					Image component2 = transform2.GetComponent<Image>();
					component2.color = profileColors[j];
					m_partsColorToIndex.Add(component2.color, j);
				}
			}
			num = Mathf.Min(profileColors.Length, textureColorSwatches.Count);
			for (int k = 0; k < num; k++)
			{
				Transform transform3 = textureColorSwatches[k].transform.Find("image");
				if ((bool)transform3)
				{
					transform3.GetComponent<Image>().color = profileColors[k];
				}
			}
			num = Mathf.Min(profileColors.Length, edgeColorSwatches.Count);
			for (int l = 0; l < num; l++)
			{
				Transform transform4 = edgeColorSwatches[l].transform.Find("image");
				if ((bool)transform4)
				{
					transform4.GetComponent<Image>().color = profileColors[l];
				}
			}
		}

		public void SelectColorsFromDroneRig(DroneRigData p_rigData)
		{
			textureColorFocuses[m_textureColorSelected].Fade(0f);
			textureColorSwatches[m_textureColorSelected].Fade(0.5f);
			edgeColorFocuses[m_edgeColorSelected].Fade(0f);
			edgeColorSwatches[m_edgeColorSelected].Fade(0.5f);
			propColorFocuses[m_propColorSelected].Fade(0f);
			propColorSwatches[m_propColorSelected].Fade(0.5f);
			trailColorFocuses[m_trailColorSelected].Fade(0f);
			trailColorSwatches[m_trailColorSelected].Fade(0.5f);
			m_textureColorSelected = (m_partsColorToIndex.ContainsKey(p_rigData.color0) ? m_partsColorToIndex[p_rigData.color0] : 0);
			m_edgeColorSelected = (m_partsColorToIndex.ContainsKey(p_rigData.color1) ? m_partsColorToIndex[p_rigData.color1] : 0);
			m_propColorSelected = (m_partsColorToIndex.ContainsKey(p_rigData.color2) ? m_partsColorToIndex[p_rigData.color2] : 0);
			m_trailColorSelected = (m_profileColorToIndex.ContainsKey(base.app.model.storage.state.player.profile.color) ? m_profileColorToIndex[base.app.model.storage.state.player.profile.color] : 0);
			textureColorFocuses[m_textureColorSelected].Fade(1f);
			textureColorSwatches[m_textureColorSelected].Fade(1f);
			edgeColorFocuses[m_edgeColorSelected].Fade(1f);
			edgeColorSwatches[m_edgeColorSelected].Fade(1f);
			propColorFocuses[m_propColorSelected].Fade(1f);
			propColorSwatches[m_propColorSelected].Fade(1f);
			trailColorFocuses[m_trailColorSelected].Fade(1f);
			trailColorSwatches[m_trailColorSelected].Fade(1f);
		}
	}
}
