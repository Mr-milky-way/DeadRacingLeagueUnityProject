using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class MAEntity : MapAsset
	{
		[SerializeField]
		private List<Collider> m_hits;

		[SerializeField]
		private MDEntityAttribFlag m_attribs;

		private MAEntity m_parent;

		private MAEntity m_root;

		public List<Collider> hits
		{
			get
			{
				if (m_hits != null)
				{
					return m_hits;
				}
				return m_hits = new List<Collider>();
			}
			set
			{
				m_hits = ((value == null) ? new List<Collider>() : new List<Collider>(value));
			}
		}

		public MDEntityAttribFlag attribs
		{
			get
			{
				return m_attribs;
			}
			set
			{
				m_attribs = value;
				Write();
			}
		}

		public MAEntity parent
		{
			get
			{
				if (!m_parent)
				{
					if (!base.transform.parent)
					{
						return null;
					}
					return m_parent = base.transform.parent.GetComponent<MAEntity>();
				}
				return m_parent;
			}
			set
			{
				base.transform.SetParent(value ? value.transform : null, worldPositionStays: true);
				m_parent = (value ? m_parent.GetComponent<MAEntity>() : null);
				m_root = null;
				Write();
			}
		}

		public MAEntity root
		{
			get
			{
				if ((bool)m_root)
				{
					return m_root;
				}
				MAEntity mAEntity = this;
				while ((bool)mAEntity)
				{
					MAEntity mAEntity2 = mAEntity.parent;
					if (!mAEntity2)
					{
						return m_root = mAEntity;
					}
					mAEntity = mAEntity2;
				}
				return null;
			}
		}

		public Vector3 localPosition
		{
			get
			{
				return base.transform.localPosition;
			}
			set
			{
				base.transform.localPosition = value;
				Write();
			}
		}

		public Quaternion localRotation
		{
			get
			{
				return base.transform.localRotation;
			}
			set
			{
				base.transform.localRotation = value;
				Write();
			}
		}

		public Vector3 localEuler
		{
			get
			{
				return base.transform.localEulerAngles;
			}
			set
			{
				base.transform.localEulerAngles = value;
				Write();
			}
		}

		public Vector3 localScale
		{
			get
			{
				return base.transform.localScale;
			}
			set
			{
				base.transform.localScale = value;
				Write();
			}
		}

		public new MDEntity data
		{
			get
			{
				return base.data as MDEntity;
			}
			set
			{
				base.data = value;
			}
		}

		public override void Write()
		{
			base.Write();
			MDEntity mDEntity = data;
			mDEntity.attribs = m_attribs;
			mDEntity.parentId = (parent ? parent.id : "");
			mDEntity.localPosition = base.transform.localPosition;
			mDEntity.localRotation = base.transform.localRotation;
			mDEntity.localScale = base.transform.localScale;
		}

		public override void Read()
		{
			base.Read();
			if (!(m_data is MDEntity mDEntity))
			{
				return;
			}
			MAEntity mAEntity = root;
			if ((bool)mAEntity)
			{
				MAEntity mAEntity2 = FindParentById(mDEntity.parentId);
				if ((bool)mAEntity2)
				{
					base.transform.SetParent(mAEntity2.transform, worldPositionStays: true);
					m_root = mAEntity;
				}
			}
			m_attribs = mDEntity.attribs;
			base.transform.localPosition = AssertTransformField(mDEntity.localPosition, Vector3.up * 5f, p_scale: false);
			base.transform.localRotation = mDEntity.localRotation;
			base.transform.localScale = AssertTransformField(mDEntity.localScale, Vector3.one, p_scale: true);
		}

		protected float AssertTransformField(float v, float d, bool p_scale)
		{
			if (float.IsNaN(v))
			{
				return d;
			}
			if (p_scale && Mathf.Abs(v) <= 0.001f)
			{
				v = ((v < 0f) ? (-0.001f) : 0.001f);
			}
			return v;
		}

		protected Vector3 AssertTransformField(Vector3 v, Vector3 d, bool p_scale)
		{
			Vector3 result = d;
			result.x = AssertTransformField(v.x, d.x, p_scale);
			result.y = AssertTransformField(v.y, d.y, p_scale);
			result.z = AssertTransformField(v.z, d.z, p_scale);
			return result;
		}

		protected MAEntity FindParentById(string p_id)
		{
			MAEntity mAEntity = parent;
			bool found = false;
			if (!mAEntity)
			{
				return null;
			}
			if (mAEntity.data == null)
			{
				return null;
			}
			if (mAEntity.data.id == p_id)
			{
				return null;
			}
			MAEntity mAEntity2 = root;
			if (!mAEntity2)
			{
				return null;
			}
			MAEntity res = null;
			Hierarchy.Traverse(mAEntity2.transform, delegate(MAEntity it)
			{
				if (found)
				{
					return false;
				}
				if (it.id != p_id)
				{
					return true;
				}
				found = true;
				res = it;
				return false;
			});
			return res;
		}

		protected override MDObject NewData()
		{
			return new MDEntity();
		}

		public void SetHitEnabled(bool p_flag)
		{
			for (int i = 0; i < hits.Count; i++)
			{
				Collider collider = hits[i];
				if ((bool)collider)
				{
					collider.enabled = p_flag;
				}
			}
		}

		public bool HasHit(Ray p_ray)
		{
			bool result = false;
			float maxDistance = 2000f;
			for (int i = 0; i < hits.Count; i++)
			{
				Collider collider = hits[i];
				if ((bool)collider && collider.Raycast(p_ray, out var _, maxDistance))
				{
					return true;
				}
			}
			return result;
		}

		public bool HasHit(Rect p_rect, Camera p_camera)
		{
			if (!this)
			{
				return false;
			}
			Vector3 vector = p_camera.WorldToViewportPoint(base.transform.position);
			if (vector.x < 0f)
			{
				return false;
			}
			if (vector.y < 0f)
			{
				return false;
			}
			if (vector.x > 1f)
			{
				return false;
			}
			if (vector.y > 1f)
			{
				return false;
			}
			if (vector.z <= 0f)
			{
				return false;
			}
			return GetBounds2D(p_camera).Overlaps(p_rect);
		}

		public Rect GetBounds2D(Camera p_camera)
		{
			for (int i = 0; i < hits.Count; i++)
			{
				if (!hits[i])
				{
					hits.RemoveAt(i--);
				}
			}
			if (hits.Count <= 0)
			{
				return default(Rect);
			}
			Bounds bounds = hits[0].bounds;
			for (int j = 0; j < hits.Count; j++)
			{
				Collider collider = hits[j];
				if ((bool)collider)
				{
					Bounds bounds2 = collider.bounds;
					if (bounds2.extents.sqrMagnitude > bounds.extents.sqrMagnitude)
					{
						bounds = bounds2;
					}
				}
			}
			Vector2 vector = default(Vector2);
			Vector2 vector2 = default(Vector2);
			float x = bounds.extents.x;
			float y = bounds.extents.y;
			float z = bounds.extents.z;
			Vector3[] array = new Vector3[8]
			{
				new Vector3(x, y, z),
				new Vector3(x, y, 0f - z),
				new Vector3(x, 0f - y, z),
				new Vector3(x, 0f - y, 0f - z),
				new Vector3(0f - x, y, z),
				new Vector3(0f - x, y, 0f - z),
				new Vector3(0f - x, 0f - y, z),
				new Vector3(0f - x, 0f - y, 0f - z)
			};
			Vector3 position = bounds.center + array[0];
			position = p_camera.WorldToScreenPoint(position);
			vector = (vector2 = position);
			for (int k = 1; k < array.Length; k++)
			{
				position = bounds.center + array[k];
				position = p_camera.WorldToScreenPoint(position);
				vector.x = Mathf.Min(vector.x, position.x);
				vector.y = Mathf.Min(vector.y, position.y);
				vector2.x = Mathf.Max(vector2.x, position.x);
				vector2.y = Mathf.Max(vector2.y, position.y);
			}
			return new Rect(vector, vector2 - vector);
		}

		public Bounds GetBounds()
		{
			Bounds result = default(Bounds);
			if (hits.Count <= 0)
			{
				result.center = base.transform.position;
				return result;
			}
			for (int i = 0; i < hits.Count; i++)
			{
				Collider collider = hits[i];
				if ((bool)collider)
				{
					Bounds bounds = collider.bounds;
					if (i <= 0)
					{
						result = bounds;
					}
					else
					{
						result.Encapsulate(bounds);
					}
				}
			}
			return result;
		}

		public List<MAGate> GetSortedGates()
		{
			List<MAGate> list = Hierarchy.FindAll<MAGate>(base.transform);
			list.RemoveAll(ClearDisabledGates);
			list.Sort(SortGates);
			return list;
		}

		public List<MACollectable> GetSortedCollectables()
		{
			List<MACollectable> list = Hierarchy.FindAll<MACollectable>(base.transform);
			list.Sort(SortCollectables);
			return list;
		}

		private int SortCollectables(MACollectable a, MACollectable b)
		{
			if (a.index >= b.index)
			{
				return 1;
			}
			return -1;
		}

		public MAGate GetFinishGate()
		{
			List<MAGate> sortedGates = GetSortedGates();
			for (int i = 0; i < sortedGates.Count; i++)
			{
				if (sortedGates[i].isFinish)
				{
					return sortedGates[i];
				}
			}
			return null;
		}

		public MAGate GetLapStartGate()
		{
			List<MAGate> sortedGates = GetSortedGates();
			MAGate finishGate = GetFinishGate();
			sortedGates.Remove(finishGate);
			if (sortedGates.Count <= 0)
			{
				return null;
			}
			for (int i = 0; i < sortedGates.Count; i++)
			{
				if (sortedGates[i].isLapStart)
				{
					return sortedGates[i];
				}
			}
			return sortedGates[0];
		}

		public MAGate GetLapEndGate()
		{
			List<MAGate> sortedGates = GetSortedGates();
			MAGate finishGate = GetFinishGate();
			sortedGates.Remove(finishGate);
			if (sortedGates.Count <= 0)
			{
				return null;
			}
			for (int i = 0; i < sortedGates.Count; i++)
			{
				if (sortedGates[i].isLapEnd)
				{
					return sortedGates[i];
				}
			}
			return sortedGates[sortedGates.Count - 1];
		}

		public List<MAEntity> GetInvalids()
		{
			List<MAEntity> res = new List<MAEntity>();
			Hierarchy.Traverse(base.transform, delegate(MAEntity it)
			{
				if (!it.valid)
				{
					res.Add(it);
				}
			});
			return res;
		}

		public void ClearInvalids()
		{
			List<MAEntity> invalids = GetInvalids();
			for (int i = 0; i < invalids.Count; i++)
			{
				invalids[i].transform.parent = null;
				UnityEngine.Object.Destroy(invalids[i].gameObject);
			}
		}

		private int SortGates(MAGate a, MAGate b)
		{
			if (a.index >= b.index)
			{
				return 1;
			}
			return -1;
		}

		private bool ClearDisabledGates(MAGate a)
		{
			return !a.isTrigger;
		}

		public List<MAPodium> GetSortedPodiums()
		{
			List<MAPodium> res = new List<MAPodium>();
			Hierarchy.Traverse(base.transform, delegate(MAEntity it)
			{
				if (it is MAPodium)
				{
					res.Add(it as MAPodium);
				}
				else if (it.data.category == MapAssetType.Podium)
				{
					MAPodium mAPodium = it.gameObject.AddComponent<MAPodium>();
					mAPodium.guid = it.data.guid;
					if (it.data.TryGetValue("podium-index", out var value))
					{
						try
						{
							mAPodium.index = Convert.ToInt32(value);
						}
						catch (Exception ex)
						{
							Debug.Log("[MAEntity] GetSortedPodiums | Exception handled " + ex.Message);
							mAPodium.index = res.Count;
						}
					}
					res.Add(mAPodium);
				}
			});
			res.Sort(SortPodiums);
			for (int num = 0; num < res.Count; num++)
			{
				res[num].index = num;
			}
			return res;
		}

		public List<MACameraTool> GetCameraTools()
		{
			List<MACameraTool> res = new List<MACameraTool>();
			Hierarchy.Traverse(base.transform, delegate(MAEntity it)
			{
				if (it is MACameraTool)
				{
					MACameraTool mACameraTool = it as MACameraTool;
					if (mACameraTool.HasControlPoints())
					{
						res.Add(mACameraTool);
					}
				}
			});
			res.Sort(SortCameraTools);
			for (int num = 0; num < res.Count; num++)
			{
				res[num].index = num;
			}
			return res;
		}

		public List<MASpline> GetCourseCameras()
		{
			List<MASpline> res = new List<MASpline>();
			Hierarchy.Traverse(base.transform, delegate(MAEntity it)
			{
				if (it is MASpline)
				{
					MASpline mASpline = it as MASpline;
					if (mASpline.splineCategory == SplineCategory.CourseCamera)
					{
						res.Add(mASpline);
					}
				}
			});
			res.Sort(SortCourseCameras);
			for (int num = 0; num < res.Count; num++)
			{
				res[num].splineCourseCameraIndex = num;
			}
			return res;
		}

		private int SortPodiums(MAPodium a, MAPodium b)
		{
			if (a.index >= b.index)
			{
				return 1;
			}
			return -1;
		}

		private int SortCameraTools(MACameraTool a, MACameraTool b)
		{
			if (a.index >= b.index)
			{
				return 1;
			}
			return -1;
		}

		private int SortCourseCameras(MASpline a, MASpline b)
		{
			if (a.splineCourseCameraIndex >= b.splineCourseCameraIndex)
			{
				return 1;
			}
			return -1;
		}

		public void SetHitsLayer(int p_layer)
		{
			for (int i = 0; i < hits.Count; i++)
			{
				if ((bool)hits[i])
				{
					hits[i].gameObject.layer = p_layer;
				}
			}
		}

		public virtual void OnEditorSelect()
		{
		}

		public virtual void OnEditorUnselect()
		{
		}
	}
}
