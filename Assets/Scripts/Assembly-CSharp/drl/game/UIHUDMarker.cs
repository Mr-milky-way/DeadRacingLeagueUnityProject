using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIHUDMarker : MarkerComponent
	{
		public Graphic graphicsField;

		public Graphic innerGraphicsField;

		public Vector2 size = new Vector2(40f, 40f);

		public Color color = Color.green;

		public bool visible = true;

		public AnimationCurve alphaOverDistance;

		public AnimationCurve scaleOverDistance;

		public Sprite borderTexture;

		public Sprite centerTexture;

		public bool resizeMarkerOutsideBounds = true;

		public Vector3 targetForward;

		public bool bidirectional = true;

		public bool reverse;

		public float alpha
		{
			get
			{
				return color.a;
			}
			set
			{
				if (value != color.a)
				{
					color.a = value;
				}
			}
		}

		public override void UpdateMarker()
		{
			if (visible || alpha > 0f)
			{
				base.UpdateMarker();
			}
		}

		protected override void OnMarkUpdate()
		{
			Color color = this.color;
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
			Camera camera = (base.camera ? base.camera : cmain);
			Vector3 vector = targetPosition;
			Vector3 rhs = targetForward;
			Vector3 position = camera.transform.position;
			bool flag = Vector3.Dot(position - vector, rhs) < 0f;
			if (reverse)
			{
				flag = !flag;
			}
			Vector2 b = size;
			Vector3 b2 = new Vector3(0f, 0f, 0f);
			b2.z = (flag ? 0f : 135f);
			float num = ((inBounds || !resizeMarkerOutsideBounds) ? 1f : 2f);
			float num2 = Vector3.Dot(vector - position, camera.transform.forward);
			float time = Vector3.Distance(position, vector);
			color.a = (visible ? color.a : 0f);
			color.a = alphaOverDistance.Evaluate(time) * color.a;
			if (num2 <= 0f && color.a > 0.1f)
			{
				color = new Color(1f, 0f, 0f, Mathf.Clamp01(Mathf.Abs(Mathf.Sin(3f * Time.time))) + 0.2f);
			}
			b *= scaleOverDistance.Evaluate(time) * num;
			bool flag2 = visible && color.a > 0f;
			graphicsField.enabled = flag2;
			if (!bidirectional && (bool)innerGraphicsField)
			{
				float a = ((flag && flag2) ? color.a : 0f);
				innerGraphicsField.color = Color.Lerp(innerGraphicsField.color, new Color(1f, 1f, 1f, a), Time.deltaTime * 8f);
				innerGraphicsField.enabled = graphicsField.enabled;
				base.rt.localEulerAngles = Vector3.Lerp(base.rt.localEulerAngles, b2, Time.deltaTime * 8f);
				color = (flag ? new Color(1f, 0f, 0f) : color);
			}
			if ((bool)innerGraphicsField)
			{
				innerGraphicsField.enabled = !bidirectional && flag2;
			}
			graphicsField.color = Color.Lerp(graphicsField.color, color, Time.deltaTime * 8f);
			base.rt.sizeDelta = Vector2.Lerp(base.rt.sizeDelta, b, Time.deltaTime * 8f);
			if ((bool)borderTexture && (bool)centerTexture && (bool)graphicsField)
			{
				Image image = ((graphicsField is Image) ? ((Image)graphicsField) : null);
				if ((bool)image)
				{
					image.sprite = (inBounds ? centerTexture : borderTexture);
				}
			}
		}
	}
}
