using drl.sim.rci;
using thelab.core;

namespace drl.game
{
	public class DRLOrbitWASDInput : OrbitWASDInput
	{
		public bool useUnityInput;

		protected override float GetAxis(int p_id)
		{
			if (!RCI.HasNavigationController)
			{
				return 0f;
			}
			return p_id switch
			{
				0 => RCI.GetRawAxis(RawAxis.LeftStickX, RCI.navigationController), 
				1 => 0f - RCI.GetRawAxis(RawAxis.LeftStickY, RCI.navigationController), 
				2 => RCI.GetRawAxis(RawAxis.RightStickX, RCI.navigationController), 
				3 => RCI.GetRawAxis(RawAxis.RightStickY, RCI.navigationController), 
				_ => 0f, 
			};
		}
	}
}
