using UnityEngine;

public class PSAutoDestruct : MonoBehaviour
{
	private ParticleSystem ps;

	private void Start()
	{
		ps = GetComponent<ParticleSystem>();
		if ((bool)ps && !ps.loop)
		{
			Object.Destroy(base.gameObject, ps.duration + ps.startLifetime);
		}
	}

	public void DestroyPSystem(GameObject p_gp)
	{
		ParticleSystem component = p_gp.GetComponent<ParticleSystem>();
		Object.Destroy(p_gp, component.duration + component.startLifetime);
	}
}
