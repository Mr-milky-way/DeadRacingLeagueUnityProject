using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace drl.sim
{
	public class DroneMotorSpec : MonoBehaviour
	{
		[Serializable]
		public class BenchData
		{
			public const int ThrustColumnId = 0;

			public const int RPMColumnId = 1;

			public const int AmpereColumnId = 2;

			public const int WattsColumnId = 3;

			public const int TorqueColumnId = 4;

			public static List<string> parseResults = new List<string>();

			public string name;

			public string propGuid;

			public string dataUrl;

			public string videoUrl;

			public string verificationFilename;

			public int verificationLine;

			public int cells = 4;

			public DronePropType prop;

			public float diameter = 4f;

			public float pitch = 4f;

			public int blades = 3;

			public float temperature;

			public bool overload;

			public bool interpolated;

			public AnimationCurve thrust;

			public float thrustScale;

			public AnimationCurve torque;

			public AnimationCurve rpm;

			public AnimationCurve watts;

			public AnimationCurve amperes;

			public float mechanicalEfficiency;

			public int mechanicalEfficiencyAtRpm;

			public AnimationCurve thrustToSignal;

			public AnimationCurve torqueToSignal;

			public AnimationCurve rpmDelay;

			public AnimationCurve thrustDelay;

			public AnimationCurve torqueDelay;

			public float spinupDelay;

			public float spindownDelay;

			private float m_maxThrust;

			private float m_maxTorque;

			private float m_maxRPM;

			private float m_maxWatts;

			private float m_maxAmperes;

			private float m_maxVoltage;

			private bool m_cachedMaximums;

			public static string parseResultsToString
			{
				get
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (string parseResult in parseResults)
					{
						stringBuilder.AppendLine(parseResult);
					}
					return stringBuilder.ToString();
				}
			}

			public float score => GetPropellerScore(diameter, pitch, blades, prop);

			public static bool ParseHeaderString(BenchData d, string s)
			{
				if (string.IsNullOrEmpty(s))
				{
					return false;
				}
				if (d == null)
				{
					return false;
				}
				string[] array = s.Split(' ');
				if (array.Length < 4)
				{
					return false;
				}
				bool result = true;
				int num = 0;
				d.diameter = 4f;
				if (float.TryParse(array[num++], out d.diameter))
				{
					d.diameter /= 10f;
				}
				else
				{
					result = false;
				}
				d.pitch = 4f;
				if (float.TryParse(array[num++], out d.pitch))
				{
					d.pitch /= 10f;
				}
				else
				{
					result = false;
				}
				d.blades = DronePropTypePrefix.ToBladeCount(array[num++]);
				if (d.blades <= 0)
				{
					result = false;
				}
				d.prop = DronePropTypePrefix.ToEnum(array[num++]);
				if (d.prop < DronePropType.PointNose)
				{
					result = false;
				}
				return result;
			}

			public static string GetHeaderString(BenchData d)
			{
				return "[" + (d.diameter * 10f).ToString("00") + (d.pitch * 10f).ToString("00") + PropTypeString(d.prop) + " " + d.blades + "B " + d.cells + "S " + Mathf.RoundToInt(d.temperature * 1.8f + 32f) + "F]";
			}

			public static string PropTypeString(DronePropType e)
			{
				return e switch
				{
					DronePropType.BullNose => "BN", 
					DronePropType.ButterCutter => "BC", 
					DronePropType.HybridBullNose => "HB", 
					_ => "", 
				};
			}

			public static List<BenchData> ParseBenchFolder(string s, DroneMotorSpec p_spec = null)
			{
				parseResults.Clear();
				s = s.Replace("file://", "").Replace("file:\\\\", "").Replace("folder://", "")
					.Replace("folder:\\\\", "")
					.Trim();
				List<BenchData> list = new List<BenchData>();
				if (s.EndsWith(".csv"))
				{
					if (File.Exists(s))
					{
						list.Add(ParseBenchFile(s, p_spec));
					}
					else
					{
						Debug.LogError("DroneMotorSpec:: ParseBenchFolder:: file not found: " + s);
					}
				}
				else
				{
					s = s.TrimEnd('/', '\\');
					if (Directory.Exists(s))
					{
						string text = s;
						string[] links = null;
						while (text.Length > 3 && Directory.Exists(text) && !File.Exists(text + "/links.txt"))
						{
							text = text.Substring(0, text.LastIndexOfAny(new char[2] { '/', '\\' }));
						}
						if (File.Exists(text + "/links.txt"))
						{
							links = File.ReadAllLines(text + "/links.txt");
						}
						string[] files = Directory.GetFiles(s, "*.csv", SearchOption.AllDirectories);
						for (int i = 0; i < files.Length; i++)
						{
							BenchData benchData = ParseBenchFile(files[i], p_spec, links);
							if (benchData != null)
							{
								list.Add(benchData);
							}
						}
						Debug.Log(parseResultsToString);
					}
					else
					{
						Debug.LogError("DroneMotorSpec:: ParseBenchFolder:: folder not found: " + s);
					}
				}
				return list;
			}

			public static BenchData ParseBenchFile(string s, DroneMotorSpec p_spec = null, string[] links = null)
			{
				string text = "";
				string text2 = "";
				string text3 = "";
				string text4 = "";
				string text5 = "";
				float num = 0f;
				int num2 = 0;
				int num3 = 0;
				bool flag = false;
				text4 = s.Substring(s.LastIndexOf('\\') + 1);
				string[] array = text4.Split('_');
				if (array.Length >= 10)
				{
					if (array[2].Length != 4 || (array[3].Length != 4 && (array[3].Length != 6 || !array[3].ToLower().EndsWith("kv"))) || array[7].Substring(1, 1) != "B" || array[8].Substring(1, 1) != "S")
					{
						if (array[2].Length != 4)
						{
							parseResults.Add("ERROR:: invalid filename format (motor size): " + text4);
						}
						else if (array[3].Length == 4 || (array[3].Length == 6 && array[3].ToLower().EndsWith("kv")))
						{
							parseResults.Add("ERROR:: invalid filename format (kv): " + text4);
						}
						else if (array[7].Substring(1, 1) != "B")
						{
							parseResults.Add("ERROR:: invalid filename format (blade count): " + text4);
						}
						else if (array[8].Substring(1, 1) != "S")
						{
							parseResults.Add("ERROR:: invalid filename format (cell count): " + text4);
						}
						else
						{
							parseResults.Add("ERROR:: invalid filename format (unknown): " + text4);
						}
						return null;
					}
					string text7;
					string text8;
					int num4;
					float num5;
					int num6;
					float num7;
					try
					{
						string text6 = array[0] + " " + array[1];
						text7 = array[4] + " " + array[5];
						text8 = array[5];
						if (p_spec != null)
						{
							string text9 = p_spec.statorWidth.ToString("00") + p_spec.statorHeight.ToString("00");
							if (text6 != p_spec.motor.info.brand + " " + p_spec.motor.info.name)
							{
								parseResults.Add("WARN:: motor name missmatch: [" + p_spec.motor.info.brand + " " + p_spec.motor.info.name + "] on motor, [" + text6 + "] on file " + text4);
							}
							if (text9 != array[2])
							{
								parseResults.Add("WARN:: motor stator size missmatch: [" + text9 + "] on motor, [" + array[2] + "] on file " + text4);
							}
							if (p_spec.kv.ToString() != array[3].Substring(0, 4))
							{
								parseResults.Add("WARN:: motor kv missmatch: [" + p_spec.kv + "] on motor, [" + array[3] + "] on file " + text4);
							}
						}
						num4 = int.Parse(array[7].Substring(0, 1));
						num5 = float.Parse(array[6].Replace("in", ""));
						num6 = int.Parse(array[8].Substring(0, 1));
						num7 = num5;
						if (array.Length > 9)
						{
							text = array[9].Replace(".csv", "");
							Match match = Regex.Match(text, "^(\\d+(\\.\\d)?[fFcC])");
							if (match.Success)
							{
								text2 = match.ToString().ToUpper();
								text = text.Replace(match.ToString(), "");
							}
							if (text == "F")
							{
								text = "";
							}
						}
						if (array.Length > 10)
						{
							text = array[10];
						}
						text = text.ToLower();
						if (text.Contains("desolder") || text.Contains("blew") || text.Contains("limit") || text.Contains("smoke") || text.Contains("break") || text.Contains("melt") || text.Contains("exceed") || text.Contains("burn") || text.Contains("critical") || text.Contains("unstable"))
						{
							flag = true;
						}
						if (!flag && text.Length > 1)
						{
							parseResults.Add("WARN:: comment " + text + " found on file " + text4);
						}
						if (links != null)
						{
							string text10 = array[0] + "_" + array[1] + "_" + array[2] + "_" + array[3] + "_" + array[4] + "_" + array[5] + "_" + array[6] + "_" + array[7] + "_" + array[8];
							text10 = text10.Trim('\\');
							for (int i = 0; i < links.Length; i += 2)
							{
								if (links[i].StartsWith(text10))
								{
									if (links[i].ToLower().Trim().EndsWith("csv") && links[i + 1].ToLower().StartsWith("http"))
									{
										text3 = links[i + 1].Trim();
									}
									else if (links[i].ToLower().Trim().EndsWith("mp4") && links[i + 1].ToLower().StartsWith("http"))
									{
										text5 = links[i + 1].Trim().Replace("?dl=0", "?dl=1");
									}
								}
							}
						}
					}
					catch (Exception ex)
					{
						parseResults.Add("ERROR:: invalid filename format (parse exception: " + ex.StackTrace + "): " + text4);
						return null;
					}
					DronePropType dronePropType = DronePropType.PointNose;
					Match match2 = Regex.Match(text8, "[3456]0\\d\\d");
					if (match2.Success)
					{
						num7 = float.Parse(match2.ToString().Substring(2)) * 0.1f;
					}
					else
					{
						match2 = Regex.Match(text8, "[3456]x([\\d\\.]+)x?");
						if (match2.Success)
						{
							num7 = float.Parse(match2.Groups[1].ToString());
						}
					}
					if (text8.ToLower().Contains("bn") || text8.ToLower().Contains("bull"))
					{
						dronePropType = DronePropType.BullNose;
					}
					BenchData benchData = new BenchData();
					benchData.cells = num6;
					benchData.diameter = num5;
					benchData.pitch = num7;
					benchData.blades = num4;
					benchData.prop = dronePropType;
					benchData.overload = flag;
					if (text2.EndsWith("F"))
					{
						benchData.temperature = (float.Parse(text2.TrimEnd('F')) - 32f) / 1.8f;
					}
					else if (text2.EndsWith("C"))
					{
						benchData.temperature = float.Parse(text2.TrimEnd('C'));
					}
					else
					{
						benchData.temperature = 100f;
					}
					benchData.name = text7 + " " + GetHeaderString(benchData) + ((text != null && text.Length > 1) ? (" " + text) : "");
					string[] array2 = File.ReadAllLines(s);
					List<string[]> list = new List<string[]>(array2.Length);
					string[] array3 = array2;
					foreach (string text11 in array3)
					{
						list.Add(text11.Split(','));
					}
					List<float> list2 = new List<float>(array2.Length);
					List<float> list3 = new List<float>(array2.Length);
					List<float> list4 = new List<float>(array2.Length);
					List<float> list5 = new List<float>(array2.Length);
					List<float> list6 = new List<float>(array2.Length);
					List<float> list7 = new List<float>(array2.Length);
					List<float> list8 = new List<float>(array2.Length);
					List<float> list9 = new List<float>(array2.Length);
					List<float> list10 = new List<float>(array2.Length);
					List<float> list11 = new List<float>(array2.Length);
					List<float> list12 = new List<float>(array2.Length);
					List<float> list13 = new List<float>(array2.Length);
					string text12 = "";
					float num8 = 0f;
					int k = 1;
					bool flag2 = false;
					bool flag3 = false;
					float num9 = 0f;
					for (; k < array2.Length - 1; k++)
					{
						if (flag3)
						{
							break;
						}
						string[] array4 = list[k];
						if (!flag2)
						{
							string[] array5 = list[k + 1];
							if (array4[1] == "1000" && array5[1] != "1000")
							{
								flag2 = true;
							}
						}
						if (!flag2)
						{
							continue;
						}
						float num10 = (float.Parse(array4[1]) - 1000f) / 1000f;
						if (num8 > num10)
						{
							flag3 = true;
							continue;
						}
						float num11 = float.Parse(array4[0]);
						float item = float.Parse(array4[8]);
						float item2 = float.Parse(array4[9]);
						float.Parse(array4[10]);
						float.Parse(array4[11]);
						float item3 = float.Parse(array4[12]);
						float.Parse(array4[14]);
						if (num9 <= 0f)
						{
							num9 = num11;
						}
						num11 -= num9;
						list10.Add(num11);
						list11.Add(item3);
						list12.Add(item2);
						list13.Add(item);
						num8 = num10;
						text12 = array2[k];
					}
					if (!flag2 && !flag)
					{
						parseResults.Add("ERROR:: incomplete bench file: " + text4);
						return null;
					}
					string[] array6 = new string[7] { "1215", "1425", "1510", "1595", "1680", "1765", "1850" };
					Dictionary<string, int> dictionary = new Dictionary<string, int>();
					Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
					int num12 = 0;
					bool flag4 = false;
					int num13 = 0;
					int num14 = 0;
					for (int l = 1; l < array2.Length - 1; l++)
					{
						string[] array7 = list[l];
						string[] array8 = list[l + 1];
						if (array7.Length > 1 && array8.Length > 1 && array7[1] == array6[num12])
						{
							if (num13 == 0 && array7[1] == array8[1])
							{
								num13 = l;
							}
							if (num13 > 0 && array7[1] != array8[1])
							{
								num14 = l;
							}
						}
						else
						{
							num13 = 0;
							num14 = 0;
						}
						if (num13 > 0 && num14 > 0)
						{
							if (num14 - num13 > 3)
							{
								dictionary.Add(array6[num12], num13);
								dictionary2.Add(array6[num12], num14);
								num12++;
							}
							num13 = 0;
							num14 = 0;
						}
						if (num12 >= array6.Length)
						{
							flag4 = true;
							break;
						}
					}
					num8 = 10000f;
					float num15 = 0f;
					if (flag4)
					{
						list2.Add(0f);
						list3.Add(0f);
						list4.Add(0f);
						list5.Add(0f);
						list6.Add(0f);
						list8.Add(0f);
						list9.Add(0f);
						list7.Add(0f);
						array3 = array6;
						foreach (string text13 in array3)
						{
							float item4 = (float.Parse(text13) - 1000f) / 1000f;
							float num16 = 0f;
							float num17 = 0f;
							float num18 = 0f;
							float num19 = 0f;
							float num20 = 0f;
							float num21 = 0f;
							float num22 = 0f;
							float num23 = 0f;
							int num24 = (dictionary2[text13] - dictionary[text13]) / 2;
							int num25 = dictionary2[text13];
							for (int m = num25 - num24; m <= num25; m++)
							{
								string[] array9 = list[m];
								num16 += float.Parse(array9[0]);
								num17 += float.Parse(array9[8]);
								num18 += float.Parse(array9[9]);
								num19 += float.Parse(array9[10]);
								num20 += float.Parse(array9[11]);
								num21 += float.Parse(array9[12]);
								num22 += float.Parse(array9[14]);
								num23 += float.Parse(array9[18]);
							}
							num16 /= (float)num24;
							num17 /= (float)num24;
							num18 /= (float)num24;
							num19 /= (float)num24;
							num20 /= (float)num24;
							num21 /= (float)num24;
							num22 /= (float)num24;
							num23 /= (float)num24;
							list2.Add(num16);
							list3.Add(item4);
							list4.Add(num17);
							list5.Add(num18);
							list6.Add(num19);
							list8.Add(num20);
							list9.Add(num21);
							list7.Add(num22);
							if (num < num23)
							{
								num = num23;
								num2 = Mathf.RoundToInt(num21);
							}
						}
					}
					else
					{
						flag2 = false;
						flag3 = false;
						bool flag5 = false;
						int num26 = 0;
						for (; k < array2.Length; k++)
						{
							if (flag3)
							{
								break;
							}
							string[] array10 = list[k];
							float num27 = (float.Parse(array10[1]) - 1000f) / 1000f;
							if (!flag2 && num8 < num27)
							{
								flag2 = true;
							}
							if (flag2)
							{
								float num28 = float.Parse(array10[0]);
								float num29 = float.Parse(array10[8]);
								float num30 = float.Parse(array10[9]);
								float num31 = float.Parse(array10[10]);
								float num32 = float.Parse(array10[11]);
								float num33 = float.Parse(array10[12]);
								float num34 = float.Parse(array10[14]);
								float num35 = float.Parse(array10[18]);
								if (num8 == num27)
								{
									list2[list2.Count - 1] += num28;
									list3[list2.Count - 1] += num27;
									list4[list2.Count - 1] += num29;
									list5[list2.Count - 1] += num30;
									list6[list2.Count - 1] += num31;
									list8[list2.Count - 1] += num32;
									list9[list2.Count - 1] += num33;
									list7[list2.Count - 1] += num34;
									num26++;
									flag5 = true;
								}
								else if (num8 > num27)
								{
									flag3 = true;
									num26++;
									list2[list2.Count - 1] /= num26;
									list3[list2.Count - 1] /= num26;
									list4[list2.Count - 1] /= num26;
									list5[list2.Count - 1] /= num26;
									list6[list2.Count - 1] /= num26;
									list8[list2.Count - 1] /= num26;
									list9[list2.Count - 1] /= num26;
									list7[list2.Count - 1] /= num26;
									num3 = k;
								}
								else if (num27 >= 0.85f || num15 == 0f || num27 - num15 >= 0.1f)
								{
									list2.Add(num28);
									list3.Add(num27);
									list4.Add(num29);
									list5.Add(num30);
									list6.Add(num31);
									list8.Add(num32);
									list9.Add(num33);
									list7.Add(num34);
									if (num < num35)
									{
										num = num35;
										num2 = Mathf.RoundToInt(num33);
									}
									num15 = num27;
								}
							}
							num8 = num27;
						}
						if (!flag5)
						{
							if (text12 != "")
							{
								if (!benchData.overload)
								{
									parseResults.Add("WARN:: incomplete bench file: " + text4);
								}
								benchData.name = text7 + (benchData.overload ? "" : " [  !!CHECK !!  ] ") + GetHeaderString(benchData) + ((text != null && text.Length > 1) ? (" " + text) : "");
								string[] array11 = text12.Split(',');
								list2.Add(float.Parse(array11[0]));
								list3.Add((float.Parse(array11[1]) - 1000f) / 1000f);
								list4.Add(float.Parse(array11[8]));
								list5.Add(float.Parse(array11[9]));
								list6.Add(float.Parse(array11[10]));
								list8.Add(float.Parse(array11[11]));
								list9.Add(float.Parse(array11[12]));
								list7.Add(float.Parse(array11[14]));
							}
							else if (!benchData.overload)
							{
								parseResults.Add("ERROR:: incomplete bench file: " + text4);
								return null;
							}
						}
						else if (!flag3)
						{
							if (!benchData.overload)
							{
								parseResults.Add("WARN:: potential overload on file " + text4);
							}
							if (num26 < 1)
							{
								num26 = 1;
							}
							list2[list2.Count - 1] /= num26;
							list3[list2.Count - 1] /= num26;
							list4[list2.Count - 1] /= num26;
							list5[list2.Count - 1] /= num26;
							list6[list2.Count - 1] /= num26;
							list8[list2.Count - 1] /= num26;
							list9[list2.Count - 1] /= num26;
							list7[list2.Count - 1] /= num26;
							benchData.name = text7 + (benchData.overload ? "" : " [  !!CHECK !!  ] ") + GetHeaderString(benchData) + ((text != null && text.Length > 1) ? (" " + text) : "");
						}
					}
					if (num3 == 0)
					{
						num3 = array2.Length;
					}
					for (k = 0; k < list3.Count; k++)
					{
						list3[k] /= 0.85f;
					}
					benchData.thrust = new AnimationCurve();
					benchData.rpm = new AnimationCurve();
					benchData.amperes = new AnimationCurve();
					benchData.watts = new AnimationCurve();
					benchData.torque = new AnimationCurve();
					benchData.thrustToSignal = new AnimationCurve();
					benchData.torqueToSignal = new AnimationCurve();
					benchData.rpmDelay = new AnimationCurve();
					benchData.thrustDelay = new AnimationCurve();
					benchData.torqueDelay = new AnimationCurve();
					FixValues(list8);
					FixValues(list7);
					FixValues(list4);
					FixValues(list9);
					FixValues(list5);
					for (k = 0; k < list2.Count; k++)
					{
						benchData.amperes.AddKey(list3[k], list8[k]);
						benchData.watts.AddKey(list8[k], list7[k]);
						benchData.torque.AddKey(list7[k], list4[k]);
						benchData.rpm.AddKey(list7[k], list9[k]);
						benchData.thrust.AddKey(list9[k], list5[k]);
						benchData.thrustToSignal.AddKey(list5[k], list3[k]);
						benchData.torqueToSignal.AddKey(list4[k], list3[k]);
					}
					for (k = 0; k < list10.Count; k++)
					{
						benchData.rpmDelay.AddKey(list10[k], list11[k]);
						benchData.torqueDelay.AddKey(list10[k], list13[k]);
						benchData.thrustDelay.AddKey(list10[k], list12[k]);
					}
					if (!benchData.overload && (benchData.amperes.keys.Length < 2 || benchData.watts.keys.Length < 2 || benchData.rpm.keys.Length < 2 || benchData.thrust.keys.Length < 2 || benchData.torque.keys.Length < 2))
					{
						parseResults.Add("ERROR:: invalid measurement in bench file: " + text4);
						return null;
					}
					benchData.verificationFilename = text4;
					benchData.verificationLine = num3;
					benchData.dataUrl = text3;
					benchData.videoUrl = text5;
					benchData.mechanicalEfficiency = num;
					benchData.mechanicalEfficiencyAtRpm = num2;
					float num36 = 0f;
					float num37 = 0f;
					float num38 = 0f;
					float num39 = 0f;
					for (k = 0; k < list10.Count; k++)
					{
						if (num36 == 0f && list11[k] > list11[list11.Count - 1] * 0.05f)
						{
							num36 = list10[(k > 0) ? (k - 1) : k];
						}
						if (num37 == 0f && list12[k] > list12[list12.Count - 1] * 0.05f)
						{
							num37 = list10[(k > 0) ? (k - 1) : k];
						}
						if (num38 == 0f && list11[k] > list11[list11.Count - 1] * 0.9f)
						{
							num38 = list10[k];
						}
						if (num39 == 0f && list12[k] > list12[list12.Count - 1] * 0.9f)
						{
							num39 = list10[k];
						}
						if (num38 < 0f && num39 > 0f)
						{
							break;
						}
					}
					benchData.spinupDelay = (num38 - num36 + (num39 - num37)) * 0.5f;
					benchData.spindownDelay = benchData.spinupDelay * 0.15f;
					return benchData;
				}
				parseResults.Add("ERROR:: invalid filename format (missing fields): " + text4);
				return null;
			}

			private static void FixValues(List<float> v)
			{
				if (v[0] > v[v.Count - 1])
				{
					for (int i = 0; i < v.Count; i++)
					{
						v[i] = Mathf.Clamp(0f - v[i], 0f, float.MaxValue);
					}
				}
				else
				{
					for (int j = 0; j < v.Count; j++)
					{
						v[j] = Mathf.Clamp(v[j], 0f, float.MaxValue);
					}
				}
			}

			private static void FixCurve(AnimationCurve c)
			{
			}

			public static List<BenchData> ParseBenchChart(string s, DroneMotorSpec p_spec = null)
			{
				if (s.TrimStart().StartsWith("file") || s.TrimStart().StartsWith("folder") || (s.TrimStart().Length > 2 && (s.TrimStart().Substring(1, 2) == ":/" || s.TrimStart().Substring(1, 2) == ":\\")))
				{
					return ParseBenchFolder(s, p_spec);
				}
				List<BenchData> list = new List<BenchData>();
				List<string> list2 = new List<string>();
				if (string.IsNullOrEmpty(s))
				{
					return list;
				}
				s = s.Trim();
				string[] array = s.Split(new string[1] { "##" }, StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					string[] array2 = array[i].Split('\n');
					int j;
					for (j = 0; j < array2.Length && string.IsNullOrEmpty(array2[j].Trim()); j++)
					{
					}
					int result = 0;
					if (!int.TryParse(array2[j].Trim(), out result))
					{
						Debug.LogWarning("DroneMotorSpec> Failed to parse Cell Count [" + array2[j] + "]");
						return list;
					}
					for (j++; j < array2.Length && string.IsNullOrEmpty(array2[j].Trim()); j++)
					{
					}
					if (j >= array2.Length)
					{
						Debug.LogWarning("DroneMotorSpec> Failed to parse! Not enough data - " + array2.Length + " total lines");
						return list;
					}
					while (j < array2.Length)
					{
						string text = array2[j++].Trim();
						if (string.IsNullOrEmpty(text))
						{
							break;
						}
						BenchData benchData = new BenchData();
						benchData.cells = result;
						bool flag = false;
						if (!ParseHeaderString(benchData, text))
						{
							Debug.LogWarning("DroneMotorSpec> Failed to parse Prop Data Header [" + text + "]");
							flag = true;
						}
						benchData.name = benchData.cells + "S " + benchData.diameter.ToString("0.0") + "in " + benchData.pitch.ToString("0.0") + "in " + DronePropTypePrefix.FromBladeCount(benchData.blades) + " " + DronePropTypePrefix.FromEnum(benchData.prop);
						if (flag)
						{
							benchData.name = "[error] " + text;
						}
						else if (list2.Contains(benchData.name))
						{
							benchData.name = "[duplicate] " + benchData.name;
						}
						else
						{
							list2.Add(benchData.name);
						}
						list.Add(benchData);
					}
					for (; j < array2.Length && string.IsNullOrEmpty(array2[j].Trim()); j++)
					{
					}
					int num = 4;
					List<float> list3 = new List<float>();
					int num2 = 0;
					while (j < array2.Length)
					{
						string text2 = array2[j++];
						num2++;
						if (string.IsNullOrEmpty(text2))
						{
							continue;
						}
						text2 = text2.Replace("\t", " ").Trim();
						if (text2 == " ")
						{
							continue;
						}
						string[] array3 = text2.Split(' ');
						if (array3.Length > num)
						{
							num = array3.Length;
						}
						for (int k = 0; k < array3.Length; k++)
						{
							string text3 = array3[k];
							if (!string.IsNullOrEmpty(text3))
							{
								float result2 = 0f;
								if (!float.TryParse(text3, out result2))
								{
									Debug.LogWarning("DroneMotorSpec> Failed to parse Table value row[" + (num2 - 1) + "] col[" + k + "] cs[" + text3 + "] vs[" + text2 + "]");
								}
								list3.Add(result2);
							}
						}
					}
					int num3 = num * list.Count;
					int num4 = list3.Count / num3;
					for (int l = 0; l < list3.Count; l++)
					{
						int num5 = l / num;
						int num6 = l % num;
						int num7 = num5 % list.Count;
						if (num7 >= list.Count)
						{
							Debug.LogWarning("DroneMotorSpec> Failed to parse BenchData! Position overflow idx[" + l + "] row[" + num5 + "] col[" + num6 + "] bid[" + num7 + "]");
							break;
						}
						int num8 = l / num3;
						float num9 = ((num4 <= 1) ? 1f : ((float)num8 / (float)(num4 - 1)));
						num9 = Mathf.Max(0.05f, 1f - num9);
						float value = list3[l];
						BenchData benchData2 = list[num7];
						AnimationCurve animationCurve = null;
						switch (num6)
						{
						case 0:
							animationCurve = (benchData2.thrust = ((benchData2.thrust == null) ? new AnimationCurve() : benchData2.thrust));
							break;
						case 1:
							animationCurve = (benchData2.rpm = ((benchData2.rpm == null) ? new AnimationCurve() : benchData2.rpm));
							break;
						case 2:
							animationCurve = (benchData2.amperes = ((benchData2.amperes == null) ? new AnimationCurve() : benchData2.amperes));
							break;
						case 3:
							animationCurve = (benchData2.watts = ((benchData2.watts == null) ? new AnimationCurve() : benchData2.watts));
							break;
						case 4:
							animationCurve = (benchData2.torque = ((benchData2.torque == null) ? new AnimationCurve() : benchData2.torque));
							break;
						}
						if (animationCurve == null)
						{
							Debug.LogWarning("DroneMotorSpec> Failed to parse BenchData Curve at col[" + num6 + "]");
							break;
						}
						animationCurve.AddKey(num9, value);
						if (num8 >= num4 - 1)
						{
							animationCurve.AddKey(0f, 0f);
						}
					}
					for (int m = 0; m < list.Count; m++)
					{
						BenchData benchData3 = list[m];
						Keyframe[] keys = benchData3.amperes.keys;
						Keyframe[] keys2 = benchData3.watts.keys;
						Mathf.Min(keys.Length, keys2.Length);
						AnimationCurve animationCurve2 = new AnimationCurve();
						for (int n = 0; n < keys.Length; n++)
						{
							animationCurve2.AddKey(keys[n].value, keys2[n].value);
						}
						benchData3.watts = animationCurve2;
						keys = benchData3.watts.keys;
						keys2 = benchData3.rpm.keys;
						Mathf.Min(keys.Length, keys2.Length);
						animationCurve2 = new AnimationCurve();
						for (int num10 = 0; num10 < keys.Length; num10++)
						{
							animationCurve2.AddKey(keys[num10].value, keys2[num10].value);
						}
						benchData3.rpm = animationCurve2;
						if (benchData3.torque != null)
						{
							keys = benchData3.watts.keys;
							keys2 = benchData3.torque.keys;
							Mathf.Min(keys.Length, keys2.Length);
							animationCurve2 = new AnimationCurve();
							for (int num11 = 0; num11 < keys.Length; num11++)
							{
								animationCurve2.AddKey(keys[num11].value, keys2[num11].value);
							}
							benchData3.torque = animationCurve2;
						}
						keys = benchData3.rpm.keys;
						keys2 = benchData3.thrust.keys;
						Mathf.Min(keys.Length, keys2.Length);
						animationCurve2 = new AnimationCurve();
						for (int num12 = 0; num12 < keys.Length; num12++)
						{
							animationCurve2.AddKey(keys[num12].value, keys2[num12].value);
						}
						benchData3.thrust = animationCurve2;
						float num13 = 0f;
						float num14 = benchData3.amperes.keys[benchData3.amperes.keys.Length - 1].time / 20f;
						animationCurve2 = new AnimationCurve();
						for (int num15 = 0; num15 < 21; num15++)
						{
							float num16 = benchData3.watts.Evaluate(benchData3.amperes.Evaluate((float)num15 * num14));
							float value2 = (float)num15 * num14;
							if (num13 < num16)
							{
								animationCurve2.AddKey(num16, value2);
								num13 = num16;
							}
						}
						benchData3.torqueToSignal = animationCurve2;
						num13 = 0f;
						animationCurve2 = new AnimationCurve();
						for (int num17 = 0; num17 < 21; num17++)
						{
							float num18 = benchData3.thrust.Evaluate(benchData3.rpm.Evaluate(benchData3.watts.Evaluate(benchData3.amperes.Evaluate((float)num17 * num14))));
							float value3 = (float)num17 * num14;
							if (num13 < num18)
							{
								animationCurve2.AddKey(num18, value3);
								num13 = num18;
							}
						}
						benchData3.thrustToSignal = animationCurve2;
						if (benchData3.torque == null || benchData3.torque.keys == null || benchData3.torque.keys.Length < 2)
						{
							benchData3.torque = DefaultTorqueCurve(benchData3.rpm);
						}
					}
				}
				return list;
			}

			public float GetMaxThrust()
			{
				CheckMaximums();
				return m_maxThrust;
			}

			public float GetMaxTorque()
			{
				CheckMaximums();
				return m_maxTorque;
			}

			public float GetMaxRPM()
			{
				CheckMaximums();
				return m_maxRPM;
			}

			public float GetMaxWatts()
			{
				CheckMaximums();
				return m_maxWatts;
			}

			public float GetMaxAmperes()
			{
				CheckMaximums();
				return m_maxAmperes;
			}

			public float GetMaxVoltage()
			{
				CheckMaximums();
				return m_maxVoltage;
			}

			public void RefreshMaximums()
			{
				m_cachedMaximums = true;
				m_maxVoltage = 3.7f * (float)cells * 1.095f;
				m_maxAmperes = GetValue(amperes, 100000);
				m_maxWatts = GetValue(watts, 100000);
				m_maxRPM = GetValue(rpm, 100000);
				m_maxTorque = GetValue(torque, 100000);
				m_maxThrust = GetValue(thrust, 100000);
			}

			private void CheckMaximums()
			{
				if (!m_cachedMaximums)
				{
					RefreshMaximums();
				}
			}

			public float GetEstimatedTopSpeed()
			{
				return EstimateTopSpeed(GetMaxRPM(), pitch, diameter, blades);
			}

			protected float GetValue(AnimationCurve c, int p_id)
			{
				int num = c.keys.Length;
				if (num <= 0)
				{
					return 0f;
				}
				p_id = Mathf.Clamp(p_id, 0, num - 1);
				return c.keys[p_id].value;
			}

			public static AnimationCurve DefaultTorqueCurve(AnimationCurve p_rpm)
			{
				AnimationCurve animationCurve = new AnimationCurve();
				for (int i = 0; i < p_rpm.keys.Length; i++)
				{
					float num = ((p_rpm.keys[i].value < 1f) ? 0f : (1f / p_rpm.keys[i].value));
					float value = p_rpm.keys[i].time * 30f * num / (float)Math.PI;
					animationCurve.AddKey(p_rpm.keys[i].time, value);
				}
				return animationCurve;
			}

			public static float EstimateTopSpeed(float p_rpm, float p_pitch, float p_diameter, float p_blades)
			{
				float num = 0f;
				float num2 = 350f;
				float num3 = 1f;
				float num4 = 0.45f;
				float num5 = 0.2f;
				float num6 = 0.3f;
				float num7 = Mathf.Pow((p_rpm * 0.0001f - 1f) / 3.5f, 1f);
				float num8 = Mathf.Pow((p_pitch - 3f) / 1.5f, 3f);
				float num9 = (p_diameter - 3f) / 3f;
				float num10 = Mathf.Min((p_blades - 2f) / 2f, 1f);
				float num11 = num3 + num4 + num5 + num6;
				float num12 = (num7 * num3 + num8 * num4 + num9 * num5 + num10 * num6) / num11;
				return num + (num2 - num) * num12;
			}
		}

		protected static Dictionary<DronePropType, float> PropTypeScore = new Dictionary<DronePropType, float>
		{
			{
				DronePropType.PointNose,
				0.9f
			},
			{
				DronePropType.HybridBullNose,
				1.2f
			},
			{
				DronePropType.BullNose,
				1.4f
			},
			{
				DronePropType.ButterCutter,
				1.8f
			}
		};

		[SerializeField]
		private DroneMotor m_motor;

		private bool m_hasMotor;

		public List<BenchData> measurements;

		public BenchData data;

		public float kv = 2300f;

		public float statorWidth = 22f;

		public float statorHeight = 4f;

		private int m_lastAllowedCells;

		private bool m_lastAllowedOverload;

		private List<string> m_lastAllowedList;

		private string m_lastAllowedGuid = "";

		private bool m_lastAllowedLipoOverload;

		private List<int> m_lastAllowedLipoList;

		public DroneMotor motor
		{
			get
			{
				if (m_hasMotor)
				{
					return m_motor;
				}
				if ((bool)m_motor)
				{
					m_hasMotor = true;
					return m_motor;
				}
				m_motor = GetComponent<DroneMotor>();
				if ((bool)m_motor)
				{
					m_hasMotor = true;
					return m_motor;
				}
				return null;
			}
			set
			{
				m_motor = value;
				m_hasMotor = m_motor != null;
			}
		}

		public bool hasMotor => m_hasMotor;

		public float stator => (float)Math.PI * Mathf.Pow(statorWidth * 0.5f, 2f) * statorHeight;

		public static float GetPropellerScore(float p_diameter, float p_pitch, int p_blades, DronePropType p_type)
		{
			float num = p_diameter;
			float num2 = p_pitch;
			float num3 = p_blades;
			float num4 = 1f;
			float num5 = 3f;
			float num6 = 6f;
			float num7 = 3f;
			float num8 = 5f;
			float num9 = 2f;
			float num10 = 4f;
			float num11 = 35f;
			float num12 = 5f;
			float num13 = 3f;
			float num14 = 1.5f;
			float p = 1.8f;
			float p2 = 0.3f;
			num = (num - num5) / (num6 - num5);
			num = Mathf.Pow(num, p);
			num2 = (num2 - num7) / (num8 - num7);
			num3 = (num3 - num9) / (num10 - num9);
			num4 = PropTypeScore[p_type];
			float num15 = num11 + num12 + num13 + num14;
			num15 = ((num15 <= 0f) ? 0f : (1f / num15));
			return Mathf.Pow((0f + num * num11 + num2 * num12 + num3 * num13 + num4 * num14) * num15, p2);
		}

		public void Build()
		{
			data = null;
			if (motor != null && motor.drone != null && motor.drone.body != null && motor.drone.body.frame != null && motor.prop != null)
			{
				List<DroneBattery> batteries = motor.drone.body.frame.batteries;
				if (batteries == null || batteries.Count <= 0)
				{
					Debug.LogError("DroneMotorSpec> battery not found!");
				}
				int p_cells = ((batteries == null || batteries.Count <= 0 || batteries[0] == null || batteries[0].cells == null) ? 4 : batteries[0].cells.Length);
				data = GetBenchData(motor.prop, p_cells);
			}
			else
			{
				Debug.LogError("DroneMotorSpec> drone components not found!");
				data = new BenchData();
			}
			if (data == null)
			{
				Debug.LogError("DroneMotorSpec> bench data not found!");
				data = new BenchData();
			}
			if (data.torque == null || data.torque.keys == null || data.torque.keys.Length < 2)
			{
				data.torque = BenchData.DefaultTorqueCurve(data.rpm);
			}
		}

		public BenchData GetBenchData(DroneProp p_prop, int p_cells)
		{
			if (!p_prop)
			{
				Debug.LogError("DroneMotorSpec> Invalid Prop to get BenchData!");
				return null;
			}
			if (measurements == null || measurements.Count < 1)
			{
				Debug.LogError("DroneMotorSpec> No measurement data!");
				return null;
			}
			BenchData benchData = null;
			float num = 0f;
			Dictionary<BenchData, float> dictionary = new Dictionary<BenchData, float>();
			string value = p_prop.benchId.ToLower().Replace(" ", "");
			for (int i = 0; i < measurements.Count; i++)
			{
				BenchData benchData2 = measurements[i];
				float num2 = 0f;
				if (benchData2.cells == p_cells && benchData2.blades == p_prop.blades && benchData2.diameter == p_prop.diameter && (benchData2.propGuid == p_prop.guid || benchData2.name.ToLower().Replace(" ", "").StartsWith(value)))
				{
					benchData = benchData2;
					break;
				}
				num2 += (float)(Mathf.Abs(benchData2.cells - p_cells) * 2);
				num2 += (float)(Mathf.Abs(benchData2.blades - p_prop.blades) * 10);
				num2 += Mathf.Abs(benchData2.diameter - p_prop.diameter) * 5f;
				num2 += Mathf.Abs(benchData2.pitch - p_prop.pitch);
				if (benchData2.prop != p_prop.type)
				{
					num2 += 0.3f;
				}
				if (!benchData2.name.StartsWith(p_prop.info.brand))
				{
					num2 += 10f;
				}
				dictionary.Add(benchData2, num2);
			}
			if (benchData == null)
			{
				num = float.MaxValue;
				foreach (KeyValuePair<BenchData, float> item in dictionary)
				{
					if (item.Value < num)
					{
						num = item.Value;
						benchData = item.Key;
					}
				}
			}
			if (benchData == null)
			{
				Debug.LogError("DroneMotorSpec> no spec found, this shouldn't happen!");
			}
			else if (num > 0f)
			{
				Debug.LogWarning("DronePropSpec> Exact BenchData not found, differences of the closest match: " + (benchData.name.StartsWith(p_prop.info.brand) ? "" : ("[" + p_prop.info.name + " -> " + benchData.name + "] ")) + ((benchData.cells == p_cells) ? "" : ("[" + p_cells + "->" + benchData.cells + " cells] ")) + ((benchData.blades == p_prop.blades) ? "" : ("[" + p_prop.blades + "->" + benchData.blades + " blades] ")) + ((benchData.diameter == p_prop.diameter) ? "" : ("[" + p_prop.diameter + "->" + benchData.diameter + " diameter] ")) + ((benchData.pitch == p_prop.pitch) ? "" : ("[" + p_prop.pitch + "->" + benchData.pitch + " pitch] ")) + ((benchData.prop == p_prop.type) ? "" : ("[" + p_prop.type.ToString() + "->" + benchData.prop.ToString() + "] ")));
				benchData = Interpolate(benchData, p_prop, p_cells);
			}
			if (benchData.rpm.keys.Length != 0 && benchData.rpm.keys[0].value > 100f)
			{
				benchData.thrust.AddKey(0f, 0f);
				benchData.torque.AddKey(0f, 0f);
				benchData.rpm.AddKey(0f, 0f);
				benchData.watts.AddKey(0f, 0f);
				benchData.amperes.AddKey(0f, 0f);
				benchData.thrustToSignal.AddKey(0f, 0f);
				benchData.torqueToSignal.AddKey(0f, 0f);
			}
			if (benchData.torque.keys.Length > 2 && benchData.torque.keys[1].value <= 0f)
			{
				benchData.torque.RemoveKey(1);
			}
			if (benchData.rpm.keys.Length > 2 && benchData.rpm.keys[1].value <= 0f)
			{
				benchData.rpm.RemoveKey(1);
			}
			benchData.RefreshMaximums();
			return benchData;
		}

		public BenchData Interpolate(BenchData p_data, DroneProp p_prop, int p_cells)
		{
			BenchData benchData = new BenchData();
			benchData.overload = true;
			benchData.name = p_data.name + " (Interpolated)";
			benchData.dataUrl = p_data.dataUrl;
			benchData.videoUrl = p_data.videoUrl;
			benchData.cells = p_data.cells;
			benchData.prop = p_data.prop;
			benchData.pitch = p_data.pitch;
			benchData.blades = p_data.blades;
			benchData.thrust = new AnimationCurve(p_data.thrust.keys);
			if (p_data.torque != null)
			{
				benchData.torque = new AnimationCurve(p_data.torque.keys);
			}
			benchData.rpm = new AnimationCurve(p_data.rpm.keys);
			benchData.watts = new AnimationCurve(p_data.watts.keys);
			benchData.amperes = new AnimationCurve(p_data.amperes.keys);
			benchData.thrustToSignal = new AnimationCurve(p_data.thrustToSignal.keys);
			benchData.torqueToSignal = new AnimationCurve(p_data.torqueToSignal.keys);
			benchData.spinupDelay = p_data.spinupDelay;
			benchData.spindownDelay = p_data.spindownDelay;
			float num = 1f;
			float num2 = 1f;
			float num3 = 1f;
			float num4 = 1f;
			float num5 = 1f;
			float num6 = 1f;
			if (p_prop.diameter > p_data.diameter)
			{
				num *= 1.2f;
				num3 *= 0.95f;
				num5 *= 1.1f;
				num4 *= 1.1f;
			}
			else if (p_prop.diameter < p_data.diameter)
			{
				num /= 1.2f;
				num3 /= 0.95f;
				num5 /= 1.1f;
				num4 /= 1.1f;
			}
			if (p_prop.pitch > p_data.pitch)
			{
				num *= 1.1f;
				num3 *= 0.97f;
				num5 *= 1.1f;
				num4 *= 1.1f;
			}
			else if (p_prop.pitch < p_data.pitch)
			{
				num /= 1.1f;
				num3 /= 0.97f;
				num5 /= 1.1f;
				num4 /= 1.1f;
			}
			if (p_prop.blades > p_data.blades)
			{
				num *= 1.2f;
				num3 *= 0.93f;
				num5 *= 1.1f;
				num4 *= 1.1f;
			}
			else if (p_prop.blades < p_data.blades)
			{
				num /= 1.2f;
				num3 /= 0.93f;
				num5 /= 1.1f;
				num4 /= 1.1f;
			}
			for (int i = 0; i < benchData.thrust.keys.Length; i++)
			{
				benchData.thrust.keys[i].value *= num;
			}
			if (benchData.torque != null)
			{
				for (int j = 0; j < benchData.torque.keys.Length; j++)
				{
					benchData.torque.keys[j].value *= num2;
				}
			}
			for (int k = 0; k < benchData.rpm.keys.Length; k++)
			{
				benchData.rpm.keys[k].value *= num3;
			}
			for (int l = 0; l < benchData.watts.keys.Length; l++)
			{
				benchData.watts.keys[l].value *= num4;
			}
			for (int m = 0; m < benchData.amperes.keys.Length; m++)
			{
				benchData.amperes.keys[m].value *= num5;
			}
			benchData.mechanicalEfficiency *= num6;
			return benchData;
		}

		public List<string> AllowedProps(int p_cells, bool p_allowOverload = false)
		{
			if (p_cells == m_lastAllowedCells && m_lastAllowedOverload == p_allowOverload && m_lastAllowedList != null)
			{
				return m_lastAllowedList;
			}
			m_lastAllowedCells = p_cells;
			m_lastAllowedOverload = p_allowOverload;
			if (m_lastAllowedList == null)
			{
				m_lastAllowedList = new List<string>();
			}
			else
			{
				m_lastAllowedList.Clear();
			}
			for (int i = 0; i < measurements.Count; i++)
			{
				BenchData benchData = measurements[i];
				if (benchData.cells == p_cells && (p_allowOverload || (!benchData.overload && benchData.GetMaxThrust() > 10f && benchData.GetMaxTorque() > 1E-05f && benchData.GetMaxRPM() > 1000f)) && !string.IsNullOrEmpty(benchData.propGuid))
				{
					m_lastAllowedList.Add(benchData.propGuid);
				}
			}
			return m_lastAllowedList;
		}

		public List<int> AllowedLipos(string p_guid, bool p_allowOverload = false)
		{
			if (p_guid == m_lastAllowedGuid && m_lastAllowedLipoOverload == p_allowOverload && m_lastAllowedLipoList != null)
			{
				return m_lastAllowedLipoList;
			}
			m_lastAllowedGuid = p_guid;
			m_lastAllowedLipoOverload = p_allowOverload;
			if (m_lastAllowedLipoList == null)
			{
				m_lastAllowedLipoList = new List<int>();
			}
			else
			{
				m_lastAllowedLipoList.Clear();
			}
			for (int i = 0; i < measurements.Count; i++)
			{
				BenchData benchData = measurements[i];
				if (benchData.propGuid == p_guid && (p_allowOverload || (!benchData.overload && benchData.GetMaxThrust() > 10f && benchData.GetMaxTorque() > 1E-05f && benchData.GetMaxRPM() > 1000f)))
				{
					m_lastAllowedLipoList.Add(benchData.cells);
				}
			}
			return m_lastAllowedLipoList;
		}
	}
}
