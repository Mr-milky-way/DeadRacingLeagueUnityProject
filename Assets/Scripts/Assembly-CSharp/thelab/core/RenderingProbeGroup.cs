using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace thelab.core
{
	public class RenderingProbeGroup : MonoBehaviour
	{
		[Serializable]
		public struct ProbeResult
		{
			public RenderingProbe probe;

			public float intensity;

			public float distance;

			public float weight;
		}

		public int kNearest = 4;

		public RenderingProbe[] probes;

		public ProbeResult[] results;

		public int count;

		public Camera target;

		public Color ambientColor;

		public Color ambientBrightness = Color.black;

		public Color fogColor;

		public float fogDensity;

		public float fogTexBlend;

		public PostProcessingBehaviour postProcessing;

		public RenderingProbe.Sample defaultSample;

		public RenderingProbe.Sample currentSample;

		protected Camera cmain;

		protected Color rsAmbientSkyColor
		{
			get
			{
				return RenderSettings.ambientSkyColor;
			}
			set
			{
				RenderSettings.ambientSkyColor = value;
			}
		}

		protected Color rsFogColor
		{
			get
			{
				return RenderSettings.fogColor;
			}
			set
			{
				RenderSettings.fogColor = value;
			}
		}

		protected float rsFogDensity
		{
			get
			{
				return RenderSettings.fogDensity;
			}
			set
			{
				RenderSettings.fogDensity = value;
			}
		}

		protected float rsFogTexBlend
		{
			get
			{
				if (!postProcessing)
				{
					return 1f;
				}
				return postProcessing.profile.fog.settings.textureBlend;
			}
			set
			{
				if ((bool)postProcessing)
				{
					FogModel.Settings settings = postProcessing.profile.fog.settings;
					settings.textureBlend = value;
					postProcessing.profile.fog.settings = settings;
				}
			}
		}

		protected void Awake()
		{
			defaultSample = default(RenderingProbe.Sample);
			defaultSample.ambientColor = rsAmbientSkyColor;
			defaultSample.fogColor = rsFogColor;
			defaultSample.fogDensity = rsFogDensity;
			defaultSample.fogTexBlend = rsFogTexBlend;
			ambientColor = rsAmbientSkyColor;
			fogColor = rsFogColor;
			fogDensity = rsFogDensity;
			fogTexBlend = rsFogTexBlend;
			results = new ProbeResult[20];
			List<RenderingProbe> l = new List<RenderingProbe>();
			Hierarchy.Traverse(base.transform, delegate(RenderingProbe it)
			{
				l.Add(it);
			});
			probes = l.ToArray();
		}

		protected void Update()
		{
			if (!cmain)
			{
				cmain = Camera.main;
			}
			Camera camera = (target ? target : cmain);
			if (!postProcessing)
			{
				postProcessing = (camera ? camera.GetComponent<PostProcessingBehaviour>() : null);
			}
			Vector3 p_position = (camera ? camera.transform.position : (Vector3.one * 10000f));
			count = 0;
			for (int i = 0; i < probes.Length; i++)
			{
				RenderingProbe renderingProbe = probes[i];
				if (!renderingProbe || !renderingProbe.isActiveAndEnabled)
				{
					continue;
				}
				float intensity = renderingProbe.GetIntensity(p_position);
				if (!(intensity <= 0f))
				{
					float distance = renderingProbe.GetDistance(p_position);
					results[count].probe = renderingProbe;
					results[count].intensity = intensity;
					results[count].distance = distance;
					results[count].weight = 0f;
					count++;
					if (count >= kNearest)
					{
						break;
					}
				}
			}
			Array.Sort(results, delegate(ProbeResult a, ProbeResult b)
			{
				if (!a.probe && !b.probe)
				{
					return 0;
				}
				if (!a.probe)
				{
					return 1;
				}
				if (!b.probe)
				{
					return -1;
				}
				if (a.probe.importance < b.probe.importance)
				{
					return -1;
				}
				if (a.probe.importance > b.probe.importance)
				{
					return 1;
				}
				if (a.distance < b.distance)
				{
					return 1;
				}
				return (a.distance > b.distance) ? (-1) : 0;
			});
			currentSample = defaultSample;
			for (int num = 0; num < count; num++)
			{
				ProbeResult probeResult = results[num];
				currentSample = RenderingProbe.Sample.Lerp(currentSample, probeResult.probe.sample, probeResult.intensity);
			}
			rsAmbientSkyColor = currentSample.ambientColor + ambientBrightness;
			rsFogColor = currentSample.fogColor;
			rsFogDensity = currentSample.fogDensity;
			rsFogTexBlend = currentSample.fogTexBlend;
		}

		protected virtual void OnDrawGizmos()
		{
			if (probes == null)
			{
				return;
			}
			Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
			for (int i = 0; i < probes.Length; i++)
			{
				for (int j = i; j < probes.Length; j++)
				{
					RenderingProbe obj = probes[i];
					Gizmos.DrawLine(to: probes[j].transform.position, from: obj.transform.position);
				}
			}
		}
	}
}
