using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	[CreateAssetMenu(fileName = "name.physics.asset", menuName = "DRL/DronePhysicsData")]
	public class DronePhysicsData : ScriptableObject
	{
		public enum AerodynamicsModelType
		{
			Legacy = 0,
			Traditional = 1,
			GATech = 2
		}

		private static List<DronePhysicsData> m_pool = new List<DronePhysicsData>();

		public bool threaded;

		public int threadTargetFrequency = 2000;

		public float gravity = 9.81f;

		public float airDensity = 1.225f;

		[NonSerialized]
		public float gravityScale;

		public float efficiency;

		public float efficiencyMax = 0.85f;

		public float efficiencyZero = 1.1f;

		public bool legacyDrag;

		public float ClMin = 0.2f;

		public float ClMax = 0.8f;

		public float CdMin = 0.8f;

		public float CdMax = 1.6f;

		public float surfaceArea = 0.03f;

		public float dragScale = -1f;

		public float liftScale = -1f;

		public float sideScale = -1f;

		public float gravityFactor = 5f;

		public float groundEffectStrength = 0.3f;

		public float groundeffectDistance = 0.3f;

		public float thrust;

		public float torque;

		public float mass;

		public bool torqueBoost;

		public float torqueBoostWeight = 1f;

		public float torqueBoostBalance = 0.2f;

		public bool overrideSpinup;

		public float spinupTime = 0.05f;

		public float spindownTime = 0.01f;

		public bool advancedPropLimits = true;

		public float maxTipSpeed = 0.85f;

		public float propDragFactor;

		public bool batterySag = true;

		public bool batteryDrain;

		public float batteryCapacity = 1f;

		public float batteryResistance = 1f;

		public bool arcadePhysics;

		public bool linearTorque = true;

		public bool linearThrust = true;

		public bool realisticTorque;

		public bool correctRates;

		public float overrideAirmode;

		public float inertia;

		public float arcing;

		public bool useCOG;

		public bool isLocked;

		public AerodynamicsModelType aerodynamicsType = AerodynamicsModelType.Traditional;

		private AeroModel m_aerodynamics;

		public string aerodynamicsData;

		private GATechLookupData m_currentDragData;

		[NonSerialized]
		public bool gatechDataAvailable;

		public bool gatechUseCrossflow = true;

		public bool gatechUseUnsteady = true;

		public bool gatechUseShedding = true;

		public bool overrideMaxSpeed;

		public float maxSpeedOverride = 45f;

		private SerializedData m_data;

		public AeroModel aerodynamics
		{
			get
			{
				if (m_aerodynamics == null)
				{
					m_aerodynamics = new AeroModelTraditional();
					aerodynamicsType = AerodynamicsModelType.Traditional;
				}
				return m_aerodynamics;
			}
		}

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

		public static DronePhysicsData GetPool()
		{
			DronePhysicsData dronePhysicsData = null;
			if (m_pool.Count <= 0)
			{
				dronePhysicsData = ScriptableObject.CreateInstance<DronePhysicsData>();
			}
			else
			{
				dronePhysicsData = m_pool[0];
				m_pool.RemoveAt(0);
			}
			return dronePhysicsData;
		}

		public static void SetPool(DronePhysicsData p_data)
		{
			if (!m_pool.Contains(p_data))
			{
				m_pool.Add(p_data);
			}
		}

		public static void PoolWarmup(int p_count)
		{
			while (m_pool.Count < p_count)
			{
				m_pool.Add(ScriptableObject.CreateInstance<DronePhysicsData>());
			}
		}

		public static void ClearPool()
		{
			for (int i = 0; i < m_pool.Count; i++)
			{
				UnityEngine.Object.Destroy(m_pool[i]);
			}
			m_pool.Clear();
		}

		public static float DefaultArcing(string p_frame)
		{
			if (p_frame == "F-c2d")
			{
				return 1f;
			}
			return 0.75f;
		}

		public static float DefaultInertia(string p_frame)
		{
			if (p_frame == "F-c2d")
			{
				return 1f;
			}
			return 1.15f;
		}

		public void SetAerodynamics(AerodynamicsModelType p_type, GATechLookupData p_data = null, string p_frame = null)
		{
			if (p_type == aerodynamicsType && m_aerodynamics != null && (p_type != AerodynamicsModelType.GATech || p_data == m_currentDragData))
			{
				return;
			}
			aerodynamicsType = p_type;
			switch (p_type)
			{
			case AerodynamicsModelType.Traditional:
				m_aerodynamics = new AeroModelTraditional();
				break;
			case AerodynamicsModelType.GATech:
				if (p_data == null)
				{
					Debug.LogError("DronePhysicsData> \"GATech\" aerodynamics type requires a dataset.");
					aerodynamicsType = AerodynamicsModelType.Traditional;
				}
				else
				{
					m_aerodynamics = new AeroModelGATech(p_data);
				}
				break;
			case AerodynamicsModelType.Legacy:
				Debug.LogError("DronePhysicsData> \"Legacy\" aerodynamics type not defined.");
				break;
			}
		}

		public bool IsEqual(DronePhysicsData p_other)
		{
			if (p_other == null)
			{
				return false;
			}
			if (threaded != p_other.threaded)
			{
				return false;
			}
			if (!Mathf.Approximately(threadTargetFrequency, p_other.threadTargetFrequency))
			{
				return false;
			}
			if (!Mathf.Approximately(gravity, p_other.gravity))
			{
				return false;
			}
			if (!Mathf.Approximately(airDensity, p_other.airDensity) && (airDensity != 0f || !Mathf.Approximately(p_other.airDensity, 1.225f)) && (p_other.airDensity != 0f || !Mathf.Approximately(airDensity, 1.225f)))
			{
				return false;
			}
			if (!Mathf.Approximately(efficiency, p_other.efficiency))
			{
				return false;
			}
			if (efficiency <= 0f)
			{
				if (!Mathf.Approximately(efficiencyMax, p_other.efficiencyMax))
				{
					return false;
				}
				if (!Mathf.Approximately(efficiencyZero, p_other.efficiencyZero))
				{
					return false;
				}
			}
			if (legacyDrag != p_other.legacyDrag)
			{
				return false;
			}
			if (!Mathf.Approximately(ClMin, p_other.ClMin))
			{
				return false;
			}
			if (!Mathf.Approximately(ClMax, p_other.ClMax))
			{
				return false;
			}
			if (!Mathf.Approximately(CdMin, p_other.CdMin))
			{
				return false;
			}
			if (!Mathf.Approximately(CdMax, p_other.CdMax))
			{
				return false;
			}
			if (!Mathf.Approximately(surfaceArea, p_other.surfaceArea))
			{
				return false;
			}
			if (!Mathf.Approximately(gravityFactor, p_other.gravityFactor))
			{
				return false;
			}
			if (!Mathf.Approximately(groundEffectStrength, p_other.groundEffectStrength))
			{
				return false;
			}
			if (!Mathf.Approximately(groundeffectDistance, p_other.groundeffectDistance))
			{
				return false;
			}
			if (!Mathf.Approximately(thrust, p_other.thrust))
			{
				return false;
			}
			if (!Mathf.Approximately(torque, p_other.torque))
			{
				return false;
			}
			if (!Mathf.Approximately(mass, p_other.mass))
			{
				return false;
			}
			if (torqueBoost != p_other.torqueBoost)
			{
				return false;
			}
			if (!Mathf.Approximately(torqueBoostWeight, p_other.torqueBoostWeight))
			{
				return false;
			}
			if (!Mathf.Approximately(torqueBoostBalance, p_other.torqueBoostBalance))
			{
				return false;
			}
			if (overrideSpinup != p_other.overrideSpinup)
			{
				return false;
			}
			if (!Mathf.Approximately(spinupTime, p_other.spinupTime))
			{
				return false;
			}
			if (!Mathf.Approximately(spindownTime, p_other.spindownTime))
			{
				return false;
			}
			if (advancedPropLimits != p_other.advancedPropLimits)
			{
				return false;
			}
			if (!Mathf.Approximately(maxTipSpeed, p_other.maxTipSpeed))
			{
				return false;
			}
			if (!Mathf.Approximately(propDragFactor, p_other.propDragFactor))
			{
				return false;
			}
			if (batterySag != p_other.batterySag)
			{
				return false;
			}
			if (batteryDrain != p_other.batteryDrain)
			{
				return false;
			}
			if (!Mathf.Approximately(batteryCapacity, p_other.batteryCapacity))
			{
				return false;
			}
			if (!Mathf.Approximately(batteryResistance, p_other.batteryResistance))
			{
				return false;
			}
			if (arcadePhysics != p_other.arcadePhysics)
			{
				return false;
			}
			if (linearTorque != p_other.linearTorque)
			{
				return false;
			}
			if (linearThrust != p_other.linearThrust)
			{
				return false;
			}
			if (realisticTorque != p_other.realisticTorque)
			{
				return false;
			}
			if (correctRates != p_other.correctRates)
			{
				return false;
			}
			if (!Mathf.Approximately(overrideAirmode, p_other.overrideAirmode))
			{
				return false;
			}
			if (gatechDataAvailable && aerodynamicsType != p_other.aerodynamicsType)
			{
				return false;
			}
			if (legacyDrag != p_other.legacyDrag)
			{
				return false;
			}
			if (!Mathf.Approximately(surfaceArea, p_other.surfaceArea))
			{
				return false;
			}
			if (aerodynamicsType == AerodynamicsModelType.GATech)
			{
				if (gatechUseCrossflow != p_other.gatechUseCrossflow)
				{
					return false;
				}
				if (gatechUseUnsteady != p_other.gatechUseUnsteady)
				{
					return false;
				}
				if (gatechUseShedding != p_other.gatechUseShedding)
				{
					return false;
				}
				if (string.IsNullOrEmpty(aerodynamicsData) != string.IsNullOrEmpty(p_other.aerodynamicsData))
				{
					return false;
				}
				if (aerodynamicsData != p_other.aerodynamicsData)
				{
					return false;
				}
				if (!Mathf.Approximately(inertia, p_other.inertia))
				{
					return false;
				}
				if (!Mathf.Approximately(arcing, p_other.arcing))
				{
					return false;
				}
			}
			else
			{
				if (!Mathf.Approximately(ClMin, p_other.ClMin))
				{
					return false;
				}
				if (!Mathf.Approximately(ClMax, p_other.ClMax))
				{
					return false;
				}
				if (!Mathf.Approximately(CdMin, p_other.CdMin))
				{
					return false;
				}
				if (!Mathf.Approximately(CdMax, p_other.CdMax))
				{
					return false;
				}
			}
			if (useCOG != p_other.useCOG)
			{
				return false;
			}
			return true;
		}

		public SerializedData ToSerializedData()
		{
			data.Set("threaded", threaded);
			data.Set("threadTargetFrequency", threadTargetFrequency);
			data.Set("gravity", gravity);
			data.Set("airDensity", airDensity);
			data.Set("efficiency", efficiency);
			data.Set("efficiencyMax", efficiencyMax);
			data.Set("efficiencyZero", efficiencyZero);
			data.Set("legacyDrag", legacyDrag);
			data.Set("ClMin", ClMin);
			data.Set("ClMax", ClMax);
			data.Set("CdMin", CdMin);
			data.Set("CdMax", CdMax);
			data.Set("surfaceArea", surfaceArea);
			data.Set("dragScale", dragScale);
			data.Set("liftScale", liftScale);
			data.Set("sideScale", sideScale);
			data.Set("gravityFactor", gravityFactor);
			data.Set("groundEffectStrength", groundEffectStrength);
			data.Set("groundeffectDistance", groundeffectDistance);
			data.Set("thrust", thrust);
			data.Set("torque", torque);
			data.Set("mass", mass);
			data.Set("torqueBoost", torqueBoost);
			data.Set("torqueBoostWeight", torqueBoostWeight);
			data.Set("torqueBoostBalance", torqueBoostBalance);
			data.Set("overrideSpinup", overrideSpinup);
			data.Set("spinupTime", spinupTime);
			data.Set("spindownTime", spindownTime);
			data.Set("advancedPropLimits", advancedPropLimits);
			data.Set("maxTipSpeed", maxTipSpeed);
			data.Set("propDragFactor", propDragFactor);
			data.Set("batterySag", batterySag);
			data.Set("batteryDrain", batteryDrain);
			data.Set("batteryCapacity", batteryCapacity);
			data.Set("batteryResistance", batteryResistance);
			data.Set("arcadePhysics", arcadePhysics);
			data.Set("linearTorque", linearTorque);
			data.Set("linearThrust", linearThrust);
			data.Set("realisticTorque", realisticTorque);
			data.Set("correctRates", correctRates);
			data.Set("overrideAirmode", overrideAirmode);
			data.Set("aerodynamics", aerodynamicsType);
			data.Set("gatechUseCrossflow", gatechUseCrossflow);
			data.Set("gatechUseShedding", gatechUseShedding);
			data.Set("gatechUseUnsteady", gatechUseUnsteady);
			data.Set("inertia", inertia);
			data.Set("arcing", arcing);
			data.Set("useCOG", useCOG);
			data.Set("locked", isLocked);
			data.Set("aerodynamicsData", aerodynamicsData);
			data.Set("overrideMaxSpeed", overrideMaxSpeed);
			data.Set("maxSpeedOverride", maxSpeedOverride);
			return data;
		}

		public static DronePhysicsData FromSerializedData(SerializedData p_data)
		{
			DronePhysicsData pool = GetPool();
			pool.gravity = p_data.Get("gravity", 9.81f);
			pool.airDensity = p_data.Get("airDensity", 1.225f);
			pool.threaded = p_data.Get("threaded", d: false);
			pool.threadTargetFrequency = p_data.Get("threadTargetFrequency", 2000);
			pool.efficiency = p_data.Get("efficiency", 0f);
			pool.efficiencyMax = p_data.Get("efficiencyMax", 0.85f);
			pool.efficiencyZero = p_data.Get("efficiencyZero", 1.1f);
			pool.legacyDrag = p_data.Get("legacyDrag", d: false);
			pool.ClMin = p_data.Get("ClMin", 0.2f);
			pool.ClMax = p_data.Get("ClMax", 0.8f);
			pool.CdMin = p_data.Get("CdMin", 0.8f);
			pool.CdMax = p_data.Get("CdMax", 1.6f);
			pool.surfaceArea = p_data.Get("surfaceArea", 0.03f);
			pool.dragScale = p_data.Get("dragScale", -1f);
			pool.liftScale = p_data.Get("liftScale", -1f);
			pool.sideScale = p_data.Get("sideScale", -1f);
			pool.gravityFactor = p_data.Get("gravityFactor", 5f);
			pool.groundEffectStrength = p_data.Get("groundEffectStrength", 0.3f);
			pool.groundeffectDistance = p_data.Get("groundeffectDistance", 0.3f);
			pool.thrust = p_data.Get("thrust", 0f);
			pool.torque = p_data.Get("torque", 0f);
			pool.mass = p_data.Get("mass", 0f);
			pool.torqueBoost = p_data.Get("torqueBoost", d: false);
			pool.torqueBoostWeight = p_data.Get("torqueBoostWeight", 1f);
			pool.torqueBoostBalance = p_data.Get("torqueBoostBalance", 0.2f);
			pool.overrideSpinup = p_data.Get("overrideSpinup", d: false);
			pool.spinupTime = p_data.Get("spinupTime", 0.05f);
			pool.spindownTime = p_data.Get("spindownTime", 0.01f);
			pool.advancedPropLimits = p_data.Get("advancedPropLimits", d: true);
			pool.maxTipSpeed = p_data.Get("maxTipSpeed", 0.85f);
			pool.propDragFactor = p_data.Get("propDragFactor", 0f);
			pool.batterySag = p_data.Get("batterySag", d: true);
			pool.batteryDrain = p_data.Get("batteryDrain", d: false);
			pool.batteryCapacity = p_data.Get("batteryCapacity", 1f);
			pool.batteryResistance = p_data.Get("batteryResistance", 1f);
			pool.arcadePhysics = p_data.Get("arcadePhysics", d: false);
			pool.linearTorque = p_data.Get("linearTorque", d: true);
			pool.linearThrust = p_data.Get("linearThrust", d: true);
			pool.realisticTorque = p_data.Get("realisticTorque", d: true);
			pool.correctRates = p_data.Get("correctRates", d: false);
			pool.overrideAirmode = p_data.Get("overrideAirmode", 0f);
			switch (p_data.Get("aerodynamics", 1))
			{
			case 0:
				pool.aerodynamicsType = AerodynamicsModelType.Legacy;
				break;
			case 1:
				pool.aerodynamicsType = AerodynamicsModelType.Traditional;
				break;
			case 2:
				pool.aerodynamicsType = AerodynamicsModelType.GATech;
				break;
			default:
				pool.aerodynamicsType = AerodynamicsModelType.Traditional;
				break;
			}
			pool.gatechUseCrossflow = p_data.Get("gatechUseCrossflow", d: true);
			pool.gatechUseShedding = p_data.Get("gatechUseShedding", d: true);
			pool.gatechUseUnsteady = p_data.Get("gatechUseUnsteady", d: true);
			pool.inertia = p_data.Get("inertia", 1f);
			pool.arcing = p_data.Get("arcing", 0f);
			pool.isLocked = p_data.Get("locked", d: false);
			pool.aerodynamicsData = p_data.Get<string>("aerodynamicsData", null);
			pool.useCOG = p_data.Get("useCOG", d: false);
			pool.overrideMaxSpeed = p_data.Get("overrideMaxSpeed", d: false);
			pool.maxSpeedOverride = p_data.Get("maxSpeedOverride", 45f);
			return pool;
		}

		public string ToJson(bool p_indented = false)
		{
			return ToSerializedData().ToJson(p_indented);
		}

		public static DronePhysicsData FromJson(string p_json)
		{
			return FromSerializedData(SerializedData.FromJson<SerializedData>(p_json));
		}

		public DronePhysicsData Clone()
		{
			DronePhysicsData dronePhysicsData = ScriptableObject.CreateInstance<DronePhysicsData>();
			dronePhysicsData.threaded = threaded;
			dronePhysicsData.threadTargetFrequency = threadTargetFrequency;
			dronePhysicsData.gravity = gravity;
			dronePhysicsData.airDensity = airDensity;
			dronePhysicsData.efficiency = efficiency;
			dronePhysicsData.efficiencyMax = efficiencyMax;
			dronePhysicsData.efficiencyZero = efficiencyZero;
			dronePhysicsData.legacyDrag = legacyDrag;
			dronePhysicsData.ClMin = ClMin;
			dronePhysicsData.ClMax = ClMax;
			dronePhysicsData.CdMin = CdMin;
			dronePhysicsData.CdMax = CdMax;
			dronePhysicsData.surfaceArea = surfaceArea;
			dronePhysicsData.dragScale = dragScale;
			dronePhysicsData.liftScale = liftScale;
			dronePhysicsData.sideScale = sideScale;
			dronePhysicsData.gravityFactor = gravityFactor;
			dronePhysicsData.groundEffectStrength = groundEffectStrength;
			dronePhysicsData.groundeffectDistance = groundeffectDistance;
			dronePhysicsData.thrust = thrust;
			dronePhysicsData.torque = torque;
			dronePhysicsData.mass = mass;
			dronePhysicsData.torqueBoost = torqueBoost;
			dronePhysicsData.torqueBoostWeight = torqueBoostWeight;
			dronePhysicsData.torqueBoostBalance = torqueBoostBalance;
			dronePhysicsData.overrideSpinup = overrideSpinup;
			dronePhysicsData.spinupTime = spinupTime;
			dronePhysicsData.spindownTime = spindownTime;
			dronePhysicsData.advancedPropLimits = advancedPropLimits;
			dronePhysicsData.maxTipSpeed = maxTipSpeed;
			dronePhysicsData.propDragFactor = propDragFactor;
			dronePhysicsData.batterySag = batterySag;
			dronePhysicsData.batteryDrain = batteryDrain;
			dronePhysicsData.batteryCapacity = batteryCapacity;
			dronePhysicsData.batteryResistance = batteryResistance;
			dronePhysicsData.arcadePhysics = arcadePhysics;
			dronePhysicsData.linearTorque = linearTorque;
			dronePhysicsData.linearThrust = linearThrust;
			dronePhysicsData.realisticTorque = realisticTorque;
			dronePhysicsData.correctRates = correctRates;
			dronePhysicsData.overrideAirmode = overrideAirmode;
			dronePhysicsData.aerodynamicsType = aerodynamicsType;
			dronePhysicsData.inertia = inertia;
			dronePhysicsData.arcing = arcing;
			dronePhysicsData.m_aerodynamics = m_aerodynamics;
			dronePhysicsData.gatechUseCrossflow = gatechUseCrossflow;
			dronePhysicsData.gatechUseShedding = gatechUseShedding;
			dronePhysicsData.gatechUseUnsteady = gatechUseUnsteady;
			dronePhysicsData.isLocked = isLocked;
			dronePhysicsData.aerodynamicsData = aerodynamicsData;
			dronePhysicsData.useCOG = useCOG;
			dronePhysicsData.overrideMaxSpeed = overrideMaxSpeed;
			dronePhysicsData.maxSpeedOverride = maxSpeedOverride;
			return dronePhysicsData;
		}
	}
}
