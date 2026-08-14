using System;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class DroneScreenshotCapture : ScreenshotCapture
	{
		private static DroneScreenshotCapture m_instance;

		public static DroneScreenshotCapture instance
		{
			get
			{
				if ((bool)m_instance)
				{
					return m_instance;
				}
				string text = "drone-screenshot-capture";
				m_instance = LevelManager.GetRootComponent<DroneScreenshotCapture>(text);
				if ((bool)m_instance)
				{
					return m_instance;
				}
				GameObject gameObject = Resources.Load<GameObject>(text);
				if (!gameObject)
				{
					return null;
				}
				gameObject = UnityEngine.Object.Instantiate(gameObject);
				gameObject.transform.SetAsFirstSibling();
				gameObject.name = text;
				return m_instance = gameObject.GetComponent<DroneScreenshotCapture>();
			}
		}

		public DroneScreenshotData GetScreenshotData(Transform p_target)
		{
			Drone component = p_target.GetComponent<Drone>();
			DroneScreenshotData component2;
			if ((bool)component)
			{
				component2 = component.GetComponent<DroneScreenshotData>();
				if (!component2)
				{
					component2 = component.body.frame.GetComponent<DroneScreenshotData>();
				}
			}
			else
			{
				component2 = p_target.GetComponent<DroneScreenshotData>();
			}
			return component2;
		}

		public Texture Capture(int p_width, int p_height, Transform p_target, DroneScreenshotData p_data, bool p_smooth, bool p_preview, bool p_mipmap = true)
		{
			if (!p_target)
			{
				return Texture2D.blackTexture;
			}
			Drone component = p_target.GetComponent<Drone>();
			bool trailsEnabled = false;
			bool shadowsOnly = false;
			Vector3 localPosition = Vector3.zero;
			if ((bool)component)
			{
				shadowsOnly = component.renderer.shadowsOnly;
				trailsEnabled = component.renderer.GetTrailsEnabled();
				component.renderer.SetTrailsEnabled(p_flag: false);
				component.renderer.shadowsOnly = false;
				localPosition = component.body.frame.transform.localPosition;
				component.body.frame.transform.localPosition = Vector3.zero;
			}
			DroneScreenshotData p_data2 = (p_data ? p_data : GetScreenshotData(p_target));
			Texture texture = null;
			try
			{
				texture = Capture(p_width, p_height, p_target, (ScreenshotData)p_data2, p_smooth, p_preview, p_mipmap);
			}
			catch (ArgumentException ex)
			{
				if (!ex.Message.Contains("RenderTextureDesc"))
				{
					throw;
				}
				try
				{
					texture = Capture(p_width / 2, p_height / 2, p_target, (ScreenshotData)p_data2, p_smooth, p_preview, p_mipmap);
				}
				catch (ArgumentException)
				{
					if (!ex.Message.Contains("RenderTextureDesc"))
					{
						throw;
					}
				}
			}
			if (texture == null)
			{
				texture = new Texture2D(1, 1, TextureFormat.ARGB32, p_mipmap);
			}
			if ((bool)component)
			{
				component.renderer.SetTrailsEnabled(trailsEnabled);
				component.renderer.shadowsOnly = shadowsOnly;
				component.body.frame.transform.localPosition = localPosition;
			}
			return texture;
		}

		public Texture2D Capture(int p_width, int p_height, Transform p_target, DroneScreenshotData p_data, bool p_smooth, bool p_mipmap = true)
		{
			return (Texture2D)Capture(p_width, p_height, p_target, p_data, p_smooth, p_preview: false, p_mipmap);
		}

		public Texture2D Capture(int p_width, int p_height, Transform p_target, bool p_smooth, bool p_mipmap = true)
		{
			return Capture(p_width, p_height, p_target, null, p_smooth, p_mipmap);
		}
	}
}
