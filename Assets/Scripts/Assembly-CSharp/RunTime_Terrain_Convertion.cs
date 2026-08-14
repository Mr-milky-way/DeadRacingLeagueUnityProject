using UnityEngine;
using VacuumShaders.TerrainToMesh;

[AddComponentMenu("VacuumShaders/Terrain To Mesh/Example/Runtime Converter")]
public class RunTime_Terrain_Convertion : MonoBehaviour
{
	public Terrain sourceTerrain;

	public TerrainConvertInfo convertInfo;

	public bool generateBasemap;

	private void Start()
	{
		if (!(sourceTerrain != null))
		{
			return;
		}
		Mesh[] array = TerrainToMeshConverter.Convert(sourceTerrain, convertInfo);
		if (array == null)
		{
			return;
		}
		Material material = null;
		material = ((!generateBasemap) ? GenerateMaterial_Splatmap() : GenerateMaterial_Basemap());
		if (array.Length == 1)
		{
			MeshFilter meshFilter = base.gameObject.GetComponent<MeshFilter>();
			if (meshFilter == null)
			{
				meshFilter = base.gameObject.AddComponent<MeshFilter>();
			}
			meshFilter.sharedMesh = array[0];
			MeshRenderer meshRenderer = base.gameObject.GetComponent<MeshRenderer>();
			if (meshRenderer == null)
			{
				meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
			}
			meshRenderer.sharedMaterial = material;
		}
		else
		{
			for (int i = 0; i < array.Length; i++)
			{
				GameObject obj = new GameObject(array[i].name);
				obj.transform.parent = base.gameObject.transform;
				obj.transform.localPosition = Vector3.zero;
				obj.AddComponent<MeshFilter>().sharedMesh = array[i];
				obj.AddComponent<MeshRenderer>().sharedMaterial = material;
			}
		}
	}

	private Material GenerateMaterial_Basemap()
	{
		Texture2D _diffuseMap = null;
		Texture2D _normalMap = null;
		TerrainToMeshConverter.ExtractBasemap(sourceTerrain, out _diffuseMap, out _normalMap, 1024, 1024);
		Material material = new Material(Shader.Find((_normalMap != null) ? "Legacy Shaders/Bumped Diffuse" : "Legacy Shaders/Diffuse"));
		material.mainTexture = _diffuseMap;
		if (_normalMap != null)
		{
			material.SetTexture("_BumpMap", _normalMap);
		}
		return material;
	}

	private Material GenerateMaterial_Splatmap()
	{
		Material material = null;
		Texture2D[] array = TerrainToMeshConverter.ExtractSplatmaps(sourceTerrain);
		if (array == null || array.Length == 0)
		{
			return material;
		}
		int num = TerrainToMeshConverter.ExtractTexturesInfo(sourceTerrain, out var _diffuseTextures, out var _, out var _uvScale, out var _uvOffset);
		if (num == 0 || _diffuseTextures == null)
		{
			Debug.LogWarning("usedTexturesCount == 0");
			return material;
		}
		if (num == 1)
		{
			Shader shader = Shader.Find("Legacy Shaders/Diffuse");
			if (shader != null)
			{
				material = new Material(shader);
				material.mainTexture = _diffuseTextures[0];
				material.mainTextureScale = _uvScale[0];
				material.mainTextureOffset = _uvOffset[0];
			}
			return material;
		}
		num = Mathf.Clamp(num, 2, 8);
		Shader shader2 = Shader.Find($"VacuumShaders/Terrain To Mesh/One Directional Light/Diffuse/{num} Textures");
		if (shader2 == null)
		{
			Debug.LogWarning("Shader not found: " + $"VacuumShaders/Terrain To Mesh/Standard/Diffuse/{num} Textures");
			return material;
		}
		material = new Material(shader2);
		if (array.Length == 1)
		{
			material.SetTexture("_V_T2M_Control", array[0]);
		}
		else
		{
			if (array.Length > 2)
			{
				Debug.Log("TerrainToMesh shaders support max 2 control textures. Current terrain uses " + array.Length);
			}
			material.SetTexture("_V_T2M_Control", array[0]);
			material.SetTexture("_V_T2M_Control2", array[1]);
		}
		for (int i = 0; i < num; i++)
		{
			material.SetTexture($"_V_T2M_Splat{i + 1}", _diffuseTextures[i]);
			material.SetFloat($"_V_T2M_Splat{i + 1}_uvScale", _uvScale[i].x);
		}
		return material;
	}
}
