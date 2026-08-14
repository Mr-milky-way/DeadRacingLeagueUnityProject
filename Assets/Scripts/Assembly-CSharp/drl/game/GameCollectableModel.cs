using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using thelab.mvc;

namespace drl.game
{
	public class GameCollectableModel : Model<DRLApp>
	{
		[Header("Drone")]
		public TextAsset defaultRig;

		private DroneRigData m_rig;

		public Drone playerDrone;

		[Header("Game")]
		public RaceStatusType status;

		public bool stopTimeOnPause = true;

		public bool gameActive;

		public bool gameComplete;

		public bool countActive;

		public int score;

		public int total;

		public float time;

		public int crashes;

		public float topSpeed;

		public float distanceTraveled;

		[Header("Track Assets")]
		public List<CollectableView> trackCollectables;

		[SerializeField]
		private Transform particlesParent;

		[SerializeField]
		private ParticleSystem collectableFX;

		[SerializeField]
		private int collectableFXPoolSize;

		[SerializeField]
		private int collectableFXIndex;

		[SerializeField]
		private ParticleSystem bogeyFX;

		[SerializeField]
		private int bogeyFXPoolSize;

		[SerializeField]
		private int bogeyFXIndex;

		public List<ParticleSystem> collectableFXPool;

		public List<ParticleSystem> bogeyFXPool;

		public GameCollectableController controller => AssertLocal<GameCollectableController>("controller");

		public DroneRigData rig
		{
			get
			{
				if (m_rig != null)
				{
					return m_rig;
				}
				if (!defaultRig)
				{
					return m_rig;
				}
				DroneRigData droneRigData = ScriptableObject.CreateInstance<DroneRigData>();
				droneRigData.Set(defaultRig.bytes);
				return m_rig = droneRigData;
			}
		}

		public void UpdateScore(int p_score)
		{
			score += p_score;
		}

		public void ResetScore()
		{
			score = 0;
		}

		public void UpdateTotal(int p_total)
		{
			total = p_total;
		}

		public void ResetTotal()
		{
			total = 0;
		}

		public void ResetTime()
		{
			time = 0f;
		}

		public void BuildTrack(MAEntity p_root)
		{
			if (!p_root)
			{
				Debug.LogError("GameCollectableModel> BuildTrack / Track Root is <null>");
				return;
			}
			List<MACollectable> sortedCollectables = p_root.GetSortedCollectables();
			Debug.Log($"GameCollectableModel> BuildTrack / Found {sortedCollectables.Count} Collectables");
			int num = 0;
			for (int i = 0; i < sortedCollectables.Count; i++)
			{
				MACollectable mACollectable = sortedCollectables[i];
				CollectableView collectableView = mACollectable.collider.gameObject.AddComponent<CollectableView>();
				collectableView.collectable = mACollectable;
				collectableView.collectable.index = i;
				collectableView.controller = controller;
				trackCollectables.Add(collectableView);
				if (sortedCollectables[i].collectableMode != MapCollectableMode.Kill)
				{
					num++;
				}
			}
			UpdateTotal(num);
		}

		public void InitializeCollectableFXPool()
		{
			for (int i = 0; i < collectableFXPoolSize; i++)
			{
				ParticleSystem particleSystem = Object.Instantiate(collectableFX, particlesParent);
				particleSystem.name = "bp-balloon-collectable-particle-" + i;
				collectableFXPool.Add(particleSystem);
			}
		}

		public void InitializeBogeyFXPool()
		{
			for (int i = 0; i < bogeyFXPoolSize; i++)
			{
				ParticleSystem particleSystem = Object.Instantiate(bogeyFX, particlesParent);
				particleSystem.name = "bp-balloon-bogey-particle-" + i;
				bogeyFXPool.Add(particleSystem);
			}
		}

		public void RestoreAll()
		{
			for (int i = 0; i < trackCollectables.Count; i++)
			{
				trackCollectables[i].Restore();
			}
		}

		public ParticleSystem GetCollectablePoolParticle()
		{
			IncrementCollectableFXPoolIndex();
			return collectableFXPool[collectableFXIndex];
		}

		private void IncrementCollectableFXPoolIndex()
		{
			collectableFXIndex++;
			if (collectableFXIndex >= collectableFXPool.Count)
			{
				collectableFXIndex = 0;
			}
		}

		public ParticleSystem GetBogeyPoolParticle()
		{
			IncrementBogeyFXPoolIndex();
			return bogeyFXPool[bogeyFXIndex];
		}

		private void IncrementBogeyFXPoolIndex()
		{
			bogeyFXIndex++;
			if (bogeyFXIndex >= bogeyFXPool.Count)
			{
				bogeyFXIndex = 0;
			}
		}

		public void SetPoolParticle(ParticleSystem p_particle)
		{
			p_particle.gameObject.SetActive(value: true);
		}

		public bool IsCollectableComplete()
		{
			if (score != total)
			{
				return false;
			}
			return true;
		}
	}
}
