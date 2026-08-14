namespace thelab.mvc
{
	public class HeaderItemView : UIElementView
	{
		public int level;

		public override void Notify(string p_event, params object[] p_data)
		{
			base.Notify(p_event, (object)level);
		}
	}
}
