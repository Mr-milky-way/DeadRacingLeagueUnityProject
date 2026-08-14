using UnityEngine;
using drl.sim;

public class DebugThrottleCutTest : MonoBehaviour
{
	public float startAltitude;

	public float launchAltitude;

	public float launchTime;

	public float launchSpeed;

	public float recoveryAltitude;

	public float recoveryTime;

	public float recoverySpeed;

	public float distanceToLaunch;

	public float distanceToRecovery;

	public float launchTilt;

	public float thrustFalloff;

	private Vector3 startPos;

	private Vector3 endPos;

	private float startTime;

	public bool launch;

	private bool flying;

	private bool prearm;

	private Drone d;

	public bool manual;

	private void Start()
	{
		d = GetComponent<Drone>();
	}

	private void Update()
	{
		if (d == null)
		{
			d = GetComponent<Drone>();
		}
		if (!flying && manual && !prearm && d.fc.rawSignal.throttle >= 0.99f)
		{
			prearm = true;
		}
		if (!flying && manual && prearm && d.fc.rawSignal.throttle < 0.97f)
		{
			flying = true;
			prearm = false;
			Vector3 vector = new Vector3(0f, launchAltitude - startAltitude, distanceToLaunch);
			Vector3 velocity = d.rigidbody.rb.velocity;
			velocity.y = 0f;
			Debug.LogWarning("manual launch with speed " + launchSpeed + ", from distance it should be " + vector.magnitude / launchTime + ", virtual=" + d.rigidbody.rb.velocity.magnitude + " ground=" + velocity.magnitude);
			vector = d.rigidbody.rb.velocity.normalized * launchSpeed;
			startPos = base.transform.position;
			d.rigidbody.rb.velocity = vector;
			startTime = Time.time;
		}
		if (manual && flying && base.transform.position.y < startPos.y - launchAltitude + recoveryAltitude)
		{
			flying = false;
			endPos = base.transform.position;
			Vector3 vector2 = endPos - startPos;
			endPos.y = 0f;
			startPos.y = 0f;
			Vector3 vector3 = endPos - startPos;
			float num = Time.time - startTime;
			Vector3 velocity2 = d.rigidbody.rb.velocity;
			velocity2.y = 0f;
			Debug.LogError("finished, distance=" + vector2.magnitude + " groundDistance=" + vector3.magnitude + " [" + distanceToRecovery + "] speed=" + d.rigidbody.rb.velocity.magnitude + " (ground=" + velocity2.magnitude + ") [" + recoverySpeed + "] elapsed=" + num + " [" + recoveryTime + "]");
		}
		if (!manual && launch && !flying && !prearm)
		{
			launch = false;
			flying = true;
			prearm = true;
			d.fc.debugThrottle = 1f;
			d.fc.allowThrottle = false;
			base.transform.rotation = Quaternion.identity;
			base.transform.Rotate(launchTilt, 0f, 0f);
			base.transform.position = new Vector3(0f, launchAltitude + 40f, 0f);
			Debug.LogWarning("prearming");
			startTime = Time.time;
		}
		if (!manual && flying && prearm && Time.time - startTime > launchTime)
		{
			Drone component = GetComponent<Drone>();
			prearm = false;
			base.transform.rotation = Quaternion.identity;
			base.transform.Rotate(launchTilt, 0f, 0f);
			base.transform.position = new Vector3(0f, launchAltitude + 40f, 0f);
			Vector3 vector4 = new Vector3(0f, launchAltitude - startAltitude, distanceToLaunch);
			Vector3 velocity3 = component.rigidbody.rb.velocity;
			velocity3.y = 0f;
			Debug.LogWarning("launching with speed " + launchSpeed + ", from distance it should be " + vector4.magnitude / launchTime + ", virtual=" + component.rigidbody.rb.velocity.magnitude + " ground=" + velocity3.magnitude);
			vector4 = vector4.normalized * launchSpeed;
			startPos = base.transform.position;
			component.rigidbody.rb.velocity = vector4;
			startTime = Time.time;
		}
		if (!manual && flying && !prearm)
		{
			if (thrustFalloff <= 0f)
			{
				d.fc.debugThrottle = 0f;
			}
			else
			{
				d.fc.debugThrottle = Mathf.Lerp(1f, 0f, (Time.time - startTime) / thrustFalloff);
			}
			if (base.transform.position.y < recoveryAltitude + 40f)
			{
				d.fc.debugThrottle = 0f;
				d.fc.allowThrottle = true;
				flying = false;
				endPos = base.transform.position;
				Vector3 vector5 = endPos - startPos;
				endPos.y = 0f;
				startPos.y = 0f;
				Vector3 vector6 = endPos - startPos;
				float num2 = Time.time - startTime;
				Vector3 velocity4 = d.rigidbody.rb.velocity;
				velocity4.y = 0f;
				Debug.LogError("finished, distance=" + vector5.magnitude + " groundDistance=" + vector6.magnitude + " [" + distanceToRecovery + "] speed=" + d.rigidbody.rb.velocity.magnitude + " (ground=" + velocity4.magnitude + ") [" + recoverySpeed + "] elapsed=" + num2 + " [" + recoveryTime + "]");
			}
		}
	}
}
