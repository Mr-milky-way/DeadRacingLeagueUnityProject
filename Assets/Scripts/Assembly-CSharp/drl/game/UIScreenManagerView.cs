using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIScreenManagerView : View<DRLApp>
	{
		public bool dragScrollingEnabled = true;

		public bool mouseWheelScrollingEnabled = true;

		public ScrollRect dragScroller;

		public RectTransform clickDragLabel;

		public GameObject clickDragPC;

		public GameObject clickDragXbox;

		public GameObject clickDragPS;

		public Image clickDragPSEnterIcon;

		public Image clickDragPSBackIcon;

		public Sprite psButtonX;

		public Sprite psButtonO;

		public RawImage promoContentImage;

		public UIScreen current;

		public List<UIScreen> staticBackgroundScreens;

		private RenderTexture staticBackgroundTexture;

		public Material backgroundBlurMaterial;

		public float duration = 0.15f;

		private Activity m_SBActivity;

		public LayoutFitter bounds => Assert<LayoutFitter>("bounds");

		public UIScreenManager manager => AssertLocal<UIScreenManager>("manager");

		public UIScreenManagerController controller => AssertLocal<UIScreenManagerController>("controller");

		public FadeComponent fade => AssertLocal<FadeComponent>("fade");

		protected void Awake()
		{
			int siblingIndex = base.transform.GetSiblingIndex();
			if ((bool)bounds)
			{
				bounds.transform.SetParent(base.transform.parent, worldPositionStays: true);
				bounds.transform.SetSiblingIndex(siblingIndex);
			}
			if ((bool)dragScroller)
			{
				dragScroller.transform.SetParent(base.transform.parent, worldPositionStays: true);
				dragScroller.transform.SetSiblingIndex(siblingIndex);
			}
			if ((bool)clickDragLabel)
			{
				clickDragLabel.transform.SetParent(base.transform.parent, worldPositionStays: true);
				clickDragLabel.transform.SetSiblingIndex(siblingIndex);
			}
		}

		public Transform TryGetFirstLeftNavigationLink(UIScreen p_screen)
		{
			if (!p_screen)
			{
				return null;
			}
			UIScreenView component = p_screen.GetComponent<UIScreenView>();
			if (base.app.inGame && base.app.view.ui.game.hud.dashboard.isShowing)
			{
				return base.app.view.ui.game.hud.dashboard.transform;
			}
			if (!p_screen)
			{
				return null;
			}
			return p_screen.transform;
		}

		protected UIScreen Open(string p_id, float p_delay, bool p_notify_history)
		{
			if (!manager)
			{
				return null;
			}
			float dl = 0.1f + p_delay;
			UIScreen from = current;
			UIScreen uIScreen = manager.Get(p_id, p_create: false);
			manager.Switch(from, uIScreen, p_sequential: false, duration, dl);
			if ((bool)from)
			{
				Notify("ui.screen@switch", from, uIScreen);
			}
			UIScreen scr = uIScreen;
			if (!scr)
			{
				Debug.LogWarning("UIScreenManagerView> Open - Screen [" + p_id + "] not found!");
				return null;
			}
			UINavigationScroll uINavigationScroll = (from ? from.GetComponent<UINavigationScroll>() : null);
			if ((bool)uINavigationScroll)
			{
				uINavigationScroll.enabled = false;
			}
			this.TimerRunOnce(delegate
			{
				UINavigationSystem.Focus(scr);
				UINavigation.DisableFocusForTime(dl + duration);
				SetupScrollingSystem(scr, from);
				if ((bool)from)
				{
					from.Hide(0f);
				}
				scr.Show();
			}, dl + 1f / 60f);
			if ((bool)clickDragLabel)
			{
				clickDragLabel.gameObject.SetActive(p_id == "home-screen-grid");
			}
			if ((bool)promoContentImage)
			{
				promoContentImage.gameObject.SetActive(p_id == "home-screen-grid" && promoContentImage.texture != null);
			}
			current = scr;
			RefreshNavigationTooltips();
			bool flag = false;
			if (base.app.inGame)
			{
				string text = scr.name;
				if (text != null && text == "game-spectate-screen")
				{
					flag = true;
				}
			}
			if (flag)
			{
				base.app.view.ui.game.hud.Hide();
			}
			Notify(dl, "ui.screen@open", scr);
			Notify(dl, "ui.screen@change", scr);
			if (p_notify_history)
			{
				Notify(dl, "ui.screen.history.add", scr);
			}
			this.TimerRunOnce(delegate
			{
				UINavigationSystem.Focus(scr);
				UINavigation.DisableFocusForTime(dl + duration);
				SetupScrollingSystem(scr, from);
				if ((bool)from)
				{
					from.Hide(0f);
				}
				scr.Show();
			}, dl + 1f / 60f);
			bool flag2 = false;
			switch (scr.name)
			{
			case "map-editor-screen":
				flag2 = true;
				break;
			case "game-spectate-screen":
				flag2 = true;
				break;
			}
			if (manager.history.Any((UIScreen x) => x.name == "garage-rig-edit-screen"))
			{
				return scr;
			}
			if (flag2)
			{
				ClearStaticBackground();
			}
			else
			{
				m_SBActivity = this.TimerRunOnce(delegate
				{
					bool num = staticBackgroundScreens.Any((UIScreen o) => o.name == scr.name);
					bool flag3 = manager.history.Any((UIScreen x) => staticBackgroundScreens.Any((UIScreen y) => y.name == x.name));
					bool flag4 = false;
					if (current == null)
					{
						flag4 = true;
					}
					if (!num && !flag3)
					{
						flag4 = true;
					}
					if (flag4)
					{
						ClearStaticBackground();
					}
					else
					{
						SetStaticBackground();
					}
				}, dl + duration + 0.8f);
			}
			return scr;
		}

		public void RefreshNavigationTooltips()
		{
			if (!(current == null) && !(current.name != "home-screen-grid"))
			{
				DefaultControllerType defaultControllerType = RCI.GetDefaultControllerType(DefaultControllerType.XBox);
				bool flag = defaultControllerType == DefaultControllerType.XBox && RCI.GetActiveJoystick() != null;
				bool flag2 = defaultControllerType == DefaultControllerType.PS && RCI.GetActiveJoystick() != null;
				clickDragXbox.SetActive(flag);
				clickDragPS.SetActive(flag2);
				clickDragPC.SetActive(!flag2 && !flag);
			}
		}

		private void SetupScrollingSystem(UIScreen p_screen, UIScreen p_fromScreen)
		{
			if (p_screen == null)
			{
				return;
			}
			Debug.Log("UIScreenManagerView> SetupScrollingSystem");
			UINavigationScroll uINavigationScroll = (p_screen ? p_screen.GetComponent<UINavigationScroll>() : null);
			if (uINavigationScroll == null)
			{
				return;
			}
			uINavigationScroll.bounds = bounds;
			uINavigationScroll.enabled = true;
			if (uINavigationScroll.viewrect == null)
			{
				uINavigationScroll.viewrect = (RectTransform)base.transform;
			}
			uINavigationScroll.ResetScroll(p_force: true);
			bounds.targets.Clear();
			if (uINavigationScroll.boundsTargets.Length != 0)
			{
				for (int i = 0; i < uINavigationScroll.boundsTargets.Length; i++)
				{
					bounds.targets.Add(uINavigationScroll.boundsTargets[i]);
				}
			}
			else
			{
				bounds.targets.Add((RectTransform)p_screen.transform);
			}
			bounds.marginLeft = uINavigationScroll.boundsMarginLftBtm.x;
			bounds.marginBottom = uINavigationScroll.boundsMarginLftBtm.y;
			bounds.marginRight = uINavigationScroll.boundsMarginRgtTop.x;
			bounds.marginTop = uINavigationScroll.boundsMarginRgtTop.y;
			Debug.Log("UIScreenManagerView> bounds.Refresh");
			bounds.Refresh();
			if ((bool)dragScroller)
			{
				SetDragScrollingEnabled(dragScrollingEnabled, p_screen);
				if (dragScrollingEnabled)
				{
					dragScroller.horizontal = uINavigationScroll.scrollX;
					dragScroller.vertical = uINavigationScroll.scrollY;
					dragScroller.movementType = (uINavigationScroll.dragScrollUseElasticity ? ScrollRect.MovementType.Elastic : ScrollRect.MovementType.Clamped);
					if ((bool)uINavigationScroll.container)
					{
						dragScroller.content = uINavigationScroll.container;
					}
					else
					{
						dragScroller.content = p_screen.GetComponent<RectTransform>();
					}
					RectTransform obj = (RectTransform)dragScroller.transform;
					obj.offsetMin = uINavigationScroll.dragScrollerOffsetMinLftBtm;
					obj.offsetMax = uINavigationScroll.dragScrollerOffsetMaxRgtTop;
					uINavigationScroll.RefreshDragScrollersContentSize(0.01f);
					controller.DisableDragScrollNavigation(p_fromScreen);
					controller.DisableDragScrollNavigation(p_screen);
				}
			}
			SetMouseWheelScrollingEnabled(mouseWheelScrollingEnabled, p_screen);
		}

		public void RefreshDragScrollerOffsets(UINavigationScroll p_nav_scroll)
		{
			if (!(p_nav_scroll == null) && dragScrollingEnabled)
			{
				RectTransform obj = (RectTransform)dragScroller.transform;
				obj.offsetMin = p_nav_scroll.dragScrollerOffsetMinLftBtm;
				obj.offsetMax = p_nav_scroll.dragScrollerOffsetMaxRgtTop;
			}
		}

		public void SetDragScrollingEnabled(bool p_enabled, UIScreen p_scr = null)
		{
			UIScreen uIScreen = ((p_scr != null) ? p_scr : current);
			if (!uIScreen)
			{
				return;
			}
			UINavigationScroll component = uIScreen.GetComponent<UINavigationScroll>();
			if ((bool)component)
			{
				bool flag = (component.scrollClickAndDrag &= p_enabled);
				component.dragScroller = (flag ? dragScroller : null);
				if (!flag)
				{
					dragScroller.gameObject.SetActive(value: false);
				}
			}
		}

		public void SetMouseWheelScrollingEnabled(bool p_enabled, UIScreen p_scr = null)
		{
			UIScreen uIScreen = ((p_scr != null) ? p_scr : current);
			if ((bool)uIScreen)
			{
				UINavigationScroll component = uIScreen.GetComponent<UINavigationScroll>();
				if ((bool)component)
				{
					bool scrollMouseWheel = component.scrollMouseWheel && p_enabled;
					component.scrollMouseWheel = scrollMouseWheel;
				}
			}
		}

		public UIScreen Open(string p_id, float p_delay)
		{
			return Open(p_id, p_delay, p_notify_history: true);
		}

		public UIScreen Open(string p_id)
		{
			return Open(p_id, 0f);
		}

		public T Open<T>(string p_id, float p_delay = 0f) where T : Component
		{
			UIScreen uIScreen = Open(p_id, p_delay);
			if (!uIScreen)
			{
				return null;
			}
			return uIScreen.GetComponent<T>();
		}

		public void Close(string p_id)
		{
			if (!manager)
			{
				return;
			}
			UIScreen uIScreen = manager.Find<UIScreen>(p_id);
			if (!uIScreen)
			{
				Debug.LogWarning("UIScreenManagerView> Close - Screen [" + p_id + "] not found!");
				return;
			}
			if (uIScreen == current)
			{
				current = null;
			}
			if ((bool)uIScreen)
			{
				Notify("ui.screen@close", uIScreen);
				Notify("ui.screen@change", uIScreen);
				if (current == null)
				{
					ClearStaticBackground();
				}
				this.TimerRunOnce(delegate
				{
					if (current == null)
					{
						manager.ClearHistory();
					}
				}, 2f);
			}
			manager.Close(uIScreen.name, duration);
			if (base.app.model.game != null && current == null)
			{
				base.app.view.ui.game.hud.Show();
			}
		}

		public void CloseAllScreens()
		{
			List<UIScreen> screens = manager.GetScreens();
			for (int i = 0; i < screens.Count; i++)
			{
				UIScreen uIScreen = screens[i];
				if (uIScreen.open)
				{
					manager.Close(uIScreen);
					Notify("ui.screen@close", uIScreen);
				}
			}
			this.TimerRunOnce(delegate
			{
				if (current == null)
				{
					manager.ClearHistory();
				}
			}, 2f);
		}

		public void Return(int p_levels, float p_delay = 0f)
		{
			if (!manager)
			{
				return;
			}
			float num = 1f / 60f;
			float delay = p_delay;
			if (p_delay == 0f)
			{
				delay = num;
			}
			RunOnce(delegate
			{
				Debug.Log("UIScreenManager> Return - levels[" + p_levels + "]");
				if ((bool)base.gameObject)
				{
					int num2 = manager.history.Count;
					if (num2 > 1)
					{
						int num3 = Mathf.Min(num2, p_levels);
						UIScreen uIScreen = null;
						for (int i = 0; i < num3; i++)
						{
							uIScreen = manager.history[num2 - 1];
							if ((bool)uIScreen)
							{
								manager.Close(uIScreen, duration);
								Notify("ui.screen@close", uIScreen);
								Notify("ui.screen@change", uIScreen);
							}
							manager.history.RemoveAt(num2 - 1);
							num2--;
						}
						if (num2 > 0)
						{
							uIScreen = manager.history[num2 - 1];
							manager.history.RemoveAt(num2 - 1);
						}
						if ((bool)uIScreen)
						{
							Notify("ui.screen@return", uIScreen);
							Notify("ui.screen.history.remove", uIScreen);
							Open(uIScreen.name, 0f, p_notify_history: false);
						}
					}
				}
			}, delay);
		}

		public void GoToBreadCrumbSelectedScreen(int p_screen)
		{
			if (!manager)
			{
				return;
			}
			RunOnce(delegate
			{
				int num = p_screen;
				int count = manager.history.Count;
				Debug.Log("UIScreenManager> GoToBreadCrumbSelectedScreen - selected screen[" + num + "]");
				if (count > 1 && (num >= 0 || num < count))
				{
					UIScreen uIScreen = null;
					UIScreen uIScreen2 = manager.history[num];
					for (int num2 = count - 1; num2 >= num; num2--)
					{
						uIScreen = manager.history[num2];
						Debug.Log("UIScreenManagerView> Return - Close[" + uIScreen?.ToString() + "]" + num2);
						Notify("ui.screen@close", uIScreen);
						Close(uIScreen.name);
						manager.RemoveFromHistory(uIScreen, p_firstOnly: true);
					}
					if ((bool)uIScreen2)
					{
						Open(uIScreen2.name, 0f, p_notify_history: false);
					}
				}
			});
		}

		public T Prepare<T>(string p_id, float p_delay = 0f) where T : Component
		{
			UIScreen uIScreen = Open(p_id, p_delay);
			if (!uIScreen)
			{
				return null;
			}
			Return();
			return uIScreen.GetComponent<T>();
		}

		public void Return()
		{
			Return(1);
		}

		public bool IsCurrent(string p_screenID)
		{
			if (current == null)
			{
				return false;
			}
			return current.name == p_screenID;
		}

		public void SetStaticBackground()
		{
			if (!(base.app.model.game == null) && !base.app.view.ui.cameraBackground.gameObject.activeSelf && !(base.app.view.ui.cameraBackground.texture != null))
			{
				base.app.model.game.camera.SetGameCameraEnabled(p_flag: true);
				base.app.model.game.camera.CaptureAsync(delegate(RenderTexture p_rt)
				{
					ClearStaticBackground(p_textureCleanup: true);
					staticBackgroundTexture = new RenderTexture(p_rt);
					Graphics.CopyTexture(p_rt, staticBackgroundTexture);
					base.app.view.ui.cameraBackground.texture = staticBackgroundTexture;
					base.app.view.ui.cameraBackground.gameObject.SetActive(value: true);
					base.app.model.game.camera.SetGameCameraEnabled(p_flag: false);
				});
			}
		}

		public void SetStaticBackground(RenderTexture rt)
		{
			if (!(rt == null) && !(base.app.model.game == null) && !base.app.view.ui.cameraBackground.gameObject.activeSelf)
			{
				ClearStaticBackground(p_textureCleanup: true);
				staticBackgroundTexture = new RenderTexture(rt.width, rt.height, rt.depth);
				Graphics.CopyTexture(rt, staticBackgroundTexture);
				base.app.view.ui.cameraBackground.texture = staticBackgroundTexture;
				base.app.view.ui.cameraBackground.gameObject.SetActive(value: true);
				base.app.model.game.camera.SetGameCameraEnabled(p_flag: false);
			}
		}

		public void ClearStaticBackground(bool p_textureCleanup = false, bool p_isDestroy = false)
		{
			if (!base.validContext || base.app.model.game == null)
			{
				return;
			}
			if (m_SBActivity != null)
			{
				m_SBActivity.Stop();
				m_SBActivity = null;
			}
			if (!p_isDestroy)
			{
				if ((bool)base.app.model.game.camera)
				{
					base.app.model.game.camera.main.gameObject.SetActive(value: true);
					base.app.model.game.camera.SetGameCameraEnabled(p_flag: true);
				}
				base.app.view.ui.cameraBackground.gameObject.SetActive(value: false);
			}
			base.app.view.ui.cameraBackground.texture = null;
			if (p_textureCleanup && staticBackgroundTexture != null)
			{
				staticBackgroundTexture.Release();
				Object.DestroyImmediate(staticBackgroundTexture, allowDestroyingAssets: true);
				staticBackgroundTexture = null;
			}
		}

		private void OnDestroy()
		{
			ClearStaticBackground(p_textureCleanup: true, p_isDestroy: true);
		}

		public string BackButtonPressedEvent()
		{
			UIScreenView component = current.GetComponent<UIScreenView>();
			if (component != null)
			{
				return component.BackButtonPressedEvent();
			}
			return "ui.screen.return@click";
		}
	}
}
