using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using thelab;

namespace drl.sim
{
	[CreateAssetMenu(fileName = "lookup.gatech.asset", menuName = "DRL/GATech Lookup Data")]
	public class GATechLookupData : ScriptableObject
	{
		[Serializable]
		public struct LookupTable
		{
			public float _0;

			public float _0prime;

			public AnimationCurve _alpha;

			public AnimationCurve _alphaPrime;

			public AnimationCurve _beta;

			public AnimationCurve _betaPrime;

			public float _pqr;

			public AnimationCurve _slopeAlpha;

			public AnimationCurve _slopeBeta;
		}

		public class CachedLookupTable
		{
			public float _0;

			public float _0prime;

			public AnimationCurveExtension.AnimationCurveLUT _alpha;

			public AnimationCurveExtension.AnimationCurveLUT _alphaPrime;

			public AnimationCurveExtension.AnimationCurveLUT _beta;

			public AnimationCurveExtension.AnimationCurveLUT _betaPrime;

			public float _pqr;

			public AnimationCurveExtension.AnimationCurveLUT _slopeAlpha;

			public AnimationCurveExtension.AnimationCurveLUT _slopeBeta;
		}

		[Serializable]
		public struct LookupTables
		{
			public LookupTable CD;

			public LookupTable CY;

			public LookupTable CL;

			public LookupTable Cl;

			public LookupTable Cm;

			public LookupTable Cn;
		}

		public class CachedLookupTables
		{
			public CachedLookupTable CD;

			public CachedLookupTable CY;

			public CachedLookupTable CL;

			public CachedLookupTable Cl;

			public CachedLookupTable Cm;

			public CachedLookupTable Cn;
		}

		public float areaReference;

		public float lengthReference;

		public LookupTables orientation1;

		public LookupTables orientation2;

		[NonSerialized]
		public bool isCached;

		private CachedLookupTables m_cached1;

		private CachedLookupTables m_cached2;

		private int m_samples;

		public CachedLookupTables cached1
		{
			get
			{
				if (!isCached)
				{
					Cache(360);
				}
				return m_cached1;
			}
		}

		public CachedLookupTables cached2
		{
			get
			{
				if (!isCached)
				{
					Cache(360);
				}
				return m_cached2;
			}
		}

		public void Cache(int p_samples)
		{
			if (!isCached || m_samples != p_samples)
			{
				isCached = true;
				m_cached1 = CacheTables(orientation1, p_samples);
				m_cached2 = CacheTables(orientation2, p_samples);
			}
		}

		private CachedLookupTables CacheTables(LookupTables p_tables, int p_samples)
		{
			return new CachedLookupTables
			{
				CD = CacheCurves(p_tables.CD, p_samples),
				CL = CacheCurves(p_tables.CL, p_samples),
				CY = CacheCurves(p_tables.CY, p_samples),
				Cm = CacheCurves(p_tables.Cm, p_samples),
				Cn = CacheCurves(p_tables.Cn, p_samples),
				Cl = CacheCurves(p_tables.Cl, p_samples)
			};
		}

		private CachedLookupTable CacheCurves(LookupTable p_table, int p_samples)
		{
			return new CachedLookupTable
			{
				_0 = p_table._0,
				_0prime = p_table._0prime,
				_pqr = p_table._pqr,
				_alpha = p_table._alpha.Cache(360, p_force: false),
				_alphaPrime = p_table._alphaPrime.Cache(360, p_force: false),
				_beta = p_table._beta.Cache(360, p_force: false),
				_betaPrime = p_table._betaPrime.Cache(360, p_force: false),
				_slopeAlpha = p_table._slopeAlpha.Cache(360, p_force: false),
				_slopeBeta = p_table._slopeBeta.Cache(360, p_force: false)
			};
		}

		public string Import(string p_folder)
		{
			if (string.IsNullOrEmpty(p_folder))
			{
				return null;
			}
			p_folder = p_folder.Trim('\\', '/');
			if (!Directory.Exists(p_folder))
			{
				return "folder not found";
			}
			if (!File.Exists(p_folder + "/aero_data_orientation1/reference_dimensions.dat"))
			{
				return "invalid folder structure";
			}
			string[] array = File.ReadAllLines(p_folder + "/aero_data_orientation1/reference_dimensions.dat");
			if (array.Length < 3)
			{
				return "invalid reference_dimensions.dat";
			}
			if (!float.TryParse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture, out areaReference))
			{
				return "can't parse area from line:" + array[1];
			}
			if (!float.TryParse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture, out lengthReference))
			{
				return "can't parse length from line:" + array[2];
			}
			if (!ImportTable(p_folder + "/aero_data_orientation1/CD", ref orientation1.CD, "CD"))
			{
				return "cant parse orientation 1 table CD";
			}
			if (!ImportTable(p_folder + "/aero_data_orientation1/CY", ref orientation1.CY, "CY"))
			{
				return "cant parse orientation 1 table CY";
			}
			if (!ImportTable(p_folder + "/aero_data_orientation1/CL", ref orientation1.CL, "CL"))
			{
				return "cant parse orientation 1 table CL";
			}
			if (!ImportTable(p_folder + "/aero_data_orientation1/C_l", ref orientation1.Cl, "C_l", p_isMoment: true))
			{
				return "cant parse orientation 1 table C_l";
			}
			if (!ImportTable(p_folder + "/aero_data_orientation1/Cm", ref orientation1.Cm, "Cm", p_isMoment: true))
			{
				return "cant parse orientation 1 table Cm";
			}
			if (!ImportTable(p_folder + "/aero_data_orientation1/Cn", ref orientation1.Cn, "Cn", p_isMoment: true))
			{
				return "cant parse orientation 1 table Cn";
			}
			if (!ImportTable(p_folder + "/aero_data_orientation2/CD", ref orientation2.CD, "CD"))
			{
				return "cant parse orientation 2 table CD";
			}
			if (!ImportTable(p_folder + "/aero_data_orientation2/CY", ref orientation2.CY, "CY"))
			{
				return "cant parse orientation 2 table CY";
			}
			if (!ImportTable(p_folder + "/aero_data_orientation2/CL", ref orientation2.CL, "CL"))
			{
				return "cant parse orientation 2 table CL";
			}
			if (!ImportTable(p_folder + "/aero_data_orientation2/C_l", ref orientation2.Cl, "C_l", p_isMoment: true))
			{
				return "cant parse orientation 2 table C_l";
			}
			if (!ImportTable(p_folder + "/aero_data_orientation2/Cm", ref orientation2.Cm, "Cm", p_isMoment: true))
			{
				return "cant parse orientation 2 table Cm";
			}
			if (!ImportTable(p_folder + "/aero_data_orientation2/Cn", ref orientation2.Cn, "Cn", p_isMoment: true))
			{
				return "cant parse orientation 2 table Cn";
			}
			return "done";
		}

		public bool ImportTable(string p_folder, ref LookupTable p_table, string p_tableName, bool p_isMoment = false)
		{
			if (string.IsNullOrEmpty(p_folder))
			{
				Debug.LogError("folder is empty");
				return false;
			}
			p_folder = p_folder.Trim('\\', '/');
			if (!Directory.Exists(p_folder))
			{
				Debug.LogError("folder doesn't exist:" + p_folder);
				return false;
			}
			if (!File.Exists(p_folder + "/" + p_tableName + "_0.dat"))
			{
				Debug.LogError("_0 file doesn't exist");
				return false;
			}
			string[] array = File.ReadAllLines(p_folder + "/" + p_tableName + "_0.dat");
			if (array.Length < 2)
			{
				Debug.LogError("_0.dat less than 2 lines");
				return false;
			}
			if (!float.TryParse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture, out p_table._0))
			{
				Debug.LogError("can't parse factor 0 from line:" + array[1]);
				return false;
			}
			array = File.ReadAllLines(p_folder + "/" + p_tableName + "_0prime.dat");
			if (array.Length < 2)
			{
				Debug.LogError("_0prime.dat less than 2 lines");
				return false;
			}
			if (!float.TryParse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture, out p_table._0prime))
			{
				Debug.LogError("can't parse factor 0prime from line:" + array[1]);
				return false;
			}
			char[] separator = new char[1] { ' ' };
			array = File.ReadAllLines(p_folder + "/" + p_tableName + "_alpha.dat");
			if (array.Length < 180)
			{
				Debug.LogError("alpha.dat less than 180 lines");
				return false;
			}
			List<Keyframe> list = new List<Keyframe>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!string.IsNullOrEmpty(text) && !text.Contains("#"))
				{
					string[] array3 = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
					if (array3.Length < 2)
					{
						Debug.LogError("invalid param count in alpha line:" + text);
						return false;
					}
					if (!float.TryParse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
					{
						Debug.LogError("can't parse factor key from alpha line:" + text);
						return false;
					}
					if (!float.TryParse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result2))
					{
						Debug.LogError("can't parse factor value from alpha line:" + text);
						return false;
					}
					list.Add(new Keyframe(result, result2));
				}
			}
			p_table._alpha = new AnimationCurve(list.ToArray());
			array = File.ReadAllLines(p_folder + "/" + p_tableName + "_alphaprime.dat");
			if (array.Length < 3)
			{
				Debug.LogError("alphaprime.dat less than 3 lines");
				return false;
			}
			list = new List<Keyframe>();
			array2 = array;
			foreach (string text2 in array2)
			{
				if (!string.IsNullOrEmpty(text2) && !text2.Contains("#"))
				{
					string[] array4 = text2.Split(separator, StringSplitOptions.RemoveEmptyEntries);
					if (array4.Length < 2)
					{
						Debug.LogError("invalid param count in alpha prime line:" + text2);
						return false;
					}
					if (!float.TryParse(array4[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result3))
					{
						Debug.LogError("can't parse factor key from alpha prime line:" + text2);
						return false;
					}
					if (!float.TryParse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result4))
					{
						Debug.LogError("can't parse factor value from alpha prime line:" + text2);
						return false;
					}
					list.Add(new Keyframe(result3, result4));
				}
			}
			p_table._alphaPrime = new AnimationCurve(list.ToArray());
			array = File.ReadAllLines(p_folder + "/" + p_tableName + "_beta.dat");
			if (array.Length < 180)
			{
				Debug.LogError("beta.dat less than 180 lines");
				return false;
			}
			list.Clear();
			array2 = array;
			foreach (string text3 in array2)
			{
				if (!string.IsNullOrEmpty(text3) && !text3.Contains("#"))
				{
					string[] array5 = text3.Split(separator, StringSplitOptions.RemoveEmptyEntries);
					if (array5.Length < 2)
					{
						Debug.LogError("invalid param count in beta line:" + text3);
						return false;
					}
					if (!float.TryParse(array5[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result5))
					{
						Debug.LogError("can't parse factor key from beta line:" + text3);
						return false;
					}
					if (!float.TryParse(array5[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result6))
					{
						Debug.LogError("can't parse factor value from beta line:" + text3);
						return false;
					}
					list.Add(new Keyframe(result5, result6));
				}
			}
			p_table._beta = new AnimationCurve(list.ToArray());
			array = File.ReadAllLines(p_folder + "/" + p_tableName + "_betaprime.dat");
			if (array.Length < 3)
			{
				Debug.LogError("betaprime.dat less than 3 lines");
				return false;
			}
			list.Clear();
			array2 = array;
			foreach (string text4 in array2)
			{
				if (!string.IsNullOrEmpty(text4) && !text4.Contains("#"))
				{
					string[] array6 = text4.Split(separator, StringSplitOptions.RemoveEmptyEntries);
					if (array6.Length < 2)
					{
						Debug.LogError("invalid param count in beta prime line:" + text4);
						return false;
					}
					if (!float.TryParse(array6[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result7))
					{
						Debug.LogError("can't parse factor key from beta prime line:" + text4);
						return false;
					}
					if (!float.TryParse(array6[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result8))
					{
						Debug.LogError("can't parse factor value from beta prime line:" + text4);
						return false;
					}
					list.Add(new Keyframe(result7, result8));
				}
			}
			p_table._betaPrime = new AnimationCurve(list.ToArray());
			if (p_isMoment)
			{
				string path = p_folder + "/" + p_tableName + "_p.dat";
				if (!File.Exists(path))
				{
					path = p_folder + "/" + p_tableName + "_q.dat";
				}
				if (!File.Exists(path))
				{
					path = p_folder + "/" + p_tableName + "_r.dat";
				}
				if (!File.Exists(path))
				{
					return false;
				}
				array = File.ReadAllLines(path);
				if (array.Length < 2)
				{
					return false;
				}
				if (!float.TryParse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture, out p_table._pqr))
				{
					Debug.LogError("can't parse factor pqr from line:" + array[1]);
					return false;
				}
			}
			list.Clear();
			Keyframe[] keys = p_table._alpha.keys;
			int num = keys.Length - 1;
			list.Add(new Keyframe(keys[0].time, (keys[1].value - keys[0].value) / (keys[1].time - keys[0].time)));
			for (int j = 1; j < num; j++)
			{
				float num2 = keys[j + 1].time - keys[j].time;
				float num3 = keys[j].time - keys[j - 1].time;
				list.Add(new Keyframe(keys[j].time, (keys[j + 1].value - keys[j - 1].value) / (num2 + num3)));
			}
			list.Add(new Keyframe(keys[num].time, (keys[num].value - keys[num - 1].value) / (keys[num].time - keys[num - 1].time)));
			p_table._slopeAlpha = new AnimationCurve(list.ToArray());
			list.Clear();
			keys = p_table._beta.keys;
			num = keys.Length - 1;
			list.Add(new Keyframe(keys[0].time, (keys[1].value - keys[0].value) / (keys[1].time - keys[0].time)));
			for (int k = 1; k < num; k++)
			{
				float num4 = keys[k + 1].time - keys[k].time;
				float num5 = keys[k].time - keys[k - 1].time;
				list.Add(new Keyframe(keys[k].time, (keys[k + 1].value - keys[k - 1].value) / (num4 + num5)));
			}
			list.Add(new Keyframe(keys[num].time, (keys[num].value - keys[num - 1].value) / (keys[num].time - keys[num - 1].time)));
			p_table._slopeBeta = new AnimationCurve(list.ToArray());
			return true;
		}
	}
}
