using UnityEngine;

namespace thelab.mvc
{
	public class RaycastView<T> : RaycastView where T : BaseApplication
	{
		public new T app => (T)base.app;
	}
	public class RaycastView : NotificationView
	{
		public bool down;

		public bool over;

		public float hold;

		public Collider[] colliders;

		public Camera view;

		protected Camera cmain;

		private void Awake()
		{
			hold = 0f;
			down = false;
			over = false;
			colliders = GetComponentsInChildren<Collider>();
		}

		private void Update()
		{
			if (!cmain)
			{
				cmain = Camera.main;
			}
			Camera camera = (view ? view : cmain);
			bool flag = false;
			if ((bool)camera)
			{
				Ray ray = camera.ScreenPointToRay(Input.mousePosition);
				for (int i = 0; i < colliders.Length; i++)
				{
					if (colliders[i].Raycast(ray, out var _, 1000f))
					{
						flag = true;
						break;
					}
				}
			}
			if (over)
			{
				if (!flag)
				{
					Notify(notification + "@out");
				}
			}
			else if (flag)
			{
				Notify(notification + "@over");
			}
			over = flag;
			bool flag2 = over && (Input.GetKey(KeyCode.Mouse0) || Input.touchCount == 1);
			if (down)
			{
				if (!flag2)
				{
					Notify(notification + "@up");
					if (flag)
					{
						Notify(notification + "@click");
					}
					hold = 0f;
				}
			}
			else if (flag2)
			{
				Notify(notification + "@down");
				hold = 0f;
			}
			down = flag2;
			if (down)
			{
				Notify(notification + "@hold");
				hold += Time.unscaledDeltaTime;
			}
		}
	}
}
