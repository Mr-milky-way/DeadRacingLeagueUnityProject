using System;
using System.Reflection;
using UnityEngine;

[AddComponentMenu("Relief Terrain/Engine - Terrain or Mesh")]
[ExecuteInEditMode]
public class ReliefTerrain : MonoBehaviour
{
	public Texture2D controlA;

	public Texture2D controlB;

	public Texture2D controlC;

	public string save_path_controlA = "";

	public string save_path_controlB = "";

	public string save_path_controlC = "";

	public string save_path_colormap = "";

	public string save_path_BumpGlobalCombined = "";

	public string save_path_WetMask = "";

	public Texture2D NormalGlobal;

	public Texture2D TreesGlobal;

	public Texture2D ColorGlobal;

	public Texture2D AmbientEmissiveMap;

	public Texture2D BumpGlobalCombined;

	public Texture2D TERRAIN_WetMask;

	public Texture2D tmp_globalColorMap;

	public Texture2D tmp_CombinedMap;

	public Texture2D tmp_WaterMap;

	public bool globalColorModifed_flag;

	public bool globalCombinedModifed_flag;

	public bool globalWaterModifed_flag;

	public bool splat_layer_ordered_mode;

	public RTPColorChannels[] source_controls_channels;

	public int[] splat_layer_seq;

	public float[] splat_layer_boost;

	public bool[] splat_layer_calc;

	public bool[] splat_layer_masked;

	public RTPColorChannels[] source_controls_mask_channels;

	public Texture2D[] source_controls;

	public bool[] source_controls_invert;

	public Texture2D[] source_controls_mask;

	public bool[] source_controls_mask_invert;

	public Vector2 customTiling = new Vector2(3f, 3f);

	[SerializeField]
	public ReliefTerrainPresetHolder[] presetHolders;

	[SerializeField]
	public ReliefTerrainGlobalSettingsHolder globalSettingsHolder;

	public void GetGlobalSettingsHolder()
	{
		if (globalSettingsHolder != null)
		{
			return;
		}
		ReliefTerrain[] array = (ReliefTerrain[])UnityEngine.Object.FindObjectsOfType(typeof(ReliefTerrain));
		bool flag = GetComponent(typeof(Terrain));
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].transform.parent == base.transform.parent && array[i].globalSettingsHolder != null && ((flag && array[i].GetComponent(typeof(Terrain)) != null) || (!flag && array[i].GetComponent(typeof(Terrain)) == null)))
			{
				globalSettingsHolder = array[i].globalSettingsHolder;
				if ((bool)globalSettingsHolder.Get_RTP_LODmanagerScript() && !globalSettingsHolder.Get_RTP_LODmanagerScript().RTP_WETNESS_FIRST && !globalSettingsHolder.Get_RTP_LODmanagerScript().RTP_WETNESS_ADD)
				{
					BumpGlobalCombined = array[i].BumpGlobalCombined;
					globalCombinedModifed_flag = false;
				}
				break;
			}
		}
		if (globalSettingsHolder == null)
		{
			globalSettingsHolder = new ReliefTerrainGlobalSettingsHolder();
			if (flag)
			{
				globalSettingsHolder.numTiles = 0;
				Terrain terrain = (Terrain)GetComponent(typeof(Terrain));
				globalSettingsHolder.terrainLayers = new TerrainLayer[terrain.terrainData.terrainLayers.Length];
				Array.Copy(terrain.terrainData.terrainLayers, globalSettingsHolder.terrainLayers, globalSettingsHolder.terrainLayers.Length);
				globalSettingsHolder.splats = new Texture2D[terrain.terrainData.terrainLayers.Length];
				globalSettingsHolder.Bumps = new Texture2D[terrain.terrainData.terrainLayers.Length];
				globalSettingsHolder.terrainLayers = terrain.terrainData.terrainLayers;
				for (int j = 0; j < terrain.terrainData.terrainLayers.Length; j++)
				{
					globalSettingsHolder.splats[j] = terrain.terrainData.terrainLayers[j].diffuseTexture;
					globalSettingsHolder.Bumps[j] = terrain.terrainData.terrainLayers[j].normalMapTexture;
				}
			}
			else
			{
				globalSettingsHolder.splats = new Texture2D[4];
			}
			globalSettingsHolder.numLayers = globalSettingsHolder.splats.Length;
			globalSettingsHolder.ReturnToDefaults();
		}
		else if (flag)
		{
			GetSplatsFromGlobalSettingsHolder();
		}
		source_controls_mask = new Texture2D[12];
		source_controls = new Texture2D[12];
		source_controls_channels = new RTPColorChannels[12];
		source_controls_mask_channels = new RTPColorChannels[12];
		splat_layer_seq = new int[12]
		{
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
			10, 11
		};
		splat_layer_boost = new float[12]
		{
			1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,
			1f, 1f
		};
		splat_layer_calc = new bool[12];
		splat_layer_masked = new bool[12];
		source_controls_invert = new bool[12];
		source_controls_mask_invert = new bool[12];
		if (flag)
		{
			globalSettingsHolder.numTiles++;
		}
	}

	private void GetSplatsFromGlobalSettingsHolder()
	{
		Terrain terrain = (Terrain)GetComponent(typeof(Terrain));
		if (globalSettingsHolder.terrainLayers != null && globalSettingsHolder.terrainLayers.Length == globalSettingsHolder.numLayers && globalSettingsHolder.terrainLayers.Length != 0 && globalSettingsHolder.terrainLayers[0] != null)
		{
			if (terrain.terrainData.terrainLayers.Length != 0 && terrain.terrainData.terrainLayers[0] == null)
			{
				TerrainLayer[] array = new TerrainLayer[globalSettingsHolder.numLayers];
				Array.Copy(globalSettingsHolder.terrainLayers, array, globalSettingsHolder.terrainLayers.Length);
				terrain.terrainData.terrainLayers = array;
			}
			return;
		}
		TerrainLayer[] array2 = new TerrainLayer[globalSettingsHolder.numLayers];
		ReliefTerrain[] array3 = (ReliefTerrain[])UnityEngine.Object.FindObjectsOfType(typeof(ReliefTerrain));
		bool flag = false;
		for (int i = 0; i < array3.Length; i++)
		{
			Terrain component = array3[i].GetComponent<Terrain>();
			if (component != null && (array3.Length == 1 || array3[i] != this) && component.terrainData.terrainLayers.Length == array2.Length)
			{
				globalSettingsHolder.terrainLayers = new TerrainLayer[component.terrainData.terrainLayers.Length];
				Array.Copy(component.terrainData.terrainLayers, globalSettingsHolder.terrainLayers, globalSettingsHolder.terrainLayers.Length);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Debug.LogWarning("TerrainLayers from GlobalSettingsHolder can't be found. Create a set of layers and setup first terrain before adding RTP script to it!");
			for (int j = 0; j < globalSettingsHolder.numLayers; j++)
			{
				array2[j] = new TerrainLayer();
				array2[j].tileSize = Vector2.one;
				array2[j].tileOffset = new Vector2(1f / customTiling.x, 1f / customTiling.y);
				array2[j].diffuseTexture = globalSettingsHolder.splats[j];
				array2[j].normalMapTexture = globalSettingsHolder.Bumps[j];
			}
		}
		terrain.terrainData.terrainLayers = array2;
	}

	public void InitTerrainTileSizes()
	{
		Terrain terrain = (Terrain)GetComponent(typeof(Terrain));
		if ((bool)terrain)
		{
			globalSettingsHolder.terrainTileSize = terrain.terrainData.size;
			return;
		}
		globalSettingsHolder.terrainTileSize = GetComponent<Renderer>().bounds.size;
		globalSettingsHolder.terrainTileSize.y = globalSettingsHolder.tessHeight;
	}

	private void Awake()
	{
		UpdateBasemapDistance(apply_material_if_applicable: false);
		RefreshTextures();
	}

	public void InitArrays()
	{
		RefreshTextures();
	}

	private void UpdateBasemapDistance(bool apply_material_if_applicable)
	{
		Terrain terrain = (Terrain)GetComponent(typeof(Terrain));
		if (!terrain || globalSettingsHolder == null)
		{
			return;
		}
		terrain.basemapDistance = globalSettingsHolder.distance_start + globalSettingsHolder.distance_transition;
		if (apply_material_if_applicable)
		{
			bool flag = false;
			if (terrain.materialTemplate == null || flag)
			{
				Shader shader = Shader.Find("Relief Pack/ReliefTerrain-FirstPass");
				if ((bool)shader)
				{
					Material material = new Material(shader);
					material.name = base.gameObject.name + " material";
					terrain.materialTemplate = material;
				}
			}
			else
			{
				Material materialTemplate = terrain.materialTemplate;
				terrain.materialTemplate = null;
				terrain.materialTemplate = materialTemplate;
			}
		}
		if (globalSettingsHolder != null && globalSettingsHolder._RTP_LODmanagerScript != null && globalSettingsHolder._RTP_LODmanagerScript.numLayersProcessedByFarShader != globalSettingsHolder.numLayers)
		{
			terrain.basemapDistance = 500000f;
		}
		globalSettingsHolder.Refresh(terrain.materialTemplate);
	}

	public void RefreshTextures(Material mat = null, bool check_weak_references = false)
	{
		GetGlobalSettingsHolder();
		InitTerrainTileSizes();
		if (globalSettingsHolder != null && BumpGlobalCombined != null)
		{
			globalSettingsHolder.BumpGlobalCombinedSize = BumpGlobalCombined.width;
		}
		UpdateBasemapDistance(apply_material_if_applicable: true);
		Terrain terrain = (Terrain)GetComponent(typeof(Terrain));
		globalSettingsHolder.use_mat = mat;
		if (!terrain && !mat)
		{
			if (GetComponent<Renderer>().sharedMaterial == null || GetComponent<Renderer>().sharedMaterial.name != "RTPMaterial")
			{
				GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Relief Pack/Terrain2Geometry"));
				GetComponent<Renderer>().sharedMaterial.name = "RTPMaterial";
			}
			globalSettingsHolder.use_mat = GetComponent<Renderer>().sharedMaterial;
		}
		if ((bool)terrain && terrain.materialTemplate != null)
		{
			globalSettingsHolder.use_mat = terrain.materialTemplate;
			terrain.materialTemplate.SetVector("RTP_CustomTiling", new Vector4(1f / customTiling.x, 1f / customTiling.y, 0f, 0f));
		}
		globalSettingsHolder.use_mat = null;
		RefreshControlMaps(mat);
		if ((bool)mat)
		{
			mat.SetVector("RTP_CustomTiling", new Vector4(1f / customTiling.x, 1f / customTiling.y, 0f, 0f));
		}
	}

	public void RefreshControlMaps(Material mat = null)
	{
		globalSettingsHolder.use_mat = mat;
		Terrain terrain = (Terrain)GetComponent(typeof(Terrain));
		if (!terrain && !mat)
		{
			globalSettingsHolder.use_mat = GetComponent<Renderer>().sharedMaterial;
		}
		if ((bool)terrain && !mat && terrain.materialTemplate != null)
		{
			globalSettingsHolder.use_mat = terrain.materialTemplate;
		}
		globalSettingsHolder.SetShaderParam("_Control1", controlA);
		if (globalSettingsHolder.numLayers > 4)
		{
			globalSettingsHolder.SetShaderParam("_Control3", controlB);
			globalSettingsHolder.SetShaderParam("_Control2", controlB);
		}
		if (globalSettingsHolder.numLayers > 8)
		{
			globalSettingsHolder.SetShaderParam("_Control3", controlC);
		}
		globalSettingsHolder.SetShaderParam("_ColorMapGlobal", ColorGlobal);
		globalSettingsHolder.SetShaderParam("_NormalMapGlobal", NormalGlobal);
		globalSettingsHolder.SetShaderParam("_TreesMapGlobal", TreesGlobal);
		globalSettingsHolder.SetShaderParam("_AmbientEmissiveMapGlobal", AmbientEmissiveMap);
		globalSettingsHolder.SetShaderParam("_BumpMapGlobal", BumpGlobalCombined);
		globalSettingsHolder.use_mat = null;
	}

	public void GetControlMaps()
	{
		Terrain terrain = (Terrain)GetComponent(typeof(Terrain));
		if (!terrain)
		{
			Debug.Log("Can't fint terrain component !!!");
			return;
		}
		PropertyInfo property = terrain.terrainData.GetType().GetProperty("alphamapTextures", BindingFlags.Instance | BindingFlags.Public);
		if (property != null)
		{
			Texture2D[] array = (Texture2D[])property.GetValue(terrain.terrainData, null);
			if (array.Length != 0)
			{
				controlA = array[0];
			}
			else
			{
				controlA = null;
			}
			if (array.Length > 1)
			{
				controlB = array[1];
			}
			else
			{
				controlB = null;
			}
			if (array.Length > 2)
			{
				controlC = array[2];
			}
			else
			{
				controlC = null;
			}
		}
		else
		{
			Debug.LogError("Can't access alphamapTexture directly...");
		}
	}

	public void SetCustomControlMaps()
	{
		Terrain terrain = (Terrain)GetComponent(typeof(Terrain));
		if (!terrain)
		{
			Debug.Log("Can't fint terrain component !!!");
		}
		else
		{
			if (controlA == null)
			{
				return;
			}
			if (terrain.terrainData.alphamapResolution != controlA.width)
			{
				Debug.LogError("Terrain controlmap resolution differs fromrequested control texture...");
			}
			else
			{
				if (!controlA)
				{
					return;
				}
				float[,,] alphamaps = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapResolution, terrain.terrainData.alphamapResolution);
				Color[] pixels = controlA.GetPixels();
				for (int i = 0; i < terrain.terrainData.alphamapLayers; i++)
				{
					int num = 0;
					switch (i)
					{
					case 4:
						if (!controlB)
						{
							return;
						}
						pixels = controlB.GetPixels();
						break;
					case 8:
						if (!controlC)
						{
							return;
						}
						pixels = controlC.GetPixels();
						break;
					}
					int index = i & 3;
					for (int j = 0; j < terrain.terrainData.alphamapResolution; j++)
					{
						for (int k = 0; k < terrain.terrainData.alphamapResolution; k++)
						{
							alphamaps[j, k, i] = pixels[num++][index];
						}
					}
				}
				terrain.terrainData.SetAlphamaps(0, 0, alphamaps);
			}
		}
	}

	public void RestorePreset(ReliefTerrainPresetHolder holder)
	{
		controlA = holder.controlA;
		controlB = holder.controlB;
		controlC = holder.controlC;
		SetCustomControlMaps();
		ColorGlobal = holder.ColorGlobal;
		NormalGlobal = holder.NormalGlobal;
		TreesGlobal = holder.TreesGlobal;
		AmbientEmissiveMap = holder.AmbientEmissiveMap;
		BumpGlobalCombined = holder.BumpGlobalCombined;
		TERRAIN_WetMask = holder.TERRAIN_WetMask;
		globalColorModifed_flag = holder.globalColorModifed_flag;
		globalCombinedModifed_flag = holder.globalCombinedModifed_flag;
		globalWaterModifed_flag = holder.globalWaterModifed_flag;
		RefreshTextures();
		globalSettingsHolder.RestorePreset(holder);
	}

	public ReliefTerrainPresetHolder GetPresetByID(string PresetID)
	{
		if (presetHolders != null)
		{
			for (int i = 0; i < presetHolders.Length; i++)
			{
				if (presetHolders[i].PresetID == PresetID)
				{
					return presetHolders[i];
				}
			}
		}
		return null;
	}

	public ReliefTerrainPresetHolder GetPresetByName(string PresetName)
	{
		if (presetHolders != null)
		{
			for (int i = 0; i < presetHolders.Length; i++)
			{
				if (presetHolders[i].PresetName == PresetName)
				{
					return presetHolders[i];
				}
			}
		}
		return null;
	}

	public bool InterpolatePresets(string PresetID1, string PresetID2, float t)
	{
		ReliefTerrainPresetHolder presetByID = GetPresetByID(PresetID1);
		ReliefTerrainPresetHolder presetByID2 = GetPresetByID(PresetID2);
		if (presetByID == null || presetByID2 == null || presetByID.Spec == null || presetByID2.Spec == null || presetByID.Spec.Length != presetByID2.Spec.Length)
		{
			return false;
		}
		globalSettingsHolder.InterpolatePresets(presetByID, presetByID2, t);
		return true;
	}
}
