using drl.game;
using thelab.mvc;

public class ChatView : View<DRLApp>
{
	protected void Awake()
	{
	}

	public void OnPersistency()
	{
		base.app.view.chat = this;
	}
}
