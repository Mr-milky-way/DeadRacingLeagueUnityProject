using System;
using UnityEngine;

namespace drl.game
{
	public class LayoutGeometry
	{
		private static float[] surface_args = new float[4];

		protected static int LayoutDistributeBase(LayoutGeometryType p_type, float[] p_args, LayoutParams p_params, Vector3[] p_buffer)
		{
			LayoutParams layoutParams = p_params;
			Vector3 vector = Vector3.zero;
			float num = (float)Math.PI;
			float num2 = num * 2f;
			int num3 = 0;
			System.Random random = new System.Random(layoutParams.seed);
			float num4 = Mathf.Max(0.01f, layoutParams.span);
			LayoutSlice slices = layoutParams.slices;
			float num5 = 0f;
			float num6 = 0f;
			float num7 = 0f;
			float value = 0f;
			float f = 0f;
			float num8 = 0f;
			float num9 = 0f;
			float num10 = 0f;
			float num11 = 0f;
			float num12 = 0f;
			float num13 = 0f;
			float num14 = 0f;
			float num15 = 0f;
			float num16 = 0f;
			float num17 = 0f;
			float num18 = 0f;
			float num19 = 0f;
			float num20 = 0f;
			float num21 = 0f;
			float num22 = 0f;
			float num23 = 0f;
			float num24 = 0f;
			float num25 = 0f;
			float num26 = 0f;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			num10 = Mathf.Lerp(0f, 1f - slices.rangeX, Mathf.Clamp01(slices.x));
			num11 = num10 + slices.rangeX;
			num16 = Mathf.Lerp(0f, 1f - slices.rangeY, Mathf.Clamp01(slices.y));
			num17 = num16 + slices.rangeY;
			num22 = Mathf.Lerp(0f, 1f - slices.rangeZ, Mathf.Clamp01(slices.z));
			num23 = num22 + slices.rangeZ;
			switch (p_type)
			{
			case LayoutGeometryType.Grid:
				num14 = Mathf.Max(0.01f, p_args[0]);
				num20 = Mathf.Max(0.01f, p_args[1]);
				num26 = Mathf.Max(0.01f, p_args[2]);
				num13 = 1f / Mathf.Floor(num14 / num4);
				num19 = 1f / Mathf.Floor(num20 / num4);
				num25 = 1f / Mathf.Floor(num26 / num4);
				num12 = num13;
				num18 = num19;
				num24 = num25;
				break;
			case LayoutGeometryType.Sphere:
				num8 = Mathf.Max(0.01f, p_args[0]);
				num19 = 1f / Mathf.Floor(num * num8 / num4);
				num18 = num19;
				num25 = 1f / Mathf.Floor(num8 / num4);
				num24 = num25;
				if (!layoutParams.fill)
				{
					num22 = 0f;
					num23 = 1f;
					num24 = 0f;
					num25 = 1f;
				}
				break;
			case LayoutGeometryType.Cone:
			{
				num8 = Mathf.Max(0.001f, p_args[0]);
				num5 = Mathf.Max(0.001f, p_args[1]);
				value = Mathf.Floor(num8 / num4);
				value = ((Mathf.Abs(value) <= 0f) ? 0f : (Mathf.Floor(Mathf.Clamp01(p_args[2]) * value) / value));
				float a = Mathf.Sqrt(num8 * num8 + num5 * num5);
				f = Mathf.Atan(num8 / num5);
				num25 = Mathf.Lerp(a, num5, Mathf.Clamp01(value));
				num25 = 1f / Mathf.Floor(num25 / num4);
				num24 = num25;
				break;
			}
			case LayoutGeometryType.Cylinder:
				num8 = Mathf.Max(0.001f, p_args[0]);
				num5 = Mathf.Max(0.001f, p_args[1]);
				num19 = 1f / Mathf.Floor(num5 / num4);
				num25 = 1f / Mathf.Floor(num8 / num4);
				num24 = num25;
				num18 = num19;
				if (!layoutParams.fill)
				{
					num22 = 0f;
					num23 = 1f;
					num24 = 0f;
					num25 = 1f;
				}
				break;
			}
			if (num25 > 0f)
			{
				for (float num27 = num22; num27 < num23 + num24; num27 += num25)
				{
					switch (p_type)
					{
					case LayoutGeometryType.Grid:
						flag5 = num27 <= num22;
						flag6 = num27 >= num23 + num24 - num25;
						if (num25 > 1f)
						{
							flag5 = (flag6 = false);
						}
						break;
					case LayoutGeometryType.Sphere:
						num7 = Mathf.Lerp(num8, 0f, num27);
						break;
					case LayoutGeometryType.Cone:
						num6 = Mathf.Lerp(0f, num5, Mathf.Clamp01(num27));
						num9 = Mathf.Abs(Mathf.Tan(f) * (num5 - num6));
						num9 = Mathf.Lerp(num9, num8, Mathf.Clamp01(value));
						num19 = Mathf.Floor(num9 / num4);
						num19 = ((Mathf.Abs(num19) <= 0.001f) ? 1f : (1f / num19));
						num18 = ((Mathf.Abs(num19) >= 1f) ? 0f : num19);
						if (!layoutParams.fill)
						{
							num16 = 0f;
							num17 = 1f;
							num18 = 0f;
							num19 = 1f;
						}
						break;
					case LayoutGeometryType.Cylinder:
						num7 = Mathf.Lerp(num8, 0f, num27);
						break;
					}
					if (!(num19 > 0f))
					{
						continue;
					}
					for (float num28 = num16; num28 < num17 + num18; num28 += num19)
					{
						switch (p_type)
						{
						case LayoutGeometryType.Grid:
							flag3 = num28 <= num16;
							flag4 = num28 >= num17 + num18 - num19;
							if (num19 > 1f)
							{
								flag3 = (flag4 = false);
							}
							break;
						case LayoutGeometryType.Sphere:
						{
							float f2 = Mathf.Lerp(0f, num, num28);
							num14 = Mathf.Sin(f2);
							num15 = Mathf.Cos(f2);
							num13 = (int)(num2 * num7 * num14 / num4);
							num13 = ((Mathf.Abs(num13) <= 0.001f) ? 1f : (1f / num13));
							break;
						}
						case LayoutGeometryType.Cone:
							num7 = Mathf.Lerp(num9, 0f, num28);
							num13 = (int)(num2 * num7 / num4);
							num13 = ((Mathf.Abs(num13) <= 0.001f) ? 1f : (1f / num13));
							break;
						case LayoutGeometryType.Cylinder:
							num6 = Mathf.Lerp(0f, num5, Mathf.Clamp01(num28));
							num13 = (int)(num2 * num7 / num4);
							num13 = ((Mathf.Abs(num13) <= 0.001f) ? 1f : (1f / num13));
							break;
						}
						if (!(num13 > 0f))
						{
							continue;
						}
						for (float num29 = num10; num29 < num11 + num12; num29 += num13)
						{
							if (p_buffer != null && num3 < p_buffer.Length)
							{
								vector = p_buffer[num3];
							}
							if (p_buffer != null)
							{
								switch (p_type)
								{
								case LayoutGeometryType.Grid:
								{
									flag = num29 <= num10;
									flag2 = num29 >= num11 + num12 - num13;
									if (num13 > 1f)
									{
										flag = (flag2 = false);
									}
									bool flag7 = layoutParams.fill;
									if (!layoutParams.fill)
									{
										if (flag || flag2)
										{
											flag7 = true;
										}
										if (flag3 || flag4)
										{
											flag7 = true;
										}
										if (flag5 || flag6)
										{
											flag7 = true;
										}
									}
									if (!flag7)
									{
										continue;
									}
									vector.x = Mathf.Lerp(0f, num14, num29);
									vector.y = Mathf.Lerp(0f, num20, num28);
									vector.z = Mathf.Lerp(0f, num26, num27);
									break;
								}
								case LayoutGeometryType.Sphere:
								{
									float f5 = Mathf.Lerp(0f, num2, num29);
									num20 = Mathf.Sin(f5);
									num21 = Mathf.Cos(f5);
									vector.x = num14 * num21 * num7;
									vector.y = num15 * num7;
									vector.z = num14 * num20 * num7;
									break;
								}
								case LayoutGeometryType.Cone:
								{
									float f4 = Mathf.Lerp(0f, num2, num29);
									num14 = Mathf.Sin(f4);
									num15 = Mathf.Cos(f4);
									vector.x = num14 * num7;
									vector.y = num6;
									vector.z = num15 * num7;
									break;
								}
								case LayoutGeometryType.Cylinder:
								{
									float f3 = Mathf.Lerp(0f, num2, num29);
									num14 = Mathf.Sin(f3);
									num15 = Mathf.Cos(f3);
									vector.x = num14 * num7;
									vector.y = num6;
									vector.z = num15 * num7;
									break;
								}
								}
							}
							if (p_buffer != null && num3 < p_buffer.Length)
							{
								if (layoutParams.random.sqrMagnitude > 0f)
								{
									vector.x += layoutParams.random.x * Mathf.Lerp(0f - num4, num4, (float)random.NextDouble()) * 0.25f;
									vector.y += layoutParams.random.y * Mathf.Lerp(0f - num4, num4, (float)random.NextDouble()) * 0.25f;
									vector.z += layoutParams.random.z * Mathf.Lerp(0f - num4, num4, (float)random.NextDouble()) * 0.25f;
								}
								p_buffer[num3] = vector;
							}
							num3++;
						}
					}
				}
			}
			return num3;
		}

		protected static int LayoutDistributeDynamic(LayoutGeometryType p_type, float[] p_args, LayoutParams p_params, Vector3[] p_buffer)
		{
			float num = 1f;
			float num2 = 0.5f;
			int num3 = 0;
			LayoutParams p_params2;
			for (int i = 0; i < 30; i++)
			{
				p_params2 = p_params;
				p_params2.span /= num;
				num3 = LayoutDistributeBase(p_type, p_args, p_params2, null);
				if (num >= 1f && num3 < p_params2.max)
				{
					break;
				}
				num = Mathf.Clamp01(num + ((num3 < p_params2.max) ? num2 : (0f - num2)));
				num2 *= 0.5f;
			}
			p_params2 = p_params;
			p_params2.span /= num;
			return LayoutDistributeBase(p_type, p_args, p_params2, p_buffer);
		}

		protected static int LayoutDistribute(LayoutGeometryType p_type, float[] p_args, LayoutParams p_params, Vector3[] p_buffer)
		{
			if (p_params.dynamic)
			{
				return LayoutDistributeDynamic(p_type, p_args, p_params, p_buffer);
			}
			return LayoutDistributeBase(p_type, p_args, p_params, p_buffer);
		}

		public static int SphereDistribute(float p_radius, LayoutParams p_params, Vector3[] p_buffer)
		{
			surface_args[0] = p_radius;
			return LayoutDistribute(LayoutGeometryType.Sphere, surface_args, p_params, p_buffer);
		}

		public static int ConeDistribute(float p_radius, float p_height, float p_aperture, LayoutParams p_params, Vector3[] p_buffer)
		{
			surface_args[0] = p_radius;
			surface_args[1] = p_height;
			surface_args[2] = p_aperture;
			return LayoutDistribute(LayoutGeometryType.Cone, surface_args, p_params, p_buffer);
		}

		public static int CylinderDistribute(float p_radius, float p_height, LayoutParams p_params, Vector3[] p_buffer)
		{
			surface_args[0] = p_radius;
			surface_args[1] = p_height;
			return LayoutDistribute(LayoutGeometryType.Cylinder, surface_args, p_params, p_buffer);
		}

		public static int GridDistribute(float p_x, float p_y, float p_z, LayoutParams p_params, Vector3[] p_buffer)
		{
			surface_args[0] = p_x;
			surface_args[1] = p_y;
			surface_args[2] = p_z;
			return LayoutDistribute(LayoutGeometryType.Grid, surface_args, p_params, p_buffer);
		}
	}
}
