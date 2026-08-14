using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MEPropertyInspector : MEInspector
	{
		[Serializable]
		public class AnchorProperty
		{
			public Transform target;

			public Vector3 position;

			public Vector3 rotation;

			public Vector3 scale;
		}

		[Serializable]
		public class LayoutProperty
		{
			private static Vector3 m_distribute_spacing;

			private static Vector3 m_orient_offset;

			public List<Transform> targets;

			private static Transform m_anchor;

			private static BoxCollider m_world_box;

			private static BoxCollider m_local_box;

			public Vector3 distributeSpacing
			{
				get
				{
					return m_distribute_spacing;
				}
				set
				{
					m_distribute_spacing = value;
				}
			}

			public Vector3 orientOffset
			{
				get
				{
					return m_orient_offset;
				}
				set
				{
					m_orient_offset = value;
				}
			}

			public Transform head
			{
				get
				{
					if (targets.Count > 0)
					{
						return targets[0];
					}
					return null;
				}
			}

			public Transform tail
			{
				get
				{
					if (targets.Count > 0)
					{
						return targets[targets.Count - 1];
					}
					return null;
				}
			}

			public Ray ray
			{
				get
				{
					if (!valid)
					{
						return new Ray(Vector3.zero, Vector3.forward);
					}
					return new Ray(head.position, (tail.position - head.position).normalized);
				}
			}

			public bool valid
			{
				get
				{
					if (!head)
					{
						return false;
					}
					if (!tail)
					{
						return false;
					}
					return head != tail;
				}
			}

			public Transform anchor
			{
				get
				{
					if ((bool)m_anchor)
					{
						return m_anchor;
					}
					m_anchor = new GameObject("inspector-layout-anchor").transform;
					return m_anchor;
				}
			}

			public BoxCollider worldBox
			{
				get
				{
					if ((bool)m_world_box)
					{
						return m_world_box;
					}
					m_world_box = new GameObject("inspector-world-box").AddComponent<BoxCollider>();
					m_world_box.enabled = false;
					m_world_box.gameObject.layer = 2;
					m_world_box.isTrigger = true;
					return m_world_box;
				}
			}

			public BoxCollider localBox
			{
				get
				{
					if ((bool)m_local_box)
					{
						return m_local_box;
					}
					m_local_box = new GameObject("inspector-local-box").AddComponent<BoxCollider>();
					m_local_box.enabled = false;
					m_local_box.gameObject.layer = 2;
					m_local_box.isTrigger = true;
					return m_local_box;
				}
			}

			public static Vector3 Abs(Vector3 v)
			{
				return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
			}

			public void Clear()
			{
				targets = new List<Transform>();
			}

			public void Set<T>(IList<T> p_list) where T : Component
			{
				Clear();
				List<Transform> list = ((p_list == null) ? null : new List<Transform>());
				if (list == null)
				{
					return;
				}
				for (int i = 0; i < p_list.Count; i++)
				{
					list.Add(p_list[i].transform);
				}
				if (list.Count > 0)
				{
					targets = list;
					Bounds b = GetBounds();
					worldBox.transform.position = b.center;
					worldBox.transform.localRotation = Quaternion.identity;
					worldBox.size = Abs(b.size);
					list.Sort((Transform ta, Transform tb) => (!(Vector3.Distance(ta.position, b.center) > Vector3.Distance(tb.position, b.center))) ? 1 : (-1));
					Vector3 hpos = list[0].position;
					list.Sort((Transform ta, Transform tb) => (!(Vector3.Distance(ta.position, hpos) < Vector3.Distance(tb.position, hpos))) ? 1 : (-1));
					Vector3 position = head.position;
					anchor.position = position;
					Vector3 vector = (tail.position - head.position).normalized;
					if (vector == Vector3.zero)
					{
						vector = Vector3.forward;
					}
					Vector3 vector2 = ((Mathf.Abs(Vector3.Dot(vector, Vector3.up)) > 0.99f) ? (-Vector3.forward) : Vector3.up);
					if (vector2 == Vector3.zero)
					{
						vector2 = Vector3.up;
					}
					anchor.localRotation = Quaternion.LookRotation(vector, vector2);
					b = GetBounds(p_world_space: false);
					localBox.transform.position = anchor.TransformPoint(b.center);
					localBox.transform.localRotation = anchor.localRotation;
					localBox.size = Abs(b.size);
				}
			}

			public void Distribute(bool p_world_space, float p_spacing, Vector3 p_mask, Vector3 p_direction)
			{
				if (targets.Count <= 0)
				{
					return;
				}
				bool num = Mathf.Abs(p_spacing) <= 0.0001f;
				Vector3 vdir = p_direction;
				Vector3 vector = Vector3.one * p_spacing;
				Vector3 size = GetBounds(p_world_space).size;
				size.Scale(vdir);
				if (num)
				{
					vector = size * (1f / (float)(targets.Count - 1));
				}
				List<Transform> list = new List<Transform>(targets);
				Vector3 c = list[0].position;
				list.Sort(delegate(Transform ta, Transform tb)
				{
					if (ta == tb)
					{
						return 0;
					}
					Vector3 vector6 = ta.position;
					if (!p_world_space)
					{
						vector6 = anchor.InverseTransformPoint(vector6);
					}
					Vector3 vector7 = tb.position;
					if (!p_world_space)
					{
						vector7 = anchor.InverseTransformPoint(vector7);
					}
					Vector3 lhs = vector6 - c;
					Vector3 lhs2 = vector7 - c;
					float num3 = Vector3.Dot(lhs, vdir);
					float num4 = Vector3.Dot(lhs2, vdir);
					return (!(num3 < num4)) ? 1 : (-1);
				});
				Vector3 vector2 = list[0].position;
				if (!p_world_space)
				{
					vector2 = anchor.InverseTransformPoint(vector2);
				}
				Vector3 vector3 = p_mask;
				Vector3 vector4 = vector2;
				for (int num2 = 0; num2 < list.Count; num2++)
				{
					Transform transform = list[num2];
					if ((bool)transform)
					{
						Vector3 position = transform.position;
						position = (p_world_space ? position : anchor.InverseTransformPoint(position));
						position.x = Mathf.Lerp(position.x, vector4.x, vector3.x);
						position.y = Mathf.Lerp(position.y, vector4.y, vector3.y);
						position.z = Mathf.Lerp(position.z, vector4.z, vector3.z);
						position = (p_world_space ? position : anchor.TransformPoint(position));
						transform.position = position;
						Vector3 vector5 = vector;
						vector5.Scale(vdir);
						vector4 += vector5;
					}
				}
			}

			public void Distribute(bool p_world_space, float p_spacing, Vector3 p_mask)
			{
				Distribute(p_world_space, p_spacing, p_mask, p_mask);
			}

			public void Align(bool p_world_space, Vector3 p_mask)
			{
				if (targets.Count <= 0)
				{
					return;
				}
				Bounds bounds = GetBounds(p_world_space);
				Vector3 vector = p_mask;
				for (int i = 0; i < targets.Count; i++)
				{
					Transform transform = targets[i];
					if ((bool)transform)
					{
						Vector3 position = transform.position;
						position = (p_world_space ? position : anchor.InverseTransformPoint(position));
						position.x = ((vector.x >= 0f) ? Mathf.Lerp(bounds.min.x, bounds.max.x, vector.x) : position.x);
						position.y = ((vector.y >= 0f) ? Mathf.Lerp(bounds.min.y, bounds.max.y, vector.y) : position.y);
						position.z = ((vector.z >= 0f) ? Mathf.Lerp(bounds.min.z, bounds.max.z, vector.z) : position.z);
						position = (p_world_space ? position : anchor.TransformPoint(position));
						transform.position = position;
					}
				}
			}

			public void Orient(bool p_smooth, Vector3 p_offset, Vector3 p_steps)
			{
				List<Transform> list = targets;
				if (list == null || list.Count <= 0)
				{
					return;
				}
				int num = 0;
				list = new List<Transform>(list);
				Transform transform = head;
				List<Transform> list2 = new List<Transform>();
				while (list.Count > 0 && (bool)transform && num++ <= 800)
				{
					list2.Add(transform);
					list.Remove(transform);
					transform = FindClosest(transform, list, Vector3.zero);
				}
				list = list2;
				for (int i = 0; i < list.Count; i++)
				{
					list[i].name = "layout-orient-" + i;
				}
				Vector3 zero = Vector3.zero;
				for (int j = 0; j < list.Count; j++)
				{
					Transform transform2 = ((j - 1 < 0) ? null : list[j - 1]);
					Transform transform3 = list[j];
					Transform transform4 = ((j + 1 >= list.Count) ? null : list[j + 1]);
					if ((bool)transform2 || (bool)transform4)
					{
						Transform transform5 = (transform2 ? transform2 : transform3);
						Transform transform6 = (transform4 ? transform4 : transform3);
						if (p_smooth)
						{
							transform5 = (transform4 ? transform3 : transform2);
							transform6 = (transform4 ? transform4 : transform3);
						}
						Vector3 normalized = (transform6.position - transform5.position).normalized;
						Vector3 upwards = ((Mathf.Abs(Vector3.Dot(normalized, Vector3.up)) > 0.99f) ? (-Vector3.forward) : Vector3.up);
						Vector3 vector = p_offset + zero;
						zero += p_steps;
						Quaternion quaternion = Quaternion.LookRotation(normalized, upwards);
						Quaternion quaternion2 = Quaternion.AngleAxis(vector.y, Vector3.up) * Quaternion.AngleAxis(vector.x, Vector3.right) * Quaternion.AngleAxis(vector.z, Vector3.forward);
						transform3.localRotation = quaternion * quaternion2;
					}
				}
			}

			public void Orient(bool p_smooth, Vector3 p_offset)
			{
				Orient(p_smooth, p_offset, Vector3.zero);
			}

			public void Orient(bool p_smooth)
			{
				Orient(p_smooth, Vector3.zero, Vector3.zero);
			}

			private Transform FindClosest(Transform p_target, IList<Transform> p_list, Vector3 p_direction)
			{
				Transform result = null;
				if (!p_target)
				{
					return result;
				}
				float num = 99999f;
				Vector3 rhs = ((p_direction.magnitude <= 0f) ? Vector3.zero : p_direction.normalized);
				for (int i = 0; i < p_list.Count; i++)
				{
					Transform transform = p_list[i];
					if ((bool)transform && !(transform == p_target))
					{
						Vector3 lhs = transform.position - p_target.position;
						float num2 = ((rhs.magnitude <= 0f) ? lhs.magnitude : Vector3.Dot(lhs, rhs));
						if (num2 < num)
						{
							result = transform;
							num = num2;
						}
					}
				}
				return result;
			}

			private Transform FindClosest(Transform p_target, IList<Transform> p_list)
			{
				return FindClosest(p_target, p_list, Vector3.zero);
			}

			public Bounds GetBounds(bool p_world_space = true)
			{
				Bounds result = default(Bounds);
				List<Transform> list = targets;
				if (list.Count <= 0)
				{
					return result;
				}
				bool flag = true;
				for (int i = 0; i < list.Count; i++)
				{
					if ((bool)list[i])
					{
						Vector3 vector = list[i].position;
						if (!p_world_space)
						{
							vector = anchor.InverseTransformPoint(vector);
						}
						if (flag)
						{
							result.center = vector;
							result.size = Vector3.zero;
							flag = false;
						}
						else
						{
							result.Encapsulate(vector);
						}
					}
				}
				return result;
			}
		}

		[Header("Properties")]
		[SerializeField]
		private AnchorProperty m_anchor;

		[SerializeField]
		private LayoutProperty m_layout;

		private bool m_ignore_change;

		private Texture2D m_texture_data;

		private Activity m_refresh_field_action;

		public AnchorProperty anchor
		{
			get
			{
				if (m_anchor != null)
				{
					return m_anchor;
				}
				return m_anchor = new AnchorProperty();
			}
		}

		public LayoutProperty layout
		{
			get
			{
				if (m_layout != null)
				{
					return m_layout;
				}
				return m_layout = new LayoutProperty();
			}
		}

		public override void OnInspectorEnable()
		{
			RefreshTransformAnchor(p_instantiate: true);
			RefreshFields(panel.fieldIds);
			layout.Set(base.targets);
		}

		public override void OnInspectorDisable()
		{
			if ((bool)anchor.target)
			{
				UnityEngine.Object.Destroy(anchor.target.gameObject);
			}
			anchor.target = null;
		}

		protected void RefreshFields(IList<string> p_fields, float p_delay)
		{
			if (m_refresh_field_action != null)
			{
				m_refresh_field_action.Stop();
			}
			m_refresh_field_action = Activity.RunOnce(delegate
			{
				RefreshFields(p_fields);
			}, p_delay);
		}

		protected void RefreshFields(IList<string> p_fields)
		{
			MapData data = base.editor.model.data;
			LevelModel level = base.editor.controller.game.model.level;
			LevelSettings settings = base.editor.controller.game.model.level.settings;
			MEStateModel state = base.editor.model.state;
			bool flag = base.targets.Count > 1;
			bool flag2 = base.targets.Count == 1;
			bool flag3 = base.targets.Count <= 0;
			bool flag4 = base.targets.Count > 0;
			bool flag5 = (flag && IsMultiTargetSameGUID()) || flag2;
			bool flag6 = (flag && IsMultiTargetSameType()) || flag2;
			bool isDeveloper = base.editor.app.model.storage.state.player.profile.isDeveloper;
			bool flag7 = !tags.Contains(MapAssetType.NoTranformMove);
			bool flag8 = !tags.Contains(MapAssetType.NoTranformRotate);
			bool flag9 = tags.Contains(MapAssetType.ModularScale);
			bool flag10 = !tags.Contains(MapAssetType.NoTranformScale) && !flag9;
			bool num = !tags.Contains(MapAssetType.NoGroupMove) && flag7;
			bool flag11 = !tags.Contains(MapAssetType.NoGroupRotate) && flag8;
			bool flag12 = !tags.Contains(MapAssetType.NoGroupScale) && flag10;
			if (!num && flag)
			{
				flag7 = false;
			}
			if (!flag11 && flag)
			{
				flag8 = false;
			}
			if (!flag12 && flag)
			{
				flag10 = false;
			}
			bool flag13 = !tags.Contains(MapAssetType.NoLayout);
			bool flag14 = !tags.Contains(MapAssetType.NoPhysics);
			bool snapMap = state.metric.snapMap;
			if (tags.TrueForAll(IsNoForceGrid))
			{
				state.metric.snapMap = false;
			}
			state.metric.IsSnapMove();
			state.metric.IsSnapRotate();
			float snapMoveUnit = state.metric.GetSnapMoveUnit();
			float snapRotateUnit = state.metric.GetSnapRotateUnit();
			state.metric.snapMap = snapMap;
			m_ignore_change = true;
			panel.SetFieldsNotificationEnabled(p_flag: false);
			for (int i = 0; i < panel.fields.Count; i++)
			{
				Component component = panel.fields[i];
				string text = component.name;
				bool flag15 = text.Contains("property-space");
				bool flag16 = text.Contains("project-space");
				if (flag15 || flag16)
				{
					SetFieldActive(component, p_flag: false);
					if (flag3)
					{
						SetFieldActive(component, flag16);
					}
					if (flag4)
					{
						SetFieldActive(component, flag15);
					}
				}
				if (!p_fields.Contains(text))
				{
					continue;
				}
				if (!flag3)
				{
					switch (text)
					{
					case "transform-position":
					{
						DRLVectorFieldView component30 = Hierarchy.GetComponent<DRLVectorFieldView>(component.gameObject);
						SetFieldActive(component30, flag7);
						component30.SetSnap(Vector3.one * snapMoveUnit);
						component30.Set(anchor.position);
						break;
					}
					case "transform-rotation":
					{
						DRLVectorFieldView component14 = Hierarchy.GetComponent<DRLVectorFieldView>(component.gameObject);
						SetFieldActive(component14, flag8);
						component14.SetSnap(Vector3.one * snapRotateUnit);
						component14.Set(anchor.rotation);
						break;
					}
					case "transform-scale":
					{
						DRLVectorFieldView component29 = Hierarchy.GetComponent<DRLVectorFieldView>(component.gameObject);
						SetFieldActive(component29, flag10);
						component29.Set(anchor.scale);
						break;
					}
					case "transform-modular-scale":
					{
						DRLVectorFieldView component37 = Hierarchy.GetComponent<DRLVectorFieldView>(component.gameObject);
						if ((bool)component37)
						{
							MAModularScaleRenderer mAModularScaleRenderer = GetTarget<MAModularScaleRenderer>(0);
							if ((bool)mAModularScaleRenderer)
							{
								SetFieldActive(component37, flag9 && (bool)mAModularScaleRenderer && !flag);
								component37.SetMinValue(mAModularScaleRenderer.modules.min);
								component37.SetMaxValue(mAModularScaleRenderer.modules.max);
								component37.Set(mAModularScaleRenderer.moduleScale);
							}
						}
						break;
					}
					case "transform-snap-ground":
					{
						UIElementView component17 = Hierarchy.GetComponent<UIElementView>(component.gameObject);
						SetFieldActive(component17, flag7);
						break;
					}
					case "physics-simulation-toggle":
					{
						UIElementView component19 = Hierarchy.GetComponent<UIElementView>(component.gameObject);
						SetFieldActive(component19, flag7 && flag14);
						if (flag14)
						{
							bool flag36 = FindAll<MEPreviewPhysics>().Count > 0;
							component19.GetComponent<SwitcherComponent>().index = (flag36 ? 1 : 0);
						}
						break;
					}
					case "attrib-ruler":
						if (flag13)
						{
							DRLToggleView component35 = Hierarchy.GetComponent<DRLToggleView>(component.gameObject);
							SetFieldActive(component35, !flag);
							MAEntity mAEntity = GetTarget<MAEntity>(0);
							if ((bool)mAEntity)
							{
								component35.toggle.isOn = (mAEntity.attribs & MDEntityAttribFlag.Ruler) != 0;
								component35.SetState(component35.toggle.isOn);
							}
						}
						break;
					case "attrib-layout":
						if (flag13)
						{
							DRLToggleView component25 = Hierarchy.GetComponent<DRLToggleView>(component.gameObject);
							MARenderer spline2 = GetTarget<MARenderer>(0);
							if (spline2 is MASplineControlPoint)
							{
								spline2 = (spline2 as MASplineControlPoint).spline;
							}
							SetFieldActive(component25, !flag && (bool)spline2);
							if ((bool)spline2)
							{
								component25.toggle.isOn = spline2.isLayout;
								component25.SetState(component25.toggle.isOn);
							}
						}
						break;
					case "gate-attribs-0":
						SetFieldActive(component, p_flag: true);
						break;
					case "gate-enabled":
					{
						DRLToggleView component31 = Hierarchy.GetComponent<DRLToggleView>(component.gameObject);
						MAGate mAGate3 = GetTarget<MAGate>(0);
						bool flag46 = (object)mAGate3 != null;
						SetFieldActive(component31, flag6 && flag46);
						if (flag46 && (bool)mAGate3)
						{
							component31.toggle.isOn = mAGate3.isTrigger;
							component31.SetState(component31.toggle.isOn);
						}
						break;
					}
					case "gate-respawn-visible":
					{
						DRLToggleView component11 = Hierarchy.GetComponent<DRLToggleView>(component.gameObject);
						MAGate mAGate = GetTarget<MAGate>(0);
						bool flag27 = (object)mAGate != null;
						if (flag27)
						{
							bool flag28 = mAGate.GetRespawnGuide();
							SetFieldActive(component11, flag6 && flag27);
							if ((bool)mAGate)
							{
								component11.toggle.isOn = flag28 && mAGate.isRespawnVisible;
								component11.SetState(component11.toggle.isOn);
							}
						}
						break;
					}
					case "gate-lap-start":
					case "gate-lap-end":
					case "gate-finish":
					{
						DRLToggleView component12 = Hierarchy.GetComponent<DRLToggleView>(component.gameObject);
						MAGate mAGate2 = GetTarget<MAGate>(0);
						bool flag29 = (object)mAGate2 != null;
						SetFieldActive(component12, !flag && flag29);
						if (!flag && flag29 && (bool)mAGate2)
						{
							bool isOn2 = false;
							switch (text)
							{
							case "gate-finish":
								isOn2 = mAGate2.isFinish;
								break;
							case "gate-lap-start":
								isOn2 = mAGate2.isLapStart;
								break;
							case "gate-lap-end":
								isOn2 = mAGate2.isLapEnd;
								break;
							}
							component12.toggle.isOn = isOn2;
							component12.SetState(component12.toggle.isOn);
						}
						break;
					}
					case "gate-mode":
					{
						DRLStepperView component36 = Hierarchy.GetComponent<DRLStepperView>(component.gameObject);
						MAGate mAGate4 = GetTarget<MAGate>(0);
						bool flag50 = (object)mAGate4 != null;
						SetFieldActive(component36, flag6 && flag50);
						if (flag50)
						{
							int gateMode = (int)mAGate4.gateMode;
							component36.index = gateMode;
							component36.Refresh();
						}
						break;
					}
					case "spline-attribs-0":
					case "spline-attribs-1":
					case "spline-attribs-2":
					case "spline-attribs-3":
					case "spline-attribs-4":
					{
						MASplineControlPoint mASplineControlPoint9 = GetTarget<MASplineControlPoint>(0);
						bool flag39 = (bool)mASplineControlPoint9 && (bool)mASplineControlPoint9.spline;
						if ((bool)mASplineControlPoint9 && (bool)mASplineControlPoint9.spline)
						{
							switch (text)
							{
							case "spline-attribs-1":
							case "spline-attribs-2":
							case "spline-attribs-3":
								flag39 = mASplineControlPoint9.spline.splineCategory == SplineCategory.Visual;
								break;
							case "spline-attribs-4":
								flag39 = mASplineControlPoint9.spline.splineCategory == SplineCategory.CourseCamera;
								break;
							}
						}
						SetFieldActive(component, !flag && flag39);
						break;
					}
					case "spline-category":
					{
						DRLStepperView component39 = Hierarchy.GetComponent<DRLStepperView>(component.gameObject);
						MASplineControlPoint mASplineControlPoint13 = GetTarget<MASplineControlPoint>(0);
						MASpline mASpline9 = (mASplineControlPoint13 ? mASplineControlPoint13.spline : null);
						SetFieldActive(component39, !flag && mASpline9 != null);
						if ((bool)mASpline9)
						{
							int splineCategory = (int)mASpline9.splineCategory;
							component39.index = Mathf.Clamp(splineCategory, component39.min, component39.max);
							component39.Refresh();
							component39.Refresh();
						}
						break;
					}
					case "spline-smooth":
					{
						DRLToggleView component4 = Hierarchy.GetComponent<DRLToggleView>(component.gameObject);
						MASplineControlPoint mASplineControlPoint2 = GetTarget<MASplineControlPoint>(0);
						MASpline mASpline = (mASplineControlPoint2 ? mASplineControlPoint2.spline : null);
						SetFieldActive(component4, !flag && mASpline != null);
						if ((bool)mASpline && (bool)mASplineControlPoint2)
						{
							component4.toggle.isOn = mASpline.splineMode != SplineType.Linear;
							component4.SetState(component4.toggle.isOn);
						}
						break;
					}
					case "spline-loop":
					{
						DRLToggleView component33 = Hierarchy.GetComponent<DRLToggleView>(component.gameObject);
						MASplineControlPoint mASplineControlPoint12 = GetTarget<MASplineControlPoint>(0);
						MASpline mASpline8 = (mASplineControlPoint12 ? mASplineControlPoint12.spline : null);
						SetFieldActive(component33, !flag && mASpline8 != null);
						if ((bool)mASpline8 && (bool)mASplineControlPoint12)
						{
							component33.toggle.isOn = mASpline8.isLoop;
							component33.SetState(component33.toggle.isOn);
						}
						break;
					}
					case "spline-control-point-index":
					{
						MASplineControlPoint mASplineControlPoint3 = GetTarget<MASplineControlPoint>(0);
						MASpline mASpline2 = (mASplineControlPoint3 ? mASplineControlPoint3.spline : null);
						int num2 = (mASpline2 ? mASpline2.transform.childCount : 0);
						bool flag20 = num2 > 0;
						DRLNumberFieldView component5 = Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject);
						SetFieldActive(component5, flag20 && !flag && mASpline2 != null);
						if ((bool)mASpline2)
						{
							mASpline2.RefreshHierarchy();
							component5.minValue = 1f;
							component5.maxValue = num2;
							component5.value = mASplineControlPoint3.index + 1;
							component5.Refresh();
						}
						break;
					}
					case "spline-course-camera-index":
					{
						MASplineControlPoint mASplineControlPoint4 = GetTarget<MASplineControlPoint>(0);
						MASpline mASpline3 = (mASplineControlPoint4 ? mASplineControlPoint4.spline : null);
						bool flag23 = (bool)mASpline3 && mASpline3.splineCategory == SplineCategory.CourseCamera;
						List<MASpline> list4 = base.editor.scene.FindAll((MASpline it) => it.splineCategory == SplineCategory.CourseCamera);
						int count = list4.Count;
						bool flag24 = count > 0;
						DRLNumberFieldView component8 = Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject);
						SetFieldActive(component8, flag24 && !flag && mASpline3 != null && flag23);
						if (!mASpline3)
						{
							break;
						}
						bool flag25 = false;
						int splineCourseCameraIndex = mASpline3.splineCourseCameraIndex;
						for (int num3 = 0; num3 < list4.Count; num3++)
						{
							if (list4[num3] != mASpline3 && list4[num3].splineCourseCameraIndex == splineCourseCameraIndex)
							{
								flag25 = true;
								break;
							}
						}
						if (flag25)
						{
							mASpline3.splineCourseCameraIndex = list4.Count - 1;
						}
						component8.minValue = 1f;
						component8.maxValue = count;
						component8.value = mASpline3.splineCourseCameraIndex + 1;
						component8.Refresh();
						break;
					}
					case "spline-snap-gates":
					{
						MASplineControlPoint mASplineControlPoint11 = GetTarget<MASplineControlPoint>(0);
						MASpline mASpline7 = (mASplineControlPoint11 ? mASplineControlPoint11.spline : null);
						bool flag43 = (((bool)mASpline7 && mASpline7.transform.childCount != 0) ? 1 : 0) > (false ? 1 : 0);
						SetFieldActive(component, flag43 && !flag && mASpline7 != null);
						break;
					}
					case "spl-course-camera-preview":
					{
						MESplineCourseCameraPreviewInspector component22 = component.GetComponent<MESplineCourseCameraPreviewInspector>();
						MASplineControlPoint mASplineControlPoint10 = GetTarget<MASplineControlPoint>(0);
						MASpline mASpline6 = (mASplineControlPoint10 ? mASplineControlPoint10.spline : null);
						bool flag42 = (bool)mASpline6 && mASpline6.splineCategory == SplineCategory.CourseCamera;
						SetFieldActive(component, !flag && flag42);
						if ((bool)mASplineControlPoint10 && (bool)mASpline6)
						{
							component22.Init(base.editor);
							component22.Stop();
							component22.RenderLoop(mASpline6, mASplineControlPoint10.transform);
						}
						break;
					}
					case "spline-snap-closest-gate":
					{
						MASplineControlPoint mASplineControlPoint = GetTarget<MASplineControlPoint>(0);
						if ((bool)mASplineControlPoint)
						{
							_ = mASplineControlPoint.spline;
						}
						SetFieldActive(component, !flag && (bool)mASplineControlPoint && (bool)mASplineControlPoint);
						break;
					}
					case "spline-course-camera-speed":
					case "spline-course-camera-fov":
					case "spline-start-width":
					case "spline-end-width":
					case "spline-thickness":
					case "spline-alpha":
					{
						MASplineControlPoint mASplineControlPoint7 = GetTarget<MASplineControlPoint>(0);
						MASpline mASpline4 = (mASplineControlPoint7 ? mASplineControlPoint7.spline : null);
						bool flag33 = (((bool)mASpline4 && mASpline4.transform.childCount != 0) ? 1 : 0) > (false ? 1 : 0);
						DRLNumberFieldView component18 = Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject);
						SetFieldActive(component18, flag33 && !flag && mASpline4 != null);
						if ((bool)mASpline4)
						{
							float value = 0f;
							switch (text)
							{
							case "spline-start-width":
								value = mASpline4.splineStartWidth;
								break;
							case "spline-end-width":
								value = mASpline4.splineEndWidth;
								break;
							case "spline-thickness":
								value = mASpline4.splineThickness;
								break;
							case "spline-alpha":
								value = mASpline4.splineAlpha * 100f;
								break;
							case "spline-course-camera-speed":
								value = mASpline4.splineCourseCameraSpeed;
								break;
							case "spline-course-camera-fov":
								value = mASpline4.splineCourseCameraFOV;
								break;
							}
							component18.value = value;
							component18.Refresh();
						}
						break;
					}
					case "spline-snap-select-next":
					{
						bool flag32 = true;
						DRLToggleView component16 = Hierarchy.GetComponent<DRLToggleView>(component.gameObject);
						MASplineControlPoint mASplineControlPoint6 = GetTarget<MASplineControlPoint>(0);
						if ((bool)mASplineControlPoint6 && (bool)mASplineControlPoint6.spline && mASplineControlPoint6.spline.splineCategory != SplineCategory.CourseCamera)
						{
							flag32 = false;
						}
						SetFieldActive(component16, flag32);
						if (flag32)
						{
							component16.toggle.isOn = MASpline.splineSnapSelectNext;
							component16.SetState(component16.toggle.isOn);
						}
						break;
					}
					case "transform-snap-camera":
					{
						MASplineControlPoint mASplineControlPoint8 = GetTarget<MASplineControlPoint>(0);
						if ((bool)mASplineControlPoint8)
						{
							MASpline mASpline5 = (mASplineControlPoint8 ? mASplineControlPoint8.spline : null);
							bool flag34 = (((bool)mASpline5 && mASpline5.transform.childCount != 0) ? 1 : 0) > (false ? 1 : 0);
							bool flag35 = (bool)mASplineControlPoint8 && mASpline5.splineCategory == SplineCategory.CourseCamera;
							SetFieldActive(component, flag34 && !flag && mASpline5 != null && flag35);
						}
						else
						{
							MACameraToolControlPoint mACameraToolControlPoint7 = GetTarget<MACameraToolControlPoint>(0);
							SetFieldActive(component, !flag && (bool)mACameraToolControlPoint7);
						}
						break;
					}
					case "ctcp-attribs-1":
					{
						MACameraToolControlPoint mACameraToolControlPoint = GetTarget<MACameraToolControlPoint>(0);
						bool flag18 = !flag && (bool)mACameraToolControlPoint;
						if (flag18 && (bool)mACameraToolControlPoint && mACameraToolControlPoint.trackingMode == CameraToolTrackingMode.FPV)
						{
							flag18 = false;
						}
						SetFieldActive(component, flag18);
						break;
					}
					case "ctcp-attribs-0":
					{
						MACameraToolControlPoint mACameraToolControlPoint3 = GetTarget<MACameraToolControlPoint>(0);
						SetFieldActive(component, !flag && (bool)mACameraToolControlPoint3);
						break;
					}
					case "ct-attribs-0":
					{
						MACameraToolControlPoint mACameraToolControlPoint8 = GetTarget<MACameraToolControlPoint>(0);
						MACameraTool mACameraTool3 = (mACameraToolControlPoint8 ? mACameraToolControlPoint8.tool : null);
						bool flag37 = (bool)mACameraTool3 && mACameraTool3.mode == CameraToolMode.Wire;
						SetFieldActive(component, !flag && (bool)mACameraToolControlPoint8 && (bool)mACameraTool3 && flag37);
						break;
					}
					case "ctcp-camera-tracking-mode":
					{
						MACameraToolControlPoint mACameraToolControlPoint5 = GetTarget<MACameraToolControlPoint>(0);
						SetFieldActive(component, !flag && (bool)mACameraToolControlPoint5);
						if ((bool)mACameraToolControlPoint5)
						{
							DRLStepperView component9 = Hierarchy.GetComponent<DRLStepperView>(component.gameObject);
							if ((bool)mACameraToolControlPoint5.tool)
							{
								component9.max = ((mACameraToolControlPoint5.tool.mode == CameraToolMode.Wire) ? 3 : 4);
							}
							int trackingMode = (int)mACameraToolControlPoint5.trackingMode;
							component9.index = Mathf.Clamp(trackingMode, component9.min, component9.max);
							component9.Refresh();
						}
						break;
					}
					case "ctcp-camera-tracking-delay":
					{
						MACameraToolControlPoint mACameraToolControlPoint13 = GetTarget<MACameraToolControlPoint>(0);
						bool flag48 = !flag && (bool)mACameraToolControlPoint13;
						if (flag48 && (bool)mACameraToolControlPoint13)
						{
							CameraToolTrackingMode trackingMode2 = mACameraToolControlPoint13.trackingMode;
							if ((uint)(trackingMode2 - 1) > 2u && trackingMode2 == CameraToolTrackingMode.FPV)
							{
								flag48 = false;
							}
						}
						SetFieldActive(component, flag48);
						if ((bool)mACameraToolControlPoint13)
						{
							DRLNumberFieldView component34 = Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject);
							component34.value = mACameraToolControlPoint13.trackingDelay;
							component34.Refresh();
						}
						break;
					}
					case "ctcp-camera-orbit-angle":
					{
						MACameraToolControlPoint mACameraToolControlPoint12 = GetTarget<MACameraToolControlPoint>(0);
						bool flag44 = !flag && (bool)mACameraToolControlPoint12;
						if (flag44 && (bool)mACameraToolControlPoint12)
						{
							switch (mACameraToolControlPoint12.trackingMode)
							{
							case CameraToolTrackingMode.Static:
							case CameraToolTrackingMode.LookAt:
							case CameraToolTrackingMode.FPV:
								flag44 = false;
								break;
							}
						}
						SetFieldActive(component, flag44);
						if ((bool)mACameraToolControlPoint12)
						{
							Hierarchy.GetComponent<DRLVectorFieldView>(component.gameObject).Set(mACameraToolControlPoint12.cameraOrbitAngle);
						}
						break;
					}
					case "ctcp-camera-distance":
					{
						MACameraToolControlPoint mACameraToolControlPoint9 = GetTarget<MACameraToolControlPoint>(0);
						bool flag41 = !flag && (bool)mACameraToolControlPoint9;
						if (flag41 && (bool)mACameraToolControlPoint9)
						{
							CameraToolTrackingMode trackingMode2 = mACameraToolControlPoint9.trackingMode;
							if ((uint)(trackingMode2 - 1) > 2u && trackingMode2 == CameraToolTrackingMode.FPV)
							{
								flag41 = false;
							}
						}
						SetFieldActive(component, flag41);
						if ((bool)mACameraToolControlPoint9)
						{
							DRLNumberFieldView component21 = Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject);
							component21.value = mACameraToolControlPoint9.cameraDistance;
							component21.Refresh();
						}
						break;
					}
					case "ctcp-camera-fov":
					{
						MACameraToolControlPoint mACameraToolControlPoint2 = GetTarget<MACameraToolControlPoint>(0);
						bool flag19 = !flag && (bool)mACameraToolControlPoint2;
						if (flag19 && (bool)mACameraToolControlPoint2 && mACameraToolControlPoint2.trackingMode == CameraToolTrackingMode.FPV)
						{
							flag19 = false;
						}
						SetFieldActive(component, flag19);
						if ((bool)mACameraToolControlPoint2)
						{
							DRLNumberFieldView component3 = Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject);
							component3.value = mACameraToolControlPoint2.fov;
							component3.Refresh();
						}
						break;
					}
					case "ctcp-camera-offset":
					{
						MACameraToolControlPoint mACameraToolControlPoint14 = GetTarget<MACameraToolControlPoint>(0);
						bool flag49 = !flag && (bool)mACameraToolControlPoint14;
						if (flag49 && (bool)mACameraToolControlPoint14)
						{
							CameraToolTrackingMode trackingMode2 = mACameraToolControlPoint14.trackingMode;
							if ((uint)(trackingMode2 - 3) <= 1u)
							{
								flag49 = false;
							}
						}
						SetFieldActive(component, flag49);
						if ((bool)mACameraToolControlPoint14)
						{
							Hierarchy.GetComponent<DRLVectorFieldView>(component.gameObject).Set(mACameraToolControlPoint14.cameraOffset);
						}
						break;
					}
					case "ct-index":
					{
						MACameraToolControlPoint mACameraToolControlPoint11 = GetTarget<MACameraToolControlPoint>(0);
						MACameraTool mACameraTool5 = (mACameraToolControlPoint11 ? mACameraToolControlPoint11.tool : null);
						DRLNumberFieldView component24 = Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject);
						SetFieldActive(component24, !flag && mACameraTool5 != null);
						if ((bool)mACameraTool5)
						{
							List<MACameraTool> list6 = base.editor.scene.FindCameraTools();
							component24.minValue = 1f;
							component24.maxValue = list6.Count;
							component24.value = mACameraTool5.index + 1;
							component24.Refresh();
						}
						break;
					}
					case "ct-preview":
					{
						MECameraToolPreviewInspector component15 = component.GetComponent<MECameraToolPreviewInspector>();
						MACameraToolControlPoint mACameraToolControlPoint6 = GetTarget<MACameraToolControlPoint>(0);
						MACameraTool mACameraTool2 = (mACameraToolControlPoint6 ? mACameraToolControlPoint6.tool : null);
						SetFieldActive(component, !flag && (bool)mACameraToolControlPoint6 && (bool)mACameraTool2);
						if ((bool)mACameraToolControlPoint6 && (bool)mACameraTool2)
						{
							component15.Init(base.editor);
							component15.RenderLoop(mACameraToolControlPoint6);
						}
						break;
					}
					case "ct-camera-easing-help":
					case "ct-camera-easing-test":
					{
						MACameraToolControlPoint mACameraToolControlPoint4 = GetTarget<MACameraToolControlPoint>(0);
						MACameraTool mACameraTool = (mACameraToolControlPoint4 ? mACameraToolControlPoint4.tool : null);
						SetFieldActive(component, !flag && (bool)mACameraToolControlPoint4 && (bool)mACameraTool);
						break;
					}
					case "ct-camera-easing":
					{
						MACameraToolControlPoint mACameraToolControlPoint10 = GetTarget<MACameraToolControlPoint>(0);
						MACameraTool mACameraTool4 = (mACameraToolControlPoint10 ? mACameraToolControlPoint10.tool : null);
						MACameraToolAnimation mACameraToolAnimation = (mACameraTool4 ? mACameraTool4.animation : null);
						SetFieldActive(component, !flag && (bool)mACameraToolControlPoint10 && (bool)mACameraTool4 && (bool)mACameraToolAnimation);
						if (!mACameraToolControlPoint10 || !mACameraTool4 || !mACameraToolAnimation)
						{
							break;
						}
						DRLStepperView component23 = Hierarchy.GetComponent<DRLStepperView>(component.gameObject);
						if (component23.labels.Length == 0)
						{
							component23.labels = mACameraToolAnimation.labels.ToArray();
							for (int num4 = 0; num4 < component23.labels.Length; num4++)
							{
								component23.labels[num4] = component23.labels[num4].ToUpper();
							}
							component23.min = 0;
							component23.max = component23.labels.Length - 1;
						}
						int indexById = mACameraToolAnimation.GetIndexById(mACameraTool4.easingMode);
						component23.index = Mathf.Clamp(indexById, component23.min, component23.max);
						component23.Refresh();
						break;
					}
					case "collectable-attribs-0":
					case "collectable-attribs-1":
					{
						MACollectable mACollectable2 = GetTarget<MACollectable>(0);
						bool p_flag = (!flag || flag5) && mACollectable2 != null;
						SetFieldActive(component, p_flag);
						break;
					}
					case "collectable-mode":
					{
						DRLIntStepperView component7 = Hierarchy.GetComponent<DRLIntStepperView>(component.gameObject);
						MACollectable mACollectable = GetTarget<MACollectable>(0);
						bool flag22 = (!flag || flag5) && mACollectable != null;
						SetFieldActive(component, flag22);
						if (!flag22)
						{
							break;
						}
						List<MapCollectableMode> list = new List<MapCollectableMode>(new MapCollectableMode[2]
						{
							MapCollectableMode.Regular,
							MapCollectableMode.Kill
						});
						for (int j = 0; j < list.Count; j++)
						{
							if (mACollectable.GetStyleCount(list[j]) <= 0)
							{
								list.RemoveAt(j--);
							}
						}
						if (list.Count <= 0)
						{
							flag22 = false;
						}
						if (flag22)
						{
							List<string> list2 = list.ConvertAll(delegate(MapCollectableMode it)
							{
								int num16 = (int)it;
								return num16.ToString("00");
							});
							list2.Insert(0, "");
							List<int> list3 = list.ConvertAll((MapCollectableMode it) => (int)it);
							list3.Insert(0, -1);
							component7.labels = list2.ToArray();
							component7.values = list3;
							component7.min = 1;
							component7.max = list.Count;
							component7.index = list.IndexOf(mACollectable.collectableMode) + 1;
							component7.Refresh();
						}
						break;
					}
					case "collectable-style-0":
					{
						DRLStepperView component32 = Hierarchy.GetComponent<DRLStepperView>(component.gameObject);
						MACollectable mACollectable5 = GetTarget<MACollectable>(0);
						bool flag47 = (!flag || flag5) && mACollectable5 != null;
						int num5 = (mACollectable5 ? mACollectable5.GetStyleCount() : 0);
						if (num5 <= 0)
						{
							flag47 = false;
						}
						SetFieldActive(component, flag47);
						if (!flag47)
						{
							break;
						}
						for (int num6 = 0; num6 < base.targets.Count; num6++)
						{
							MACollectable mACollectable6 = (MACollectable)base.targets[num6];
							if ((bool)mACollectable6 && mACollectable6.collectableMode != mACollectable5.collectableMode)
							{
								flag47 = false;
								break;
							}
						}
						SetFieldActive(component, flag47);
						if (flag47)
						{
							component32.min = 1;
							component32.max = num5;
							component32.index = mACollectable5.collectableStyle + 1;
							component32.Refresh();
						}
						break;
					}
					case "collectable-score":
					{
						DRLNumberFieldView component27 = Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject);
						MACollectable mACollectable4 = GetTarget<MACollectable>(0);
						if ((bool)mACollectable4 && mACollectable4.collectableMode == MapCollectableMode.Regular)
						{
							_ = mACollectable4 != null;
						}
						if (0 == 0)
						{
							SetFieldActive(component, p_flag: false);
							break;
						}
						SetFieldActive(component, p_flag: true);
						component27.value = mACollectable4.score;
						component27.Refresh();
						break;
					}
					case "collectable-size":
					{
						DRLNumberFieldView component20 = Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject);
						MACollectable mACollectable3 = GetTarget<MACollectable>(0);
						bool flag40 = (!flag || flag5) && mACollectable3 != null;
						SetFieldActive(component, flag40);
						if (flag40)
						{
							component20.value = mACollectable3.size;
							component20.Refresh();
						}
						break;
					}
					case "lgt-apply":
					case "lgt-attribs-0":
					case "lgt-attribs-1":
					case "lgt-attribs-asset":
					case "lgt-attribs-shape":
					{
						MALayoutGeometryTool mALayoutGeometryTool4 = GetTarget<MALayoutGeometryTool>(0);
						bool flag38 = !flag && mALayoutGeometryTool4 != null;
						if (flag38)
						{
							LayoutGeometryType layoutType = mALayoutGeometryTool4.layoutType;
							switch (text)
							{
							case "lgt-apply":
								flag38 = !mALayoutGeometryTool4.isDefaultTemplate;
								break;
							case "lgt-attribs-shape":
								flag38 = layoutType != LayoutGeometryType.Grid;
								break;
							}
						}
						SetFieldActive(component, flag38);
						break;
					}
					case "lgt-stats":
					{
						MALayoutGeometryTool mALayoutGeometryTool3 = GetTarget<MALayoutGeometryTool>(0);
						bool flag31 = !flag && mALayoutGeometryTool3 != null;
						SetFieldActive(component, flag31);
						if (flag31)
						{
							Timer.Set(component.transform.Find("text").GetComponent<Text>(), "text", 0.15f, mALayoutGeometryTool3.layoutCount.ToString("0"));
						}
						break;
					}
					case "lgt-type":
					{
						MALayoutGeometryTool mALayoutGeometryTool2 = GetTarget<MALayoutGeometryTool>(0);
						bool flag26 = !flag && mALayoutGeometryTool2 != null;
						SetFieldActive(component, flag26);
						if (flag26)
						{
							DRLStepperView component10 = Hierarchy.GetComponent<DRLStepperView>(component.gameObject);
							component10.min = 0;
							component10.max = 3;
							component10.index = (int)mALayoutGeometryTool2.layoutType;
							component10.Refresh();
						}
						break;
					}
					case "lgt-visibility":
					case "lgt-fill":
					{
						MALayoutGeometryTool mALayoutGeometryTool = GetTarget<MALayoutGeometryTool>(0);
						bool flag21 = !flag && mALayoutGeometryTool != null;
						SetFieldActive(component, flag21);
						if (flag21)
						{
							DRLToggleView component6 = Hierarchy.GetComponent<DRLToggleView>(component.gameObject);
							bool isOn = false;
							LayoutParams layoutParams = mALayoutGeometryTool.layoutParams;
							switch (text)
							{
							case "lgt-visibility":
								isOn = mALayoutGeometryTool.previewVisible;
								break;
							case "lgt-fill":
								isOn = layoutParams.fill;
								break;
							}
							component6.toggle.isOn = isOn;
							component6.SetState(component6.toggle.isOn);
						}
						break;
					}
					case "lgt-asset-size":
					case "lgt-asset-margin":
					case "lgt-asset-density":
					case "lgt-shape-radius":
					case "lgt-shape-height":
					case "lgt-shape-aperture":
					{
						MALayoutGeometryTool mALayoutGeometryTool6 = GetTarget<MALayoutGeometryTool>(0);
						bool flag51 = !flag && mALayoutGeometryTool6 != null;
						if (flag51)
						{
							LayoutGeometryType layoutType3 = mALayoutGeometryTool6.layoutType;
							switch (text)
							{
							case "lgt-shape-radius":
								if (flag51)
								{
									flag51 = layoutType3 == LayoutGeometryType.Sphere || layoutType3 == LayoutGeometryType.Cone || layoutType3 == LayoutGeometryType.Cylinder;
								}
								break;
							case "lgt-shape-height":
								if (flag51)
								{
									flag51 = layoutType3 == LayoutGeometryType.Cone || layoutType3 == LayoutGeometryType.Cylinder;
								}
								break;
							case "lgt-shape-aperture":
								if (flag51)
								{
									flag51 = layoutType3 == LayoutGeometryType.Cone;
								}
								break;
							}
						}
						SetFieldActive(component, flag51);
						if (flag51)
						{
							DRLNumberFieldView component38 = Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject);
							float value2 = 0f;
							switch (text)
							{
							case "lgt-asset-size":
								value2 = mALayoutGeometryTool6.assetRadius;
								break;
							case "lgt-asset-margin":
								value2 = mALayoutGeometryTool6.assetMargin;
								break;
							case "lgt-asset-density":
								value2 = mALayoutGeometryTool6.assetDensity * 100f;
								break;
							case "lgt-shape-radius":
								value2 = mALayoutGeometryTool6.layoutRadius;
								break;
							case "lgt-shape-height":
								value2 = mALayoutGeometryTool6.layoutHeight;
								break;
							case "lgt-shape-aperture":
								value2 = mALayoutGeometryTool6.layoutAperture * 100f;
								break;
							}
							component38.value = value2;
							component38.Refresh();
						}
						break;
					}
					case "lgt-slices-size":
					case "lgt-slices-offset":
					case "lgt-random":
					case "lgt-grid-size":
					{
						MALayoutGeometryTool mALayoutGeometryTool5 = GetTarget<MALayoutGeometryTool>(0);
						bool flag45 = !flag && mALayoutGeometryTool5 != null;
						if (flag45)
						{
							LayoutGeometryType layoutType2 = mALayoutGeometryTool5.layoutType;
							if (text != null && text == "lgt-grid-size" && flag45)
							{
								flag45 = layoutType2 == LayoutGeometryType.Grid;
							}
						}
						SetFieldActive(component, flag45);
						if (flag45)
						{
							LayoutParams layoutParams2 = mALayoutGeometryTool5.layoutParams;
							DRLVectorFieldView component28 = Hierarchy.GetComponent<DRLVectorFieldView>(component.gameObject);
							Vector3 v = Vector3.zero;
							switch (text)
							{
							case "lgt-grid-size":
								v = mALayoutGeometryTool5.layoutGridSize;
								break;
							case "lgt-random":
								v = layoutParams2.random;
								break;
							case "lgt-slices-offset":
								v = new Vector3(layoutParams2.slices.x * 100f, layoutParams2.slices.y * 100f, layoutParams2.slices.z * 100f);
								break;
							case "lgt-slices-size":
								v = new Vector3(layoutParams2.slices.rangeX * 100f, layoutParams2.slices.rangeY * 100f, layoutParams2.slices.rangeZ * 100f);
								break;
							}
							component28.Set(v);
						}
						break;
					}
					case "material-color-intensity":
					{
						DRLNumberFieldView component26 = Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject);
						MARenderer mARenderer2 = GetTarget<MARenderer>(0);
						if (!mARenderer2 || mARenderer2.palleteEmission.Length == 0)
						{
							SetFieldActive(component, p_flag: false);
							break;
						}
						SetFieldActive(component, p_flag: true);
						component26.enabled = true;
						component26.value = mARenderer2.colorIntensity;
						component26.Refresh();
						break;
					}
					case "material-color-reset":
					case "material-color-0":
					case "material-color-1":
					case "material-color-2":
					case "material-color-emission":
					{
						DRLColorPickerView component13 = Hierarchy.GetComponent<DRLColorPickerView>(component.gameObject);
						MARenderer mARenderer = GetTarget<MARenderer>(0);
						if (mARenderer is MASplineControlPoint)
						{
							MASplineControlPoint mASplineControlPoint5 = mARenderer as MASplineControlPoint;
							mARenderer = mASplineControlPoint5.spline;
							if ((bool)mASplineControlPoint5.spline && mASplineControlPoint5.spline.splineCategory != SplineCategory.Visual)
							{
								mARenderer = null;
							}
						}
						if (!mARenderer)
						{
							SetFieldActive(component, p_flag: false);
							break;
						}
						bool flag30 = true;
						Vector4 vector = Color.clear;
						Color[] array = null;
						switch (text)
						{
						case "material-color-0":
							vector = mARenderer.color0;
							array = mARenderer.pallete0;
							flag30 = array.Length != 0;
							break;
						case "material-color-1":
							vector = mARenderer.color1;
							array = mARenderer.pallete1;
							flag30 = array.Length != 0;
							break;
						case "material-color-2":
							vector = mARenderer.color2;
							array = mARenderer.pallete2;
							flag30 = array.Length != 0;
							break;
						case "material-color-emission":
						{
							vector = mARenderer.emissionColor;
							float w = vector.w;
							float colorIntensity = mARenderer.colorIntensity;
							vector /= colorIntensity;
							vector.w = w;
							array = mARenderer.palleteEmission;
							flag30 = array.Length != 0;
							break;
						}
						}
						flag30 = flag30 && mARenderer.hasPalletes;
						SetFieldActive(component, flag30);
						if (array != null)
						{
							List<Color> list5 = new List<Color>();
							list5.AddRange(array);
							component13.colors = list5;
							if (flag)
							{
								component13.Invalidate();
							}
							else
							{
								component13.SetCurrent(vector);
							}
						}
						Text text5 = Hierarchy.Find<Text>(component.transform, "content.label");
						string text6 = "";
						string text7 = "";
						string p_key = "me-inspector-" + text;
						switch (text)
						{
						case "material-color-emission":
							text6 = Localization.instance.Get<string>(p_key, "Emission");
							text7 = mARenderer.GetMaterialLabel("emission");
							break;
						case "material-color-0":
							text6 = Localization.instance.Get<string>(p_key, "Color 1");
							text7 = mARenderer.GetMaterialLabel("color0");
							break;
						case "material-color-1":
							text6 = Localization.instance.Get<string>(p_key, "Color 2");
							text7 = mARenderer.GetMaterialLabel("color1");
							break;
						case "material-color-2":
							text6 = Localization.instance.Get<string>(p_key, "Color 3");
							text7 = mARenderer.GetMaterialLabel("color2");
							break;
						}
						if (string.IsNullOrEmpty(text7))
						{
							text7 = text6;
						}
						if ((bool)text5)
						{
							text5.text = text7.ToUpper();
						}
						break;
					}
					case "material-style-reset":
					case "material-style-0":
					case "material-style-1":
					case "material-style-2":
					{
						DRLStepperView component2 = Hierarchy.GetComponent<DRLStepperView>(component.gameObject);
						MARenderer spline = GetTarget<MARenderer>(0);
						if (spline is MASplineControlPoint)
						{
							spline = (spline as MASplineControlPoint).spline;
						}
						if (!spline)
						{
							SetFieldActive(component, p_flag: false);
							break;
						}
						bool flag17 = true;
						switch (text)
						{
						case "material-style-0":
							flag17 = spline.styleList0 != null;
							break;
						case "material-style-1":
							flag17 = spline.styleList1 != null;
							break;
						case "material-style-2":
							flag17 = spline.styleList2 != null;
							break;
						}
						flag17 = flag17 && spline.hasStyles;
						SetFieldActive(component, flag17);
						int index = 0;
						MARendererMaterial mARendererMaterial = null;
						switch (text)
						{
						case "material-style-0":
							mARendererMaterial = spline.styleList0;
							index = spline.style0;
							break;
						case "material-style-1":
							mARendererMaterial = spline.styleList1;
							index = spline.style1;
							break;
						case "material-style-2":
							mARendererMaterial = spline.styleList2;
							index = spline.style2;
							break;
						}
						if ((bool)mARendererMaterial)
						{
							component2.labels = mARendererMaterial.GetStyleLabels(p_uppercase: true);
							component2.min = 0;
							component2.max = mARendererMaterial.styles.Length - 1;
							component2.index = index;
							Text text2 = Hierarchy.Find<Text>(component.transform, "content.field");
							string text3 = "";
							string text4 = "";
							switch (text)
							{
							case "material-style-0":
								text3 = Localization.instance.Get<string>("me-inspector-" + text, "Style 1");
								text4 = mARendererMaterial.label;
								break;
							case "material-style-1":
								text3 = Localization.instance.Get<string>("me-inspector-" + text, "Style 2");
								text4 = mARendererMaterial.label;
								break;
							case "material-style-2":
								text3 = Localization.instance.Get<string>("me-inspector-" + text, "Style 3");
								text4 = mARendererMaterial.label;
								break;
							}
							if (string.IsNullOrEmpty(text4))
							{
								text4 = text3;
							}
							if ((bool)text2)
							{
								text2.text = text4.ToUpper();
							}
							component2.Refresh();
						}
						break;
					}
					}
				}
				if (flag)
				{
					switch (text)
					{
					case "layout-orient":
					case "layout-distribute":
					case "layout-aligment":
						if (flag13)
						{
							bool p_flag2 = flag7;
							if (text == "layout-orient" && tags.Contains(MapAssetType.NoTranformRotate))
							{
								p_flag2 = false;
							}
							SetFieldActive(component, p_flag2);
						}
						break;
					case "layout-distribute-spacing":
						if (flag13)
						{
							DRLNumberFieldView component41 = Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject);
							SetFieldActive(component, flag7);
							component41.value = layout.distributeSpacing.x;
							if (Mathf.Abs(component41.value) <= 0.0001f)
							{
								component41.input.text = "";
							}
						}
						break;
					case "layout-orient-offset":
						if (flag13)
						{
							DRLNumberFieldView component40 = Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject);
							SetFieldActive(component, flag8);
							component40.value = layout.orientOffset.y;
						}
						break;
					}
				}
				if (!flag3)
				{
					continue;
				}
				switch (text)
				{
				case "map-attribs-0":
				case "map-attribs-1":
				case "map-attribs-2":
				case "map-attribs-3":
				case "map-attribs-4":
					SetFieldActive(component, p_flag: true);
					break;
				case "map-title":
				{
					DRLInputFieldView component49 = Hierarchy.GetComponent<DRLInputFieldView>(component.gameObject);
					SetFieldActive(component49, p_flag: true);
					component49.field.text = data.mapTitle;
					break;
				}
				case "map-track-id":
				{
					if (!isDeveloper)
					{
						break;
					}
					DRLStepperView component55 = Hierarchy.GetComponent<DRLStepperView>(component.gameObject);
					Transform tracks = level.track.tracks;
					List<string> list10 = new List<string>();
					for (int num11 = 0; num11 < tracks.childCount; num11++)
					{
						string text10 = tracks.GetChild(num11).name;
						if (text10 == "freefly")
						{
							text10 = "freestyle";
						}
						list10.Add(text10);
					}
					component55.min = 0;
					component55.max = list10.Count - 1;
					component55.index = list10.IndexOf(data.trackId);
					component55.labels = list10.ToArray();
					for (int num12 = 0; num12 < component55.labels.Length; num12++)
					{
						component55.labels[num12] = component55.labels[num12].ToUpper();
					}
					if (component55.index >= 0)
					{
						component55.Refresh();
					}
					SetFieldActive(component55, isDeveloper && component55.index >= 0);
					break;
				}
				case "map-laps":
				{
					DRLStepperView component54 = Hierarchy.GetComponent<DRLStepperView>(component.gameObject);
					bool flag54 = data.mode.typeFlag != GameFlag.Collectable;
					SetFieldActive(component54, flag54);
					if (flag54)
					{
						component54.index = data.mode.race.lapCount;
						component54.Refresh();
					}
					break;
				}
				case "map-category":
					if (isDeveloper)
					{
						DRLStepperView component45 = Hierarchy.GetComponent<DRLStepperView>(component.gameObject);
						SetFieldActive(component45, isDeveloper);
						int value3 = (int)(data.mapCategoryFlag - 300);
						component45.index = Mathf.Clamp(value3, component45.min, component45.max);
						component45.Refresh();
					}
					break;
				case "map-difficulty":
				{
					DRLStepperView component51 = Hierarchy.GetComponent<DRLStepperView>(component.gameObject);
					SetFieldActive(component51, p_flag: true);
					component51.index = data.mapDifficulty;
					component51.Refresh();
					break;
				}
				case "map-visibility":
				{
					DRLToggleView component52 = Hierarchy.GetComponent<DRLToggleView>(component.gameObject);
					SetFieldActive(component52, p_flag: true);
					component52.toggle.isOn = data.isPublic;
					component52.SetState(component52.toggle.isOn);
					component52.GetComponent<SwitcherComponent>().index = (component52.toggle.isOn ? 1 : 0);
					break;
				}
				case "map-allow-copy":
				{
					DRLToggleView component56 = Hierarchy.GetComponent<DRLToggleView>(component.gameObject);
					SetFieldActive(component56, p_flag: true);
					component56.toggle.isOn = data.allowCopy;
					component56.SetState(component56.toggle.isOn);
					bool isPublic = data.isPublic;
					FadeComponent component57 = Hierarchy.GetComponent<FadeComponent>(component.gameObject);
					component57.Fade(isPublic ? 1f : 0.2f);
					component57.allowMouseInput = isPublic;
					break;
				}
				case "map-base-assets":
				{
					DRLToggleView component44 = Hierarchy.GetComponent<DRLToggleView>(component.gameObject);
					SetFieldActive(component44, base.editor.controller.game.model.level.HasBaseAssets());
					component44.toggle.isOn = data.baseAssetsEnabled;
					component44.SetState(component44.toggle.isOn);
					break;
				}
				case "map-lighting":
				{
					DRLStepperView component53 = Hierarchy.GetComponent<DRLStepperView>(component.gameObject);
					bool flag53 = level.HasLightingPresets();
					SetFieldActive(component53, flag53);
					if (flag53)
					{
						component53.min = 0;
						component53.max = level.settings.light.presets.Count - 1;
						component53.labels = level.settings.light.presetLabels.ToArray();
						for (int num10 = 0; num10 < component53.labels.Length; num10++)
						{
							component53.labels[num10] = component53.labels[num10].ToUpper();
						}
						component53.index = data.mapLighting;
						component53.Refresh();
					}
					break;
				}
				case "map-style-0":
				case "map-style-1":
				case "map-style-2":
				{
					DRLIntStepperView component47 = Hierarchy.GetComponent<DRLIntStepperView>(component.gameObject);
					bool flag52 = false;
					int num8 = 0;
					int value4 = 0;
					string text8 = "STYLE";
					List<LevelSettings.Scene.Style> list7 = ((settings.scene.styles == null) ? new List<LevelSettings.Scene.Style>() : settings.scene.styles);
					List<int> list8 = null;
					switch (text)
					{
					case "map-style-0":
						list8 = ((list7.Count >= 1) ? list7[0].GetStyleIndexes() : null);
						break;
					case "map-style-1":
						list8 = ((list7.Count >= 2) ? list7[1].GetStyleIndexes() : null);
						break;
					case "map-style-2":
						list8 = ((list7.Count >= 3) ? list7[2].GetStyleIndexes() : null);
						break;
					}
					if (list8 == null)
					{
						list8 = new List<int>();
					}
					num8 = list8.Count;
					flag52 = num8 > 1;
					switch (text)
					{
					case "map-style-0":
						text8 = ((num8 <= 0) ? "STYLE A" : list7[0].label);
						value4 = data.mapStyle0;
						break;
					case "map-style-1":
						text8 = ((num8 <= 0) ? "STYLE B" : list7[1].label);
						value4 = data.mapStyle1;
						break;
					case "map-style-2":
						text8 = ((num8 <= 0) ? "STYLE C" : list7[2].label);
						value4 = data.mapStyle2;
						break;
					}
					SetFieldActive(component47, flag52);
					if (flag52)
					{
						Text text9 = Hierarchy.Find<Text>(component47.transform, "content.field");
						if ((bool)text9)
						{
							text9.text = text8.ToUpper();
						}
						component47.min = 0;
						component47.max = num8 - 1;
						component47.minValue = 0;
						component47.maxValue = 9999;
						component47.values = list8;
						List<string> list9 = new List<string>();
						for (int num9 = 0; num9 < num8; num9++)
						{
							list9.Add((list8[num9] + 1).ToString("00"));
						}
						component47.labels = list9.ToArray();
						component47.SetValue(value4);
					}
					break;
				}
				case "map-asset-layer-2":
				case "map-asset-layer-1":
				case "map-asset-layer-0":
				{
					DRLStepperView component58 = Hierarchy.GetComponent<DRLStepperView>(component.gameObject);
					int num13 = -1;
					int index2 = -1;
					switch (text)
					{
					case "map-asset-layer-0":
						num13 = 0;
						index2 = data.mapAssetLayer0;
						break;
					case "map-asset-layer-1":
						num13 = 1;
						index2 = data.mapAssetLayer1;
						break;
					case "map-asset-layer-2":
						num13 = 2;
						index2 = data.mapAssetLayer2;
						break;
					}
					int assetLayerCount = level.GetAssetLayerCount();
					LevelSettings.Scene.AssetLayer assetLayer = level.GetAssetLayer(num13);
					int num14 = assetLayer?.Count ?? 0;
					string text11 = ((assetLayer == null) ? ("Layer " + (num13 + 1)) : assetLayer.label);
					bool p_flag5 = num13 < assetLayerCount && num14 >= 2;
					SetFieldActive(component58, p_flag5);
					Text text12 = Hierarchy.Find<Text>(component58.transform, "content.field");
					if ((bool)text12)
					{
						text12.text = text11.ToUpper();
					}
					List<string> list11 = new List<string>();
					for (int num15 = 0; num15 < num14; num15++)
					{
						list11.Add((num15 + 1).ToString("00"));
					}
					component58.labels = list11.ToArray();
					component58.min = 0;
					component58.max = num14 - 1;
					component58.index = index2;
					component58.Refresh();
					break;
				}
				case "map-thumb":
				{
					MEMapThumbInspector component50 = Hierarchy.GetComponent<MEMapThumbInspector>(component.gameObject);
					SetFieldActive(component50, p_flag: true);
					component50.SetDefaultImage(base.editor.model.map.background);
					string path = DRLPaths.Storage.offlineMapEditorMapsRoot + base.editor.model.data.guid + ".jpg";
					byte[] array2 = (File.Exists(path) ? File.ReadAllBytes(path) : null);
					if (array2 == null)
					{
						component50.LoadImage(data.mapThumbURL, p_fade: true);
						break;
					}
					Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
					texture2D.LoadImage(array2, markNonReadable: false);
					texture2D.hideFlags = HideFlags.HideAndDontSave;
					if ((bool)m_texture_data)
					{
						UnityEngine.Object.Destroy(m_texture_data);
					}
					m_texture_data = texture2D;
					component50.photoFade.alpha = -0.1f;
					component50.SetImage(texture2D);
					break;
				}
				case "physics-drop-timing":
				case "physics-drop-velocity":
				case "physics-label":
					SetFieldActive(component, p_flag: true);
					break;
				case "physics-drop-v-up":
					SetFieldActive(component, p_flag: true);
					Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject).value = state.physics.velocity.y;
					break;
				case "physics-drop-v-forward":
					SetFieldActive(component, p_flag: true);
					Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject).value = state.physics.velocity.z;
					break;
				case "physics-drop-spin":
					SetFieldActive(component, p_flag: true);
					Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject).value = Mathf.Clamp01(state.physics.angularVelocity.magnitude / 50f) * 100f;
					break;
				case "physics-drop-delay":
					SetFieldActive(component, p_flag: true);
					Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject).value = state.physics.delay;
					break;
				case "physics-drop-duration":
				{
					SetFieldActive(component, p_flag: true);
					DRLNumberFieldView component48 = Hierarchy.GetComponent<DRLNumberFieldView>(component.gameObject);
					component48.value = state.physics.duration;
					if (Mathf.Abs(component48.value) <= 0.001f)
					{
						component48.input.text = "";
					}
					break;
				}
				case "map-collabs":
					if (isDeveloper)
					{
						DRLInputFieldView component46 = Hierarchy.GetComponent<DRLInputFieldView>(component.gameObject);
						SetFieldActive(component46, isDeveloper);
					}
					break;
				case "map-collab-list":
				{
					if (!isDeveloper)
					{
						break;
					}
					ListComponent component43 = Hierarchy.GetComponent<ListComponent>(component.gameObject);
					SetFieldActive(component43, isDeveloper);
					component43.Clear();
					int collaboratorCount = data.GetCollaboratorCount();
					for (int num7 = 0; num7 < collaboratorCount; num7++)
					{
						DRLPlayerProfileData collaborator = data.GetCollaborator(num7);
						if (collaborator != null)
						{
							DRLMapEditorCollabItem dRLMapEditorCollabItem = component43.Push<DRLMapEditorCollabItem>();
							dRLMapEditorCollabItem.name = "map-collab-list";
							dRLMapEditorCollabItem.Set(collaborator);
						}
					}
					break;
				}
				case "prefs-replay-cache":
				{
					bool p_flag4 = base.editor.model.cachedReplaysCount > 0;
					SetFieldActive(component, p_flag4);
					break;
				}
				case "replay-cache-delete":
				{
					bool p_flag3 = base.editor.model.cachedReplaysCount > 0;
					SetFieldActive(component, p_flag3);
					break;
				}
				case "prefs-map-save":
					SetFieldActive(component, p_flag: true);
					break;
				case "map-save":
					SetFieldActive(component, p_flag: true);
					break;
				case "map-auto-save":
				{
					SetFieldActive(component, p_flag: true);
					DRLToggleView component42 = Hierarchy.GetComponent<DRLToggleView>(component.gameObject);
					component42.toggle.isOn = data.prefs.autoSave;
					component42.SetState(component42.toggle.isOn);
					break;
				}
				}
			}
			m_ignore_change = false;
			panel.SetFieldsNotificationEnabled(p_flag: true);
		}

		protected void SetFieldActive(Component p_field, bool p_flag)
		{
			if (p_field.gameObject.activeInHierarchy != p_flag)
			{
				p_field.gameObject.SetActive(p_flag);
			}
		}

		protected void RefreshFields(IList<RectTransform> p_fields)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < p_fields.Count; i++)
			{
				list.Add(p_fields[i].name);
			}
			RefreshFields(list);
		}

		public void RefreshTransformFields()
		{
			RefreshFields(MEInspectorFieldIds.TransformFields);
		}

		protected void RefreshTransformAnchor(bool p_instantiate)
		{
			if (p_instantiate)
			{
				if ((bool)anchor.target)
				{
					UnityEngine.Object.Destroy(anchor.target.gameObject);
				}
				anchor.target = null;
			}
			anchor.target = TRSHandle.GetAnchor(base.targets, "me-inspector-anchor", anchor.target);
			if ((bool)anchor.target)
			{
				anchor.position = anchor.target.position;
				anchor.rotation = anchor.target.localEulerAngles;
				anchor.scale = anchor.target.localScale;
			}
		}

		public static Vector3 Abs(Vector3 v)
		{
			return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
		}

		private Vector3 ScaleRangeCheck(Vector3 v)
		{
			float num = 0.001f;
			Vector3 v2 = v;
			float num2 = ((v2.x < 0f) ? (-1f) : 1f);
			if (Mathf.Abs(v2.x) < num)
			{
				v2.x = num * num2;
			}
			num2 = ((v2.y < 0f) ? (-1f) : 1f);
			if (Mathf.Abs(v2.y) < num)
			{
				v2.y = num * num2;
			}
			num2 = ((v2.z < 0f) ? (-1f) : 1f);
			if (Mathf.Abs(v2.z) < num)
			{
				v2.z = num * num2;
			}
			return Abs(v2);
		}

		public override void OnNotification(string p_notification, UnityEngine.Object p_target, params object[] p_data)
		{
			string item = p_target.name;
			if ((panel.fieldIds.Contains(item) || p_notification.Contains("map-editor.inspector.form.event")) && !p_notification.Contains("map-editor.graph.layout.widget.event"))
			{
				OnFieldsFormNotification(p_notification, p_target, p_data);
				return;
			}
			switch (p_notification)
			{
			case "map-editor.action.undo":
			case "map-editor.action.redo":
				RefreshFields(panel.fieldIds, 1f / 30f);
				if ((p_data[0] as MEActionData).type == MEActionType.ChangeTransform)
				{
					layout.Set(base.targets);
				}
				break;
			case "map-editor.control.end-change":
			case "map-editor.handle@update":
				RefreshTransformAnchor(p_instantiate: false);
				RefreshTransformFields();
				break;
			case "map-editor.handle@drag-end":
				layout.Set(base.targets);
				break;
			}
		}

		protected virtual void OnFieldsFormNotification(string p_notification, UnityEngine.Object p_target, params object[] p_data)
		{
			string text = p_target.name;
			bool flag = p_notification.Contains("@change");
			bool flag2 = p_notification.Contains("@end-edit");
			bool flag3 = p_notification.Contains("@click");
			if (p_notification.Contains("@disabled"))
			{
				return;
			}
			bool flag4 = false;
			if (flag && m_ignore_change)
			{
				return;
			}
			MEStateModel state = base.editor.model.state;
			LevelModel level = base.editor.controller.game.model.level;
			switch (text)
			{
			case "transform-position":
				if (!flag3)
				{
					BeginChange(text);
					Vector3 vector = (p_target as DRLVectorFieldView).Get<Vector3>();
					Vector3 vector3 = vector - anchor.position;
					for (int m = 0; m < base.targets.Count; m++)
					{
						base.targets[m].transform.position += vector3;
					}
					RefreshTransformAnchor(p_instantiate: false);
					EndChange(text);
				}
				break;
			case "transform-rotation":
				if (!flag3)
				{
					BeginChange(text);
					Vector3 vector = (p_target as DRLVectorFieldView).Get<Vector3>();
					Vector3 vector5 = vector - anchor.rotation;
					for (int num29 = 0; num29 < base.targets.Count; num29++)
					{
						Transform obj8 = base.targets[num29].transform;
						Transform parent2 = obj8.parent;
						int siblingIndex2 = obj8.GetSiblingIndex();
						obj8.SetParent(anchor.target, worldPositionStays: true);
						anchor.target.localEulerAngles += vector5;
						obj8.SetParent(parent2, worldPositionStays: true);
						obj8.SetSiblingIndex(siblingIndex2);
						anchor.target.localEulerAngles = anchor.rotation;
					}
					anchor.rotation += vector5;
					EndChange(text);
				}
				break;
			case "transform-scale":
				if (!flag3)
				{
					Vector3 vector = (p_target as DRLVectorFieldView).Get<Vector3>();
					Vector3 vector2 = vector - anchor.scale;
					BeginChange(text);
					for (int j = 0; j < base.targets.Count; j++)
					{
						Transform transform = base.targets[j].transform;
						Transform parent = transform.parent;
						_ = transform.position;
						int siblingIndex = transform.GetSiblingIndex();
						anchor.target.localScale = anchor.scale;
						transform.SetParent(anchor.target, worldPositionStays: true);
						anchor.target.localScale = ScaleRangeCheck(anchor.target.localScale + vector2);
						transform.SetParent(parent, worldPositionStays: true);
						transform.SetSiblingIndex(siblingIndex);
						transform.localScale = ScaleRangeCheck(transform.localScale);
					}
					anchor.scale = ScaleRangeCheck(anchor.scale + vector2);
					EndChange(text);
				}
				break;
			case "transform-modular-scale":
				if (!flag3)
				{
					MAModularScaleRenderer mAModularScaleRenderer = GetTarget<MAModularScaleRenderer>(0);
					if ((bool)mAModularScaleRenderer)
					{
						DRLVectorFieldView obj4 = p_target as DRLVectorFieldView;
						Vector3 vector = obj4.Get<Vector3>();
						BeginChange(text);
						vector = (mAModularScaleRenderer.moduleScale = mAModularScaleRenderer.modules.GetVectorSizzle(vector));
						EndChange(text);
						m_ignore_change = true;
						obj4.Set(vector);
						m_ignore_change = false;
						flag4 = true;
					}
				}
				break;
			case "transform-snap-ground":
			{
				if (!flag3)
				{
					break;
				}
				BeginChange(text);
				for (int n = 0; n < base.targets.Count; n++)
				{
					Transform transform2 = base.targets[n].transform;
					RaycastHit raycastHit = base.editor.scene.GroundRaycast(transform2);
					if (raycastHit.distance < 0f)
					{
						continue;
					}
					transform2.position = raycastHit.point;
					transform2.localRotation = Quaternion.LookRotation(transform2.forward, Vector3.up);
					if (base.targets[n].data.type == MapAssetType.SplineControlPoint)
					{
						MASplineControlPoint mASplineControlPoint = base.targets[n] as MASplineControlPoint;
						if ((bool)mASplineControlPoint.spline)
						{
							mASplineControlPoint.spline.RefreshSpline(1f / 60f);
						}
					}
				}
				RefreshTransformAnchor(p_instantiate: false);
				RefreshTransformFields();
				EndChange(text);
				break;
			}
			case "physics-simulation-toggle":
			{
				if (!flag3)
				{
					break;
				}
				SwitcherComponent component = (p_target as UIElementView).GetComponent<SwitcherComponent>();
				BeginChange(text);
				List<MEPreviewPhysics> list4 = FindAll<MEPreviewPhysics>();
				bool num4 = list4.Count > 0;
				bool flag7 = component.index == 1;
				if (num4)
				{
					for (int num5 = 0; num5 < list4.Count; num5++)
					{
						list4[num5].Clear();
					}
				}
				else
				{
					List<MapAsset> list5 = base.targets;
					if (!flag7)
					{
						for (int num6 = 0; num6 < list5.Count; num6++)
						{
							MapAsset mapAsset = list5[num6];
							if ((bool)mapAsset && !mapAsset.GetComponent<MEPreviewPhysics>())
							{
								MEPreviewPhysics mEPreviewPhysics = mapAsset.gameObject.AddComponent<MEPreviewPhysics>();
								mEPreviewPhysics.Set(0f, 0f, Vector3.zero, Vector3.zero);
								mEPreviewPhysics.Run();
							}
						}
					}
				}
				Activity.RunOnce(delegate
				{
					RefreshFields(new string[1] { "physics-simulation-toggle" });
				}, 1f / 15f);
				EndChange(text);
				break;
			}
			case "layout-align-nx":
			case "layout-align-ny":
			case "layout-align-nz":
			case "layout-align-px":
			case "layout-align-py":
			case "layout-align-pz":
			case "layout-align-xc":
			case "layout-align-yc":
			case "layout-align-zc":
				if (flag3)
				{
					BeginChange(text);
					string text5 = text.Replace("layout-align-", "").ToLower();
					bool p_world_space = true;
					Vector3 p_mask = Vector3.one * -1f;
					float num23 = -1f;
					if (text5.Contains("n"))
					{
						num23 = 0f;
					}
					if (text5.Contains("c"))
					{
						num23 = 0.5f;
					}
					if (text5.Contains("p"))
					{
						num23 = 1f;
					}
					if (text5.Contains("x"))
					{
						p_mask.x = num23;
					}
					if (text5.Contains("y"))
					{
						p_mask.y = num23;
					}
					if (text5.Contains("z"))
					{
						p_mask.z = num23;
					}
					layout.Align(p_world_space, p_mask);
					RefreshTransformAnchor(p_instantiate: false);
					RefreshTransformFields();
					EndChange(text);
				}
				break;
			case "layout-distribute-apply-x":
			case "layout-distribute-apply-y":
			case "layout-distribute-apply-z":
			case "layout-distribute-apply-d":
				if (flag3)
				{
					BeginChange(text);
					string text9 = text.Replace("layout-distribute-apply-", "").ToLower();
					bool p_world_space2 = ((!(text9 == "d")) ? true : false);
					Vector3 p_direction = Vector3.zero;
					Vector3 zero = Vector3.zero;
					float x = layout.distributeSpacing.x;
					if (text9.Contains("x"))
					{
						zero.Set(1f, 0f, 0f);
						p_direction = Vector3.right;
					}
					if (text9.Contains("y"))
					{
						zero.Set(0f, 1f, 0f);
						p_direction = Vector3.up;
					}
					if (text9.Contains("z"))
					{
						zero.Set(0f, 0f, 1f);
						p_direction = Vector3.forward;
					}
					if (text9.Contains("d"))
					{
						zero.Set(1f, 1f, 1f);
						p_direction = Vector3.forward;
					}
					layout.Distribute(p_world_space2, x, zero, p_direction);
					RefreshTransformAnchor(p_instantiate: false);
					RefreshTransformFields();
					EndChange(text);
				}
				break;
			case "layout-distribute-spacing":
				if (flag)
				{
					DRLNumberFieldView dRLNumberFieldView3 = p_target as DRLNumberFieldView;
					Vector3 distributeSpacing = layout.distributeSpacing;
					distributeSpacing.x = dRLNumberFieldView3.value;
					layout.distributeSpacing = distributeSpacing;
					if (Mathf.Abs(distributeSpacing.x) <= 0.001f)
					{
						dRLNumberFieldView3.input.text = "";
					}
				}
				break;
			case "layout-orient-offset":
				if (flag)
				{
					DRLNumberFieldView dRLNumberFieldView4 = p_target as DRLNumberFieldView;
					Vector3 orientOffset = layout.orientOffset;
					orientOffset.y = dRLNumberFieldView4.value;
					layout.orientOffset = orientOffset;
				}
				break;
			case "layout-orient-flat-apply":
			case "layout-orient-smooth-apply":
				if (flag3)
				{
					BeginChange(text);
					bool flag6 = false;
					if (IsMultiTargetSameGUID() && base.targets[0] is MASplineControlPoint)
					{
						flag6 = true;
					}
					if (!flag6)
					{
						bool p_smooth = text.Contains("smooth");
						layout.Orient(p_smooth, layout.orientOffset, Vector3.zero);
					}
					else
					{
						(base.targets[0] as MASplineControlPoint).spline.OrientControlPoints();
					}
					EndChange(text);
				}
				break;
			case "attrib-ruler":
				if (flag)
				{
					DRLToggleView dRLToggleView8 = p_target as DRLToggleView;
					MAEntity mAEntity = GetTarget<MAEntity>(0);
					BeginChange(text);
					mAEntity.attribs = (dRLToggleView8.toggle.isOn ? (mAEntity.attribs | MDEntityAttribFlag.Ruler) : (mAEntity.attribs & (MDEntityAttribFlag)(-2)));
					EndChange(text);
				}
				break;
			case "attrib-layout":
			{
				if (!flag)
				{
					break;
				}
				DRLToggleView dRLToggleView7 = p_target as DRLToggleView;
				MARenderer spline9 = GetTarget<MARenderer>(0);
				if (!spline9)
				{
					break;
				}
				if (spline9.data.type == MapAssetType.SplineControlPoint)
				{
					spline9 = (spline9 as MASplineControlPoint).spline;
				}
				if (!spline9)
				{
					break;
				}
				BeginChange(text);
				spline9.isLayout = dRLToggleView7.toggle.isOn;
				if (spline9.isLayout)
				{
					List<MARenderer> list9 = base.editor.scene.FindAll<MARenderer>();
					for (int num22 = 0; num22 < list9.Count; num22++)
					{
						if (list9[num22] != spline9 && list9[num22].isLayout)
						{
							list9[num22].isLayout = false;
						}
					}
				}
				EndChange(text);
				break;
			}
			case "gate-enabled":
			{
				if (!flag)
				{
					break;
				}
				DRLToggleView dRLToggleView3 = p_target as DRLToggleView;
				BeginChange(text);
				for (int num20 = 0; num20 < base.targets.Count; num20++)
				{
					MAGate mAGate4 = GetTarget<MAGate>(num20);
					if ((bool)mAGate4)
					{
						mAGate4.isTrigger = dRLToggleView3.toggle.isOn;
						mAGate4.SetTriggerRendererEnabled(mAGate4.isTrigger);
					}
				}
				EndChange(text);
				break;
			}
			case "gate-mode":
			{
				if (!flag)
				{
					break;
				}
				MapGateMode index3 = (MapGateMode)(p_target as DRLStepperView).index;
				BeginChange(text);
				for (int num2 = 0; num2 < base.targets.Count; num2++)
				{
					MAGate mAGate = GetTarget<MAGate>(num2);
					if ((bool)mAGate)
					{
						mAGate.gateMode = index3;
					}
				}
				EndChange(text);
				flag4 = true;
				break;
			}
			case "gate-respawn-visible":
			{
				if (!flag)
				{
					break;
				}
				bool isOn3 = (p_target as DRLToggleView).toggle.isOn;
				MAGuide p_template = (isOn3 ? base.editor.app.model.storage.library.FindByGUID<MAGuide>("DMA-d529") : null);
				BeginChange(text);
				for (int num32 = 0; num32 < base.targets.Count; num32++)
				{
					MAGate mAGate5 = GetTarget<MAGate>(num32);
					if ((bool)mAGate5)
					{
						mAGate5.isRespawnVisible = isOn3;
						MAGuide mAGuide2 = (isOn3 ? mAGate5.AssertRespawnGuide(p_template) : mAGate5.GetRespawnGuide());
						if ((bool)mAGuide2)
						{
							mAGuide2.gameObject.SetActive(mAGate5.isRespawnVisible);
						}
					}
				}
				EndChange(text);
				break;
			}
			case "gate-lap-end":
			case "gate-lap-start":
			case "gate-finish":
			{
				if (!flag)
				{
					break;
				}
				DRLToggleView dRLToggleView2 = p_target as DRLToggleView;
				MAGate mAGate2 = GetTarget<MAGate>(0);
				if (!mAGate2)
				{
					break;
				}
				bool isOn = dRLToggleView2.toggle.isOn;
				BeginChange(text);
				switch (text)
				{
				case "gate-finish":
					mAGate2.isFinish = isOn;
					break;
				case "gate-lap-start":
					mAGate2.isLapStart = isOn;
					break;
				case "gate-lap-end":
					mAGate2.isLapEnd = isOn;
					break;
				}
				bool flag8 = false;
				switch (text)
				{
				case "gate-finish":
					if (isOn && (mAGate2.isLapStart || mAGate2.isLapEnd))
					{
						mAGate2.isLapStart = false;
						mAGate2.isLapEnd = false;
						flag8 = true;
					}
					break;
				case "gate-lap-start":
					if (isOn && (mAGate2.isFinish || mAGate2.isLapEnd))
					{
						mAGate2.isFinish = false;
						mAGate2.isLapEnd = false;
						flag8 = true;
					}
					break;
				case "gate-lap-end":
					if (isOn && (mAGate2.isFinish || mAGate2.isLapStart))
					{
						mAGate2.isFinish = false;
						mAGate2.isLapStart = false;
						flag8 = true;
					}
					break;
				}
				List<MAGate> list6 = base.editor.scene.FindAll<MAGate>();
				switch (text)
				{
				case "gate-finish":
				{
					if (!isOn)
					{
						break;
					}
					for (int num11 = 0; num11 < list6.Count; num11++)
					{
						if (list6[num11] != mAGate2 && list6[num11].isFinish)
						{
							list6[num11].isFinish = false;
						}
					}
					break;
				}
				case "gate-lap-start":
				{
					if (!isOn)
					{
						break;
					}
					for (int num10 = 0; num10 < list6.Count; num10++)
					{
						if (list6[num10] != mAGate2 && list6[num10].isLapStart)
						{
							list6[num10].isLapStart = false;
						}
					}
					break;
				}
				case "gate-lap-end":
				{
					if (!isOn)
					{
						break;
					}
					for (int num9 = 0; num9 < list6.Count; num9++)
					{
						if (list6[num9] != mAGate2 && list6[num9].isLapEnd)
						{
							list6[num9].isLapEnd = false;
						}
					}
					break;
				}
				}
				if (flag8)
				{
					RefreshFields(MEInspectorFieldIds.GateLapLogicFields, 1f / 60f);
				}
				EndChange(text);
				break;
			}
			case "spline-category":
				if (flag)
				{
					MASpline spline2 = GetTarget<MASplineControlPoint>(0).spline;
					if ((bool)spline2)
					{
						flag4 = true;
						DRLStepperView obj2 = p_target as DRLStepperView;
						BeginChange(text);
						SplineCategory index = (SplineCategory)obj2.index;
						spline2.splineCategory = index;
						RefreshFields(MEInspectorFieldIds.SplineFields);
						RefreshFields(MEInspectorFieldIds.MaterialColorFields);
						RefreshFields(MEInspectorFieldIds.MaterialStyleFields);
						EndChange(text);
					}
				}
				break;
			case "spline-smooth":
				if (flag)
				{
					MASpline spline12 = GetTarget<MASplineControlPoint>(0).spline;
					if ((bool)spline12)
					{
						DRLToggleView dRLToggleView11 = p_target as DRLToggleView;
						BeginChange(text);
						spline12.splineMode = (dRLToggleView11.toggle.isOn ? SplineType.Catmull : SplineType.Linear);
						EndChange(text);
					}
				}
				break;
			case "spline-loop":
				if (flag)
				{
					MASpline spline11 = GetTarget<MASplineControlPoint>(0).spline;
					if ((bool)spline11)
					{
						DRLToggleView dRLToggleView10 = p_target as DRLToggleView;
						BeginChange(text);
						spline11.isLoop = dRLToggleView10.toggle.isOn;
						EndChange(text);
					}
				}
				break;
			case "spline-control-point-index":
				if (flag)
				{
					BeginChange(text);
					MASplineControlPoint mASplineControlPoint10 = GetTarget<MASplineControlPoint>(0);
					MASpline spline10 = mASplineControlPoint10.spline;
					int p_index = (int)(p_target as DRLNumberFieldView).value - 1;
					spline10.SetControlPointIndex(mASplineControlPoint10, p_index);
					Debug.Log("MEPropertyInspector> OnFormEvent / scp-index[" + p_index + "]");
					flag4 = true;
					EndChange(text);
				}
				break;
			case "spline-course-camera-index":
				if (flag)
				{
					BeginChange(text);
					MASpline spline5 = GetTarget<MASplineControlPoint>(0).spline;
					int index4 = (int)(p_target as DRLNumberFieldView).value - 1;
					List<MASpline> list3 = base.editor.scene.FindAll((MASpline it) => it.splineCategory == SplineCategory.CourseCamera);
					list3.Sort((MASpline sa, MASpline sb) => (sa.splineCourseCameraIndex >= sb.splineCourseCameraIndex) ? 1 : (-1));
					list3.Remove(spline5);
					list3.Insert(index4, spline5);
					for (int num3 = 0; num3 < list3.Count; num3++)
					{
						list3[num3].splineCourseCameraIndex = num3;
					}
					Debug.Log("MEPropertyInspector> OnFormEvent / scc-index[" + index4 + "]");
					flag4 = true;
					EndChange(text);
				}
				break;
			case "spline-course-camera-fov":
			case "spline-course-camera-speed":
			case "spline-start-width":
			case "spline-end-width":
			case "spline-thickness":
			case "spline-alpha":
				if (flag)
				{
					MASpline spline4 = GetTarget<MASplineControlPoint>(0).spline;
					DRLNumberFieldView obj3 = p_target as DRLNumberFieldView;
					BeginChange(text);
					float value = obj3.value;
					switch (text)
					{
					case "spline-start-width":
						spline4.splineStartWidth = value;
						break;
					case "spline-end-width":
						spline4.splineEndWidth = value;
						break;
					case "spline-thickness":
						spline4.splineThickness = value;
						break;
					case "spline-alpha":
						spline4.splineAlpha = Mathf.Clamp01(value / 100f);
						break;
					case "spline-course-camera-speed":
						spline4.splineCourseCameraSpeed = value;
						panel.GetField<MESplineCourseCameraPreviewInspector>("spl-course-camera-preview").actor.speed = value;
						break;
					case "spline-course-camera-fov":
						spline4.splineCourseCameraFOV = value;
						panel.GetField<MESplineCourseCameraPreviewInspector>("spl-course-camera-preview").camera.camera.fieldOfView = value;
						break;
					}
					flag4 = true;
					EndChange(text);
				}
				break;
			case "spline-snap-gates":
			{
				if (!flag3)
				{
					break;
				}
				MASpline spline7 = GetTarget<MASplineControlPoint>(0).spline;
				string splineControlPointId = spline7.splineControlPointId;
				MASplineControlPoint mASplineControlPoint4 = base.editor.app.model.storage.library.FindByGUID<MASplineControlPoint>(splineControlPointId);
				if (!mASplineControlPoint4)
				{
					Debug.LogWarning("MEPropertyInspector> Failed to find template [" + splineControlPointId + "]");
					break;
				}
				if (!spline7)
				{
					Debug.LogWarning("MEPropertyInspector> Failed to find spline / SplineSnapGates");
					break;
				}
				List<Vector3> raceLine = base.editor.scene.GetRaceLine(3f, 2f);
				if (raceLine.Count <= 0)
				{
					Debug.LogWarning("MEPropertyInspector> Failed to find race line coordinates");
					break;
				}
				List<MASplineControlPoint> controlPoints = spline7.GetControlPoints();
				int count = controlPoints.Count;
				if (count < raceLine.Count)
				{
					count = raceLine.Count - count;
					for (int num13 = 0; num13 < count; num13++)
					{
						MASplineControlPoint mASplineControlPoint5 = (MASplineControlPoint)base.editor.factory.Build(mASplineControlPoint4.data, spline7.transform);
						mASplineControlPoint5.data.id = MDObject.GenerateId();
						base.editor.scene.HierarchyAdd(mASplineControlPoint5);
						controlPoints.Add(mASplineControlPoint5);
					}
				}
				Spline<Vector3> spline8 = new Spline<Vector3>(SplineType.Linear, raceLine.Count);
				spline8.values = raceLine.ToArray();
				for (int num14 = 0; num14 < controlPoints.Count; num14++)
				{
					MASplineControlPoint mASplineControlPoint6 = controlPoints[num14];
					float p_ratio = (float)num14 / (float)(controlPoints.Count - 1);
					Vector3 normalized = spline8.GetNormalized(p_ratio);
					mASplineControlPoint6.transform.position = normalized;
				}
				for (int num15 = 1; num15 < controlPoints.Count; num15++)
				{
					MASplineControlPoint mASplineControlPoint7 = controlPoints[num15 - 1];
					MASplineControlPoint mASplineControlPoint8 = controlPoints[num15];
					mASplineControlPoint7.transform.LookAt(mASplineControlPoint8.transform.position, Vector3.up);
					if (num15 >= controlPoints.Count - 1)
					{
						mASplineControlPoint8.localRotation = mASplineControlPoint7.localRotation;
					}
				}
				List<MAGate> list8 = base.editor.scene.FindGates();
				for (int num16 = 0; num16 < list8.Count - 1; num16++)
				{
					MAGate mAGate3 = list8[num16];
					float num17 = 99999f;
					MASplineControlPoint mASplineControlPoint9 = null;
					for (int num18 = 0; num18 < controlPoints.Count; num18++)
					{
						float num19 = Vector3.Distance(controlPoints[num18].transform.position, mAGate3.triggerCenter);
						if (num19 < num17)
						{
							mASplineControlPoint9 = controlPoints[num18];
							num17 = num19;
						}
					}
					if ((bool)mASplineControlPoint9)
					{
						mASplineControlPoint9.transform.position = mAGate3.triggerCenter;
					}
				}
				spline7.DelayRefresh();
				spline7.DelayedRefreshHierarchy();
				RefreshTransformAnchor(p_instantiate: false);
				RefreshTransformFields();
				flag4 = true;
				break;
			}
			case "spline-snap-closest-gate":
			{
				if (!flag3)
				{
					break;
				}
				MASplineControlPoint mASplineControlPoint12 = GetTarget<MASplineControlPoint>(0);
				MASpline spline14 = mASplineControlPoint12.spline;
				Vector3 position = mASplineControlPoint12.transform.position;
				BeginChange(text);
				List<MAGate> list10 = base.editor.scene.FindGates();
				MAGate mAGate6 = ((list10.Count <= 0) ? null : list10[0]);
				float num34 = (mAGate6 ? Vector3.Distance(mAGate6.triggerCenter, position) : 9999f);
				for (int num35 = 1; num35 < list10.Count; num35++)
				{
					MAGate mAGate7 = list10[num35];
					float num36 = Vector3.Distance(position, mAGate7.triggerCenter);
					if (num36 < num34)
					{
						num34 = num36;
						mAGate6 = mAGate7;
					}
				}
				if ((bool)mAGate6)
				{
					mASplineControlPoint12.transform.position = mAGate6.triggerCenter;
				}
				spline14.DelayRefresh();
				spline14.DelayedRefreshHierarchy();
				RefreshTransformAnchor(p_instantiate: false);
				RefreshTransformFields();
				flag4 = true;
				EndChange(text);
				break;
			}
			case "spl-course-camera-toggle":
				if (flag3)
				{
					MASplineControlPoint mASplineControlPoint11 = GetTarget<MASplineControlPoint>(0);
					MESplineCourseCameraPreviewInspector field3 = panel.GetField<MESplineCourseCameraPreviewInspector>("spl-course-camera-preview");
					MASpline p_spline = (mASplineControlPoint11 ? mASplineControlPoint11.spline : null);
					Debug.Log("MEPropertyInspector> OnFormEvent / " + text);
					field3.Toggle();
					if (field3.actor.auto)
					{
						field3.RenderLoop(p_spline, field3.actor.transform);
					}
					else
					{
						field3.RenderLoop(p_spline, mASplineControlPoint11.transform);
					}
					flag4 = false;
				}
				break;
			case "spline-snap-select-next":
				if (flag)
				{
					DRLToggleView dRLToggleView9 = p_target as DRLToggleView;
					MASpline.splineSnapSelectNext = dRLToggleView9.toggle.isOn;
					Debug.Log("MEPropertyInspector> OnFormEvent / SplineSnapSelectNext [" + dRLToggleView9.toggle.isOn + "]");
					flag4 = true;
				}
				break;
			case "transform-snap-camera":
			{
				if (!flag3)
				{
					break;
				}
				BeginChange(text);
				MAGuide mAGuide = GetTarget<MAGuide>(0);
				Camera main = base.editor.camera.main;
				mAGuide.transform.position = main.transform.position;
				mAGuide.transform.rotation = main.transform.rotation;
				if (mAGuide is MACameraToolControlPoint)
				{
					((MACameraToolControlPoint)mAGuide).tool.Refresh();
				}
				if (mAGuide is MASplineControlPoint)
				{
					MASplineControlPoint mASplineControlPoint2 = (MASplineControlPoint)mAGuide;
					mASplineControlPoint2.spline.Refresh();
					MASplineControlPoint mASplineControlPoint3 = (MASpline.splineSnapSelectNext ? mASplineControlPoint2.spline.GetNextControlPoint(mASplineControlPoint2) : null);
					if ((bool)mASplineControlPoint3)
					{
						base.editor.model.selection.ClearEntities();
						base.editor.model.selection.entity = mASplineControlPoint3;
						Activity.RunOnce(delegate
						{
							base.editor.ui.tabGroupRight.index = 1;
						}, 0.1f);
					}
				}
				RefreshTransformAnchor(p_instantiate: false);
				RefreshTransformFields();
				EndChange(text);
				break;
			}
			case "spl-course-camera-preview-expand":
			case "ct-preview-expand":
				if (flag3)
				{
					Debug.Log("MEPropertyInspector> OnFormEvent / " + text + " " + p_target);
					Component obj = p_target as Component;
					MECameraToolPreviewInspector mECameraToolPreviewInspector = Hierarchy.FindReverse<MECameraToolPreviewInspector>(obj.transform);
					MESplineCourseCameraPreviewInspector mESplineCourseCameraPreviewInspector = Hierarchy.FindReverse<MESplineCourseCameraPreviewInspector>(obj.transform);
					if ((bool)mECameraToolPreviewInspector)
					{
						mECameraToolPreviewInspector.Expand(!mECameraToolPreviewInspector.expanded, 0.3f);
					}
					if ((bool)mESplineCourseCameraPreviewInspector)
					{
						mESplineCourseCameraPreviewInspector.Expand(!mESplineCourseCameraPreviewInspector.expanded, 0.3f);
					}
				}
				break;
			case "ctcp-camera-tracking-mode":
				if (flag)
				{
					MACameraToolControlPoint mACameraToolControlPoint9 = GetTarget<MACameraToolControlPoint>(0);
					if ((bool)mACameraToolControlPoint9)
					{
						CameraToolTrackingMode index6 = (CameraToolTrackingMode)(p_target as DRLStepperView).index;
						BeginChange(text);
						mACameraToolControlPoint9.trackingMode = index6;
						EndChange(text);
						RefreshFields(new string[6] { "ctcp-camera-offset", "ctcp-camera-fov", "ctcp-camera-orbit-angle", "ctcp-camera-distance", "ctcp-camera-tracking-delay", "ctcp-attribs-1" });
						flag4 = true;
					}
				}
				break;
			case "ctcp-camera-tracking-delay":
				if (flag)
				{
					MACameraToolControlPoint mACameraToolControlPoint8 = GetTarget<MACameraToolControlPoint>(0);
					if ((bool)mACameraToolControlPoint8)
					{
						DRLNumberFieldView obj7 = p_target as DRLNumberFieldView;
						BeginChange(text);
						float value5 = obj7.value;
						mACameraToolControlPoint8.trackingDelay = value5;
						flag4 = true;
						EndChange(text);
					}
				}
				break;
			case "ctcp-camera-orbit-angle":
				if (!flag3)
				{
					MACameraToolControlPoint mACameraToolControlPoint7 = GetTarget<MACameraToolControlPoint>(0);
					if ((bool)mACameraToolControlPoint7)
					{
						Vector3 vector = (p_target as DRLVectorFieldView).Get<Vector3>();
						BeginChange(text);
						mACameraToolControlPoint7.cameraOrbitAngle = vector;
						EndChange(text);
						flag4 = true;
					}
				}
				break;
			case "ctcp-camera-distance":
				if (flag)
				{
					MACameraToolControlPoint mACameraToolControlPoint6 = GetTarget<MACameraToolControlPoint>(0);
					if ((bool)mACameraToolControlPoint6)
					{
						DRLNumberFieldView obj6 = p_target as DRLNumberFieldView;
						BeginChange(text);
						float value4 = obj6.value;
						mACameraToolControlPoint6.cameraDistance = value4;
						flag4 = true;
						EndChange(text);
					}
				}
				break;
			case "ctcp-camera-fov":
				if (flag)
				{
					MACameraToolControlPoint mACameraToolControlPoint5 = GetTarget<MACameraToolControlPoint>(0);
					if ((bool)mACameraToolControlPoint5)
					{
						DRLNumberFieldView obj5 = p_target as DRLNumberFieldView;
						BeginChange(text);
						float value3 = obj5.value;
						mACameraToolControlPoint5.fov = value3;
						flag4 = true;
						EndChange(text);
					}
				}
				break;
			case "ctcp-camera-offset":
				if (!flag3)
				{
					MACameraToolControlPoint mACameraToolControlPoint4 = GetTarget<MACameraToolControlPoint>(0);
					if ((bool)mACameraToolControlPoint4)
					{
						Vector3 vector = (p_target as DRLVectorFieldView).Get<Vector3>();
						BeginChange(text);
						mACameraToolControlPoint4.cameraOffset = vector;
						EndChange(text);
						flag4 = true;
					}
				}
				break;
			case "ct-camera-easing-test":
				if (flag3)
				{
					MACameraToolControlPoint mACameraToolControlPoint3 = GetTarget<MACameraToolControlPoint>(0);
					MECameraToolPreviewInspector field2 = panel.GetField<MECameraToolPreviewInspector>("ct-preview");
					MACameraTool p_tool = (mACameraToolControlPoint3 ? mACameraToolControlPoint3.tool : null);
					Debug.Log("MEPropertyInspector> OnFormEvent / " + text);
					field2.AnimateLoop(p_tool, mACameraToolControlPoint3, 3f);
					flag4 = false;
				}
				break;
			case "ct-camera-easing-help":
				if (flag3)
				{
					Application.OpenURL("https://easings.net/");
				}
				break;
			case "ct-index":
			{
				if (!flag)
				{
					break;
				}
				BeginChange(text);
				MACameraToolControlPoint mACameraToolControlPoint2 = GetTarget<MACameraToolControlPoint>(0);
				MACameraTool mACameraTool = (mACameraToolControlPoint2 ? mACameraToolControlPoint2.tool : null);
				if ((bool)mACameraTool)
				{
					int index2 = (int)(p_target as DRLNumberFieldView).value - 1;
					Debug.Log("MEPropertyInspector> OnFormEvent / ct-index[" + index2 + "]");
					List<MACameraTool> list2 = base.editor.scene.FindCameraTools();
					list2.Remove(mACameraTool);
					list2.Insert(index2, mACameraTool);
					for (int num = 0; num < list2.Count; num++)
					{
						list2[num].index = num;
					}
					flag4 = true;
					EndChange(text);
				}
				break;
			}
			case "ct-camera-easing":
				if (flag)
				{
					MACameraToolControlPoint mACameraToolControlPoint = GetTarget<MACameraToolControlPoint>(0);
					if ((bool)mACameraToolControlPoint)
					{
						DRLStepperView dRLStepperView = p_target as DRLStepperView;
						BeginChange(text);
						string idByIndex = mACameraToolControlPoint.tool.animation.GetIdByIndex(dRLStepperView.index);
						mACameraToolControlPoint.tool.easingMode = idByIndex;
						EndChange(text);
						flag4 = true;
					}
				}
				break;
			case "collectable-mode":
			{
				if (!flag)
				{
					break;
				}
				MapCollectableMode value6 = (MapCollectableMode)(p_target as DRLIntStepperView).value;
				BeginChange(text);
				for (int num37 = 0; num37 < base.targets.Count; num37++)
				{
					MACollectable mACollectable3 = (MACollectable)base.targets[num37];
					if ((bool)mACollectable3)
					{
						if (mACollectable3.collectableMode != value6)
						{
							mACollectable3.collectableMode = value6;
						}
						if (mACollectable3.collectableStyle != 0)
						{
							mACollectable3.collectableStyle = 0;
						}
						mACollectable3.SetCollision();
					}
				}
				EndChange(text);
				flag4 = true;
				RefreshFields(new string[2] { "collectable-style-0", "collectable-score" });
				break;
			}
			case "collectable-style-0":
			{
				if (!flag)
				{
					break;
				}
				DRLStepperView dRLStepperView8 = p_target as DRLStepperView;
				BeginChange(text);
				for (int num30 = 0; num30 < base.targets.Count; num30++)
				{
					MACollectable mACollectable2 = (MACollectable)base.targets[num30];
					if ((bool)mACollectable2)
					{
						int num31 = dRLStepperView8.index - 1;
						if (num31 != mACollectable2.collectableStyle)
						{
							mACollectable2.collectableStyle = num31;
						}
					}
				}
				EndChange(text);
				flag4 = true;
				break;
			}
			case "collectable-size":
			{
				if (!flag)
				{
					break;
				}
				DRLNumberFieldView dRLNumberFieldView5 = p_target as DRLNumberFieldView;
				BeginChange(text);
				for (int num26 = 0; num26 < base.targets.Count; num26++)
				{
					MACollectable mACollectable = (MACollectable)base.targets[num26];
					if ((bool)mACollectable)
					{
						mACollectable.size = (int)dRLNumberFieldView5.value;
					}
				}
				EndChange(text);
				flag4 = true;
				break;
			}
			case "lgt-type":
			{
				if (!flag)
				{
					break;
				}
				LayoutGeometryType index5 = (LayoutGeometryType)(p_target as DRLStepperView).index;
				BeginChange(text);
				for (int num25 = 0; num25 < base.targets.Count; num25++)
				{
					MALayoutGeometryTool mALayoutGeometryTool5 = (MALayoutGeometryTool)base.targets[num25];
					if ((bool)mALayoutGeometryTool5 && mALayoutGeometryTool5.layoutType != index5)
					{
						mALayoutGeometryTool5.layoutType = index5;
					}
				}
				EndChange(text);
				flag4 = true;
				RefreshFields(MEInspectorFieldIds.LayoutGeometryShapeStats);
				break;
			}
			case "lgt-visibility":
			case "lgt-fill":
			{
				if (!flag)
				{
					break;
				}
				MALayoutGeometryTool mALayoutGeometryTool4 = GetTarget<MALayoutGeometryTool>(0);
				if ((bool)mALayoutGeometryTool4)
				{
					bool isOn2 = (p_target as DRLToggleView).toggle.isOn;
					LayoutParams layoutParams2 = mALayoutGeometryTool4.layoutParams;
					BeginChange(text);
					switch (text)
					{
					case "lgt-visibility":
						mALayoutGeometryTool4.previewVisible = isOn2;
						break;
					case "lgt-fill":
						layoutParams2.fill = isOn2;
						mALayoutGeometryTool4.layoutParams = layoutParams2;
						break;
					}
					EndChange(text);
					flag4 = true;
					RefreshFields(MEInspectorFieldIds.LayoutGeometryShapeStats);
				}
				break;
			}
			case "lgt-asset-size":
			case "lgt-asset-margin":
			case "lgt-asset-density":
			case "lgt-shape-radius":
			case "lgt-shape-height":
			case "lgt-shape-aperture":
			{
				MALayoutGeometryTool mALayoutGeometryTool3 = GetTarget<MALayoutGeometryTool>(0);
				if (!mALayoutGeometryTool3 || !flag)
				{
					break;
				}
				float value2 = (p_target as DRLNumberFieldView).value;
				BeginChange(text);
				for (int num24 = 0; num24 < base.targets.Count; num24++)
				{
					mALayoutGeometryTool3 = (MALayoutGeometryTool)base.targets[num24];
					switch (text)
					{
					case "lgt-asset-size":
						mALayoutGeometryTool3.assetRadius = value2;
						break;
					case "lgt-asset-margin":
						mALayoutGeometryTool3.assetMargin = value2;
						break;
					case "lgt-asset-density":
						mALayoutGeometryTool3.assetDensity = value2 / 100f;
						break;
					case "lgt-shape-radius":
						mALayoutGeometryTool3.layoutRadius = value2;
						break;
					case "lgt-shape-height":
						mALayoutGeometryTool3.layoutHeight = value2;
						break;
					case "lgt-shape-aperture":
						mALayoutGeometryTool3.layoutAperture = value2 / 100f;
						break;
					}
				}
				EndChange(text);
				flag4 = true;
				RefreshFields(MEInspectorFieldIds.LayoutGeometryShapeStats);
				break;
			}
			case "lgt-slices-size":
			case "lgt-slices-offset":
			case "lgt-random":
			case "lgt-grid-size":
			{
				MALayoutGeometryTool mALayoutGeometryTool2 = GetTarget<MALayoutGeometryTool>(0);
				if (!mALayoutGeometryTool2 || !flag)
				{
					break;
				}
				Vector3 vector4 = (p_target as DRLVectorFieldView).Get<Vector3>();
				LayoutParams layoutParams = mALayoutGeometryTool2.layoutParams;
				BeginChange(text);
				for (int num21 = 0; num21 < base.targets.Count; num21++)
				{
					mALayoutGeometryTool2 = (MALayoutGeometryTool)base.targets[num21];
					switch (text)
					{
					case "lgt-grid-size":
						mALayoutGeometryTool2.layoutGridSize = vector4;
						break;
					case "lgt-random":
						layoutParams.random = vector4;
						mALayoutGeometryTool2.layoutParams = layoutParams;
						break;
					case "lgt-slices-offset":
						layoutParams.slices.x = vector4.x / 100f;
						layoutParams.slices.y = vector4.y / 100f;
						layoutParams.slices.z = vector4.z / 100f;
						mALayoutGeometryTool2.layoutParams = layoutParams;
						break;
					case "lgt-slices-size":
						layoutParams.slices.rangeX = vector4.x / 100f;
						layoutParams.slices.rangeY = vector4.y / 100f;
						layoutParams.slices.rangeZ = vector4.z / 100f;
						mALayoutGeometryTool2.layoutParams = layoutParams;
						break;
					}
				}
				EndChange(text);
				flag4 = true;
				RefreshFields(MEInspectorFieldIds.LayoutGeometryShapeStats);
				break;
			}
			case "lgt-apply":
			{
				if (!flag3)
				{
					break;
				}
				MALayoutGeometryTool mALayoutGeometryTool = GetTarget<MALayoutGeometryTool>(0);
				if (!mALayoutGeometryTool || mALayoutGeometryTool.isDefaultTemplate)
				{
					break;
				}
				List<MAEntity> list7 = new List<MAEntity>();
				for (int num12 = 0; num12 < mALayoutGeometryTool.layoutCount; num12++)
				{
					if (mALayoutGeometryTool.previews[num12] is MAEntity)
					{
						list7.Add(mALayoutGeometryTool.previews[num12] as MAEntity);
					}
				}
				base.editor.controller.ApplyClone(list7, null, p_force_parent: true, p_force_selection: false);
				break;
			}
			case "material-color-reset":
			{
				if (!flag3)
				{
					break;
				}
				BeginChange(text);
				Debug.Log("MEPropertyInspector> OnFieldsFormNotification / field[" + text + "]");
				string[] array = new string[4] { "emission", "color0", "color1", "color2" };
				for (int num7 = 0; num7 < base.targets.Count; num7++)
				{
					MARenderer spline6 = GetTarget<MARenderer>(num7);
					if (spline6 is MASplineControlPoint)
					{
						spline6 = (spline6 as MASplineControlPoint).spline;
					}
					if (!spline6)
					{
						continue;
					}
					for (int num8 = 0; num8 < array.Length; num8++)
					{
						MARendererMaterial materialById2 = spline6.GetMaterialById(array[num8]);
						if ((bool)materialById2)
						{
							switch (array[num8])
							{
							case "emission":
								spline6.emissionColor = materialById2.defaultColor;
								break;
							case "color0":
								spline6.color0 = materialById2.defaultColor;
								break;
							case "color1":
								spline6.color1 = materialById2.defaultColor;
								break;
							case "color2":
								spline6.color2 = materialById2.defaultColor;
								break;
							}
						}
					}
				}
				RefreshFields(MEInspectorFieldIds.MaterialColorFields);
				EndChange(text);
				break;
			}
			case "material-color-emission":
			case "material-color-intensity":
			case "material-color-0":
			case "material-color-1":
			case "material-color-2":
			{
				if (!flag)
				{
					break;
				}
				float colorIntensity = 1.5f;
				Color clear = Color.clear;
				if (text != null && text == "material-color-intensity")
				{
					DRLNumberFieldView dRLNumberFieldView = p_target as DRLNumberFieldView;
					colorIntensity = Mathf.Max(dRLNumberFieldView.value, dRLNumberFieldView.minValue);
					clear = panel.GetField<DRLColorPickerView>("material-color-emission").current;
				}
				else
				{
					clear = (Color)p_data[0];
				}
				BeginChange(text);
				for (int l = 0; l < base.targets.Count; l++)
				{
					MARenderer spline3 = GetTarget<MARenderer>(l);
					if (text != null && text == "material-color-intensity")
					{
						spline3.colorIntensity = colorIntensity;
					}
					float colorIntensity2 = spline3.colorIntensity;
					bool flag5 = spline3 is MASplineControlPoint;
					if (flag5)
					{
						spline3 = (spline3 as MASplineControlPoint).spline;
					}
					if (!spline3)
					{
						continue;
					}
					string text2 = "";
					switch (text)
					{
					case "material-color-intensity":
						text2 = "emission";
						break;
					case "material-color-emission":
						text2 = "emission";
						break;
					case "material-color-0":
						text2 = "color0";
						break;
					case "material-color-1":
						text2 = "color1";
						break;
					case "material-color-2":
						text2 = "color2";
						break;
					}
					if (string.IsNullOrEmpty(text2))
					{
						continue;
					}
					MARendererMaterial materialById = spline3.GetMaterialById(text2);
					if (flag5 || (bool)materialById)
					{
						switch (text)
						{
						case "material-color-intensity":
							spline3.emissionColor = clear * colorIntensity2;
							break;
						case "material-color-emission":
							spline3.emissionColor = clear * colorIntensity2;
							break;
						case "material-color-0":
							spline3.color0 = clear;
							break;
						case "material-color-1":
							spline3.color1 = clear;
							break;
						case "material-color-2":
							spline3.color2 = clear;
							break;
						}
					}
				}
				EndChange(text);
				break;
			}
			case "material-style-reset":
			{
				if (!flag3)
				{
					break;
				}
				BeginChange(text);
				for (int i = 0; i < base.targets.Count; i++)
				{
					MARenderer spline = GetTarget<MARenderer>(i);
					if (spline is MASplineControlPoint)
					{
						spline = (spline as MASplineControlPoint).spline;
					}
					if ((bool)spline)
					{
						if ((bool)spline.styleList0)
						{
							spline.style0 = 0;
						}
						if ((bool)spline.styleList1)
						{
							spline.style1 = 0;
						}
						if ((bool)spline.styleList2)
						{
							spline.style2 = 0;
						}
					}
				}
				RefreshFields(MEInspectorFieldIds.MaterialStyleFields);
				EndChange(text);
				break;
			}
			case "material-style-0":
			case "material-style-1":
			case "material-style-2":
			{
				if (!flag)
				{
					break;
				}
				int index7 = (p_target as DRLStepperView).index;
				BeginChange(text);
				for (int num33 = 0; num33 < base.targets.Count; num33++)
				{
					MARenderer spline13 = GetTarget<MARenderer>(num33);
					if (spline13 is MASplineControlPoint)
					{
						spline13 = (spline13 as MASplineControlPoint).spline;
					}
					if (!spline13)
					{
						continue;
					}
					switch (text)
					{
					case "material-style-0":
						if ((bool)spline13.styleList0)
						{
							spline13.style0 = index7;
						}
						break;
					case "material-style-1":
						if ((bool)spline13.styleList1)
						{
							spline13.style1 = index7;
						}
						break;
					case "material-style-2":
						if ((bool)spline13.styleList2)
						{
							spline13.style2 = index7;
						}
						break;
					}
				}
				EndChange(text);
				break;
			}
			case "map-thumb":
			{
				if (!flag3)
				{
					break;
				}
				UIElementView uIElementView = p_target as UIElementView;
				MEMapThumbInspector f2 = Hierarchy.FindReverse<MEMapThumbInspector>(uIElementView.transform);
				if (f2.isSaving)
				{
					break;
				}
				f2.isSaving = true;
				if (base.editor.model.state.render != MERenderStateType.Scene)
				{
					base.editor.app.view.audio.PlayUIGenericError();
					break;
				}
				float num27 = Screen.height;
				float num28 = num27 / 720f;
				float f3 = (float)Screen.width / num28;
				num27 = 720f;
				int p_width = Mathf.FloorToInt(f3);
				int p_height = Mathf.FloorToInt(num27);
				base.editor.CaptureScreenshot(p_width, p_height, delegate(string p_state, string p_url, Texture2D p_texture)
				{
					switch (p_state)
					{
					case "map-editor.save.map-thumb@error":
						f2.isSaving = false;
						break;
					case "map-editor.save.map-thumb@success":
					{
						MapData data = base.editor.model.data;
						base.editor.app.view.audio.PlayUISnapshot();
						data.mapThumbURL = p_url;
						f2.isSaving = false;
						bool num38 = base.editor.app.model.storage.state.player.profile.isDeveloper || DRLApp.offline;
						if (!DRLApp.offline)
						{
							Debug.Log("MEPropertyInspector> OnFormEvent / map-thumb[" + p_url + "]");
							f2.LoadImage(p_url);
						}
						else
						{
							f2.photoFade.alpha = -0.1f;
							f2.SetImage(p_texture);
						}
						if (num38)
						{
							byte[] bytes = p_texture.EncodeToJPG(90);
							File.WriteAllBytes(DRLPaths.Storage.offlineMapEditorMapsRoot + data.guid + ".jpg", bytes);
						}
						break;
					}
					case "map-editor.save.map-thumb@start":
						break;
					}
				});
				break;
			}
			case "map-title":
			{
				DRLInputFieldView dRLInputFieldView = p_target as DRLInputFieldView;
				string text6 = dRLInputFieldView.field.text;
				if (flag2)
				{
					if (string.IsNullOrEmpty(text6))
					{
						string text7 = (dRLInputFieldView.field.text = base.editor.app.model.storage.state.player.profile.username + " Map");
						text6 = text7;
					}
				}
				else if (flag)
				{
					base.editor.model.data.mapTitle = text6;
				}
				flag4 = true;
				break;
			}
			case "map-track-id":
			{
				if (!flag)
				{
					break;
				}
				DRLStepperView dRLStepperView7 = p_target as DRLStepperView;
				if (dRLStepperView7.index >= 0 && dRLStepperView7.index < dRLStepperView7.labels.Length)
				{
					string tid = dRLStepperView7.labels[dRLStepperView7.index].ToLower();
					base.editor.model.data.trackId = tid;
					Debug.Log("MEPropertyInspector> OnFormEvent / track-id[" + base.editor.model.data.trackId + "]");
					this.TimerRunOnce(delegate
					{
						base.editor.controller.game.model.level.track.SetTrackEnabled(tid, p_flag: true);
					}, 0.2f);
					flag4 = true;
				}
				break;
			}
			case "map-category":
				if (flag)
				{
					DRLStepperView dRLStepperView6 = p_target as DRLStepperView;
					GameFlag mapCategoryFlag = (GameFlag)(300 + dRLStepperView6.index);
					base.editor.model.data.mapCategoryFlag = mapCategoryFlag;
					Debug.Log("MEPropertyInspector> OnFormEvent / map-category[" + mapCategoryFlag.ToString() + "]");
					flag4 = true;
				}
				break;
			case "map-laps":
				if (flag)
				{
					DRLStepperView dRLStepperView5 = p_target as DRLStepperView;
					base.editor.model.data.mode.race.lapCount = dRLStepperView5.index;
					Debug.Log("MEPropertyInspector> OnFormEvent / map-laps[" + dRLStepperView5.index + "]");
					flag4 = true;
				}
				break;
			case "map-difficulty":
				if (flag)
				{
					DRLStepperView dRLStepperView4 = p_target as DRLStepperView;
					base.editor.model.data.mapDifficulty = dRLStepperView4.index;
					Debug.Log("MEPropertyInspector> OnFormEvent / map-difficulty[" + dRLStepperView4.index + "]");
					flag4 = true;
				}
				break;
			case "map-lighting":
				if (flag)
				{
					DRLStepperView dRLStepperView3 = p_target as DRLStepperView;
					base.editor.model.data.mapLighting = dRLStepperView3.index;
					base.editor.controller.game.level.SetLightingPreset(dRLStepperView3.index);
					Debug.Log("MEPropertyInspector> OnFormEvent / map-lighting[" + dRLStepperView3.index + "]");
					flag4 = true;
				}
				break;
			case "map-style-0":
			case "map-style-1":
			case "map-style-2":
				if (flag)
				{
					DRLIntStepperView dRLIntStepperView = p_target as DRLIntStepperView;
					int p_style = -1;
					switch (text)
					{
					case "map-style-0":
						base.editor.model.data.mapStyle0 = dRLIntStepperView.value;
						p_style = 0;
						break;
					case "map-style-1":
						base.editor.model.data.mapStyle1 = dRLIntStepperView.value;
						p_style = 1;
						break;
					case "map-style-2":
						base.editor.model.data.mapStyle2 = dRLIntStepperView.value;
						p_style = 2;
						break;
					}
					base.editor.scene.SetRenderersMapStyle(p_style, dRLIntStepperView.value);
					level.settings.scene.SetStyle(p_style, dRLIntStepperView.value);
					Debug.Log("MEPropertyInspector> OnFormEvent / map-style[" + p_style + "] style-index[" + dRLIntStepperView.value + "]");
					flag4 = true;
				}
				break;
			case "map-asset-layer-2":
			case "map-asset-layer-1":
			case "map-asset-layer-0":
				if (flag)
				{
					DRLStepperView dRLStepperView2 = p_target as DRLStepperView;
					SetFieldActive(dRLStepperView2, p_flag: true);
					switch (text)
					{
					case "map-asset-layer-0":
						base.editor.model.data.mapAssetLayer0 = dRLStepperView2.index;
						level.SetAssetLayerIndex(0, dRLStepperView2.index);
						break;
					case "map-asset-layer-1":
						base.editor.model.data.mapAssetLayer1 = dRLStepperView2.index;
						level.SetAssetLayerIndex(1, dRLStepperView2.index);
						break;
					case "map-asset-layer-2":
						base.editor.model.data.mapAssetLayer2 = dRLStepperView2.index;
						level.SetAssetLayerIndex(2, dRLStepperView2.index);
						break;
					}
					flag4 = true;
				}
				break;
			case "map-visibility":
				if (flag)
				{
					DRLToggleView dRLToggleView6 = p_target as DRLToggleView;
					dRLToggleView6.GetComponent<SwitcherComponent>().index = (dRLToggleView6.toggle.isOn ? 1 : 0);
					base.editor.model.data.isPublic = dRLToggleView6.toggle.isOn;
					Debug.Log("MEPropertyInspector> OnFormEvent / map-visiblity[" + dRLToggleView6.toggle.isOn + "]");
					RefreshFields(new string[1] { "map-allow-copy" });
					flag4 = true;
				}
				break;
			case "map-allow-copy":
				if (flag)
				{
					DRLToggleView dRLToggleView5 = p_target as DRLToggleView;
					base.editor.model.data.allowCopy = dRLToggleView5.toggle.isOn;
					Debug.Log("MEPropertyInspector> OnFormEvent / map-allow-copy[" + dRLToggleView5.toggle.isOn + "]");
					flag4 = true;
				}
				break;
			case "map-base-assets":
				if (flag)
				{
					DRLToggleView dRLToggleView4 = p_target as DRLToggleView;
					SetFieldActive(dRLToggleView4, p_flag: true);
					base.editor.model.data.baseAssetsEnabled = dRLToggleView4.toggle.isOn;
					base.editor.controller.game.model.level.SetBaseAssetsEnabled(dRLToggleView4.toggle.isOn);
					base.editor.controller.RefreshRendererStats();
					flag4 = true;
				}
				break;
			case "map-collabs":
			{
				if (!flag2)
				{
					break;
				}
				DRLInputFieldView f = p_target as DRLInputFieldView;
				string text4 = f.field.text;
				if (string.IsNullOrEmpty(text4))
				{
					break;
				}
				text4 = text4.ToLower();
				ServiceModel service = base.editor.app.model.service;
				DRLCommunityMapData p_query = new DRLCommunityMapData
				{
					["q"] = "@" + text4,
					page = 1,
					limit = 5
				};
				Debug.Log("MEPropertyInspector> MapCollabs / q[@" + text4 + "]");
				service.GetCommunityMaps(p_query, delegate(DRLCommunityMapResult p_result)
				{
					DRLCommunityMapData[] data = p_result.data;
					if (data == null)
					{
						Debug.Log("MEPropertyInspector> MapCollabs / No Results Found!");
					}
					else if (data.Length == 0)
					{
						Debug.Log("MEPropertyInspector> MapCollabs / No Results Found!");
					}
					else
					{
						DRLCommunityMapData dRLCommunityMapData = data[0];
						DRLPlayerProfileData p_data2 = new DRLPlayerProfileData
						{
							profileName = dRLCommunityMapData.profileName,
							profileColorHex = dRLCommunityMapData.profileColorHex,
							playerId = dRLCommunityMapData.playerId,
							profileThumbURL = dRLCommunityMapData.profileThumbURL
						};
						base.editor.model.data.AddCollaborator(p_data2);
						base.editor.app.view.audio.PlayUIMapEditorPlace();
						base.editor.controller.ScheduleSave();
						RefreshFields(new string[1] { "map-collab-list" });
						f.text = "";
					}
				});
				break;
			}
			case "map-collab-list":
			{
				DRLMapEditorCollabItem dRLMapEditorCollabItem = p_target as DRLMapEditorCollabItem;
				string text3 = ((p_data.Length != 0) ? ((string)p_data[0]) : "");
				Debug.Log("MEPropertyInspector> MapCollabList / collab-item[" + dRLMapEditorCollabItem?.ToString() + "] label[" + text3 + "]");
				if (text3 != null && text3 == "delete")
				{
					ListComponent field = panel.GetField<ListComponent>("map-collab-list");
					if (dRLMapEditorCollabItem.data != null)
					{
						Debug.Log("MEPropertyInspector> MapCollabList / delete[" + dRLMapEditorCollabItem.data.profileName + "]");
						base.editor.model.data.RemoveCollaboratorById(dRLMapEditorCollabItem.data.playerId);
						base.editor.app.view.audio.PlayUIMapEditorDelete();
					}
					field.Remove(dRLMapEditorCollabItem);
					flag4 = true;
				}
				break;
			}
			case "physics-drop-delay":
			case "physics-drop-duration":
			case "physics-drop-spin":
			case "physics-drop-v-forward":
			case "physics-drop-v-up":
			{
				if (!flag)
				{
					break;
				}
				DRLNumberFieldView dRLNumberFieldView2 = p_target as DRLNumberFieldView;
				Vector3 velocity = state.physics.velocity;
				switch (text)
				{
				case "physics-drop-delay":
					state.physics.delay = dRLNumberFieldView2.value;
					break;
				case "physics-drop-duration":
					state.physics.duration = dRLNumberFieldView2.value;
					if (Mathf.Abs(dRLNumberFieldView2.value) <= 0.001f)
					{
						dRLNumberFieldView2.input.text = "";
					}
					break;
				case "physics-drop-v-forward":
					velocity.z = dRLNumberFieldView2.value;
					break;
				case "physics-drop-v-up":
					velocity.y = dRLNumberFieldView2.value;
					break;
				case "physics-drop-spin":
				{
					float t = Mathf.Clamp01(dRLNumberFieldView2.value / 100f);
					state.physics.angularVelocity = Vector3.Lerp(Vector3.zero, Vector3.one * 50f, t);
					break;
				}
				}
				state.physics.velocity = velocity;
				break;
			}
			case "replay-cache-delete":
			{
				if (base.editor.model.lockInput || !flag3 || base.editor.model.cachedReplaysCount <= 0)
				{
					break;
				}
				List<FileInfo> list = base.editor.app.model.storage.replays.FindAllMapEditorReplays();
				for (int k = 0; k < list.Count; k++)
				{
					FileInfo fileInfo = list[k];
					if (fileInfo.Exists)
					{
						fileInfo.Delete();
					}
				}
				base.editor.model.cachedReplays.Clear();
				base.editor.model.cachedReplaysV2.Clear();
				base.editor.ui.SetReplayCacheCount(0);
				RefreshFields(new string[2] { "replay-cache-delete", "prefs-replay-cache" });
				break;
			}
			case "map-save":
				if (!base.editor.model.lockInput && flag3)
				{
					base.editor.controller.ScheduleSave(0.1f, p_force: true);
				}
				break;
			case "map-auto-save":
				if (flag)
				{
					DRLToggleView dRLToggleView = p_target as DRLToggleView;
					base.editor.model.data.prefs.autoSave = dRLToggleView.toggle.isOn;
					Debug.Log("MEPropertyInspector> OnFormEvent / map-auto-save[" + dRLToggleView.toggle.isOn + "]");
					base.editor.controller.ScheduleSave(3f, p_force: true);
				}
				break;
			}
			if (flag4)
			{
				base.editor.controller.ScheduleSave();
			}
		}

		private bool IsNoForceGrid(MapAssetType f)
		{
			return f == MapAssetType.NoForceGrid;
		}
	}
}
