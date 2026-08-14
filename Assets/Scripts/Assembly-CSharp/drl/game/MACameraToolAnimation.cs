using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	[ExecuteInEditMode]
	public class MACameraToolAnimation : MonoBehaviour
	{
		[Serializable]
		public class Easing
		{
			public string id = "";

			public string label = "";

			public AnimationCurve curve;

			public float[] coefs;
		}

		public List<Easing> easings;

		public List<string> labels
		{
			get
			{
				List<string> list = new List<string>();
				for (int i = 0; i < easings.Count; i++)
				{
					list.Add(easings[i].label);
				}
				return list;
			}
		}

		protected void Awake()
		{
			if (easings == null)
			{
				easings = new List<Easing>();
			}
			if (easings.Count <= 0)
			{
				Easing easing = new Easing();
				easing.id = "linear";
				easing.label = "Linear";
				easing.curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
				easings.Add(easing);
				string[] array = new string[28]
				{
					"in-sine", "in-quad", "in-cubic", "in-quart", "in-quint", "in-expo", "in-circ", "out-sine", "out-quad", "out-cubic",
					"out-quart", "out-quint", "out-expo", "out-circ", "inout-sine", "inout-quad", "inout-cubic", "inout-quart", "inout-quint", "inout-expo",
					"inout-circ", "outin-sine", "outin-quad", "outin-cubic", "outin-quart", "outin-quint", "outin-expo", "outin-circ"
				};
				string[] array2 = new string[28]
				{
					"In Sine", "In Quad", "In Cubic", "In Quart", "In Quint", "In Expo", "In Circ", "Out Sine", "Out Quad", "Out Cubic",
					"Out Quart", "Out Quint", "Out Expo", "Out Circ", "InOut Sine", "InOut Quad", "InOut Cubic", "InOut Quart", "InOut Quint", "InOut Expo",
					"InOut Circ", "OutIn Sine", "OutIn Quad", "OutIn Cubic", "OutIn Quart", "OutIn Quint", "OutIn Expo", "OutIn Circ"
				};
				float[][] array3 = new float[28][]
				{
					BezierEasing.InSineConst,
					BezierEasing.InQuadConst,
					BezierEasing.InCubicConst,
					BezierEasing.InQuartConst,
					BezierEasing.InQuintConst,
					BezierEasing.InExpoConst,
					BezierEasing.InCircConst,
					BezierEasing.OutSineConst,
					BezierEasing.OutQuadConst,
					BezierEasing.OutCubicConst,
					BezierEasing.OutQuartConst,
					BezierEasing.OutQuintConst,
					BezierEasing.OutExpoConst,
					BezierEasing.OutCircConst,
					BezierEasing.InOutSineConst,
					BezierEasing.InOutQuadConst,
					BezierEasing.InOutCubicConst,
					BezierEasing.InOutQuartConst,
					BezierEasing.InOutQuintConst,
					BezierEasing.InOutExpoConst,
					BezierEasing.InOutCircConst,
					BezierEasing.OutInSineConst,
					BezierEasing.OutInQuadConst,
					BezierEasing.OutInCubicConst,
					BezierEasing.OutInQuartConst,
					BezierEasing.OutInQuintConst,
					BezierEasing.OutInExpoConst,
					BezierEasing.OutInCircConst
				};
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					easing = new Easing();
					easing.id = array[i];
					easing.label = array2[i];
					easing.curve = Tween.GetBezierAnimationCurve(array3[i], 40);
					easing.coefs = array3[i];
					easings.Add(easing);
				}
			}
		}

		public Easing Get(string p_id)
		{
			if (easings.Count <= 0)
			{
				return null;
			}
			Easing easing = easings.Find((Easing it) => it.id == p_id);
			if (easing != null)
			{
				return easing;
			}
			return easings[0];
		}

		public string GetIdByIndex(int p_index)
		{
			if (p_index < 0)
			{
				return "";
			}
			if (p_index >= easings.Count)
			{
				return "";
			}
			return easings[p_index].id;
		}

		public int GetIndexById(string p_id)
		{
			Easing easing = Get(p_id);
			if (easing != null)
			{
				return easings.IndexOf(easing);
			}
			return -1;
		}

		public void Step(Camera p_target, MACameraToolControlPoint p_from, MACameraToolControlPoint p_to, float p_ratio, Easing p_easing)
		{
			if ((bool)p_from && (bool)p_to)
			{
				MACameraToolControlPoint.Sample sample = p_from.GetSample();
				MACameraToolControlPoint.Sample sample2 = p_to.GetSample();
				float r = ((p_easing == null) ? easings[0] : p_easing).curve.Evaluate(p_ratio);
				MACameraToolControlPoint.Sample sample3 = MACameraToolControlPoint.Sample.Lerp(sample, sample2, r);
				p_target.transform.position = sample3.position;
				p_target.transform.localRotation = sample3.rotation;
				p_target.fieldOfView = sample3.fov;
			}
		}
	}
}
