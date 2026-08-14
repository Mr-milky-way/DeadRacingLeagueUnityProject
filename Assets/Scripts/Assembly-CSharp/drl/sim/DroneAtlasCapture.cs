using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl.sim
{
	[RequireComponent(typeof(Camera))]
	public class DroneAtlasCapture : MonoBehaviour
	{
		[Serializable]
		public class Batch
		{
			public Texture target;

			public List<Texture2D> textures;

			public bool isNormal;
		}

		private static DroneAtlasCapture m_instance;

		[SerializeField]
		private Camera m_camera;

		public List<Texture> pool;

		public List<MeshRenderer> planes;

		public List<Batch> queue;

		public List<Batch> batches;

		public List<Texture> history;

		public bool debug;

		public Camera camera
		{
			get
			{
				if (!m_camera)
				{
					return m_camera = GetComponent<Camera>();
				}
				return m_camera;
			}
		}

		public int atlasSize
		{
			get
			{
				int graphicsMemorySize = SystemInfo.graphicsMemorySize;
				if (graphicsMemorySize > 2048)
				{
					if (graphicsMemorySize < 4096)
					{
						return 1024;
					}
					return 1024;
				}
				return 512;
			}
		}

		public static Texture GenerateAtlas(bool p_is_normal, params Texture2D[] p_textures)
		{
			if (!m_instance)
			{
				return Texture2D.blackTexture;
			}
			return m_instance.Generate(p_is_normal, p_textures);
		}

		public static Texture GenerateAtlas(params Texture2D[] p_textures)
		{
			return GenerateAtlas(p_is_normal: false, p_textures);
		}

		public static void Restore(Texture p_atlas)
		{
			if ((bool)m_instance)
			{
				if (p_atlas is Texture2D)
				{
					Texture2D texture2D = (Texture2D)p_atlas;
					texture2D.Resize(texture2D.width, texture2D.height, TextureFormat.ARGB32, hasMipMap: true);
				}
				m_instance.pool.Add(p_atlas);
			}
		}

		protected void Awake()
		{
			m_instance = this;
			int num = atlasSize;
			int num2 = 48;
			pool = new List<Texture>();
			for (int i = 0; i < num2; i++)
			{
				RenderTexture renderTexture = new RenderTexture(num, num, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
				renderTexture.name = "drone-atlas-" + renderTexture.GetHashCode().ToString("x6");
				pool.Add(renderTexture);
			}
			for (int j = 0; j < planes.Count; j++)
			{
				Material sharedMaterial = planes[j].sharedMaterial;
				sharedMaterial = UnityEngine.Object.Instantiate(sharedMaterial);
				sharedMaterial.name = sharedMaterial.name.Replace("(Clone)", "") + "-" + sharedMaterial.GetHashCode().ToString("x6");
				planes[j].sharedMaterial = sharedMaterial;
			}
		}

		public Texture Generate(bool p_is_normal, params Texture2D[] p_textures)
		{
			if (pool.Count <= 0)
			{
				return Texture2D.blackTexture;
			}
			RenderTexture targetTexture = camera.targetTexture;
			targetTexture = (RenderTexture)pool[0];
			pool.RemoveAt(0);
			if (!targetTexture)
			{
				int num = atlasSize;
				targetTexture = new RenderTexture(num, num, 0, RenderTextureFormat.ARGB32);
			}
			camera.targetTexture = targetTexture;
			_ = targetTexture.width;
			_ = targetTexture.height;
			if (debug)
			{
				history.Add(targetTexture);
			}
			Batch batch = new Batch();
			batch.target = targetTexture;
			batch.textures = new List<Texture2D>(p_textures);
			batch.isNormal = p_is_normal;
			queue.Add(batch);
			if (debug)
			{
				batches.Add(batch);
			}
			if (queue.Count == 1)
			{
				RunQueue();
			}
			return targetTexture;
		}

		public Texture Generate(params Texture2D[] p_textures)
		{
			return Generate(p_is_normal: false, p_textures);
		}

		protected void RunQueue()
		{
			if (queue.Count > 0)
			{
				Batch batch = queue[0];
				int num = Mathf.Min(batch.textures.Count, planes.Count);
				for (int i = 0; i < num; i++)
				{
					planes[i].sharedMaterial.mainTexture = batch.textures[i];
				}
				camera.targetTexture = (RenderTexture)batch.target;
				camera.Render();
				camera.targetTexture = null;
				queue.RemoveAt(0);
				for (int j = 0; j < num; j++)
				{
					planes[j].sharedMaterial.mainTexture = null;
				}
				RunQueue();
			}
		}
	}
}
