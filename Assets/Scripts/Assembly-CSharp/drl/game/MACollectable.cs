using System.Collections.Generic;
using UnityEngine;

namespace drl.game
{
	public class MACollectable : MARenderer
	{
		public Collider collider;

		public MACollectableStyle styleInstance;

		public List<MACollectableStyle> styleList;

		private Vector3 m_collider_center;

		private bool m_has_collider_center;

		[SerializeField]
		private int m_index = -1;

		[SerializeField]
		private int m_size = 4;

		public float sizeUnit = 0.33f;

		public int colliderLayer = 28;

		[SerializeField]
		private MapCollectableMode m_collectable_mode = MapCollectableMode.Regular;

		[SerializeField]
		private int m_score = 1;

		[SerializeField]
		private string m_group = "";

		[SerializeField]
		private int m_group_bonus = 1;

		[SerializeField]
		private MapCollectableGroupMode m_group_mode;

		[SerializeField]
		private int m_collectable_style;

		public Vector3 colliderCenter
		{
			get
			{
				if (base.destroyed)
				{
					return Vector3.zero;
				}
				if (!collider)
				{
					return base.transform.position;
				}
				if (!m_has_collider_center)
				{
					RefreshColliderCenter();
					m_has_collider_center = true;
				}
				return collider.transform.TransformPoint(m_collider_center);
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

		public int size
		{
			get
			{
				return m_size;
			}
			set
			{
				m_size = value;
				RefreshSize();
				Write();
			}
		}

		public Vector3 sizeScale
		{
			get
			{
				return Vector3.one * sizeUnit * m_size;
			}
			set
			{
				int value2 = Mathf.RoundToInt(value.x / sizeUnit);
				size = Mathf.Clamp(value2, 1, 8);
			}
		}

		public MapCollectableMode collectableMode
		{
			get
			{
				return m_collectable_mode;
			}
			set
			{
				m_collectable_mode = value;
				Write();
				DelayRefresh();
			}
		}

		public int score
		{
			get
			{
				return m_score;
			}
			set
			{
				m_score = value;
				Write();
			}
		}

		public string group
		{
			get
			{
				return m_group;
			}
			set
			{
				m_group = value;
				Write();
			}
		}

		public int groupBonus
		{
			get
			{
				return m_group_bonus;
			}
			set
			{
				m_group_bonus = value;
				Write();
			}
		}

		public MapCollectableGroupMode groupMode
		{
			get
			{
				return m_group_mode;
			}
			set
			{
				m_group_mode = value;
				Write();
			}
		}

		public int collectableStyle
		{
			get
			{
				return m_collectable_style;
			}
			set
			{
				m_collectable_style = value;
				Write();
				DelayRefresh();
			}
		}

		public new MDCollectable data
		{
			get
			{
				return base.data as MDCollectable;
			}
			set
			{
				base.data = value;
			}
		}

		internal void RefreshColliderCenter()
		{
			Vector3 vector = m_collider_center;
			if (collider is BoxCollider)
			{
				vector = (collider as BoxCollider).center;
			}
			if (collider is SphereCollider)
			{
				vector = (collider as SphereCollider).center;
			}
			if (collider is CapsuleCollider)
			{
				vector = (collider as CapsuleCollider).center;
			}
			if (collider is MeshCollider)
			{
				MeshCollider meshCollider = collider as MeshCollider;
				vector = (meshCollider.sharedMesh ? meshCollider.sharedMesh.bounds.center : vector);
			}
			m_collider_center = vector;
		}

		public void RefreshSize()
		{
			base.transform.localScale = Vector3.one * sizeUnit * m_size;
		}

		protected void Start()
		{
			if (Application.isPlaying)
			{
				AssertCollectableMode();
			}
		}

		private void AssertCollectableMode()
		{
			MapCollectableMode[] array = new MapCollectableMode[2]
			{
				MapCollectableMode.Regular,
				MapCollectableMode.Kill
			};
			if (GetStyleCount(m_collectable_mode) > 0)
			{
				return;
			}
			foreach (MapCollectableMode mapCollectableMode in array)
			{
				if (GetStyleCount(mapCollectableMode) > 0)
				{
					m_collectable_mode = mapCollectableMode;
					break;
				}
			}
		}

		public override void Write()
		{
			base.Write();
			MDCollectable mDCollectable = data;
			if (mDCollectable != null)
			{
				mDCollectable.index = index;
				mDCollectable.size = size;
				mDCollectable.mode = collectableMode;
				mDCollectable.score = score;
				mDCollectable.group = group;
				mDCollectable.groupBonus = groupBonus;
				mDCollectable.groupMode = groupMode;
				mDCollectable.style = collectableStyle;
			}
		}

		public override void Read()
		{
			if (m_data is MDCollectable mDCollectable)
			{
				m_index = mDCollectable.index;
				m_size = mDCollectable.size;
				m_collectable_mode = mDCollectable.mode;
				m_score = mDCollectable.score;
				m_group = mDCollectable.group;
				m_group_bonus = mDCollectable.groupBonus;
				m_group_mode = mDCollectable.groupMode;
				m_collectable_style = mDCollectable.style;
			}
			base.Read();
			RefreshSize();
		}

		protected override MDObject NewData()
		{
			return new MDCollectable();
		}

		protected override void OnRefresh()
		{
			MACollectableStyle style = GetStyle(collectableStyle);
			if (!(style == null))
			{
				if ((bool)styleInstance)
				{
					Object.Destroy(styleInstance.gameObject);
				}
				styleInstance = Object.Instantiate(style);
				styleInstance.name = "asset";
				styleInstance.transform.parent = base.transform;
				styleInstance.transform.localPosition = Vector3.zero;
				styleInstance.transform.localRotation = Quaternion.identity;
				styleInstance.transform.localScale = Vector3.one;
			}
			if ((bool)styleInstance)
			{
				collider = ((styleInstance.hits.Count <= 0) ? null : styleInstance.hits[0]);
				if ((bool)collider)
				{
					collider.gameObject.layer = colliderLayer;
				}
				RefreshColliderCenter();
				base.hits.Clear();
				base.hits.AddRange(styleInstance.hits);
				base.renderers.Clear();
				base.renderers.AddRange(styleInstance.renderers);
			}
			base.OnRefresh();
		}

		public MACollectableStyle GetStyle(MapCollectableMode p_mode, int p_index)
		{
			int num = 0;
			MACollectableStyle result = null;
			for (int i = 0; i < styleList.Count; i++)
			{
				MACollectableStyle mACollectableStyle = styleList[i];
				if (mACollectableStyle.mode == p_mode)
				{
					if (p_index == num)
					{
						result = mACollectableStyle;
						break;
					}
					num++;
				}
			}
			return result;
		}

		public MACollectableStyle GetStyle(int p_index)
		{
			return GetStyle(collectableMode, p_index);
		}

		public int GetStyleCount(MapCollectableMode p_mode)
		{
			int num = 0;
			for (int i = 0; i < styleList.Count; i++)
			{
				if (styleList[i].mode == p_mode)
				{
					num++;
				}
			}
			return num;
		}

		public int GetStyleCount()
		{
			return GetStyleCount(collectableMode);
		}

		public void SetCollision()
		{
			colliderLayer = 12;
			collider.gameObject.layer = 12;
		}
	}
}
