using System;
using UnityEngine;
using thelab;

namespace drl.sim.gatech
{
	public class AeroModel
	{
		public float alfa;

		public float beta;

		public Vector3 faB;

		public Vector3 maB;

		public float CD;

		public float CY;

		public float CL;

		public float Cl;

		public float Cm;

		public float Cn;

		public float Aref;

		public float Lref;

		public float force_ref;

		public float moment_ref;

		public float b;

		public int ab5_order;

		private GATechLookupData lookup;

		private float[][] qs_aero_old = new float[3][];

		private float[] qu_aero_state = new float[12];

		private float[][] f_hist_aero = new float[3][];

		private int step = 1;

		public float airDensity;

		private float p;

		private float q;

		private float r;

		private Vector3 wb;

		private float wbmag;

		private Vector2 wxy;

		private const float small = 1E-15f;

		private float[] weighting = new float[2];

		private Vector2[] qs_aero_coef2 = new Vector2[6];

		private float[] qs_aero_coef = new float[6];

		private float[] wind_fluct = new float[6];

		private float[] qs_aero_dot = new float[6];

		private float[] qs_aero_dot_local = new float[6];

		private float[,] qs_aero_dot2 = new float[6, 2];

		private float[] qs_aero_ddot = new float[6];

		private float[] qs_aero_ddot_local = new float[6];

		private float[] qu_aero_dot = new float[12];

		private float[,] alf;

		private float[] xn_1 = new float[12];

		private Vector2[] wind_fluct2 = new Vector2[6];

		private float[] sinephase_2 = new float[6];

		private float omega_s;

		private float period;

		private float sinetime;

		private float sinecounter;

		private float[] sinephase = new float[6];

		private float[] sinephasespec = new float[6];

		private float omega_s_2;

		private float period_2;

		private float sinetime_2;

		private float sinecounter_2;

		public Vector3 faB_temp;

		private Vector3 maB_temp;

		public Vector3 faW;

		private Vector3 base_vect = new Vector3(1f, 0f, 0f);

		private float _base = float.NaN;

		private Vector3 cg2mc = Vector3.zero;

		private System.Random random = new System.Random();

		private const float St = 0.2f;

		private const float St_2 = 0.4f;

		private Vector2 CDprime = Vector3.zero;

		private Vector2 CYprime = Vector3.zero;

		private Vector2 CLprime = Vector3.zero;

		private Vector2 Clprime = Vector3.zero;

		private Vector2 Cmprime = Vector3.zero;

		private Vector2 Cnprime = Vector3.zero;

		public AeroModel(GATechLookupData lookup, float rho)
		{
			airDensity = rho;
			Aref = lookup.areaReference;
			Lref = lookup.lengthReference;
			force_ref = 0.5f * rho * Aref;
			moment_ref = 0.5f * rho * Aref * Lref;
			b = Lref;
			ab5_order = 3;
			this.lookup = lookup;
			qs_aero_old[0] = new float[6];
			qs_aero_old[1] = new float[6];
			qs_aero_old[2] = new float[6];
			f_hist_aero[0] = new float[12];
			f_hist_aero[1] = new float[12];
			f_hist_aero[2] = new float[12];
			alf = new float[5, 5];
			alf[0, 0] = 1f;
			alf[1, 0] = 1.5f;
			alf[1, 1] = -0.5f;
			alf[2, 0] = 1.9166666f;
			alf[2, 1] = -1.3333334f;
			alf[2, 2] = 5f / 12f;
			alf[3, 0] = 2.2916667f;
			alf[3, 1] = -2.4583333f;
			alf[3, 2] = 1.5416666f;
			alf[3, 3] = -0.375f;
			alf[4, 0] = 2.6402779f;
			alf[4, 1] = -3.8527777f;
			alf[4, 2] = 3.6333334f;
			alf[4, 3] = -1.7694445f;
			alf[4, 4] = 0.34861112f;
			step = 0;
			lookup.Cache(360);
		}

		public void Reset()
		{
			step = 0;
			ResetArray(qs_aero_old);
			ResetArray(qu_aero_state);
			ResetArray(f_hist_aero);
			ResetArray(qs_aero_coef2);
			ResetArray(qs_aero_coef);
			ResetArray(wind_fluct);
			ResetArray(qs_aero_dot);
			ResetArray(qs_aero_dot2);
			ResetArray(qs_aero_ddot);
			ResetArray(qu_aero_dot);
			ResetArray(xn_1);
			ResetArray(wind_fluct2);
			ResetArray(sinephase_2);
			omega_s = 0f;
			period = 0f;
			sinetime = 0f;
			sinecounter = 0f;
			ResetArray(sinephase);
			ResetArray(sinephasespec);
			omega_s_2 = 0f;
			period_2 = 0f;
			sinetime_2 = 0f;
			sinecounter_2 = 0f;
			force_ref = 0.5f * airDensity * Aref;
			moment_ref = 0.5f * airDensity * Aref * Lref;
			b = Lref;
		}

		private void ResetArray(float[] p_array)
		{
			for (int i = 0; i < p_array.Length; i++)
			{
				p_array[i] = 0f;
			}
		}

		private void ResetArray(float[,] p_array)
		{
			for (int i = 0; i < p_array.GetLength(0); i++)
			{
				for (int j = 0; j < p_array.GetLength(1); j++)
				{
					p_array[i, j] = 0f;
				}
			}
		}

		private void ResetArray(float[][] p_array)
		{
			for (int i = 0; i < p_array.Length; i++)
			{
				ResetArray(p_array[i]);
			}
		}

		private void ResetArray(Vector2[] p_array)
		{
			for (int i = 0; i < p_array.Length; i++)
			{
				p_array[i].x = 0f;
				p_array[i].y = 0f;
			}
		}

		public AeroModel Calculate(Vector3 wind, Vector3 angular, Vector3 cog, bool use_crossflow, bool use_unsteady, bool use_shedding, float fix_sideslip, float delta, float time, float dscale = 1f, float lscale = 1f, float sscale = 1f)
		{
			if (step < 5)
			{
				step++;
			}
			p = angular.z;
			q = 0f - angular.x;
			r = angular.y;
			wb.x = wind.z;
			wb.y = 0f - wind.x;
			wb.z = wind.y;
			wbmag = wind.magnitude;
			wxy.x = wb.x;
			wxy.y = wb.y;
			weighting[0] = wb.x * wb.x / Mathf.Max(wxy.sqrMagnitude, 1E-15f);
			weighting[1] = wb.y * wb.y / Mathf.Max(wxy.sqrMagnitude, 1E-15f);
			alfa = angle_of_attack(wb, wxy) * 180f / (float)Math.PI;
			beta = angle_of_yaw(wxy) * 180f / (float)Math.PI;
			if (float.IsNaN(alfa))
			{
				alfa = 0f;
			}
			if (float.IsNaN(beta))
			{
				beta = 0f;
			}
			qs_aero_model(alfa, beta, p, q, r, ref qs_aero_coef2);
			if (fix_sideslip > 1f)
			{
				if (Mathf.Abs(beta) < fix_sideslip)
				{
					qs_aero_coef2[1] *= Mathf.Clamp01(Mathf.Abs(beta) / (fix_sideslip * 0.5f) - 1f);
				}
				else if (Mathf.Abs(beta) > 180f - fix_sideslip)
				{
					qs_aero_coef2[1] *= Mathf.Clamp01((180f - Mathf.Abs(beta)) / (fix_sideslip * 0.5f) - 1f);
				}
			}
			combine_qs_coef(qs_aero_coef2, weighting, ref qs_aero_coef);
			if (use_crossflow)
			{
				qs_aero_coef[4] = qs_aero_coef[4] * (Mathf.Cos(2f * beta * (float)Math.PI / 180f) + 1f) / 2f;
				qs_aero_coef[5] = qs_aero_coef[5] * (Mathf.Cos(2f * alfa * (float)Math.PI / 180f) + 1f) / 2f;
			}
			if (use_unsteady)
			{
				qs_aero_derivatives(qs_aero_coef, qs_aero_old, step, delta, alfa, beta, p, q, r, weighting, ref qs_aero_dot, ref qs_aero_ddot);
				for (int i = 0; i < 6; i++)
				{
					qs_aero_old[2][i] = qs_aero_old[1][i];
					qs_aero_old[1][i] = qs_aero_old[0][i];
					qs_aero_old[0][i] = qs_aero_coef[i];
				}
				if (step == 1)
				{
					for (int j = 0; j < 6; j++)
					{
						qu_aero_state[2 * j] = qs_aero_coef[j];
						qu_aero_state[2 * j + 1] = qs_aero_dot[j];
					}
				}
				aero_filter(time, ref qu_aero_state, qs_aero_coef, qs_aero_dot, qs_aero_ddot, wbmag, b);
				adams_bashforth(ref qu_aero_state, time, ref f_hist_aero, delta, ab5_order, step);
				CD = qu_aero_state[0];
				CY = qu_aero_state[2];
				CL = qu_aero_state[4];
				Cl = qu_aero_state[6];
				Cm = qu_aero_state[8];
				Cn = qu_aero_state[10];
			}
			else
			{
				CD = qs_aero_coef[0];
				CY = qs_aero_coef[1];
				CL = qs_aero_coef[2];
				Cl = qs_aero_coef[3];
				Cm = qs_aero_coef[4];
				Cn = qs_aero_coef[5];
			}
			if (use_shedding)
			{
				shedding_model(alfa, beta, wbmag, b, time, delta, step, ref wind_fluct2);
				combine_qs_coef(wind_fluct2, weighting, ref wind_fluct);
				CD += wind_fluct[0];
				CY += wind_fluct[1];
				CL += wind_fluct[2];
				Cl += wind_fluct[3];
				Cm += wind_fluct[4];
				Cn += wind_fluct[5];
			}
			CD *= dscale;
			CY *= sscale;
			CL *= lscale;
			faW.x = CD * force_ref * wbmag * wbmag;
			faW.y = CY * force_ref * wbmag * wbmag;
			faW.z = CL * force_ref * wbmag * wbmag;
			maB_temp.x = Cl * moment_ref * wbmag * wbmag;
			maB_temp.y = Cm * moment_ref * wbmag * wbmag;
			maB_temp.z = Cn * moment_ref * wbmag * wbmag;
			faB_temp.x = 0f - faW.y;
			faB_temp.y = faW.z;
			faB_temp.z = faW.x;
			faB = Quaternion.Euler(0f - alfa, beta, 0f) * faB_temp;
			cg2mc.x = cog.z;
			cg2mc.y = 0f - cog.x;
			cg2mc.z = cog.y;
			maB_temp.x += cg2mc.y * faB.z - cg2mc.z * faB.y;
			maB_temp.y -= cg2mc.x * faB.z - cg2mc.z * faB.x;
			maB_temp.z += cg2mc.x * faB.y - cg2mc.y * faB.x;
			maB.x = 0f - maB_temp.y;
			maB.y = maB_temp.z;
			maB.z = maB_temp.x;
			return this;
		}

		public AeroModel RecalculateForces(Vector3 wind, Vector3 angular, Vector3 cog, bool use_crossflow, bool use_unsteady, bool use_shedding, float fix_sideslip, float delta, float time, float dscale = 1f, float lscale = 1f, float sscale = 1f)
		{
			wb.x = wind.z;
			wb.y = 0f - wind.x;
			wb.z = wind.y;
			wbmag = wind.magnitude;
			wxy.x = wb.x;
			wxy.y = wb.y;
			alfa = angle_of_attack(wb, wxy) * 180f / (float)Math.PI;
			beta = angle_of_yaw(wxy) * 180f / (float)Math.PI;
			if (float.IsNaN(alfa))
			{
				alfa = 0f;
			}
			if (float.IsNaN(beta))
			{
				beta = 0f;
			}
			faW.x = CD * force_ref * wbmag * wbmag;
			faW.y = CY * force_ref * wbmag * wbmag;
			faW.z = CL * force_ref * wbmag * wbmag;
			maB_temp.x = Cl * moment_ref * wbmag * wbmag;
			maB_temp.y = Cm * moment_ref * wbmag * wbmag;
			maB_temp.z = Cn * moment_ref * wbmag * wbmag;
			faB_temp.x = 0f - faW.y;
			faB_temp.y = faW.z;
			faB_temp.z = faW.x;
			faB = Quaternion.Euler(0f - alfa, beta, 0f) * faB_temp;
			cg2mc.x = cog.z;
			cg2mc.y = 0f - cog.x;
			cg2mc.z = cog.y;
			maB_temp.x += cg2mc.y * faB.z - cg2mc.z * faB.y;
			maB_temp.y -= cg2mc.x * faB.z - cg2mc.z * faB.x;
			maB_temp.z += cg2mc.x * faB.y - cg2mc.y * faB.x;
			maB.x = 0f - maB_temp.y;
			maB.y = maB_temp.z;
			maB.z = maB_temp.x;
			return this;
		}

		private float angle_of_attack(Vector3 wb, Vector2 wxy)
		{
			float num = ((!((double)wb.z >= 0.0)) ? (-1f) : 1f);
			float num2 = ((wxy.magnitude > 0f) ? Mathf.Acos(Mathf.Clamp(Vector3.Dot(wb, wxy) / (wxy.magnitude * wb.magnitude), -1f, 1f)) : ((!(wb.magnitude > 0f)) ? 0f : ((float)Math.PI / 2f)));
			return num * num2;
		}

		private float angle_of_yaw(Vector2 wxy)
		{
			int num = determine_quadrant(wxy);
			_base = float.NaN;
			if (num < 5)
			{
				_base = Mathf.Acos(Mathf.Clamp(Vector3.Dot(wxy, base_vect) / wxy.magnitude, -1f, 1f));
			}
			if (num < 3)
			{
				return 0f - _base;
			}
			switch (num)
			{
			case 3:
			case 4:
				return _base;
			case 5:
				return -(float)Math.PI / 2f;
			case 6:
				return (float)Math.PI / 2f;
			case 7:
				return 0f;
			case 8:
				return (float)Math.PI;
			default:
				return 0f;
			}
		}

		private int determine_quadrant(Vector2 vector)
		{
			if (vector.x > 0f && vector.y > 0f)
			{
				return 1;
			}
			if (vector.x < 0f && vector.y > 0f)
			{
				return 2;
			}
			if (vector.x < 0f && vector.y < 0f)
			{
				return 3;
			}
			if (vector.x > 0f && vector.y < 0f)
			{
				return 4;
			}
			if (vector.x == 0f && vector.y > 0f)
			{
				return 5;
			}
			if (vector.x == 0f && vector.y < 0f)
			{
				return 6;
			}
			if (vector.x > 0f && vector.y == 0f)
			{
				return 7;
			}
			if (vector.x < 0f && vector.y == 0f)
			{
				return 8;
			}
			if (vector.x == 0f && vector.y == 0f)
			{
				return 9;
			}
			return 0;
		}

		private void my_normrnd(float mu, float sigma, ref float[] val)
		{
			for (int i = 0; i < val.Length; i++)
			{
				val[i] = Mathf.Sqrt(-2f * Mathf.Log((float)random.NextDouble())) * Mathf.Cos((float)Math.PI * 2f * (float)random.NextDouble());
				val[i] = mu + sigma * val[i];
			}
		}

		private void randneg1_1(ref float[] val)
		{
			my_normrnd((float)Math.PI, 1.62f, ref val);
		}

		private void qs_aero_model(float alfa, float beta, float p, float q, float r, ref Vector2[] qs_aero_coef_out)
		{
			Vector2 zero = Vector2.zero;
			Vector2 zero2 = Vector2.zero;
			Vector2 zero3 = Vector2.zero;
			Vector2 zero4 = Vector2.zero;
			Vector2 zero5 = Vector2.zero;
			Vector2 zero6 = Vector2.zero;
			zero.x = lookup.cached1.CD._0;
			zero.x += aero_sensitivity(lookup.cached1.CD._alpha, lookup.cached1.CD._0, alfa);
			zero.x += aero_sensitivity(lookup.cached1.CD._beta, lookup.cached1.CD._0, beta);
			zero.y = lookup.cached2.CD._0;
			zero.y += aero_sensitivity(lookup.cached2.CD._alpha, lookup.cached2.CD._0, alfa);
			zero.y += aero_sensitivity(lookup.cached2.CD._beta, lookup.cached2.CD._0, beta);
			zero3.x = lookup.cached1.CY._0;
			zero3.x += aero_sensitivity(lookup.cached1.CY._alpha, lookup.cached1.CY._0, alfa);
			zero3.x += aero_sensitivity(lookup.cached1.CY._beta, lookup.cached1.CY._0, beta);
			zero3.y = lookup.cached2.CY._0;
			zero3.y += aero_sensitivity(lookup.cached2.CY._alpha, lookup.cached2.CY._0, alfa);
			zero3.y += aero_sensitivity(lookup.cached2.CY._beta, lookup.cached2.CY._0, beta);
			zero2.x = lookup.cached1.CL._0;
			zero2.x += aero_sensitivity(lookup.cached1.CL._alpha, lookup.cached1.CL._0, alfa);
			zero2.x += aero_sensitivity(lookup.cached1.CL._beta, lookup.cached1.CL._0, beta);
			zero2.y = lookup.cached2.CL._0;
			zero2.y += aero_sensitivity(lookup.cached2.CL._alpha, lookup.cached2.CL._0, alfa);
			zero2.y += aero_sensitivity(lookup.cached2.CL._beta, lookup.cached2.CL._0, beta);
			zero4.x = lookup.cached1.Cl._0;
			zero4.x += aero_sensitivity(lookup.cached1.Cl._alpha, lookup.cached1.Cl._0, alfa);
			zero4.x += aero_sensitivity(lookup.cached1.Cl._beta, lookup.cached1.Cl._0, beta);
			zero4.x += aero_sensitivity(lookup.cached1.Cl._pqr, 0f, p);
			zero4.y = lookup.cached2.Cl._0;
			zero4.y += aero_sensitivity(lookup.cached2.Cl._alpha, lookup.cached2.Cl._0, alfa);
			zero4.y += aero_sensitivity(lookup.cached2.Cl._beta, lookup.cached2.Cl._0, beta);
			zero4.y += aero_sensitivity(lookup.cached2.Cl._pqr, 0f, p);
			zero5.x = lookup.cached1.Cm._0;
			zero5.x += aero_sensitivity(lookup.cached1.Cm._alpha, lookup.cached1.Cm._0, alfa);
			zero5.x += aero_sensitivity(lookup.cached1.Cm._beta, lookup.cached1.Cm._0, beta);
			zero5.x += aero_sensitivity(lookup.cached1.Cm._pqr, 0f, q);
			zero5.y = lookup.cached2.Cm._0;
			zero5.y += aero_sensitivity(lookup.cached2.Cm._alpha, lookup.cached2.Cm._0, alfa);
			zero5.y += aero_sensitivity(lookup.cached2.Cm._beta, lookup.cached2.Cm._0, beta);
			zero5.y += aero_sensitivity(lookup.cached2.Cm._pqr, 0f, q);
			zero6.x = lookup.cached1.Cn._0;
			zero6.x += aero_sensitivity(lookup.cached1.Cn._alpha, lookup.cached1.Cn._0, alfa);
			zero6.x += aero_sensitivity(lookup.cached1.Cn._beta, lookup.cached1.Cn._0, beta);
			zero6.x += aero_sensitivity(lookup.cached1.Cn._pqr, 0f, r);
			zero6.y = lookup.cached2.Cn._0;
			zero6.y += aero_sensitivity(lookup.cached2.Cn._alpha, lookup.cached2.Cn._0, alfa);
			zero6.y += aero_sensitivity(lookup.cached2.Cn._beta, lookup.cached2.Cn._0, beta);
			zero6.y += aero_sensitivity(lookup.cached2.Cn._pqr, 0f, r);
			qs_aero_coef_out[0] = zero;
			qs_aero_coef_out[1] = zero3;
			qs_aero_coef_out[2] = zero2;
			qs_aero_coef_out[3] = zero4;
			qs_aero_coef_out[4] = zero5;
			qs_aero_coef_out[5] = zero6;
		}

		private float aero_sensitivity(AnimationCurveExtension.AnimationCurveLUT coefdata, float coef0, float parameter)
		{
			return coefdata.Evaluate(parameter) - coef0;
		}

		private float aero_sensitivity(float coefdata, float coef0, float parameter)
		{
			return coefdata * parameter;
		}

		private void combine_qs_coef(Vector2[] qs_aero_coef2, float[] weighting, ref float[] qs_aero_coef_out)
		{
			for (int i = 0; i < qs_aero_coef2.Length; i++)
			{
				qs_aero_coef_out[i] = qs_aero_coef2[i].x * weighting[0] + qs_aero_coef2[i].y * weighting[1];
			}
		}

		private void combine_qs_coef(float[,] qs_aero_coef2, float[] weighting, ref float[] qs_aero_coef_out)
		{
			for (int i = 0; i < qs_aero_coef.Length; i++)
			{
				qs_aero_coef_out[i] = qs_aero_coef2[i, 0] * weighting[0] + qs_aero_coef2[i, 1] * weighting[1];
			}
		}

		private void qs_aero_derivatives(float[] qs_aero_coef, float[][] qs_aero_old, int step, float delt, float alfa, float beta, float p, float q, float r, float[] weighting, ref float[] qs_aero_dot_out, ref float[] qs_aero_ddot_out)
		{
			if (step == 1)
			{
				float num = lookup.cached1.CD._slopeAlpha.Evaluate(alfa);
				float num2 = lookup.cached1.CD._slopeAlpha.Evaluate(beta);
				float num3 = lookup.cached1.CY._slopeAlpha.Evaluate(alfa);
				float num4 = lookup.cached1.CY._slopeAlpha.Evaluate(beta);
				float num5 = lookup.cached1.CL._slopeAlpha.Evaluate(alfa);
				float num6 = lookup.cached1.CL._slopeAlpha.Evaluate(beta);
				float num7 = lookup.cached1.Cl._slopeAlpha.Evaluate(alfa);
				float num8 = lookup.cached1.Cl._slopeAlpha.Evaluate(beta);
				float num9 = lookup.cached1.Cm._slopeAlpha.Evaluate(alfa);
				float num10 = lookup.cached1.Cm._slopeAlpha.Evaluate(beta);
				float num11 = lookup.cached1.Cn._slopeAlpha.Evaluate(alfa);
				float num12 = lookup.cached1.Cn._slopeAlpha.Evaluate(beta);
				float num13 = lookup.cached2.CD._slopeAlpha.Evaluate(alfa);
				float num14 = lookup.cached2.CD._slopeAlpha.Evaluate(beta);
				float num15 = lookup.cached2.CY._slopeAlpha.Evaluate(alfa);
				float num16 = lookup.cached2.CY._slopeAlpha.Evaluate(beta);
				float num17 = lookup.cached2.CL._slopeAlpha.Evaluate(alfa);
				float num18 = lookup.cached2.CL._slopeAlpha.Evaluate(beta);
				float num19 = lookup.cached2.Cl._slopeAlpha.Evaluate(alfa);
				float num20 = lookup.cached2.Cl._slopeAlpha.Evaluate(beta);
				float num21 = lookup.cached2.Cm._slopeAlpha.Evaluate(alfa);
				float num22 = lookup.cached2.Cm._slopeAlpha.Evaluate(beta);
				float num23 = lookup.cached2.Cn._slopeAlpha.Evaluate(alfa);
				float num24 = lookup.cached2.Cn._slopeAlpha.Evaluate(beta);
				qs_aero_dot2[0, 0] = num * q + num2 * r;
				qs_aero_dot2[1, 0] = num3 * q + num4 * r;
				qs_aero_dot2[2, 0] = num5 * q + num6 * r;
				qs_aero_dot2[3, 0] = num7 * q + num8 * r;
				qs_aero_dot2[4, 0] = num9 * q + num10 * r;
				qs_aero_dot2[5, 0] = num11 * q + num12 * r;
				qs_aero_dot2[0, 1] = num13 * p + num14 * r;
				qs_aero_dot2[1, 1] = num15 * p + num16 * r;
				qs_aero_dot2[2, 1] = num17 * p + num18 * r;
				qs_aero_dot2[3, 1] = num19 * p + num20 * r;
				qs_aero_dot2[4, 1] = num21 * p + num22 * r;
				qs_aero_dot2[5, 1] = num23 * p + num24 * r;
				combine_qs_coef(qs_aero_dot2, weighting, ref qs_aero_dot_out);
			}
			switch (step)
			{
			case 1:
			{
				for (int m = 0; m < 6; m++)
				{
					qs_aero_ddot_out[m] = 0f;
				}
				break;
			}
			case 2:
			{
				for (int n = 0; n < qs_aero_dot_out.Length; n++)
				{
					qs_aero_dot_out[n] = 1f / delt * (qs_aero_coef[n] - qs_aero_old[0][n]);
				}
				for (int num25 = 0; num25 < 6; num25++)
				{
					qs_aero_ddot_out[num25] = 0f;
				}
				break;
			}
			case 3:
			{
				for (int k = 0; k < qs_aero_dot_out.Length; k++)
				{
					qs_aero_dot_out[k] = 1f / (2f * delt) * (3f * qs_aero_coef[k] - 4f * qs_aero_old[0][k] + qs_aero_old[1][k]);
				}
				for (int l = 0; l < qs_aero_dot_out.Length; l++)
				{
					qs_aero_ddot_out[l] = 1f / delt * delt * (qs_aero_coef[l] - 2f * qs_aero_old[0][l] + qs_aero_old[1][l]);
				}
				break;
			}
			default:
			{
				for (int i = 0; i < qs_aero_dot_out.Length; i++)
				{
					qs_aero_dot_out[i] = 1f / (2f * delt) * (3f * qs_aero_coef[i] - 4f * qs_aero_old[0][i] + qs_aero_old[1][i]);
				}
				for (int j = 0; j < qs_aero_dot_out.Length; j++)
				{
					qs_aero_ddot_out[j] = 1f / delt * delt * (2f * qs_aero_coef[j] - 5f * qs_aero_old[0][j] + 4f * qs_aero_old[1][j] - qs_aero_old[2][j]);
				}
				break;
			}
			}
		}

		private void aero_filter(float time, ref float[] qu_aero_state, float[] qs_aero_coef, float[] qs_aero_tder, float[] qs_aero_2tder, float Uinf, float b)
		{
			for (int i = 0; i < 6; i++)
			{
				qs_aero_dot_local[i] = qs_aero_tder[i];
				qs_aero_ddot_local[i] = qs_aero_2tder[i];
			}
			for (int j = 0; j < 6; j++)
			{
				qu_aero_dot[2 * j] = qu_aero_state[2 * j];
				qu_aero_dot[2 * j + 1] = Uinf / b * (Uinf / b) * (0.01365f * qs_aero_coef[j] - 0.01365f * qu_aero_state[2 * j]) + Uinf / b * (0.2808f * qs_aero_dot_local[j] - 0.3455f * qu_aero_state[2 * j + 1]) + 0.5f * qs_aero_ddot_local[j];
			}
		}

		private void adams_bashforth(ref float[] xn, float time, ref float[][] f_hist, float delt, int order, int n)
		{
			int num = xn.Length;
			if (f_hist[0].Length != num)
			{
				Debug.LogError("GATech> Error in adams_bashforth: wrong number of states in f_hist");
			}
			if (f_hist.Length < order)
			{
				Debug.LogError("GATech> Error in adams_bashforth: f_hist storage too short");
			}
			int num2 = ((n < order) ? (num2 = n) : (num2 = order));
			float[] array = f_hist[f_hist.Length - 1];
			if (order > 1)
			{
				for (int num3 = order - 1; num3 > 0; num3--)
				{
					f_hist[num3] = f_hist[num3 - 1];
				}
			}
			f_hist[0] = array;
			for (int i = 0; i < array.Length && i < qs_aero_dot.Length; i++)
			{
				f_hist[0][i] = qs_aero_dot[i];
			}
			for (int j = 0; j < num; j++)
			{
				xn_1[j] = 0f;
				for (int k = 0; k < num2; k++)
				{
					xn_1[j] += alf[num2, k] * f_hist[k][j];
				}
				xn_1[j] = xn[j] + delt * xn_1[j];
			}
			for (int l = 0; l < xn.Length; l++)
			{
				xn[l] = xn_1[l];
			}
		}

		private void shedding_model(float alpha, float beta, float Uref, float bref, float time, float delt, int step, ref Vector2[] wind_fluct2_out)
		{
			bool flag = false;
			if (step == 1)
			{
				omega_s = 0.2f * Uref / bref;
				period = (float)Math.PI * 2f / omega_s;
				omega_s_2 = 0.4f * Uref / bref;
				period_2 = (float)Math.PI * 2f / omega_s;
				sinecounter = 1f;
				sinetime = 0f;
				sinecounter_2 = 1f;
				sinetime_2 = 0f;
			}
			if (sinetime <= 0f)
			{
				randneg1_1(ref sinephase);
			}
			if (sinetime_2 <= 0f)
			{
				if (flag)
				{
					randneg1_1(ref sinephase_2);
				}
				else
				{
					sinephase_2 = new float[6];
				}
			}
			CDprime.x = lookup.cached1.CD._0prime;
			CDprime.x += aero_sensitivity(lookup.cached1.CD._alphaPrime, lookup.cached1.CD._0prime, alpha);
			CDprime.x += aero_sensitivity(lookup.cached1.CD._betaPrime, lookup.cached1.CD._0prime, beta);
			CDprime.x *= Mathf.Sin(omega_s * sinetime + sinephase[0]) / 0.70711f;
			CDprime.y = lookup.cached2.CD._0prime;
			CDprime.y += aero_sensitivity(lookup.cached2.CD._alphaPrime, lookup.cached2.CD._0prime, alpha);
			CDprime.y += aero_sensitivity(lookup.cached2.CD._betaPrime, lookup.cached2.CD._0prime, beta);
			CDprime.y *= Mathf.Sin(omega_s * sinetime + sinephase[0]) / 0.70711f;
			CYprime.x = lookup.cached1.CY._0prime;
			CYprime.x += aero_sensitivity(lookup.cached1.CY._alphaPrime, lookup.cached1.CY._0prime, alpha);
			CYprime.x += aero_sensitivity(lookup.cached1.CY._betaPrime, lookup.cached1.CY._0prime, beta);
			CYprime.x *= Mathf.Sin(omega_s * sinetime + sinephase[1]) / 0.70711f;
			CYprime.y = lookup.cached2.CY._0prime;
			CYprime.y += aero_sensitivity(lookup.cached2.CY._alphaPrime, lookup.cached2.CY._0prime, alpha);
			CYprime.y += aero_sensitivity(lookup.cached2.CY._betaPrime, lookup.cached2.CY._0prime, beta);
			CYprime.y *= Mathf.Sin(omega_s * sinetime + sinephase[1]) / 0.70711f;
			CLprime.x = lookup.cached1.CL._0prime;
			CLprime.x += aero_sensitivity(lookup.cached1.CL._alphaPrime, lookup.cached1.CL._0prime, alpha);
			CLprime.x += aero_sensitivity(lookup.cached1.CL._betaPrime, lookup.cached1.CL._0prime, beta);
			CLprime.x *= Mathf.Sin(omega_s * sinetime + sinephase[2]) / 0.70711f;
			CLprime.y = lookup.cached2.CL._0prime;
			CLprime.y += aero_sensitivity(lookup.cached2.CL._alphaPrime, lookup.cached2.CL._0prime, alpha);
			CLprime.y += aero_sensitivity(lookup.cached2.CL._betaPrime, lookup.cached2.CL._0prime, beta);
			CLprime.y *= Mathf.Sin(omega_s * sinetime + sinephase[2]) / 0.70711f;
			Clprime.x = lookup.cached1.Cl._0prime;
			Clprime.x += aero_sensitivity(lookup.cached1.Cl._alphaPrime, lookup.cached1.Cl._0prime, alpha);
			Clprime.x += aero_sensitivity(lookup.cached1.Cl._betaPrime, lookup.cached1.Cl._0prime, beta);
			Clprime.x *= Mathf.Sin(omega_s * sinetime + sinephase[3]) / 0.70711f;
			Clprime.y = lookup.cached2.Cl._0prime;
			Clprime.y += aero_sensitivity(lookup.cached2.Cl._alphaPrime, lookup.cached2.Cl._0prime, alpha);
			Clprime.y += aero_sensitivity(lookup.cached2.Cl._betaPrime, lookup.cached2.Cl._0prime, beta);
			Clprime.y *= Mathf.Sin(omega_s * sinetime + sinephase[3]) / 0.70711f;
			Cmprime.x = lookup.cached1.Cm._0prime;
			Cmprime.x += aero_sensitivity(lookup.cached1.Cm._alphaPrime, lookup.cached1.Cm._0prime, alpha);
			Cmprime.x += aero_sensitivity(lookup.cached1.Cm._betaPrime, lookup.cached1.Cm._0prime, beta);
			Cmprime.x *= Mathf.Sin(omega_s * sinetime + sinephase[4]) / 0.70711f;
			Cmprime.y = lookup.cached2.Cm._0prime;
			Cmprime.y += aero_sensitivity(lookup.cached2.Cm._alphaPrime, lookup.cached2.Cm._0prime, alpha);
			Cmprime.y += aero_sensitivity(lookup.cached2.Cm._betaPrime, lookup.cached2.Cm._0prime, beta);
			Cmprime.y *= Mathf.Sin(omega_s * sinetime + sinephase[4]) / 0.70711f;
			Cnprime.x = lookup.cached1.Cn._0prime;
			Cnprime.x += aero_sensitivity(lookup.cached1.Cn._alphaPrime, lookup.cached1.Cn._0prime, alpha);
			Cnprime.x += aero_sensitivity(lookup.cached1.Cn._betaPrime, lookup.cached1.Cn._0prime, beta);
			Cnprime.x *= Mathf.Sin(omega_s * sinetime + sinephase[5]) / 0.70711f;
			Cnprime.y = lookup.cached2.Cn._0prime;
			Cnprime.y += aero_sensitivity(lookup.cached2.Cn._alphaPrime, lookup.cached2.Cn._0prime, alpha);
			Cnprime.y += aero_sensitivity(lookup.cached2.Cn._betaPrime, lookup.cached2.Cn._0prime, beta);
			Cnprime.y *= Mathf.Sin(omega_s * sinetime + sinephase[5]) / 0.70711f;
			if (flag)
			{
				CDprime *= Mathf.Sin(omega_s_2 * sinetime_2 + sinephase_2[0]) / 0.70711f;
				CYprime *= Mathf.Sin(omega_s_2 * sinetime_2 + sinephase_2[1]) / 0.70711f;
				CLprime *= Mathf.Sin(omega_s_2 * sinetime_2 + sinephase_2[2]) / 0.70711f;
				Clprime *= Mathf.Sin(omega_s_2 * sinetime_2 + sinephase_2[3]) / 0.70711f;
				Cmprime *= Mathf.Sin(omega_s_2 * sinetime_2 + sinephase_2[4]) / 0.70711f;
				Cnprime *= Mathf.Sin(omega_s_2 * sinetime_2 + sinephase_2[5]) / 0.70711f;
			}
			sinetime += delt;
			if (sinetime > period)
			{
				sinecounter += 1f;
				sinetime = 0f;
			}
			sinetime_2 += delt;
			if (sinetime_2 > period_2)
			{
				sinecounter_2 += 1f;
				sinetime_2 = 0f;
			}
			wind_fluct2_out[0] = CDprime;
			wind_fluct2_out[1] = CYprime;
			wind_fluct2_out[2] = CLprime;
			wind_fluct2_out[3] = Clprime;
			wind_fluct2_out[4] = Cmprime;
			wind_fluct2_out[5] = Cnprime;
		}
	}
}
