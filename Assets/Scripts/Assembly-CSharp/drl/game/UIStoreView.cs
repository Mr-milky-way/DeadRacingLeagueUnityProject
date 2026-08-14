using System;
using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UIStoreView : UIScreenView
	{
		public ListComponent listField;

		[Header("Filters")]
		public DRLStepperView categoriesStepper;

		public DRLStepperView sortStepper;

		public DRLInputFieldView searchInput;

		public DRLStepperView itemStepper;

		[Header("Navigation")]
		public UINavigation categoriesStepperNav;

		public UINavigation sortStepperNav;

		public UINavigation searchInputNav;

		public UINavigation itemStepperNav;

		public FadeComponent feedbackFade;

		public FadeComponent listFade;

		public List<GameObject> feedbacks;

		public DRLPagePickerView pageField;

		public GameObject menuContainer;

		public ImageClip categoryImages;

		public UIStoreCategoryFilterType categoryFilter;

		public UIStoreSortType sortMode;

		public string searchQuery;

		public UIStatusView statusField;

		public int pageLength = 10;

		public List<DRLStoreProductData> products;

		public GameObject exitButton;

		private UINavigation m_exitButtonNav;

		private UINavigation exitButtonNav
		{
			get
			{
				if (m_exitButtonNav == null)
				{
					m_exitButtonNav = exitButton.GetComponent<UINavigation>();
				}
				return m_exitButtonNav;
			}
		}

		public void Clear()
		{
			listField.Clear();
			products = new List<DRLStoreProductData>();
		}

		public void InitializeSteppers()
		{
			UpdateBaseProductsStepper();
			ResetSortStepper();
			ClearSearch();
		}

		public void UpdateBaseProductsStepper()
		{
		}

		public void ResetSortStepper()
		{
			sortStepper.index = 0;
			sortStepper.Refresh();
		}

		public void ClearSearch()
		{
			searchInput.text = "";
		}

		public void UpdateList(List<DRLStoreProductData> p_products, int p_page, int p_page_length, int p_pages_count = -1, bool p_allow_search = false)
		{
			new List<DRLStoreProductData>((p_products == null) ? new List<DRLStoreProductData>() : p_products);
			List<DRLStoreProductData> collection = ((p_products == null) ? new List<DRLStoreProductData>() : p_products);
			collection = new List<DRLStoreProductData>(collection);
			if (p_allow_search)
			{
				collection.RemoveAll(delegate(DRLStoreProductData p_it)
				{
					string text = searchInput.field.text;
					text = text.Trim().ToLower();
					return !string.IsNullOrEmpty(text) && !p_it.name.ToLower().Contains(text);
				});
			}
			int num = ((p_page_length > 0) ? ((collection.Count - 1) / p_page_length) : 0) + 1;
			if (p_pages_count > 0)
			{
				num = p_pages_count;
			}
			int num2 = Mathf.Clamp(p_page, 0, num - 1);
			List<DRLStoreProductData> list = new List<DRLStoreProductData>();
			int num3 = ((collection.Count > p_page_length) ? Mathf.Max(0, num2 * p_page_length) : 0);
			for (int num4 = 0; num4 < p_page_length; num4++)
			{
				if (num3 >= collection.Count)
				{
					break;
				}
				DRLStoreProductData dRLStoreProductData = collection[num3];
				if (!(dRLStoreProductData.category != "skins"))
				{
					list.Add(dRLStoreProductData);
					num3++;
				}
			}
			Debug.Log("UIStoreView> UpdateList - total[" + collection.Count + "] page[" + num2 + "] total-pages[" + num + "] elements[" + list.Count + "]");
			List<DRLStoreProductData> list2 = new List<DRLStoreProductData>();
			List<DRLStoreProductData> list3 = new List<DRLStoreProductData>();
			if (products == null)
			{
				products = new List<DRLStoreProductData>();
			}
			for (int num5 = 0; num5 < list.Count; num5++)
			{
				if (!ContainsProduct(products, list[num5]))
				{
					list2.Add(list[num5]);
				}
			}
			for (int num6 = 0; num6 < products.Count; num6++)
			{
				if (!ContainsProduct(list, products[num6]))
				{
					list3.Add(products[num6]);
				}
			}
			Debug.Log("UIStoreView> UpdateList - add[" + list2.Count + "] remove[" + list3.Count + "]");
			for (int num7 = 0; num7 < list3.Count; num7++)
			{
				RemoveProduct(list3[num7]);
			}
			for (int num8 = 0; num8 < list2.Count; num8++)
			{
				if (products.Count < p_page_length)
				{
					AddProduct(list2[num8]);
				}
			}
			for (int num9 = 0; num9 < list.Count; num9++)
			{
				int productIndex = GetProductIndex(list[num9]);
				if (productIndex >= 0)
				{
					products[productIndex] = list[num9];
				}
			}
			UpdateNavigation();
			if (num > 1)
			{
				ShowPages();
			}
			else
			{
				HidePages();
			}
			pageField.Set(num);
			pageField.index = num2;
			UIStoreFeedbackType p_type = ((products.Count <= 0) ? UIStoreFeedbackType.NoProducts : UIStoreFeedbackType.None);
			SetFeedback(p_type, p_hide_list: true, 0.1f);
		}

		public void HidePages()
		{
			FadeComponent fade = pageField.fade;
			if (fade.alpha < 0f)
			{
				fade.alpha = 0f;
			}
			fade.FadeOut(0.01f);
		}

		public void ShowPages()
		{
			FadeComponent fade = pageField.fade;
			if (fade.alpha < 0f)
			{
				fade.alpha = 0f;
			}
			fade.FadeIn(0.3f);
		}

		public UIStoreProductItemView GetByProductId(string p_id)
		{
			for (int i = 0; i < listField.Count; i++)
			{
				UIStoreProductItemView uIStoreProductItemView = listField.Get<UIStoreProductItemView>(i);
				if (uIStoreProductItemView.data != null && uIStoreProductItemView.data.productId == p_id)
				{
					return uIStoreProductItemView;
				}
			}
			return null;
		}

		public int GetProductIndex(DRLStoreProductData p_product)
		{
			for (int i = 0; i < products.Count; i++)
			{
				if (products[i].productId == p_product.productId)
				{
					return i;
				}
			}
			return -1;
		}

		public void AddProduct(DRLStoreProductData p_data)
		{
			products.Add(p_data);
			_ = base.app.model.storage.locale;
			listField.Push<UIStoreProductItemView>().Set(p_data);
		}

		public void RemoveProduct(DRLStoreProductData p_data)
		{
			for (int i = 0; i < products.Count; i++)
			{
				if (products[i].productId == p_data.productId)
				{
					products.RemoveAt(i);
					break;
				}
			}
			for (int j = 0; j < listField.Count; j++)
			{
				UIStoreProductItemView uIStoreProductItemView = listField.Get<UIStoreProductItemView>(j);
				if ((bool)uIStoreProductItemView && uIStoreProductItemView.data.productId == p_data.productId)
				{
					listField.Remove(j);
					break;
				}
			}
		}

		public void UpdateProduct(DRLStoreProductData p_data)
		{
			if ((bool)GetByProductId(p_data.productId))
			{
				_ = base.app.model.storage.locale;
				listField.Push<UIStoreProductItemView>().Set(p_data);
			}
		}

		public bool ContainsProduct(List<DRLStoreProductData> p_list, DRLStoreProductData p_product)
		{
			if (p_product == null)
			{
				return false;
			}
			if (p_list == null)
			{
				return false;
			}
			if (p_list.Count <= 0)
			{
				return false;
			}
			for (int i = 0; i < p_list.Count; i++)
			{
				if (p_list[i].productId == p_product.productId)
				{
					return true;
				}
			}
			return false;
		}

		protected void UpdateNavigation()
		{
			ListComponent listComponent = listField;
			pageField.GetComponent<UINavigation>();
			for (int i = 0; i < listComponent.Count; i++)
			{
				UIStoreProductItemView uIStoreProductItemView = listComponent.Get<UIStoreProductItemView>(i);
				UIStoreProductItemView uIStoreProductItemView2 = listComponent.Get<UIStoreProductItemView>(i + 5);
				UIStoreProductItemView uIStoreProductItemView3 = listComponent.Get<UIStoreProductItemView>(i - 5);
				UIStoreProductItemView uIStoreProductItemView4 = ((i >= listComponent.Count - 1) ? listComponent.Get<UIStoreProductItemView>(i) : listComponent.Get<UIStoreProductItemView>(i + 1));
				if (uIStoreProductItemView4 != null)
				{
					if (i == 0)
					{
						uIStoreProductItemView.LinkRight(uIStoreProductItemView4, exitButtonNav);
					}
					else if (i < listComponent.Count - 1)
					{
						uIStoreProductItemView.LinkRight(uIStoreProductItemView4);
					}
					else
					{
						uIStoreProductItemView.LinkRight(uIStoreProductItemView4, null, lastCard: true);
					}
				}
				switch (i)
				{
				case 0:
					if (uIStoreProductItemView2 != null)
					{
						uIStoreProductItemView.LinkDown(uIStoreProductItemView2, searchInputNav);
					}
					break;
				case 1:
					if (uIStoreProductItemView2 != null)
					{
						uIStoreProductItemView.LinkDown(uIStoreProductItemView2, searchInputNav);
					}
					break;
				case 2:
					if (uIStoreProductItemView2 != null)
					{
						uIStoreProductItemView.LinkDown(uIStoreProductItemView2, searchInputNav);
					}
					break;
				case 3:
					if (uIStoreProductItemView2 != null)
					{
						uIStoreProductItemView.LinkDown(uIStoreProductItemView2, searchInputNav);
					}
					break;
				case 4:
					if (uIStoreProductItemView2 != null)
					{
						uIStoreProductItemView.LinkDown(uIStoreProductItemView2, searchInputNav);
					}
					break;
				default:
					if (uIStoreProductItemView3 != null)
					{
						uIStoreProductItemView.LinkDown(uIStoreProductItemView3);
					}
					break;
				}
			}
			_ = sortStepperNav;
		}

		public void ResetNavigation()
		{
			base.leftNavigation.right = sortStepperNav;
			base.rightNavigation.left = sortStepperNav;
			exitButtonNav.right = sortStepperNav;
		}

		public void SetFeedback(UIStoreFeedbackType p_type, bool p_hide_list, float p_delay)
		{
			float feedback_alpha = ((p_type == UIStoreFeedbackType.None) ? (-0.1f) : 1f);
			float content_alpha = ((p_type == UIStoreFeedbackType.None) ? 1f : (p_hide_list ? (-0.1f) : 1f));
			Action action = delegate
			{
				feedbackFade.Fade(feedback_alpha, 0.3f, 0.05f, Cubic.Out);
				listFade.Fade(content_alpha, 0.3f, 0f, Cubic.Out);
				if (p_type != UIStoreFeedbackType.None)
				{
					int num = (int)p_type;
					for (int i = 0; i < feedbacks.Count; i++)
					{
						feedbacks[i].SetActive(i == num);
					}
				}
			};
			if (p_delay <= 0f)
			{
				action();
			}
			else
			{
				RunOnce(p_delay, action);
			}
			switch (p_type)
			{
			case UIStoreFeedbackType.Loading:
				statusField.SetLoading(0f);
				break;
			case UIStoreFeedbackType.NoProducts:
				statusField.SetWarning("NO PRODUCTS FOUND");
				break;
			case UIStoreFeedbackType.OperationFailure:
				statusField.SetWarning("OPERATION FAILURE");
				break;
			}
		}

		public void SetFeedback(UIStoreFeedbackType p_type, bool p_hide_list)
		{
			SetFeedback(p_type, p_hide_list, 0f);
		}

		public void SetFeedback(UIStoreFeedbackType p_type)
		{
			SetFeedback(p_type, p_hide_list: true, 0f);
		}
	}
}
