namespace thelab.mvc
{
	public class Model : Element
	{
	}
	public class Model<T> : Model where T : BaseApplication
	{
		public new T app => (T)base.app;
	}
}
