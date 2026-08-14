using UnityEngine;
using drl.game;
using thelab.mvc;

public class ChatController : Controller<DRLApp>
{
	public ChatModel model => AssertLocal<ChatModel>("model");

	public override void OnNotification(string p_event, Object p_target, params object[] p_data)
	{
		switch (p_event)
		{
		case "boot@complete":
			model.TryConnect();
			VerifyTournamentChannels();
			break;
		case "settings.profile.color@changed":
		{
			Color userColor = (Color)p_data[0];
			model.service.UserColor = userColor;
			break;
		}
		case "missions.dmv.rank.updated":
		{
			PlayerStateModel player = base.app.model.storage.state.player;
			model.UpdateUserData(player);
			break;
		}
		case "tournament.brackets.open":
			VerifyTournamentChannels();
			break;
		case "network.room.invite":
		{
			CloudRegionCode cloudRegionCode = (CloudRegionCode)p_data[0];
			string text = (string)p_data[1];
			bool isQuickMatch = (bool)p_data[2];
			string roomName = (string)p_data[3];
			bool isRace = (bool)p_data[4];
			bool isCrossplay = (bool)p_data[5];
			string inviteBody = $"This is a test Invite for region: {cloudRegionCode} and roomId: {text}";
			model.SendGameInvite(cloudRegionCode, text, roomName, isRace, inviteBody, isQuickMatch, isCrossplay);
			break;
		}
		}
	}

	public void OnPersistency()
	{
		base.app.controller.chat = this;
	}

	private void VerifyTournamentChannels()
	{
	}
}
