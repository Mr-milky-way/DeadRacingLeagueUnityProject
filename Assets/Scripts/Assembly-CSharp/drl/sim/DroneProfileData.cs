using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	[CreateAssetMenu(fileName = "name.profile.asset", menuName = "DRL/DroneProfileData")]
	public class DroneProfileData : ScriptableObject
	{
		[Serializable]
		public struct RatesStructure
		{
			public int Roll;

			public int Pitch;

			public int Yaw;
		}

		public float cameraTilt = 30f;

		public PIDVector pitchPID = new PIDVector(75f, 0f, 50f);

		public PIDVector rollPID = new PIDVector(75f, 0f, 50f);

		public PIDVector yawPID = new PIDVector(75f, 0f, 0f);

		public PIDVector levelPID = new PIDVector(50f, 50f, 50f);

		public RatesStructure SuperRates;

		public RatesStructure RcExpoRates;

		public RatesStructure RcRates;

		public float[] pidCorrectionP;

		public float[] pidCorrectionR;

		public float[] pidCorrectionY;

		public float overheatFactor;

		public float minSignal = 0.025f;

		public float pitchFF;

		public float rollFF;

		public float yawFF;

		public int betaflightVersion = 40;

		public bool airmode = true;

		public bool antigravity = true;

		public bool dynamicFilter = true;

		public byte feedForwardTransition = 100;

		public bool iTermRotation = true;

		public bool smartFeedForward;

		public byte iTermRelax;

		public byte iTermRelaxValue = 11;

		public byte iTermRelaxType = 1;

		public byte antigravityMode;

		public ushort antigravityGain = 1000;

		private bool autotuned;

		private PIDVector autoPitchPID;

		private PIDVector autoRollPID;

		private PIDVector autoYawPID;

		private SerializedData m_data;

		protected SerializedData data
		{
			get
			{
				if (m_data == null)
				{
					m_data = new SerializedData();
				}
				return m_data;
			}
		}

		public void CheckAutotune()
		{
			if (autotuned)
			{
				if (Mathf.Abs(pitchPID.p - autoPitchPID.p) < 8f && Mathf.Abs(pitchPID.d - autoPitchPID.d) < 8f && Mathf.Abs(pitchPID.i - autoPitchPID.i) < 8f)
				{
					pitchPID.Set(autoPitchPID);
				}
				if (Mathf.Abs(rollPID.p - autoRollPID.p) < 8f && Mathf.Abs(rollPID.d - autoRollPID.d) < 8f && Mathf.Abs(rollPID.i - autoRollPID.i) < 8f)
				{
					rollPID.Set(autoRollPID);
				}
				if (Mathf.Abs(yawPID.p - autoYawPID.p) < 8f && Mathf.Abs(yawPID.d - autoYawPID.d) < 8f && Mathf.Abs(yawPID.i - autoYawPID.i) < 8f)
				{
					yawPID.Set(autoYawPID);
				}
			}
			else
			{
				autotuned = true;
				autoPitchPID = new PIDVector(pitchPID);
				autoRollPID = new PIDVector(rollPID);
				autoYawPID = new PIDVector(yawPID);
			}
		}

		public SerializedData ToSerializedData()
		{
			data.Set("pitchPID", pitchPID);
			data.Set("rollPID", rollPID);
			data.Set("yawPID", yawPID);
			data.Set("levelPID", levelPID);
			data.Set("pidCorrectionP", pidCorrectionP);
			data.Set("pidCorrectionR", pidCorrectionR);
			data.Set("pidCorrectionY", pidCorrectionY);
			data.Set("minSignal", minSignal);
			data.Set("overheat", overheatFactor);
			data.Set("pitchFF", pitchFF);
			data.Set("rollFF", rollFF);
			data.Set("yawFF", yawFF);
			data.Set("betaflightVersion", betaflightVersion);
			data.Set("airmode", airmode);
			data.Set("antigravity", antigravity);
			data.Set("dynamicFilter", dynamicFilter);
			data.Set("feedForwardTransition", feedForwardTransition);
			data.Set("iTermRotation", iTermRotation);
			data.Set("smartFeedForward", smartFeedForward);
			data.Set("iTermRelax", iTermRelax);
			data.Set("iTermRelaxValue", iTermRelaxValue);
			data.Set("iTermRelaxType", iTermRelaxType);
			data.Set("antigravityMode", antigravityMode);
			data.Set("antigravityGain", antigravityGain);
			return data;
		}

		public static DroneProfileData FromSerializedData(SerializedData p_data)
		{
			DroneProfileData droneProfileData = ScriptableObject.CreateInstance<DroneProfileData>();
			droneProfileData.pitchPID = p_data.Get("pitchPID", new PIDVector(75f, 0f, 50f));
			droneProfileData.rollPID = p_data.Get("rollPID", new PIDVector(75f, 0f, 50f));
			droneProfileData.yawPID = p_data.Get("yawPID", new PIDVector(75f, 0f, 0f));
			droneProfileData.levelPID = p_data.Get("levelPID", new PIDVector(50f, 50f, 50f));
			if (droneProfileData.pitchPID.p < 1f || droneProfileData.pitchPID.d < 1f)
			{
				droneProfileData.pitchPID = new PIDVector(75f, 0f, 50f);
			}
			if (droneProfileData.rollPID.p < 1f || droneProfileData.rollPID.d < 1f)
			{
				droneProfileData.rollPID = new PIDVector(75f, 0f, 50f);
			}
			if (droneProfileData.yawPID.p < 1f)
			{
				droneProfileData.yawPID = new PIDVector(75f, 0f, 0f);
			}
			droneProfileData.pidCorrectionP = p_data.Get("pidCorrectionP", new float[0]);
			droneProfileData.pidCorrectionR = p_data.Get("pidCorrectionR", new float[0]);
			droneProfileData.pidCorrectionY = p_data.Get("pidCorrectionY", new float[0]);
			droneProfileData.minSignal = p_data.Get("minSignal", 0.025f);
			droneProfileData.overheatFactor = p_data.Get("overheat", 0f);
			droneProfileData.pitchFF = p_data.Get("pitchFF", 0f);
			droneProfileData.rollFF = p_data.Get("rollFF", 0f);
			droneProfileData.yawFF = p_data.Get("yawFF", 0f);
			droneProfileData.betaflightVersion = p_data.Get("betaflightVersion", 40);
			droneProfileData.airmode = p_data.Get("airmode", d: true);
			droneProfileData.antigravity = p_data.Get("antigravity", d: true);
			droneProfileData.dynamicFilter = p_data.Get("dynamicFilter", d: true);
			droneProfileData.feedForwardTransition = p_data.Get("feedForwardTransition", (byte)100);
			droneProfileData.iTermRotation = p_data.Get("iTermRotation", d: true);
			droneProfileData.smartFeedForward = p_data.Get("smartFeedForward", d: false);
			droneProfileData.iTermRelax = p_data.Get("iTermRelax", (byte)0);
			droneProfileData.iTermRelaxValue = p_data.Get("iTermRelaxValue", (byte)11);
			droneProfileData.iTermRelaxType = p_data.Get("iTermRelaxType", (byte)1);
			droneProfileData.antigravityMode = p_data.Get("antigravityMode", (byte)0);
			droneProfileData.antigravityGain = p_data.Get("antigravityGain", (ushort)1000);
			return droneProfileData;
		}

		public string ToJson(bool p_indented = false)
		{
			return ToSerializedData().ToJson(p_indented);
		}

		public static DroneProfileData FromJson(string p_json)
		{
			string text = p_json;
			if (!string.IsNullOrEmpty(text) && !text.Trim().StartsWith("{"))
			{
				text = "{" + text;
			}
			if (!string.IsNullOrEmpty(text) && !text.Trim().EndsWith("}"))
			{
				text += "}";
			}
			SerializedData serializedData = SerializedData.FromJson<SerializedData>(text);
			if (serializedData != null)
			{
				string text2 = "pitchPID";
				JObject jObject = serializedData.Get<JObject>(text2, null);
				if (jObject == null)
				{
					serializedData.Remove(text2);
				}
				else
				{
					serializedData[text2] = jObject.ToObject<PIDVector>();
				}
				text2 = "rollPID";
				jObject = serializedData.Get<JObject>(text2, null);
				if (jObject == null)
				{
					serializedData.Remove(text2);
				}
				else
				{
					serializedData[text2] = jObject.ToObject<PIDVector>();
				}
				text2 = "yawPID";
				jObject = serializedData.Get<JObject>(text2, null);
				if (jObject == null)
				{
					serializedData.Remove(text2);
				}
				else
				{
					serializedData[text2] = jObject.ToObject<PIDVector>();
				}
				text2 = "levelPID";
				jObject = serializedData.Get<JObject>(text2, null);
				if (jObject == null)
				{
					serializedData.Remove(text2);
				}
				else
				{
					serializedData[text2] = jObject.ToObject<PIDVector>();
				}
				text2 = "pidCorrectionP";
				JArray jArray = serializedData.Get<JArray>(text2, null);
				if (jArray == null)
				{
					serializedData.Remove(text2);
				}
				else
				{
					serializedData[text2] = jArray.ToObject<float[]>();
				}
				text2 = "pidCorrectionR";
				jArray = serializedData.Get<JArray>(text2, null);
				if (jArray == null)
				{
					serializedData.Remove(text2);
				}
				else
				{
					serializedData[text2] = jArray.ToObject<float[]>();
				}
				text2 = "pidCorrectionY";
				jArray = serializedData.Get<JArray>(text2, null);
				if (jArray == null)
				{
					serializedData.Remove(text2);
				}
				else
				{
					serializedData[text2] = jArray.ToObject<float[]>();
				}
				text2 = "minSignal";
				float num = serializedData.Get(text2, 0.025f);
				serializedData[text2] = num;
				text2 = "pitchFF";
				num = serializedData.Get(text2, 0f);
				serializedData[text2] = num;
				text2 = "rollFF";
				num = serializedData.Get(text2, 0f);
				serializedData[text2] = num;
				text2 = "yawFF";
				num = serializedData.Get(text2, 0f);
				serializedData[text2] = num;
				text2 = "betaflightVersion";
				serializedData[text2] = serializedData.Get(text2, 40);
				text2 = "airmode";
				serializedData[text2] = serializedData.Get(text2, d: true);
				text2 = "antigravity";
				serializedData[text2] = serializedData.Get(text2, d: true);
				text2 = "dynamicFilter";
				serializedData[text2] = serializedData.Get(text2, d: true);
				text2 = "feedForwardTransition";
				serializedData[text2] = (byte)serializedData.Get(text2, 100);
				text2 = "iTermRotation";
				serializedData[text2] = serializedData.Get(text2, d: true);
				text2 = "smartFeedForward";
				serializedData[text2] = serializedData.Get(text2, d: false);
				text2 = "iTermRelax";
				serializedData[text2] = (byte)serializedData.Get(text2, 0);
				text2 = "iTermRelaxValue";
				serializedData[text2] = (byte)serializedData.Get(text2, 11);
				text2 = "iTermRelaxType";
				serializedData[text2] = (byte)serializedData.Get(text2, 1);
				text2 = "antigravityMode";
				serializedData[text2] = (byte)serializedData.Get(text2, 0);
				text2 = "antigravityGain";
				serializedData[text2] = (ushort)serializedData.Get(text2, 1000);
			}
			return FromSerializedData(serializedData);
		}

		public void SavePID()
		{
		}

		public DroneProfileData Clone()
		{
			DroneProfileData droneProfileData = ScriptableObject.CreateInstance<DroneProfileData>();
			droneProfileData.pitchPID = new PIDVector(pitchPID.p, pitchPID.i, pitchPID.d);
			droneProfileData.rollPID = new PIDVector(rollPID.p, rollPID.i, rollPID.d);
			droneProfileData.yawPID = new PIDVector(yawPID.p, yawPID.i, yawPID.d);
			droneProfileData.levelPID = new PIDVector(levelPID.p, levelPID.i, levelPID.d);
			droneProfileData.pidCorrectionP = (float[])pidCorrectionP.Clone();
			droneProfileData.pidCorrectionR = (float[])pidCorrectionR.Clone();
			droneProfileData.pidCorrectionY = (float[])pidCorrectionY.Clone();
			droneProfileData.minSignal = minSignal;
			droneProfileData.overheatFactor = overheatFactor;
			droneProfileData.pitchFF = pitchFF;
			droneProfileData.rollFF = rollFF;
			droneProfileData.yawFF = yawFF;
			droneProfileData.betaflightVersion = betaflightVersion;
			droneProfileData.airmode = airmode;
			droneProfileData.antigravity = antigravity;
			droneProfileData.dynamicFilter = dynamicFilter;
			droneProfileData.feedForwardTransition = feedForwardTransition;
			droneProfileData.iTermRotation = iTermRotation;
			droneProfileData.smartFeedForward = smartFeedForward;
			droneProfileData.iTermRelax = iTermRelax;
			droneProfileData.iTermRelaxValue = iTermRelaxValue;
			droneProfileData.iTermRelaxType = iTermRelaxType;
			droneProfileData.antigravityMode = antigravityMode;
			droneProfileData.antigravityGain = antigravityGain;
			return droneProfileData;
		}
	}
}
