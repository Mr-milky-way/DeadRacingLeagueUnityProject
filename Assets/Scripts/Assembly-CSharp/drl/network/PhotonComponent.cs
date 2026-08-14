using UnityEngine;

namespace drl.network
{
	public class PhotonComponent<T> : MonoBehaviour
	{
		public T Data;

		public virtual void UpdateData(T data)
		{
			Data = data;
		}
	}
}
