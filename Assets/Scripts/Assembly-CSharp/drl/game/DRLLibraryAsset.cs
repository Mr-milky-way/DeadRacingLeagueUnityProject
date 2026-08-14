using System;
using UnityEngine;

namespace drl.game
{
	public class DRLLibraryAsset : MonoBehaviour
	{
		public bool removeItem;

		public bool available = true;

		public bool inDevelopment;

		public bool inventoryOnly;

		[NonSerialized]
		public bool isPromo;
	}
}
