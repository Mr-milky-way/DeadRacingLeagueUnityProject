using UnityEngine;

namespace drl.network
{
	public class PhotonDebugPlayer
	{
		private static string nickname = "";

		private static string photo = "";

		public static void DrawPlayer(NetworkActor player)
		{
			GUILayout.BeginHorizontal();
			if (player.IsMaster)
			{
				GUILayout.Label("Master ");
			}
			GUILayout.Label("ID: " + player.ID, GUILayout.Width(50f));
			GUILayout.Label("UserId: " + player.UserId, GUILayout.Width(50f));
			player.ProfileName = PhotonDebugHelper.DrawProperty("Name: ", player.ProfileName, 50f);
			player.ProfilePhoto = PhotonDebugHelper.DrawProperty("PhotoURL: ", player.ProfilePhoto);
			GUILayout.Label("Spectate: " + player.IsSpectator, GUILayout.Width(100f));
			GUILayout.EndHorizontal();
		}
	}
}
