using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MESceneView : View<DRLApp>
	{
		public static int RaycastFlags = DRLLayerFlag.EditorMapBit | DRLLayerFlag.EditorRaceBit | DRLLayerFlag.EditorSelectionBit | DRLLayerFlag.CollisionBit | DRLLayerFlag.TerrainBit;

		public bool debug;

		public List<MAEntity> hierarchy;

		private Dictionary<GameObject, Dictionary<Type, Component>> m_gccache;

		private Vector3[] m_gbrl = new Vector3[8]
		{
			Vector3.zero,
			Vector3.zero,
			Vector3.zero,
			Vector3.zero,
			Vector3.zero,
			Vector3.zero,
			Vector3.zero,
			Vector3.zero
		};

		private static Dictionary<string, int> idcache;

		private Dictionary<MAGuide, bool> m_gvt;

		public MapEditorView editor => AssertParent<MapEditorView>("editor");

		public MAEntity root => editor.model.root;

		internal Dictionary<GameObject, Dictionary<Type, Component>> gccache
		{
			get
			{
				if (m_gccache != null)
				{
					return m_gccache;
				}
				return m_gccache = new Dictionary<GameObject, Dictionary<Type, Component>>();
			}
		}

		public List<T> SelectRectangle<T>(Rect p_screen_rect) where T : Component
		{
			Camera main = editor.camera.main;
			List<T> list = new List<T>();
			bool flag = typeof(T) == typeof(MAEntity);
			for (int i = 0; i < hierarchy.Count; i++)
			{
				MAEntity mAEntity = hierarchy[i];
				if (!mAEntity)
				{
					hierarchy.RemoveAt(i--);
				}
				else if (mAEntity.HasHit(p_screen_rect, main))
				{
					T item = (flag ? ((T)(Component)mAEntity) : Hierarchy.GetComponent<T>(mAEntity.gameObject));
					list.Add(item);
				}
			}
			return list;
		}

		public List<T> SelectRectangle<T>(Vector2 p_start, Vector2 p_end) where T : Component
		{
			Vector2 vector = default(Vector2);
			vector.x = Mathf.Min(p_start.x, p_end.x);
			vector.y = Mathf.Min(p_start.y, p_end.y);
			Vector2 vector2 = default(Vector2);
			vector2.x = Mathf.Max(p_start.x, p_end.x);
			vector2.y = Mathf.Max(p_start.y, p_end.y);
			return SelectRectangle<T>(new Rect(vector, vector2 - vector));
		}

		protected Rect GetBounds2D(Camera p_camera, Bounds p_bounds)
		{
			Bounds bounds = p_bounds;
			float x = bounds.extents.x;
			float y = bounds.extents.y;
			float z = bounds.extents.z;
			Vector3[] gbrl = m_gbrl;
			m_gbrl[0].Set(x, y, z);
			m_gbrl[1].Set(x, y, 0f - z);
			m_gbrl[2].Set(x, 0f - y, z);
			m_gbrl[3].Set(x, 0f - y, 0f - z);
			m_gbrl[4].Set(0f - x, y, z);
			m_gbrl[5].Set(0f - x, y, 0f - z);
			m_gbrl[6].Set(0f - x, 0f - y, z);
			m_gbrl[7].Set(0f - x, 0f - y, 0f - z);
			Vector3 position = bounds.center + gbrl[0];
			position = p_camera.WorldToScreenPoint(position);
			Vector2 vector2;
			Vector2 vector = (vector2 = position);
			for (int i = 1; i < gbrl.Length; i++)
			{
				position = bounds.center + gbrl[i];
				position = p_camera.WorldToScreenPoint(position);
				vector.x = Mathf.Min(vector.x, position.x);
				vector.y = Mathf.Min(vector.y, position.y);
				vector2.x = Mathf.Max(vector2.x, position.x);
				vector2.y = Mathf.Max(vector2.y, position.y);
			}
			return new Rect(vector, vector2 - vector);
		}

		public RaycastHit ScreenRaycast(Ray p_ray)
		{
			RaycastHit hitInfo = default(RaycastHit);
			if (Physics.Raycast(p_ray, out hitInfo, 1000f, RaycastFlags, QueryTriggerInteraction.Collide))
			{
				if (debug)
				{
					Debug.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal * 0.4f, Color.magenta, 100f);
				}
			}
			else
			{
				hitInfo.distance = -1f;
			}
			return hitInfo;
		}

		public RaycastHit Raycast(Ray p_ray, float p_distance = 1000f)
		{
			RaycastHit hitInfo = default(RaycastHit);
			if (!Physics.Raycast(p_ray, out hitInfo, p_distance, RaycastFlags, QueryTriggerInteraction.Collide))
			{
				hitInfo.distance = -1f;
			}
			return hitInfo;
		}

		public RaycastHit GroundRaycast(Component p_target, float p_distance = 1000f)
		{
			RaycastHit result = default(RaycastHit);
			Transform transform = (p_target ? p_target.transform : null);
			if (!transform)
			{
				result.distance = -1f;
				return result;
			}
			return Raycast(new Ray
			{
				origin = transform.position,
				direction = -Vector3.up
			}, p_distance);
		}

		public RaycastHit ScreenRaycast()
		{
			Camera main = editor.camera.main;
			Vector3 mousePosition = Input.mousePosition;
			Ray p_ray = main.ScreenPointToRay(mousePosition);
			return ScreenRaycast(p_ray);
		}

		public bool ScreenRaycast(out Vector3 p_position, out Vector3 p_normal)
		{
			RaycastHit raycastHit = ScreenRaycast();
			p_position = raycastHit.point;
			p_normal = raycastHit.normal;
			return raycastHit.distance >= 0f;
		}

		public List<T> SelectRayAll<T>(Ray p_ray, int p_max = 0) where T : Component
		{
			_ = editor.camera.main;
			Ray p_ray2 = p_ray;
			List<T> list = new List<T>();
			bool flag = typeof(T) == typeof(MAEntity);
			for (int i = 0; i < hierarchy.Count; i++)
			{
				if (p_max > 0 && list.Count >= p_max)
				{
					break;
				}
				MAEntity mAEntity = hierarchy[i];
				if (!mAEntity)
				{
					hierarchy.RemoveAt(i--);
					continue;
				}
				T val = (flag ? ((T)(Component)mAEntity) : GetComponentCached<T>(mAEntity.gameObject));
				if ((bool)val)
				{
					if (!mAEntity)
					{
						hierarchy.RemoveAt(i--);
					}
					else if (mAEntity.HasHit(p_ray2))
					{
						list.Add(val);
					}
				}
			}
			Vector3 o = p_ray2.origin;
			list.Sort(delegate(T a, T b)
			{
				float num = Vector3.Distance(o, a.transform.position);
				float num2 = Vector3.Distance(o, b.transform.position);
				return (!(num < num2)) ? 1 : (-1);
			});
			if (p_max > 0)
			{
				while (list.Count > p_max)
				{
					list.RemoveAt(list.Count - 1);
				}
			}
			return list;
		}

		public List<T> SelectRayAll<T>(int p_max = 0) where T : Component
		{
			Camera main = editor.camera.main;
			Vector3 mousePosition = Input.mousePosition;
			Ray p_ray = main.ScreenPointToRay(mousePosition);
			return SelectRayAll<T>(p_ray, p_max);
		}

		public T SelectRay<T>() where T : Component
		{
			List<T> list = SelectRayAll<T>(1);
			if (list.Count > 0)
			{
				return list[0];
			}
			return null;
		}

		public List<T> FindAll<T>(Predicate<T> p_callback) where T : Component
		{
			List<T> list = new List<T>();
			bool flag = typeof(T) == typeof(MAEntity);
			for (int i = 0; i < hierarchy.Count; i++)
			{
				MAEntity mAEntity = hierarchy[i];
				if ((bool)mAEntity)
				{
					T val = (flag ? ((T)(Component)mAEntity) : GetComponentCached<T>(mAEntity.gameObject));
					if (val != null)
					{
						list.Add((T)val);
					}
				}
			}
			if (p_callback == null)
			{
				return list;
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (!p_callback(list[j]))
				{
					list.RemoveAt(j--);
				}
			}
			return list;
		}

		public List<T> FindAll<T>(MDEntityAttribFlag p_flags) where T : MAEntity
		{
			return FindAll((T it) => (it.attribs & p_flags) != 0);
		}

		public List<T> FindAll<T>() where T : Component
		{
			return FindAll<T>(null);
		}

		public T Find<T>() where T : Component
		{
			return (T)(Component)hierarchy.Find((MAEntity it) => (bool)it && GetComponentCached<T>(it.gameObject) != null);
		}

		public bool Contains<T>() where T : Component
		{
			return Find<T>() != null;
		}

		public List<T> FindAllByGUID<T>(string p_guid) where T : MapAsset
		{
			List<T> list = FindAll<T>();
			for (int i = 0; i < list.Count; i++)
			{
				if (!string.IsNullOrEmpty(p_guid) && list[i].guid != p_guid)
				{
					list.RemoveAt(i--);
				}
			}
			return list;
		}

		public T FindByGUID<T>(string p_guid) where T : MapAsset
		{
			List<T> list = FindAll<T>();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].guid == p_guid)
				{
					return list[i];
				}
			}
			return null;
		}

		public T FindById<T>(string p_id) where T : MapAsset
		{
			List<T> list = FindAll<T>();
			for (int i = 0; i < list.Count; i++)
			{
				if (!string.IsNullOrEmpty(p_id) && list[i].id == p_id)
				{
					return list[i];
				}
			}
			return null;
		}

		public List<T> FindAllById<T>(IList<string> p_ids) where T : MapAsset
		{
			List<T> list = new List<T>();
			List<T> list2 = FindAll<T>();
			if (idcache == null)
			{
				idcache = new Dictionary<string, int>();
			}
			idcache.Clear();
			int num = 0;
			for (int i = 0; i < p_ids.Count; i++)
			{
				string text = p_ids[i];
				idcache[text] = i;
				for (int j = num; j < list2.Count; j++)
				{
					if (list2[j].id == text)
					{
						list.Add(list2[j]);
					}
				}
			}
			list.Sort(delegate(T a, T b)
			{
				int num2 = idcache[a.id];
				int num3 = idcache[b.id];
				return (num2 >= num3) ? 1 : (-1);
			});
			return list;
		}

		public int AttribCount<T>(IList<T> p_list, MDEntityAttribFlag p_flags) where T : MAEntity
		{
			int num = 0;
			for (int i = 0; i < p_list.Count; i++)
			{
				if ((bool)p_list[i] && (p_list[i].attribs & p_flags) != MDEntityAttribFlag.None)
				{
					num++;
				}
			}
			return num;
		}

		public T GetComponentCached<T>(GameObject p_target) where T : Component
		{
			T result = null;
			if (!p_target)
			{
				return result;
			}
			Dictionary<Type, Component> dictionary = null;
			if (!gccache.ContainsKey(p_target))
			{
				dictionary = new Dictionary<Type, Component>();
				gccache[p_target] = dictionary;
			}
			else
			{
				dictionary = gccache[p_target];
			}
			Type typeFromHandle = typeof(T);
			return (T)(dictionary.ContainsKey(typeFromHandle) ? ((T)dictionary[typeFromHandle]) : (dictionary[typeFromHandle] = p_target.GetComponent<T>()));
		}

		public List<MAGate> FindGates(bool p_assert = true)
		{
			List<MAGate> list = FindAll<MAGate>();
			list.RemoveAll(GateDisableRemove);
			list.Sort(GateSort);
			if (p_assert)
			{
				for (int i = 0; i < list.Count; i++)
				{
					list[i].index = (list[i].isTrigger ? i : (-1));
				}
			}
			return list;
		}

		public void SetGatesTriggerVisible(bool p_flag)
		{
			List<MAGate> list = FindGates(p_assert: false);
			for (int i = 0; i < list.Count; i++)
			{
				if ((bool)list[i])
				{
					list[i].SetTriggerRendererEnabled(p_flag && list[i].isTrigger);
				}
			}
		}

		public void SetGuidesVisible(bool p_flag)
		{
			List<MAGuide> list = FindAll<MAGuide>();
			for (int i = 0; i < list.Count; i++)
			{
				MAGuide mAGuide = list[i];
				if ((bool)mAGuide)
				{
					mAGuide.gameObject.SetActive(p_flag);
				}
			}
			if (p_flag)
			{
				List<MAGate> list2 = FindAll<MAGate>();
				for (int j = 0; j < list2.Count; j++)
				{
					MAGate mAGate = list2[j];
					if ((bool)mAGate)
					{
						MAGuide respawnGuide = mAGate.GetRespawnGuide();
						if ((bool)respawnGuide)
						{
							respawnGuide.gameObject.SetActive(mAGate.isRespawnVisible);
						}
					}
				}
			}
			List<MASpline> list3 = FindAll<MASpline>();
			for (int k = 0; k < list3.Count; k++)
			{
				MASpline mASpline = list3[k];
				if ((bool)mASpline)
				{
					bool active = p_flag;
					if (mASpline.splineCategory == SplineCategory.Visual)
					{
						active = true;
					}
					mASpline.gameObject.SetActive(active);
				}
			}
			MAEntity entity = editor.model.selection.entity;
			List<MACameraTool> list4 = FindAll<MACameraTool>();
			for (int l = 0; l < list4.Count; l++)
			{
				MACameraTool mACameraTool = list4[l];
				if ((bool)mACameraTool)
				{
					mACameraTool.gameObject.SetActive(p_flag);
					if ((bool)mACameraTool.collider)
					{
						bool flag = (bool)entity && mACameraTool.collider.transform.IsChild(entity.transform.parent);
						mACameraTool.collider.gameObject.SetActive(flag && p_flag);
					}
				}
			}
		}

		public void SaveGuidesVisibility()
		{
			if (m_gvt == null)
			{
				m_gvt = new Dictionary<MAGuide, bool>();
			}
			List<MAGuide> list = FindAll<MAGuide>();
			for (int i = 0; i < list.Count; i++)
			{
				MAGuide mAGuide = list[i];
				if ((bool)mAGuide)
				{
					m_gvt[mAGuide] = mAGuide.gameObject.activeInHierarchy;
				}
			}
		}

		public void RestoreGuidesVisibility()
		{
			if (m_gvt == null)
			{
				m_gvt = new Dictionary<MAGuide, bool>();
			}
			List<MAGuide> list = FindAll<MAGuide>();
			for (int i = 0; i < list.Count; i++)
			{
				MAGuide mAGuide = list[i];
				if ((bool)mAGuide && m_gvt.ContainsKey(mAGuide))
				{
					bool active = m_gvt[mAGuide];
					mAGuide.gameObject.SetActive(active);
				}
			}
		}

		public int GetNextGateIndex(IList<MAGate> p_list)
		{
			int num = 0;
			for (int i = 0; i < p_list.Count; i++)
			{
				num = Mathf.Max(num, p_list[i].index);
			}
			if (p_list.Count > 0)
			{
				return num + 1;
			}
			return 0;
		}

		public int GetNextGateIndex()
		{
			return GetNextGateIndex(FindAll<MAGate>());
		}

		public List<MAGate> SetGateOrder(int p_i0, int p_i1)
		{
			List<MAGate> list = FindGates();
			MAGate item = list[p_i0];
			list.RemoveAt(p_i0);
			list.Insert(p_i1, item);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].index = i;
			}
			return list;
		}

		public List<MAGate> InsertGates(IList<MAEntity> p_list)
		{
			List<MAGate> l0 = new List<MAGate>();
			for (int i = 0; i < p_list.Count; i++)
			{
				if (p_list[i] is MAGate)
				{
					l0.Add(p_list[i] as MAGate);
				}
			}
			List<MAGate> list = FindGates(p_assert: false);
			list.Sort(delegate(MAGate a, MAGate b)
			{
				if (a.index < b.index)
				{
					return -1;
				}
				if (a.index > b.index)
				{
					return 1;
				}
				if (l0.Contains(a) && !l0.Contains(b))
				{
					return 1;
				}
				return (!l0.Contains(a) && l0.Contains(b)) ? (-1) : 0;
			});
			for (int num = 0; num < list.Count; num++)
			{
				list[num].index = num;
			}
			return list;
		}

		private int GateSort(MAGate a, MAGate b)
		{
			if (a.isTrigger && !b.isTrigger)
			{
				return -1;
			}
			if (!a.isTrigger && b.isTrigger)
			{
				return 1;
			}
			if (a.index < 0 && b.index < 0)
			{
				return 0;
			}
			if (a.index < 0)
			{
				return 1;
			}
			if (b.index < 0)
			{
				return -1;
			}
			if (a.isFinish)
			{
				return 1;
			}
			if (b.isFinish)
			{
				return -1;
			}
			if (a.index >= b.index)
			{
				return 1;
			}
			return -1;
		}

		private bool GateDisableRemove(MAGate a)
		{
			return !a.isTrigger;
		}

		private string LogGates(List<MAGate> p_list)
		{
			return string.Join("\n", p_list.ConvertAll((MAGate it) => "[" + it.index.ToString("00") + "] " + it.name));
		}

		public List<MAPodium> FindPodiums()
		{
			List<MAPodium> list = FindAll<MAPodium>();
			list.Sort(PodiumSort);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].index = i;
			}
			return list;
		}

		public Vector3 GetPodiumsCenter(Vector3 p_offset)
		{
			List<MAPodium> list = FindPodiums();
			Vector3 zero = Vector3.zero;
			if (list.Count <= 0)
			{
				return zero;
			}
			for (int i = 0; i < list.Count; i++)
			{
				zero += list[i].transform.position;
			}
			float num = 1f / (float)list.Count;
			zero *= num;
			return zero + p_offset;
		}

		public Vector3 GetPodiumsRotation(Vector3 p_offset)
		{
			List<MAPodium> list = FindPodiums();
			Vector3 zero = Vector3.zero;
			if (list.Count <= 0)
			{
				return zero;
			}
			for (int i = 0; i < list.Count; i++)
			{
				zero += list[i].transform.localEulerAngles;
			}
			float num = 1f / (float)list.Count;
			zero *= num;
			return zero + p_offset;
		}

		public Vector3 GetPodiumsCenter()
		{
			return GetPodiumsCenter(Vector3.zero);
		}

		public Vector3 GetPodiumsRotation()
		{
			return GetPodiumsRotation(Vector3.zero);
		}

		public int GetNextPodiumIndex(IList<MAPodium> p_list)
		{
			int num = 0;
			for (int i = 0; i < p_list.Count; i++)
			{
				num = Mathf.Max(num, p_list[i].index);
			}
			if (p_list.Count > 0)
			{
				return num + 1;
			}
			return 0;
		}

		public int GetNextPodiumIndex()
		{
			return GetNextPodiumIndex(FindAll<MAPodium>());
		}

		public List<MAPodium> SetPodiumOrder(int p_i0, int p_i1)
		{
			List<MAPodium> list = FindPodiums();
			MAPodium item = list[p_i0];
			list.RemoveAt(p_i0);
			list.Insert(p_i1, item);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].index = i;
			}
			return list;
		}

		private int PodiumSort(MAPodium a, MAPodium b)
		{
			if (a.index >= b.index)
			{
				return 1;
			}
			return -1;
		}

		public List<MACameraTool> FindCameraTools(bool p_assert = true)
		{
			List<MACameraTool> list = FindAll<MACameraTool>();
			list.RemoveAll(CameraToolEmptyRemove);
			list.Sort(CameraToolSort);
			if (p_assert)
			{
				for (int i = 0; i < list.Count; i++)
				{
					list[i].index = i;
				}
			}
			return list;
		}

		private bool CameraToolEmptyRemove(MACameraTool a)
		{
			return !a.HasControlPoints();
		}

		private int CameraToolSort(MACameraTool a, MACameraTool b)
		{
			bool flag = a.HasControlPoints();
			bool flag2 = b.HasControlPoints();
			if (flag && !flag2)
			{
				return -1;
			}
			if (!flag && flag2)
			{
				return -1;
			}
			if (a.index >= b.index)
			{
				return 1;
			}
			return -1;
		}

		public List<Vector3> GetRaceLine(float p_podium_margin, float p_finish_margin)
		{
			List<Vector3> list = new List<Vector3>();
			bool flag = Contains<MAPodium>();
			if (!Contains<MAGate>())
			{
				return list;
			}
			List<MAGate> list2 = FindGates();
			Vector3 zero = Vector3.zero;
			Vector3 vector = (flag ? GetPodiumsCenter(Vector3.up * 1f) : list2[0].triggerCenter);
			zero = (flag ? (list2[0].triggerCenter - vector) : (-list2[0].transform.forward));
			zero.Normalize();
			vector += zero * p_podium_margin;
			Vector3 triggerCenter = list2[list2.Count - 1].triggerCenter;
			zero = ((list2.Count <= 1) ? list2[list2.Count - 1].transform.forward : (-(list2[list2.Count - 1].triggerCenter - list2[list2.Count - 2].triggerCenter)));
			zero.Normalize();
			triggerCenter += zero * p_finish_margin;
			list.Add(vector);
			for (int i = 0; i < list2.Count - 1; i++)
			{
				MAGate mAGate = list2[i];
				list.Add(mAGate.triggerCenter);
			}
			list.Add(triggerCenter);
			return list;
		}

		public List<Vector3> GetRaceLine()
		{
			return GetRaceLine(0f, 0f);
		}

		public void Destroy(string p_id)
		{
			MAEntity p_target = FindById<MAEntity>(p_id);
			Destroy(p_target);
		}

		public void Destroy(List<string> p_ids)
		{
			List<MAEntity> p_targets = FindAllById<MAEntity>(p_ids);
			Destroy(p_targets);
		}

		public void Destroy(IList p_targets)
		{
			GameObject gameObject = new GameObject("dump");
			bool flag = false;
			for (int i = 0; i < p_targets.Count; i++)
			{
				object obj = p_targets[i];
				GameObject gameObject2 = null;
				if (obj is Component)
				{
					gameObject2 = ((Component)obj).gameObject;
				}
				else if (obj is GameObject)
				{
					gameObject2 = (GameObject)obj;
				}
				if (obj is MAEntity)
				{
					MAEntity item = (MAEntity)obj;
					if (hierarchy.Contains(item))
					{
						hierarchy.Remove(item);
					}
				}
				if (obj is MACameraToolControlPoint)
				{
					flag = true;
				}
				if ((bool)gameObject2)
				{
					gameObject2.transform.parent = gameObject.transform;
				}
			}
			UnityEngine.Object.Destroy(gameObject);
			if (flag)
			{
				AssertDeletion();
			}
		}

		public void Destroy(Component p_target)
		{
			if ((bool)p_target)
			{
				MAEntity mAEntity = ((p_target is MAEntity) ? ((MAEntity)p_target) : null);
				if ((bool)mAEntity && hierarchy.Contains(mAEntity))
				{
					hierarchy.Remove(mAEntity);
				}
				UnityEngine.Object.Destroy(p_target.gameObject);
			}
		}

		protected void AssertDeletion()
		{
			List<MAEntity> list = new List<MAEntity>();
			for (int i = 0; i < hierarchy.Count; i++)
			{
				MAEntity mAEntity = hierarchy[i];
				if (mAEntity.data.type == MapAssetType.CameraTool)
				{
					MACameraTool mACameraTool = mAEntity as MACameraTool;
					if (!mACameraTool.HasControlPoints())
					{
						list.Add(mACameraTool);
					}
				}
			}
			Destroy(list);
		}

		public void LoadHierarchy()
		{
			hierarchy = new List<MAEntity>();
			hierarchy.AddRange(Hierarchy.FindAll<MAEntity>(base.transform));
			hierarchy.Sort(HierarchyIdSort);
		}

		private static int HierarchyIdSort(MAEntity a, MAEntity b)
		{
			if (!a)
			{
				if (!b)
				{
					return 0;
				}
				return 1;
			}
			if (!b)
			{
				if (!a)
				{
					return 0;
				}
				return 1;
			}
			return string.Compare(a.id, b.id);
		}

		public void HierarchyAdd(MAEntity p_target)
		{
			if (!hierarchy.Contains(p_target))
			{
				hierarchy.Add(p_target);
				hierarchy.Sort(HierarchyIdSort);
			}
		}

		public void HierarchyAdd(IList p_targets)
		{
			for (int i = 0; i < p_targets.Count; i++)
			{
				object obj = p_targets[i];
				if (obj is MAEntity)
				{
					HierarchyAdd(obj as MAEntity);
				}
			}
		}

		protected void AssertCreate()
		{
			List<MAEntity> list = new List<MAEntity>();
			for (int i = 0; i < hierarchy.Count; i++)
			{
				MAEntity mAEntity = hierarchy[i];
				if (mAEntity.data.type == MapAssetType.CameraToolControlPoint)
				{
					MACameraToolControlPoint mACameraToolControlPoint = mAEntity as MACameraToolControlPoint;
					if (!mACameraToolControlPoint.tool)
					{
						list.Add(mACameraToolControlPoint);
					}
				}
			}
			Destroy(list);
		}

		public T Create<T>(MDEntity p_data, MAEntity p_container) where T : Component
		{
			Transform p_container2 = (p_container ? p_container : root).transform;
			MAEntity mAEntity = editor.factory.Build(p_data, p_container2);
			bool flag = false;
			HierarchyAdd(mAEntity);
			if (mAEntity is MAGate)
			{
				MAGuide p_template = base.app.model.storage.library.FindByGUID<MAGuide>("DMA-d529");
				MAGate mAGate = mAEntity as MAGate;
				List<MAGate> p_list = FindGates(mAGate.index < 0);
				if (mAGate.index < 0)
				{
					mAGate.index = GetNextGateIndex(p_list);
				}
				mAGate.SetTriggerRendererEnabled(mAGate.isTrigger);
				p_template = mAGate.AssertRespawnGuide(p_template);
				if ((bool)p_template)
				{
					HierarchyAdd(p_template);
				}
				mAGate.Write();
			}
			if (mAEntity is MAPodium)
			{
				MAPodium mAPodium = mAEntity as MAPodium;
				if (mAPodium.index < 0)
				{
					mAPodium.index = GetNextPodiumIndex();
				}
			}
			if (mAEntity is MACameraToolControlPoint)
			{
				flag = true;
			}
			T componentCached = GetComponentCached<T>(mAEntity.gameObject);
			GetComponentCached<MARenderer>(mAEntity.gameObject);
			GetComponentCached<MAEntity>(mAEntity.gameObject);
			GetComponentCached<MAGate>(mAEntity.gameObject);
			GetComponentCached<MAPodium>(mAEntity.gameObject);
			if (flag)
			{
				AssertCreate();
			}
			return componentCached;
		}

		public T Create<T>(MDEntity p_data) where T : Component
		{
			return Create<T>(p_data, null);
		}

		public List<T> Create<T>(List<MDEntity> p_data, MAEntity p_container) where T : Component
		{
			List<T> list = new List<T>();
			for (int i = 0; i < p_data.Count; i++)
			{
				MAEntity mAEntity = p_container;
				MAEntity mAEntity2 = FindById<MAEntity>(p_data[i].parentId);
				mAEntity = (mAEntity ? mAEntity : mAEntity2);
				T item = Create<T>(p_data[i], mAEntity);
				list.Add(item);
			}
			return list;
		}

		public List<T> Create<T>(List<MDEntity> p_data) where T : Component
		{
			return Create<T>(p_data, null);
		}

		public T Create<T>(GameObject p_target, MAEntity p_container) where T : Component
		{
			MAEntity mAEntity = (p_container ? p_container : root);
			Transform transform = mAEntity.transform;
			GameObject gameObject = p_target;
			if (!gameObject.GetComponent<T>())
			{
				return null;
			}
			gameObject = UnityEngine.Object.Instantiate(gameObject);
			gameObject.name = gameObject.name.Replace("(Clone)", "");
			MAEntity componentCached = GetComponentCached<MAEntity>(gameObject);
			T result = null;
			if ((bool)componentCached)
			{
				MAEntity mAEntity2 = FindById<MAEntity>(componentCached.data.parentId);
				transform = (p_container ? mAEntity.transform : mAEntity2.transform);
				for (int i = 0; i < componentCached.hits.Count; i++)
				{
					if ((bool)componentCached.hits[i])
					{
						componentCached.hits[i].gameObject.layer = 30;
					}
				}
				componentCached.name = componentCached.name + "-" + UnityEngine.Random.Range(0, 512).ToString("x6");
				gameObject.transform.SetParent(transform, worldPositionStays: true);
				componentCached.data.parent = mAEntity.data;
				componentCached.Write();
				hierarchy.Add(componentCached);
				hierarchy.Sort(HierarchyIdSort);
				result = GetComponentCached<T>(componentCached.gameObject);
				GetComponentCached<MARenderer>(componentCached.gameObject);
				GetComponentCached<MAGate>(componentCached.gameObject);
				GetComponentCached<MAPodium>(componentCached.gameObject);
			}
			return result;
		}

		public T Create<T>(GameObject p_target) where T : Component
		{
			return Create<T>(p_target, null);
		}

		public void SetRenderersMapStyle(int p_style, int p_index)
		{
			List<MARenderer> list = FindAll<MARenderer>();
			for (int i = 0; i < list.Count; i++)
			{
				MARenderer mARenderer = list[i];
				switch (p_style)
				{
				case 0:
					mARenderer.mapStyle0 = p_index;
					break;
				case 1:
					mARenderer.mapStyle1 = p_index;
					break;
				case 2:
					mARenderer.mapStyle2 = p_index;
					break;
				}
			}
		}

		public void SetTriggerRenderersEnabled(bool p_flag)
		{
			List<MAGate> list = FindGates(p_assert: false);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].SetTriggerRendererEnabled(p_flag && list[i].isTrigger);
			}
		}

		public void ApplySceneBounds(IList p_targets)
		{
			if (p_targets == null)
			{
				return;
			}
			LevelModel level = base.app.model.game.level;
			for (int i = 0; i < p_targets.Count; i++)
			{
				object obj = p_targets[i];
				Transform transform = null;
				if (obj is Component)
				{
					transform = ((Component)obj).transform;
				}
				if (obj is GameObject)
				{
					transform = ((GameObject)obj).transform;
				}
				if (!transform)
				{
					continue;
				}
				LevelSettings.Scene.AssetBounds assetBounds = level.settings.scene.assetBounds;
				if (assetBounds != null)
				{
					Vector3 position = transform.position;
					if (!assetBounds.IsValidAssetPosition(position))
					{
						position = assetBounds.GetValidPosition(position);
						transform.position = position;
					}
				}
			}
		}
	}
}
