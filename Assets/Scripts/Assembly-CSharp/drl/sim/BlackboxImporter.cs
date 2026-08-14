using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace drl.sim
{
	public class BlackboxImporter : MonoBehaviour
	{
		public string filename;

		public int samplesPerSecond = 100;

		public Drone drone;

		public float startTIme = 10f;

		public float endTime = 20f;

		[Header("Drone info")]
		public string firmware;

		public string throttleRange;

		public string motorOutputRange;

		public string rcRate;

		public string rcExpo;

		public string rcRateYaw;

		public string rcExpoYaw;

		public string throttleMid;

		public string throttleExpo;

		public string rates;

		public string rollPID;

		public string pitchPID;

		public string yawPID;

		public string airmodeActivateThrottle;

		[Header("GPS results")]
		public AnimationCurve latCurve;

		public AnimationCurve longCurve;

		public AnimationCurve gpsAltCurve;

		public AnimationCurve speedCurve;

		public AnimationCurve headingCurve;

		[Header("FC inputs")]
		public AnimationCurve rollCurve;

		public AnimationCurve pitchCurve;

		public AnimationCurve yawCurve;

		public AnimationCurve throttleCurve;

		[Header("FC internals")]
		public AnimationCurve motorFLCurve;

		public AnimationCurve motorFRCurve;

		public AnimationCurve motorBLCurve;

		public AnimationCurve motorBRCurve;

		public AnimationCurve rollPCurve;

		public AnimationCurve rollICurve;

		public AnimationCurve rollDCurve;

		public AnimationCurve pitchPCurve;

		public AnimationCurve pitchICurve;

		public AnimationCurve pitchDCurve;

		public AnimationCurve yawPCurve;

		public AnimationCurve yawICurve;

		public AnimationCurve yawDCurve;

		[Header("FC sensors")]
		public AnimationCurve baroAltCurve;

		public AnimationCurve gyroRollCurve;

		public AnimationCurve gyroPitchCurve;

		public AnimationCurve gyroYawCurve;

		public AnimationCurve accelXCurve;

		public AnimationCurve accelYCurve;

		public AnimationCurve accelZCurve;

		public AnimationCurve rotationRollCurve;

		public AnimationCurve rotationPitchCurve;

		public AnimationCurve rotationHeadingCurve;

		public bool playbackRunning;

		public void Playback(float start = -1f, float length = -1f)
		{
			StartCoroutine(PlaybackLoop(start, length));
		}

		private IEnumerator PlaybackLoop(float start, float length)
		{
			playbackRunning = true;
			yield return null;
			drone.fc.allowYaw = false;
			drone.fc.allowRoll = false;
			drone.fc.allowPitch = false;
			drone.fc.allowThrottle = false;
			float time = ((start < 0f) ? 0f : start);
			for (float end = ((length < 0f) ? rollCurve.keys[rollCurve.keys.Length - 1].time : (start + length)); time < end; time += Time.deltaTime)
			{
				if (!playbackRunning)
				{
					break;
				}
				drone.fc.debugRoll = rollCurve.Evaluate(time) / 500f;
				drone.fc.debugPitch = pitchCurve.Evaluate(time) / 500f;
				drone.fc.debugYaw = yawCurve.Evaluate(time) / 500f;
				drone.fc.debugThrottle = (throttleCurve.Evaluate(time) - 1000f) / 1000f;
				yield return null;
			}
			drone.fc.allowYaw = true;
			drone.fc.allowRoll = true;
			drone.fc.allowPitch = true;
			drone.fc.allowThrottle = true;
			playbackRunning = false;
		}

		public void LoadFilesRange(string p_filename, float loadFromTime = -1f, float loadDuration = -1f)
		{
			filename = p_filename;
			string text = filename + ".BFL";
			string text2 = filename + ".01.csv";
			string text3 = filename + ".01.gps.csv";
			if (!File.Exists(text))
			{
				Debug.LogError("BlackboxImporter:: BFL file \"" + text + "\" not found");
				return;
			}
			if (!File.Exists(text2))
			{
				Debug.LogError("BlackboxImporter:: csv file \"" + text2 + "\" not found");
				return;
			}
			if (!File.Exists(text3))
			{
				Debug.LogError("BlackboxImporter:: GPS file \"" + text3 + "\" not found");
			}
			string[] array = File.ReadAllLines(text);
			string[] array2 = File.ReadAllLines(text2);
			string[] array3 = (File.Exists(text3) ? File.ReadAllLines(text3) : null);
			float num = 1f / (float)samplesPerSecond;
			for (int i = 0; i < 97; i++)
			{
				if (array[i].StartsWith("H Firmware revision:"))
				{
					firmware = array[i].Replace("H Firmware revision:", "");
				}
				if (array[i].StartsWith("H minthrottle:"))
				{
					throttleRange = array[i].Replace("H minthrottle:", "");
				}
				if (array[i].StartsWith("H maxthrottle:"))
				{
					throttleRange = throttleRange + "-" + array[i].Replace("H maxthrottle:", "");
				}
				if (array[i].StartsWith("H motorOutput:"))
				{
					motorOutputRange = array[i].Replace("H motorOutput:", "").Replace(",", "-");
				}
				if (array[i].StartsWith("H rc_rate:"))
				{
					rcRate = array[i].Replace("H rc_rate:", "");
				}
				if (array[i].StartsWith("H rc_expo:"))
				{
					rcExpo = array[i].Replace("H rc_expo:", "");
				}
				if (array[i].StartsWith("H rc_rate_yaw:"))
				{
					rcRateYaw = array[i].Replace("H rc_rate_yaw:", "");
				}
				if (array[i].StartsWith("H rc_expo_yaw:"))
				{
					rcExpoYaw = array[i].Replace("H rc_expo_yaw:", "");
				}
				if (array[i].StartsWith("H thr_mid:"))
				{
					throttleMid = array[i].Replace("H thr_mid:", "");
				}
				if (array[i].StartsWith("H thr_expo:"))
				{
					throttleExpo = array[i].Replace("H thr_expo:", "");
				}
				if (array[i].StartsWith("H rates:"))
				{
					rates = array[i].Replace("H rates:", "");
				}
				if (array[i].StartsWith("H rollPID:"))
				{
					rollPID = array[i].Replace("H rollPID:", "");
				}
				if (array[i].StartsWith("H pitchPID:"))
				{
					pitchPID = array[i].Replace("H pitchPID:", "");
				}
				if (array[i].StartsWith("H yawPID:"))
				{
					yawPID = array[i].Replace("H yawPID:", "");
				}
				if (array[i].StartsWith("H airmode_activate_throttle:"))
				{
					airmodeActivateThrottle = array[i].Replace("H airmode_activate_throttle:", "");
				}
			}
			if (array3 != null)
			{
				double num2 = 0.0;
				double num3 = 0.0;
				List<Keyframe> list = new List<Keyframe>();
				List<Keyframe> list2 = new List<Keyframe>();
				List<Keyframe> list3 = new List<Keyframe>();
				List<Keyframe> list4 = new List<Keyframe>();
				List<Keyframe> list5 = new List<Keyframe>();
				double num4 = 0.0;
				double num5 = 0.0;
				float num6 = 0f;
				float num7 = 0f;
				float num8 = 0f;
				int num9 = 0;
				string[] array4 = null;
				float num10 = -10f;
				float num11 = -1f;
				int num12 = 1;
				while (num10 - num11 < loadFromTime)
				{
					array4 = array3[num12++].Split(',');
					num10 = float.Parse(array4[0], CultureInfo.InvariantCulture);
					if (num11 < 0f)
					{
						num11 = num10;
					}
				}
				double num13 = double.Parse(array4[2], CultureInfo.InvariantCulture);
				double num14 = double.Parse(array4[3], CultureInfo.InvariantCulture);
				float value = float.Parse(array4[4], CultureInfo.InvariantCulture);
				float value2 = float.Parse(array4[5], CultureInfo.InvariantCulture);
				float value3 = float.Parse(array4[6], CultureInfo.InvariantCulture);
				num11 = num10;
				float num15 = num10;
				double num16 = num13;
				double num17 = num14;
				num2 = 111412.84 * Math.Cos(num16 * Math.PI / 180.0) - 93.5 * Math.Cos(3.0 * num16 * Math.PI / 180.0) + 0.118 * Math.Cos(5.0 * num16 * Math.PI / 180.0);
				num3 = 111132.92 - 559.82 * Math.Cos(2.0 * num16 * Math.PI / 180.0) + 1.175 * Math.Cos(4.0 * num16 * Math.PI / 180.0) - 0.0023 * Math.Cos(6.0 * num16 * Math.PI / 180.0);
				list.Add(new Keyframe(0f, 0f));
				list2.Add(new Keyframe(0f, 0f));
				list3.Add(new Keyframe(0f, value));
				list4.Add(new Keyframe(0f, value2));
				list5.Add(new Keyframe(0f, value3));
				float num18 = 0f;
				for (int j = num12; j < array3.Length; j++)
				{
					if (!(loadDuration < 0f) && !(num10 - num11 < loadDuration))
					{
						break;
					}
					array4 = array3[j].Split(',');
					num10 = float.Parse(array4[0], CultureInfo.InvariantCulture);
					num13 = double.Parse(array4[2], CultureInfo.InvariantCulture);
					num14 = double.Parse(array4[3], CultureInfo.InvariantCulture);
					value = float.Parse(array4[4], CultureInfo.InvariantCulture);
					value2 = float.Parse(array4[5], CultureInfo.InvariantCulture);
					value3 = float.Parse(array4[6], CultureInfo.InvariantCulture);
					num4 += num13;
					num5 += num14;
					num6 += value;
					num7 += value2;
					num8 += value3;
					num9++;
					num18 += num10 - num15;
					if (num18 >= num / 2f)
					{
						list.Add(new Keyframe(num10 - num11, (float)((num4 / (double)num9 - num16) * num3)));
						list2.Add(new Keyframe(num10 - num11, (float)((num5 / (double)num9 - num17) * num2)));
						list3.Add(new Keyframe(num10 - num11, num6 / (float)num9));
						list4.Add(new Keyframe(num10 - num11, num7 / (float)num9));
						list5.Add(new Keyframe(num10 - num11, num8 / (float)num9));
						num4 = 0.0;
						num5 = 0.0;
						num6 = 0f;
						num7 = 0f;
						num8 = 0f;
						num9 = 0;
						num18 = Mathf.Repeat(num18, num / 2f);
					}
					num15 = num10;
				}
				latCurve = new AnimationCurve(list.ToArray());
				longCurve = new AnimationCurve(list2.ToArray());
				gpsAltCurve = new AnimationCurve(list3.ToArray());
				speedCurve = new AnimationCurve(list4.ToArray());
				headingCurve = new AnimationCurve(list5.ToArray());
			}
			List<Keyframe> list6 = new List<Keyframe>();
			List<Keyframe> list7 = new List<Keyframe>();
			List<Keyframe> list8 = new List<Keyframe>();
			List<Keyframe> list9 = new List<Keyframe>();
			List<Keyframe> list10 = new List<Keyframe>();
			List<Keyframe> list11 = new List<Keyframe>();
			List<Keyframe> list12 = new List<Keyframe>();
			List<Keyframe> list13 = new List<Keyframe>();
			List<Keyframe> list14 = new List<Keyframe>();
			List<Keyframe> list15 = new List<Keyframe>();
			List<Keyframe> list16 = new List<Keyframe>();
			List<Keyframe> list17 = new List<Keyframe>();
			List<Keyframe> list18 = new List<Keyframe>();
			List<Keyframe> list19 = new List<Keyframe>();
			List<Keyframe> list20 = new List<Keyframe>();
			List<Keyframe> list21 = new List<Keyframe>();
			List<Keyframe> list22 = new List<Keyframe>();
			List<Keyframe> list23 = new List<Keyframe>();
			List<Keyframe> list24 = new List<Keyframe>();
			List<Keyframe> list25 = new List<Keyframe>();
			List<Keyframe> list26 = new List<Keyframe>();
			List<Keyframe> list27 = new List<Keyframe>();
			List<Keyframe> list28 = new List<Keyframe>();
			List<Keyframe> list29 = new List<Keyframe>();
			List<Keyframe> list30 = new List<Keyframe>();
			List<Keyframe> list31 = new List<Keyframe>();
			List<Keyframe> list32 = new List<Keyframe>();
			float num19 = 0f;
			float num20 = 0f;
			float num21 = 0f;
			float num22 = 0f;
			float num23 = 0f;
			float num24 = 0f;
			float num25 = 0f;
			float num26 = 0f;
			float num27 = 0f;
			float num28 = 0f;
			float num29 = 0f;
			float num30 = 0f;
			float num31 = 0f;
			float num32 = 0f;
			float num33 = 0f;
			float num34 = 0f;
			float num35 = 0f;
			float num36 = 0f;
			float num37 = 0f;
			float num38 = 0f;
			float num39 = 0f;
			float num40 = 0f;
			float num41 = 0f;
			float num42 = 0f;
			float num43 = 0f;
			float num44 = 0f;
			float num45 = 0f;
			int num46 = 0;
			string[] array5 = null;
			float num47 = -10f;
			float num48 = -1f;
			int num49 = 1;
			while (num47 - num48 < loadFromTime)
			{
				array5 = array2[num49++].Split(',');
				num47 = float.Parse(array5[1], CultureInfo.InvariantCulture);
				if (num48 < 0f)
				{
					num48 = num47;
				}
			}
			float value4 = float.Parse(array5[2], CultureInfo.InvariantCulture);
			float value5 = float.Parse(array5[3], CultureInfo.InvariantCulture);
			float value6 = float.Parse(array5[4], CultureInfo.InvariantCulture);
			float value7 = float.Parse(array5[5], CultureInfo.InvariantCulture);
			float value8 = float.Parse(array5[6], CultureInfo.InvariantCulture);
			float value9 = float.Parse(array5[7], CultureInfo.InvariantCulture);
			float value10 = float.Parse(array5[8], CultureInfo.InvariantCulture);
			float value11 = float.Parse(array5[9], CultureInfo.InvariantCulture);
			float value12 = float.Parse(array5[10], CultureInfo.InvariantCulture);
			float value13 = float.Parse(array5[11], CultureInfo.InvariantCulture);
			float value14 = float.Parse(array5[12], CultureInfo.InvariantCulture);
			float value15 = float.Parse(array5[13], CultureInfo.InvariantCulture);
			float num50 = float.Parse(array5[14], CultureInfo.InvariantCulture);
			float value16 = float.Parse(array5[17], CultureInfo.InvariantCulture);
			float value17 = float.Parse(array5[19], CultureInfo.InvariantCulture);
			float value18 = float.Parse(array5[20], CultureInfo.InvariantCulture);
			float value19 = float.Parse(array5[21], CultureInfo.InvariantCulture);
			float value20 = float.Parse(array5[22], CultureInfo.InvariantCulture);
			float value21 = float.Parse(array5[23], CultureInfo.InvariantCulture);
			float value22 = float.Parse(array5[24], CultureInfo.InvariantCulture);
			float value23 = float.Parse(array5[25], CultureInfo.InvariantCulture);
			float value24 = float.Parse(array5[26], CultureInfo.InvariantCulture);
			float value25 = float.Parse(array5[27], CultureInfo.InvariantCulture);
			float value26 = float.Parse(array5[28], CultureInfo.InvariantCulture);
			float value27 = float.Parse(array5[29], CultureInfo.InvariantCulture);
			float value28 = float.Parse(array5[30], CultureInfo.InvariantCulture);
			float value29 = float.Parse(array5[31], CultureInfo.InvariantCulture);
			num48 = num47;
			float num51 = num47;
			list6.Add(new Keyframe(0f, value4));
			list7.Add(new Keyframe(0f, value5));
			list8.Add(new Keyframe(0f, value6));
			list9.Add(new Keyframe(0f, value7));
			list10.Add(new Keyframe(0f, value8));
			list11.Add(new Keyframe(0f, value9));
			list12.Add(new Keyframe(0f, value10));
			list13.Add(new Keyframe(0f, value11));
			list14.Add(new Keyframe(0f, value12));
			list15.Add(new Keyframe(0f, value13));
			list16.Add(new Keyframe(0f, value14));
			list17.Add(new Keyframe(0f, value15));
			list15.Add(new Keyframe(0f, value13));
			list19.Add(new Keyframe(0f, value16));
			list20.Add(new Keyframe(0f, value17));
			list21.Add(new Keyframe(0f, value18));
			list22.Add(new Keyframe(0f, value19));
			list23.Add(new Keyframe(0f, value20));
			list24.Add(new Keyframe(0f, value21));
			list25.Add(new Keyframe(0f, value22));
			list30.Add(new Keyframe(0f, value27));
			list31.Add(new Keyframe(0f, value28));
			list32.Add(new Keyframe(0f, value29));
			list27.Add(new Keyframe(0f, value26));
			list26.Add(new Keyframe(0f, value24));
			list29.Add(new Keyframe(0f, value25));
			list28.Add(new Keyframe(0f, value23));
			float num52 = 0f;
			for (int k = num49; k < array2.Length; k++)
			{
				if (!(loadDuration < 0f) && !(num47 - num48 < loadDuration))
				{
					break;
				}
				array5 = array2[k].Split(',');
				num47 = float.Parse(array5[1], CultureInfo.InvariantCulture);
				value4 = float.Parse(array5[2], CultureInfo.InvariantCulture);
				value5 = float.Parse(array5[3], CultureInfo.InvariantCulture);
				value6 = float.Parse(array5[4], CultureInfo.InvariantCulture);
				value7 = float.Parse(array5[5], CultureInfo.InvariantCulture);
				value8 = float.Parse(array5[6], CultureInfo.InvariantCulture);
				value9 = float.Parse(array5[7], CultureInfo.InvariantCulture);
				value10 = float.Parse(array5[8], CultureInfo.InvariantCulture);
				value11 = float.Parse(array5[9], CultureInfo.InvariantCulture);
				value12 = float.Parse(array5[10], CultureInfo.InvariantCulture);
				value13 = float.Parse(array5[11], CultureInfo.InvariantCulture);
				value14 = float.Parse(array5[12], CultureInfo.InvariantCulture);
				value15 = float.Parse(array5[13], CultureInfo.InvariantCulture);
				num50 = float.Parse(array5[14], CultureInfo.InvariantCulture);
				value16 = float.Parse(array5[17], CultureInfo.InvariantCulture);
				value17 = float.Parse(array5[19], CultureInfo.InvariantCulture);
				value18 = float.Parse(array5[20], CultureInfo.InvariantCulture);
				value19 = float.Parse(array5[21], CultureInfo.InvariantCulture);
				value20 = float.Parse(array5[22], CultureInfo.InvariantCulture);
				value21 = float.Parse(array5[23], CultureInfo.InvariantCulture);
				value22 = float.Parse(array5[24], CultureInfo.InvariantCulture);
				value23 = float.Parse(array5[25], CultureInfo.InvariantCulture);
				value24 = float.Parse(array5[26], CultureInfo.InvariantCulture);
				value25 = float.Parse(array5[27], CultureInfo.InvariantCulture);
				value26 = float.Parse(array5[28], CultureInfo.InvariantCulture);
				value27 = float.Parse(array5[29], CultureInfo.InvariantCulture);
				value28 = float.Parse(array5[30], CultureInfo.InvariantCulture);
				value29 = float.Parse(array5[31], CultureInfo.InvariantCulture);
				num19 += value4;
				num20 += value5;
				num21 += value6;
				num22 += value7;
				num23 += value8;
				num24 += value9;
				num25 += value10;
				num26 += value11;
				num27 += value12;
				num28 += value13;
				num29 += value14;
				num30 += value15;
				num31 += num50;
				num32 += value16;
				num33 += value17;
				num34 += value18;
				num35 += value19;
				num36 += value20;
				num37 += value21;
				num38 += value22;
				num39 += value26;
				num40 += value24;
				num41 += value25;
				num42 += value23;
				num43 += value27;
				num44 += value28;
				num45 += value29;
				num46++;
				num52 += num47 - num51;
				if (num52 >= num / 2f)
				{
					list6.Add(new Keyframe(num47 - num48, num19 / (float)num46));
					list7.Add(new Keyframe(num47 - num48, num20 / (float)num46));
					list8.Add(new Keyframe(num47 - num48, num21 / (float)num46));
					list9.Add(new Keyframe(num47 - num48, num22 / (float)num46));
					list10.Add(new Keyframe(num47 - num48, num23 / (float)num46));
					list11.Add(new Keyframe(num47 - num48, num24 / (float)num46));
					list12.Add(new Keyframe(num47 - num48, num25 / (float)num46));
					list13.Add(new Keyframe(num47 - num48, num26 / (float)num46));
					list14.Add(new Keyframe(num47 - num48, num27 / (float)num46));
					list15.Add(new Keyframe(num47 - num48, num28 / (float)num46));
					list16.Add(new Keyframe(num47 - num48, num29 / (float)num46));
					list17.Add(new Keyframe(num47 - num48, num30 / (float)num46));
					list18.Add(new Keyframe(num47 - num48, num31 / (float)num46));
					list20.Add(new Keyframe(num47 - num48, num33 / (float)num46));
					list21.Add(new Keyframe(num47 - num48, num34 / (float)num46));
					list22.Add(new Keyframe(num47 - num48, num35 / (float)num46));
					list23.Add(new Keyframe(num47 - num48, num36 / (float)num46));
					list24.Add(new Keyframe(num47 - num48, num37 / (float)num46));
					list25.Add(new Keyframe(num47 - num48, num38 / (float)num46));
					list30.Add(new Keyframe(num47 - num48, num43 / (float)num46));
					list31.Add(new Keyframe(num47 - num48, num44 / (float)num46));
					list32.Add(new Keyframe(num47 - num48, num45 / (float)num46));
					list27.Add(new Keyframe(num47 - num48, num39 / (float)num46));
					list26.Add(new Keyframe(num47 - num48, num40 / (float)num46));
					list29.Add(new Keyframe(num47 - num48, num41 / (float)num46));
					list28.Add(new Keyframe(num47 - num48, num42 / (float)num46));
					list19.Add(new Keyframe(num47 - num48, num32 / (float)num46));
					num19 = 0f;
					num20 = 0f;
					num21 = 0f;
					num22 = 0f;
					num23 = 0f;
					num24 = 0f;
					num25 = 0f;
					num26 = 0f;
					num27 = 0f;
					num28 = 0f;
					num29 = 0f;
					num30 = 0f;
					num31 = 0f;
					num32 = 0f;
					num33 = 0f;
					num34 = 0f;
					num35 = 0f;
					num36 = 0f;
					num37 = 0f;
					num38 = 0f;
					num39 = 0f;
					num40 = 0f;
					num41 = 0f;
					num42 = 0f;
					num43 = 0f;
					num44 = 0f;
					num45 = 0f;
					num46 = 0;
					num52 = Mathf.Repeat(num52, num / 2f);
				}
				num51 = num47;
			}
			rollPCurve = new AnimationCurve(list6.ToArray());
			pitchPCurve = new AnimationCurve(list7.ToArray());
			yawPCurve = new AnimationCurve(list8.ToArray());
			rollICurve = new AnimationCurve(list9.ToArray());
			pitchICurve = new AnimationCurve(list10.ToArray());
			yawICurve = new AnimationCurve(list11.ToArray());
			rollDCurve = new AnimationCurve(list12.ToArray());
			pitchDCurve = new AnimationCurve(list13.ToArray());
			yawDCurve = new AnimationCurve(list14.ToArray());
			rollCurve = new AnimationCurve(list15.ToArray());
			pitchCurve = new AnimationCurve(list16.ToArray());
			yawCurve = new AnimationCurve(list17.ToArray());
			throttleCurve = new AnimationCurve(list18.ToArray());
			gyroRollCurve = new AnimationCurve(list20.ToArray());
			gyroPitchCurve = new AnimationCurve(list21.ToArray());
			gyroYawCurve = new AnimationCurve(list22.ToArray());
			rotationRollCurve = new AnimationCurve(list30.ToArray());
			rotationPitchCurve = new AnimationCurve(list31.ToArray());
			rotationHeadingCurve = new AnimationCurve(list32.ToArray());
			accelXCurve = new AnimationCurve(list23.ToArray());
			accelYCurve = new AnimationCurve(list24.ToArray());
			accelZCurve = new AnimationCurve(list25.ToArray());
			baroAltCurve = new AnimationCurve(list19.ToArray());
			motorFLCurve = new AnimationCurve(list27.ToArray());
			motorFRCurve = new AnimationCurve(list26.ToArray());
			motorBLCurve = new AnimationCurve(list29.ToArray());
			motorBRCurve = new AnimationCurve(list28.ToArray());
		}
	}
}
