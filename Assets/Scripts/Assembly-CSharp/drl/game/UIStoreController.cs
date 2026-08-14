using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIStoreController : Controller<DRLApp>
	{
		public List<DRLStoreProductData> productsList;

		public int pageLength = 10;

		private int m_pagesTotalCount;

		private int m_currentPage;

		private bool m_lock_ui;

		private bool m_lock_refresh;

		protected WebAsyncRequest m_web_loader;

		private MonoActivity m_refresh_timer;

		private MonoActivity m_search_timer;

		private bool m_showing;

		private Activity m_purchaseListener;

		public UIStoreCategoryFilterType filterCategory;

		public string searchQuery;

		public string productId = "";

		public UIStoreView view => AssertLocal<UIStoreView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "ui.screen@close":
			{
				if (p_data.Length == 0)
				{
					break;
				}
				UIScreen uIScreen2 = p_data[0] as UIScreen;
				if (!(uIScreen2 == null) && !(uIScreen2.name != view.screen.name))
				{
					base.app.view.audio.StopUILoadingLoop();
					CancelWebLoad();
					if (m_purchaseListener != null)
					{
						m_purchaseListener.Stop();
						m_purchaseListener = null;
					}
				}
				break;
			}
			case "ui.screen@switch":
				if (p_data.Length != 0)
				{
					UIScreen uIScreen = p_data[0] as UIScreen;
					if (!(uIScreen == null))
					{
						_ = uIScreen.name != view.screen.name;
					}
				}
				break;
			}
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
			{
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				if (m_lock_refresh)
				{
					m_lock_refresh = false;
					break;
				}
				view.InitializeSteppers();
				view.listFade.FadeOut(0.001f);
				ResetAllFilters();
				bool flag = base.app.arguments.game.type == GameFlag.MapEditor;
				view.rightNavigation.gameObject.SetActive(flag);
				UIStoreCategoryFilterType category_filter = ((!flag) ? UIStoreCategoryFilterType.Attachments : UIStoreCategoryFilterType.AllCategories);
				PlatformService platform = base.app.model.service.platform;
				Debug.Log("StoreController> Trying to refresh platform flags..");
				platform.RefreshFlags(delegate
				{
					Debug.Log("StoreController> Platform flags refreshed! Category: " + category_filter);
					Show(category_filter);
				});
				if (DRLApp.offline)
				{
					view.menuContainer.SetActive(value: false);
				}
				break;
			}
			case "store.item.preview@click":
			{
				UIElementView uIElementView = p_target as UIElementView;
				UIStoreProductItemView product = uIElementView.GetComponentInParent<UIStoreProductItemView>();
				if (!(uIElementView == null) && !(product == null))
				{
					StorageModel storage = base.app.model.storage;
					_ = base.app.model.storage.state.player.garage;
					storage.PreloadDroneBundleData(null, null, p_ingame: false, delegate
					{
						base.enabled = true;
						base.app.view.ui.screens.Open<UIProductPreviewView>("store-preview-screen").ProductScreenInit(product);
					});
				}
				break;
			}
			case "ui.screen.return@click":
				CancelWebLoad();
				base.app.view.ui.screens.Return();
				if (m_purchaseListener != null)
				{
					m_purchaseListener.Stop();
					m_purchaseListener = null;
				}
				break;
			case "store.page@select":
				if (!m_lock_ui)
				{
					int num = (m_currentPage = (int)p_data[0]);
					Debug.Log("UIStoreController> Page Select [" + num + "]");
					UpdatePage(m_currentPage, view.pageLength, view.categoryFilter, view.sortMode, view.searchQuery);
				}
				break;
			case "store.page-next@click":
				if (!m_lock_ui && view.pageField.index + 1 != view.pageField.listField.Count)
				{
					view.pageField.index = view.pageField.index + 1;
					m_currentPage = view.pageField.index;
					Debug.Log("UIStoreController> Page Next");
					UpdatePage(m_currentPage, view.pageLength, view.categoryFilter, view.sortMode, view.searchQuery);
				}
				break;
			case "store.page-previous@click":
				if (!m_lock_ui && view.pageField.index != 0)
				{
					view.pageField.index = view.pageField.index - 1;
					m_currentPage = view.pageField.index;
					Debug.Log("UIStoreController> Page Previous");
					UpdatePage(m_currentPage, view.pageLength, view.categoryFilter, view.sortMode, view.searchQuery);
				}
				break;
			case "store.form.event":
				OnFormNotification(p_target, p_is_change: false, p_event);
				break;
			case "store.form.event@change":
				OnFormNotification(p_target, p_is_change: true, p_event);
				break;
			case "store.form.event@end-edit":
				OnFormNotification(p_target, p_is_change: false, p_event);
				break;
			case "store.item.buy@click":
			{
				UIStoreProductItemView componentInParent = (p_target as UIElementView).GetComponentInParent<UIStoreProductItemView>();
				DRLStoreProductData productData = (componentInParent ? componentInParent.data : null);
				if (productData == null)
				{
					break;
				}
				base.app.model.service.BuyProduct(productData, delegate(bool p_success, string p_error)
				{
					if (!p_success)
					{
						base.app.view.ui.dialog.Open(DialogType.Error, "PURCHASE NOT COMPLETE", p_error.ToUpper(), new string[1] { "OK" });
					}
					else
					{
						OnItemBuyComplete(productData);
					}
				});
				break;
			}
			}
		}

		private void LoadGarage()
		{
		}

		protected void OnItemBuyComplete(DRLStoreProductData p_product)
		{
			string text = ((p_product.items.Length <= 1) ? "ITEM".ToUpper() : "ITEMS".ToUpper());
			base.app.view.ui.dialog.Open(DialogType.Info, "PURCHASE SUCCESSFUL", "THANK YOU FOR YOUR PURCHASE. \nYOU CAN VIEW YOUR " + text + " IN THE GARAGE.", new string[2] { "GARAGE", "CLOSE" }, null, "", delegate(string p_id, int p_option)
			{
				if (p_option == 1)
				{
					DroneRigData defaultStoreRig = base.app.model.storage.state.player.garage.GetOriginalByGUID("DRD-fc5bf84d13e5bac67957921c");
					StorageModel storage = base.app.model.storage;
					List<DRLAsset> assets = storage.library.FindByGUID<DRLAsset>(new List<string>(p_product.items));
					storage.state.player.profile.RegisterInventoryGUIDs(p_product.items);
					storage.PreloadDroneBundleData(null, null, p_ingame: false, delegate
					{
						OpenGarageEdit(defaultStoreRig, assets);
					});
				}
			});
		}

		public void Show(UIStoreCategoryFilterType p_categoryFilter)
		{
			view.Clear();
			view.categoryFilter = p_categoryFilter;
			m_showing = true;
			Refresh(0.2f);
			UINavigation.Focus(view.sortStepperNav);
		}

		public void Hide()
		{
			m_showing = false;
			FadeComponent component = base.gameObject.GetComponent<FadeComponent>();
			if ((bool)component)
			{
				component.FadeOut();
			}
		}

		protected void OnFormNotification(Object p_target, bool p_is_change, string p_event)
		{
			if (m_lock_ui)
			{
				return;
			}
			bool flag = p_is_change;
			bool flag2 = p_event.Contains("@end-edit");
			switch (p_target.name)
			{
			case "item-categories-sort":
				if (flag)
				{
					DRLStepperView dRLStepperView2 = p_target as DRLStepperView;
					view.categoryFilter = (UIStoreCategoryFilterType)dRLStepperView2.index;
					view.categoryImages.frame = view.categoriesStepper.index;
					Refresh(0.6f);
				}
				break;
			case "item-filter-sort":
				if (flag)
				{
					DRLStepperView dRLStepperView = p_target as DRLStepperView;
					view.sortMode = (UIStoreSortType)dRLStepperView.index;
					Refresh(0.6f);
				}
				break;
			case "product-search-input":
				if (flag2)
				{
					DRLInputFieldView dRLInputFieldView = p_target as DRLInputFieldView;
					view.searchQuery = dRLInputFieldView.field.text.ToUpper();
					Refresh(0.6f);
				}
				break;
			case "item-filter-selector":
				if (flag)
				{
					Refresh(0.6f);
				}
				break;
			}
			Debug.Log($"<color=blue>Search Filter Type set to: {view.categoryFilter}</color>");
		}

		protected void ResetAllFilters()
		{
			searchQuery = "";
			view.searchQuery = "";
			productId = "";
			filterCategory = UIStoreCategoryFilterType.AllCategories;
		}

		protected void CancelWebLoad()
		{
			if (m_web_loader != null)
			{
				m_web_loader.Cancel();
			}
		}

		public void Refresh(float p_delay)
		{
			if (!m_lock_refresh)
			{
				if (m_refresh_timer != null)
				{
					m_refresh_timer.Stop();
				}
				m_refresh_timer = RunOnce(delegate
				{
					UpdatePage(0, view.pageLength, view.categoryFilter, view.sortMode, view.searchQuery);
				}, p_delay);
			}
		}

		public void UpdatePage(int p_page, int p_total, UIStoreCategoryFilterType p_category_filter, UIStoreSortType p_sort_type, string p_search)
		{
			base.app.view.audio.PlayUILoadingLoop();
			view.statusField.fade.FadeIn();
			view.statusField.SetLoading(0f);
			DRLStoreData dRLStoreData = new DRLStoreData();
			dRLStoreData.page = p_page + 1;
			dRLStoreData.limit = p_total;
			if (!string.IsNullOrEmpty(p_search))
			{
				dRLStoreData.search = p_search;
			}
			base.app.model.service.GetStoreProducts(dRLStoreData, delegate(DRLStoreResult p_result)
			{
				base.app.view.audio.StopUILoadingLoop();
				view.statusField.fade.FadeOut();
				ApplyPageData(p_page, p_total, p_result);
			});
			CancelWebLoad();
			view.Clear();
			if (view.categoryFilter == UIStoreCategoryFilterType.Motors)
			{
				_ = base.app.model.service;
				Debug.Log("UIStoreController> UpdatePage - was called - and requested Motors");
			}
			else
			{
				_ = view.categoryFilter;
				_ = 1;
			}
		}

		protected void ApplyPageData(int p_page, int p_total, DRLStoreResult p_result)
		{
			bool flag = (bool)this && (bool)base.app && (bool)base.app.view && (bool)view && (bool)base.gameObject;
			if (!flag)
			{
				return;
			}
			if (flag && (bool)base.app.view.audio)
			{
				base.app.view.audio.StopUILoadingLoop();
			}
			if (m_web_loader != null && (m_web_loader.status == AsyncRequestStatus.Created || m_web_loader.status == AsyncRequestStatus.Cancelled))
			{
				return;
			}
			if (p_result == null)
			{
				if ((bool)base.app.view && (bool)base.app.view.audio)
				{
					base.app.view.audio.PlayUIGenericError();
				}
				view.SetFeedback(UIStoreFeedbackType.OperationFailure);
				Debug.LogWarning("UIStoreController> UpdatePage - Failed!");
				return;
			}
			if ((bool)base.app.view && (bool)base.app.view.audio)
			{
				base.app.view.audio.PlayUILoadingSuccess();
			}
			productsList = new List<DRLStoreProductData>(p_result.data);
			m_pagesTotalCount = p_result.pagging.pageTotal;
			if (productsList.Count > 0)
			{
				view.UpdateList(productsList, p_page, p_total, m_pagesTotalCount);
				return;
			}
			view.SetFeedback(UIStoreFeedbackType.NoProducts);
			view.pageField.Set(0);
			view.ResetNavigation();
		}

		protected void OpenGarageEdit(DroneRigData p_data, UIStoreProductItemView p_purchasedProduct, bool p_templateSelector = false)
		{
			OpenGarageEdit(p_data, p_purchasedProduct.productParts, p_templateSelector);
		}

		protected void OpenGarageEdit(DroneRigData p_data, IList<DRLAsset> p_parts, bool p_templateSelector = false)
		{
			if (p_data == null)
			{
				Debug.LogWarning("UIStoreController> Failed to Open Edit");
			}
			else if (!p_data.isLocked)
			{
				UIGarageRigEditView uIGarageRigEditView = base.app.view.ui.screens.Open<UIGarageRigEditView>("garage-rig-edit-screen");
				uIGarageRigEditView.data = p_data;
				uIGarageRigEditView.data.isPublic = false;
				uIGarageRigEditView.currentProduct = p_parts[0];
				if (p_templateSelector)
				{
					uIGarageRigEditView.openedFromRigTemplateSelector = true;
				}
				else
				{
					uIGarageRigEditView.openedFromRigSelection = true;
				}
				uIGarageRigEditView.isOpenedFromStore = true;
			}
		}
	}
}
