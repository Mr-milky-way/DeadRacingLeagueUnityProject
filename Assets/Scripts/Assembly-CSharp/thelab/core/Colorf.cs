using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine;

namespace thelab.core
{
	public class Colorf
	{
		private const float InvByte = 0.003921569f;

		public static Color transparent => new Color(1f, 1f, 1f, 0f);

		public static Color red25 => new Color(0.25f, 0f, 0f, 1f);

		public static Color red50 => new Color(0.5f, 0f, 0f, 1f);

		public static Color red75 => new Color(0.75f, 0f, 0f, 1f);

		public static Color green25 => new Color(0f, 0.25f, 0f, 1f);

		public static Color green50 => new Color(0f, 0.5f, 0f, 1f);

		public static Color green75 => new Color(0f, 0.75f, 0f, 1f);

		public static Color blue25 => new Color(0f, 0f, 0.25f, 1f);

		public static Color blue50 => new Color(0f, 0f, 0.5f, 1f);

		public static Color blue75 => new Color(0f, 0f, 0.75f, 1f);

		public static Color yellow25 => new Color(0.25f, 0.25f, 0f, 1f);

		public static Color yellow50 => new Color(0.5f, 0.5f, 0f, 1f);

		public static Color yellow75 => new Color(0.75f, 0.75f, 0f, 1f);

		public static Color unityFocusBlue => RGBToColor(3364825u);

		public static Color Color32ToColor(Color32 c)
		{
			return ARGBToColor(c.a, c.r, c.g, c.b);
		}

		public static Color32 ColorToColor32(Color c)
		{
			return new Color32(R(c), G(c), B(c), A(c));
		}

		public static Color ARGBToColor(byte a, byte r, byte g, byte b)
		{
			float a2 = (float)(int)a * 0.003921569f;
			float r2 = (float)(int)r * 0.003921569f;
			float g2 = (float)(int)g * 0.003921569f;
			float b2 = (float)(int)b * 0.003921569f;
			return new Color(r2, g2, b2, a2);
		}

		public static Color ARGBToColor(uint v)
		{
			return ARGBToColor((byte)((v >> 24) & 0xFF), (byte)((v >> 16) & 0xFF), (byte)((v >> 8) & 0xFF), (byte)(v & 0xFF));
		}

		public static Color RGBAToColor(uint v)
		{
			return ARGBToColor((byte)(v & 0xFF), (byte)((v >> 24) & 0xFF), (byte)((v >> 16) & 0xFF), (byte)((v >> 8) & 0xFF));
		}

		public static Color RGBToColor(uint v, float a = 1f)
		{
			return ARGBToColor((byte)(Mathf.Clamp01(a) * 255f), (byte)((v >> 16) & 0xFF), (byte)((v >> 8) & 0xFF), (byte)(v & 0xFF));
		}

		public static uint ColorToARGB(Color v)
		{
			return (uint)((A(v) << 24) | (R(v) << 16) | (G(v) << 8) | B(v));
		}

		public static uint ColorToRGBA(Color v)
		{
			return (uint)((R(v) << 24) | (G(v) << 16) | (B(v) << 8) | A(v));
		}

		public static uint ColorToRGB(Color v)
		{
			return (uint)((R(v) << 16) | (G(v) << 8) | B(v));
		}

		public static byte R(Color c)
		{
			return (byte)(c.r * 255f);
		}

		public static byte G(Color c)
		{
			return (byte)(c.g * 255f);
		}

		public static byte B(Color c)
		{
			return (byte)(c.b * 255f);
		}

		public static byte A(Color c)
		{
			return (byte)(c.a * 255f);
		}

		public static byte[] RGB(Color c)
		{
			return new byte[3]
			{
				R(c),
				G(c),
				B(c)
			};
		}

		public static byte[] RGBA(Color c)
		{
			return new byte[4]
			{
				R(c),
				G(c),
				B(c),
				A(c)
			};
		}

		public static byte[] ARGB(Color c)
		{
			return new byte[4]
			{
				A(c),
				R(c),
				G(c),
				B(c)
			};
		}

		public static Color ParseRGB(string p_v, NumberStyles p_number_style, Color p_default)
		{
			uint result = 0u;
			if (!uint.TryParse(p_v, p_number_style, null, out result))
			{
				return p_default;
			}
			return RGBToColor(result);
		}

		public static Color ParseRGB(string p_v, NumberStyles p_number_style)
		{
			return ParseRGB(p_v, p_number_style, transparent);
		}

		public static Color ParseRGB(string p_v, Color p_default)
		{
			return ParseRGB(p_v, NumberStyles.HexNumber, p_default);
		}

		public static Color ParseRGB(string p_v)
		{
			return ParseRGB(p_v, NumberStyles.HexNumber, transparent);
		}

		public static Color ParseARGB(string p_v, NumberStyles p_number_style, Color p_default)
		{
			uint result = 0u;
			if (!uint.TryParse(p_v, p_number_style, null, out result))
			{
				return p_default;
			}
			return ARGBToColor(result);
		}

		public static Color ParseARGB(string p_v, NumberStyles p_number_style)
		{
			return ParseARGB(p_v, p_number_style, transparent);
		}

		public static Color ParseARGB(string p_v, Color p_default)
		{
			return ParseARGB(p_v, NumberStyles.HexNumber, p_default);
		}

		public static Color ParseARGB(string p_v)
		{
			return ParseARGB(p_v, NumberStyles.HexNumber, transparent);
		}

		public static string ToRGBHex(Color p_color, string p_prefix = "")
		{
			byte b = R(p_color);
			byte b2 = G(p_color);
			byte b3 = B(p_color);
			return p_prefix + b.ToString("x2") + b3.ToString("x2") + b2.ToString("x2");
		}

		public static string ToARGBHex(Color p_color, string p_prefix = "")
		{
			byte b = A(p_color);
			byte b2 = R(p_color);
			byte b3 = G(p_color);
			byte b4 = B(p_color);
			return p_prefix + b.ToString("x2") + b2.ToString("x2") + b4.ToString("x2") + b3.ToString("x2");
		}

		public static string ToRGBAHex(Color p_color, string p_prefix = "")
		{
			byte b = A(p_color);
			byte b2 = R(p_color);
			byte b3 = G(p_color);
			byte b4 = B(p_color);
			return p_prefix + b2.ToString("x2") + b3.ToString("x2") + b4.ToString("x2") + b.ToString("x2");
		}

		public static Color Gradient(float r, params Color[] p_colors)
		{
			float num = p_colors.Length;
			float num2 = r * (num - 1f);
			int num3 = Mathf.FloorToInt(num2);
			int num4 = Mathf.CeilToInt(num2);
			if (num4 >= p_colors.Length)
			{
				num4 = p_colors.Length - 1;
			}
			float t = num2 - (float)num3;
			return Color.Lerp(p_colors[num3], p_colors[num4], t);
		}

		public static Color Gradient(float r, params uint[] p_colors)
		{
			float num = p_colors.Length;
			float num2 = r * (num - 1f);
			int num3 = Mathf.FloorToInt(num2);
			int num4 = Mathf.CeilToInt(num2);
			if (num4 >= p_colors.Length)
			{
				num4 = p_colors.Length - 1;
			}
			float t = num2 - (float)num3;
			Color a = ARGBToColor(p_colors[num3]);
			Color b = ARGBToColor(p_colors[num4]);
			return Color.Lerp(a, b, t);
		}

		public static uint AddARGB(uint a, uint b)
		{
			int num = (int)Mathf.Clamp(((a >> 24) & 0xFF) + ((b >> 24) & 0xFF), 0f, 255f);
			int num2 = (int)Mathf.Clamp(((a >> 16) & 0xFF) + ((b >> 16) & 0xFF), 0f, 255f);
			int num3 = (int)Mathf.Clamp(((a >> 8) & 0xFF) + ((b >> 8) & 0xFF), 0f, 255f);
			int num4 = (int)Mathf.Clamp((a & 0xFF) + (b & 0xFF), 0f, 255f);
			return (uint)((num << 24) | (num2 << 16) | (num3 << 8) | num4);
		}

		public static uint AddRGBA(uint a, uint b)
		{
			int num = (int)Mathf.Clamp(((a >> 24) & 0xFF) + ((b >> 24) & 0xFF), 0f, 255f);
			int num2 = (int)Mathf.Clamp(((a >> 16) & 0xFF) + ((b >> 16) & 0xFF), 0f, 255f);
			int num3 = (int)Mathf.Clamp(((a >> 8) & 0xFF) + ((b >> 8) & 0xFF), 0f, 255f);
			int num4 = (int)Mathf.Clamp((a & 0xFF) + (b & 0xFF), 0f, 255f);
			return (uint)((num << 24) | (num2 << 16) | (num3 << 8) | num4);
		}

		public static uint AddRGB(uint a, uint b)
		{
			int num = (int)Mathf.Clamp(((a >> 16) & 0xFF) + ((b >> 16) & 0xFF), 0f, 255f);
			int num2 = (int)Mathf.Clamp(((a >> 8) & 0xFF) + ((b >> 8) & 0xFF), 0f, 255f);
			int num3 = (int)Mathf.Clamp((a & 0xFF) + (b & 0xFF), 0f, 255f);
			return (uint)((num << 16) | (num2 << 8) | num3);
		}

		public static Color[] BilinearScale(Color[] p_image, int p_width, int p_height, int p_out_width, int p_out_height)
		{
			Color[] array = new Color[p_out_width * p_out_height];
			float num = p_out_width - 1;
			float num2 = p_out_height - 1;
			num = ((num <= 0f) ? 0f : (1f / num));
			num2 = ((num2 <= 0f) ? 0f : (1f / num2));
			for (int i = 0; i < p_out_height; i++)
			{
				float p_uvy = (float)i * num2;
				for (int j = 0; j < p_out_width; j++)
				{
					float p_uvx = (float)j * num;
					int num3 = j + i * p_out_width;
					array[num3] = GetPixelBilinear(p_image, p_width, p_height, p_uvx, p_uvy);
				}
			}
			return array;
		}

		public static void BilinearScale(Color[] p_image, int p_width, int p_height, int p_out_width, int p_out_height, Action<Color[]> p_callback)
		{
			new Thread((ThreadStart)delegate
			{
				Color[] res = BilinearScale(p_image, p_width, p_height, p_out_width, p_out_height);
				Activity.RunOnce(delegate
				{
					if (p_callback != null)
					{
						p_callback(res);
					}
				}, 1f / 60f);
			}).Start();
		}

		public static Texture2D BilinearScale(Texture2D p_tex, int p_width, int p_height, bool p_mipmap = false)
		{
			Color[] pixels = BilinearScale(p_tex.GetPixels(), p_tex.width, p_tex.height, p_width, p_height);
			Texture2D texture2D = new Texture2D(p_width, p_height, TextureFormat.ARGB32, p_mipmap);
			texture2D.SetPixels(pixels);
			return texture2D;
		}

		public static Texture2D BilinearScale(Texture2D p_tex, int p_width, int p_height, Action<Texture2D> p_callback, bool p_mipmap = false)
		{
			Color[] pixels = p_tex.GetPixels();
			Texture2D output = new Texture2D(p_width, p_height, TextureFormat.ARGB32, p_mipmap);
			BilinearScale(pixels, p_tex.width, p_tex.height, p_width, p_height, delegate(Color[] p_pixels)
			{
				output.SetPixels(p_pixels);
				output.Apply(updateMipmaps: true);
				if (p_callback != null)
				{
					p_callback(output);
				}
			});
			return output;
		}

		public static Color GetPixelBilinear(Color[] p_image, int p_width, int p_height, float p_uvx, float p_uvy)
		{
			float num = Mathf.Clamp01(p_uvx);
			float num2 = Mathf.Clamp01(p_uvy);
			float num3 = num * (float)p_width;
			float num4 = num2 * (float)p_height;
			float t = num3 - Mathf.Floor(num3);
			float t2 = num4 - Mathf.Floor(num4);
			int num5 = Mathf.FloorToInt(num3);
			int num6 = Mathf.FloorToInt(num4);
			Color pixel = GetPixel(p_image, p_width, p_height, num5, num6);
			Color pixel2 = GetPixel(p_image, p_width, p_height, num5 + 1, num6);
			Color pixel3 = GetPixel(p_image, p_width, p_height, num5, num6 + 1);
			Color pixel4 = GetPixel(p_image, p_width, p_height, num5 + 1, num6 + 1);
			Color a = Color.Lerp(pixel, pixel2, t);
			Color b = Color.Lerp(pixel3, pixel4, t);
			return Color.Lerp(a, b, t2);
		}

		public static Color GetPixelNearest(Color[] p_image, int p_width, int p_height, float p_uvx, float p_uvy)
		{
			float num = Mathf.Clamp01(p_uvx);
			float num2 = Mathf.Clamp01(p_uvy);
			float f = num * (float)(p_width - 1);
			float f2 = num2 * (float)(p_height - 1);
			int p_x = Mathf.FloorToInt(f);
			int p_y = Mathf.FloorToInt(f2);
			return GetPixel(p_image, p_width, p_height, p_x, p_y);
		}

		public static Color GetPixel(Color[] p_image, int p_width, int p_height, int p_x, int p_y)
		{
			if (p_image == null)
			{
				return Color.clear;
			}
			if (p_image.Length == 0)
			{
				return Color.clear;
			}
			int num = Mathf.Clamp(p_x, 0, p_width - 1);
			int num2 = Mathf.Clamp(p_y, 0, p_height - 1);
			int value = num + num2 * p_width;
			value = Mathf.Clamp(value, 0, p_image.Length - 1);
			return p_image[value];
		}

		public static int GetColorIndex(Color p_color, IList<Color> p_list, float p_bias = 0.008f, bool p_clamp = true)
		{
			if (p_list.Count <= 0)
			{
				return -1;
			}
			int result = 0;
			float num = GetBias(p_color, p_list[0]).magnitude;
			for (int i = 1; i < p_list.Count; i++)
			{
				float magnitude = GetBias(p_color, p_list[i]).magnitude;
				if (magnitude < num)
				{
					result = i;
					num = magnitude;
				}
			}
			if (num > p_bias)
			{
				return -1;
			}
			return result;
		}

		public static Vector4 GetBias(Color a, Color b, bool p_clamp = true)
		{
			Vector4 result = new Vector4(0f, 0f, 0f, 0f);
			if (p_clamp)
			{
				a.r = Mathf.Clamp01(a.r);
				a.g = Mathf.Clamp01(a.g);
				a.b = Mathf.Clamp01(a.b);
				a.a = Mathf.Clamp01(a.a);
				b.r = Mathf.Clamp01(b.r);
				b.g = Mathf.Clamp01(b.g);
				b.b = Mathf.Clamp01(b.b);
				b.a = Mathf.Clamp01(b.a);
			}
			result.x = Mathf.Abs(a.r - b.r);
			result.y = Mathf.Abs(a.g - b.g);
			result.z = Mathf.Abs(a.b - b.b);
			result.w = Mathf.Abs(a.a - b.a);
			return result;
		}
	}
}
