using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using drl.sim.rci;
using thelab.core;

namespace drl.sim
{
	public class UIControllerOverlay : MonoBehaviour
	{
		[Serializable]
		public class UIControllerCameraAnimation
		{
			[Header("Camera")]
			public Transform camera;

			[Header("XBox")]
			public Transform leftStickFocusXbox;

			public Transform rightStickFocusXbox;

			[Header("PS")]
			public Transform leftStickFocusPS;

			public Transform rightStickFocusPS;

			[Header("RC")]
			public Transform leftStickFocusRC;

			public Transform rightStickFocusRC;

			[Header("Nikko")]
			public Transform leftStickFocusNK;

			public Transform rightStickFocusNK;

			public void Animate(bool p_left = true, float p_duration = 0.3f)
			{
				if (!(camera == null))
				{
					ControllerStateType controllerStateType = RCI.GetControllerStateType(ControllerStateType.Taranis);
					Transform transform = null;
					Tween.Kill(camera, "localPosition");
					switch (controllerStateType)
					{
					case ControllerStateType.Taranis:
						transform = (p_left ? leftStickFocusRC : rightStickFocusRC);
						break;
					case ControllerStateType.XBox:
						transform = (p_left ? leftStickFocusXbox : rightStickFocusXbox);
						break;
					case ControllerStateType.PS4:
						transform = (p_left ? leftStickFocusPS : rightStickFocusPS);
						break;
					case ControllerStateType.Nikko:
						transform = (p_left ? leftStickFocusNK : rightStickFocusNK);
						break;
					}
					if (!(transform == null))
					{
						Tween.Add(camera, "localPosition", transform.localPosition, p_duration, Cubic.InOut);
					}
				}
			}

			public void Reset(float p_duration = 0.3f)
			{
				if (!(camera == null))
				{
					Tween.Kill(camera, "localPosition");
					Tween.Add(camera, "localPosition", new Vector3(0f, 0f, 0f), p_duration, Cubic.InOut);
				}
			}
		}

		[SerializeField]
		private FadeComponent m_fade;

		public List<Transform> leftHorizontalList;

		public List<Transform> leftVerticalList;

		public List<Vector2> leftHorizontalRange;

		public List<Vector2> leftVerticalRange;

		public List<Transform> rightHorizontalList;

		public List<Transform> rightVerticalList;

		public List<Vector2> rightHorizontalRange;

		public List<Vector2> rightVerticalRange;

		public List<Renderer> leftLeftArrows;

		public List<Renderer> leftRightArrows;

		public List<Renderer> leftUpArrows;

		public List<Renderer> leftDownArrows;

		public List<Renderer> leftCWArrows;

		public List<Renderer> leftCCWArrows;

		public List<Renderer> leftToggleUpArrows;

		public List<Renderer> leftToggleDownArrows;

		public List<Renderer> rightLeftArrows;

		public List<Renderer> rightRightArrows;

		public List<Renderer> rightUpArrows;

		public List<Renderer> rightDownArrows;

		public List<Renderer> rightCWArrows;

		public List<Renderer> rightCCWArrows;

		public List<Renderer> rightToggleUpArrows;

		public List<Renderer> rightToggleDownArrows;

		[SerializeField]
		[HideInInspector]
		private float m_leftLeftArrowAlpha;

		[SerializeField]
		[HideInInspector]
		private float m_leftUpArrowAlpha;

		[SerializeField]
		[HideInInspector]
		private float m_leftRightArrowAlpha;

		[SerializeField]
		[HideInInspector]
		private float m_leftDownArrowAlpha;

		[SerializeField]
		[HideInInspector]
		private float m_leftCCWArrowAlpha;

		[SerializeField]
		[HideInInspector]
		private float m_leftCWArrowAlpha;

		[SerializeField]
		[HideInInspector]
		private float m_leftToggleUpArrowAlpha;

		[SerializeField]
		[HideInInspector]
		private float m_leftToggleDownArrowAlpha;

		[SerializeField]
		[HideInInspector]
		private float m_rightLeftArrowAlpha;

		[SerializeField]
		[HideInInspector]
		private float m_rightUpArrowAlpha;

		[SerializeField]
		[HideInInspector]
		private float m_rightRightArrowAlpha;

		[SerializeField]
		[HideInInspector]
		private float m_rightDownArrowAlpha;

		[SerializeField]
		[HideInInspector]
		private float m_rightCCWArrowAlpha;

		[SerializeField]
		[HideInInspector]
		private float m_rightCWArrowAlpha;

		[SerializeField]
		[HideInInspector]
		private float m_rightToggleUpArrowAlpha;

		[SerializeField]
		[HideInInspector]
		private float m_rightToggleDownArrowAlpha;

		public Transform leftToggleArrowHolder;

		public Transform rightToggleArrowHolder;

		public Transform leftToggle;

		public Transform rightToggle;

		public UIControllerCameraAnimation cameraAnimation;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_leftStick;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_rightStick;

		private Activity m_user_input_loop;

		private Activity m_left_stick_animation;

		private readonly Dictionary<RawAxis, bool> inverts = new Dictionary<RawAxis, bool>
		{
			{
				RawAxis.LeftStickX,
				false
			},
			{
				RawAxis.LeftStickY,
				false
			},
			{
				RawAxis.RightStickX,
				false
			},
			{
				RawAxis.RightStickY,
				false
			}
		};

		private readonly Dictionary<RawAxis, float> min = new Dictionary<RawAxis, float>
		{
			{
				RawAxis.LeftStickX,
				-1f
			},
			{
				RawAxis.LeftStickY,
				-1f
			},
			{
				RawAxis.RightStickX,
				-1f
			},
			{
				RawAxis.RightStickY,
				-1f
			}
		};

		private readonly Dictionary<RawAxis, float> max = new Dictionary<RawAxis, float>
		{
			{
				RawAxis.LeftStickX,
				1f
			},
			{
				RawAxis.LeftStickY,
				1f
			},
			{
				RawAxis.RightStickX,
				1f
			},
			{
				RawAxis.RightStickY,
				1f
			}
		};

		private readonly Dictionary<RawAxis, float> center = new Dictionary<RawAxis, float>
		{
			{
				RawAxis.LeftStickX,
				0f
			},
			{
				RawAxis.LeftStickY,
				0f
			},
			{
				RawAxis.RightStickX,
				0f
			},
			{
				RawAxis.RightStickY,
				0f
			}
		};

		private readonly Dictionary<RawAxis, float> deadzone = new Dictionary<RawAxis, float>
		{
			{
				RawAxis.LeftStickX,
				0f
			},
			{
				RawAxis.LeftStickY,
				0f
			},
			{
				RawAxis.RightStickX,
				0f
			},
			{
				RawAxis.RightStickY,
				0f
			}
		};

		private readonly Dictionary<RawAxis, int> channels = new Dictionary<RawAxis, int>
		{
			{
				RawAxis.LeftStickX,
				-1
			},
			{
				RawAxis.LeftStickY,
				-1
			},
			{
				RawAxis.RightStickX,
				-1
			},
			{
				RawAxis.RightStickY,
				-1
			},
			{
				RawAxis.ToggleA,
				-2
			},
			{
				RawAxis.ToggleB,
				-2
			}
		};

		private bool useChannels;

		private Activity m_right_stick_animation;

		private Activity m_toggle_animation;

		private ControllerStateType m_current_type = (ControllerStateType)(-1);

		public FadeComponent fade
		{
			get
			{
				if (!m_fade)
				{
					return m_fade = GetComponent<FadeComponent>();
				}
				return m_fade;
			}
		}

		public float controllerImgHorizontalSize { get; private set; }

		public float controllerImgHorizontalOffset { get; private set; }

		public float leftLeftArrowAlpha
		{
			get
			{
				return m_leftLeftArrowAlpha;
			}
			set
			{
				m_leftLeftArrowAlpha = value;
				RefreshAlpha();
			}
		}

		public float leftUpArrowAlpha
		{
			get
			{
				return m_leftUpArrowAlpha;
			}
			set
			{
				m_leftUpArrowAlpha = value;
				RefreshAlpha();
			}
		}

		public float leftRightArrowAlpha
		{
			get
			{
				return m_leftRightArrowAlpha;
			}
			set
			{
				m_leftRightArrowAlpha = value;
				RefreshAlpha();
			}
		}

		public float leftDownArrowAlpha
		{
			get
			{
				return m_leftDownArrowAlpha;
			}
			set
			{
				m_leftDownArrowAlpha = value;
				RefreshAlpha();
			}
		}

		public float leftCCWArrowAlpha
		{
			get
			{
				return m_leftCCWArrowAlpha;
			}
			set
			{
				m_leftCCWArrowAlpha = value;
				RefreshAlpha();
			}
		}

		public float leftCWArrowAlpha
		{
			get
			{
				return m_leftCWArrowAlpha;
			}
			set
			{
				m_leftCWArrowAlpha = value;
				RefreshAlpha();
			}
		}

		public float leftToggleUpArrowAlpha
		{
			get
			{
				return m_leftToggleUpArrowAlpha;
			}
			set
			{
				m_leftToggleUpArrowAlpha = value;
				RefreshAlpha();
			}
		}

		public float leftToggleDownArrowAlpha
		{
			get
			{
				return m_leftToggleDownArrowAlpha;
			}
			set
			{
				m_leftToggleDownArrowAlpha = value;
				RefreshAlpha();
			}
		}

		public float rightLeftArrowAlpha
		{
			get
			{
				return m_rightLeftArrowAlpha;
			}
			set
			{
				m_rightLeftArrowAlpha = value;
				RefreshAlpha();
			}
		}

		public float rightUpArrowAlpha
		{
			get
			{
				return m_rightUpArrowAlpha;
			}
			set
			{
				m_rightUpArrowAlpha = value;
				RefreshAlpha();
			}
		}

		public float rightRightArrowAlpha
		{
			get
			{
				return m_rightRightArrowAlpha;
			}
			set
			{
				m_rightRightArrowAlpha = value;
				RefreshAlpha();
			}
		}

		public float rightDownArrowAlpha
		{
			get
			{
				return m_rightDownArrowAlpha;
			}
			set
			{
				m_rightDownArrowAlpha = value;
				RefreshAlpha();
			}
		}

		public float rightCCWArrowAlpha
		{
			get
			{
				return m_rightCCWArrowAlpha;
			}
			set
			{
				m_rightCCWArrowAlpha = value;
				RefreshAlpha();
			}
		}

		public float rightCWArrowAlpha
		{
			get
			{
				return m_rightCWArrowAlpha;
			}
			set
			{
				m_rightCWArrowAlpha = value;
				RefreshAlpha();
			}
		}

		public float rightToggleUpArrowAlpha
		{
			get
			{
				return m_rightToggleUpArrowAlpha;
			}
			set
			{
				m_rightToggleUpArrowAlpha = value;
				RefreshAlpha();
			}
		}

		public float rightToggleDownArrowAlpha
		{
			get
			{
				return m_rightToggleDownArrowAlpha;
			}
			set
			{
				m_rightToggleDownArrowAlpha = value;
				RefreshAlpha();
			}
		}

		public Vector2 leftStick
		{
			get
			{
				return m_leftStick;
			}
			set
			{
				m_leftStick = value;
				RefreshSticks();
			}
		}

		public Vector2 rightStick
		{
			get
			{
				return m_rightStick;
			}
			set
			{
				m_rightStick = value;
				RefreshSticks();
			}
		}

		public void TweenArrows(string p_prefix, float p_duration, float p_left, float p_right, float p_up, float p_down, float p_cw, float p_ccw, float p_toggleUp, float p_toggleDown)
		{
			string[] array = new string[8]
			{
				p_prefix + "LeftArrowAlpha",
				p_prefix + "RightArrowAlpha",
				p_prefix + "UpArrowAlpha",
				p_prefix + "DownArrowAlpha",
				p_prefix + "CWArrowAlpha",
				p_prefix + "CCWArrowAlpha",
				p_prefix + "ToggleUpArrowAlpha",
				p_prefix + "ToggleDownArrowAlpha"
			};
			float[] array2 = new float[8] { p_left, p_right, p_up, p_down, p_cw, p_ccw, p_toggleUp, p_toggleDown };
			for (int i = 0; i < array.Length; i++)
			{
				Tween.Add(this, array[i], array2[i], p_duration, Cubic.Out);
			}
		}

		private void TweenKillArrows(string p_prefix)
		{
			string[] array = new string[8]
			{
				p_prefix + "LeftArrowAlpha",
				p_prefix + "RightArrowAlpha",
				p_prefix + "UpArrowAlpha",
				p_prefix + "DownArrowAlpha",
				p_prefix + "CWArrowAlpha",
				p_prefix + "CCWArrowAlpha",
				p_prefix + "ToggleUpArrowAlpha",
				p_prefix + "ToggleDownArrowAlpha"
			};
			for (int i = 0; i < array.Length; i++)
			{
				Tween.Kill(this, array[i]);
			}
		}

		public void HideArrows(float p_duration)
		{
			TweenKillArrows("left");
			TweenKillArrows("right");
			TweenLeftArrows(p_duration, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
			TweenRightArrows(p_duration, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
		}

		public void ShowInwardDirectionArrows(bool show, float p_duration)
		{
			if (show)
			{
				Tween.Kill(this);
				TweenLeftArrows(p_duration, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 0f);
				TweenRightArrows(p_duration, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 0f);
			}
			else
			{
				Tween.Kill(this);
				TweenLeftArrows(p_duration, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
				TweenRightArrows(p_duration, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
			}
		}

		public void ShowRotationArrows(float p_duration)
		{
			Tween.Kill(this);
			TweenLeftArrows(p_duration, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f);
			TweenRightArrows(p_duration, 0f, 0f, 0f, 0f, 0f, 1f, 0f, 0f);
		}

		public void TweenLeftArrows(float p_duration, float p_left, float p_right, float p_up, float p_down, float p_cw, float p_ccw, float p_toggleUp, float p_toggleDown)
		{
			TweenArrows("left", p_duration, p_left, p_right, p_up, p_down, p_cw, p_ccw, p_toggleUp, p_toggleDown);
		}

		public void ShowLeftDirectionArrows(float p_duration)
		{
			Tween.Kill(this);
			TweenLeftArrows(p_duration, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 0f);
		}

		public void ShowLeftHorizontalArrows(float p_duration)
		{
			Tween.Kill(this);
			TweenLeftArrows(p_duration, 1f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);
		}

		public void ShowLeftVerticalArrows(float p_duration)
		{
			Tween.Kill(this);
			TweenLeftArrows(p_duration, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);
		}

		public void ShowLeftCWArrows(float p_duration)
		{
			Tween.Kill(this);
			TweenLeftArrows(p_duration, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f);
		}

		public void ShowLeftCCWArrows(float p_duration)
		{
			Tween.Kill(this);
			TweenLeftArrows(p_duration, 0f, 0f, 0f, 0f, 0f, 1f, 0f, 0f);
		}

		public void ShowLeftToggleArrows(float p_duration)
		{
			Tween.Kill(this);
			TweenLeftArrows(p_duration, 0f, 0f, 0f, 0f, 0f, 0f, 1f, 1f);
		}

		public void TweenRightArrows(float p_duration, float p_left, float p_right, float p_up, float p_down, float p_cw, float p_ccw, float p_toggleUp, float p_toggleDown)
		{
			TweenArrows("right", p_duration, p_left, p_right, p_up, p_down, p_cw, p_ccw, p_toggleUp, p_toggleDown);
		}

		public void ShowRightDirectionArrows(float p_duration)
		{
			Tween.Kill(this);
			TweenRightArrows(p_duration, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 0f);
		}

		public void ShowRightHorizontalArrows(float p_duration)
		{
			Tween.Kill(this);
			TweenRightArrows(p_duration, 1f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);
		}

		public void ShowRightVerticalArrows(float p_duration)
		{
			Tween.Kill(this);
			TweenRightArrows(p_duration, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);
		}

		public void ShowRightCWArrows(float p_duration)
		{
			Tween.Kill(this);
			TweenRightArrows(p_duration, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f);
		}

		public void ShowRightCCWArrows(float p_duration)
		{
			Tween.Kill(this);
			TweenRightArrows(p_duration, 0f, 0f, 0f, 0f, 0f, 1f, 0f, 0f);
		}

		public void ShowRightToggleArrows(float p_duration)
		{
			Tween.Kill(this);
			TweenRightArrows(p_duration, 0f, 0f, 0f, 0f, 0f, 0f, 1f, 1f);
		}

		protected void RefreshAlpha()
		{
			float[] array = new float[16]
			{
				leftLeftArrowAlpha, leftRightArrowAlpha, leftUpArrowAlpha, leftDownArrowAlpha, leftCWArrowAlpha, leftCCWArrowAlpha, leftToggleUpArrowAlpha, leftToggleDownArrowAlpha, rightLeftArrowAlpha, rightRightArrowAlpha,
				rightUpArrowAlpha, rightDownArrowAlpha, rightCWArrowAlpha, rightCCWArrowAlpha, rightToggleUpArrowAlpha, rightToggleDownArrowAlpha
			};
			List<Renderer>[] array2 = new List<Renderer>[16]
			{
				leftLeftArrows, leftRightArrows, leftUpArrows, leftDownArrows, leftCWArrows, leftCCWArrows, leftToggleUpArrows, leftToggleDownArrows, rightLeftArrows, rightRightArrows,
				rightUpArrows, rightDownArrows, rightCWArrows, rightCCWArrows, rightToggleUpArrows, rightToggleDownArrows
			};
			int num = Mathf.Min(array.Length, array2.Length);
			for (int i = 0; i < num; i++)
			{
				ApplyAlpha(array2[i], array[i]);
			}
		}

		private void ApplyAlpha(List<Renderer> p_list, float p_alpha)
		{
			string text = "_TintColor";
			for (int i = 0; i < p_list.Count; i++)
			{
				Renderer renderer = p_list[i];
				if ((bool)renderer)
				{
					Material sharedMaterial = renderer.sharedMaterial;
					Color color = sharedMaterial.GetColor(text);
					color.a = p_alpha;
					sharedMaterial.SetColor(text, color);
				}
			}
		}

		public void AnimateLeftSticks(float p_speed, bool p_left, bool p_right, bool p_up, bool p_down)
		{
			if (m_left_stick_animation != null)
			{
				m_left_stick_animation.Stop();
			}
			if (m_user_input_loop != null)
			{
				m_user_input_loop.Stop();
			}
			if (m_toggle_animation != null)
			{
				m_toggle_animation.Stop();
			}
			m_left_stick_animation = Activity.Run(delegate(Activity a)
			{
				float num = a.elapsed * p_speed * 360f;
				float num2 = Mathf.Sin(num * ((float)Math.PI / 180f)) * 0.95f;
				float num3 = Mathf.Cos(num * ((float)Math.PI / 180f)) * 0.95f;
				if (!p_left)
				{
					num2 = Mathf.Clamp(num2, 0f, 1f);
				}
				if (!p_right)
				{
					num2 = Mathf.Clamp(num2, -1f, 0f);
				}
				if (!p_up)
				{
					num3 = Mathf.Clamp(num3, 0f, 1f);
				}
				if (!p_down)
				{
					num3 = Mathf.Clamp(num3, -1f, 0f);
				}
				if (!p_left && !p_right)
				{
					num2 = 0f;
				}
				if (!p_up && !p_down)
				{
					num3 = 0f;
				}
				leftStick = Vector2.Lerp(leftStick, new Vector2(num2, 0f - num3), a.deltaTime * 5f);
			});
		}

		public void ResetLeftSticks()
		{
			if (m_left_stick_animation != null)
			{
				m_left_stick_animation.Stop();
			}
			Tween.Add(this, "leftStick", Vector2.zero, 0.3f, Cubic.Out);
		}

		public void AnimateRightSticks(float p_speed, bool p_left, bool p_right, bool p_up, bool p_down)
		{
			if (m_right_stick_animation != null)
			{
				m_right_stick_animation.Stop();
			}
			if (m_user_input_loop != null)
			{
				m_user_input_loop.Stop();
			}
			if (m_toggle_animation != null)
			{
				m_toggle_animation.Stop();
			}
			m_right_stick_animation = Activity.Run(delegate(Activity a)
			{
				float num = a.elapsed * p_speed * 360f;
				float num2 = Mathf.Sin(num * ((float)Math.PI / 180f)) * 0.95f;
				float num3 = Mathf.Cos(num * ((float)Math.PI / 180f)) * 0.95f;
				if (!p_left)
				{
					num2 = Mathf.Clamp(num2, 0f, 1f);
				}
				if (!p_right)
				{
					num2 = Mathf.Clamp(num2, -1f, 0f);
				}
				if (!p_up)
				{
					num3 = Mathf.Clamp(num3, 0f, 1f);
				}
				if (!p_down)
				{
					num3 = Mathf.Clamp(num3, -1f, 0f);
				}
				if (!p_left && !p_right)
				{
					num2 = 0f;
				}
				if (!p_up && !p_down)
				{
					num3 = 0f;
				}
				rightStick = Vector2.Lerp(rightStick, new Vector2(num2, 0f - num3), a.deltaTime * 5f);
			});
		}

		public void AnimateToggle(RawAxis p_toggle)
		{
			if (m_toggle_animation != null)
			{
				m_toggle_animation.Stop();
			}
			m_toggle_animation = Activity.Run(delegate(Activity a)
			{
				float range = Mathf.Sin(a.elapsed * 360f * ((float)Math.PI / 180f)) * 0.95f;
				if (p_toggle == RawAxis.ToggleA)
				{
					AnimateToggleLeft(range);
				}
				else
				{
					AnimateToggleRight(range);
				}
			});
		}

		public void ResetRightSticks()
		{
			if (m_right_stick_animation != null)
			{
				m_right_stick_animation.Stop();
			}
			Tween.Add(this, "rightStick", Vector2.zero, 0.3f, Cubic.Out);
		}

		protected void RefreshSticks()
		{
			List<Transform> list = leftHorizontalList;
			List<Vector2> list2 = leftHorizontalRange;
			float x = leftStick.x;
			x = (x + 1f) * 0.5f;
			int num = Mathf.Min(list.Count, list2.Count);
			for (int i = 0; i < num; i++)
			{
				Transform transform = list[i];
				if ((bool)transform)
				{
					float x2 = list2[i].x;
					float y = list2[i].y;
					Vector3 localEulerAngles = transform.localEulerAngles;
					localEulerAngles.x = Mathf.Floor(localEulerAngles.x) + 0.8f;
					localEulerAngles.z = Mathf.Lerp(x2, y, x);
					transform.localEulerAngles = localEulerAngles;
				}
			}
			list = leftVerticalList;
			list2 = leftVerticalRange;
			x = 0f - leftStick.y;
			x = (x + 1f) * 0.5f;
			num = Mathf.Min(list.Count, list2.Count);
			for (int j = 0; j < num; j++)
			{
				Transform transform2 = list[j];
				if ((bool)transform2)
				{
					float x3 = list2[j].x;
					float y2 = list2[j].y;
					Vector3 localEulerAngles2 = transform2.localEulerAngles;
					localEulerAngles2.x = Mathf.Floor(localEulerAngles2.x) + 0.8f;
					localEulerAngles2.z = Mathf.Lerp(x3, y2, x);
					transform2.localEulerAngles = localEulerAngles2;
				}
			}
			list = rightHorizontalList;
			list2 = rightHorizontalRange;
			x = rightStick.x;
			x = (x + 1f) * 0.5f;
			num = Mathf.Min(list.Count, list2.Count);
			for (int k = 0; k < num; k++)
			{
				Transform transform3 = list[k];
				if ((bool)transform3)
				{
					float x4 = list2[k].x;
					float y3 = list2[k].y;
					Vector3 localEulerAngles3 = transform3.localEulerAngles;
					localEulerAngles3.x = Mathf.Floor(localEulerAngles3.x) + 0.8f;
					localEulerAngles3.z = Mathf.Lerp(x4, y3, x);
					transform3.localEulerAngles = localEulerAngles3;
				}
			}
			list = rightVerticalList;
			list2 = rightVerticalRange;
			x = 0f - rightStick.y;
			x = (x + 1f) * 0.5f;
			num = Mathf.Min(list.Count, list2.Count);
			for (int l = 0; l < num; l++)
			{
				Transform transform4 = list[l];
				if ((bool)transform4)
				{
					float x5 = list2[l].x;
					float y4 = list2[l].y;
					Vector3 localEulerAngles4 = transform4.localEulerAngles;
					localEulerAngles4.x = Mathf.Floor(localEulerAngles4.x) + 0.8f;
					localEulerAngles4.z = Mathf.Lerp(x5, y4, x);
					transform4.localEulerAngles = localEulerAngles4;
				}
			}
		}

		protected void Start()
		{
			Camera componentInChildren = GetComponentInChildren<Camera>();
			if (!componentInChildren)
			{
				return;
			}
			RenderTexture targetTexture = componentInChildren.targetTexture;
			if ((bool)targetTexture)
			{
				string text = targetTexture.name;
				targetTexture = UnityEngine.Object.Instantiate(targetTexture);
				targetTexture.name = text + "-" + targetTexture.GetHashCode().ToString("X6");
				componentInChildren.targetTexture = targetTexture;
				Transform transform = base.transform.Find("image");
				if ((bool)transform)
				{
					transform.GetComponent<RawImage>().texture = targetTexture;
				}
			}
		}

		public void SetController(ControllerStateType p_type)
		{
			if (p_type == m_current_type)
			{
				return;
			}
			m_current_type = p_type;
			Transform transform = Hierarchy.Find(base.transform, "render.controllers");
			if ((bool)transform)
			{
				List<ControllerTypeTag> list = Hierarchy.FindAll<ControllerTypeTag>(transform);
				for (int i = 0; i < list.Count; i++)
				{
					list[i].gameObject.SetActive(value: false);
					if (list[i].tags.Count > 0)
					{
						list[i].gameObject.SetActive(list[i].tags[0] == p_type);
					}
				}
			}
			transform = Hierarchy.Find(base.transform, "calibration");
			if ((bool)transform)
			{
				List<ControllerTypeTag> list = Hierarchy.FindAll<ControllerTypeTag>(transform);
				for (int j = 0; j < list.Count; j++)
				{
					list[j].gameObject.SetActive(value: false);
					if (list[j].tags.Count > 0)
					{
						list[j].gameObject.SetActive(list[j].tags[0] == p_type);
					}
				}
			}
			switch (p_type)
			{
			case ControllerStateType.XBox:
				controllerImgHorizontalSize = 345f;
				controllerImgHorizontalOffset = -12f;
				break;
			case ControllerStateType.PS4:
				controllerImgHorizontalSize = 310f;
				controllerImgHorizontalOffset = 15f;
				break;
			case ControllerStateType.Nikko:
				controllerImgHorizontalSize = 355f;
				controllerImgHorizontalOffset = 6f;
				break;
			default:
				controllerImgHorizontalSize = 450f;
				controllerImgHorizontalOffset = -9f;
				break;
			}
		}

		public void SetCalibrationLayer(ControllerCalibrationStateType p_type)
		{
			Transform transform = Hierarchy.Find(base.transform, "calibration");
			if (!transform)
			{
				return;
			}
			Transform transform2 = null;
			for (int i = 0; i < transform.childCount; i++)
			{
				if (transform.GetChild(i).gameObject.activeInHierarchy)
				{
					transform2 = transform.GetChild(i);
					break;
				}
			}
			if (!transform2)
			{
				return;
			}
			List<ControllerCalibrationStateTypeTag> list = Hierarchy.FindAll<ControllerCalibrationStateTypeTag>(transform2);
			for (int j = 0; j < list.Count; j++)
			{
				ControllerCalibrationStateTypeTag controllerCalibrationStateTypeTag = list[j];
				controllerCalibrationStateTypeTag.gameObject.SetActive(value: false);
				if (controllerCalibrationStateTypeTag.tags.Count > 0)
				{
					controllerCalibrationStateTypeTag.gameObject.SetActive(controllerCalibrationStateTypeTag.tags[0] == p_type);
				}
			}
		}

		public void SetAnimation(UIControllerAnimationType p_type, Drone p_drone = null)
		{
			float num = 1.3f;
			float p_duration = 0.5f;
			if (p_type != UIControllerAnimationType.UserInput || p_type != UIControllerAnimationType.DroneInput)
			{
				if (m_user_input_loop != null)
				{
					m_user_input_loop.Stop();
				}
				if (m_toggle_animation != null)
				{
					m_toggle_animation.Stop();
				}
				AnimateToggleLeft(0f);
				AnimateToggleRight(0f);
				ResetRightSticks();
			}
			switch (p_type)
			{
			case UIControllerAnimationType.StopAll:
				if (m_user_input_loop != null)
				{
					m_user_input_loop.Stop();
				}
				if (m_toggle_animation != null)
				{
					m_toggle_animation.Stop();
				}
				AnimateToggleLeft(0f);
				AnimateToggleRight(0f);
				HideArrows(0f);
				break;
			case UIControllerAnimationType.UserInput:
			case UIControllerAnimationType.DroneInput:
				HideArrows(p_duration);
				ResetLeftSticks();
				ResetRightSticks();
				if (m_user_input_loop != null)
				{
					m_user_input_loop.Stop();
				}
				if (m_toggle_animation != null)
				{
					m_toggle_animation.Stop();
				}
				if (p_type == UIControllerAnimationType.DroneInput && p_drone != null)
				{
					m_user_input_loop = Activity.Run((Func<bool>)delegate
					{
						if (!this)
						{
							return false;
						}
						if (!base.gameObject.activeInHierarchy)
						{
							return true;
						}
						if (!p_drone.hasFc)
						{
							return true;
						}
						Vector2 vector = new Vector2
						{
							x = p_drone.fc.rawSignal.yaw,
							y = p_drone.fc.rawSignal.throttle
						};
						leftStick = vector;
						vector.x = p_drone.fc.rawSignal.roll;
						vector.y = p_drone.fc.rawSignal.pitch;
						rightStick = vector;
						AnimateToggleLeft(RCI.GetToggleDown(RawAxis.ToggleA));
						AnimateToggleRight(RCI.GetToggleDown(RawAxis.ToggleB));
						return true;
					}, 0f, false);
					break;
				}
				m_user_input_loop = Activity.Run((Func<bool>)delegate
				{
					if (!this)
					{
						return false;
					}
					if (!base.gameObject.activeInHierarchy)
					{
						return true;
					}
					Vector2 vector = default(Vector2);
					Vector2 vector2 = default(Vector2);
					vector.x = (useChannels ? GetAssignedValue(RawAxis.LeftStickX) : RCI.GetRawAxis(RawAxis.LeftStickX));
					vector.y = (useChannels ? GetAssignedValue(RawAxis.LeftStickY) : RCI.GetRawAxis(RawAxis.LeftStickY));
					leftStick = vector;
					vector2.x = (useChannels ? GetAssignedValue(RawAxis.RightStickX) : RCI.GetRawAxis(RawAxis.RightStickX));
					vector2.y = (useChannels ? GetAssignedValue(RawAxis.RightStickY) : RCI.GetRawAxis(RawAxis.RightStickY));
					rightStick = vector2;
					AnimateToggleLeft(RCI.GetToggleDown(RawAxis.ToggleA));
					AnimateToggleRight(RCI.GetToggleDown(RawAxis.ToggleB));
					return true;
				}, 0f, false);
				break;
			default:
				ResetLeftSticks();
				ResetRightSticks();
				AnimateToggleLeft(0f);
				AnimateToggleRight(0f);
				if (m_user_input_loop != null)
				{
					m_user_input_loop.Stop();
				}
				if (m_toggle_animation != null)
				{
					m_toggle_animation.Stop();
				}
				break;
			}
			switch (p_type)
			{
			case UIControllerAnimationType.StopAll:
				ResetLeftSticks();
				ResetRightSticks();
				if (m_user_input_loop != null)
				{
					m_user_input_loop.Stop();
				}
				if (m_toggle_animation != null)
				{
					m_toggle_animation.Stop();
				}
				AnimateToggleLeft(0f);
				AnimateToggleRight(0f);
				break;
			case UIControllerAnimationType.StopLeft:
				ResetLeftSticks();
				break;
			case UIControllerAnimationType.StopRight:
				ResetRightSticks();
				break;
			case UIControllerAnimationType.LeftStickLeft:
				AnimateLeftSticks(num, p_left: true, p_right: false, p_up: false, p_down: false);
				break;
			case UIControllerAnimationType.LeftStickRight:
				AnimateLeftSticks(num, p_left: false, p_right: true, p_up: false, p_down: false);
				break;
			case UIControllerAnimationType.LeftStickUp:
				AnimateLeftSticks(num, p_left: false, p_right: false, p_up: true, p_down: false);
				break;
			case UIControllerAnimationType.LeftStickDown:
				AnimateLeftSticks(num, p_left: false, p_right: false, p_up: false, p_down: true);
				break;
			case UIControllerAnimationType.LeftStickHorizontal:
				AnimateLeftSticks(num, p_left: true, p_right: true, p_up: false, p_down: false);
				break;
			case UIControllerAnimationType.LeftStickVertical:
				AnimateLeftSticks(num, p_left: false, p_right: false, p_up: true, p_down: true);
				break;
			case UIControllerAnimationType.LeftStickCW:
				AnimateLeftSticks(0f - num, p_left: true, p_right: true, p_up: true, p_down: true);
				break;
			case UIControllerAnimationType.LeftStickCCW:
				AnimateLeftSticks(num, p_left: true, p_right: true, p_up: true, p_down: true);
				break;
			case UIControllerAnimationType.LeftTurn:
				AnimateLeftSticks(num, p_left: true, p_right: false, p_up: false, p_down: false);
				AnimateRightSticks(num, p_left: true, p_right: false, p_up: false, p_down: false);
				break;
			case UIControllerAnimationType.LeftToggle:
				AnimateToggle(RawAxis.ToggleA);
				break;
			case UIControllerAnimationType.RightStickLeft:
				AnimateRightSticks(num, p_left: true, p_right: false, p_up: false, p_down: false);
				break;
			case UIControllerAnimationType.RightStickRight:
				AnimateRightSticks(num, p_left: false, p_right: true, p_up: false, p_down: false);
				break;
			case UIControllerAnimationType.RightStickUp:
				AnimateRightSticks(num, p_left: false, p_right: false, p_up: true, p_down: false);
				break;
			case UIControllerAnimationType.RightStickDown:
				AnimateRightSticks(num, p_left: false, p_right: false, p_up: false, p_down: true);
				break;
			case UIControllerAnimationType.RightStickHorizontal:
				AnimateRightSticks(num, p_left: true, p_right: true, p_up: false, p_down: false);
				break;
			case UIControllerAnimationType.RightStickVertical:
				AnimateRightSticks(num, p_left: false, p_right: false, p_up: true, p_down: true);
				break;
			case UIControllerAnimationType.RightStickCW:
				AnimateRightSticks(0f - num, p_left: true, p_right: true, p_up: true, p_down: true);
				break;
			case UIControllerAnimationType.RightStickCCW:
				AnimateRightSticks(num, p_left: true, p_right: true, p_up: true, p_down: true);
				break;
			case UIControllerAnimationType.RightTurn:
				AnimateLeftSticks(num, p_left: false, p_right: true, p_up: false, p_down: false);
				AnimateRightSticks(num, p_left: false, p_right: true, p_up: false, p_down: false);
				break;
			case UIControllerAnimationType.RightToggle:
				AnimateToggle(RawAxis.ToggleB);
				break;
			}
			switch (p_type)
			{
			case UIControllerAnimationType.StopLeft:
				TweenArrows("left", p_duration, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.StopRight:
				TweenArrows("right", p_duration, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.StopAll:
				HideArrows(p_duration);
				break;
			case UIControllerAnimationType.LeftStickLeft:
				TweenArrows("left", p_duration, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.LeftStickRight:
				TweenArrows("left", p_duration, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.LeftStickUp:
				TweenArrows("left", p_duration, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.LeftStickDown:
				TweenArrows("left", p_duration, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.LeftStickHorizontal:
				TweenArrows("left", p_duration, 1f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.LeftStickVertical:
				TweenArrows("left", p_duration, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.LeftStickCW:
				TweenArrows("left", p_duration, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.LeftStickCCW:
				TweenArrows("left", p_duration, 0f, 0f, 0f, 0f, 0f, 1f, 0f, 0f);
				break;
			case UIControllerAnimationType.LeftAll:
				TweenArrows("left", p_duration, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.LeftTurn:
				TweenArrows("left", p_duration, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
				TweenArrows("right", p_duration, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.LeftToggle:
				TweenArrows("left", p_duration, 0f, 0f, 0f, 0f, 0f, 0f, 1f, 1f);
				break;
			case UIControllerAnimationType.RightStickLeft:
				TweenArrows("right", p_duration, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.RightStickRight:
				TweenArrows("right", p_duration, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.RightStickUp:
				TweenArrows("right", p_duration, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.RightStickDown:
				TweenArrows("right", p_duration, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.RightStickHorizontal:
				TweenArrows("right", p_duration, 1f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.RightStickVertical:
				TweenArrows("right", p_duration, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.RightStickCW:
				TweenArrows("right", p_duration, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.RightStickCCW:
				TweenArrows("right", p_duration, 0f, 0f, 0f, 0f, 0f, 1f, 0f, 0f);
				break;
			case UIControllerAnimationType.RightAll:
				TweenArrows("right", p_duration, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.RightTurn:
				TweenArrows("left", p_duration, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);
				TweenArrows("right", p_duration, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);
				break;
			case UIControllerAnimationType.RightToggle:
				TweenArrows("right", p_duration, 0f, 0f, 0f, 0f, 0f, 0f, 1f, 1f);
				break;
			case UIControllerAnimationType.UserInput:
			case UIControllerAnimationType.DroneInput:
				break;
			}
		}

		public void AnimateSticksRotating()
		{
			AnimateRightSticks(1.3f, p_left: true, p_right: true, p_up: true, p_down: true);
			AnimateLeftSticks(-1.3f, p_left: true, p_right: true, p_up: true, p_down: true);
		}

		public void AnimateToggleLeft(float range)
		{
			if (!(leftToggle == null))
			{
				if (range > 0f)
				{
					leftToggle.localEulerAngles = new Vector3(Mathf.Lerp(30f, 60f, range), 90f, 180f);
				}
				else
				{
					leftToggle.localEulerAngles = new Vector3(Mathf.Lerp(30f, 0f, 0f - range), 90f, 180f);
				}
			}
		}

		public void AnimateToggleRight(float range)
		{
			if (!(rightToggle == null))
			{
				if (range > 0f)
				{
					rightToggle.localEulerAngles = new Vector3(Mathf.Lerp(30f, 60f, range), 270f, 180f);
				}
				else
				{
					rightToggle.localEulerAngles = new Vector3(Mathf.Lerp(30f, 0f, 0f - range), 270f, 180f);
				}
			}
		}

		public void UseRCChannels(Dictionary<RawAxis, int> p_channels)
		{
			useChannels = true;
			if (p_channels == null)
			{
				return;
			}
			foreach (KeyValuePair<RawAxis, int> p_channel in p_channels)
			{
				if (channels.ContainsKey(p_channel.Key))
				{
					channels[p_channel.Key] = p_channel.Value;
				}
			}
		}

		public void UseRCChannels()
		{
			if (!RCI.HasSavedProfile())
			{
				return;
			}
			useChannels = true;
			RCDeviceData savedProfile = RCI.GetSavedProfile();
			foreach (RawAxis item in channels.Keys.ToList())
			{
				AssignedAxisData aAD = savedProfile.GetAAD(item);
				if (aAD != null)
				{
					channels[item] = aAD.ElementID;
					center[item] = aAD.center;
					inverts[item] = aAD.inverted;
					min[item] = aAD.min;
					max[item] = aAD.max;
					deadzone[item] = aAD.deadzone;
				}
				else
				{
					Debug.LogError("UIControllerOverlay> axis [" + item.ToString() + "] not found");
				}
			}
		}

		public void UseRawAxis()
		{
			useChannels = false;
		}

		public void ResetChannelData()
		{
			inverts[RawAxis.LeftStickX] = false;
			inverts[RawAxis.LeftStickY] = false;
			inverts[RawAxis.RightStickX] = false;
			inverts[RawAxis.RightStickY] = false;
			min[RawAxis.LeftStickX] = -1f;
			min[RawAxis.LeftStickY] = -1f;
			min[RawAxis.RightStickX] = -1f;
			min[RawAxis.RightStickY] = -1f;
			max[RawAxis.LeftStickX] = 1f;
			max[RawAxis.LeftStickY] = 1f;
			max[RawAxis.RightStickX] = 1f;
			max[RawAxis.RightStickY] = 1f;
			center[RawAxis.LeftStickX] = 0f;
			center[RawAxis.LeftStickY] = 0f;
			center[RawAxis.RightStickX] = 0f;
			center[RawAxis.RightStickY] = 0f;
			deadzone[RawAxis.LeftStickX] = 0f;
			deadzone[RawAxis.LeftStickY] = 0f;
			deadzone[RawAxis.RightStickX] = 0f;
			deadzone[RawAxis.RightStickY] = 0f;
		}

		public void UpdateChannelData(RawAxis axis, CalibrationData data)
		{
			useChannels = true;
			channels[axis] = data.ElementIDs[axis];
			if (axis != RawAxis.ToggleA && axis != RawAxis.ToggleB)
			{
				center[axis] = data.Centers[data.ElementIDs[axis]];
				if (data.Invert.ContainsKey(axis))
				{
					inverts[axis] = data.Invert[axis];
				}
				if (data.RangeMin.ContainsKey(axis))
				{
					min[axis] = data.RangeMin[axis];
				}
				if (data.RangeMax.ContainsKey(axis))
				{
					max[axis] = data.RangeMax[axis];
				}
				if (data.Deadzone.ContainsKey(axis))
				{
					deadzone[axis] = data.Deadzone[axis];
				}
			}
		}

		public void UpdateChannelData(CalibrationData data)
		{
			useChannels = true;
			foreach (RawAxis key in data.ElementIDs.Keys)
			{
				channels[key] = data.ElementIDs[key];
				if (key != RawAxis.ToggleA && key != RawAxis.ToggleB)
				{
					if (data.ElementIDs[key] >= 0 && data.ElementIDs[key] < data.Centers.Length)
					{
						center[key] = data.Centers[data.ElementIDs[key]];
					}
					if (data.Invert.ContainsKey(key))
					{
						inverts[key] = data.Invert[key];
					}
					if (data.RangeMin.ContainsKey(key))
					{
						min[key] = data.RangeMin[key];
					}
					if (data.RangeMax.ContainsKey(key))
					{
						max[key] = data.RangeMax[key];
					}
					if (data.Deadzone.ContainsKey(key))
					{
						deadzone[key] = data.Deadzone[key];
					}
				}
			}
		}

		public void UpdateInvert(RawAxis axis, bool p_invert)
		{
			inverts[axis] = p_invert;
		}

		private float GetAssignedValue(RawAxis axis)
		{
			return RCI.GetAssignedAxisValueFromIndex(channels[axis], min[axis], max[axis], center[axis], deadzone[axis], -2f, inverts[axis]);
		}
	}
}
