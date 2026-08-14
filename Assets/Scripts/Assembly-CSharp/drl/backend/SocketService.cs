using System;
using drl.game;
using thelab.mvc;

namespace drl.backend
{
	public class SocketService<T> : Controller<DRLApp>
	{
		private string url = DRLService.baseWebsocketUri;

		protected DRLSocket<T> socket = new DRLSocket<T>();

		public virtual void Connect()
		{
			socket.StartConnect(url);
		}

		public virtual void Disconnect()
		{
			socket.StartDisconnect();
		}

		public virtual void On(string p_event, Action<T> p_callback)
		{
			UnsubscribeAll(p_event);
			socket.Subscribe(p_event, delegate(T message)
			{
				if (message == null && p_callback != null)
				{
					p_callback(default(T));
				}
				if (p_callback != null)
				{
					p_callback(message);
				}
			});
		}

		public virtual void Off(string p_event, Action<T> p_callback)
		{
			socket.Unsubscribe(p_event, p_callback);
		}

		public virtual void UnsubscribeAll()
		{
			socket.UnsubscribeAll();
		}

		public virtual void UnsubscribeAll(string p_event)
		{
			socket.UnsubscribeAll(p_event);
		}

		public virtual void OnConnected(Action p_callback)
		{
			socket.Connected = null;
			DRLSocket<T> dRLSocket = socket;
			dRLSocket.Connected = (Action)Delegate.Combine(dRLSocket.Connected, p_callback);
		}

		public virtual void Send(string p_event, T p_data)
		{
			socket.SendMessage(p_event, p_data);
		}

		public virtual bool IsConnected()
		{
			return socket.IsConnected();
		}

		private void Update()
		{
			socket.OnUpdate();
		}

		private void OnApplicationQuit()
		{
			socket.ForceClose();
		}
	}
}
