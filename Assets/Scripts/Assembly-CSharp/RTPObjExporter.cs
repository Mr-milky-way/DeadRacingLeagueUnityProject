using System.IO;
using System.Text;
using UnityEngine;

public class RTPObjExporter
{
	public static string MeshToString(MeshFilter mf)
	{
		Mesh sharedMesh = mf.sharedMesh;
		Material[] sharedMaterials = mf.GetComponent<Renderer>().sharedMaterials;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("g ").Append(mf.name).Append("\n");
		Vector3[] vertices = sharedMesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 vector = vertices[i];
			stringBuilder.Append($"v {0f - vector.x} {vector.y} {vector.z}\n");
		}
		stringBuilder.Append("\n");
		vertices = sharedMesh.normals;
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 vector2 = vertices[i];
			stringBuilder.Append($"vn {0f - vector2.x} {vector2.y} {vector2.z}\n");
		}
		stringBuilder.Append("\n");
		Vector2[] uv = sharedMesh.uv;
		for (int i = 0; i < uv.Length; i++)
		{
			Vector3 vector3 = uv[i];
			stringBuilder.Append($"vt {vector3.x} {vector3.y}\n");
		}
		for (int j = 0; j < sharedMesh.subMeshCount; j++)
		{
			stringBuilder.Append("\n");
			stringBuilder.Append("usemtl ").Append(sharedMaterials[j].name).Append("\n");
			stringBuilder.Append("usemap ").Append(sharedMaterials[j].name).Append("\n");
			int[] triangles = sharedMesh.GetTriangles(j);
			for (int k = 0; k < triangles.Length; k += 3)
			{
				stringBuilder.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", triangles[k + 2] + 1, triangles[k + 1] + 1, triangles[k] + 1));
			}
		}
		return stringBuilder.ToString();
	}

	public static void MeshToFile(MeshFilter mf, string filename)
	{
		using StreamWriter streamWriter = new StreamWriter(filename);
		streamWriter.Write(MeshToString(mf));
	}
}
