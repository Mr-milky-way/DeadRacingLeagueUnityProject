using System;
using System.Collections.Generic;

namespace drl.network
{
	public class NetworkRoomState
	{
		private Dictionary<NetworkRoom.StateCode, IRoomState> stateMachine = new Dictionary<NetworkRoom.StateCode, IRoomState>();

		private NetworkRoom currentRoom;

		public NetworkRoom.StateCode State { get; private set; }

		public NetworkRoomState(NetworkRoom room)
		{
			currentRoom = room;
		}

		public void ClearAllStates()
		{
			stateMachine.Clear();
			State = NetworkRoom.StateCode.None;
		}

		public void AddState(NetworkRoom.StateCode stateCode, IRoomState state)
		{
			stateMachine.Add(stateCode, state);
		}

		public void SetState(NetworkRoom.StateCode newState)
		{
			if (stateMachine.ContainsKey(newState))
			{
				if (newState != State)
				{
					ExitState();
					State = newState;
					currentRoom.ServerState = State;
					stateMachine[State].OnEnter(currentRoom);
				}
				return;
			}
			throw new Exception("Unregistered Room State: " + newState);
		}

		private void ExitState()
		{
			if (State != NetworkRoom.StateCode.None)
			{
				stateMachine[State].OnExit(currentRoom);
				State = NetworkRoom.StateCode.None;
				currentRoom.ServerState = State;
			}
		}

		public virtual void Update(NetworkRoom room)
		{
			if (State != NetworkRoom.StateCode.None)
			{
				stateMachine[State].OnUpdate(room);
			}
		}
	}
}
