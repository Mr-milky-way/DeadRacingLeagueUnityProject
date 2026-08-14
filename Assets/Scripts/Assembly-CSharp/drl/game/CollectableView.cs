using System;
using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class CollectableView : View<DRLApp>
	{
		public MACollectable collectable;

		public Transform models;

		public List<Renderer> modelRenderers;

		public List<Material> modelMaterialsOriginal;

		public List<Material> modelMaterialsUniques;

		public Transform collisions;

		public Transform effects;

		public List<ParticleSystem> effectList;

		public List<Vector3> effectListStarts;

		public GameCollectableController controller;

		private float m_effect_cutout;

		private bool m_effect_active;

		public float effectCutoutDuration = 0.3f;

		public float effectCameraFollowDuration = 0.33f;

		public float evaluateStartTime;

		private ColliderEvent m_event;

		public bool triggerStay;

		public bool isDestroyed;

		private Activity m_effect_camera_loop;

		private bool m_will_destroy;

		private bool m_is_evaluate_destroy;

		public float effectCutout
		{
			get
			{
				return m_effect_cutout;
			}
			set
			{
				float num = Mathf.Clamp01(value);
				if (Mathf.Abs(num - m_effect_cutout) <= 0.005f)
				{
					return;
				}
				if (num > 0f)
				{
					if (!m_effect_active)
					{
						m_effect_active = true;
						SetMaterialMode(p_unique: true);
					}
				}
				else if (m_effect_active)
				{
					m_effect_active = false;
					SetMaterialMode(p_unique: false);
				}
				m_effect_cutout = num;
				for (int i = 0; i < modelMaterialsUniques.Count; i++)
				{
					modelMaterialsUniques[i].SetFloat("_Cutoff", m_effect_cutout);
				}
			}
		}

		private void Start()
		{
			controller = UnityEngine.Object.FindObjectOfType<GameCollectableController>();
			collectable = GetComponentInParent<MACollectable>();
			Transform child = collectable.transform.GetChild(0);
			models = child.Find("lods");
			modelRenderers = (models ? Hierarchy.FindAll<Renderer>(models) : new List<Renderer>());
			modelMaterialsOriginal = new List<Material>();
			modelMaterialsUniques = new List<Material>();
			for (int i = 0; i < modelRenderers.Count; i++)
			{
				Material[] sharedMaterials = modelRenderers[i].sharedMaterials;
				foreach (Material item in sharedMaterials)
				{
					if (!modelMaterialsOriginal.Contains(item))
					{
						modelMaterialsOriginal.Add(item);
					}
				}
			}
			for (int k = 0; k < modelMaterialsOriginal.Count; k++)
			{
				Material original = modelMaterialsOriginal[k];
				original = UnityEngine.Object.Instantiate(original);
				original.name = "$" + original.name.Replace("(Clone)", "").Trim();
				modelMaterialsUniques.Add(original);
			}
			collisions = child.Find("collisions");
			List<Collider> list = (collisions ? Hierarchy.FindAll<Collider>(collisions) : new List<Collider>());
			for (int l = 0; l < list.Count; l++)
			{
				Collider collider = list[l];
				collider.isTrigger = true;
				if (!collider.gameObject.GetComponent<ColliderEventComponent>())
				{
					ColliderEventComponent colliderEventComponent = collider.gameObject.AddComponent<ColliderEventComponent>();
					colliderEventComponent.mask = ColliderEvent.Type.Enter;
					colliderEventComponent.callback.AddListener(OnColliderEvent);
				}
			}
			effects = child.Find("particles");
			effectList = (effects ? Hierarchy.FindAll<ParticleSystem>(effects) : new List<ParticleSystem>());
			effectListStarts = new List<Vector3>();
			for (int m = 0; m < effectList.Count; m++)
			{
				ParticleSystem particleSystem = effectList[m];
				particleSystem.randomSeed = (uint)((m + 1) * 10);
				ParticleSystem.MainModule main = effectList[m].main;
				main.playOnAwake = false;
				effectListStarts.Add(particleSystem.transform.localPosition);
			}
			m_event = new ColliderEvent();
		}

		public void Destroy()
		{
			if (!isDestroyed)
			{
				isDestroyed = true;
				float num = effectCutoutDuration;
				Tween.Add(this, "effectCutout", 1f, num, Cubic.Out);
				Activity.RunOnce(delegate
				{
					models.transform.position += Vector3.up * 2000f;
				}, num + 1f / 30f);
				collisions.transform.position += Vector3.up * 2000f;
			}
		}

		public void Restore()
		{
			if (isDestroyed)
			{
				isDestroyed = false;
				effectCutout = 0f;
				models.transform.position -= Vector3.up * 2000f;
				collisions.transform.position -= Vector3.up * 2000f;
				if ((bool)effects)
				{
					effects.gameObject.SetActive(value: false);
				}
				if (m_effect_camera_loop != null)
				{
					m_effect_camera_loop.Stop();
				}
			}
		}

		protected void EvaluateRestore()
		{
			effectCutout = 0f;
			if (m_is_evaluate_destroy)
			{
				m_is_evaluate_destroy = false;
				m_will_destroy = false;
				if ((bool)effects)
				{
					effects.gameObject.SetActive(value: false);
				}
				for (int i = 0; i < effectList.Count; i++)
				{
					effectList[i].transform.localPosition = effectListStarts[i];
				}
				models.transform.position -= Vector3.up * 2000f;
			}
		}

		protected void EvaluateDestroy()
		{
			if (!m_is_evaluate_destroy)
			{
				m_is_evaluate_destroy = true;
				if ((bool)effects)
				{
					effects.gameObject.SetActive(value: true);
				}
				models.transform.position += Vector3.up * 2000f;
			}
		}

		public void SetMaterialMode(bool p_unique)
		{
			List<Material> list = (p_unique ? modelMaterialsOriginal : modelMaterialsUniques);
			List<Material> list2 = (p_unique ? modelMaterialsUniques : modelMaterialsOriginal);
			for (int i = 0; i < modelRenderers.Count; i++)
			{
				Renderer renderer = modelRenderers[i];
				Material[] sharedMaterials = renderer.sharedMaterials;
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					Material item = sharedMaterials[j];
					int num = list.IndexOf(item);
					if (num >= 0)
					{
						sharedMaterials[j] = list2[num];
					}
				}
				renderer.sharedMaterials = sharedMaterials;
			}
		}

		[ContextMenu("Play Effects")]
		public void PlayEffects()
		{
			if ((bool)effects)
			{
				effects.gameObject.SetActive(value: true);
			}
			Activity.RunOnce(delegate
			{
				for (int i = 0; i < effectList.Count; i++)
				{
					effectList[i].transform.localPosition = effectListStarts[i];
					effectList[i].Play(withChildren: true);
				}
			}, 1f / 60f);
		}

		public void PlayEffects(Vector3 p_position)
		{
			if ((bool)effects)
			{
				effects.gameObject.SetActive(value: true);
			}
			for (int i = 0; i < effectList.Count; i++)
			{
				effectList[i].transform.position = p_position;
				effectList[i].Play(withChildren: true);
			}
		}

		public void PlayEffects(Camera p_camera)
		{
			if (m_effect_camera_loop != null)
			{
				m_effect_camera_loop.Stop();
			}
			if ((bool)effects)
			{
				effects.gameObject.SetActive(value: true);
			}
			Camera c = p_camera;
			Vector3 position = base.transform.position;
			Vector3 vpos = c.WorldToViewportPoint(position);
			float num = 0.2f;
			vpos.x = Mathf.Clamp(vpos.x, num, 1f - num);
			vpos.y = Mathf.Clamp(vpos.y, num, 1f - num);
			vpos.z = 0.8f + Mathf.Lerp(-0.1f, 0.1f, UnityEngine.Random.value);
			bool is_first = true;
			m_effect_camera_loop = Activity.Run((Func<bool>)delegate
			{
				float num2 = ((m_effect_camera_loop == null) ? 0f : m_effect_camera_loop.elapsed);
				if (!c)
				{
					return false;
				}
				bool flag = true;
				if (num2 > 1f)
				{
					flag = false;
				}
				for (int i = 0; i < effectList.Count; i++)
				{
					ParticleSystem particleSystem = effectList[i];
					Vector3 vector = c.ViewportToWorldPoint(vpos);
					Vector3 vector2 = (effects ? particleSystem.transform.TransformPoint(effectListStarts[i]) : vector);
					float f = Mathf.Clamp01(num2 / effectCameraFollowDuration);
					particleSystem.transform.position = ((!flag) ? vector2 : Vector3.Lerp(vector, vector2, Mathf.Pow(f, 1.5f)));
					if (is_first)
					{
						effectList[i].Play(withChildren: true);
						is_first = false;
					}
				}
				return flag;
			}, 0f, false);
			m_effect_camera_loop.late = true;
		}

		public bool Evaluate(float p_time)
		{
			if (evaluateStartTime < 0f)
			{
				return false;
			}
			float num = p_time - evaluateStartTime;
			if (num < 0f)
			{
				EvaluateRestore();
				return false;
			}
			bool result = false;
			if (!m_will_destroy)
			{
				result = (m_will_destroy = true);
			}
			float num2 = Mathf.Clamp01(num / effectCutoutDuration);
			if (num2 >= 1f)
			{
				EvaluateDestroy();
			}
			else
			{
				EvaluateRestore();
			}
			effectCutout = num2;
			for (int i = 0; i < effectList.Count; i++)
			{
				ParticleSystem particleSystem = effectList[i];
				if ((bool)particleSystem)
				{
					particleSystem.Simulate(num, withChildren: true, restart: true);
				}
			}
			return result;
		}

		public void OnColliderEvent(ColliderEvent p_event)
		{
			if (!isDestroyed)
			{
				Drone p_drone = Hierarchy.FindReverse<Drone>(p_event.collider.transform);
				controller.OnCollectableEvent(p_event, this, p_drone);
			}
		}

		protected void OnDestroy()
		{
			if (modelMaterialsUniques != null)
			{
				for (int i = 0; i < modelMaterialsUniques.Count; i++)
				{
					UnityEngine.Object.Destroy(modelMaterialsUniques[i]);
				}
			}
			if (m_effect_camera_loop != null)
			{
				m_effect_camera_loop.Stop();
			}
		}
	}
}
