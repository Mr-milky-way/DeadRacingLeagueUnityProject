using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	[Serializable]
	public class MEStateModel : Model<DRLApp>
	{
		[Serializable]
		public class Mouse
		{
			public bool focus;

			public Vector2 screenSize;

			public Rect rect;

			public Vector2 scroll;

			public RaycastHit hit;

			public Rect screenRect
			{
				get
				{
					Rect result = rect;
					result.y = screenSize.y + result.y;
					float x = screenSize.x;
					float num = ((x <= 0f) ? 0f : (1f / x));
					float y = screenSize.y;
					float num2 = ((y <= 0f) ? 0f : (1f / y));
					float num3 = Screen.width;
					float num4 = Screen.height;
					result.xMin = result.xMin * num * num3;
					result.yMin = result.yMin * num2 * num4;
					result.xMax = result.xMax * num * num3;
					result.yMax = result.yMax * num2 * num4;
					return result;
				}
			}

			public bool valid => hit.distance >= 0f;

			public Vector3 position => hit.point;

			public Vector3 normal => hit.normal;

			public void Place(Transform p_target, float p_distance, float p_angle, float p_snap_distance, float p_snap_angle, float p_snap_position, bool p_orient)
			{
				if ((bool)p_target)
				{
					float y = ((p_snap_distance <= 0f) ? p_distance : (Mathf.Round(p_distance / p_snap_distance) * p_snap_distance));
					float y2 = ((p_snap_angle <= 0f) ? p_angle : (Mathf.Round(p_angle / p_snap_angle) * p_snap_angle));
					Vector3 vector = position;
					vector.x = ((p_snap_position <= 0f) ? vector.x : (Mathf.Round(vector.x / p_snap_position) * p_snap_position));
					vector.y = ((p_snap_position <= 0f) ? vector.y : (Mathf.Round(vector.y / p_snap_position) * p_snap_position));
					vector.z = ((p_snap_position <= 0f) ? vector.z : (Mathf.Round(vector.z / p_snap_position) * p_snap_position));
					Vector3 up = (p_orient ? normal : Vector3.up);
					Transform parent = p_target.transform.parent;
					Transform transform = new GameObject("p").transform;
					transform.transform.position = vector;
					transform.transform.up = up;
					Vector3 localEulerAngles = transform.transform.localEulerAngles;
					localEulerAngles.x = ((p_snap_angle <= 0f) ? localEulerAngles.x : (Mathf.Round(localEulerAngles.x / p_snap_angle) * p_snap_angle));
					localEulerAngles.y = ((p_snap_angle <= 0f) ? localEulerAngles.y : (Mathf.Round(localEulerAngles.y / p_snap_angle) * p_snap_angle));
					localEulerAngles.z = ((p_snap_angle <= 0f) ? localEulerAngles.z : (Mathf.Round(localEulerAngles.z / p_snap_angle) * p_snap_angle));
					transform.transform.localEulerAngles = localEulerAngles;
					p_target.SetParent(transform, worldPositionStays: true);
					p_target.localPosition = new Vector3(0f, y, 0f);
					p_target.localEulerAngles = new Vector3(0f, y2, 0f);
					p_target.SetParent(parent, worldPositionStays: true);
					UnityEngine.Object.Destroy(transform.gameObject);
				}
			}

			public void Place(Transform p_target, float p_distance, float p_angle, float p_snap_distance, float p_snap_position, bool p_orient)
			{
				Place(p_target, p_distance, p_angle, p_snap_distance, 0f, p_orient);
			}

			public void Place(Transform p_target, float p_distance, float p_angle, bool p_orient)
			{
				Place(p_target, p_distance, p_angle, 0f, 0f, p_orient);
			}

			public void Place(Transform p_target, float p_distance, bool p_orient)
			{
				Place(p_target, p_distance, 0f, 0f, 0f, p_orient);
			}

			public void Place(Transform p_target, bool p_orient)
			{
				Place(p_target, 0f, 0f, 0f, 0f, p_orient);
			}

			public void Place(Transform p_target)
			{
				Place(p_target, 0f, 0f, 0f, 0f, p_orient: true);
			}
		}

		[Serializable]
		public class Physics
		{
			public MEStateModel state;

			public List<MEPreviewPhysics> targets;

			public bool enabled;

			public bool willApply;

			public Vector3 velocity;

			public Vector3 angularVelocity;

			public float bounce;

			public float delay;

			public float duration;

			public List<MEPreviewPhysics> Push(IList p_targets, Transform p_emitter)
			{
				List<MEPreviewPhysics> list = new List<MEPreviewPhysics>();
				if (p_targets == null)
				{
					return list;
				}
				for (int i = 0; i < p_targets.Count; i++)
				{
					if (p_targets[i] != null)
					{
						MEPreviewPhysics mEPreviewPhysics = null;
						if (p_targets[i] is MEPreviewPhysics)
						{
							mEPreviewPhysics = Push(p_targets[i] as MEPreviewPhysics, p_emitter);
						}
						if (p_targets[i] is MAEntity)
						{
							mEPreviewPhysics = Push(p_targets[i] as MAEntity, p_emitter);
						}
						if ((bool)mEPreviewPhysics)
						{
							list.Add(mEPreviewPhysics);
						}
					}
				}
				return list;
			}

			public List<MEPreviewPhysics> Push(IList p_targets)
			{
				return Push(p_targets, null);
			}

			public MEPreviewPhysics Push(MAEntity p_target, Transform p_emitter)
			{
				if (!p_target)
				{
					return null;
				}
				if (p_target.tags.Contains(MapAssetType.NoPhysics))
				{
					return null;
				}
				MEPreviewPhysics mEPreviewPhysics = p_target.gameObject.GetComponent<MEPreviewPhysics>();
				if (!mEPreviewPhysics)
				{
					mEPreviewPhysics = p_target.gameObject.AddComponent<MEPreviewPhysics>();
				}
				return Push(mEPreviewPhysics, p_emitter);
			}

			public MEPreviewPhysics Push(MAEntity p_target)
			{
				return Push(p_target, null);
			}

			public MEPreviewPhysics Push(MEPreviewPhysics p_target, Transform p_emitter)
			{
				if (!p_target)
				{
					return null;
				}
				Vector3 vector = velocity;
				if ((bool)p_emitter)
				{
					Vector3 vector2 = p_target.transform.position - p_emitter.transform.position;
					vector2.y = 0f;
					vector2.Normalize();
					vector2 *= vector.z;
					vector2.y = vector.y;
					vector = vector2;
				}
				p_target.velocity = vector;
				p_target.angularVelocity = angularVelocity;
				p_target.bounce = bounce;
				p_target.delay = delay;
				p_target.duration = duration;
				targets.RemoveAll((MEPreviewPhysics it) => it == null);
				if (!targets.Contains(p_target))
				{
					targets.Add(p_target);
				}
				p_target.Run();
				return p_target;
			}
		}

		[Serializable]
		public class Preview
		{
			public MEStateModel state;

			public List<MapAsset> assets;

			public List<GameObject> objects;

			public Transform target;

			public MAEntity container;

			public Vector3 scale = Vector3.one;

			[SerializeField]
			private float m_distance;

			[SerializeField]
			private float m_angle;

			public float distanceSnap;

			public float positionSnap;

			public float angleSnap;

			public bool orient;

			public int index
			{
				get
				{
					for (int i = 0; i < objects.Count; i++)
					{
						if ((bool)objects[i] && objects[i].activeInHierarchy)
						{
							return i;
						}
					}
					return -1;
				}
				set
				{
					for (int i = 0; i < objects.Count; i++)
					{
						if ((bool)objects[i])
						{
							objects[i].SetActive(i == Mathf.Clamp(value, 0, objects.Count - 1));
						}
					}
				}
			}

			public GameObject current
			{
				get
				{
					int num = index;
					if (num < 0)
					{
						return null;
					}
					if (num >= objects.Count)
					{
						return null;
					}
					return objects[num];
				}
			}

			public float distance
			{
				get
				{
					return m_distance;
				}
				set
				{
					m_distance = value;
					Place();
				}
			}

			public float angle
			{
				get
				{
					return m_angle;
				}
				set
				{
					m_angle = value;
					Place();
				}
			}

			public void Clear()
			{
				if ((bool)target)
				{
					UnityEngine.Object.Destroy(target.gameObject);
				}
				m_distance = 0f;
				m_angle = 0f;
				orient = true;
				state.PreviewVisible = false;
				ClearContainer();
				if ((bool)container && container.transform.parent == state.transform)
				{
					UnityEngine.Object.Destroy(container.gameObject);
				}
				container = null;
			}

			public void Reset()
			{
				m_distance = 0f;
				m_angle = 0f;
			}

			public T Get<T>() where T : Component
			{
				GameObject gameObject = current;
				_ = index;
				if (!gameObject)
				{
					return null;
				}
				return gameObject.GetComponent<T>();
			}

			public void Create(List<MapAsset> p_items)
			{
				Clear();
				MapData data = state.model.data;
				GameObject gameObject = new GameObject("preview");
				gameObject.transform.parent = state.model.transform;
				gameObject.transform.localScale = scale;
				target = gameObject.transform;
				objects = new List<GameObject>();
				MAEntity mAEntity = null;
				for (int i = 0; i < p_items.Count; i++)
				{
					MapAsset mapAsset = p_items[i];
					MapAsset instanceReference = GetInstanceReference(mapAsset);
					instanceReference = CreateInstance(instanceReference, target.transform);
					instanceReference.gameObject.SetActive(i == 0);
					if (instanceReference is MARenderer)
					{
						MARenderer mARenderer = instanceReference as MARenderer;
						for (int j = 0; j < mARenderer.hits.Count; j++)
						{
							if ((bool)mARenderer.hits[j])
							{
								mARenderer.hits[j].enabled = false;
							}
						}
						mARenderer.mapStyle0 = data.mapStyle0;
						mARenderer.mapStyle1 = data.mapStyle1;
						mARenderer.mapStyle2 = data.mapStyle2;
						mARenderer.Refresh();
					}
					switch (mapAsset.data.type)
					{
					case MapAssetType.Gate:
					{
						MAGate obj = instanceReference as MAGate;
						obj.SetTriggerRendererEnabled(p_flag: true);
						obj.index = -1;
						break;
					}
					case MapAssetType.Spline:
					{
						MASpline mASpline = mapAsset as MASpline;
						mASpline.RefreshSpline(0.1f);
						if (!mAEntity)
						{
							mASpline = CreateInstance(mASpline, state.model.transform);
							RefreshInstanceData(mASpline);
							mAEntity = mASpline;
						}
						break;
					}
					case MapAssetType.CameraTool:
					{
						MACameraTool mACameraTool = mapAsset as MACameraTool;
						mACameraTool.Refresh();
						if (!mAEntity)
						{
							mACameraTool = CreateInstance(mACameraTool, state.model.transform);
							RefreshInstanceData(mACameraTool);
							mAEntity = mACameraTool;
						}
						break;
					}
					case MapAssetType.Collectable:
						(instanceReference as MACollectable).RefreshSize();
						break;
					}
					objects.Add(instanceReference.gameObject);
				}
				assets = new List<MapAsset>(p_items);
				container = mAEntity;
			}

			public void Place()
			{
				if ((bool)target)
				{
					state.mouse.Place(target, distance, angle, distanceSnap, angleSnap, positionSnap, orient);
				}
			}

			public void RefreshContainer()
			{
				if (!container)
				{
					return;
				}
				switch (container.data.type)
				{
				case MapAssetType.Spline:
				{
					MASpline mASpline = container as MASpline;
					Transform transform2 = mASpline.transform.Find("$scp-dummy");
					if (!transform2)
					{
						transform2 = new GameObject("$scp-dummy").transform;
						transform2.transform.parent = mASpline.transform;
					}
					transform2.transform.position = target.position;
					mASpline.Refresh();
					break;
				}
				case MapAssetType.CameraTool:
				{
					MACameraTool mACameraTool = container as MACameraTool;
					Transform transform = mACameraTool.transform.Find("$ctcp-dummy");
					if (!transform)
					{
						transform = new GameObject("$ctcp-dummy").transform;
						transform.transform.parent = mACameraTool.transform;
						if ((bool)mACameraTool.lineRenderer.from && !mACameraTool.lineRenderer.to)
						{
							mACameraTool.lineRenderer.to = transform.transform;
						}
						if (!mACameraTool.lineRenderer.from)
						{
							mACameraTool.lineRenderer.from = transform.transform;
						}
					}
					transform.transform.position = target.position;
					mACameraTool.Refresh();
					break;
				}
				}
			}

			public void ClearContainer()
			{
				if (!container)
				{
					return;
				}
				switch (container.data.type)
				{
				case MapAssetType.Spline:
				{
					MASpline mASpline = container as MASpline;
					Transform transform2 = mASpline.transform.Find("$scp-dummy");
					if ((bool)transform2)
					{
						UnityEngine.Object.Destroy(transform2.gameObject);
						mASpline.splineRenderer.Refresh();
					}
					break;
				}
				case MapAssetType.CameraTool:
				{
					MACameraTool mACameraTool = container as MACameraTool;
					Transform transform = mACameraTool.transform.Find("$ctcp-dummy");
					if ((bool)transform)
					{
						UnityEngine.Object.Destroy(transform.gameObject);
						mACameraTool.Refresh();
					}
					break;
				}
				}
			}

			protected MapAsset GetInstanceReference(MapAsset p_asset)
			{
				if (!p_asset)
				{
					return null;
				}
				MapAsset result = p_asset;
				switch (p_asset.data.type)
				{
				case MapAssetType.Spline:
				{
					MASpline mASpline = p_asset as MASpline;
					MASplineControlPoint mASplineControlPoint = state.app.model.storage.library.FindByGUID<MASplineControlPoint>(mASpline.splineControlPointId);
					mASplineControlPoint.SetEnabled(p_flag: true);
					mASplineControlPoint.SetAssetMode(p_flag: true);
					result = mASplineControlPoint;
					break;
				}
				case MapAssetType.CameraTool:
				{
					MACameraTool mACameraTool = p_asset as MACameraTool;
					MACameraToolControlPoint mACameraToolControlPoint = state.app.model.storage.library.FindByGUID<MACameraToolControlPoint>(mACameraTool.cameraToolControlPointId);
					mACameraToolControlPoint.SetIconMode(p_flag: true);
					result = mACameraToolControlPoint;
					break;
				}
				}
				return result;
			}

			protected T CreateInstance<T>(T p_asset, Transform p_container) where T : MapAsset
			{
				if (!p_asset)
				{
					return null;
				}
				T original = p_asset;
				original = UnityEngine.Object.Instantiate(original);
				original.name = original.name.Replace("(Clone)", "");
				original.transform.SetParent(p_container, worldPositionStays: true);
				Transform transform = original.transform;
				Vector3 localPosition = (original.transform.localEulerAngles = Vector3.zero);
				transform.localPosition = localPosition;
				original.transform.localScale = Vector3.one;
				return original;
			}

			protected void RefreshInstanceData(MapAsset p_asset)
			{
				if ((bool)p_asset)
				{
					string name = p_asset.name;
					_ = p_asset.id;
					p_asset.name = name;
					p_asset.name = (p_asset.name.Contains("$") ? p_asset.name.Split('$')[0] : p_asset.name);
					p_asset.name = p_asset.name + "$" + UnityEngine.Random.Range(0, 512).ToString("x4");
					p_asset.data.id = MDObject.GenerateId();
					p_asset.Write();
				}
			}
		}

		[Serializable]
		public class Metric
		{
			public MEStateModel state;

			[SerializeField]
			internal MEMetricMode m_mode;

			[SerializeField]
			private bool m_show_rulers;

			public bool snapMap;

			public float snapMapMoveUnit;

			public float snapMapRotateUnit;

			public bool snapMove;

			public float snapMoveUnit;

			public bool snapRotate;

			public float snapRotateUnit;

			public bool snapKeyboard;

			public List<MEMetricConvertable> convertables;

			public MEMetricMode mode
			{
				get
				{
					return m_mode;
				}
				set
				{
					SetMetricConvertables(value);
					state.model.OnMetricModeChange(m_mode, value);
				}
			}

			public bool showRulers
			{
				get
				{
					return m_show_rulers;
				}
				set
				{
					if (m_show_rulers != value)
					{
						m_show_rulers = value;
						state.model.Notify("map-editor.metric.ruler.state.change", value);
					}
				}
			}

			public bool IsSnapMove()
			{
				if (!snapMap)
				{
					return snapMove;
				}
				return true;
			}

			public bool IsSnapRotate()
			{
				if (!snapMap)
				{
					return snapRotate;
				}
				return true;
			}

			public float GetSnapMoveUnit()
			{
				if (!snapMap)
				{
					if (!snapMove)
					{
						return 0f;
					}
					return snapMoveUnit;
				}
				return snapMapMoveUnit;
			}

			public float GetSnapRotateUnit()
			{
				if (!snapMap)
				{
					if (!snapRotate)
					{
						return 0f;
					}
					return snapRotateUnit;
				}
				return snapMapRotateUnit;
			}

			public void AddMetricConvertable(MEMetricConvertable p_item)
			{
				if (!convertables.Contains(p_item))
				{
					convertables.Add(p_item);
				}
			}

			public void SetMetricConvertables(MEMetricMode p_mode)
			{
				convertables.RemoveAll((MEMetricConvertable it) => it == null);
				for (int num = 0; num < convertables.Count; num++)
				{
					convertables[num].mode = p_mode;
				}
			}
		}

		[SerializeField]
		internal MEInputStateType m_input;

		[SerializeField]
		internal MEActionStateType m_action;

		[SerializeField]
		internal MERenderStateType m_render;

		[SerializeField]
		internal MEHandlePivotType m_pivot;

		public List<MAEntity> entities;

		public List<MapAssetType> entityTags;

		private List<string> m_entity_ids;

		[SerializeField]
		private Preview m_preview;

		[SerializeField]
		private Metric m_metric;

		[SerializeField]
		private Physics m_physics;

		[SerializeField]
		private Mouse m_mouse;

		public int inspectorTabAfterSelection;

		public List<TransformVector> transformFrom;

		public List<TransformVector> transformTo;

		public List<string> propertyFrom;

		public List<string> propertyTo;

		public Vector3 cameraPosition;

		public Quaternion cameraRotation;

		public bool AllowRaycast;

		public bool ActiveDragSelect;

		[SerializeField]
		private bool m_previewEnabled;

		public MapEditorModel model => Assert<MapEditorModel>("model");

		public MEInputStateType input
		{
			get
			{
				return m_input;
			}
			set
			{
				model.OnInputStateChange(m_input, value);
			}
		}

		public MEActionStateType action
		{
			get
			{
				return m_action;
			}
			set
			{
				model.OnActionStateChange(m_action, value);
			}
		}

		public MERenderStateType render
		{
			get
			{
				return m_render;
			}
			set
			{
				model.OnRenderStateChange(m_render, value);
			}
		}

		public MEHandlePivotType pivot
		{
			get
			{
				return m_pivot;
			}
			set
			{
				model.OnPivotModeChange(m_pivot, value);
			}
		}

		public List<string> entitiesIds
		{
			get
			{
				if (m_entity_ids != null)
				{
					return m_entity_ids;
				}
				return m_entity_ids = new List<string>();
			}
			set
			{
				m_entity_ids = ((value == null) ? new List<string>() : value);
			}
		}

		public bool anyEntity => entities.Count > 0;

		public Preview preview
		{
			get
			{
				Preview obj = ((m_preview == null) ? (m_preview = new Preview()) : m_preview);
				obj.state = this;
				return obj;
			}
		}

		public Metric metric
		{
			get
			{
				Metric obj = ((m_metric == null) ? (m_metric = new Metric()) : m_metric);
				obj.state = this;
				return obj;
			}
		}

		public Physics physics
		{
			get
			{
				Physics obj = ((m_physics == null) ? (m_physics = new Physics()) : m_physics);
				obj.state = this;
				return obj;
			}
		}

		public Mouse mouse
		{
			get
			{
				if (m_mouse != null)
				{
					return m_mouse;
				}
				return m_mouse = new Mouse();
			}
		}

		public bool inputFocus
		{
			get
			{
				DRLInputFieldView dRLInputFieldView = null;
				if ((bool)UINavigation.focus)
				{
					dRLInputFieldView = Hierarchy.FindReverse<DRLInputFieldView>(UINavigation.focus.transform);
				}
				if ((bool)dRLInputFieldView)
				{
					return true;
				}
				InputField inputField = null;
				if (EventSystem.current.currentSelectedGameObject != null)
				{
					inputField = EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();
				}
				return inputField != null;
			}
		}

		public bool ActivePreview
		{
			get
			{
				if (!preview.target)
				{
					return false;
				}
				return preview.target.childCount > 0;
			}
		}

		public bool PreviewVisible
		{
			get
			{
				return m_previewEnabled;
			}
			set
			{
				m_previewEnabled = value;
				if (ActivePreview)
				{
					preview.target.gameObject.SetActive(m_previewEnabled);
				}
			}
		}

		public bool IsShift
		{
			get
			{
				if (!Input.GetKey(KeyCode.LeftShift))
				{
					return Input.GetKey(KeyCode.RightShift);
				}
				return true;
			}
		}

		public bool IsAlt
		{
			get
			{
				if (!Input.GetKey(KeyCode.LeftAlt))
				{
					return Input.GetKey(KeyCode.RightAlt);
				}
				return true;
			}
		}

		public bool IsCtrl
		{
			get
			{
				if (!Input.GetKey(KeyCode.LeftControl))
				{
					return Input.GetKey(KeyCode.RightControl);
				}
				return true;
			}
		}

		public bool IsCtrlDown
		{
			get
			{
				if (!Input.GetKeyDown(KeyCode.LeftControl))
				{
					return Input.GetKeyDown(KeyCode.RightControl);
				}
				return true;
			}
		}

		public bool IsCtrlUp
		{
			get
			{
				if (!Input.GetKeyUp(KeyCode.LeftControl))
				{
					return Input.GetKeyUp(KeyCode.RightControl);
				}
				return true;
			}
		}

		public bool IsCommand
		{
			get
			{
				if (!Input.GetKey(KeyCode.LeftCommand))
				{
					return Input.GetKey(KeyCode.RightCommand);
				}
				return true;
			}
		}

		public bool IsCommandDown
		{
			get
			{
				if (!Input.GetKeyDown(KeyCode.LeftCommand))
				{
					return Input.GetKeyDown(KeyCode.RightCommand);
				}
				return true;
			}
		}

		public bool IsCommandUp
		{
			get
			{
				if (!Input.GetKeyUp(KeyCode.LeftCommand))
				{
					return Input.GetKeyUp(KeyCode.RightCommand);
				}
				return true;
			}
		}

		public bool IsCameraMove
		{
			get
			{
				if (input != MEInputStateType.Action)
				{
					return input == MEInputStateType.None;
				}
				return true;
			}
		}

		public bool AllowSelect
		{
			get
			{
				if (input != MEInputStateType.Action)
				{
					return false;
				}
				if (render != MERenderStateType.Scene)
				{
					return false;
				}
				if (!mouse.focus)
				{
					return false;
				}
				if (ActivePreview)
				{
					return false;
				}
				return true;
			}
		}

		public bool ApplyBoxSelect
		{
			get
			{
				if (ActiveDragSelect)
				{
					return AllowSelect;
				}
				return false;
			}
		}

		public bool AllowHilight
		{
			get
			{
				if (input != MEInputStateType.Action)
				{
					return false;
				}
				if (render != MERenderStateType.Scene)
				{
					return false;
				}
				if (entities.Count <= 0)
				{
					return false;
				}
				if (!mouse.focus)
				{
					return false;
				}
				if (ActivePreview)
				{
					if (entityTags.Contains(MapAssetType.LayoutTool))
					{
						return true;
					}
					return false;
				}
				return true;
			}
		}

		public bool AllowDragSelect
		{
			get
			{
				bool result = ActiveDragSelect;
				if (input != MEInputStateType.Action)
				{
					result = false;
				}
				if (render != MERenderStateType.Scene)
				{
					result = false;
				}
				return result;
			}
		}

		public bool AllowSelectBoxRefresh
		{
			get
			{
				if (AllowSelect)
				{
					return ActiveDragSelect;
				}
				return false;
			}
		}

		public bool AllowSelectCancel
		{
			get
			{
				if (!ActivePreview)
				{
					return AllowSelect;
				}
				return false;
			}
		}

		public bool AllowCreate
		{
			get
			{
				if (!AllowDragSelect && ActivePreview)
				{
					return PreviewVisible;
				}
				return false;
			}
		}

		public bool ApplySelect
		{
			get
			{
				if (AllowSelect)
				{
					return !ActivePreview;
				}
				return false;
			}
		}

		public bool AllowPreviewInput
		{
			get
			{
				if (!ActiveDragSelect && ActivePreview)
				{
					return PreviewVisible;
				}
				return false;
			}
		}

		public bool AllowPreviewRefresh
		{
			get
			{
				if (!ActiveDragSelect)
				{
					return ActivePreview;
				}
				return false;
			}
		}

		public bool AllowActionChange
		{
			get
			{
				bool result = true;
				if (render != MERenderStateType.Scene)
				{
					result = false;
				}
				return result;
			}
		}

		public bool AllowMetricsTools
		{
			get
			{
				bool result = true;
				if (render != MERenderStateType.Scene)
				{
					result = false;
				}
				if (ActivePreview)
				{
					return false;
				}
				return result;
			}
		}

		public bool AllowLayoutTools
		{
			get
			{
				bool result = true;
				if (render != MERenderStateType.Scene)
				{
					result = false;
				}
				return result;
			}
		}

		public bool AllowSceneControls
		{
			get
			{
				if (!AllowMetricsTools)
				{
					return false;
				}
				return true;
			}
		}

		private string EntityToId(MAEntity it)
		{
			return it.id;
		}

		private int EntityIdSort(MAEntity a, MAEntity b)
		{
			return string.Compare(a.id, b.id);
		}

		public void SetEntities(List<MAEntity> p_list)
		{
			entityTags.Clear();
			entities.Clear();
			entities.AddRange(p_list);
			entities.Sort(EntityIdSort);
			entitiesIds = entities.ConvertAll<string>(EntityToId);
			for (int i = 0; i < entities.Count; i++)
			{
				if ((bool)entities[i])
				{
					PushEntityTags(entities[i].tags);
				}
			}
		}

		public void PushEntity(MAEntity p_item)
		{
			if ((bool)p_item && !entities.Contains(p_item))
			{
				entities.Add(p_item);
				entities.Sort(EntityIdSort);
				entitiesIds = entities.ConvertAll<string>(EntityToId);
				PushEntityTags(p_item.tags);
			}
		}

		public void PushEntityTags(List<MapAssetType> p_tags)
		{
			for (int i = 0; i < p_tags.Count; i++)
			{
				if (!entityTags.Contains(p_tags[i]))
				{
					entityTags.Add(p_tags[i]);
				}
			}
		}

		public void SetProperties(IList p_targets, List<string> p_list)
		{
			p_list.Clear();
			if (p_targets == null)
			{
				return;
			}
			for (int i = 0; i < p_targets.Count; i++)
			{
				object obj = p_targets[i];
				if (obj is MapAsset)
				{
					MapAsset mapAsset = obj as MapAsset;
					if ((bool)mapAsset)
					{
						mapAsset.Write();
						string item = mapAsset.data.ToJsonProperties(Application.isEditor);
						p_list.Add(item);
					}
				}
			}
		}

		public void SetProperties(List<string> p_list)
		{
			SetProperties(entities, p_list);
		}

		public void SetPropertiesFrom(IList p_targets)
		{
			SetProperties(p_targets, propertyFrom);
		}

		public void SetPropertiesTo(IList p_targets)
		{
			SetProperties(p_targets, propertyTo);
		}

		public void SetTransforms(IList p_targets, List<TransformVector> p_list)
		{
			p_list.Clear();
			if (p_targets == null)
			{
				return;
			}
			for (int i = 0; i < p_targets.Count; i++)
			{
				object obj = p_targets[i];
				if (obj is Component)
				{
					Transform p_target = (obj as Component).transform;
					TransformVector item = new TransformVector(p_target, p_local: true);
					p_list.Add(item);
				}
			}
		}

		public void SetTransforms(List<TransformVector> p_list)
		{
			SetTransforms(entities, p_list);
		}

		public void SetTransformsFrom(IList p_targets)
		{
			SetTransforms(p_targets, transformFrom);
		}

		public void SetTransformsTo(IList p_targets)
		{
			SetTransforms(p_targets, transformTo);
		}
	}
}
