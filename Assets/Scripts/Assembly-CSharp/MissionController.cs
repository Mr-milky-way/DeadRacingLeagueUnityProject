using System.Linq;
using UnityEngine;
using drl.sim;
using thelab.core;

public class MissionController : MonoBehaviour
{
	public GameObject simulationPrefab;

	public DroneFactory factory;

	public FlowModuleUI ui;

	public bool autoFade = true;

	private void Start()
	{
		GameObject gameObject = Object.Instantiate(simulationPrefab, base.transform);
		gameObject.transform.position = Vector3.zero;
		gameObject.transform.localEulerAngles = Vector3.zero;
		gameObject.name = simulationPrefab.name;
		gameObject.SetActive(value: true);
		SimulationFlowModule simulationFlowModule = Hierarchy.Find<SimulationFlowModule>(gameObject.transform);
		simulationFlowModule.factory = factory;
		simulationFlowModule.ui = ui;
		Flow flow = simulationFlowModule.main;
		if (!flow)
		{
			flow = GetMainFlow(gameObject.transform);
		}
		if ((bool)flow)
		{
			Activity.RunOnce(flow.Run, 1f);
		}
		else
		{
			Debug.LogWarning("MissionController> Flow not found [" + gameObject.name + "]");
		}
		if (autoFade)
		{
			ui.FadeOut(0f);
		}
	}

	private Flow GetMainFlow(Transform simulation)
	{
		return Hierarchy.FindAll<Flow>(simulation).FirstOrDefault((Flow it) => it.name.IndexOf("main") >= 0);
	}
}
