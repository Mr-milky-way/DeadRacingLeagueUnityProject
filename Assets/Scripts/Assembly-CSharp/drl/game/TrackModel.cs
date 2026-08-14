using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class TrackModel : Model<DRLApp>
	{
		public TrackSettings settings;

		public DRLMapTrack data;

		public GameObject[] containers;

		private Transform m_tracks;

		private Transform m_animations;

		private Transform m_anchors;

		[SerializeField]
		private List<Animator> m_animators;

		[SerializeField]
		private GameObject m_root;

		public MAEntity rootMap;

		public List<RenderingProbeGroup> renderingProbeGroups;

		public List<MACameraTool> cameraTools;

		public List<MASpline> courseCameras;

		public Transform podiumAnimation;

		private Transform m_starts;

		[SerializeField]
		private SplineComponent m_path_spline;

		public List<Transform> podiums;

		public List<MapAssetAction> actions;

		[SerializeField]
		private List<Collider> m_gates;

		[SerializeField]
		private List<Collider> m_base_gates;

		public int lapLoopStartIndex;

		public int lapLoopEndIndex;

		public int lapLoopCount;

		public List<float> gateDistances;

		private RaycastHit[] hits;

		public SplineTracerComponent pathTrace => AssertFind<SplineTracerComponent>("path-tracer");

		public Transform tracks
		{
			get
			{
				if ((bool)m_tracks)
				{
					return m_tracks;
				}
				if (containers == null)
				{
					return null;
				}
				for (int i = 0; i < containers.Length; i++)
				{
					if (containers[i] != null && containers[i].name == "tracks")
					{
						m_tracks = containers[i].transform;
						break;
					}
				}
				return m_tracks;
			}
		}

		public Transform animations
		{
			get
			{
				if ((bool)m_animations)
				{
					return m_animations;
				}
				if (containers == null)
				{
					return null;
				}
				for (int i = 0; i < containers.Length; i++)
				{
					if (containers[i] != null && containers[i].name == "animations")
					{
						m_animations = containers[i].transform;
						break;
					}
				}
				return m_animations;
			}
		}

		public Transform anchors
		{
			get
			{
				if ((bool)m_anchors)
				{
					return m_anchors;
				}
				if (!root)
				{
					return null;
				}
				return m_anchors = root.transform.Find("anchors");
			}
		}

		public List<Animator> animators
		{
			get
			{
				if (m_animators == null)
				{
					m_animators = new List<Animator>();
				}
				if ((bool)animations)
				{
					m_animators = Hierarchy.FindAll<Animator>(m_animations);
				}
				return m_animators;
			}
		}

		public GameObject root
		{
			get
			{
				if ((bool)m_root)
				{
					return m_root;
				}
				Transform transform = tracks;
				if (!transform)
				{
					return null;
				}
				if (!data)
				{
					return null;
				}
				transform = transform.Find(data.id);
				if (!transform)
				{
					return null;
				}
				m_root = transform.gameObject;
				if ((bool)m_root)
				{
					OnRootLoaded();
				}
				return m_root;
			}
		}

		public Transform starts
		{
			get
			{
				if ((bool)m_starts)
				{
					return m_starts;
				}
				if (!root)
				{
					return null;
				}
				Transform transform = root.transform.Find("starts");
				if ((bool)transform)
				{
					return m_starts = transform;
				}
				transform = new GameObject("starts").transform;
				transform.transform.parent = root.transform;
				transform.transform.localPosition = Vector3.zero;
				transform.transform.localRotation = Quaternion.identity;
				return m_starts = transform;
			}
		}

		public Animator trackAnimation
		{
			get
			{
				if (animators.Count <= 0)
				{
					return null;
				}
				return animators[0];
			}
		}

		public bool hasTrackAnimation => animators.Count > 0;

		public SplineComponent pathSpline
		{
			get
			{
				if ((bool)m_path_spline)
				{
					return m_path_spline;
				}
				Transform transform = root.transform.Find("path");
				return m_path_spline = (transform ? transform.GetComponent<SplineComponent>() : null);
			}
			set
			{
				m_path_spline = value;
			}
		}

		public List<Collider> gates
		{
			get
			{
				return m_gates;
			}
			set
			{
				if (m_gates != null || m_base_gates != null)
				{
					for (int i = 0; i < m_gates.Count; i++)
					{
						Collider collider = m_gates[i];
						if (collider.name.Contains("$lap-gate"))
						{
							Object.Destroy(collider.gameObject);
						}
					}
				}
				List<Collider> collection = ((value == null) ? new List<Collider>() : value);
				m_gates = new List<Collider>(collection);
				m_base_gates = new List<Collider>(collection);
			}
		}

		public int laps
		{
			get
			{
				int num = Mathf.Max(lapLoopEndIndex - lapLoopStartIndex + 1, 0);
				if (lapLoopCount > 0)
				{
					return num / lapLoopCount;
				}
				return 0;
			}
		}

		public void AddActionListener(UnityAction<AssetActionEvent> p_callback)
		{
			for (int i = 0; i < actions.Count; i++)
			{
				if (actions[i].OnEvent != null)
				{
					actions[i].OnEvent.AddListener(p_callback);
				}
			}
		}

		public void EvaluateActions(float p_time)
		{
			for (int i = 0; i < actions.Count; i++)
			{
				if (actions[i].gameObject.activeInHierarchy)
				{
					actions[i].Evaluate(p_time);
				}
			}
		}

		public void ClearActionEvents()
		{
			for (int i = 0; i < actions.Count; i++)
			{
				if (actions[i].OnEvent != null)
				{
					actions[i].OnEvent.RemoveAllListeners();
				}
			}
		}

		public void SetActionsMode(MapAssetActionMode p_mode)
		{
			for (int i = 0; i < actions.Count; i++)
			{
				actions[i].mode = p_mode;
			}
		}

		public void RestoreActions()
		{
			for (int i = 0; i < actions.Count; i++)
			{
				actions[i].Restore();
			}
		}

		public void SetActionsEnabled(bool p_flag)
		{
			for (int i = 0; i < actions.Count; i++)
			{
				actions[i].SetActive(p_flag);
			}
		}

		public int GetLapIndex(int p_gate_index)
		{
			int num = p_gate_index;
			if (num < lapLoopStartIndex)
			{
				return 0;
			}
			if (num > lapLoopEndIndex)
			{
				return laps - 1;
			}
			num = num - lapLoopStartIndex + 1;
			int num2 = lapLoopCount;
			return Mathf.Clamp((num2 > 0) ? (num / num2) : 0, 0, laps - 1);
		}

		public void SetStartsFrontTransform(Transform p_target, Vector3 p_offset, bool p_lookat_center = false)
		{
			if ((bool)p_target)
			{
				if (podiums.Count <= 0 || !root)
				{
					p_target.position = new Vector3(0f, 20f, 0f);
				}
				Vector3 zero = Vector3.zero;
				Vector3 zero2 = Vector3.zero;
				float num = 1f / (float)podiums.Count;
				for (int i = 0; i < podiums.Count; i++)
				{
					zero += podiums[i].position;
					zero2 += podiums[i].forward;
				}
				zero *= num;
				zero2 *= num;
				zero2.Normalize();
				p_target.position = zero;
				p_target.localRotation = Quaternion.LookRotation(zero2, Vector3.up);
				p_target.position += p_target.right * p_offset.x;
				p_target.position += p_target.up * p_offset.y;
				p_target.position += p_target.forward * p_offset.z;
				if (p_lookat_center)
				{
					p_target.LookAt(zero, Vector3.up);
				}
			}
		}

		public void RefreshStarts()
		{
			if (!root)
			{
				return;
			}
			podiums = new List<Transform>();
			Transform startsRoot = GetStartsRoot();
			if (!startsRoot)
			{
				Debug.LogWarning("TrackModel> Failed to find 'starts' container.");
				return;
			}
			int childCount = startsRoot.childCount;
			for (int i = 0; i < childCount; i++)
			{
				podiums.Add(startsRoot.GetChild(i));
			}
		}

		public void RefreshStarts(IList p_list)
		{
			podiums = new List<Transform>();
			for (int i = 0; i < p_list.Count; i++)
			{
				Component component = p_list[i] as Component;
				if ((bool)component)
				{
					Transform item = component.transform;
					if (component is MAPodium)
					{
						MAPodium obj = component as MAPodium;
						obj.name = obj.name + "-" + i;
						obj.gameObject.SetActive(value: false);
					}
					podiums.Add(item);
				}
			}
			if (podiums.Count <= 0)
			{
				Debug.LogWarning("TrackModel> No Podiums Found on this Map - Fallback to [" + data.title + "] podiums");
				RefreshStarts();
			}
		}

		public Transform GetStartsRoot()
		{
			return root.transform.Find("starts");
		}

		public void ClearStarts()
		{
			Transform startsRoot = GetStartsRoot();
			if ((bool)startsRoot)
			{
				for (int i = 0; i < startsRoot.childCount; i++)
				{
					Object.Destroy(startsRoot.GetChild(i).gameObject);
				}
			}
		}

		public void RefreshGates()
		{
			if (!root)
			{
				return;
			}
			List<Collider> list = new List<Collider>();
			Transform gatesRoot = GetGatesRoot();
			if (!gatesRoot)
			{
				Debug.LogWarning("TrackModel> Failed to find 'gates' container.");
				return;
			}
			int childCount = gatesRoot.childCount;
			for (int i = 0; i < childCount; i++)
			{
				list.Add(gatesRoot.GetChild(i).GetComponent<Collider>());
			}
			gates = list;
		}

		public void RefreshGates(IList p_list)
		{
			List<Collider> list = new List<Collider>();
			for (int i = 0; i < p_list.Count; i++)
			{
				Component component = p_list[i] as Component;
				if ((bool)component)
				{
					Collider collider = null;
					if (!(component is MAGate))
					{
						collider = ((!(component is Collider)) ? component.GetComponent<Collider>() : (component as Collider));
					}
					else
					{
						MAGate obj = component as MAGate;
						obj.name = obj.name + "-" + i;
						collider = obj.trigger;
					}
					if ((bool)collider)
					{
						list.Add(collider);
					}
				}
			}
			gates = list;
		}

		public void GenerateLaps(int p_count, Collider p_finish_gate = null, Collider p_lap_start = null, Collider p_lap_end = null)
		{
			if (p_count <= 1 || m_base_gates == null)
			{
				m_gates = ((m_base_gates == null) ? new List<Collider>() : new List<Collider>(m_base_gates));
				return;
			}
			List<Collider> base_gates = m_base_gates;
			if (m_gates == null)
			{
				m_gates = new List<Collider>();
			}
			if ((bool)p_finish_gate && base_gates.Contains(p_finish_gate))
			{
				base_gates.Remove(p_finish_gate);
			}
			List<Collider> list = new List<Collider>();
			List<Collider> list2 = new List<Collider>();
			int num = base_gates.IndexOf(p_lap_start);
			int num2 = base_gates.IndexOf(p_lap_end);
			if (num2 < 0)
			{
				num2 = 9999;
			}
			int num3 = Mathf.Min(num, num2);
			int num4 = Mathf.Max(num, num2);
			for (int i = 0; i < base_gates.Count; i++)
			{
				if (i < num3)
				{
					list.Add(base_gates[i]);
				}
				if (i > num4)
				{
					list2.Add(base_gates[i]);
				}
			}
			num = Mathf.Clamp(num, 0, base_gates.Count - 1);
			num2 = Mathf.Clamp(num2, 0, base_gates.Count - 1);
			m_gates.Clear();
			m_gates.AddRange(list);
			lapLoopStartIndex = Mathf.Min(num, num2);
			lapLoopEndIndex = lapLoopStartIndex - 1;
			lapLoopCount = 0;
			int num5 = ((num <= num2) ? 1 : (-1));
			bool flag = false;
			for (int j = 0; j < p_count; j++)
			{
				for (int k = num; (num5 < 0) ? (k >= num2) : (k <= num2); k += num5)
				{
					Collider collider = ((k < 0) ? null : ((k >= base_gates.Count) ? null : base_gates[k]));
					if (!collider)
					{
						flag = true;
						continue;
					}
					if (j > 0)
					{
						Transform parent = collider.transform.parent;
						Vector3 position = collider.transform.position;
						Quaternion rotation = collider.transform.rotation;
						Vector3 localScale = collider.transform.localScale;
						collider = Object.Instantiate(collider);
						collider.name = collider.name.Replace("(Clone)", "$lap-gate-" + j);
						collider.transform.SetParent(parent, worldPositionStays: true);
						collider.transform.position = position;
						collider.transform.rotation = rotation;
						collider.transform.localScale = localScale;
					}
					m_gates.Add(collider);
					if (j <= 0)
					{
						lapLoopCount++;
					}
					lapLoopEndIndex++;
				}
			}
			m_gates.AddRange(list2);
			if ((bool)p_finish_gate)
			{
				m_gates.Add(p_finish_gate);
			}
			if (flag)
			{
				Debug.LogWarning("TrackModel> GenerateLaps / Map had indexing issues during lap generation");
			}
		}

		public Transform GetGatesRoot()
		{
			return root.transform.Find("gates");
		}

		public void ClearGates()
		{
			Transform gatesRoot = GetGatesRoot();
			if ((bool)gatesRoot)
			{
				for (int i = 0; i < gatesRoot.childCount; i++)
				{
					Object.Destroy(gatesRoot.GetChild(i).gameObject);
				}
			}
		}

		public Transform GetClosestFinishAnchor(Vector3 p_position)
		{
			return GetClosestAnchor("finish", p_position);
		}

		public Transform GetClosestAnchor(string p_id, Vector3 p_position)
		{
			List<Transform> list = GetAnchors(p_id);
			Transform transform = ((list.Count <= 0) ? null : list[0]);
			float num = (transform ? Vector3.Distance(p_position, transform.position) : 0f);
			for (int i = 0; i < list.Count; i++)
			{
				Transform transform2 = list[i];
				float num2 = Vector3.Distance(transform2.position, p_position);
				if (num2 < num)
				{
					transform = transform2;
					num = num2;
				}
			}
			return transform;
		}

		public List<Transform> GetAnchors(string p_id)
		{
			List<Transform> list = new List<Transform>();
			Transform transform = (anchors ? anchors.Find(p_id) : null);
			if (!transform)
			{
				return list;
			}
			for (int i = 0; i < transform.childCount; i++)
			{
				list.Add(transform.GetChild(i));
			}
			return list;
		}

		public bool PlayTrackAnimation(DroneCamera p_target)
		{
			if (!p_target)
			{
				return false;
			}
			Camera camera = null;
			Transform transform = null;
			if ((bool)trackAnimation)
			{
				transform = trackAnimation.transform.Find("camera");
				camera = (transform ? Hierarchy.Find<Camera>(transform) : null);
				trackAnimation.gameObject.SetActive(value: true);
			}
			if (!transform && !camera)
			{
				return false;
			}
			if ((bool)camera)
			{
				p_target.SetFollow(camera, -0.01f);
			}
			else if ((bool)transform)
			{
				p_target.SetFollow(transform, -0.01f);
			}
			return true;
		}

		public void StopTrackAnimation()
		{
			if ((bool)trackAnimation)
			{
				trackAnimation.StopPlayback();
				trackAnimation.gameObject.SetActive(value: false);
			}
		}

		public void RefreshPath()
		{
			SplineComponent p_spline = pathSpline;
			pathTrace.Initialize(p_spline, gates, laps);
		}

		public void RefreshNavMeshes()
		{
			List<NavMeshSurface> list = new List<NavMeshSurface>(settings.navmeshes);
			Debug.Log("TrackModel> Found [" + list.Count + "] NavMeshSurfaces");
			for (int i = 0; i < list.Count; i++)
			{
				list[i].transform.parent.gameObject.SetActive(value: true);
			}
		}

		public bool GetClosestNavMeshPoint(Vector3 p_target, out Vector3 o_position)
		{
			float[] array = new float[5] { 5f, 10f, 50f, 200f, 1000f };
			for (int i = 0; i < array.Length; i++)
			{
				NavMesh.SamplePosition(p_target, out var hit, array[i], -1);
				if (hit.hit)
				{
					o_position = hit.position;
					return true;
				}
			}
			o_position = Vector3.zero;
			return false;
		}

		public void SetTrackEnabled(string p_id, bool p_flag)
		{
			int childCount = tracks.childCount;
			GameObject gameObject = null;
			GameObject gameObject2 = null;
			string text = (string.IsNullOrEmpty(p_id) ? "freestyle" : p_id);
			for (int i = 0; i < childCount; i++)
			{
				Transform child = tracks.GetChild(i);
				if (child.name == "freefly" || child.name == "freestyle")
				{
					gameObject2 = child.gameObject;
				}
				if (child.name == text || (child.name == "freefly" && text == "freestyle"))
				{
					child.gameObject.SetActive(p_flag);
				}
				else
				{
					child.gameObject.SetActive(value: false);
				}
				if (child.gameObject.activeInHierarchy)
				{
					gameObject = child.gameObject;
				}
			}
			if ((bool)gameObject2 && !gameObject)
			{
				gameObject = gameObject2;
				gameObject.SetActive(p_flag);
			}
			m_root = gameObject;
			settings = (root ? root.GetComponent<TrackSettings>() : null);
		}

		protected virtual void OnRootLoaded()
		{
			renderingProbeGroups = Hierarchy.FindAll<RenderingProbeGroup>(root.transform);
			renderingProbeGroups.RemoveAll((RenderingProbeGroup it) => !it.gameObject.activeInHierarchy);
		}
	}
}
