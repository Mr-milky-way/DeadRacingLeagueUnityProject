namespace Epic.OnlineServices.AntiCheatCommon
{
	public class Vec3f : ISettable
	{
		public float x { get; set; }

		public float y { get; set; }

		public float z { get; set; }

		internal void Set(Vec3fInternal? other)
		{
			if (other.HasValue)
			{
				x = other.Value.x;
				y = other.Value.y;
				z = other.Value.z;
			}
		}

		public void Set(object other)
		{
			Set(other as Vec3fInternal?);
		}
	}
}
