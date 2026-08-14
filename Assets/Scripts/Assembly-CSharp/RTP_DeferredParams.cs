using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[AddComponentMenu("Relief Terrain/Helpers/Deferred Params")]
[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
[ExecuteInEditMode]
public class RTP_DeferredParams : MonoBehaviour
{
	private Camera mycam;

	private CommandBuffer combufPreLight;

	private CommandBuffer combufPostLight;

	public Material CopyPropsMat;

	private HashSet<Camera> sceneCamsWithBuffer = new HashSet<Camera>();

	public void OnEnable()
	{
		if (NotifyDecals())
		{
			return;
		}
		if (mycam == null)
		{
			mycam = GetComponent<Camera>();
			if (mycam == null)
			{
				return;
			}
		}
		Initialize();
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(SetupCam));
	}

	public void OnDisable()
	{
		NotifyDecals();
		Cleanup();
	}

	public void OnDestroy()
	{
		NotifyDecals();
		Cleanup();
	}

	private bool NotifyDecals()
	{
		Type type = Type.GetType("UBERDecalSystem.DecalManager");
		if (type != null && UnityEngine.Object.FindObjectOfType(type) != null && UnityEngine.Object.FindObjectOfType(type) is MonoBehaviour && (UnityEngine.Object.FindObjectOfType(type) as MonoBehaviour).enabled)
		{
			(UnityEngine.Object.FindObjectOfType(type) as MonoBehaviour).Invoke("OnDisable", 0f);
			(UnityEngine.Object.FindObjectOfType(type) as MonoBehaviour).Invoke("OnEnable", 0f);
			return true;
		}
		return false;
	}

	private void Cleanup()
	{
		if (combufPreLight != null)
		{
			if ((bool)mycam)
			{
				mycam.RemoveCommandBuffer(CameraEvent.BeforeReflections, combufPreLight);
				mycam.RemoveCommandBuffer(CameraEvent.AfterLighting, combufPostLight);
			}
			foreach (Camera item in sceneCamsWithBuffer)
			{
				if ((bool)item)
				{
					item.RemoveCommandBuffer(CameraEvent.BeforeReflections, combufPreLight);
					item.RemoveCommandBuffer(CameraEvent.AfterLighting, combufPostLight);
				}
			}
		}
		sceneCamsWithBuffer.Clear();
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(SetupCam));
	}

	private void SetupCam(Camera cam)
	{
		bool flag = false;
		if (cam == mycam || flag)
		{
			RefreshComBufs(cam, flag);
		}
	}

	public void RefreshComBufs(Camera cam, bool isSceneCam)
	{
		if (!cam || combufPreLight == null || combufPostLight == null)
		{
			return;
		}
		CommandBuffer[] commandBuffers = cam.GetCommandBuffers(CameraEvent.BeforeReflections);
		bool flag = false;
		CommandBuffer[] array = commandBuffers;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].name == combufPreLight.name)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			cam.AddCommandBuffer(CameraEvent.BeforeReflections, combufPreLight);
			cam.AddCommandBuffer(CameraEvent.AfterLighting, combufPostLight);
			if (isSceneCam)
			{
				sceneCamsWithBuffer.Add(cam);
			}
		}
	}

	public void Initialize()
	{
		if (combufPreLight != null)
		{
			return;
		}
		int num = Shader.PropertyToID("_UBERPropsBuffer");
		if (CopyPropsMat == null)
		{
			if (CopyPropsMat != null)
			{
				UnityEngine.Object.DestroyImmediate(CopyPropsMat);
			}
			CopyPropsMat = new Material(Shader.Find("Hidden/UBER_CopyPropsTexture"));
			CopyPropsMat.hideFlags = HideFlags.DontSave;
		}
		combufPreLight = new CommandBuffer();
		combufPreLight.name = "UBERPropsPrelight";
		combufPreLight.GetTemporaryRT(num, -1, -1, 0, FilterMode.Point, RenderTextureFormat.RHalf);
		combufPreLight.Blit(BuiltinRenderTextureType.CameraTarget, num, CopyPropsMat);
		combufPostLight = new CommandBuffer();
		combufPostLight.name = "UBERPropsPostlight";
		combufPostLight.ReleaseTemporaryRT(num);
	}
}
