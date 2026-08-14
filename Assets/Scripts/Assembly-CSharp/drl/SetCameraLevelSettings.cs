using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;
using thelab.core;

namespace drl
{
	public class SetCameraLevelSettings : MonoBehaviour
	{
		public LevelSettings settings;

		public DRLResolveHoleCollision holes;

		protected void Awake()
		{
			string text = SceneManager.GetActiveScene().name;
			settings = LevelManager.GetRootComponent<LevelSettings>("level");
			if (!settings)
			{
				Debug.LogWarning("SetTerrainHolesFromLevel> Failed to locate settings [" + text + "]!");
				return;
			}
			holes = GetComponent<DRLResolveHoleCollision>();
			if ((bool)holes)
			{
				holes.entranceTriggers = settings.terrain.holesTriggers;
				holes.terrainColliders = settings.terrain.terrainColliders;
				holes.Initialize();
			}
			else
			{
				Debug.LogWarning("SetTerrainHolesFromLevel> Failed to locate hole collisions!");
			}
			SunShafts sunShafts = Hierarchy.Find<SunShafts>(base.transform, "main");
			if ((bool)sunShafts)
			{
				sunShafts.sunTransform = settings.sunshafts.caster;
			}
		}
	}
}
