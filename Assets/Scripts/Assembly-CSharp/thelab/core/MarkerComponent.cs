using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	public class MarkerComponent : MonoBehaviour
	{
		public enum Mode
		{
			Invalid = 0,
			Transform = 1,
			Renderer = 2,
			Collider = 3
		}

		public Mode mode;

		[SerializeField]
		private CanvasScaler m_scaler;

		[SerializeField]
		private Canvas m_canvas;

		[SerializeField]
		private Component m_target;

		public Camera camera;

		public bool isDynamic;

		public float speed;

		private RectTransform m_rt;

		private RectTransform m_parent_rt;

		protected bool m_hasPosition;

		public Vector3 targetPosition;

		public Vector2 screenPosition;

		public Vector2 screenOffset;

		public bool inBounds;

		public bool selfUpdate = true;

		public RectOffset margins;

		public bool allowNegativeMargins;

		protected Camera cmain;

		private Vector3[] _corners;

		public CanvasScaler scaler
		{
			get
			{
				if (!m_scaler)
				{
					return m_scaler = Hierarchy.FindReverse<CanvasScaler>(base.transform);
				}
				return m_scaler;
			}
		}

		public Canvas canvas
		{
			get
			{
				if (!m_canvas)
				{
					return m_canvas = Hierarchy.FindReverse<Canvas>(base.transform);
				}
				return m_canvas;
			}
		}

		public Component target
		{
			get
			{
				return Reflection<object>.Assert(ref m_target);
			}
			set
			{
				m_target = value;
				m_hasPosition = false;
				AssertMode();
			}
		}

		protected RectTransform rt => m_rt ?? Reflection<object>.Assert(ref m_rt, base.gameObject);

		internal RectTransform parentRT => m_parent_rt ?? Reflection<object>.Assert(ref m_parent_rt, base.transform.parent.gameObject);

		private Vector3[] m_corners
		{
			get
			{
				if (_corners != null)
				{
					return _corners;
				}
				return _corners = new Vector3[4];
			}
		}

		protected void Awake()
		{
			AssertMode();
		}

		public virtual void UpdateMarker()
		{
			if (!base.enabled)
			{
				return;
			}
			if (mode == Mode.Invalid)
			{
				AssertMode();
			}
			if (!target)
			{
				mode = Mode.Invalid;
				return;
			}
			RectTransform rectTransform = parentRT;
			if (!rectTransform)
			{
				return;
			}
			if (!cmain)
			{
				cmain = Camera.main;
				Transform transform = (cmain ? cmain.transform.parent : null);
				if ((bool)transform)
				{
					transform = transform.Find("main");
					if ((bool)transform)
					{
						Camera component = transform.GetComponent<Camera>();
						if ((bool)component)
						{
							cmain = component;
						}
					}
				}
			}
			Camera camera = (this.camera ? this.camera : cmain);
			if (!camera)
			{
				return;
			}
			RefreshPosition();
			float num = margins.left;
			float num2 = margins.right;
			float num3 = margins.top;
			float num4 = margins.bottom;
			Vector3 vector = targetPosition;
			Vector3 position = camera.transform.position;
			float num5 = Vector3.Dot(vector - position, camera.transform.forward);
			Vector2 vector2 = camera.WorldToViewportPoint(vector);
			if (num5 <= 0f)
			{
				Vector2 vector3 = vector2 - new Vector2(0.5f, 0.5f);
				vector3.Normalize();
				vector2 += vector3 * 2f;
			}
			if (num5 <= 0f)
			{
				vector2.x = 1f - vector2.x;
				vector2.y = 1f - vector2.y;
			}
			Vector2 scale = new Vector2(Screen.width, Screen.height);
			Vector2 screenPoint = vector2;
			screenPoint.Scale(scale);
			inBounds = true;
			if (screenPoint.x <= 0f)
			{
				inBounds = false;
			}
			else if (screenPoint.x >= scale.x)
			{
				inBounds = false;
			}
			else if (screenPoint.y <= 0f)
			{
				inBounds = false;
			}
			else if (screenPoint.y >= scale.y)
			{
				inBounds = false;
			}
			screenPosition = screenPoint;
			if ((bool)canvas)
			{
				camera = canvas.worldCamera;
				if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
				{
					camera = null;
				}
			}
			Vector2 localPoint = Vector2.zero;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, camera, out localPoint);
			Rect rect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);
			float min = num;
			float max = rect.width - num2;
			float max2 = 0f - num3;
			float min2 = 0f - (rect.height - num4);
			localPoint.x += screenOffset.x;
			localPoint.y += screenOffset.y;
			localPoint.x = Mathf.Clamp(localPoint.x, min, max);
			localPoint.y = Mathf.Clamp(localPoint.y, min2, max2);
			if (allowNegativeMargins)
			{
				RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, camera);
			}
			rt.anchoredPosition = ((speed <= 0f) ? localPoint : Vector2.Lerp(rt.anchoredPosition, localPoint, Time.deltaTime * speed));
			OnMarkUpdate();
		}

		protected virtual void LateUpdate()
		{
			if (selfUpdate)
			{
				UpdateMarker();
			}
		}

		protected virtual void OnMarkUpdate()
		{
		}

		protected void RefreshPosition()
		{
			Component component = m_target;
			if (!component || (m_hasPosition && !isDynamic))
			{
				return;
			}
			m_hasPosition = true;
			Vector3 vector = Vector3.zero;
			_ = Vector3.forward;
			switch (mode)
			{
			case Mode.Renderer:
			{
				Renderer obj = (Renderer)component;
				vector = obj.bounds.center;
				_ = obj.transform.forward;
				break;
			}
			case Mode.Collider:
			{
				Collider collider;
				if (component is ColliderEventComponent)
				{
					ColliderEventComponent colliderEventComponent = (ColliderEventComponent)component;
					if (colliderEventComponent.colliders.Count <= 0)
					{
						break;
					}
					collider = colliderEventComponent.colliders[0];
				}
				else
				{
					collider = (Collider)component;
				}
				if ((bool)collider)
				{
					_ = collider.transform.forward;
				}
				if (collider is BoxCollider)
				{
					vector = ((BoxCollider)collider).center;
				}
				else if (collider is SphereCollider)
				{
					vector = ((SphereCollider)collider).center;
				}
				else if (collider is CapsuleCollider)
				{
					vector = ((CapsuleCollider)collider).center;
				}
				if (collider is MeshCollider)
				{
					MeshCollider meshCollider = (MeshCollider)collider;
					vector = (meshCollider.sharedMesh ? meshCollider.sharedMesh.bounds.center : vector);
				}
				break;
			}
			}
			vector = component.transform.TransformPoint(vector);
			targetPosition = vector;
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawSphere(targetPosition, 2f);
		}

		protected void AssertMode()
		{
			if (!target)
			{
				mode = Mode.Invalid;
				return;
			}
			if (target is Renderer)
			{
				mode = Mode.Renderer;
				return;
			}
			if (target is Collider)
			{
				mode = Mode.Collider;
				return;
			}
			if (target is ColliderEventComponent)
			{
				mode = Mode.Collider;
				return;
			}
			mode = Mode.Transform;
			if (!(target is Transform))
			{
				target = target.transform;
			}
		}
	}
}
