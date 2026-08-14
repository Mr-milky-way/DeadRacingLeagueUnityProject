using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class MAGate : MARenderer
	{
		public Collider trigger;

		private Vector3 m_trigger_local_center;

		private bool m_has_trigger_center;

		[SerializeField]
		private int m_index = -1;

		[SerializeField]
		private MapGateMode m_gate_mode = MapGateMode.Bidirectional;

		[SerializeField]
		private bool m_is_trigger = true;

		[SerializeField]
		private bool m_is_finish;

		[SerializeField]
		private bool m_is_lap_start;

		[SerializeField]
		private bool m_is_lap_end;

		[SerializeField]
		private bool m_is_respawn_visible = true;

		protected Material m_trigger_material;

		private bool m_trigger_renderer_enabled;

		public Vector3 triggerCenter
		{
			get
			{
				if (base.destroyed)
				{
					return Vector3.zero;
				}
				if (!trigger)
				{
					return base.transform.position;
				}
				if (!m_has_trigger_center)
				{
					RefreshTriggerCenter();
					m_has_trigger_center = true;
				}
				return trigger.transform.TransformPoint(m_trigger_local_center);
			}
		}

		public int index
		{
			get
			{
				return m_index;
			}
			set
			{
				m_index = value;
				Write();
			}
		}

		public MapGateMode gateMode
		{
			get
			{
				return m_gate_mode;
			}
			set
			{
				m_gate_mode = value;
				Write();
			}
		}

		public bool isTrigger
		{
			get
			{
				return m_is_trigger;
			}
			set
			{
				m_is_trigger = value;
				Write();
			}
		}

		public bool isFinish
		{
			get
			{
				return m_is_finish;
			}
			set
			{
				m_is_finish = value;
				Write();
			}
		}

		public bool isLapStart
		{
			get
			{
				return m_is_lap_start;
			}
			set
			{
				m_is_lap_start = value;
				Write();
			}
		}

		public bool isLapEnd
		{
			get
			{
				return m_is_lap_end;
			}
			set
			{
				m_is_lap_end = value;
				Write();
			}
		}

		public bool isRespawnVisible
		{
			get
			{
				return m_is_respawn_visible;
			}
			set
			{
				m_is_respawn_visible = value;
				Write();
			}
		}

		public new MDGate data
		{
			get
			{
				return base.data as MDGate;
			}
			set
			{
				base.data = value;
			}
		}

		internal void RefreshTriggerCenter()
		{
			Vector3 vector = m_trigger_local_center;
			if (trigger is BoxCollider)
			{
				vector = (trigger as BoxCollider).center;
			}
			if (trigger is SphereCollider)
			{
				vector = (trigger as SphereCollider).center;
			}
			if (trigger is CapsuleCollider)
			{
				vector = (trigger as CapsuleCollider).center;
			}
			if (trigger is MeshCollider)
			{
				MeshCollider meshCollider = trigger as MeshCollider;
				vector = (meshCollider.sharedMesh ? meshCollider.sharedMesh.bounds.center : vector);
			}
			m_trigger_local_center = vector;
		}

		private void AssertTriggerRenderer()
		{
			if ((bool)this && !name.Contains("-empty"))
			{
				renderers?.RemoveAll((Renderer it) => !it || it.name.Contains("-trigger"));
			}
		}

		protected void Start()
		{
			AssertTriggerRenderer();
		}

		public override void Write()
		{
			base.Write();
			MDGate mDGate = data;
			if (mDGate != null)
			{
				mDGate.index = index;
				mDGate.gateMode = gateMode;
				mDGate.isTrigger = isTrigger;
				mDGate.isFinish = isFinish;
				mDGate.isLapStart = isLapStart;
				mDGate.isLapEnd = isLapEnd;
				mDGate.isRespawnVisible = isRespawnVisible;
				RefreshTriggerRenderer();
			}
		}

		public override void Read()
		{
			base.Read();
			if (m_data is MDGate mDGate)
			{
				m_index = mDGate.index;
				m_gate_mode = mDGate.gateMode;
				m_is_trigger = mDGate.isTrigger;
				m_is_finish = mDGate.isFinish;
				m_is_lap_start = mDGate.isLapStart;
				m_is_lap_end = mDGate.isLapEnd;
				m_is_respawn_visible = mDGate.isRespawnVisible;
				RefreshTriggerRenderer();
			}
		}

		protected override MDObject NewData()
		{
			return new MDGate();
		}

		public void AssertTrigger(Transform p_target)
		{
			List<Collider> list = Hierarchy.FindAll<Collider>(p_target);
			for (int i = 0; i < list.Count; i++)
			{
				Collider collider = list[i];
				if (collider.name.Contains("-trigger") && collider != trigger)
				{
					trigger = collider;
					Refresh();
					RefreshTriggerCenter();
					break;
				}
			}
		}

		public MAGuide AssertRespawnGuide(MAGuide p_template)
		{
			if (!p_template)
			{
				return null;
			}
			MAGuide mAGuide = GetRespawnGuide();
			if (!mAGuide)
			{
				mAGuide = Object.Instantiate(p_template, base.transform);
				mAGuide.name = "$respawn-guide";
				mAGuide.transform.localRotation = Quaternion.identity;
				mAGuide.transform.localPosition = new Vector3(0f, 0f, 2f);
				data.RefreshParenting();
			}
			mAGuide.gameObject.SetActive(isRespawnVisible);
			return mAGuide;
		}

		public MAGuide GetRespawnGuide()
		{
			Transform transform = base.transform.Find("$respawn-guide");
			if (!transform)
			{
				return null;
			}
			return transform.GetComponent<MAGuide>();
		}

		public void SetTriggerRendererEnabled(bool p_flag)
		{
			m_trigger_renderer_enabled = p_flag;
			if ((bool)trigger)
			{
				List<MeshRenderer> list = Hierarchy.FindAll<MeshRenderer>(trigger.transform.parent);
				for (int i = 0; i < list.Count; i++)
				{
					list[i].enabled = p_flag;
				}
			}
		}

		public void SetTriggersLayers(int p_layer)
		{
			if ((bool)trigger)
			{
				List<Collider> list = Hierarchy.FindAll<Collider>(trigger.transform.parent);
				for (int i = 0; i < list.Count; i++)
				{
					list[i].gameObject.layer = p_layer;
				}
			}
		}

		protected override void OnRefresh()
		{
			base.OnRefresh();
			AssertTriggerMaterial();
		}

		protected void AssertTriggerMaterial()
		{
			List<Material> renderer_materials = m_renderer_materials;
			for (int i = 0; i < renderer_materials.Count; i++)
			{
				Material material = renderer_materials[i];
				if ((bool)material && material.name.Contains("trigger-grid"))
				{
					m_trigger_material = material;
					RefreshTriggerRenderer();
					break;
				}
			}
		}

		protected void RefreshTriggerRenderer()
		{
			Material trigger_material = m_trigger_material;
			if ((bool)trigger_material)
			{
				float value = 0f;
				switch (gateMode)
				{
				case MapGateMode.None:
				case MapGateMode.Bidirectional:
					value = 0f;
					break;
				case MapGateMode.BackToFront:
					value = -1f;
					break;
				case MapGateMode.FrontToBack:
					value = 1f;
					break;
				}
				trigger_material.SetFloat("_ViewZ", value);
				SetTriggerRendererEnabled(data.isTrigger && m_trigger_renderer_enabled);
			}
		}
	}
}
