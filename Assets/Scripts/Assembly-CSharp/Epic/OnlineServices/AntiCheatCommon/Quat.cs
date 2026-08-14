namespace Epic.OnlineServices.AntiCheatCommon
{
	public class Quat : ISettable
	{
		public float w { get; set; }

		public float x { get; set; }

		public float y { get; set; }

		public float z { get; set; }

		internal void Set(QuatInternal? other)
		{
			if (other.HasValue)
			{
				w = other.Value.w;
				x = other.Value.x;
				y = other.Value.y;
				z = other.Value.z;
			}
		}

		public void Set(object other)
		{
			Set(other as QuatInternal?);
		}
	}
}
