using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIGarageColorsController : Controller<DRLApp>
	{
		public UIGarageRigEditController garage;

		public UIGarageColorsView view => AssertLocal<UIGarageColorsView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "garage.edit.trailcolors@click":
				UINavigation.focus = view.trailColorSwatches[0].GetComponent<UINavigation>();
				view.ClearFocusFromPickers();
				break;
			case "garage.edit.propcolors@click":
				UINavigation.focus = view.propColorSwatches[0].GetComponent<UINavigation>();
				view.ClearFocusFromPickers();
				break;
			case "garage.edit.texturecolors@click":
				UINavigation.focus = view.textureColorSwatches[0].GetComponent<UINavigation>();
				view.ClearFocusFromPickers();
				break;
			case "garage.edit.edgecolors@click":
				UINavigation.focus = view.edgeColorSwatches[0].GetComponent<UINavigation>();
				view.ClearFocusFromPickers();
				break;
			case "garage.edit.trailcolors@focus":
				if (garage.view.scroll.mode == NavigationModeType.Controller)
				{
					view.SetTrailPickerFocus();
					break;
				}
				view.ClearFocusFromPickers();
				view.SetColorFocus(view.lastUnfocusedColor, view.trailColorSwatches, view.trailColorFocuses, ref view.m_trailColorSelected);
				break;
			case "garage.edit.propcolors@focus":
				if (garage.view.scroll.mode == NavigationModeType.Controller)
				{
					view.SetPropPickerFocus();
					break;
				}
				view.ClearFocusFromPickers();
				view.SetColorFocus(view.lastUnfocusedColor, view.propColorSwatches, view.propColorFocuses, ref view.m_propColorSelected);
				break;
			case "garage.edit.texturecolors@focus":
				if (garage.view.scroll.mode == NavigationModeType.Controller)
				{
					view.SetTexturePickerFocus();
					break;
				}
				view.ClearFocusFromPickers();
				view.SetColorFocus(view.lastUnfocusedColor, view.textureColorSwatches, view.textureColorFocuses, ref view.m_textureColorSelected);
				break;
			case "garage.edit.edgecolors@focus":
				if (garage.view.scroll.mode == NavigationModeType.Controller)
				{
					view.SetEdgePickerFocus();
					break;
				}
				view.ClearFocusFromPickers();
				view.SetColorFocus(view.lastUnfocusedColor, view.edgeColorSwatches, view.edgeColorFocuses, ref view.m_edgeColorSelected);
				break;
			case "garage.edit.trailcolors@unfocus":
				if (garage.view.scroll.mode == NavigationModeType.Mouse)
				{
					view.UnfocusColor(view.lastUnfocusedColor, view.trailColorSwatches, view.trailColorFocuses, ref view.m_trailColorSelected);
				}
				break;
			case "garage.edit.propcolors@unfocus":
				if (garage.view.scroll.mode == NavigationModeType.Mouse)
				{
					view.UnfocusColor(view.lastUnfocusedColor, view.propColorSwatches, view.propColorFocuses, ref view.m_propColorSelected);
				}
				break;
			case "garage.edit.texturecolors@unfocus":
				if (garage.view.scroll.mode == NavigationModeType.Mouse)
				{
					view.UnfocusColor(view.lastUnfocusedColor, view.textureColorSwatches, view.textureColorFocuses, ref view.m_textureColorSelected);
				}
				break;
			case "garage.edit.edgecolors@unfocus":
				if (garage.view.scroll.mode == NavigationModeType.Mouse)
				{
					view.UnfocusColor(view.lastUnfocusedColor, view.edgeColorSwatches, view.edgeColorFocuses, ref view.m_edgeColorSelected);
				}
				break;
			case "garage.edit.rig-trailcolor@click":
			{
				UIElementView uIElementView4 = p_target as UIElementView;
				int siblingIndex = uIElementView4.transform.GetSiblingIndex();
				Color p_color4 = DRLColor.profileColors[siblingIndex];
				if (garage.ChangeProfileAndTrailColor(p_color4))
				{
					view.SelectColor(uIElementView4, view.trailColorSwatches, view.trailColorFocuses, ref view.m_trailColorSelected);
				}
				UINavigation.Focus(view.trailColorsNavigation);
				break;
			}
			case "garage.edit.rig-propcolor@click":
			{
				UIElementView uIElementView3 = p_target as UIElementView;
				int num3 = uIElementView3.transform.GetSiblingIndex() - 1;
				Color p_color3 = DRLColor.profileColors[num3];
				garage.ChangePropColors(p_color3);
				view.SelectColor(uIElementView3, view.propColorSwatches, view.propColorFocuses, ref view.m_propColorSelected);
				UINavigation.Focus(view.propColorsNavigation);
				break;
			}
			case "garage.edit.rig-edgecolor@click":
			{
				UIElementView uIElementView2 = p_target as UIElementView;
				int num2 = uIElementView2.transform.GetSiblingIndex() - 1;
				Color p_color2 = DRLColor.profileColors[num2];
				garage.ChangeEdgeColor(p_color2);
				view.SelectColor(uIElementView2, view.edgeColorSwatches, view.edgeColorFocuses, ref view.m_edgeColorSelected);
				UINavigation.Focus(view.edgeColorsNavigation);
				break;
			}
			case "garage.edit.rig-texturecolor@click":
			{
				UIElementView uIElementView = p_target as UIElementView;
				int num = uIElementView.transform.GetSiblingIndex() - 1;
				Color p_color = DRLColor.profileColors[num];
				garage.ChangeTextureColor(p_color);
				view.SelectColor(uIElementView, view.textureColorSwatches, view.textureColorFocuses, ref view.m_textureColorSelected);
				UINavigation.Focus(view.textureColorsNavigation);
				break;
			}
			case "garage.edit.rig-trailcolor@focus":
			{
				UIElementView p_target5 = p_target as UIElementView;
				view.SetColorFocus(p_target5, view.trailColorSwatches, view.trailColorFocuses, ref view.m_trailColorSelected);
				break;
			}
			case "garage.edit.rig-propcolor@focus":
			{
				UIElementView p_target4 = p_target as UIElementView;
				view.SetColorFocus(p_target4, view.propColorSwatches, view.propColorFocuses, ref view.m_propColorSelected);
				break;
			}
			case "garage.edit.rig-texturecolor@focus":
			{
				UIElementView p_target3 = p_target as UIElementView;
				view.SetColorFocus(p_target3, view.textureColorSwatches, view.textureColorFocuses, ref view.m_textureColorSelected);
				break;
			}
			case "garage.edit.rig-edgecolor@focus":
			{
				UIElementView p_target2 = p_target as UIElementView;
				view.SetColorFocus(p_target2, view.edgeColorSwatches, view.edgeColorFocuses, ref view.m_edgeColorSelected);
				break;
			}
			case "garage.edit.rig-trailcolor@unfocus":
				view.lastUnfocusedColor = p_target as UIElementView;
				view.UnfocusColor(view.lastUnfocusedColor, view.trailColorSwatches, view.trailColorFocuses, ref view.m_trailColorSelected);
				break;
			case "garage.edit.rig-propcolor@unfocus":
				view.lastUnfocusedColor = p_target as UIElementView;
				view.UnfocusColor(view.lastUnfocusedColor, view.propColorSwatches, view.propColorFocuses, ref view.m_propColorSelected);
				break;
			case "garage.edit.rig-texturecolor@unfocus":
				view.lastUnfocusedColor = p_target as UIElementView;
				view.UnfocusColor(view.lastUnfocusedColor, view.textureColorSwatches, view.textureColorFocuses, ref view.m_textureColorSelected);
				break;
			case "garage.edit.rig-edgecolor@unfocus":
				view.lastUnfocusedColor = p_target as UIElementView;
				view.UnfocusColor(view.lastUnfocusedColor, view.edgeColorSwatches, view.edgeColorFocuses, ref view.m_edgeColorSelected);
				break;
			}
		}
	}
}
