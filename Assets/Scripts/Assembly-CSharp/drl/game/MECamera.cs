using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class MECamera : DroneCamera
	{
		private MapEditorView m_editor;

		public float panSpeed = 0.005f;

		public Vector3 panPosition;

		public List<Component> blueprintTargets;

		public List<Component> selectionTargets;

		public List<Component> hilightTargets;

		private Activity m_blueprint_timer;

		private Dictionary<SelectionOutlineEffect, Activity> m_sel_watcher = new Dictionary<SelectionOutlineEffect, Activity>();

		public Camera blueprint
		{
			get
			{
				if (cameras.Count > 4)
				{
					return cameras[4];
				}
				return null;
			}
		}

		public Camera hilight
		{
			get
			{
				if (cameras.Count > 2)
				{
					return cameras[2];
				}
				return null;
			}
		}

		public Camera selection
		{
			get
			{
				if (cameras.Count > 3)
				{
					return cameras[3];
				}
				return null;
			}
		}

		public MapEditorView editor
		{
			get
			{
				if (!m_editor)
				{
					return m_editor = GetComponentInParent<MapEditorView>();
				}
				return m_editor;
			}
		}

		protected void Start()
		{
			SetMode(DMEModeType.Action);
			base.orbit.constraint.distanceMin = -0.05f;
			base.orbit.constraint.distanceMax = 10f;
		}

		public void SetMotionBlurEnabled(bool p_flag)
		{
			List<PostProcessingBehaviour> list = Hierarchy.FindAll<PostProcessingBehaviour>(base.transform);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].profile.motionBlur.enabled = p_flag;
			}
		}

		public void SetBlueprint(bool p_flag, params Component[] p_targets)
		{
			blueprint.enabled = p_flag;
			float p_delay = (p_flag ? 0.1f : 0f);
			base.fx.radioLock = p_flag;
			base.fx.FadeBlur(p_flag ? 0.1f : 0f, 0.5f, p_delay);
			base.fx.FadeGrayscale(p_flag ? 1f : 0f, 0.5f, p_delay);
			p_delay = (p_flag ? 0f : 0.6f);
			if (m_blueprint_timer != null)
			{
				m_blueprint_timer.Stop();
			}
			m_blueprint_timer = Activity.RunOnce(delegate
			{
				List<Component> list = blueprintTargets;
				for (int i = 0; i < list.Count; i++)
				{
					if ((bool)list[i])
					{
						UnityEngine.Object.Destroy(list[i].gameObject);
					}
				}
				list.Clear();
				if (p_flag)
				{
					List<Component> list2 = new List<Component>();
					for (int j = 0; j < p_targets.Length; j++)
					{
						if ((bool)p_targets[j])
						{
							GameObject gameObject = p_targets[j].gameObject;
							Transform transform = gameObject.transform;
							gameObject = new GameObject();
							gameObject.name = transform.name + "-" + blueprint.name;
							gameObject.transform.SetParent(transform, worldPositionStays: false);
							gameObject.transform.parent = transform;
							Transform obj = gameObject.transform;
							Vector3 localPosition = (gameObject.transform.localEulerAngles = Vector3.zero);
							obj.localPosition = localPosition;
							gameObject.transform.localScale = Vector3.one;
							list2.Add(gameObject.transform);
						}
					}
					for (int k = 0; k < list2.Count; k++)
					{
						if ((bool)list2[k])
						{
							GameObject gameObject2 = list2[k].gameObject;
							MARenderer componentInParent = gameObject2.GetComponentInParent<MARenderer>();
							List<Renderer> list3 = (componentInParent ? componentInParent.renderers : null);
							if (list3 != null)
							{
								for (int l = 0; l < list3.Count; l++)
								{
									if (list3[l].gameObject.activeInHierarchy)
									{
										Renderer renderer = list3[l];
										if (!renderer.name.Contains("lod0") && !renderer.name.Contains("lod2"))
										{
											Vector3 position = renderer.transform.position;
											Quaternion rotation = renderer.transform.rotation;
											Vector3 localScale = renderer.transform.localScale;
											renderer = UnityEngine.Object.Instantiate(renderer);
											renderer.transform.SetParent(gameObject2.transform, worldPositionStays: true);
											renderer.transform.position = position;
											renderer.transform.rotation = rotation;
											renderer.transform.localScale = localScale;
											renderer.gameObject.layer = 31;
											renderer.name = renderer.name.Replace("(Clone)", "");
										}
									}
								}
							}
						}
					}
					list.AddRange(list2);
				}
			}, p_delay);
		}

		public void SetSelection(bool p_flag, params MAEntity[] p_targets)
		{
			SetSelectionEffect(selection, p_flag, p_targets);
		}

		public void SetHilight(bool p_flag, params MAEntity[] p_targets)
		{
			SetSelectionEffect(hilight, p_flag, p_targets);
		}

		protected void SetSelectionEffect(Camera p_camera, bool p_flag, params MAEntity[] p_targets)
		{
			SelectionOutlineEffect component = p_camera.GetComponent<SelectionOutlineEffect>();
			component.camera.enabled = p_flag;
			if (!p_flag)
			{
				component.targets = null;
			}
			List<MAEntity> list = new List<MAEntity>(p_targets);
			List<MARenderer> list2 = new List<MARenderer>();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] is MARenderer)
				{
					list2.Add(list[i] as MARenderer);
				}
			}
			RefreshSelectionRenderers(component, list2);
			WatchSelectionRenderers(component, list2);
		}

		protected void RefreshSelectionRenderers(SelectionOutlineEffect p_soe, List<MARenderer> p_list)
		{
			List<Renderer> list = new List<Renderer>();
			if (p_list == null)
			{
				return;
			}
			for (int i = 0; i < p_list.Count; i++)
			{
				MARenderer mARenderer = p_list[i];
				if (!mARenderer)
				{
					p_list.RemoveAt(i--);
					continue;
				}
				List<Renderer> renderers = mARenderer.renderers;
				if (renderers != null)
				{
					list.AddRange(renderers);
				}
			}
			if ((bool)p_soe)
			{
				p_soe.targets = list;
				p_soe.enabled = list.Count > 0;
				if ((bool)p_soe.camera)
				{
					p_soe.camera.enabled = list.Count > 0;
				}
			}
		}

		protected void WatchSelectionRenderers(SelectionOutlineEffect p_soe, List<MARenderer> p_list)
		{
			(m_sel_watcher.ContainsKey(p_soe) ? m_sel_watcher[p_soe] : null)?.Stop();
			List<Renderer> rl = p_soe.targets;
			Activity value = Activity.Run((Func<bool>)delegate
			{
				bool flag = false;
				for (int i = 0; i < rl.Count; i++)
				{
					if (!rl[i])
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					RefreshSelectionRenderers(p_soe, p_list);
					rl = p_soe.targets;
				}
				return true;
			}, 0f, false);
			m_sel_watcher[p_soe] = value;
		}

		public void SetMode(DMEModeType p_type, float p_distance)
		{
			switch (p_type)
			{
			case DMEModeType.Action:
				base.wasd.enabled = false;
				base.wasd.usePhysics = true;
				base.wasd.ResetInput();
				base.orbit.SetDistanceSnap(p_distance);
				break;
			case DMEModeType.WASD:
				base.wasd.enabled = true;
				base.wasd.usePhysics = true;
				base.wasd.orbitDragKey = KeyCode.Mouse1;
				base.wasd.ResetInput();
				base.orbit.SetDistanceSnap(p_distance);
				break;
			case DMEModeType.Orbit:
				base.wasd.enabled = true;
				base.wasd.usePhysics = false;
				base.wasd.orbitDragKey = KeyCode.Mouse0;
				base.wasd.ResetInput();
				base.orbit.SetDistanceSnap(p_distance);
				break;
			case DMEModeType.Pan:
				base.wasd.enabled = true;
				base.wasd.usePhysics = false;
				base.wasd.orbitDragKey = KeyCode.Mouse0;
				base.wasd.ResetInput();
				base.orbit.constraint.distanceMin = -0.05f;
				base.orbit.constraint.distanceMax = 10f;
				base.wasd.scrollStep = 0.5f;
				base.orbit.SetDistanceSnap(p_distance);
				break;
			}
		}

		public void SetMode(DMEModeType p_type)
		{
			SetMode(p_type, -0.05f);
		}

		public void Focus(params MAEntity[] p_targets)
		{
			if (p_targets.Length != 0)
			{
				Bounds bounds = GetBounds(base.orbit.anchor, p_targets);
				Vector3 size = bounds.size;
				float magnitude = size.magnitude;
				float p_to = Mathf.Max(size.x, size.y, size.z) * 0.8f;
				bool use_physics = base.wasd.usePhysics;
				OrbitTransform.Transition transition = base.orbit.transition;
				base.wasd.usePhysics = false;
				base.orbit.transition = (OrbitTransform.Transition)4369;
				Tween.Kill(base.orbit, "distance");
				Tween.Kill(base.orbit, "anchor");
				base.orbit.constraint.distanceMin = magnitude * 0.25f;
				base.orbit.constraint.distanceMax = magnitude * 1.5f;
				float t = Mathf.Clamp01((magnitude - 0.5f) / 10f);
				base.wasd.scrollStep = Mathf.Lerp(0.5f, 5f, t);
				Tween.Add(base.orbit, "distance", p_to, 0.3f, 0.01f, Cubic.Out);
				Tween.Add(base.orbit, "anchor", bounds.center, 0.3f, 0.01f, Cubic.Out).onComplete = delegate
				{
					base.wasd.usePhysics = use_physics;
					base.orbit.transition = transition;
				};
			}
		}

		public void Pan(Vector2 p_offset, float p_speed)
		{
			Vector2 vector = p_offset;
			Vector3 vector2 = panPosition;
			float x = vector.x;
			x = ((x < 0f) ? (-1f) : 1f) * Mathf.Pow(Mathf.Abs(x) * p_speed, 1f) / p_speed;
			float y = vector.y;
			y = ((y < 0f) ? (-1f) : 1f) * Mathf.Pow(Mathf.Abs(y) * p_speed, 1f) / p_speed;
			Vector3 zero = Vector3.zero;
			zero += base.orbit.transform.right * x * (0f - p_speed);
			zero += base.orbit.transform.up * y * (0f - p_speed);
			Vector3 anchor = base.orbit.anchor;
			anchor = vector2 + zero;
			base.orbit.anchor = anchor;
		}

		public void Pan(Vector2 p_offset)
		{
			Pan(p_offset, panSpeed);
		}

		public bool IsRendererVisible(MARenderer p_target)
		{
			Camera camera = base.main;
			if (!camera)
			{
				return false;
			}
			if (!p_target)
			{
				return false;
			}
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
			List<Renderer> renderers = p_target.renderers;
			bool flag = false;
			for (int i = 0; i < renderers.Count; i++)
			{
				Renderer renderer = renderers[i];
				if ((bool)renderer && renderer.enabled && renderer.gameObject.activeInHierarchy)
				{
					flag = flag || GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
				}
			}
			return flag;
		}

		protected Bounds GetBounds(Vector3 p_center, params MAEntity[] p_targets)
		{
			Bounds result = default(Bounds);
			if (p_targets.Length == 0)
			{
				result.center = p_center;
				result.size = Vector3.one;
				return result;
			}
			for (int i = 0; i < p_targets.Length; i++)
			{
				MAEntity mAEntity = p_targets[i];
				Bounds bounds = mAEntity.GetBounds();
				if (mAEntity is MAGuide && bounds.size.magnitude < 0.5f)
				{
					bounds.size = Vector3.one * 1f;
					bounds.center = mAEntity.transform.position;
				}
				if (i <= 0)
				{
					result = bounds;
				}
				else
				{
					result.Encapsulate(bounds);
				}
			}
			return result;
		}
	}
}
