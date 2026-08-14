using UnityEngine;

namespace drl.game
{
	public class DRLBuildFlagSettings : ScriptableObject
	{
		public bool IsOffline;

		public bool IsDevelopment = true;

		public bool IsTryouts;

		public bool IsTournaments;

		public bool IsPublicBeta;

		public bool IsEvent0;

		public bool IsEditorFastLoad;
	}
}
