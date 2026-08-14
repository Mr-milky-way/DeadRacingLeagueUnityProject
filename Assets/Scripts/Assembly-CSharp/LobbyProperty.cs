using UnityEngine;
using drl.network;

[CreateAssetMenu(fileName = "New LobbyProperty", menuName = "ScriptableObjects/Network/LobbyProperty")]
public class LobbyProperty : BaseProperty<Lobby>
{
	protected override void SetDefault()
	{
		m_Value = new Lobby();
	}
}
