using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MEInputLayer : View<DRLApp>
	{
		public void OnUIEvent(UIEvent p_event)
		{
			_ = p_event.target;
			UIEventType type = p_event.type;
			if (type != UIEventType.Move)
			{
				_ = 10;
			}
			Notify("map-editor.input.event", p_event.type, p_event.target);
		}
	}
}
