using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class DroneFactory : MonoBehaviour
	{
		public AssetLibrary library;

		public Drone drone;

		public Drone playerDrone;

		private Coroutine AsyncInstantiator;

		private List<IEnumerator> AsyncInstances = new List<IEnumerator>();

		public Drone Instantiate(DroneRigData p_rig, Transform p_parent, bool p_async = true, bool p_isUser = false)
		{
			if (!this.drone)
			{
				Debug.LogError("DroneFactory> Failed to create Drone! No template available!");
				return null;
			}
			if (!library)
			{
				Debug.LogError("DroneFactory> Failed to create Drone! No library available!");
				return null;
			}
			p_rig.Validate();
			Transform parent = (p_parent ? p_parent : base.transform.parent);
			Drone drone = UnityEngine.Object.Instantiate((p_isUser && playerDrone != null) ? playerDrone : this.drone, parent);
			drone.gameObject.AddComponent<DroneBody>();
			drone.name = "drone_" + drone.body.guid.ToLower() + " (" + ((p_isUser && playerDrone != null) ? playerDrone : this.drone).name + ")";
			drone.rig = p_rig;
			drone.rigidbody.enabled = false;
			drone.gameObject.SetActive(value: false);
			List<string> parts = p_rig.parts;
			if (p_async)
			{
				AsyncInstances.Add(InstantiateAsync(drone, p_rig, parts));
				if (AsyncInstantiator == null)
				{
					AsyncInstantiator = StartCoroutine(AsyncInstantiatorRoutine());
				}
			}
			else
			{
				for (int i = 0; i < parts.Count; i++)
				{
					try
					{
						OnDronePartCreate(i, parts, drone);
					}
					catch (Exception ex)
					{
						Exception ex2 = ex;
						Exception ex3 = ex2;
						Activity.RunOnce(delegate
						{
							throw ex3;
						}, 0.01f);
					}
				}
				drone.gameObject.SetActive(value: true);
				drone.Initialize();
				if (p_rig.allowDynamicColor)
				{
					drone.renderer.color0 = p_rig.color0;
					drone.renderer.color1 = p_rig.color1;
					drone.renderer.color2 = p_rig.color2;
				}
			}
			return drone;
		}

		public IEnumerator AsyncInstantiatorRoutine()
		{
			yield return null;
			while (AsyncInstances.Count > 0)
			{
				while (AsyncInstances[0].MoveNext())
				{
					yield return null;
				}
				AsyncInstances.RemoveAt(0);
				yield return null;
			}
			AsyncInstantiator = null;
		}

		public IEnumerator InstantiateAsync(Drone new_drone, DroneRigData p_rig, List<string> pl)
		{
			yield return null;
			for (int i = 0; i < pl.Count; i++)
			{
				OnDronePartCreate(i, pl, new_drone);
				yield return null;
			}
			new_drone.gameObject.SetActive(value: true);
			yield return null;
			IEnumerator init = new_drone.InitializeAsync();
			while (init.MoveNext())
			{
				yield return null;
			}
			new_drone.renderer.color0 = p_rig.color0;
			new_drone.renderer.color1 = p_rig.color1;
			new_drone.renderer.color2 = p_rig.color2;
		}

		public Drone Instantiate(DroneRigData p_rig, bool p_async = true, bool p_isUser = false)
		{
			return Instantiate(p_rig, null, p_async, p_isUser);
		}

		public Drone InstantiateDummy(DroneRigData p_rig, Transform p_parent)
		{
			Drone obj = Instantiate(p_rig, p_parent, p_async: false);
			obj.SetEnabled(p_flag: false);
			obj.enabled = false;
			obj.SetMotorRPM(0f);
			obj.SetMotorSpinSpeed(0f);
			UnityEngine.Object.Destroy(obj.fc.gameObject);
			obj.MakeStatic(p_flag: true);
			return obj;
		}

		public Drone Replace(DroneRigData p_new, Drone p_old, Transform p_parent, Transform p_anchor, bool p_async = true)
		{
			Transform p = (p_old ? p_old.transform.parent : p_parent);
			if (!Application.isPlaying)
			{
				p_async = false;
			}
			bool p_isUser = (bool)p_old && p_old.hasThreaded;
			Drone d = Instantiate(p_new, p, p_async, p_isUser);
			d.rig = p_new;
			Transform transform = (p_anchor ? p_anchor : (p_old ? p_old.transform : null));
			Vector3 new_pos = (p_anchor ? p_anchor.position : (p_old ? p_old.position : Vector3.zero));
			Quaternion new_rot = (transform ? transform.transform.rotation : Quaternion.identity);
			Vector3 new_scl = Vector3.one;
			bool isKinematic = (bool)p_old && p_old.rigidbody.isKinematic;
			if ((bool)p_old)
			{
				p_old.MakeStatic(p_flag: true);
			}
			if (p_async)
			{
				Activity.Run((Func<bool>)delegate
				{
					if (!d)
					{
						return false;
					}
					if (!d.ready)
					{
						return true;
					}
					d.transform.parent = null;
					d.position = new_pos;
					d.transform.rotation = new_rot;
					d.transform.localScale = new_scl;
					d.transform.parent = p;
					if ((bool)d.renderer)
					{
						d.renderer.ClearTrails();
					}
					if ((bool)d.rigidbody)
					{
						d.rigidbody.isKinematic = isKinematic;
					}
					if ((bool)p_old)
					{
						foreach (DroneSimulation instance in DroneSimulation.instances)
						{
							instance.ReplaceDrone(p_old, d);
						}
						p_old.Destroy(p_async: true);
					}
					return false;
				}, 0f, false);
			}
			else if ((bool)d)
			{
				d.transform.parent = null;
				d.position = new_pos;
				d.transform.rotation = new_rot;
				d.transform.localScale = new_scl;
				d.transform.parent = p;
				if ((bool)d.renderer)
				{
					d.renderer.ClearTrails();
				}
				if ((bool)d.rigidbody)
				{
					d.rigidbody.isKinematic = isKinematic;
				}
				if ((bool)p_old)
				{
					foreach (DroneSimulation instance2 in DroneSimulation.instances)
					{
						instance2.ReplaceDrone(p_old, d);
					}
					p_old.Destroy(p_async: true);
				}
			}
			return d;
		}

		public Drone Replace(Drone p_new, Drone p_old, Transform p_parent, Transform p_anchor, bool p_async = true)
		{
			Transform p = (p_old ? p_old.transform.parent : p_parent);
			if (!Application.isPlaying)
			{
				p_async = false;
			}
			Drone d = UnityEngine.Object.Instantiate(p_new.gameObject, p).GetComponent<Drone>();
			d.rig = p_new.rig;
			Transform transform = (p_anchor ? p_anchor : (p_old ? p_old.transform : null));
			Vector3 new_pos = (p_anchor ? p_anchor.position : (p_old ? p_old.position : Vector3.zero));
			Quaternion new_rot = (transform ? transform.transform.rotation : Quaternion.identity);
			Vector3 new_scl = Vector3.one;
			bool isKinematic = (bool)p_old && p_old.rigidbody.isKinematic;
			if ((bool)p_old)
			{
				p_old.MakeStatic(p_flag: true);
			}
			if (p_async)
			{
				Activity.Run((Func<bool>)delegate
				{
					if (!d)
					{
						return false;
					}
					if (!d.ready)
					{
						return true;
					}
					d.transform.parent = null;
					d.position = new_pos;
					d.transform.rotation = new_rot;
					d.transform.localScale = new_scl;
					d.transform.parent = p;
					if ((bool)d.renderer)
					{
						d.renderer.ClearTrails();
					}
					if ((bool)d.rigidbody)
					{
						d.rigidbody.isKinematic = isKinematic;
					}
					if ((bool)p_old)
					{
						foreach (DroneSimulation instance in DroneSimulation.instances)
						{
							instance.ReplaceDrone(p_old, d);
						}
						p_old.Destroy(p_async: true);
					}
					return false;
				}, 0f, false);
			}
			else if ((bool)d)
			{
				d.transform.parent = null;
				d.position = new_pos;
				d.transform.rotation = new_rot;
				d.transform.localScale = new_scl;
				d.transform.parent = p;
				if ((bool)d.renderer)
				{
					d.renderer.ClearTrails();
				}
				if ((bool)d.rigidbody)
				{
					d.rigidbody.isKinematic = isKinematic;
				}
				if ((bool)p_old)
				{
					foreach (DroneSimulation instance2 in DroneSimulation.instances)
					{
						instance2.ReplaceDrone(p_old, d);
					}
					p_old.Destroy(p_async: true);
				}
			}
			return d;
		}

		public Drone Replace(DroneRigData p_new, Drone p_old, Transform p_parent, bool p_async = true)
		{
			return Replace(p_new, p_old, p_parent, null, p_async);
		}

		public Drone Replace(DroneRigData p_new, Drone p_old, bool p_async = true)
		{
			return Replace(p_new, p_old, null, p_async);
		}

		public Drone Instantiate(TextAsset p_rig_asset, bool p_async = true)
		{
			if (!p_rig_asset)
			{
				return null;
			}
			DroneRigData droneRigData = ScriptableObject.CreateInstance<DroneRigData>();
			droneRigData.Set(p_rig_asset.bytes);
			return Instantiate(droneRigData, p_async);
		}

		protected void OnDronePartCreate(int p_id, List<string> p_rig_parts, Drone p_drone)
		{
			if (p_drone == null || p_drone.gameObject == null || p_drone.transform == null)
			{
				throw new NullReferenceException("OnDronePartCreate: Drone GameObject missing");
			}
			if (p_rig_parts == null)
			{
				throw new NullReferenceException("OnDronePartCreate: Drone parts list missing");
			}
			string text = p_rig_parts[p_id];
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			if (library == null)
			{
				throw new NullReferenceException("OnDronePartCreate: Drone parts library missing");
			}
			DronePart dronePart = library.InstantiateByGUID<DronePart>(text, p_drone.transform);
			if (!dronePart)
			{
				dronePart = library.InstantiateByGUID<DronePart>(text.Substring(0, text.IndexOf('-')) + "-000", p_drone.transform);
			}
			if ((bool)dronePart)
			{
				if (dronePart == null || dronePart.gameObject == null || dronePart.transform == null)
				{
					throw new NullReferenceException("OnDronePartCreate: Drone part GameObject missing for part " + text);
				}
				dronePart.transform.localPosition = Vector3.zero;
				dronePart.transform.localScale = Vector3.one;
				dronePart.transform.localEulerAngles = Vector3.zero;
				dronePart.name = dronePart.name.Replace("(Clone)", "");
				if (dronePart is DroneFrame)
				{
					if (dronePart.info == null)
					{
						throw new NullReferenceException("OnDronePartCreate: Drone part info missing for part " + text);
					}
					p_drone.name = p_drone.name.Replace("drone_", dronePart.info.name.ToLower() + "_");
					Transform transform = dronePart.transform.Find("colliders");
					if ((bool)transform)
					{
						Hierarchy.Traverse(transform, delegate(BoxCollider p_it)
						{
							Transform transform2 = (p_it ? p_it.transform.parent : null);
							if ((bool)transform2 && transform2.name.Contains("motor-") && p_it.transform.GetSiblingIndex() > 0)
							{
								UnityEngine.Object.Destroy(p_it.gameObject);
							}
						});
					}
				}
				if (dronePart is DroneSkin)
				{
					dronePart.gameObject.SetActive(value: false);
				}
			}
			else
			{
				Debug.LogWarning("DroneFactory> OnDronePartCreate - [" + p_drone.name + "] Failed to find part [" + text + "]");
			}
		}

		public Drone UpdateRig(Drone p_drone, DroneRigData p_rig = null)
		{
			if (p_drone == null)
			{
				Debug.LogError("DroneFactory> UpdateRig - drone is null");
				return null;
			}
			if (p_rig != null)
			{
				p_drone.rig = p_rig;
			}
			if (p_drone.body.frame.guid != p_drone.rig.frame)
			{
				return Replace(p_drone.rig, p_drone, p_async: false);
			}
			if (p_drone.body.frame.batteries[0].guid != p_drone.rig.battery)
			{
				ReplacePart(p_drone, p_drone.rig.battery);
			}
			if (p_drone.body.frame.escs[0].motor.guid != p_drone.rig.motor)
			{
				ReplacePart(p_drone, p_drone.rig.motor);
			}
			if (p_drone.body.frame.escs[0].motor.prop.guid != p_drone.rig.prop)
			{
				ReplacePart(p_drone, p_drone.rig.prop);
			}
			if (p_drone.body.parts[3] == null || p_drone.body.parts[3].guid != p_drone.rig.antenna)
			{
				ReplacePart(p_drone, p_drone.rig.antenna);
			}
			if (p_drone.body.parts[7] == null || p_drone.body.parts[7].guid != p_drone.rig.attachment0)
			{
				ReplacePart(p_drone, p_drone.rig.attachment0);
			}
			if (p_drone.body.frame.escs[0].guid != p_drone.rig.esc)
			{
				ReplacePart(p_drone, p_drone.rig.esc);
			}
			return p_drone;
		}

		public Drone ReplacePart(Drone p_drone, string p_guid)
		{
			if (string.IsNullOrEmpty(p_guid))
			{
				Debug.LogWarning("DroneFactory> ReplacePart - guid is null or empty");
				return p_drone;
			}
			if (p_drone == null)
			{
				Debug.LogError("DroneFactory> ReplacePart - drone is null");
				return null;
			}
			DronePart dronePart = library.InstantiateByGUID<DronePart>(p_guid, p_drone.transform);
			if ((bool)dronePart)
			{
				dronePart.name = dronePart.name.Replace("(Clone)", "");
				if (dronePart is DroneFrame || dronePart is DroneRFCamera)
				{
					UnityEngine.Object.DestroyImmediate(dronePart.gameObject);
					DroneRigData rig = p_drone.rig;
					rig.frame = p_guid;
					return Replace(rig, p_drone, p_async: false);
				}
				if (dronePart is DroneAntennaTx)
				{
					SwapParts(p_drone.body.parts[3], dronePart, "node-antenna", p_drone);
					p_drone.body.parts[3] = dronePart;
				}
				else if (dronePart is DroneAttachment)
				{
					SwapParts(p_drone.body.parts[7], dronePart, "node-attachment", p_drone);
					p_drone.body.parts[7] = dronePart;
				}
				else if (dronePart is DroneBattery)
				{
					p_drone.body.frame.batteries[0] = (DroneBattery)dronePart;
					SwapParts(p_drone.body.parts[4], dronePart);
					p_drone.body.parts[4] = dronePart;
					for (int i = 0; i < p_drone.body.frame.escs.Count; i++)
					{
						p_drone.body.frame.escs[i].motor.Build();
					}
				}
				else if (dronePart is DroneProp)
				{
					DroneProp droneProp = (DroneProp)dronePart;
					for (int j = 0; j < p_drone.body.frame.escs.Count; j++)
					{
						DroneESC droneESC = p_drone.body.frame.escs[j];
						if (j > 0)
						{
							droneProp = UnityEngine.Object.Instantiate((DroneProp)dronePart, p_drone.transform);
						}
						else
						{
							p_drone.body.parts[2] = dronePart;
						}
						SwapParts(droneESC.motor.prop, droneProp);
						droneESC.motor.prop = droneProp;
						droneProp.motor = droneESC.motor;
						droneESC.motor.Build();
					}
					p_drone.fc.SetLayout(FrameLayoutType.QuadX);
				}
				else if (dronePart is DroneMotor)
				{
					p_drone.body.parts[1] = dronePart;
					for (int k = 0; k < p_drone.body.frame.escs.Count; k++)
					{
						DroneESC droneESC2 = p_drone.body.frame.escs[k];
						DroneProp prop = droneESC2.motor.prop;
						DroneMotor droneMotor = ((k != p_drone.body.frame.escs.Count - 1) ? UnityEngine.Object.Instantiate((DroneMotor)dronePart, p_drone.transform) : ((DroneMotor)dronePart));
						DroneMotorAnimation component = droneMotor.GetComponent<DroneMotorAnimation>();
						prop.motor = droneMotor;
						if (component.cap == null)
						{
							for (int l = 0; l < component.transform.childCount; l++)
							{
								if (component.transform.GetChild(l).name.EndsWith("cap"))
								{
									component.cap = component.transform.GetChild(l);
									break;
								}
							}
						}
						prop.transform.parent = component.cap.Find("node-propeller");
						prop.transform.localPosition = Vector3.zero;
						prop.transform.localScale = Vector3.one;
						prop.transform.parent = component.cap.transform;
						SwapParts(droneESC2.motor, droneMotor);
						droneESC2.motor = droneMotor;
						droneMotor.esc = droneESC2;
						droneMotor.Build();
					}
					p_drone.fc.SetLayout(FrameLayoutType.QuadX);
				}
				else if (dronePart is DroneESC)
				{
					DroneESC droneESC3 = (DroneESC)dronePart;
					for (int m = 0; m < p_drone.body.frame.escs.Count; m++)
					{
						DroneESC droneESC4 = p_drone.body.frame.escs[m];
						if (m > 0)
						{
							droneESC3 = UnityEngine.Object.Instantiate((DroneESC)dronePart, p_drone.transform);
						}
						else
						{
							p_drone.body.parts[5] = dronePart;
						}
						SwapParts(droneESC4, droneESC3);
						droneESC3.motor = droneESC4.motor;
						droneESC3.motor.esc = droneESC3;
						p_drone.body.frame.escs[m] = droneESC3;
						droneESC3.motor.Build();
					}
					p_drone.fc.SetLayout(FrameLayoutType.QuadX);
				}
				else if (dronePart is DroneSkin)
				{
					p_drone.body.LinkSkins();
				}
				else
				{
					dronePart.transform.localPosition = Vector3.zero;
					dronePart.transform.localScale = Vector3.one;
					dronePart.transform.localEulerAngles = Vector3.zero;
				}
			}
			else
			{
				Debug.LogWarning("DroneFactory> ReplacePart - [" + p_drone.name + "] Failed to find part [" + p_guid + "]");
			}
			p_drone.renderer.Build();
			p_drone.body.RecalculateWeight();
			p_drone.body.UpdateBatteryHooks();
			p_drone.body.UpdateColliders();
			return p_drone;
		}

		private void SwapParts(DronePart p_old, DronePart p_new, string p_node, Drone p_drone)
		{
			if (p_new == null || p_new.transform == null)
			{
				return;
			}
			if (p_old == null || p_old.transform == null)
			{
				Transform partNode = p_drone.body.frame.GetPartNode(p_node);
				if (!(partNode == null))
				{
					p_new.transform.parent = partNode.parent;
					p_new.transform.localPosition = partNode.localPosition;
					p_new.transform.rotation = Quaternion.LookRotation(-partNode.up, partNode.forward);
				}
			}
			else
			{
				SwapParts(p_old, p_new);
			}
		}

		private void SwapParts(DronePart p_old, DronePart p_new)
		{
			if (!(p_old == null) && !(p_new == null) && !(p_old.transform == null) && !(p_new.transform == null))
			{
				p_new.transform.parent = p_old.transform.parent;
				p_new.transform.localPosition = p_old.transform.localPosition;
				p_new.transform.localScale = p_old.transform.localScale;
				p_new.transform.localRotation = p_old.transform.localRotation;
				p_old.transform.parent = null;
				p_old.gameObject.SetActive(value: false);
				UnityEngine.Object.Destroy(p_old.gameObject);
			}
		}
	}
}
