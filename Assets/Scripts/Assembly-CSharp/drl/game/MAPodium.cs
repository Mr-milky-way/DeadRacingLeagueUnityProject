using UnityEngine;

namespace drl.game
{
	public class MAPodium : MARenderer
	{
		[SerializeField]
		private int m_index;

		public int index
		{
			get
			{
				return m_index;
			}
			set
			{
				m_index = value;
				Write();
			}
		}

		public new MDPodium data
		{
			get
			{
				return base.data as MDPodium;
			}
			set
			{
				base.data = value;
			}
		}

		public override void Write()
		{
			base.Write();
			MDPodium mDPodium = data;
			if (mDPodium != null)
			{
				mDPodium.index = index;
			}
		}

		public override void Read()
		{
			base.Read();
			if (m_data is MDPodium mDPodium)
			{
				m_index = mDPodium.index;
			}
		}

		protected override MDObject NewData()
		{
			return new MDPodium();
		}
	}
}
