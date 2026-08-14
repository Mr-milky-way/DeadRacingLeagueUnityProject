using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MECameraToolPreviewInspector : View<DRLApp>
	{
		public RawImage captureImageField;

		public RectTransform contentField;

		public RenderTexture captureImageRT;

		public MapEditorView editor;

		public MECameraPreview camera;

		private Activity p_render_loop;

		public bool expanded
		{
			get
			{
				Vector2 sizeDelta = contentField.sizeDelta;
				if (sizeDelta.x > 430f)
				{
					return true;
				}
				if (sizeDelta.y > 270f)
				{
					return true;
				}
				return false;
			}
		}

		protected void Awake()
		{
			if (!captureImageRT)
			{
				captureImageRT = new RenderTexture(600, 400, 24, RenderTextureFormat.ARGBFloat);
				captureImageRT.name = "$ct-preview-rt";
				captureImageRT.useMipMap = false;
				captureImageRT.filterMode = FilterMode.Bilinear;
				captureImageField.texture = captureImageRT;
			}
		}

		public void Expand(bool p_flag, float p_duration = 0f)
		{
			float x = (p_flag ? 645f : 430f);
			float y = (p_flag ? 405f : 270f);
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
				Camera main = editor.camera.main;
				GameObject gameObject = UnityEngine.Object.Instantiate(main.gameObject, main.transform.parent.parent);
				gameObject.name = "$ct-preview-camera";
				UnityEngine.Object.Destroy(gameObject.GetComponent<AkAudioListener>());
				UnityEngine.Object.Destroy(gameObject.GetComponent<AkGameObj>());
				camera = gameObject.AddComponent<MECameraPreview>();
				camera.camera.cullingMask = camera.camera.cullingMask & -3;
				camera.camera.targetTexture = captureImageRT;
				camera.camera.depth = 100f;
				camera.editor = editor;
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

		public void Render(Camera p_camera, float p_fov, Transform p_anchor)
		{
			if ((bool)p_anchor)
			{
				Render(p_camera, p_fov, p_anchor.position, p_anchor.rotation);
			}
		}

		public void Render(MACameraToolControlPoint p_control_point)
		{
			if ((bool)editor)
			{
				Render(camera.camera, p_control_point.fov, p_control_point.transform);
			}
		}

		public void RenderLoop(MACameraToolControlPoint p_control_point)
		{
			if (p_render_loop != null)
			{
				p_render_loop.Stop();
			}
			camera.camera.enabled = true;
			p_render_loop = Activity.Run((Func<bool>)delegate
			{
				if (!base.validContext)
				{
					return false;
				}
				if (!base.gameObject.activeInHierarchy)
				{
					camera.camera.enabled = false;
					return false;
				}
				Render(p_control_point);
				return true;
			}, 0f, false);
		}

		public void AnimateLoop(MACameraTool p_tool, MACameraToolControlPoint p_caller, float p_duration)
		{
			if (!p_tool || p_tool.mode != CameraToolMode.Wire)
			{
				return;
			}
			List<MACameraToolControlPoint> cpl = p_tool.GetControlPoints();
			if (p_render_loop != null)
			{
				p_render_loop.Stop();
			}
			camera.camera.enabled = true;
			float t = ((p_caller == cpl[1]) ? (-1.5f) : 0f);
			float tf = ((p_caller == cpl[0]) ? (p_duration + 1.5f) : p_duration);
			p_render_loop = Activity.Run((Func<bool>)delegate
			{
				if (!p_tool)
				{
					return false;
				}
				if (!base.gameObject.activeInHierarchy)
				{
					camera.camera.enabled = false;
					return false;
				}
				float p_ratio = Mathf.Clamp01(t / p_duration);
				t += Time.deltaTime;
				if (t > tf)
				{
					RenderLoop(p_caller);
					return false;
				}
				p_tool.animation.Step(camera.camera, cpl[0], cpl[1], p_ratio, p_tool.easing);
				return true;
			}, 0f, false);
		}
	}
}
