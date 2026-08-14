using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace drl.network
{
	public class PhotonDebugGame : MonoBehaviour
	{
		[SerializeField]
		private GameObject droneModel;

		private PhotonService service;

		private List<INetworkObservable> drones = new List<INetworkObservable>();

		private void Awake()
		{
			service = GetComponent<PhotonService>();
			if (service == null)
			{
				Debug.LogError("PhotonDebugHelper can't run without a PhotonService attached to the same GameObject");
				return;
			}
			PhotonService photonService = service;
			photonService.OnNetworkEvent = (Action<PhotonService.EventType, object>)Delegate.Combine(photonService.OnNetworkEvent, new Action<PhotonService.EventType, object>(OnNetworkEvent));
			PhotonService photonService2 = service;
			photonService2.OnGameEvent = (Action<NetworkRoom.GameEvent>)Delegate.Combine(photonService2.OnGameEvent, new Action<NetworkRoom.GameEvent>(OnPhotonRoomEvent));
		}

		private void OnDestroy()
		{
			PhotonService photonService = service;
			photonService.OnNetworkEvent = (Action<PhotonService.EventType, object>)Delegate.Remove(photonService.OnNetworkEvent, new Action<PhotonService.EventType, object>(OnNetworkEvent));
			PhotonService photonService2 = service;
			photonService2.OnGameEvent = (Action<NetworkRoom.GameEvent>)Delegate.Remove(photonService2.OnGameEvent, new Action<NetworkRoom.GameEvent>(OnPhotonRoomEvent));
		}

		private void OnNetworkEvent(PhotonService.EventType eventCode, object content)
		{
			if (eventCode != PhotonService.EventType.OnLeftRoom)
			{
				return;
			}
			foreach (INetworkObservable drone in drones)
			{
				if (drone != null && drone.gameObject != null)
				{
					UnityEngine.Object.Destroy(drone.gameObject);
				}
			}
			drones.Clear();
		}

		private void OnPhotonRoomEvent(NetworkRoom.GameEvent eventData)
		{
			switch (eventData.EventCode)
			{
			case NetworkRoom.GameEventCode.OnLoadLevel:
				Debug.Log("OnLoadGame event");
				StopAllCoroutines();
				StartCoroutine(LoadDebugLevel());
				break;
			case NetworkRoom.GameEventCode.OnGameStart:
			{
				Debug.Log("OnGameStart");
				GameObject obj2 = UnityEngine.Object.Instantiate(droneModel);
				obj2.name = "LocalDrone";
				obj2.AddComponent<PhotonDebugInput>();
				INetworkObservable networkObservable2 = obj2.AddComponent<PhotonDebugDrone>();
				drones.Add(networkObservable2);
				service.CurrentRoom.CreateLocalRacer(networkObservable2).transform.parent = base.transform;
				break;
			}
			case NetworkRoom.GameEventCode.OnPlayerSpawned:
			{
				NetworkActor networkActor = (NetworkActor)eventData.Content;
				if (networkActor != null)
				{
					GameObject obj = UnityEngine.Object.Instantiate(droneModel);
					obj.name = "RemotDrone";
					INetworkObservable networkObservable = obj.AddComponent<PhotonDebugDrone>();
					drones.Add(networkObservable);
					service.CurrentRoom.CreateRemoteRacer(networkActor, networkObservable).transform.parent = base.transform;
				}
				break;
			}
			}
		}

		private IEnumerator LoadDebugLevel()
		{
			Debug.Log("Loading game in... " + UnityEngine.Random.Range(1f, 4f));
			yield return SceneManager.LoadSceneAsync("photon-debug-level");
			service.SendLevelLoaded();
			service.CurrentRoom.Outgoing.SendPlayerReady();
		}
	}
}
