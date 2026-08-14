using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class LayerSwitch : MonoBehaviour
	{
		[SerializeField]
		private LayerMask m_layer;

		public int layerFlag;

		public List<Collider> colliders;

		public List<GameObject> targets;

		private Dictionary<GameObject, int> m_cache;

		public LayerMask layer
		{
			get
			{
				return m_layer;
			}
			set
			{
				m_layer = value;
				int num = 0;
				int num2 = layer.value;
				for (int i = 8; i < 32; i++)
				{
					num2 >>= 1;
					if (num2 == 0)
					{
						break;
					}
					num++;
				}
				layerFlag = num;
			}
		}

		public Dictionary<GameObject, int> cache
		{
			get
			{
				if (m_cache != null)
				{
					return m_cache;
				}
				return m_cache = new Dictionary<GameObject, int>();
			}
		}

		protected void Awake()
		{
			layer = m_layer;
		}

		protected void OnTriggerEnter(Collider p_collider)
		{
			if (!colliders.Contains(p_collider))
			{
				return;
			}
			for (int i = 0; i < targets.Count; i++)
			{
				GameObject gameObject = targets[i];
				if ((bool)gameObject)
				{
					cache[gameObject] = gameObject.layer;
					gameObject.layer = layerFlag;
				}
			}
		}

		protected void OnTriggerExit(Collider p_collider)
		{
			if (!colliders.Contains(p_collider))
			{
				return;
			}
			for (int i = 0; i < targets.Count; i++)
			{
				GameObject gameObject = targets[i];
				if ((bool)gameObject && cache.ContainsKey(gameObject))
				{
					gameObject.layer = cache[gameObject];
				}
			}
		}
	}
}
