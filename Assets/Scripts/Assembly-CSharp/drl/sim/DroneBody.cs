using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class DroneBody : DronePart
	{
		public Collider[] frameColliders;

		private Vector3 m_centerOfMass;

		private bool m_cogInitialized;

		private Transform m_com;

		private bool m_hasCOMM;

		[SerializeField]
		private DroneFrame m_frame;

		private bool m_hasFrame;

		private DronePart[] m_parts;

		public bool hasAtlas;

		public List<Texture> atlases;

		public DroneBatteryPlacement frameBatPts;

		public DroneBatteryPlacement batteryPts;

		private bool batteryBelowTheFrame;

		private float m_rightIntMinX;

		private float m_leftIntMaxX;

		private static string[] m_skin_material_properties = new string[14]
		{
			"_SkinEnabled", "_SkinAlbedoTex", "_SkinMaskTex", "_SkinAnimated", "_SkinAnimBlendMode", "_RampReplaceColors", "_RampAlphaAsEmission", "_MetallicOverride", "_SkinSpeedXMin", "_SkinSpeedXMax",
			"_SkinSpeedYMin", "_SkinSpeedYMax", "_SkinRampTex", "_SkinEffectTex"
		};

		private List<Collider> m_landingGear;

		public const float landingGearRadius = 0.005f;

		public const float landingGearOffset = 0.25f;

		private Transform battery;

		private BoxCollider batteryCollider;

		private Mesh batteryMesh;

		private MeshFilter batteryMeshFilter;

		private MeshRenderer batteryMeshRenderer;

		private List<BoxCollider> boxColliders = new List<BoxCollider>();

		public Vector3 centerOfMass
		{
			get
			{
				if (!m_cogInitialized)
				{
					RecalculateWeight();
				}
				return m_centerOfMass;
			}
		}

		public Transform centerOfMassMarker
		{
			get
			{
				if (m_hasCOMM)
				{
					return m_com;
				}
				if (m_com == null)
				{
					m_com = new GameObject("center-of-mass").transform;
					m_com.parent = base.drone.transform;
					m_com.localPosition = Vector3.zero;
					m_com.localRotation = Quaternion.identity;
				}
				m_hasCOMM = true;
				return m_com;
			}
		}

		public DroneFrame frame
		{
			get
			{
				if (m_hasFrame)
				{
					return m_frame;
				}
				if ((bool)m_frame)
				{
					m_hasFrame = true;
					return m_frame;
				}
				m_frame = GetComponentInChildren<DroneFrame>();
				if ((bool)m_frame)
				{
					m_hasFrame = true;
					return m_frame;
				}
				return null;
			}
			set
			{
				m_frame = value;
				m_hasFrame = m_frame != null;
			}
		}

		public bool hasFrame => m_hasFrame;

		public DronePart[] parts
		{
			get
			{
				if (m_parts == null || m_parts.Length < 8)
				{
					m_parts = new DronePart[8]
					{
						GetComponentInChildren<DroneFrame>(),
						GetComponentInChildren<DroneMotor>(),
						GetComponentInChildren<DroneProp>(),
						GetComponentInChildren<DroneAntennaTx>(),
						GetComponentInChildren<DroneBattery>(),
						GetComponentInChildren<DroneESC>(),
						GetComponentInChildren<DroneRFCamera>(),
						GetComponentInChildren<DroneAttachment>()
					};
				}
				return m_parts;
			}
		}

		public List<Collider> landingGear
		{
			get
			{
				if (m_landingGear == null)
				{
					m_landingGear = new List<Collider>();
				}
				return m_landingGear;
			}
		}

		public Transform GetRigColliderContainer()
		{
			Transform transform = frame.transform.Find("colliders");
			if ((bool)transform)
			{
				transform = transform.Find("rig");
			}
			return transform;
		}

		protected override void OnInitialize()
		{
		}

		public void Build()
		{
			if (!frame)
			{
				Debug.LogWarning("DroneBody> Failed to find Frame for [" + GetPath() + "]");
				return;
			}
			RefreshNodes();
			frame.Initialize();
			frame.RefreshNodes();
			hasAtlas = false;
			atlases = new List<Texture>();
			LinkParts();
			UpdateBatteryHooks();
			InitializeColliders();
			RecalculateWeight();
			Validate();
		}

		public bool Validate()
		{
			bool flag = true;
			flag &= (bool)frame;
			if (!flag)
			{
				Debug.LogError("DroneBody.Validate> Missing 'frame' at [" + Hierarchy.Path(base.transform) + "]");
			}
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>(includeInactive: true);
			foreach (Collider collider in componentsInChildren)
			{
				Vector3 localScale = collider.transform.localScale;
				if (float.IsNaN(localScale.x))
				{
					flag = false;
					localScale.x = 0.001f;
					Debug.LogError("DroneBody.Validate> collider scale.x=NaN on " + collider.name + " of " + base.name);
				}
				if (float.IsNaN(localScale.y))
				{
					flag = false;
					localScale.y = 0.001f;
					Debug.LogError("DroneBody.Validate> collider scale.y=NaN on " + collider.name + " of " + base.name);
				}
				if (float.IsNaN(localScale.z))
				{
					flag = false;
					localScale.z = 0.001f;
					Debug.LogError("DroneBody.Validate> collider scale.z=NaN on " + collider.name + " of " + base.name);
				}
				if (localScale.x == 0f)
				{
					flag = false;
					localScale.x = 0.001f;
					Debug.LogError("DroneBody.Validate> collider scale.x=0 on " + collider.name + " of " + base.name);
				}
				if (localScale.y == 0f)
				{
					flag = false;
					localScale.y = 0.001f;
					Debug.LogError("DroneBody.Validate> collider scale.y=0 on " + collider.name + " of " + base.name);
				}
				if (localScale.z == 0f)
				{
					flag = false;
					localScale.z = 0.001f;
					Debug.LogError("DroneBody.Validate> collider scale.z=0 on " + collider.name + " of " + base.name);
				}
			}
			return flag;
		}

		public void GenerateAtlas()
		{
			if (hasAtlas)
			{
				return;
			}
			Material sharedMaterial = frame.GetComponentInChildren<MeshRenderer>().sharedMaterial;
			sharedMaterial = UnityEngine.Object.Instantiate(sharedMaterial);
			sharedMaterial.name = sharedMaterial.name.Replace("(Clone)", "") + "-copy-batch";
			Material original = ((frame.escs.Count <= 0) ? sharedMaterial : (frame.escs[0].motor ? frame.escs[0].motor.prop.GetComponentInChildren<MeshRenderer>().sharedMaterial : sharedMaterial));
			original = UnityEngine.Object.Instantiate(original);
			original.name = original.name.Replace("(Clone)", "") + "-alpha-copy-batch";
			List<Texture2D> list = new List<Texture2D>();
			List<Texture2D> list2 = new List<Texture2D>();
			List<Texture2D> list3 = new List<Texture2D>();
			List<Texture2D> list4 = new List<Texture2D>();
			DronePart[] array = parts;
			MeshRenderer[] array2 = new MeshRenderer[8]
			{
				array[0] ? array[0].GetComponentInChildren<MeshRenderer>() : null,
				array[1] ? array[1].GetComponentInChildren<MeshRenderer>() : null,
				array[2] ? array[2].GetComponentInChildren<MeshRenderer>() : null,
				array[3] ? array[3].GetComponentInChildren<MeshRenderer>() : null,
				array[4] ? array[4].GetComponentInChildren<MeshRenderer>() : null,
				array[5] ? array[5].GetComponentInChildren<MeshRenderer>() : null,
				array[6] ? array[6].GetComponentInChildren<MeshRenderer>() : null,
				array[7] ? array[7].GetComponentInChildren<MeshRenderer>() : null
			};
			foreach (MeshRenderer p_target in array2)
			{
				Material material = FindMaterial(p_target, "atlas");
				Texture2D texture2D = (material ? ((Texture2D)material.GetTexture("_MainTex")) : null);
				list.Add(texture2D ? texture2D : Texture2D.blackTexture);
				texture2D = (material ? ((Texture2D)material.GetTexture("_MOETex")) : null);
				list2.Add(texture2D ? texture2D : Texture2D.blackTexture);
				texture2D = (material ? ((Texture2D)material.GetTexture("_MasksTex")) : null);
				list3.Add(texture2D ? texture2D : Texture2D.blackTexture);
				texture2D = (material ? ((Texture2D)material.GetTexture("_NormalTex")) : null);
				list4.Add(texture2D ? texture2D : Texture2D.blackTexture);
			}
			List<MeshRenderer> list5 = Hierarchy.FindAll<MeshRenderer>(base.transform);
			Texture texture = DroneAtlasCapture.GenerateAtlas(p_is_normal: false, list.ToArray());
			Texture texture2 = DroneAtlasCapture.GenerateAtlas(p_is_normal: false, list2.ToArray());
			Texture texture3 = DroneAtlasCapture.GenerateAtlas(p_is_normal: false, list3.ToArray());
			Texture texture4 = DroneAtlasCapture.GenerateAtlas(p_is_normal: true, list4.ToArray());
			atlases.Add(texture);
			atlases.Add(texture2);
			atlases.Add(texture3);
			atlases.Add(texture4);
			sharedMaterial.SetTexture("_MainTex", texture);
			sharedMaterial.SetTexture("_MOETex", texture2);
			sharedMaterial.SetTexture("_MasksTex", texture3);
			sharedMaterial.SetTexture("_NormalTex", texture4);
			int num = 0;
			for (int j = 0; j < list5.Count; j++)
			{
				MeshRenderer meshRenderer = list5[j];
				Material material2 = sharedMaterial;
				if (meshRenderer.sharedMaterial.name.IndexOf("propeller") >= 0)
				{
					material2 = UnityEngine.Object.Instantiate(original);
					material2.name = material2.name.Replace("(Clone)", "-copy-p" + num++);
				}
				SetMaterial(meshRenderer, "atlas", material2);
			}
			if (original != sharedMaterial)
			{
				UnityEngine.Object.Destroy(original);
			}
			hasAtlas = true;
		}

		public void LinkSkins()
		{
			if (!frame)
			{
				return;
			}
			List<DroneSkin> list = Hierarchy.FindAll<DroneSkin>(base.transform);
			Material sharedMaterial = frame.GetComponentInChildren<MeshRenderer>().sharedMaterial;
			if (!sharedMaterial)
			{
				Debug.LogWarning($"DroneBody> LinkSkins / Failed to fetch Material from [{frame}]");
				return;
			}
			DroneAssetTag[] array = (frame ? frame.GetComponentsInChildren<DroneAssetTag>() : new DroneAssetTag[0]);
			List<Material> list2 = new List<Material>();
			base.drone.renderer._skin = list2.ToArray();
			for (int i = 0; i < list.Count; i++)
			{
				DroneSkin droneSkin = list[i];
				if (droneSkin.category == DroneAssetTagType.Frame)
				{
					if (!frame)
					{
						return;
					}
					foreach (DroneAssetTag droneAssetTag in array)
					{
						if (droneAssetTag.label != "skin" || !droneAssetTag.Contains(droneSkin.category))
						{
							continue;
						}
						sharedMaterial = ((!(droneAssetTag.GetComponent<MeshRenderer>() != null)) ? droneAssetTag.GetComponent<SkinnedMeshRenderer>().sharedMaterial : droneAssetTag.GetComponent<MeshRenderer>().sharedMaterial);
						if ((bool)droneSkin.material)
						{
							for (int k = 0; k < m_skin_material_properties.Length; k++)
							{
								string text = m_skin_material_properties[k];
								if (!sharedMaterial.HasProperty(text))
								{
									Debug.LogWarning("DroneBody> LinkSkins / Skin [" + sharedMaterial.name + "] does not contain [" + text + "]");
									continue;
								}
								switch (text)
								{
								case "_SkinEnabled":
								case "_SkinAnimated":
								case "_SkinAnimBlendMode":
								case "_RampReplaceColors":
								case "_RampAlphaAsEmission":
								case "_MetallicOverride":
								case "_SkinSpeedXMin":
								case "_SkinSpeedXMax":
								case "_SkinSpeedYMin":
								case "_SkinSpeedYMax":
									sharedMaterial.SetFloat(text, droneSkin.material.GetFloat(text));
									goto IL_03af;
								case "_SkinAlbedoTex":
								case "_SkinMaskTex":
								case "_SkinRampTex":
								case "_SkinEffectTex":
									sharedMaterial.SetTexture(text, droneSkin.material.GetTexture(text));
									goto IL_03af;
								case null:
									continue;
									IL_03af:
									if (text == null)
									{
										continue;
									}
									break;
								}
								switch (text)
								{
								case "_SkinAnimBlendMode":
								case "_RampReplaceColors":
								case "_RampAlphaAsEmission":
								case "_MetallicOverride":
								case "_SkinEnabled":
								case "_SkinAnimated":
								{
									bool flag = ((droneSkin.material.GetFloat(text) >= 1f) ? true : false);
									List<string> list3 = new List<string>(sharedMaterial.shaderKeywords);
									string text2 = "";
									string text3 = "";
									switch (text)
									{
									case "_SkinAnimBlendMode":
										text2 = (flag ? "_SKINANIMBLENDMODE_ALPHABLEND" : "_SKINANIMBLENDMODE_ADDITIVE");
										text3 = ((!flag) ? "_SKINANIMBLENDMODE_ALPHABLEND" : "_SKINANIMBLENDMODE_ADDITIVE");
										break;
									case "_SkinEnabled":
										text2 = (flag ? "SKIN_ENABLED" : "");
										text3 = ((!flag) ? "SKIN_ENABLED" : "");
										break;
									case "_SkinAnimated":
										text2 = (flag ? "SKIN_ANIMATED" : "");
										text3 = ((!flag) ? "SKIN_ANIMATED" : "");
										break;
									case "_RampReplaceColors":
										text2 = (flag ? "SKIN_RAMP_REPLACE_COLORS" : "");
										text3 = ((!flag) ? "SKIN_RAMP_REPLACE_COLORS" : "");
										break;
									case "_RampAlphaAsEmission":
										text2 = (flag ? "SKIN_RAMP_ALPHA_AS_EMISSION" : "");
										text3 = ((!flag) ? "SKIN_RAMP_ALPHA_AS_EMISSION" : "");
										break;
									case "_MetallicOverride":
										text2 = (flag ? "SKIN_METALLIC_OVERRIDE" : "");
										text3 = ((!flag) ? "SKIN_METALLIC_OVERRIDE" : "");
										break;
									}
									if (!string.IsNullOrEmpty(text2) && !list3.Contains(text2))
									{
										list3.Add(text2);
									}
									if (!string.IsNullOrEmpty(text3) && list3.Contains(text3))
									{
										list3.Remove(text3);
									}
									sharedMaterial.shaderKeywords = list3.ToArray();
									break;
								}
								}
							}
						}
						else
						{
							Debug.LogWarning("DroneBody> LinkSkins / Skin [" + droneSkin.guid + "] missing Material reference.");
						}
						list2.Add(sharedMaterial);
					}
				}
				UnityEngine.Object.Destroy(droneSkin.gameObject);
			}
			base.drone.renderer._skin = list2.ToArray();
		}

		public void LinkParts()
		{
			if (!frame)
			{
				Debug.LogWarning("DroneBody> Failed to find Frame for [" + GetPath() + "]");
				return;
			}
			DroneTrail droneTrail = Hierarchy.Find<DroneTrail>(base.transform);
			if ((bool)droneTrail)
			{
				UnityEngine.Object.Destroy(droneTrail.gameObject, 0.2f);
			}
			for (int i = 0; i < nodes.Count; i++)
			{
				Transform transform = nodes[i];
				Vector3 localPosition = transform.localPosition;
				_ = transform.localRotation;
				Transform parent = transform.transform.parent;
				string text2;
				string text = (text2 = transform.name);
				bool flag = false;
				DronePart dronePart = null;
				if (text.IndexOf("node-esc") == 0)
				{
					text2 = "node-esc";
				}
				if (text.IndexOf("node-motor") == 0)
				{
					text2 = "node-motor";
				}
				if (text.IndexOf("node-battery") == 0)
				{
					text2 = "node-battery";
				}
				if (text.IndexOf("node-attachment") == 0)
				{
					text2 = "node-attachment";
				}
				if (text.IndexOf("node-trail") == 0)
				{
					text2 = "node-trail";
				}
				switch (text2)
				{
				case "node-antenna":
					dronePart = GetComponentInChildren<DroneAntennaTx>();
					if ((bool)dronePart)
					{
						flag = true;
					}
					break;
				case "node-camera":
					dronePart = GetComponentInChildren<DroneRFCamera>();
					if ((bool)dronePart)
					{
						frame.camera = (DroneRFCamera)dronePart;
						frame.camera.Build();
						flag = true;
					}
					break;
				case "node-attachment":
					dronePart = GetComponentInChildren<DroneAttachment>();
					if ((bool)dronePart)
					{
						flag = true;
					}
					break;
				case "node-esc":
				{
					int nodeId = GetNodeId(transform);
					if (nodeId < 0)
					{
						Debug.LogWarning("DroneBody> Failed to parse ESC node!");
						continue;
					}
					DroneESC droneESC = ((nodeId <= 0) ? GetComponentInChildren<DroneESC>() : ((frame.escs != null && frame.escs.Count > 0 && frame.escs[0] != null) ? UnityEngine.Object.Instantiate(frame.escs[0], base.transform) : null));
					if ((bool)droneESC)
					{
						dronePart = droneESC;
						droneESC.name = droneESC.name.Replace("(Clone)", "");
						frame.escs[nodeId] = droneESC;
						flag = true;
					}
					break;
				}
				case "node-battery":
				{
					int nodeId2 = GetNodeId(transform);
					if (nodeId2 < 0)
					{
						Debug.LogWarning("DroneBody> Failed to parse Battery node!");
						continue;
					}
					DroneBattery droneBattery = ((nodeId2 <= 0) ? GetComponentInChildren<DroneBattery>() : ((frame.escs != null && frame.batteries.Count > 0 && frame.batteries[0] != null) ? UnityEngine.Object.Instantiate(frame.batteries[0], base.transform) : null));
					if (!droneBattery)
					{
						Debug.LogWarning("DroneBody> Failed to create Battery instance!");
						break;
					}
					dronePart = droneBattery;
					droneBattery.name = droneBattery.name.Replace("(Clone)", "");
					droneBattery.CacheCellCurves();
					frame.batteries[nodeId2] = droneBattery;
					flag = true;
					break;
				}
				case "node-motor":
				{
					int nodeId3 = GetNodeId(transform);
					if (nodeId3 < 0)
					{
						Debug.LogWarning("DroneBody> Failed to parse Motor node!");
						continue;
					}
					DroneMotor droneMotor = ((nodeId3 <= 0) ? GetComponentInChildren<DroneMotor>() : ((frame.escs != null && frame.escs.Count > 0 && frame.escs[0] != null && frame.escs[0].motor != null) ? UnityEngine.Object.Instantiate(frame.escs[0].motor, base.transform) : null));
					if (!droneMotor)
					{
						Debug.LogWarning("DroneBody> Failed to create Motor instance!");
						break;
					}
					droneMotor.name = droneMotor.name.Replace("(Clone)", "");
					droneMotor.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
					dronePart = droneMotor;
					if (frame.escs != null && frame.escs.Count > nodeId3 && frame.escs[nodeId3] != null)
					{
						frame.escs[nodeId3].motor = droneMotor;
					}
					droneMotor.Initialize();
					flag = true;
					break;
				}
				case "node-trail":
				{
					if (!droneTrail)
					{
						continue;
					}
					if (GetNodeId(transform) < 0)
					{
						Debug.LogWarning("DroneBody> Failed to parse Trail node!");
						continue;
					}
					DroneTrail droneTrail2 = UnityEngine.Object.Instantiate(droneTrail, base.transform);
					droneTrail2.name = droneTrail2.name.Replace("(Clone)", "");
					dronePart = droneTrail2;
					droneTrail2.Initialize();
					base.drone.renderer.trails.Add(droneTrail2);
					flag = true;
					break;
				}
				}
				if (flag && (bool)dronePart)
				{
					dronePart.transform.parent = parent;
					dronePart.transform.localPosition = localPosition;
					dronePart.transform.rotation = Quaternion.LookRotation(-transform.up, transform.forward);
				}
			}
			int count = frame.escs.Count;
			for (int j = 0; j < count; j++)
			{
				DroneESC droneESC2 = frame.escs[j];
				if (!droneESC2)
				{
					continue;
				}
				droneESC2.name = droneESC2.name + "-" + j;
				DroneMotor motor = droneESC2.motor;
				if (!motor)
				{
					continue;
				}
				motor.name = motor.name + "-" + j;
				DroneProp droneProp = ((j <= 0) ? GetComponentInChildren<DroneProp>() : frame.escs[0].motor.prop);
				if ((bool)droneProp)
				{
					if (j > 0)
					{
						droneProp = UnityEngine.Object.Instantiate(droneProp, frame.transform);
					}
					droneProp.name = droneProp.name.Replace("(Clone)", "");
					DroneProp droneProp2 = droneProp;
					droneProp2.name = droneProp2.name + "-" + j;
					Transform transform2 = motor.FindNode("node-propeller");
					if ((bool)transform2)
					{
						droneProp.transform.parent = transform2.parent;
						droneProp.transform.localPosition = transform2.localPosition;
						droneProp.transform.rotation = Quaternion.LookRotation(-transform2.up, transform2.forward);
						droneProp.motor = motor;
						motor.Build();
						UnityEngine.Object.Destroy(transform2.gameObject);
					}
				}
			}
			for (int k = 0; k < frame.batteries.Count; k++)
			{
				if (frame.batteries[k] != null)
				{
					DroneBattery droneBattery2 = frame.batteries[k];
					droneBattery2.name = droneBattery2.name + "-" + k;
				}
			}
			if (!(base.drone.renderer != null) || base.drone.renderer.trails == null)
			{
				return;
			}
			for (int l = 0; l < base.drone.renderer.trails.Count; l++)
			{
				if (base.drone.renderer.trails[l] != null)
				{
					DroneTrail droneTrail3 = base.drone.renderer.trails[l];
					droneTrail3.name = droneTrail3.name + "-" + l;
				}
			}
		}

		public void RecalculateWeight()
		{
			m_cogInitialized = true;
			weight = 0f;
			Hierarchy.Traverse(base.transform, (Action<DronePart>)IterateDronePartsWeight, true, false);
			if (frame != null)
			{
				weight += frame.extraWeight;
			}
			if (!frame)
			{
				m_centerOfMass = Vector3.zero;
				return;
			}
			Vector3 position = base.drone.position;
			frame.transform.localPosition = Vector3.zero;
			if (frame.batteryBeneath)
			{
				Transform transform = frame.batteries[0].transform.Find("guides");
				if ((bool)transform)
				{
					DroneBatteryPlacement component = transform.GetComponent<DroneBatteryPlacement>();
					Vector3 vector = frame.transform.InverseTransformPoint(component.strapExtLeft.position);
					if (vector.y < 0f)
					{
						frame.transform.localPosition = new Vector3(0f, 0f - vector.y, 0f);
					}
				}
			}
			if (base.drone.rig.name.Contains("Nikko") || base.drone.rig.name.Contains("intro") || base.drone.body.frame.guid == "F-285")
			{
				centerOfMassMarker.parent = base.drone.transform;
				centerOfMassMarker.localScale = Vector3.one;
				centerOfMassMarker.localPosition = frame.transform.localPosition;
				centerOfMassMarker.localRotation = Quaternion.identity;
				base.drone.rootOffset = Vector3.zero;
				base.drone.StabilizeDroneOnGround(p_flag: true);
				return;
			}
			m_centerOfMass = Vector3.zero;
			int num = 1;
			foreach (DroneESC esc in frame.escs)
			{
				if ((bool)esc && (bool)esc.motor)
				{
					m_centerOfMass = Vector3.Lerp(m_centerOfMass, esc.motor.transform.position, 1f / (float)num++);
				}
			}
			m_centerOfMass = Vector3.Lerp(m_centerOfMass, frame.transform.position, 0.5f);
			float num2 = frame.weight + frame.extraWeight;
			foreach (DroneESC esc2 in frame.escs)
			{
				if (esc2 == null || esc2.motor == null || esc2.motor.prop == null)
				{
					Debug.LogError("DroneBody:: unable to calculate center of mass");
					return;
				}
				num2 += esc2.weight;
				Collider componentInChildren = esc2.GetComponentInChildren<Collider>(includeInactive: true);
				if (componentInChildren != null)
				{
					bool flag = componentInChildren.enabled;
					componentInChildren.enabled = true;
					m_centerOfMass = Vector3.Lerp(m_centerOfMass, componentInChildren.bounds.center, esc2.weight / num2);
					Transform obj = new GameObject("center").transform;
					obj.parent = componentInChildren.transform;
					obj.position = componentInChildren.bounds.center;
					componentInChildren.enabled = flag;
				}
				else
				{
					m_centerOfMass = Vector3.Lerp(m_centerOfMass, esc2.transform.position, esc2.weight / num2);
				}
				num2 += esc2.motor.weight + esc2.motor.prop.weight;
				Collider componentInChildren2 = esc2.motor.GetComponentInChildren<Collider>(includeInactive: true);
				if (componentInChildren2 != null)
				{
					bool flag2 = componentInChildren2.enabled;
					componentInChildren2.enabled = true;
					m_centerOfMass = Vector3.Lerp(m_centerOfMass, componentInChildren2.bounds.center, (esc2.motor.weight + esc2.motor.prop.weight) / num2);
					Transform obj2 = new GameObject("center").transform;
					obj2.parent = componentInChildren2.transform;
					obj2.position = componentInChildren2.bounds.center;
					componentInChildren2.enabled = flag2;
				}
				m_centerOfMass = Vector3.Lerp(m_centerOfMass, esc2.motor.transform.position, (esc2.motor.weight + esc2.motor.prop.weight) / num2);
			}
			foreach (DroneBattery battery in frame.batteries)
			{
				if (battery == null)
				{
					Debug.LogError("DroneBody:: unable to calculate center of mass");
					return;
				}
				num2 += battery.weight;
				Collider componentInChildren3 = battery.GetComponentInChildren<Collider>(includeInactive: true);
				if (componentInChildren3 != null)
				{
					bool flag3 = componentInChildren3.enabled;
					componentInChildren3.enabled = true;
					m_centerOfMass = Vector3.Lerp(m_centerOfMass, componentInChildren3.bounds.center, battery.weight / num2);
					Transform obj3 = new GameObject("center").transform;
					obj3.parent = componentInChildren3.transform;
					obj3.position = componentInChildren3.bounds.center;
					componentInChildren3.enabled = flag3;
				}
				else
				{
					m_centerOfMass = Vector3.Lerp(m_centerOfMass, battery.transform.position, battery.weight / num2);
				}
			}
			DroneAttachment[] componentsInChildren = frame.GetComponentsInChildren<DroneAttachment>();
			foreach (DroneAttachment droneAttachment in componentsInChildren)
			{
				if (droneAttachment == null)
				{
					Debug.LogError("DroneBody:: unable to calculate center of mass");
					return;
				}
				num2 += droneAttachment.weight;
				Collider componentInChildren4 = droneAttachment.GetComponentInChildren<Collider>(includeInactive: true);
				if (componentInChildren4 != null)
				{
					bool flag4 = componentInChildren4.enabled;
					componentInChildren4.enabled = true;
					m_centerOfMass = Vector3.Lerp(m_centerOfMass, componentInChildren4.bounds.center, droneAttachment.weight / num2);
					Transform obj4 = new GameObject("center").transform;
					obj4.parent = componentInChildren4.transform;
					obj4.position = componentInChildren4.bounds.center;
					componentInChildren4.enabled = flag4;
				}
				else
				{
					m_centerOfMass = Vector3.Lerp(m_centerOfMass, droneAttachment.transform.position, droneAttachment.weight / num2);
				}
			}
			if (frame.camera == null)
			{
				Debug.LogError("DroneBody:: unable to calculate center of mass");
				return;
			}
			num2 += frame.camera.weight;
			Collider componentInChildren5 = frame.camera.GetComponentInChildren<Collider>(includeInactive: true);
			if (componentInChildren5 != null)
			{
				bool flag5 = componentInChildren5.enabled;
				componentInChildren5.enabled = true;
				m_centerOfMass = Vector3.Lerp(m_centerOfMass, componentInChildren5.bounds.center, frame.camera.weight / num2);
				Transform obj5 = new GameObject("center").transform;
				obj5.parent = componentInChildren5.transform;
				obj5.position = componentInChildren5.bounds.center;
				componentInChildren5.enabled = flag5;
			}
			else
			{
				m_centerOfMass = Vector3.Lerp(m_centerOfMass, frame.camera.transform.position, frame.camera.weight / num2);
			}
			m_centerOfMass = base.transform.InverseTransformPoint(m_centerOfMass);
			m_centerOfMass.x = 0f;
			Transform transform2 = centerOfMassMarker;
			transform2.parent = base.drone.transform;
			transform2.localScale = Vector3.one;
			transform2.localPosition = m_centerOfMass;
			transform2.localRotation = Quaternion.identity;
			base.drone.rootOffset = new Vector3(0f, transform2.localPosition.y, 0f);
			frame.transform.localPosition -= base.drone.rootOffset;
			transform2.localPosition = Vector3.zero;
			base.drone.position = position;
			base.drone.StabilizeDroneOnGround(p_flag: true);
		}

		public void SetLandingGear(bool p_flags)
		{
			UpdateColliders();
			for (int i = 0; i < landingGear.Count; i++)
			{
				if (landingGear[i] == null)
				{
					landingGear.RemoveAt(i--);
				}
				else
				{
					landingGear[i].enabled = p_flags;
				}
			}
		}

		public void InitializeColliders()
		{
			if (frame == null || frame.transform == null)
			{
				return;
			}
			Transform transform = frame.transform.Find("render");
			if ((bool)transform)
			{
				Collider[] componentsInChildren = transform.GetComponentsInChildren<Collider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = false;
				}
			}
			Transform transform2 = frame.transform.Find("colliders");
			if ((bool)transform2)
			{
				transform2 = transform2.Find("rig");
			}
			if ((bool)transform2)
			{
				transform2 = transform2.Find("body");
			}
			battery = ((transform2 != null) ? transform2.Find("battery-0") : null);
			if ((bool)battery)
			{
				if (frame.batteries == null || frame.batteries.Count == 0 || frame.batteries[0] == null)
				{
					return;
				}
				Transform transform3 = frame.batteries[0].transform.Find("meshes");
				if (!transform3)
				{
					return;
				}
				batteryMeshFilter = transform3.GetComponentInChildren<MeshFilter>();
				batteryMeshRenderer = batteryMeshFilter.transform.GetComponent<MeshRenderer>();
				batteryCollider = battery.GetComponent<BoxCollider>();
				if (batteryMeshFilter == null || batteryCollider == null || batteryMeshRenderer == null)
				{
					return;
				}
				batteryMesh = batteryMeshFilter.mesh;
				if (batteryMesh == null)
				{
					return;
				}
				batteryCollider.center = battery.InverseTransformPoint(batteryMeshRenderer.bounds.center);
				Vector3 size = battery.InverseTransformVector(batteryMeshFilter.transform.TransformVector(batteryMesh.bounds.size));
				size.x = Mathf.Abs(size.x);
				size.y = Mathf.Abs(size.y);
				size.z = Mathf.Abs(size.z);
				batteryCollider.size = size;
			}
			for (int j = 0; j < landingGear.Count; j++)
			{
				if (landingGear[j] != null)
				{
					UnityEngine.Object.Destroy(landingGear[j]);
				}
			}
			landingGear.Clear();
			if (!transform2)
			{
				return;
			}
			boxColliders.Clear();
			boxColliders = transform2.GetComponentsInChildren<BoxCollider>().ToList();
			for (int k = 0; k < boxColliders.Count; k++)
			{
				BoxCollider boxCollider = boxColliders[k];
				if (boxCollider.name.StartsWith("arm"))
				{
					SphereCollider sphereCollider = boxCollider.gameObject.AddComponent<SphereCollider>();
					sphereCollider.radius = 0.005f;
					sphereCollider.center = new Vector3(boxCollider.center.x - boxCollider.size.x * 0.5f, boxCollider.center.y - boxCollider.size.y * 0.5f, boxCollider.center.z - boxCollider.size.z * 0.5f + 0.00125f);
					landingGear.Add(sphereCollider);
					sphereCollider = boxCollider.gameObject.AddComponent<SphereCollider>();
					sphereCollider.radius = 0.005f;
					sphereCollider.center = new Vector3(boxCollider.center.x - boxCollider.size.x * 0.5f, boxCollider.center.y + boxCollider.size.y * 0.5f, boxCollider.center.z - boxCollider.size.z * 0.5f + 0.00125f);
					landingGear.Add(sphereCollider);
				}
				else if (!boxCollider.name.StartsWith("battery"))
				{
					SphereCollider sphereCollider2 = boxCollider.gameObject.AddComponent<SphereCollider>();
					sphereCollider2.radius = 0.005f;
					sphereCollider2.center = new Vector3(boxCollider.center.x - boxCollider.size.x * 0.5f, boxCollider.center.y - boxCollider.size.y * 0.5f + 0.00125f, boxCollider.center.z + boxCollider.size.z * 0.5f);
					landingGear.Add(sphereCollider2);
					sphereCollider2 = boxCollider.gameObject.AddComponent<SphereCollider>();
					sphereCollider2.radius = 0.005f;
					sphereCollider2.center = new Vector3(boxCollider.center.x + boxCollider.size.x * 0.5f, boxCollider.center.y - boxCollider.size.y * 0.5f + 0.00125f, boxCollider.center.z + boxCollider.size.z * 0.5f);
					landingGear.Add(sphereCollider2);
					sphereCollider2 = boxCollider.gameObject.AddComponent<SphereCollider>();
					sphereCollider2.radius = 0.005f;
					sphereCollider2.center = new Vector3(boxCollider.center.x + boxCollider.size.x * 0.5f, boxCollider.center.y - boxCollider.size.y * 0.5f + 0.00125f, boxCollider.center.z - boxCollider.size.z * 0.5f);
					landingGear.Add(sphereCollider2);
					sphereCollider2 = boxCollider.gameObject.AddComponent<SphereCollider>();
					sphereCollider2.radius = 0.005f;
					sphereCollider2.center = new Vector3(boxCollider.center.x - boxCollider.size.x * 0.5f, boxCollider.center.y - boxCollider.size.y * 0.5f + 0.00125f, boxCollider.center.z - boxCollider.size.z * 0.5f);
					landingGear.Add(sphereCollider2);
				}
			}
		}

		public void UpdateColliders()
		{
			if ((bool)battery && (bool)batteryCollider && (bool)batteryMesh && (bool)batteryMeshFilter && (bool)batteryMeshRenderer)
			{
				batteryCollider.center = battery.InverseTransformPoint(batteryMeshRenderer.bounds.center);
				Vector3 size = battery.InverseTransformVector(batteryMeshFilter.transform.TransformVector(batteryMesh.bounds.size));
				size.x = Mathf.Abs(size.x);
				size.y = Mathf.Abs(size.y);
				size.z = Mathf.Abs(size.z);
				batteryCollider.size = size;
			}
			if (landingGear == null || landingGear.Count == 0)
			{
				InitializeColliders();
			}
		}

		public void UpdateBatteryHooks()
		{
			Transform transform = frame.transform.Find("render");
			if (transform == null)
			{
				Debug.LogWarning("DroneBody> Could not find render");
				return;
			}
			if (transform.childCount == 0)
			{
				transform = frame.transform.GetChild(transform.GetSiblingIndex() + 1);
			}
			Transform transform2 = transform.Find(frame.name + "-helpers");
			if (frame.batteries == null || frame.batteries.Count == 0 || frame.batteries[0] == null)
			{
				Debug.LogWarning("DroneBody> Could not find battery");
				return;
			}
			Transform transform3 = frame.batteries[0].transform;
			if (transform2 == null)
			{
				Debug.LogWarning("DroneBody> Could not find helpers");
				return;
			}
			if (transform3 == null)
			{
				Debug.LogWarning("DroneBody> Could not find battery");
				return;
			}
			frameBatPts = transform2.GetComponent<DroneBatteryPlacement>();
			Transform transform4 = transform3.Find("guides");
			if (frameBatPts == null)
			{
				Debug.LogWarning("DroneBody> Could not find DroneBatteryPlacement on frame");
				return;
			}
			if (transform4 == null)
			{
				Debug.LogWarning("DroneBody> Could not find guides node on battery");
				return;
			}
			batteryPts = transform4.GetComponent<DroneBatteryPlacement>();
			if (batteryPts == null)
			{
				Debug.LogWarning("DroneBody> Could not find batteryHooks on battery");
				return;
			}
			if (frameBatPts.centralize && batteryPts.center != null)
			{
				Transform partNode = frame.GetPartNode("node-battery-0");
				Transform parent = batteryPts.center.parent;
				batteryPts.center.parent = transform;
				transform3.parent = batteryPts.center;
				batteryPts.center.localPosition = partNode.localPosition;
				transform3.parent = transform;
				batteryPts.center.parent = parent;
			}
			if ((bool)frameBatPts.strapIntLeft)
			{
				m_leftIntMaxX = frameBatPts.strapIntLeft.localPosition.x;
			}
			if ((bool)frameBatPts.strapIntRight)
			{
				m_rightIntMinX = frameBatPts.strapExtRight.localPosition.x;
			}
			if (frameBatPts.swapLeftAndRight)
			{
				if ((bool)frameBatPts.strapExtLeft)
				{
					Vector3 vector = frameBatPts.strapExtLeft.parent.InverseTransformPoint(batteryPts.strapExtRight.position);
					frameBatPts.strapExtLeft.localPosition = new Vector3(vector.x, frameBatPts.strapExtLeft.localPosition.y, vector.z);
				}
				if ((bool)frameBatPts.strapExtRight)
				{
					Vector3 vector2 = frameBatPts.strapExtRight.parent.InverseTransformPoint(batteryPts.strapExtLeft.position);
					frameBatPts.strapExtRight.localPosition = new Vector3(vector2.x, frameBatPts.strapExtRight.localPosition.y, vector2.z);
				}
				if ((bool)frameBatPts.strapIntLeft)
				{
					Vector3 vector3 = frameBatPts.strapIntLeft.parent.InverseTransformPoint(batteryPts.strapIntRight.position);
					frameBatPts.strapIntLeft.localPosition = new Vector3(Mathf.Clamp(vector3.x, -1f, m_leftIntMaxX), frameBatPts.strapIntLeft.localPosition.y, vector3.z);
				}
				if ((bool)frameBatPts.strapIntRight)
				{
					Vector3 vector4 = frameBatPts.strapIntRight.parent.InverseTransformPoint(batteryPts.strapIntLeft.position);
					frameBatPts.strapIntRight.localPosition = new Vector3(Mathf.Clamp(vector4.x, m_rightIntMinX, 1f), frameBatPts.strapIntRight.localPosition.y, vector4.z);
				}
			}
			else
			{
				if ((bool)frameBatPts.strapExtLeft)
				{
					Vector3 vector5 = frameBatPts.strapExtLeft.parent.InverseTransformPoint(batteryPts.strapExtLeft.position);
					frameBatPts.strapExtLeft.localPosition = new Vector3(vector5.x, frameBatPts.strapExtLeft.localPosition.y, vector5.z);
				}
				if ((bool)frameBatPts.strapExtRight)
				{
					Vector3 vector6 = frameBatPts.strapExtRight.parent.InverseTransformPoint(batteryPts.strapExtRight.position);
					frameBatPts.strapExtRight.localPosition = new Vector3(vector6.x, frameBatPts.strapExtRight.localPosition.y, vector6.z);
				}
				if ((bool)frameBatPts.strapIntLeft)
				{
					Vector3 vector7 = frameBatPts.strapIntLeft.parent.InverseTransformPoint(batteryPts.strapIntLeft.position);
					frameBatPts.strapIntLeft.localPosition = new Vector3(Mathf.Clamp(vector7.x, -1f, m_leftIntMaxX), frameBatPts.strapIntRight.localPosition.y, vector7.z);
				}
				if ((bool)frameBatPts.strapIntRight)
				{
					Vector3 vector8 = frameBatPts.strapIntRight.parent.InverseTransformPoint(batteryPts.strapIntRight.position);
					frameBatPts.strapIntRight.localPosition = new Vector3(Mathf.Clamp(vector8.x, m_rightIntMinX, 1f), frameBatPts.strapIntRight.localPosition.y, vector8.z);
				}
			}
			if ((bool)frameBatPts.strapExtCenter)
			{
				Vector3 vector9 = frameBatPts.strapExtCenter.parent.InverseTransformPoint(batteryPts.strapExtLeft.position);
				Vector3 vector10 = frameBatPts.strapExtCenter.parent.InverseTransformPoint(batteryPts.strapExtRight.position);
				Vector3 vector11 = (vector9 + vector10) / 2f;
				frameBatPts.strapExtCenter.localPosition = new Vector3(vector11.x, frameBatPts.strapExtCenter.localPosition.y, vector11.z);
			}
			frameBatPts.cables.position = batteryPts.cables.position;
		}

		protected void OnDestroy()
		{
			if (atlases != null)
			{
				for (int i = 0; i < atlases.Count; i++)
				{
					DroneAtlasCapture.Restore(atlases[i]);
				}
				atlases.Clear();
				if (m_com != null)
				{
					UnityEngine.Object.Destroy(m_com.gameObject);
				}
			}
		}

		private void IterateDronePartsWeight(DronePart it)
		{
			weight += it.weight;
		}

		private Material FindMaterial(Renderer p_target, string p_substr)
		{
			if (!p_target)
			{
				return null;
			}
			Material[] sharedMaterials = p_target.sharedMaterials;
			if (sharedMaterials.Length == 0)
			{
				return null;
			}
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				if (sharedMaterials[i].name.IndexOf(p_substr) >= 0)
				{
					return sharedMaterials[i];
				}
			}
			return null;
		}

		private void SetMaterial(Renderer p_target, string p_substr, Material p_material)
		{
			if (!p_target)
			{
				return;
			}
			Material[] sharedMaterials = p_target.sharedMaterials;
			if (sharedMaterials.Length == 0)
			{
				return;
			}
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				if (sharedMaterials[i].name.IndexOf(p_substr) >= 0)
				{
					if (sharedMaterials[i].name.IndexOf("-copy") >= 0)
					{
						UnityEngine.Object.Destroy(sharedMaterials[i]);
					}
					sharedMaterials[i] = p_material;
					break;
				}
			}
			p_target.sharedMaterials = sharedMaterials;
		}
	}
}
