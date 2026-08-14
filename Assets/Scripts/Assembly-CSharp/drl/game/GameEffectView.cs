using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class GameEffectView : View<DRLApp>
	{
		public List<GameEffectData> list;

		public List<GameObject> pool;

		public int poolCount;

		public GameEffectData Get(GameEffectTypeFlag p_flag)
		{
			return list.Find((GameEffectData it) => it.type == p_flag);
		}

		public void Warmup(int p_count)
		{
			int c = poolCount;
			Activity.Run((Func<bool>)delegate
			{
				if (c >= p_count)
				{
					return false;
				}
				for (int i = 0; i < list.Count; i++)
				{
					GameEffectData gameEffectData = list[i];
					if ((bool)gameEffectData.effect)
					{
						CreatePool(gameEffectData.effect);
					}
				}
				c++;
				poolCount = c;
				return true;
			}, 0f, false);
		}

		public GameObject FindPool(string p_name)
		{
			return pool.Find((GameObject it) => it.name == p_name);
		}

		public void CreatePool(GameObject p_template)
		{
			if ((bool)p_template)
			{
				string text = p_template.name;
				GameObject gameObject = UnityEngine.Object.Instantiate(p_template);
				gameObject.name = text;
				gameObject.transform.SetParent(base.transform);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.SetActive(value: false);
				pool.Add(gameObject);
			}
		}

		public void CreatePool(string p_template_name)
		{
			GameEffectData gameEffectData = list.Find((GameEffectData item) => (bool)item.effect && item.effect.name == p_template_name);
			if (gameEffectData != null)
			{
				CreatePool(gameEffectData.effect);
			}
		}

		public GameObject GetEffect(string p_name)
		{
			Debug.Log("GameEffectView: " + p_name);
			if (string.IsNullOrEmpty(p_name))
			{
				return null;
			}
			GameObject gameObject = FindPool(p_name);
			if (!gameObject)
			{
				CreatePool(p_name);
			}
			gameObject = FindPool(p_name);
			if (!gameObject)
			{
				return null;
			}
			if (pool.Contains(gameObject))
			{
				pool.Remove(gameObject);
			}
			gameObject.SetActive(value: true);
			return gameObject;
		}

		public void SetEffect(GameObject p_effect)
		{
			if ((bool)p_effect)
			{
				p_effect.SetActive(value: false);
				if (!pool.Contains(p_effect))
				{
					pool.Add(p_effect);
				}
			}
		}

		public GameObject PlayEffectParticle(string p_name, GameObject p_target)
		{
			GameObject effect = GetEffect(p_name);
			if (!effect)
			{
				Debug.LogWarning("GameEffectView> PlayEffectParticle: Unable to get effect (GetEffect)");
				return null;
			}
			ParticleSystem ps = effect.GetComponent<ParticleSystem>();
			if (!ps)
			{
				Debug.LogWarning("GameEffectView> PlayEffectParticle: Unable to get ParticleSystem component");
				return null;
			}
			ps.transform.position = p_target.transform.position;
			ps.Play();
			Activity.Run(delegate(float t)
			{
				bool flag = false;
				if (t >= 5f)
				{
					flag = true;
				}
				if (!ps.IsAlive())
				{
					flag = true;
				}
				if (!flag)
				{
					return true;
				}
				SetEffect(ps.gameObject);
				return false;
			});
			return effect;
		}
	}
}
