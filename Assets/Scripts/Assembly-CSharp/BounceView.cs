using thelab.mvc;

public class BounceView : View<BounceApplication>
{
	private BallView m_ball;

	private TimerView m_timer;

	public BallView ball => m_ball = Assert(m_ball);

	public TimerView timer => m_timer = Assert(m_timer);
}
