using System;
using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MESplineCourseCameraPreviewInspector : View<DRLApp>
	{
		public RawImage captureImageField;

		public RectTransform contentField;

		public RenderTexture captureImageRT;

		public MapEditorView editor;

		public MECameraPreview camera;

		public SplineActor actor;

		public SplineActor actorTemplate;

		public Vector2 expandSize = new Vector2(860f, 540f);

		private Activity p_render_loop;

		public bool expanded
		{
			get
			{
				Vector2 sizeDelta = contentField.sizeDelta;
				if (sizeDelta.x >= expandSize.x * 0.9f)
				{
					return true;
				}
				if (sizeDelta.y >= expandSize.y * 0.9f)
				{
					return true;
				}
				return false;
			}
		}

		protected void Awake()
		{
		}

		public void Expand(bool p_flag, float p_duration = 0f)
		{
			float x = (p_flag ? expandSize.x : 430f);
			float y = (p_flag ? expandSize.y : 270f);
			float x2 = (p_flag ? (-460f) : 0f);
			float y2 = (p_flag ? 35f : 0f);
			Vector2 vector = new Vector2(x, y);
			Vector2 vector2 = new Vector2(x2, y2);
			Activity.Remove(contentField);
			if (p_duration <= 0f)
			{
				contentField.sizeDelta = vector;
				contentField.anchoredPosition = vector2;
			}
			else
			{
				Tween.Add(contentField, "sizeDelta", vector, p_duration, Cubic.Out);
				Tween.Add(contentField, "anchoredPosition", vector2, p_duration, Cubic.Out);
			}
		}

		public void Init(MapEditorView p_editor)
		{
			if (!editor)
			{
				editor = p_editor;
				if (!captureImageRT)
				{
					captureImageRT = new RenderTexture((int)expandSize.x, (int)expandSize.y, 24, RenderTextureFormat.ARGBFloat);
					captureImageRT.name = "$spl-preview-rt";
					captureImageRT.useMipMap = false;
					captureImageRT.filterMode = FilterMode.Bilinear;
					captureImageField.texture = captureImageRT;
				}
				Camera main = editor.camera.main;
				GameObject gameObject = UnityEngine.Object.Instantiate(main.gameObject, main.transform.parent.parent);
				gameObject.name = "$spl-course-camera";
				UnityEngine.Object.Destroy(gameObject.GetComponent<AkAudioListener>());
				UnityEngine.Object.Destroy(gameObject.GetComponent<AkGameObj>());
				UnityEngine.Object.Destroy(gameObject.GetComponent<CameraNearPlaneSnap>());
				camera = gameObject.AddComponent<MECameraPreview>();
				camera.camera.cullingMask = camera.camera.cullingMask & -3;
				camera.camera.targetTexture = captureImageRT;
				camera.camera.depth = 100f;
				camera.camera.nearClipPlane = 0.5f;
				camera.editor = editor;
				actorTemplate.gameObject.SetActive(value: false);
				if ((bool)actor)
				{
					UnityEngine.Object.Destroy(actor.gameObject);
				}
				actor = UnityEngine.Object.Instantiate(actorTemplate);
				actor.name = "$spl-course-actor";
				actor.gameObject.SetActive(value: true);
				actor.transform.SetParent(main.transform.parent.parent);
			}
		}

		public void Render(Camera p_camera, float p_fov, Vector3 p_position, Quaternion p_rotation)
		{
			Transform transform = p_camera.transform;
			_ = transform.position;
			_ = transform.rotation;
			_ = p_camera.fieldOfView;
			_ = p_camera.targetTexture;
			transform.position = p_position;
			transform.rotation = p_rotation;
			p_camera.fieldOfView = p_fov;
		}

		public void Play()
		{
			if ((bool)actor)
			{
				actor.snap = SplineActor.SnapMode.Start;
				actor.Snap();
				actor.Run();
				actor.transform.Find("cube").gameObject.SetActive(value: true);
			}
		}

		public void Stop()
		{
			if ((bool)actor)
			{
				actor.auto = false;
				actor.snap = SplineActor.SnapMode.Start;
				actor.Snap();
				actor.transform.Find("cube").gameObject.SetActive(value: false);
				if (p_render_loop != null)
				{
					p_render_loop.Stop();
				}
			}
		}

		public void Toggle()
		{
			if (actor.auto)
			{
				Stop();
			}
			else
			{
				Play();
			}
		}

		public void RenderLoop(MASpline p_spline, Transform p_target)
		{
			if (p_render_loop != null)
			{
				p_render_loop.Stop();
			}
			if (!actor)
			{
				return;
			}
			actor.spline = p_spline.spline;
			actor.speed = p_spline.splineCourseCameraSpeed;
			actor.snap = SplineActor.SnapMode.Start;
			actor.wrap = WrapMode.Once;
			actor.Snap();
			camera.camera.enabled = true;
			p_render_loop = Activity.Run((Func<bool>)delegate
			{
				if (!actor)
				{
					return false;
				}
				if (!base.validContext || !editor || !base.gameObject || !base.gameObject.activeInHierarchy)
				{
					Stop();
					camera.camera.enabled = false;
					return false;
				}
				float splineCourseCameraFOV = p_spline.splineCourseCameraFOV;
				Render(camera.camera, splineCourseCameraFOV, p_target.position, p_target.rotation);
				if (actor.progress >= 1f)
				{
					Play();
				}
				return true;
			}, 0f, false);
			p_render_loop.late = true;
		}
	}
}
