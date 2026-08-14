using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	[Obsolete("Not used any more", true)]
	public class UIThreadHeader : MonoBehaviour
	{
		[SerializeField]
		private Text m_text;

		[SerializeField]
		private Button m_button;

		[SerializeField]
		private RawImage m_image;

		[SerializeField]
		private Image m_color;

		[SerializeField]
		private Image m_selectedOutline;

		public UINavigation ui_nav;

		public GameObject notificationBadge;

		public Text noOfUnread;

		private Color32 mTealColor = new Color32(80, 227, 194, byte.MaxValue);

		internal static Dictionary<string, Texture> m_cache;

		public string ThreadName { get; private set; }

		public string ThreadId { get; private set; }

		public bool Pending { get; private set; }

		internal static Dictionary<string, Texture> cache => Reflection<object>.Assert(ref m_cache);

		public void Setup(string threadName, string threadId, string imageURL, Color color, UnityAction callback)
		{
			ThreadId = threadId;
			ThreadName = threadName;
			m_text.text = threadName;
			base.gameObject.name = threadId;
			LoadPhoto(imageURL);
			m_button.onClick.AddListener(callback);
			m_color.color = color;
			Pending = true;
		}

		public void Highlight()
		{
			Pending = false;
			m_text.color = Color.white;
			noOfUnread.text = "";
			notificationBadge.SetActive(value: false);
			m_selectedOutline.gameObject.SetActive(value: true);
		}

		public void ClearHighlight()
		{
			m_text.color = (Pending ? ((Color)mTealColor) : Color.grey);
			m_selectedOutline.gameObject.SetActive(value: false);
		}

		public void MarkAsUnread(int p_noOfUnread)
		{
			Pending = true;
			if (p_noOfUnread > 0)
			{
				notificationBadge.SetActive(value: true);
				noOfUnread.text = p_noOfUnread.ToString();
			}
			m_text.color = mTealColor;
		}

		public void MarkNewMessage()
		{
			Pending = true;
			if (noOfUnread.text.Any(char.IsDigit))
			{
				noOfUnread.text = Regex.Replace(noOfUnread.text, "\\d+", (Match m) => (int.Parse(m.Value) + 1).ToString(new string('0', m.Value.Length)));
			}
			else
			{
				noOfUnread.text += "1";
			}
			notificationBadge.SetActive(value: true);
			m_text.color = mTealColor;
		}

		public void MoveUp()
		{
			base.transform.SetAsFirstSibling();
		}

		public virtual void LoadPhoto(string p_url)
		{
			if (cache.ContainsKey(p_url))
			{
				m_image.texture = cache[p_url];
				return;
			}
			Texture2D tpx = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false);
			tpx.SetPixel(0, 0, Colorf.transparent);
			tpx.Apply();
			Dictionary<string, Texture> dictionary = cache;
			Texture value = (m_image.texture = tpx);
			dictionary[p_url] = value;
			Web.Get(p_url, delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
			{
				if (!(p_progress < 1f) && (bool)p_result)
				{
					tpx.Resize(p_result.width, p_result.width, p_result.format, hasMipMap: false);
					tpx.LoadRawTextureData(p_result.GetRawTextureData());
					tpx.Apply();
					m_image.texture = tpx;
				}
			});
		}
	}
}
