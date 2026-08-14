using UnityEngine;
using UnityEngine.AI;

namespace drl
{
	public class TrackSettings : MonoBehaviour
	{
		public NavMeshSurface[] navmeshes;

		public NavMeshSurface[] navmeshesClean;

		public LevelSettings.Scene scene;
	}
}
