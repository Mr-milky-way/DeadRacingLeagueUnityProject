using UnityEngine;

public class SpawnInArea : MonoBehaviour
{
	public Texture2D SpawnMap;

	private float Offset = 10f;

	private float AboveGround = 1f;

	private bool TerrainOnly = true;

	private void RandomPositionOnTerrain(GameObject obj)
	{
		Vector3 size = Terrain.activeTerrain.terrainData.size;
		Vector3 vector = default(Vector3);
		bool flag = false;
		while (!flag)
		{
			vector = Terrain.activeTerrain.transform.position;
			float num = Random.Range(0f, size.x);
			float num2 = Random.Range(0f, size.z);
			vector.x += num;
			vector.y += size.y + Offset;
			vector.z += num2;
			if ((bool)SpawnMap)
			{
				int x = Mathf.RoundToInt((float)SpawnMap.width * num / size.x);
				int y = Mathf.RoundToInt((float)SpawnMap.height * num2 / size.z);
				float grayscale = SpawnMap.GetPixel(x, y).grayscale;
				flag = ((grayscale > 0f && Random.Range(0f, 1f) < grayscale) ? true : false);
			}
			else
			{
				flag = true;
			}
			if (!flag)
			{
				continue;
			}
			if (Physics.Raycast(vector, -Vector3.up, out var hitInfo))
			{
				float distance = hitInfo.distance;
				if (hitInfo.transform.name != "Terrain" && TerrainOnly)
				{
					flag = false;
				}
				vector.y -= distance - AboveGround;
			}
			else
			{
				flag = false;
			}
		}
		obj.transform.position = vector;
		base.transform.Rotate(Vector3.up * Random.Range(0, 360), Space.World);
	}
}
