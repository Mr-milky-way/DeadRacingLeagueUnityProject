using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class DroneRigViewerApp : MonoBehaviour
	{
		public DroneFactory factory;

		public OrbitTransform camera;

		public OrbitFollowInput cameraFollow;

		public Transform container;

		public TextAsset[] rigs;

		public float padding;

		public int index;

		public int nextRig;

		public bool createOnAwake;

		public Transform selection;

		public float rotateSpeed = 30f;

		protected bool m_rotateDrone;

		protected void Awake()
		{
			cameraFollow = camera.GetComponent<OrbitFollowInput>();
			factory.library.LoadResource("drone-01-stock", p_async: true, delegate
			{
				Debug.Log("Resource Loaded!");
			});
			if (createOnAwake)
			{
				int num = 0;
				TextAsset[] array = rigs;
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					_ = array[num2];
					bool p_set_camera = num == 0;
					num++;
					CreateNextRig(p_replace: false, p_set_camera);
				}
			}
		}

		protected void CreateNextRig(bool p_replace = false, bool p_set_camera = true)
		{
			if (rigs.Length == 0)
			{
				return;
			}
			DroneRigData droneRigData = ScriptableObject.CreateInstance<DroneRigData>();
			droneRigData.Set(rigs[nextRig].bytes);
			nextRig = (nextRig + 1) % rigs.Length;
			Drone drone = null;
			if ((bool)selection && p_replace)
			{
				drone = selection.GetComponent<Drone>();
			}
			Drone drone2 = (drone ? factory.Replace(droneRigData, drone) : factory.Instantiate(droneRigData, container));
			if ((bool)drone2)
			{
				Vector3 position = container.position;
				int num = container.childCount - 1;
				if (p_replace)
				{
					num = Mathf.Max(0, num - 1);
				}
				position.x += padding * (float)num;
				drone2.position = position;
				drone2.rigidbody.rb.isKinematic = true;
				selection = (p_set_camera ? (cameraFollow.target = drone2.transform) : cameraFollow.target);
			}
		}

		protected void PrevRig()
		{
			int num = (container ? container.childCount : 0);
			if (num > 0)
			{
				index = ((index - 1 >= 0) ? (index - 1) : (num - 1));
				int num2 = Mathf.Clamp(index, 0, num - 1);
				Transform child = container.GetChild(num2);
				selection = (cameraFollow.target = child);
			}
		}

		protected void NextRig()
		{
			int num = (container ? container.childCount : 0);
			if (num > 0)
			{
				index = (index + 1) % num;
				int num2 = Mathf.Clamp(index, 0, num - 1);
				Transform child = container.GetChild(num2);
				selection = (cameraFollow.target = child);
			}
		}

		protected void Update()
		{
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				NextRig();
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				PrevRig();
			}
			if (Input.GetKeyDown(KeyCode.R))
			{
				m_rotateDrone = !m_rotateDrone;
				camera.speed.angle = (m_rotateDrone ? 100 : 2);
			}
			if (Input.GetKeyDown(KeyCode.KeypadPlus))
			{
				rotateSpeed += 10f;
			}
			if (Input.GetKeyDown(KeyCode.KeypadMinus))
			{
				rotateSpeed -= 10f;
			}
			rotateSpeed = Mathf.Clamp(rotateSpeed, -180f, 180f);
			if (m_rotateDrone)
			{
				camera.angle += new Vector2(rotateSpeed * Time.deltaTime, 0f);
			}
		}
	}
}
