using System;
using UnityEngine;

[Serializable]
public class ReliefTerrainGlobalSettingsHolder
{
	public int numTiles;

	public int numLayers;

	public TerrainLayer[] terrainLayers;

	[NonSerialized]
	public bool dont_check_weak_references;

	[NonSerialized]
	public bool dont_check_for_interfering_terrain_replacement_shaders;

	[NonSerialized]
	public Texture2D[] bakeJobArray;

	public Texture2D[] splats;

	public Texture2D[] splat_atlases = new Texture2D[3];

	public string save_path_atlasA = "";

	public string save_path_atlasB = "";

	public string save_path_atlasC = "";

	public string save_path_terrain_steepness = "";

	public string save_path_terrain_height = "";

	public string save_path_terrain_direction = "";

	public string save_path_Bump01 = "";

	public string save_path_Bump23 = "";

	public string save_path_Bump45 = "";

	public string save_path_Bump67 = "";

	public string save_path_Bump89 = "";

	public string save_path_BumpAB = "";

	public string save_path_HeightMap = "";

	public string save_path_HeightMap2 = "";

	public string save_path_HeightMap3 = "";

	public string save_path_SSColorCombinedA = "";

	public string save_path_SSColorCombinedB = "";

	public string newPresetName = "a preset name...";

	public Texture2D activateObject;

	private GameObject _RTP_LODmanager;

	public RTP_LODmanager _RTP_LODmanagerScript;

	public float RTP_MIP_BIAS;

	public float MasterLayerBrightness = 1f;

	public float MasterLayerSaturation = 1f;

	public float EmissionRefractFiltering = 4f;

	public float EmissionRefractAnimSpeed = 4f;

	public RTPColorChannels SuperDetailA_channel;

	public RTPColorChannels SuperDetailB_channel;

	public Texture2D Bump01;

	public Texture2D Bump23;

	public Texture2D Bump45;

	public Texture2D Bump67;

	public Texture2D Bump89;

	public Texture2D BumpAB;

	public Texture2D BumpGlobal;

	public int BumpGlobalCombinedSize = 1024;

	public Texture2D SSColorCombinedA;

	public Texture2D SSColorCombinedB;

	public Texture2D VerticalTexture;

	public float BumpMapGlobalScale;

	public Vector3 GlobalColorMapBlendValues;

	public float _GlobalColorMapNearMIP;

	public float GlobalColorMapSaturation;

	public float GlobalColorMapSaturationFar = 1f;

	public float GlobalColorMapDistortByPerlin = 0.005f;

	public float GlobalColorMapBrightness;

	public float GlobalColorMapBrightnessFar = 1f;

	public float _FarNormalDamp;

	public float blendMultiplier;

	public Vector3 terrainTileSize;

	public Texture2D HeightMap;

	public Vector4 ReliefTransform;

	public float DIST_STEPS;

	public float WAVELENGTH;

	public float ReliefBorderBlend;

	public float ExtrudeHeight;

	public float LightmapShading;

	public float RTP_AOsharpness;

	public float RTP_AOamp;

	public float _occlusionStrength = 1f;

	public float SHADOW_STEPS;

	public float WAVELENGTH_SHADOWS;

	public float SelfShadowStrength;

	public float ShadowSmoothing;

	public float ShadowSoftnessFade = 0.8f;

	public float distance_start;

	public float distance_transition;

	public float distance_start_bumpglobal;

	public float distance_transition_bumpglobal;

	public float rtp_perlin_start_val;

	public float _Phong;

	public float tessHeight = 300f;

	public float _TessSubdivisions = 1f;

	public float _TessSubdivisionsFar = 1f;

	public float _TessYOffset;

	public float trees_shadow_distance_start;

	public float trees_shadow_distance_transition;

	public float trees_shadow_value;

	public float trees_pixel_distance_start;

	public float trees_pixel_distance_transition;

	public float trees_pixel_blend_val;

	public float global_normalMap_multiplier;

	public float global_normalMap_farUsage;

	public float _AmbientEmissiveMultiplier = 1f;

	public float _AmbientEmissiveRelief = 0.5f;

	public Texture2D HeightMap2;

	public Texture2D HeightMap3;

	public int rtp_mipoffset_globalnorm;

	public float _SuperDetailTiling;

	public Texture2D SuperDetailA;

	public Texture2D SuperDetailB;

	public float TERRAIN_GlobalWetness;

	public Texture2D TERRAIN_RippleMap;

	public float TERRAIN_RippleScale;

	public float TERRAIN_FlowScale;

	public float TERRAIN_FlowSpeed;

	public float TERRAIN_FlowCycleScale;

	public float TERRAIN_FlowMipOffset;

	public float TERRAIN_WetDarkening;

	public float TERRAIN_WetDropletsStrength;

	public float TERRAIN_WetHeight_Treshold;

	public float TERRAIN_WetHeight_Transition;

	public float TERRAIN_RainIntensity;

	public float TERRAIN_DropletsSpeed;

	public float TERRAIN_mipoffset_flowSpeed;

	public float TERRAIN_CausticsAnimSpeed;

	public Color TERRAIN_CausticsColor;

	public GameObject TERRAIN_CausticsWaterLevelRefObject;

	public float TERRAIN_CausticsWaterLevel;

	public float TERRAIN_CausticsWaterLevelByAngle;

	public float TERRAIN_CausticsWaterDeepFadeLength;

	public float TERRAIN_CausticsWaterShallowFadeLength;

	public float TERRAIN_CausticsTilingScale;

	public Texture2D TERRAIN_CausticsTex;

	public Vector4 RTP_LightDefVector;

	public Texture2D[] Bumps;

	public float[] FarSpecCorrection;

	public float[] MIPmult;

	public float[] MixScale;

	public float[] MixBlend;

	public float[] MixSaturation;

	public float[] RTP_DiffFresnel;

	public float[] RTP_metallic;

	public float[] RTP_glossMin;

	public float[] RTP_glossMax;

	public float[] RTP_glitter;

	public float[] GlobalColorBottom;

	public float[] GlobalColorTop;

	public float[] GlobalColorColormapLoSat;

	public float[] GlobalColorColormapHiSat;

	public float[] GlobalColorLayerLoSat;

	public float[] GlobalColorLayerHiSat;

	public float[] GlobalColorLoBlend;

	public float[] GlobalColorHiBlend;

	public float[] MixBrightness;

	public float[] MixReplace;

	public float[] LayerBrightness;

	public float[] LayerBrightness2Spec;

	public float[] LayerAlbedo2SpecColor;

	public float[] LayerSaturation;

	public float[] LayerEmission;

	public Color[] LayerEmissionColor;

	public float[] LayerEmissionRefractStrength;

	public float[] LayerEmissionRefractHBedge;

	public float[] GlobalColorPerLayer;

	public float[] PER_LAYER_HEIGHT_MODIFIER;

	public float[] _SuperDetailStrengthMultA;

	public float[] _SuperDetailStrengthMultASelfMaskNear;

	public float[] _SuperDetailStrengthMultASelfMaskFar;

	public float[] _SuperDetailStrengthMultB;

	public float[] _SuperDetailStrengthMultBSelfMaskNear;

	public float[] _SuperDetailStrengthMultBSelfMaskFar;

	public float[] _SuperDetailStrengthNormal;

	public float[] _BumpMapGlobalStrength;

	public float[] AO_strength = new float[12]
	{
		1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,
		1f, 1f
	};

	public float[] VerticalTextureStrength;

	public float VerticalTextureGlobalBumpInfluence;

	public float VerticalTextureTiling;

	public Texture2D[] Heights;

	public float[] _snow_strength_per_layer;

	public float[] TERRAIN_LayerWetStrength;

	public float[] TERRAIN_WaterLevel;

	public float[] TERRAIN_WaterLevelSlopeDamp;

	public float[] TERRAIN_WaterEdge;

	public float[] TERRAIN_WaterGloss;

	public float[] TERRAIN_WaterGlossDamper;

	public float[] TERRAIN_Refraction;

	public float[] TERRAIN_WetRefraction;

	public float[] TERRAIN_Flow;

	public float[] TERRAIN_WetFlow;

	public float[] TERRAIN_WaterMetallic;

	public float[] TERRAIN_WetGloss;

	public Color[] TERRAIN_WaterColor;

	public float[] TERRAIN_WaterEmission;

	public float _snow_strength;

	public float _global_color_brightness_to_snow;

	public float _snow_slope_factor;

	public float _snow_edge_definition;

	public float _snow_height_treshold;

	public float _snow_height_transition;

	public Color _snow_color;

	public float _snow_gloss;

	public float _snow_reflectivness;

	public float _snow_deep_factor;

	public float _snow_diff_fresnel;

	public float _snow_metallic;

	public float _snow_Frost;

	public float _snow_MicroTiling = 1f;

	public float _snow_BumpMicro = 0.2f;

	public Color _SnowGlitterColor = new Color(1f, 1f, 1f, 0.1f);

	public float _snow_occlusionStrength = 0.5f;

	public int _snow_TranslucencyDeferredLightIndex;

	public Color _GlitterColor;

	public float[] _GlitterStrength;

	public float _GlitterTiling;

	public float _GlitterDensity;

	public float _GlitterFilter;

	public float _GlitterColorization;

	public Texture2D _SparkleMap;

	public bool _4LAYERS_SHADER_USED;

	public bool flat_dir_ref = true;

	public bool flip_dir_ref = true;

	public GameObject direction_object;

	public bool show_details;

	public bool show_details_main;

	public bool show_details_atlasing;

	public bool show_details_layers;

	public bool show_details_uv_blend;

	public bool show_controlmaps;

	public bool show_controlmaps_build;

	public bool show_controlmaps_helpers;

	public bool show_controlmaps_highcost;

	public bool show_controlmaps_splats;

	public bool show_vert_texture;

	public bool show_global_color;

	public bool show_snow;

	public bool show_global_bump;

	public bool show_global_bump_normals;

	public bool show_global_bump_superdetail;

	public ReliefTerrainMenuItems submenu;

	public ReliefTerrainSettingsItems submenu_settings;

	public ReliefTerrainDerivedTexturesItems submenu_derived_textures;

	public ReliefTerrainControlTexturesItems submenu_control_textures;

	public bool show_global_wet_settings;

	public bool show_global_reflection_settings;

	public int show_active_layer;

	public bool show_derivedmaps;

	public bool show_settings;

	public bool undo_flag;

	public bool paint_flag;

	public float paint_size = 0.5f;

	public float paint_smoothness;

	public float paint_opacity = 1f;

	public Color paintColor = new Color(0.5f, 0.3f, 0f, 0f);

	public bool preserveBrightness = true;

	public bool paint_alpha_flag;

	public bool paint_wetmask;

	public RaycastHit paintHitInfo;

	public bool paintHitInfo_flag;

	public bool cut_holes;

	private Texture2D dumb_tex;

	public Color[] paintColorSwatches;

	public Material use_mat;

	public ReliefTerrainGlobalSettingsHolder()
	{
		Bumps = new Texture2D[12];
		Heights = new Texture2D[12];
		FarSpecCorrection = new float[12];
		MIPmult = new float[12];
		MixScale = new float[12];
		MixBlend = new float[12];
		MixSaturation = new float[12];
		RTP_DiffFresnel = new float[12];
		RTP_metallic = new float[12];
		RTP_glossMin = new float[12];
		RTP_glossMax = new float[12];
		RTP_glitter = new float[12];
		MixBrightness = new float[12];
		MixReplace = new float[12];
		LayerBrightness = new float[12];
		LayerBrightness2Spec = new float[12];
		LayerAlbedo2SpecColor = new float[12];
		LayerSaturation = new float[12];
		LayerEmission = new float[12];
		LayerEmissionColor = new Color[12];
		LayerEmissionRefractStrength = new float[12];
		LayerEmissionRefractHBedge = new float[12];
		GlobalColorPerLayer = new float[12];
		GlobalColorBottom = new float[12];
		GlobalColorTop = new float[12];
		GlobalColorColormapLoSat = new float[12];
		GlobalColorColormapHiSat = new float[12];
		GlobalColorLayerLoSat = new float[12];
		GlobalColorLayerHiSat = new float[12];
		GlobalColorLoBlend = new float[12];
		GlobalColorHiBlend = new float[12];
		PER_LAYER_HEIGHT_MODIFIER = new float[12];
		_snow_strength_per_layer = new float[12];
		_SuperDetailStrengthMultA = new float[12];
		_SuperDetailStrengthMultASelfMaskNear = new float[12];
		_SuperDetailStrengthMultASelfMaskFar = new float[12];
		_SuperDetailStrengthMultB = new float[12];
		_SuperDetailStrengthMultBSelfMaskNear = new float[12];
		_SuperDetailStrengthMultBSelfMaskFar = new float[12];
		_SuperDetailStrengthNormal = new float[12];
		_BumpMapGlobalStrength = new float[12];
		AO_strength = new float[12];
		VerticalTextureStrength = new float[12];
		TERRAIN_LayerWetStrength = new float[12];
		TERRAIN_WaterLevel = new float[12];
		TERRAIN_WaterLevelSlopeDamp = new float[12];
		TERRAIN_WaterEdge = new float[12];
		TERRAIN_WaterGloss = new float[12];
		TERRAIN_WaterGlossDamper = new float[12];
		TERRAIN_Refraction = new float[12];
		TERRAIN_WetRefraction = new float[12];
		TERRAIN_Flow = new float[12];
		TERRAIN_WetFlow = new float[12];
		TERRAIN_WaterMetallic = new float[12];
		TERRAIN_WetGloss = new float[12];
		TERRAIN_WaterColor = new Color[12];
		TERRAIN_WaterEmission = new float[12];
		_GlitterStrength = new float[12];
	}

	public void ReInit(Terrain terrainComp)
	{
		if (terrainComp.terrainData.terrainLayers.Length > numLayers)
		{
			Texture2D[] array = new Texture2D[terrainComp.terrainData.terrainLayers.Length];
			Texture2D[] array2 = new Texture2D[terrainComp.terrainData.terrainLayers.Length];
			for (int i = 0; i < splats.Length; i++)
			{
				array[i] = splats[i];
				array2[i] = Bumps[i];
			}
			splats = array;
			Bumps = array2;
			splats[terrainComp.terrainData.terrainLayers.Length - 1] = terrainComp.terrainData.terrainLayers[(terrainComp.terrainData.terrainLayers.Length - 2 >= 0) ? (terrainComp.terrainData.terrainLayers.Length - 2) : 0].diffuseTexture;
			Bumps[terrainComp.terrainData.terrainLayers.Length - 1] = terrainComp.terrainData.terrainLayers[(terrainComp.terrainData.terrainLayers.Length - 2 >= 0) ? (terrainComp.terrainData.terrainLayers.Length - 2) : 0].normalMapTexture;
		}
		else if (terrainComp.terrainData.terrainLayers.Length < numLayers)
		{
			Texture2D[] array3 = new Texture2D[terrainComp.terrainData.terrainLayers.Length];
			Texture2D[] array4 = new Texture2D[terrainComp.terrainData.terrainLayers.Length];
			for (int j = 0; j < array3.Length; j++)
			{
				array3[j] = splats[j];
				array4[j] = Bumps[j];
			}
			splats = array3;
			Bumps = array4;
		}
		numLayers = terrainComp.terrainData.terrainLayers.Length;
	}

	public void SetShaderParam(string name, Texture2D tex)
	{
		if ((bool)tex)
		{
			if ((bool)use_mat)
			{
				use_mat.SetTexture(name, tex);
			}
			else
			{
				Shader.SetGlobalTexture(name, tex);
			}
		}
	}

	public void SetShaderParam(string name, Cubemap tex)
	{
		if ((bool)tex)
		{
			if ((bool)use_mat)
			{
				use_mat.SetTexture(name, tex);
			}
			else
			{
				Shader.SetGlobalTexture(name, tex);
			}
		}
	}

	public void SetShaderParam(string name, Matrix4x4 mtx)
	{
		if ((bool)use_mat)
		{
			use_mat.SetMatrix(name, mtx);
		}
		else
		{
			Shader.SetGlobalMatrix(name, mtx);
		}
	}

	public void SetShaderParam(string name, Vector4 vec)
	{
		if ((bool)use_mat)
		{
			use_mat.SetVector(name, vec);
		}
		else
		{
			Shader.SetGlobalVector(name, vec);
		}
	}

	public void SetShaderParam(string name, float val)
	{
		if ((bool)use_mat)
		{
			use_mat.SetFloat(name, val);
		}
		else
		{
			Shader.SetGlobalFloat(name, val);
		}
	}

	public void SetShaderParam(string name, Color col)
	{
		if ((bool)use_mat)
		{
			use_mat.SetColor(name, col);
		}
		else
		{
			Shader.SetGlobalColor(name, col);
		}
	}

	public RTP_LODmanager Get_RTP_LODmanagerScript()
	{
		return _RTP_LODmanagerScript;
	}

	private void CheckLightScriptForDefered()
	{
		Light[] array = UnityEngine.Object.FindObjectsOfType<Light>();
		Light light = null;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].type == LightType.Directional)
			{
				if (!(array[i].gameObject.GetComponent<ReliefShaders_applyLightForDeferred>() == null))
				{
					return;
				}
				light = array[i];
			}
		}
		if ((bool)light)
		{
			(light.gameObject.AddComponent(typeof(ReliefShaders_applyLightForDeferred)) as ReliefShaders_applyLightForDeferred).lightForSelfShadowing = light;
		}
	}

	public void RefreshAll()
	{
		CheckLightScriptForDefered();
		ReliefTerrain[] array = UnityEngine.Object.FindObjectsOfType(typeof(ReliefTerrain)) as ReliefTerrain[];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].globalSettingsHolder != null)
			{
				Terrain terrain = array[i].GetComponent(typeof(Terrain)) as Terrain;
				if ((bool)terrain)
				{
					array[i].globalSettingsHolder.Refresh(terrain.materialTemplate);
				}
				else
				{
					array[i].globalSettingsHolder.Refresh(array[i].GetComponent<Renderer>().sharedMaterial);
				}
				array[i].RefreshTextures();
			}
		}
		GeometryVsTerrainBlend[] array2 = UnityEngine.Object.FindObjectsOfType(typeof(GeometryVsTerrainBlend)) as GeometryVsTerrainBlend[];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j].SetupValues();
		}
	}

	public void Refresh(Material mat = null, ReliefTerrain rt_caller = null)
	{
		if (splats == null)
		{
			return;
		}
		if (mat == null && rt_caller != null && rt_caller.globalSettingsHolder == this)
		{
			Terrain terrain = rt_caller.GetComponent(typeof(Terrain)) as Terrain;
			if ((bool)terrain)
			{
				rt_caller.globalSettingsHolder.Refresh(terrain.materialTemplate);
			}
			else if (rt_caller.GetComponent<Renderer>() != null && rt_caller.GetComponent<Renderer>().sharedMaterial != null)
			{
				rt_caller.globalSettingsHolder.Refresh(rt_caller.GetComponent<Renderer>().sharedMaterial);
			}
		}
		use_mat = mat;
		if (mat != null)
		{
			mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
		}
		for (int i = 0; i < numLayers; i++)
		{
			if (i < 4)
			{
				SetShaderParam("_SplatA" + i, splats[i]);
			}
			else if (i < 8)
			{
				if (_4LAYERS_SHADER_USED)
				{
					SetShaderParam("_SplatC" + (i - 4), splats[i]);
					SetShaderParam("_SplatB" + (i - 4), splats[i]);
				}
				else
				{
					SetShaderParam("_SplatB" + (i - 4), splats[i]);
				}
			}
			else if (i < 12)
			{
				SetShaderParam("_SplatC" + (i - 4), splats[i]);
			}
		}
		CheckAndUpdate(ref RTP_DiffFresnel, 0f, numLayers);
		CheckAndUpdate(ref RTP_metallic, 0f, numLayers);
		CheckAndUpdate(ref RTP_glossMin, 0f, numLayers);
		CheckAndUpdate(ref RTP_glossMax, 1f, numLayers);
		CheckAndUpdate(ref RTP_glitter, 0f, numLayers);
		CheckAndUpdate(ref TERRAIN_WaterGloss, 0.1f, numLayers);
		CheckAndUpdate(ref TERRAIN_WaterGlossDamper, 0f, numLayers);
		CheckAndUpdate(ref TERRAIN_WaterMetallic, 0.1f, numLayers);
		CheckAndUpdate(ref TERRAIN_WetGloss, 0.05f, numLayers);
		CheckAndUpdate(ref TERRAIN_WetFlow, 0.05f, numLayers);
		CheckAndUpdate(ref MixBrightness, 2f, numLayers);
		CheckAndUpdate(ref MixReplace, 0f, numLayers);
		CheckAndUpdate(ref LayerBrightness, 1f, numLayers);
		CheckAndUpdate(ref LayerBrightness2Spec, 0f, numLayers);
		CheckAndUpdate(ref LayerAlbedo2SpecColor, 0f, numLayers);
		CheckAndUpdate(ref LayerSaturation, 1f, numLayers);
		CheckAndUpdate(ref LayerEmission, 0f, numLayers);
		CheckAndUpdate(ref FarSpecCorrection, 0f, numLayers);
		CheckAndUpdate(ref LayerEmissionColor, Color.black, numLayers);
		CheckAndUpdate(ref LayerEmissionRefractStrength, 0f, numLayers);
		CheckAndUpdate(ref LayerEmissionRefractHBedge, 0f, numLayers);
		CheckAndUpdate(ref TERRAIN_WaterEmission, 0f, numLayers);
		CheckAndUpdate(ref _GlitterStrength, 0f, numLayers);
		SetShaderParam("terrainTileSize", terrainTileSize);
		SetShaderParam("RTP_AOamp", RTP_AOamp);
		SetShaderParam("RTP_AOsharpness", RTP_AOsharpness);
		SetShaderParam("_occlusionStrength", _occlusionStrength);
		SetShaderParam("EmissionRefractFiltering", EmissionRefractFiltering);
		SetShaderParam("EmissionRefractAnimSpeed", EmissionRefractAnimSpeed);
		SetShaderParam("_VerticalTexture", VerticalTexture);
		SetShaderParam("_GlobalColorMapBlendValues", GlobalColorMapBlendValues);
		SetShaderParam("_GlobalColorMapSaturation", GlobalColorMapSaturation);
		SetShaderParam("_GlobalColorMapSaturationFar", GlobalColorMapSaturationFar);
		SetShaderParam("_GlobalColorMapDistortByPerlin", GlobalColorMapDistortByPerlin);
		SetShaderParam("_GlobalColorMapBrightness", GlobalColorMapBrightness);
		SetShaderParam("_GlobalColorMapBrightnessFar", GlobalColorMapBrightnessFar);
		SetShaderParam("_GlobalColorMapNearMIP", _GlobalColorMapNearMIP);
		SetShaderParam("_RTP_MIP_BIAS", RTP_MIP_BIAS);
		SetShaderParam("_BumpMapGlobalScale", BumpMapGlobalScale);
		SetShaderParam("_FarNormalDamp", _FarNormalDamp);
		SetShaderParam("_blend_multiplier", blendMultiplier);
		SetShaderParam("_TERRAIN_ReliefTransform", ReliefTransform);
		SetShaderParam("_TERRAIN_ReliefTransformTriplanarZ", ReliefTransform.x);
		SetShaderParam("_TERRAIN_DIST_STEPS", DIST_STEPS);
		SetShaderParam("_TERRAIN_WAVELENGTH", WAVELENGTH);
		SetShaderParam("_TERRAIN_ExtrudeHeight", ExtrudeHeight);
		SetShaderParam("_TERRAIN_LightmapShading", LightmapShading);
		SetShaderParam("_TERRAIN_SHADOW_STEPS", SHADOW_STEPS);
		SetShaderParam("_TERRAIN_WAVELENGTH_SHADOWS", WAVELENGTH_SHADOWS);
		SetShaderParam("_TERRAIN_SelfShadowStrength", SelfShadowStrength);
		SetShaderParam("_TERRAIN_ShadowSmoothing", (1f - ShadowSmoothing) * 6f);
		SetShaderParam("_TERRAIN_ShadowSoftnessFade", ShadowSoftnessFade);
		SetShaderParam("_TERRAIN_distance_start", distance_start);
		SetShaderParam("_TERRAIN_distance_transition", distance_transition);
		SetShaderParam("_TERRAIN_distance_start_bumpglobal", distance_start_bumpglobal);
		SetShaderParam("_TERRAIN_distance_transition_bumpglobal", distance_transition_bumpglobal);
		SetShaderParam("rtp_perlin_start_val", rtp_perlin_start_val);
		Shader.SetGlobalVector("_TERRAIN_trees_shadow_values", new Vector4(trees_shadow_distance_start, trees_shadow_distance_transition, trees_shadow_value, global_normalMap_multiplier));
		Shader.SetGlobalVector("_TERRAIN_trees_pixel_values", new Vector4(trees_pixel_distance_start, trees_pixel_distance_transition, trees_pixel_blend_val, global_normalMap_farUsage));
		SetShaderParam("_Phong", _Phong);
		SetShaderParam("_TessSubdivisions", _TessSubdivisions);
		SetShaderParam("_TessSubdivisionsFar", _TessSubdivisionsFar);
		SetShaderParam("_TessYOffset", _TessYOffset);
		Shader.SetGlobalFloat("_AmbientEmissiveMultiplier", _AmbientEmissiveMultiplier);
		Shader.SetGlobalFloat("_AmbientEmissiveRelief", _AmbientEmissiveRelief);
		SetShaderParam("_SuperDetailTiling", _SuperDetailTiling);
		Shader.SetGlobalFloat("rtp_snow_strength", _snow_strength);
		Shader.SetGlobalFloat("rtp_global_color_brightness_to_snow", _global_color_brightness_to_snow);
		Shader.SetGlobalFloat("rtp_snow_slope_factor", _snow_slope_factor);
		Shader.SetGlobalFloat("rtp_snow_edge_definition", _snow_edge_definition);
		Shader.SetGlobalFloat("rtp_snow_height_treshold", _snow_height_treshold);
		Shader.SetGlobalFloat("rtp_snow_height_transition", _snow_height_transition);
		Shader.SetGlobalColor("rtp_snow_color", _snow_color);
		Shader.SetGlobalFloat("rtp_snow_gloss", _snow_gloss);
		Shader.SetGlobalFloat("rtp_snow_reflectivness", _snow_reflectivness);
		Shader.SetGlobalFloat("rtp_snow_deep_factor", _snow_deep_factor);
		Shader.SetGlobalFloat("rtp_snow_diff_fresnel", _snow_diff_fresnel);
		Shader.SetGlobalFloat("rtp_snow_metallic", _snow_metallic);
		Shader.SetGlobalFloat("rtp_snow_Frost", _snow_Frost);
		Shader.SetGlobalFloat("rtp_snow_MicroTiling", _snow_MicroTiling);
		Shader.SetGlobalFloat("rtp_snow_BumpMicro", _snow_BumpMicro);
		Shader.SetGlobalFloat("rtp_snow_occlusionStrength", _snow_occlusionStrength);
		Shader.SetGlobalFloat("rtp_snow_TranslucencyDeferredLightIndex", _snow_TranslucencyDeferredLightIndex);
		Shader.SetGlobalColor("_SnowGlitterColor", _SnowGlitterColor);
		SetShaderParam("TERRAIN_CausticsAnimSpeed", TERRAIN_CausticsAnimSpeed);
		SetShaderParam("TERRAIN_CausticsColor", TERRAIN_CausticsColor);
		if ((bool)TERRAIN_CausticsWaterLevelRefObject)
		{
			TERRAIN_CausticsWaterLevel = TERRAIN_CausticsWaterLevelRefObject.transform.position.y;
		}
		Shader.SetGlobalFloat("TERRAIN_CausticsWaterLevel", TERRAIN_CausticsWaterLevel);
		Shader.SetGlobalFloat("TERRAIN_CausticsWaterLevelByAngle", TERRAIN_CausticsWaterLevelByAngle);
		Shader.SetGlobalFloat("TERRAIN_CausticsWaterDeepFadeLength", TERRAIN_CausticsWaterDeepFadeLength);
		Shader.SetGlobalFloat("TERRAIN_CausticsWaterShallowFadeLength", TERRAIN_CausticsWaterShallowFadeLength);
		SetShaderParam("TERRAIN_CausticsTilingScale", TERRAIN_CausticsTilingScale);
		SetShaderParam("TERRAIN_CausticsTex", TERRAIN_CausticsTex);
		SetShaderParam("_GlitterColor", _GlitterColor);
		SetShaderParam("_GlitterTiling", _GlitterTiling);
		SetShaderParam("_GlitterDensity", _GlitterDensity);
		SetShaderParam("_GlitterFilter", _GlitterFilter);
		SetShaderParam("_GlitterColorization", _GlitterColorization);
		SetShaderParam("_SparkleMap", _SparkleMap);
		if (numLayers > 0)
		{
			int num = 512;
			for (int j = 0; j < numLayers; j++)
			{
				if ((bool)splats[j])
				{
					num = splats[j].width;
					break;
				}
			}
			SetShaderParam("rtp_mipoffset_color", (0f - Mathf.Log(1024f / (float)num)) / Mathf.Log(2f));
			if (Bump01 != null)
			{
				num = Bump01.width;
			}
			SetShaderParam("rtp_mipoffset_bump", (0f - Mathf.Log(1024f / (float)num)) / Mathf.Log(2f));
			if ((bool)HeightMap)
			{
				num = HeightMap.width;
			}
			else if ((bool)HeightMap2)
			{
				num = HeightMap2.width;
			}
			else if ((bool)HeightMap3)
			{
				num = HeightMap3.width;
			}
			SetShaderParam("rtp_mipoffset_height", (0f - Mathf.Log(1024f / (float)num)) / Mathf.Log(2f));
			num = BumpGlobalCombinedSize;
			SetShaderParam("rtp_mipoffset_globalnorm", (0f - Mathf.Log(1024f / ((float)num * BumpMapGlobalScale))) / Mathf.Log(2f) + (float)rtp_mipoffset_globalnorm);
			SetShaderParam("rtp_mipoffset_superdetail", (0f - Mathf.Log(1024f / ((float)num * _SuperDetailTiling))) / Mathf.Log(2f));
			SetShaderParam("rtp_mipoffset_flow", (0f - Mathf.Log(1024f / ((float)num * TERRAIN_FlowScale))) / Mathf.Log(2f) + TERRAIN_FlowMipOffset);
			if ((bool)TERRAIN_RippleMap)
			{
				num = TERRAIN_RippleMap.width;
			}
			SetShaderParam("rtp_mipoffset_ripple", (0f - Mathf.Log(1024f / ((float)num * TERRAIN_RippleScale))) / Mathf.Log(2f));
			if ((bool)TERRAIN_CausticsTex)
			{
				num = TERRAIN_CausticsTex.width;
			}
			SetShaderParam("rtp_mipoffset_caustics", (0f - Mathf.Log(1024f / ((float)num * TERRAIN_CausticsTilingScale))) / Mathf.Log(2f));
		}
		Shader.SetGlobalFloat("TERRAIN_GlobalWetness", TERRAIN_GlobalWetness);
		SetShaderParam("TERRAIN_RippleMap", TERRAIN_RippleMap);
		SetShaderParam("TERRAIN_RippleScale", TERRAIN_RippleScale);
		SetShaderParam("TERRAIN_FlowScale", TERRAIN_FlowScale);
		SetShaderParam("TERRAIN_FlowMipOffset", TERRAIN_FlowMipOffset);
		SetShaderParam("TERRAIN_FlowSpeed", TERRAIN_FlowSpeed);
		SetShaderParam("TERRAIN_FlowCycleScale", TERRAIN_FlowCycleScale);
		Shader.SetGlobalFloat("TERRAIN_RainIntensity", TERRAIN_RainIntensity);
		SetShaderParam("TERRAIN_DropletsSpeed", TERRAIN_DropletsSpeed);
		SetShaderParam("TERRAIN_WetDropletsStrength", TERRAIN_WetDropletsStrength);
		SetShaderParam("TERRAIN_WetDarkening", TERRAIN_WetDarkening);
		SetShaderParam("TERRAIN_mipoffset_flowSpeed", TERRAIN_mipoffset_flowSpeed);
		SetShaderParam("TERRAIN_WetHeight_Treshold", TERRAIN_WetHeight_Treshold);
		SetShaderParam("TERRAIN_WetHeight_Transition", TERRAIN_WetHeight_Transition);
		Shader.SetGlobalVector("RTP_LightDefVector", RTP_LightDefVector);
		SetShaderParam("_VerticalTextureGlobalBumpInfluence", VerticalTextureGlobalBumpInfluence);
		SetShaderParam("_VerticalTextureTiling", VerticalTextureTiling);
		SetShaderParam("_FarSpecCorrection0123", getVector(FarSpecCorrection, 0, 3));
		SetShaderParam("_MIPmult0123", getVector(MIPmult, 0, 3));
		SetShaderParam("_MixScale0123", getVector(MixScale, 0, 3));
		SetShaderParam("_MixBlend0123", getVector(MixBlend, 0, 3));
		SetShaderParam("_MixSaturation0123", getVector(MixSaturation, 0, 3));
		SetShaderParam("RTP_DiffFresnel0123", getVector(RTP_DiffFresnel, 0, 3));
		SetShaderParam("RTP_metallic0123", getVector(RTP_metallic, 0, 3));
		SetShaderParam("RTP_glossMin0123", getVector(RTP_glossMin, 0, 3));
		SetShaderParam("RTP_glossMax0123", getVector(RTP_glossMax, 0, 3));
		SetShaderParam("RTP_glitter0123", getVector(RTP_glitter, 0, 3));
		SetShaderParam("_MixBrightness0123", getVector(MixBrightness, 0, 3));
		SetShaderParam("_MixReplace0123", getVector(MixReplace, 0, 3));
		SetShaderParam("_LayerBrightness0123", MasterLayerBrightness * getVector(LayerBrightness, 0, 3));
		SetShaderParam("_LayerSaturation0123", MasterLayerSaturation * getVector(LayerSaturation, 0, 3));
		SetShaderParam("_LayerEmission0123", getVector(LayerEmission, 0, 3));
		SetShaderParam("_LayerEmissionColorR0123", getColorVector(LayerEmissionColor, 0, 3, 0));
		SetShaderParam("_LayerEmissionColorG0123", getColorVector(LayerEmissionColor, 0, 3, 1));
		SetShaderParam("_LayerEmissionColorB0123", getColorVector(LayerEmissionColor, 0, 3, 2));
		SetShaderParam("_LayerEmissionColorA0123", getColorVector(LayerEmissionColor, 0, 3, 3));
		SetShaderParam("_LayerBrightness2Spec0123", getVector(LayerBrightness2Spec, 0, 3));
		SetShaderParam("_LayerAlbedo2SpecColor0123", getVector(LayerAlbedo2SpecColor, 0, 3));
		SetShaderParam("_LayerEmissionRefractStrength0123", getVector(LayerEmissionRefractStrength, 0, 3));
		SetShaderParam("_LayerEmissionRefractHBedge0123", getVector(LayerEmissionRefractHBedge, 0, 3));
		SetShaderParam("_GlobalColorPerLayer0123", getVector(GlobalColorPerLayer, 0, 3));
		SetShaderParam("_GlobalColorBottom0123", getVector(GlobalColorBottom, 0, 3));
		SetShaderParam("_GlobalColorTop0123", getVector(GlobalColorTop, 0, 3));
		SetShaderParam("_GlobalColorColormapLoSat0123", getVector(GlobalColorColormapLoSat, 0, 3));
		SetShaderParam("_GlobalColorColormapHiSat0123", getVector(GlobalColorColormapHiSat, 0, 3));
		SetShaderParam("_GlobalColorLayerLoSat0123", getVector(GlobalColorLayerLoSat, 0, 3));
		SetShaderParam("_GlobalColorLayerHiSat0123", getVector(GlobalColorLayerHiSat, 0, 3));
		SetShaderParam("_GlobalColorLoBlend0123", getVector(GlobalColorLoBlend, 0, 3));
		SetShaderParam("_GlobalColorHiBlend0123", getVector(GlobalColorHiBlend, 0, 3));
		SetShaderParam("PER_LAYER_HEIGHT_MODIFIER0123", getVector(PER_LAYER_HEIGHT_MODIFIER, 0, 3));
		SetShaderParam("rtp_snow_strength_per_layer0123", getVector(_snow_strength_per_layer, 0, 3));
		SetShaderParam("_SuperDetailStrengthMultA0123", getVector(_SuperDetailStrengthMultA, 0, 3));
		SetShaderParam("_SuperDetailStrengthMultB0123", getVector(_SuperDetailStrengthMultB, 0, 3));
		SetShaderParam("_SuperDetailStrengthNormal0123", getVector(_SuperDetailStrengthNormal, 0, 3));
		SetShaderParam("_BumpMapGlobalStrength0123", getVector(_BumpMapGlobalStrength, 0, 3));
		SetShaderParam("_SuperDetailStrengthMultASelfMaskNear0123", getVector(_SuperDetailStrengthMultASelfMaskNear, 0, 3));
		SetShaderParam("_SuperDetailStrengthMultASelfMaskFar0123", getVector(_SuperDetailStrengthMultASelfMaskFar, 0, 3));
		SetShaderParam("_SuperDetailStrengthMultBSelfMaskNear0123", getVector(_SuperDetailStrengthMultBSelfMaskNear, 0, 3));
		SetShaderParam("_SuperDetailStrengthMultBSelfMaskFar0123", getVector(_SuperDetailStrengthMultBSelfMaskFar, 0, 3));
		SetShaderParam("TERRAIN_LayerWetStrength0123", getVector(TERRAIN_LayerWetStrength, 0, 3));
		SetShaderParam("TERRAIN_WaterLevel0123", getVector(TERRAIN_WaterLevel, 0, 3));
		SetShaderParam("TERRAIN_WaterLevelSlopeDamp0123", getVector(TERRAIN_WaterLevelSlopeDamp, 0, 3));
		SetShaderParam("TERRAIN_WaterEdge0123", getVector(TERRAIN_WaterEdge, 0, 3));
		SetShaderParam("TERRAIN_WaterGloss0123", getVector(TERRAIN_WaterGloss, 0, 3));
		SetShaderParam("TERRAIN_WaterGlossDamper0123", getVector(TERRAIN_WaterGlossDamper, 0, 3));
		SetShaderParam("TERRAIN_Refraction0123", getVector(TERRAIN_Refraction, 0, 3));
		SetShaderParam("TERRAIN_WetRefraction0123", getVector(TERRAIN_WetRefraction, 0, 3));
		SetShaderParam("TERRAIN_Flow0123", getVector(TERRAIN_Flow, 0, 3));
		SetShaderParam("TERRAIN_WetFlow0123", getVector(TERRAIN_WetFlow, 0, 3));
		SetShaderParam("TERRAIN_WaterMetallic0123", getVector(TERRAIN_WaterMetallic, 0, 3));
		SetShaderParam("TERRAIN_WetGloss0123", getVector(TERRAIN_WetGloss, 0, 3));
		SetShaderParam("TERRAIN_WaterColorR0123", getColorVector(TERRAIN_WaterColor, 0, 3, 0));
		SetShaderParam("TERRAIN_WaterColorG0123", getColorVector(TERRAIN_WaterColor, 0, 3, 1));
		SetShaderParam("TERRAIN_WaterColorB0123", getColorVector(TERRAIN_WaterColor, 0, 3, 2));
		SetShaderParam("TERRAIN_WaterColorA0123", getColorVector(TERRAIN_WaterColor, 0, 3, 3));
		SetShaderParam("TERRAIN_WaterEmission0123", getVector(TERRAIN_WaterEmission, 0, 3));
		SetShaderParam("_GlitterStrength0123", getVector(_GlitterStrength, 0, 3));
		SetShaderParam("RTP_AO_0123", getVector(AO_strength, 0, 3));
		SetShaderParam("_VerticalTexture0123", getVector(VerticalTextureStrength, 0, 3));
		if (numLayers > 4 && _4LAYERS_SHADER_USED)
		{
			SetShaderParam("_FarSpecCorrection89AB", getVector(FarSpecCorrection, 4, 7));
			SetShaderParam("_MIPmult89AB", getVector(MIPmult, 4, 7));
			SetShaderParam("_MixScale89AB", getVector(MixScale, 4, 7));
			SetShaderParam("_MixBlend89AB", getVector(MixBlend, 4, 7));
			SetShaderParam("_MixSaturation89AB", getVector(MixSaturation, 4, 7));
			SetShaderParam("RTP_DiffFresnel89AB", getVector(RTP_DiffFresnel, 4, 7));
			SetShaderParam("RTP_metallic89AB", getVector(RTP_metallic, 4, 7));
			SetShaderParam("RTP_glossMin89AB", getVector(RTP_glossMin, 4, 7));
			SetShaderParam("RTP_glossMax89AB", getVector(RTP_glossMax, 4, 7));
			SetShaderParam("RTP_glitter89AB", getVector(RTP_glitter, 4, 7));
			SetShaderParam("_MixBrightness89AB", getVector(MixBrightness, 4, 7));
			SetShaderParam("_MixReplace89AB", getVector(MixReplace, 4, 7));
			SetShaderParam("_LayerBrightness89AB", MasterLayerBrightness * getVector(LayerBrightness, 4, 7));
			SetShaderParam("_LayerSaturation89AB", MasterLayerSaturation * getVector(LayerSaturation, 4, 7));
			SetShaderParam("_LayerEmission89AB", getVector(LayerEmission, 4, 7));
			SetShaderParam("_LayerEmissionColorR89AB", getColorVector(LayerEmissionColor, 4, 7, 0));
			SetShaderParam("_LayerEmissionColorG89AB", getColorVector(LayerEmissionColor, 4, 7, 1));
			SetShaderParam("_LayerEmissionColorB89AB", getColorVector(LayerEmissionColor, 4, 7, 2));
			SetShaderParam("_LayerEmissionColorA89AB", getColorVector(LayerEmissionColor, 4, 7, 3));
			SetShaderParam("_LayerBrightness2Spec89AB", getVector(LayerBrightness2Spec, 4, 7));
			SetShaderParam("_LayerAlbedo2SpecColor89AB", getVector(LayerAlbedo2SpecColor, 4, 7));
			SetShaderParam("_LayerEmissionRefractStrength89AB", getVector(LayerEmissionRefractStrength, 4, 7));
			SetShaderParam("_LayerEmissionRefractHBedge89AB", getVector(LayerEmissionRefractHBedge, 4, 7));
			SetShaderParam("_GlobalColorPerLayer89AB", getVector(GlobalColorPerLayer, 4, 7));
			SetShaderParam("_GlobalColorBottom89AB", getVector(GlobalColorBottom, 4, 7));
			SetShaderParam("_GlobalColorTop89AB", getVector(GlobalColorTop, 4, 7));
			SetShaderParam("_GlobalColorColormapLoSat89AB", getVector(GlobalColorColormapLoSat, 4, 7));
			SetShaderParam("_GlobalColorColormapHiSat89AB", getVector(GlobalColorColormapHiSat, 4, 7));
			SetShaderParam("_GlobalColorLayerLoSat89AB", getVector(GlobalColorLayerLoSat, 4, 7));
			SetShaderParam("_GlobalColorLayerHiSat89AB", getVector(GlobalColorLayerHiSat, 4, 7));
			SetShaderParam("_GlobalColorLoBlend89AB", getVector(GlobalColorLoBlend, 4, 7));
			SetShaderParam("_GlobalColorHiBlend89AB", getVector(GlobalColorHiBlend, 4, 7));
			SetShaderParam("PER_LAYER_HEIGHT_MODIFIER89AB", getVector(PER_LAYER_HEIGHT_MODIFIER, 4, 7));
			SetShaderParam("rtp_snow_strength_per_layer89AB", getVector(_snow_strength_per_layer, 4, 7));
			SetShaderParam("_SuperDetailStrengthMultA89AB", getVector(_SuperDetailStrengthMultA, 4, 7));
			SetShaderParam("_SuperDetailStrengthMultB89AB", getVector(_SuperDetailStrengthMultB, 4, 7));
			SetShaderParam("_SuperDetailStrengthNormal89AB", getVector(_SuperDetailStrengthNormal, 4, 7));
			SetShaderParam("_BumpMapGlobalStrength89AB", getVector(_BumpMapGlobalStrength, 4, 7));
			SetShaderParam("_SuperDetailStrengthMultASelfMaskNear89AB", getVector(_SuperDetailStrengthMultASelfMaskNear, 4, 7));
			SetShaderParam("_SuperDetailStrengthMultASelfMaskFar89AB", getVector(_SuperDetailStrengthMultASelfMaskFar, 4, 7));
			SetShaderParam("_SuperDetailStrengthMultBSelfMaskNear89AB", getVector(_SuperDetailStrengthMultBSelfMaskNear, 4, 7));
			SetShaderParam("_SuperDetailStrengthMultBSelfMaskFar89AB", getVector(_SuperDetailStrengthMultBSelfMaskFar, 4, 7));
			SetShaderParam("TERRAIN_LayerWetStrength89AB", getVector(TERRAIN_LayerWetStrength, 4, 7));
			SetShaderParam("TERRAIN_WaterLevel89AB", getVector(TERRAIN_WaterLevel, 4, 7));
			SetShaderParam("TERRAIN_WaterLevelSlopeDamp89AB", getVector(TERRAIN_WaterLevelSlopeDamp, 4, 7));
			SetShaderParam("TERRAIN_WaterEdge89AB", getVector(TERRAIN_WaterEdge, 4, 7));
			SetShaderParam("TERRAIN_WaterGloss89AB", getVector(TERRAIN_WaterGloss, 4, 7));
			SetShaderParam("TERRAIN_WaterGlossDamper89AB", getVector(TERRAIN_WaterGlossDamper, 4, 7));
			SetShaderParam("TERRAIN_Refraction89AB", getVector(TERRAIN_Refraction, 4, 7));
			SetShaderParam("TERRAIN_WetRefraction89AB", getVector(TERRAIN_WetRefraction, 4, 7));
			SetShaderParam("TERRAIN_Flow89AB", getVector(TERRAIN_Flow, 4, 7));
			SetShaderParam("TERRAIN_WetFlow89AB", getVector(TERRAIN_WetFlow, 4, 7));
			SetShaderParam("TERRAIN_WaterMetallic89AB", getVector(TERRAIN_WaterMetallic, 4, 7));
			SetShaderParam("TERRAIN_WetGloss89AB", getVector(TERRAIN_WetGloss, 4, 7));
			SetShaderParam("TERRAIN_WaterColorR89AB", getColorVector(TERRAIN_WaterColor, 4, 7, 0));
			SetShaderParam("TERRAIN_WaterColorG89AB", getColorVector(TERRAIN_WaterColor, 4, 7, 1));
			SetShaderParam("TERRAIN_WaterColorB89AB", getColorVector(TERRAIN_WaterColor, 4, 7, 2));
			SetShaderParam("TERRAIN_WaterColorA89AB", getColorVector(TERRAIN_WaterColor, 4, 7, 3));
			SetShaderParam("TERRAIN_WaterEmission89AB", getVector(TERRAIN_WaterEmission, 4, 7));
			SetShaderParam("_GlitterStrength89AB", getVector(_GlitterStrength, 4, 7));
			SetShaderParam("RTP_AO_89AB", getVector(AO_strength, 4, 7));
			SetShaderParam("_VerticalTexture89AB", getVector(VerticalTextureStrength, 4, 7));
		}
		else
		{
			SetShaderParam("_FarSpecCorrection4567", getVector(FarSpecCorrection, 4, 7));
			SetShaderParam("_MIPmult4567", getVector(MIPmult, 4, 7));
			SetShaderParam("_MixScale4567", getVector(MixScale, 4, 7));
			SetShaderParam("_MixBlend4567", getVector(MixBlend, 4, 7));
			SetShaderParam("_MixSaturation4567", getVector(MixSaturation, 4, 7));
			SetShaderParam("RTP_DiffFresnel4567", getVector(RTP_DiffFresnel, 4, 7));
			SetShaderParam("RTP_metallic4567", getVector(RTP_metallic, 4, 7));
			SetShaderParam("RTP_glossMin4567", getVector(RTP_glossMin, 4, 7));
			SetShaderParam("RTP_glossMax4567", getVector(RTP_glossMax, 4, 7));
			SetShaderParam("RTP_glitter4567", getVector(RTP_glitter, 4, 7));
			SetShaderParam("_MixBrightness4567", getVector(MixBrightness, 4, 7));
			SetShaderParam("_MixReplace4567", getVector(MixReplace, 4, 7));
			SetShaderParam("_LayerBrightness4567", MasterLayerBrightness * getVector(LayerBrightness, 4, 7));
			SetShaderParam("_LayerSaturation4567", MasterLayerSaturation * getVector(LayerSaturation, 4, 7));
			SetShaderParam("_LayerEmission4567", getVector(LayerEmission, 4, 7));
			SetShaderParam("_LayerEmissionColorR4567", getColorVector(LayerEmissionColor, 4, 7, 0));
			SetShaderParam("_LayerEmissionColorG4567", getColorVector(LayerEmissionColor, 4, 7, 1));
			SetShaderParam("_LayerEmissionColorB4567", getColorVector(LayerEmissionColor, 4, 7, 2));
			SetShaderParam("_LayerEmissionColorA4567", getColorVector(LayerEmissionColor, 4, 7, 3));
			SetShaderParam("_LayerBrightness2Spec4567", getVector(LayerBrightness2Spec, 4, 7));
			SetShaderParam("_LayerAlbedo2SpecColor4567", getVector(LayerAlbedo2SpecColor, 4, 7));
			SetShaderParam("_LayerEmissionRefractStrength4567", getVector(LayerEmissionRefractStrength, 4, 7));
			SetShaderParam("_LayerEmissionRefractHBedge4567", getVector(LayerEmissionRefractHBedge, 4, 7));
			SetShaderParam("_GlobalColorPerLayer4567", getVector(GlobalColorPerLayer, 4, 7));
			SetShaderParam("_GlobalColorBottom4567", getVector(GlobalColorBottom, 4, 7));
			SetShaderParam("_GlobalColorTop4567", getVector(GlobalColorTop, 4, 7));
			SetShaderParam("_GlobalColorColormapLoSat4567", getVector(GlobalColorColormapLoSat, 4, 7));
			SetShaderParam("_GlobalColorColormapHiSat4567", getVector(GlobalColorColormapHiSat, 4, 7));
			SetShaderParam("_GlobalColorLayerLoSat4567", getVector(GlobalColorLayerLoSat, 4, 7));
			SetShaderParam("_GlobalColorLayerHiSat4567", getVector(GlobalColorLayerHiSat, 4, 7));
			SetShaderParam("_GlobalColorLoBlend4567", getVector(GlobalColorLoBlend, 4, 7));
			SetShaderParam("_GlobalColorHiBlend4567", getVector(GlobalColorHiBlend, 4, 7));
			SetShaderParam("PER_LAYER_HEIGHT_MODIFIER4567", getVector(PER_LAYER_HEIGHT_MODIFIER, 4, 7));
			SetShaderParam("rtp_snow_strength_per_layer4567", getVector(_snow_strength_per_layer, 4, 7));
			SetShaderParam("_SuperDetailStrengthMultA4567", getVector(_SuperDetailStrengthMultA, 4, 7));
			SetShaderParam("_SuperDetailStrengthMultB4567", getVector(_SuperDetailStrengthMultB, 4, 7));
			SetShaderParam("_SuperDetailStrengthNormal4567", getVector(_SuperDetailStrengthNormal, 4, 7));
			SetShaderParam("_BumpMapGlobalStrength4567", getVector(_BumpMapGlobalStrength, 4, 7));
			SetShaderParam("_SuperDetailStrengthMultASelfMaskNear4567", getVector(_SuperDetailStrengthMultASelfMaskNear, 4, 7));
			SetShaderParam("_SuperDetailStrengthMultASelfMaskFar4567", getVector(_SuperDetailStrengthMultASelfMaskFar, 4, 7));
			SetShaderParam("_SuperDetailStrengthMultBSelfMaskNear4567", getVector(_SuperDetailStrengthMultBSelfMaskNear, 4, 7));
			SetShaderParam("_SuperDetailStrengthMultBSelfMaskFar4567", getVector(_SuperDetailStrengthMultBSelfMaskFar, 4, 7));
			SetShaderParam("TERRAIN_LayerWetStrength4567", getVector(TERRAIN_LayerWetStrength, 4, 7));
			SetShaderParam("TERRAIN_WaterLevel4567", getVector(TERRAIN_WaterLevel, 4, 7));
			SetShaderParam("TERRAIN_WaterLevelSlopeDamp4567", getVector(TERRAIN_WaterLevelSlopeDamp, 4, 7));
			SetShaderParam("TERRAIN_WaterEdge4567", getVector(TERRAIN_WaterEdge, 4, 7));
			SetShaderParam("TERRAIN_WaterGloss4567", getVector(TERRAIN_WaterGloss, 4, 7));
			SetShaderParam("TERRAIN_WaterGlossDamper4567", getVector(TERRAIN_WaterGlossDamper, 4, 7));
			SetShaderParam("TERRAIN_Refraction4567", getVector(TERRAIN_Refraction, 4, 7));
			SetShaderParam("TERRAIN_WetRefraction4567", getVector(TERRAIN_WetRefraction, 4, 7));
			SetShaderParam("TERRAIN_Flow4567", getVector(TERRAIN_Flow, 4, 7));
			SetShaderParam("TERRAIN_WetFlow4567", getVector(TERRAIN_WetFlow, 4, 7));
			SetShaderParam("TERRAIN_WaterMetallic4567", getVector(TERRAIN_WaterMetallic, 4, 7));
			SetShaderParam("TERRAIN_WetGloss4567", getVector(TERRAIN_WetGloss, 4, 7));
			SetShaderParam("TERRAIN_WaterColorR4567", getColorVector(TERRAIN_WaterColor, 4, 7, 0));
			SetShaderParam("TERRAIN_WaterColorG4567", getColorVector(TERRAIN_WaterColor, 4, 7, 1));
			SetShaderParam("TERRAIN_WaterColorB4567", getColorVector(TERRAIN_WaterColor, 4, 7, 2));
			SetShaderParam("TERRAIN_WaterColorA4567", getColorVector(TERRAIN_WaterColor, 4, 7, 3));
			SetShaderParam("TERRAIN_WaterEmission4567", getVector(TERRAIN_WaterEmission, 4, 7));
			SetShaderParam("_GlitterStrength4567", getVector(_GlitterStrength, 4, 7));
			SetShaderParam("RTP_AO_4567", getVector(AO_strength, 4, 7));
			SetShaderParam("_VerticalTexture4567", getVector(VerticalTextureStrength, 4, 7));
			SetShaderParam("_FarSpecCorrection89AB", getVector(FarSpecCorrection, 8, 11));
			SetShaderParam("_MIPmult89AB", getVector(MIPmult, 8, 11));
			SetShaderParam("_MixScale89AB", getVector(MixScale, 8, 11));
			SetShaderParam("_MixBlend89AB", getVector(MixBlend, 8, 11));
			SetShaderParam("_MixSaturation89AB", getVector(MixSaturation, 8, 11));
			SetShaderParam("RTP_DiffFresnel89AB", getVector(RTP_DiffFresnel, 8, 11));
			SetShaderParam("RTP_metallic89AB", getVector(RTP_metallic, 8, 11));
			SetShaderParam("RTP_glossMin89AB", getVector(RTP_glossMin, 8, 11));
			SetShaderParam("RTP_glossMax89AB", getVector(RTP_glossMax, 8, 11));
			SetShaderParam("RTP_glitter89AB", getVector(RTP_glitter, 8, 11));
			SetShaderParam("_MixBrightness89AB", getVector(MixBrightness, 8, 11));
			SetShaderParam("_MixReplace89AB", getVector(MixReplace, 8, 11));
			SetShaderParam("_LayerBrightness89AB", MasterLayerBrightness * getVector(LayerBrightness, 8, 11));
			SetShaderParam("_LayerSaturation89AB", MasterLayerSaturation * getVector(LayerSaturation, 8, 11));
			SetShaderParam("_LayerEmission89AB", getVector(LayerEmission, 8, 11));
			SetShaderParam("_LayerEmissionColorR89AB", getColorVector(LayerEmissionColor, 8, 11, 0));
			SetShaderParam("_LayerEmissionColorG89AB", getColorVector(LayerEmissionColor, 8, 11, 1));
			SetShaderParam("_LayerEmissionColorB89AB", getColorVector(LayerEmissionColor, 8, 11, 2));
			SetShaderParam("_LayerEmissionColorA89AB", getColorVector(LayerEmissionColor, 8, 11, 3));
			SetShaderParam("_LayerBrightness2Spec89AB", getVector(LayerBrightness2Spec, 8, 11));
			SetShaderParam("_LayerAlbedo2SpecColor89AB", getVector(LayerAlbedo2SpecColor, 8, 11));
			SetShaderParam("_LayerEmissionRefractStrength89AB", getVector(LayerEmissionRefractStrength, 8, 11));
			SetShaderParam("_LayerEmissionRefractHBedge89AB", getVector(LayerEmissionRefractHBedge, 8, 11));
			SetShaderParam("_GlobalColorPerLayer89AB", getVector(GlobalColorPerLayer, 8, 11));
			SetShaderParam("_GlobalColorBottom89AB", getVector(GlobalColorBottom, 8, 11));
			SetShaderParam("_GlobalColorTop89AB", getVector(GlobalColorTop, 8, 11));
			SetShaderParam("_GlobalColorColormapLoSat89AB", getVector(GlobalColorColormapLoSat, 8, 11));
			SetShaderParam("_GlobalColorColormapHiSat89AB", getVector(GlobalColorColormapHiSat, 8, 11));
			SetShaderParam("_GlobalColorLayerLoSat89AB", getVector(GlobalColorLayerLoSat, 8, 11));
			SetShaderParam("_GlobalColorLayerHiSat89AB", getVector(GlobalColorLayerHiSat, 8, 11));
			SetShaderParam("_GlobalColorLoBlend89AB", getVector(GlobalColorLoBlend, 8, 11));
			SetShaderParam("_GlobalColorHiBlend89AB", getVector(GlobalColorHiBlend, 8, 11));
			SetShaderParam("PER_LAYER_HEIGHT_MODIFIER89AB", getVector(PER_LAYER_HEIGHT_MODIFIER, 8, 11));
			SetShaderParam("rtp_snow_strength_per_layer89AB", getVector(_snow_strength_per_layer, 8, 11));
			SetShaderParam("_SuperDetailStrengthMultA89AB", getVector(_SuperDetailStrengthMultA, 8, 11));
			SetShaderParam("_SuperDetailStrengthMultB89AB", getVector(_SuperDetailStrengthMultB, 8, 11));
			SetShaderParam("_SuperDetailStrengthNormal89AB", getVector(_SuperDetailStrengthNormal, 8, 11));
			SetShaderParam("_BumpMapGlobalStrength89AB", getVector(_BumpMapGlobalStrength, 8, 11));
			SetShaderParam("_SuperDetailStrengthMultASelfMaskNear89AB", getVector(_SuperDetailStrengthMultASelfMaskNear, 8, 11));
			SetShaderParam("_SuperDetailStrengthMultASelfMaskFar89AB", getVector(_SuperDetailStrengthMultASelfMaskFar, 8, 11));
			SetShaderParam("_SuperDetailStrengthMultBSelfMaskNear89AB", getVector(_SuperDetailStrengthMultBSelfMaskNear, 8, 11));
			SetShaderParam("_SuperDetailStrengthMultBSelfMaskFar89AB", getVector(_SuperDetailStrengthMultBSelfMaskFar, 8, 11));
			SetShaderParam("TERRAIN_LayerWetStrength89AB", getVector(TERRAIN_LayerWetStrength, 8, 11));
			SetShaderParam("TERRAIN_WaterLevel89AB", getVector(TERRAIN_WaterLevel, 8, 11));
			SetShaderParam("TERRAIN_WaterLevelSlopeDamp89AB", getVector(TERRAIN_WaterLevelSlopeDamp, 8, 11));
			SetShaderParam("TERRAIN_WaterEdge89AB", getVector(TERRAIN_WaterEdge, 8, 11));
			SetShaderParam("TERRAIN_WaterGloss89AB", getVector(TERRAIN_WaterGloss, 8, 11));
			SetShaderParam("TERRAIN_WaterGlossDamper89AB", getVector(TERRAIN_WaterGlossDamper, 8, 11));
			SetShaderParam("TERRAIN_Refraction89AB", getVector(TERRAIN_Refraction, 8, 11));
			SetShaderParam("TERRAIN_WetRefraction89AB", getVector(TERRAIN_WetRefraction, 8, 11));
			SetShaderParam("TERRAIN_Flow89AB", getVector(TERRAIN_Flow, 8, 11));
			SetShaderParam("TERRAIN_WetFlow89AB", getVector(TERRAIN_WetFlow, 8, 11));
			SetShaderParam("TERRAIN_WaterMetallic89AB", getVector(TERRAIN_WaterMetallic, 8, 11));
			SetShaderParam("TERRAIN_WetGloss89AB", getVector(TERRAIN_WetGloss, 8, 11));
			SetShaderParam("TERRAIN_WaterColorR89AB", getColorVector(TERRAIN_WaterColor, 8, 11, 0));
			SetShaderParam("TERRAIN_WaterColorG89AB", getColorVector(TERRAIN_WaterColor, 8, 11, 1));
			SetShaderParam("TERRAIN_WaterColorB89AB", getColorVector(TERRAIN_WaterColor, 8, 11, 2));
			SetShaderParam("TERRAIN_WaterColorA89AB", getColorVector(TERRAIN_WaterColor, 8, 11, 3));
			SetShaderParam("TERRAIN_WaterEmission89AB", getVector(TERRAIN_WaterEmission, 8, 11));
			SetShaderParam("_GlitterStrength89AB", getVector(_GlitterStrength, 8, 11));
			SetShaderParam("RTP_AO_89AB", getVector(AO_strength, 8, 11));
			SetShaderParam("_VerticalTexture89AB", getVector(VerticalTextureStrength, 8, 11));
		}
		if (splat_atlases.Length == 2)
		{
			Texture2D texture2D = splat_atlases[0];
			Texture2D texture2D2 = splat_atlases[1];
			splat_atlases = new Texture2D[3];
			splat_atlases[0] = texture2D;
			splat_atlases[1] = texture2D2;
		}
		SetShaderParam("_SplatAtlasA", splat_atlases[0]);
		SetShaderParam("_BumpMap01", Bump01);
		SetShaderParam("_BumpMap23", Bump23);
		SetShaderParam("_TERRAIN_HeightMap", HeightMap);
		SetShaderParam("_SSColorCombinedA", SSColorCombinedA);
		if (numLayers > 4)
		{
			SetShaderParam("_SplatAtlasB", splat_atlases[1]);
			SetShaderParam("_SplatAtlasC", splat_atlases[1]);
			SetShaderParam("_TERRAIN_HeightMap2", HeightMap2);
			SetShaderParam("_SSColorCombinedB", SSColorCombinedB);
		}
		if (numLayers > 8)
		{
			SetShaderParam("_SplatAtlasC", splat_atlases[2]);
		}
		if (numLayers > 4 && _4LAYERS_SHADER_USED)
		{
			SetShaderParam("_BumpMap89", Bump45);
			SetShaderParam("_BumpMapAB", Bump67);
			SetShaderParam("_TERRAIN_HeightMap3", HeightMap2);
			SetShaderParam("_BumpMap45", Bump45);
			SetShaderParam("_BumpMap67", Bump67);
		}
		else
		{
			SetShaderParam("_BumpMap45", Bump45);
			SetShaderParam("_BumpMap67", Bump67);
			SetShaderParam("_BumpMap89", Bump89);
			SetShaderParam("_BumpMapAB", BumpAB);
			SetShaderParam("_TERRAIN_HeightMap3", HeightMap3);
		}
		use_mat = null;
	}

	public Vector4 getVector(float[] vec, int idxA, int idxB)
	{
		if (vec == null)
		{
			return Vector4.zero;
		}
		Vector4 zero = Vector4.zero;
		for (int i = idxA; i <= idxB; i++)
		{
			if (i < vec.Length)
			{
				zero[i - idxA] = vec[i];
			}
		}
		return zero;
	}

	public Vector4 getColorVector(Color[] vec, int idxA, int idxB, int channel)
	{
		if (vec == null)
		{
			return Vector4.zero;
		}
		Vector4 zero = Vector4.zero;
		for (int i = idxA; i <= idxB; i++)
		{
			if (i < vec.Length)
			{
				zero[i - idxA] = vec[i][channel];
			}
		}
		return zero;
	}

	public Texture2D get_dumb_tex()
	{
		if (!dumb_tex)
		{
			dumb_tex = new Texture2D(32, 32, TextureFormat.RGB24, mipChain: false);
			Color[] pixels = dumb_tex.GetPixels();
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] = Color.white;
			}
			dumb_tex.SetPixels(pixels);
			dumb_tex.Apply();
		}
		return dumb_tex;
	}

	public void SyncGlobalPropsAcrossTerrainGroups()
	{
		ReliefTerrain[] array = (ReliefTerrain[])UnityEngine.Object.FindObjectsOfType(typeof(ReliefTerrain));
		ReliefTerrainGlobalSettingsHolder[] array2 = new ReliefTerrainGlobalSettingsHolder[array.Length];
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			bool flag = false;
			for (int j = 0; j < num; j++)
			{
				if (array2[j] == array[i].globalSettingsHolder)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				array2[num++] = array[i].globalSettingsHolder;
			}
		}
		for (int k = 0; k < num; k++)
		{
			if (array2[k] != this)
			{
				array2[k].trees_shadow_distance_start = trees_shadow_distance_start;
				array2[k].trees_shadow_distance_transition = trees_shadow_distance_transition;
				array2[k].trees_shadow_value = trees_shadow_value;
				array2[k].global_normalMap_multiplier = global_normalMap_multiplier;
				array2[k].trees_pixel_distance_start = trees_pixel_distance_start;
				array2[k].trees_pixel_distance_transition = trees_pixel_distance_transition;
				array2[k].trees_pixel_blend_val = trees_pixel_blend_val;
				array2[k].global_normalMap_farUsage = global_normalMap_farUsage;
				array2[k]._AmbientEmissiveMultiplier = _AmbientEmissiveMultiplier;
				array2[k]._AmbientEmissiveRelief = _AmbientEmissiveRelief;
				array2[k]._snow_strength = _snow_strength;
				array2[k]._global_color_brightness_to_snow = _global_color_brightness_to_snow;
				array2[k]._snow_slope_factor = _snow_slope_factor;
				array2[k]._snow_edge_definition = _snow_edge_definition;
				array2[k]._snow_height_treshold = _snow_height_treshold;
				array2[k]._snow_height_transition = _snow_height_transition;
				array2[k]._snow_color = _snow_color;
				array2[k]._snow_gloss = _snow_gloss;
				array2[k]._snow_reflectivness = _snow_reflectivness;
				array2[k]._snow_deep_factor = _snow_deep_factor;
				array2[k]._snow_diff_fresnel = _snow_diff_fresnel;
				array2[k]._snow_metallic = _snow_metallic;
				array2[k]._snow_Frost = _snow_Frost;
				array2[k]._snow_MicroTiling = _snow_MicroTiling;
				array2[k]._snow_BumpMicro = _snow_BumpMicro;
				array2[k]._snow_occlusionStrength = _snow_occlusionStrength;
				array2[k]._snow_TranslucencyDeferredLightIndex = _snow_TranslucencyDeferredLightIndex;
				array2[k]._SnowGlitterColor = _SnowGlitterColor;
				array2[k]._GlitterColor = _GlitterColor;
				array2[k]._GlitterTiling = _GlitterTiling;
				array2[k]._GlitterDensity = _GlitterDensity;
				array2[k]._GlitterFilter = _GlitterFilter;
				array2[k]._GlitterColorization = _GlitterColorization;
				array2[k]._SparkleMap = _SparkleMap;
				array2[k].TERRAIN_CausticsWaterLevel = TERRAIN_CausticsWaterLevel;
				array2[k].TERRAIN_CausticsWaterLevelByAngle = TERRAIN_CausticsWaterLevelByAngle;
				array2[k].TERRAIN_CausticsWaterDeepFadeLength = TERRAIN_CausticsWaterDeepFadeLength;
				array2[k].TERRAIN_CausticsWaterShallowFadeLength = TERRAIN_CausticsWaterShallowFadeLength;
				array2[k].TERRAIN_GlobalWetness = TERRAIN_GlobalWetness;
				array2[k].TERRAIN_RainIntensity = TERRAIN_RainIntensity;
				array2[k].RTP_LightDefVector = RTP_LightDefVector;
			}
		}
	}

	public void RestorePreset(ReliefTerrainPresetHolder holder)
	{
		terrainLayers = holder.terrainLayers;
		numLayers = holder.numLayers;
		splats = new Texture2D[holder.splats.Length];
		for (int i = 0; i < holder.splats.Length; i++)
		{
			splats[i] = holder.splats[i];
		}
		splat_atlases = new Texture2D[3];
		for (int j = 0; j < splat_atlases.Length; j++)
		{
			splat_atlases[j] = holder.splat_atlases[j];
		}
		RTP_MIP_BIAS = holder.RTP_MIP_BIAS;
		MasterLayerBrightness = holder.MasterLayerBrightness;
		MasterLayerSaturation = holder.MasterLayerSaturation;
		SuperDetailA_channel = holder.SuperDetailA_channel;
		SuperDetailB_channel = holder.SuperDetailB_channel;
		Bump01 = holder.Bump01;
		Bump23 = holder.Bump23;
		Bump45 = holder.Bump45;
		Bump67 = holder.Bump67;
		Bump89 = holder.Bump89;
		BumpAB = holder.BumpAB;
		SSColorCombinedA = holder.SSColorCombinedA;
		SSColorCombinedB = holder.SSColorCombinedB;
		BumpGlobal = holder.BumpGlobal;
		VerticalTexture = holder.VerticalTexture;
		BumpMapGlobalScale = holder.BumpMapGlobalScale;
		GlobalColorMapBlendValues = holder.GlobalColorMapBlendValues;
		GlobalColorMapSaturation = holder.GlobalColorMapSaturation;
		GlobalColorMapSaturationFar = holder.GlobalColorMapSaturationFar;
		GlobalColorMapDistortByPerlin = holder.GlobalColorMapDistortByPerlin;
		GlobalColorMapBrightness = holder.GlobalColorMapBrightness;
		GlobalColorMapBrightnessFar = holder.GlobalColorMapBrightnessFar;
		_GlobalColorMapNearMIP = holder._GlobalColorMapNearMIP;
		_FarNormalDamp = holder._FarNormalDamp;
		blendMultiplier = holder.blendMultiplier;
		HeightMap = holder.HeightMap;
		HeightMap2 = holder.HeightMap2;
		HeightMap3 = holder.HeightMap3;
		ReliefTransform = holder.ReliefTransform;
		DIST_STEPS = holder.DIST_STEPS;
		WAVELENGTH = holder.WAVELENGTH;
		ReliefBorderBlend = holder.ReliefBorderBlend;
		ExtrudeHeight = holder.ExtrudeHeight;
		LightmapShading = holder.LightmapShading;
		SHADOW_STEPS = holder.SHADOW_STEPS;
		WAVELENGTH_SHADOWS = holder.WAVELENGTH_SHADOWS;
		SelfShadowStrength = holder.SelfShadowStrength;
		ShadowSmoothing = holder.ShadowSmoothing;
		ShadowSoftnessFade = holder.ShadowSoftnessFade;
		distance_start = holder.distance_start;
		distance_transition = holder.distance_transition;
		distance_start_bumpglobal = holder.distance_start_bumpglobal;
		distance_transition_bumpglobal = holder.distance_transition_bumpglobal;
		rtp_perlin_start_val = holder.rtp_perlin_start_val;
		_Phong = holder._Phong;
		tessHeight = holder.tessHeight;
		_TessSubdivisions = holder._TessSubdivisions;
		_TessSubdivisionsFar = holder._TessSubdivisionsFar;
		_TessYOffset = holder._TessYOffset;
		trees_shadow_distance_start = holder.trees_shadow_distance_start;
		trees_shadow_distance_transition = holder.trees_shadow_distance_transition;
		trees_shadow_value = holder.trees_shadow_value;
		trees_pixel_distance_start = holder.trees_pixel_distance_start;
		trees_pixel_distance_transition = holder.trees_pixel_distance_transition;
		trees_pixel_blend_val = holder.trees_pixel_blend_val;
		global_normalMap_multiplier = holder.global_normalMap_multiplier;
		global_normalMap_farUsage = holder.global_normalMap_farUsage;
		_AmbientEmissiveMultiplier = holder._AmbientEmissiveMultiplier;
		_AmbientEmissiveRelief = holder._AmbientEmissiveRelief;
		rtp_mipoffset_globalnorm = holder.rtp_mipoffset_globalnorm;
		_SuperDetailTiling = holder._SuperDetailTiling;
		SuperDetailA = holder.SuperDetailA;
		SuperDetailB = holder.SuperDetailB;
		TERRAIN_GlobalWetness = holder.TERRAIN_GlobalWetness;
		TERRAIN_RippleMap = holder.TERRAIN_RippleMap;
		TERRAIN_RippleScale = holder.TERRAIN_RippleScale;
		TERRAIN_FlowScale = holder.TERRAIN_FlowScale;
		TERRAIN_FlowSpeed = holder.TERRAIN_FlowSpeed;
		TERRAIN_FlowCycleScale = holder.TERRAIN_FlowCycleScale;
		TERRAIN_FlowMipOffset = holder.TERRAIN_FlowMipOffset;
		TERRAIN_WetDarkening = holder.TERRAIN_WetDarkening;
		TERRAIN_WetDropletsStrength = holder.TERRAIN_WetDropletsStrength;
		TERRAIN_WetHeight_Treshold = holder.TERRAIN_WetHeight_Treshold;
		TERRAIN_WetHeight_Transition = holder.TERRAIN_WetHeight_Transition;
		TERRAIN_RainIntensity = holder.TERRAIN_RainIntensity;
		TERRAIN_DropletsSpeed = holder.TERRAIN_DropletsSpeed;
		TERRAIN_mipoffset_flowSpeed = holder.TERRAIN_mipoffset_flowSpeed;
		TERRAIN_CausticsAnimSpeed = holder.TERRAIN_CausticsAnimSpeed;
		TERRAIN_CausticsColor = holder.TERRAIN_CausticsColor;
		TERRAIN_CausticsWaterLevel = holder.TERRAIN_CausticsWaterLevel;
		TERRAIN_CausticsWaterLevelByAngle = holder.TERRAIN_CausticsWaterLevelByAngle;
		TERRAIN_CausticsWaterDeepFadeLength = holder.TERRAIN_CausticsWaterDeepFadeLength;
		TERRAIN_CausticsWaterShallowFadeLength = holder.TERRAIN_CausticsWaterShallowFadeLength;
		TERRAIN_CausticsTilingScale = holder.TERRAIN_CausticsTilingScale;
		TERRAIN_CausticsTex = holder.TERRAIN_CausticsTex;
		RTP_AOsharpness = holder.RTP_AOsharpness;
		RTP_AOamp = holder.RTP_AOamp;
		_occlusionStrength = holder._occlusionStrength;
		RTP_LightDefVector = holder.RTP_LightDefVector;
		EmissionRefractFiltering = holder.EmissionRefractFiltering;
		EmissionRefractAnimSpeed = holder.EmissionRefractAnimSpeed;
		VerticalTextureGlobalBumpInfluence = holder.VerticalTextureGlobalBumpInfluence;
		VerticalTextureTiling = holder.VerticalTextureTiling;
		_snow_strength = holder._snow_strength;
		_global_color_brightness_to_snow = holder._global_color_brightness_to_snow;
		_snow_slope_factor = holder._snow_slope_factor;
		_snow_edge_definition = holder._snow_edge_definition;
		_snow_height_treshold = holder._snow_height_treshold;
		_snow_height_transition = holder._snow_height_transition;
		_snow_color = holder._snow_color;
		_snow_gloss = holder._snow_gloss;
		_snow_reflectivness = holder._snow_reflectivness;
		_snow_deep_factor = holder._snow_deep_factor;
		_snow_diff_fresnel = holder._snow_diff_fresnel;
		_snow_metallic = holder._snow_metallic;
		_snow_Frost = holder._snow_Frost;
		_snow_MicroTiling = holder._snow_MicroTiling;
		_snow_BumpMicro = holder._snow_BumpMicro;
		_snow_occlusionStrength = holder._snow_occlusionStrength;
		_snow_TranslucencyDeferredLightIndex = holder._snow_TranslucencyDeferredLightIndex;
		_SnowGlitterColor = holder._SnowGlitterColor;
		_GlitterColor = holder._GlitterColor;
		_GlitterTiling = holder._GlitterTiling;
		_GlitterDensity = holder._GlitterDensity;
		_GlitterFilter = holder._GlitterFilter;
		_GlitterColorization = holder._GlitterColorization;
		_SparkleMap = holder._SparkleMap;
		Bumps = new Texture2D[holder.Bumps.Length];
		FarSpecCorrection = new float[holder.Bumps.Length];
		MixScale = new float[holder.Bumps.Length];
		MixBlend = new float[holder.Bumps.Length];
		MixSaturation = new float[holder.Bumps.Length];
		RTP_DiffFresnel = new float[holder.Bumps.Length];
		RTP_metallic = new float[holder.Bumps.Length];
		RTP_glossMin = new float[holder.Bumps.Length];
		RTP_glossMax = new float[holder.Bumps.Length];
		RTP_glitter = new float[holder.Bumps.Length];
		MixBrightness = new float[holder.Bumps.Length];
		MixReplace = new float[holder.Bumps.Length];
		LayerBrightness = new float[holder.Bumps.Length];
		LayerBrightness2Spec = new float[holder.Bumps.Length];
		LayerAlbedo2SpecColor = new float[holder.Bumps.Length];
		LayerSaturation = new float[holder.Bumps.Length];
		LayerEmission = new float[holder.Bumps.Length];
		LayerEmissionColor = new Color[holder.Bumps.Length];
		LayerEmissionRefractStrength = new float[holder.Bumps.Length];
		LayerEmissionRefractHBedge = new float[holder.Bumps.Length];
		GlobalColorPerLayer = new float[holder.Bumps.Length];
		GlobalColorBottom = new float[holder.Bumps.Length];
		GlobalColorTop = new float[holder.Bumps.Length];
		GlobalColorColormapLoSat = new float[holder.Bumps.Length];
		GlobalColorColormapHiSat = new float[holder.Bumps.Length];
		GlobalColorLayerLoSat = new float[holder.Bumps.Length];
		GlobalColorLayerHiSat = new float[holder.Bumps.Length];
		GlobalColorLoBlend = new float[holder.Bumps.Length];
		GlobalColorHiBlend = new float[holder.Bumps.Length];
		PER_LAYER_HEIGHT_MODIFIER = new float[holder.Bumps.Length];
		_SuperDetailStrengthMultA = new float[holder.Bumps.Length];
		_SuperDetailStrengthMultASelfMaskNear = new float[holder.Bumps.Length];
		_SuperDetailStrengthMultASelfMaskFar = new float[holder.Bumps.Length];
		_SuperDetailStrengthMultB = new float[holder.Bumps.Length];
		_SuperDetailStrengthMultBSelfMaskNear = new float[holder.Bumps.Length];
		_SuperDetailStrengthMultBSelfMaskFar = new float[holder.Bumps.Length];
		_SuperDetailStrengthNormal = new float[holder.Bumps.Length];
		_BumpMapGlobalStrength = new float[holder.Bumps.Length];
		AO_strength = new float[holder.Bumps.Length];
		VerticalTextureStrength = new float[holder.Bumps.Length];
		Heights = new Texture2D[holder.Bumps.Length];
		_snow_strength_per_layer = new float[holder.Bumps.Length];
		TERRAIN_LayerWetStrength = new float[holder.Bumps.Length];
		TERRAIN_WaterLevel = new float[holder.Bumps.Length];
		TERRAIN_WaterLevelSlopeDamp = new float[holder.Bumps.Length];
		TERRAIN_WaterEdge = new float[holder.Bumps.Length];
		TERRAIN_WaterGloss = new float[holder.Bumps.Length];
		TERRAIN_WaterGlossDamper = new float[holder.Bumps.Length];
		TERRAIN_Refraction = new float[holder.Bumps.Length];
		TERRAIN_WetRefraction = new float[holder.Bumps.Length];
		TERRAIN_Flow = new float[holder.Bumps.Length];
		TERRAIN_WetFlow = new float[holder.Bumps.Length];
		TERRAIN_WaterMetallic = new float[holder.Bumps.Length];
		TERRAIN_WetGloss = new float[holder.Bumps.Length];
		TERRAIN_WaterColor = new Color[holder.Bumps.Length];
		TERRAIN_WaterEmission = new float[holder.Bumps.Length];
		_GlitterStrength = new float[holder.Bumps.Length];
		for (int k = 0; k < holder.Bumps.Length; k++)
		{
			Bumps[k] = holder.Bumps[k];
			FarSpecCorrection[k] = holder.FarSpecCorrection[k];
			MixScale[k] = holder.MixScale[k];
			MixBlend[k] = holder.MixBlend[k];
			MixSaturation[k] = holder.MixSaturation[k];
			CheckAndUpdate(ref holder.RTP_DiffFresnel, 0f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.RTP_metallic, 0f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.RTP_glossMin, 0f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.RTP_glossMax, 1f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.RTP_glitter, 0f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.TERRAIN_WaterGloss, 0.1f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.TERRAIN_WaterGlossDamper, 0f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.TERRAIN_WaterMetallic, 0.1f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.TERRAIN_WetGloss, 0.05f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.TERRAIN_WetFlow, 0.05f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.MixBrightness, 2f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.MixReplace, 0f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.LayerBrightness, 1f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.LayerBrightness2Spec, 0f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.LayerAlbedo2SpecColor, 0f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.LayerSaturation, 1f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.LayerEmission, 1f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.LayerEmissionColor, Color.black, holder.Bumps.Length);
			CheckAndUpdate(ref holder.FarSpecCorrection, 0f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.LayerEmissionRefractStrength, 0f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.LayerEmissionRefractHBedge, 0f, holder.Bumps.Length);
			CheckAndUpdate(ref holder.TERRAIN_WaterEmission, 0f, holder.Bumps.Length);
			CheckAndUpdate(ref holder._GlitterStrength, 0f, holder.Bumps.Length);
			RTP_DiffFresnel[k] = holder.RTP_DiffFresnel[k];
			RTP_metallic[k] = holder.RTP_metallic[k];
			RTP_glossMin[k] = holder.RTP_glossMin[k];
			RTP_glossMax[k] = holder.RTP_glossMax[k];
			RTP_glitter[k] = holder.RTP_glitter[k];
			MixBrightness[k] = holder.MixBrightness[k];
			MixReplace[k] = holder.MixReplace[k];
			LayerBrightness[k] = holder.LayerBrightness[k];
			LayerBrightness2Spec[k] = holder.LayerBrightness2Spec[k];
			LayerAlbedo2SpecColor[k] = holder.LayerAlbedo2SpecColor[k];
			LayerSaturation[k] = holder.LayerSaturation[k];
			LayerEmission[k] = holder.LayerEmission[k];
			LayerEmissionColor[k] = holder.LayerEmissionColor[k];
			LayerEmissionRefractStrength[k] = holder.LayerEmissionRefractStrength[k];
			LayerEmissionRefractHBedge[k] = holder.LayerEmissionRefractHBedge[k];
			GlobalColorPerLayer[k] = holder.GlobalColorPerLayer[k];
			GlobalColorBottom[k] = holder.GlobalColorBottom[k];
			GlobalColorTop[k] = holder.GlobalColorTop[k];
			GlobalColorColormapLoSat[k] = holder.GlobalColorColormapLoSat[k];
			GlobalColorColormapHiSat[k] = holder.GlobalColorColormapHiSat[k];
			GlobalColorLayerLoSat[k] = holder.GlobalColorLayerLoSat[k];
			GlobalColorLayerHiSat[k] = holder.GlobalColorLayerHiSat[k];
			GlobalColorLoBlend[k] = holder.GlobalColorLoBlend[k];
			GlobalColorHiBlend[k] = holder.GlobalColorHiBlend[k];
			PER_LAYER_HEIGHT_MODIFIER[k] = holder.PER_LAYER_HEIGHT_MODIFIER[k];
			_SuperDetailStrengthMultA[k] = holder._SuperDetailStrengthMultA[k];
			_SuperDetailStrengthMultASelfMaskNear[k] = holder._SuperDetailStrengthMultASelfMaskNear[k];
			_SuperDetailStrengthMultASelfMaskFar[k] = holder._SuperDetailStrengthMultASelfMaskFar[k];
			_SuperDetailStrengthMultB[k] = holder._SuperDetailStrengthMultB[k];
			_SuperDetailStrengthMultBSelfMaskNear[k] = holder._SuperDetailStrengthMultBSelfMaskNear[k];
			_SuperDetailStrengthMultBSelfMaskFar[k] = holder._SuperDetailStrengthMultBSelfMaskFar[k];
			_SuperDetailStrengthNormal[k] = holder._SuperDetailStrengthNormal[k];
			_BumpMapGlobalStrength[k] = holder._BumpMapGlobalStrength[k];
			VerticalTextureStrength[k] = holder.VerticalTextureStrength[k];
			AO_strength[k] = holder.AO_strength[k];
			Heights[k] = holder.Heights[k];
			_snow_strength_per_layer[k] = holder._snow_strength_per_layer[k];
			TERRAIN_LayerWetStrength[k] = holder.TERRAIN_LayerWetStrength[k];
			TERRAIN_WaterLevel[k] = holder.TERRAIN_WaterLevel[k];
			TERRAIN_WaterLevelSlopeDamp[k] = holder.TERRAIN_WaterLevelSlopeDamp[k];
			TERRAIN_WaterEdge[k] = holder.TERRAIN_WaterEdge[k];
			TERRAIN_WaterGloss[k] = holder.TERRAIN_WaterGloss[k];
			TERRAIN_WaterGlossDamper[k] = holder.TERRAIN_WaterGlossDamper[k];
			TERRAIN_Refraction[k] = holder.TERRAIN_Refraction[k];
			TERRAIN_WetRefraction[k] = holder.TERRAIN_WetRefraction[k];
			TERRAIN_Flow[k] = holder.TERRAIN_Flow[k];
			TERRAIN_WetFlow[k] = holder.TERRAIN_WetFlow[k];
			TERRAIN_WaterMetallic[k] = holder.TERRAIN_WaterMetallic[k];
			TERRAIN_WetGloss[k] = holder.TERRAIN_WetGloss[k];
			TERRAIN_WaterColor[k] = holder.TERRAIN_WaterColor[k];
			TERRAIN_WaterEmission[k] = holder.TERRAIN_WaterEmission[k];
			_GlitterStrength[k] = holder._GlitterStrength[k];
		}
	}

	public void GlossBakeJob(Texture2D detailTex, Texture2D normalTex)
	{
		bakeJobArray = new Texture2D[2] { detailTex, normalTex };
	}

	public void SavePreset(ref ReliefTerrainPresetHolder holder)
	{
		holder.terrainLayers = terrainLayers;
		holder.numLayers = numLayers;
		holder.splats = new Texture2D[splats.Length];
		for (int i = 0; i < holder.splats.Length; i++)
		{
			holder.splats[i] = splats[i];
		}
		holder.splat_atlases = new Texture2D[3];
		for (int j = 0; j < splat_atlases.Length; j++)
		{
			holder.splat_atlases[j] = splat_atlases[j];
		}
		holder.RTP_MIP_BIAS = RTP_MIP_BIAS;
		holder.MasterLayerBrightness = MasterLayerBrightness;
		holder.MasterLayerSaturation = MasterLayerSaturation;
		holder.SuperDetailA_channel = SuperDetailA_channel;
		holder.SuperDetailB_channel = SuperDetailB_channel;
		holder.Bump01 = Bump01;
		holder.Bump23 = Bump23;
		holder.Bump45 = Bump45;
		holder.Bump67 = Bump67;
		holder.Bump89 = Bump89;
		holder.BumpAB = BumpAB;
		holder.SSColorCombinedA = SSColorCombinedA;
		holder.SSColorCombinedB = SSColorCombinedB;
		holder.BumpGlobal = BumpGlobal;
		holder.VerticalTexture = VerticalTexture;
		holder.BumpMapGlobalScale = BumpMapGlobalScale;
		holder.GlobalColorMapBlendValues = GlobalColorMapBlendValues;
		holder.GlobalColorMapSaturation = GlobalColorMapSaturation;
		holder.GlobalColorMapSaturationFar = GlobalColorMapSaturationFar;
		holder.GlobalColorMapDistortByPerlin = GlobalColorMapDistortByPerlin;
		holder.GlobalColorMapBrightness = GlobalColorMapBrightness;
		holder.GlobalColorMapBrightnessFar = GlobalColorMapBrightnessFar;
		holder._GlobalColorMapNearMIP = _GlobalColorMapNearMIP;
		holder._FarNormalDamp = _FarNormalDamp;
		holder.blendMultiplier = blendMultiplier;
		holder.HeightMap = HeightMap;
		holder.HeightMap2 = HeightMap2;
		holder.HeightMap3 = HeightMap3;
		holder.ReliefTransform = ReliefTransform;
		holder.DIST_STEPS = DIST_STEPS;
		holder.WAVELENGTH = WAVELENGTH;
		holder.ReliefBorderBlend = ReliefBorderBlend;
		holder.ExtrudeHeight = ExtrudeHeight;
		holder.LightmapShading = LightmapShading;
		holder.SHADOW_STEPS = SHADOW_STEPS;
		holder.WAVELENGTH_SHADOWS = WAVELENGTH_SHADOWS;
		holder.SelfShadowStrength = SelfShadowStrength;
		holder.ShadowSmoothing = ShadowSmoothing;
		holder.ShadowSoftnessFade = ShadowSoftnessFade;
		holder.distance_start = distance_start;
		holder.distance_transition = distance_transition;
		holder.distance_start_bumpglobal = distance_start_bumpglobal;
		holder.distance_transition_bumpglobal = distance_transition_bumpglobal;
		holder.rtp_perlin_start_val = rtp_perlin_start_val;
		holder._Phong = _Phong;
		holder.tessHeight = tessHeight;
		holder._TessSubdivisions = _TessSubdivisions;
		holder._TessSubdivisionsFar = _TessSubdivisionsFar;
		holder._TessYOffset = _TessYOffset;
		holder.trees_shadow_distance_start = trees_shadow_distance_start;
		holder.trees_shadow_distance_transition = trees_shadow_distance_transition;
		holder.trees_shadow_value = trees_shadow_value;
		holder.trees_pixel_distance_start = trees_pixel_distance_start;
		holder.trees_pixel_distance_transition = trees_pixel_distance_transition;
		holder.trees_pixel_blend_val = trees_pixel_blend_val;
		holder.global_normalMap_multiplier = global_normalMap_multiplier;
		holder.global_normalMap_farUsage = global_normalMap_farUsage;
		holder._AmbientEmissiveMultiplier = _AmbientEmissiveMultiplier;
		holder._AmbientEmissiveRelief = _AmbientEmissiveRelief;
		holder.rtp_mipoffset_globalnorm = rtp_mipoffset_globalnorm;
		holder._SuperDetailTiling = _SuperDetailTiling;
		holder.SuperDetailA = SuperDetailA;
		holder.SuperDetailB = SuperDetailB;
		holder.TERRAIN_GlobalWetness = TERRAIN_GlobalWetness;
		holder.TERRAIN_RippleMap = TERRAIN_RippleMap;
		holder.TERRAIN_RippleScale = TERRAIN_RippleScale;
		holder.TERRAIN_FlowScale = TERRAIN_FlowScale;
		holder.TERRAIN_FlowSpeed = TERRAIN_FlowSpeed;
		holder.TERRAIN_FlowCycleScale = TERRAIN_FlowCycleScale;
		holder.TERRAIN_FlowMipOffset = TERRAIN_FlowMipOffset;
		holder.TERRAIN_WetDarkening = TERRAIN_WetDarkening;
		holder.TERRAIN_WetDropletsStrength = TERRAIN_WetDropletsStrength;
		holder.TERRAIN_WetHeight_Treshold = TERRAIN_WetHeight_Treshold;
		holder.TERRAIN_WetHeight_Transition = TERRAIN_WetHeight_Transition;
		holder.TERRAIN_RainIntensity = TERRAIN_RainIntensity;
		holder.TERRAIN_DropletsSpeed = TERRAIN_DropletsSpeed;
		holder.TERRAIN_mipoffset_flowSpeed = TERRAIN_mipoffset_flowSpeed;
		holder.TERRAIN_CausticsAnimSpeed = TERRAIN_CausticsAnimSpeed;
		holder.TERRAIN_CausticsColor = TERRAIN_CausticsColor;
		holder.TERRAIN_CausticsWaterLevel = TERRAIN_CausticsWaterLevel;
		holder.TERRAIN_CausticsWaterLevelByAngle = TERRAIN_CausticsWaterLevelByAngle;
		holder.TERRAIN_CausticsWaterDeepFadeLength = TERRAIN_CausticsWaterDeepFadeLength;
		holder.TERRAIN_CausticsWaterShallowFadeLength = TERRAIN_CausticsWaterShallowFadeLength;
		holder.TERRAIN_CausticsTilingScale = TERRAIN_CausticsTilingScale;
		holder.TERRAIN_CausticsTex = TERRAIN_CausticsTex;
		holder.RTP_AOsharpness = RTP_AOsharpness;
		holder.RTP_AOamp = RTP_AOamp;
		holder._occlusionStrength = _occlusionStrength;
		holder.RTP_LightDefVector = RTP_LightDefVector;
		holder.EmissionRefractFiltering = EmissionRefractFiltering;
		holder.EmissionRefractAnimSpeed = EmissionRefractAnimSpeed;
		holder.VerticalTextureGlobalBumpInfluence = VerticalTextureGlobalBumpInfluence;
		holder.VerticalTextureTiling = VerticalTextureTiling;
		holder._snow_strength = _snow_strength;
		holder._global_color_brightness_to_snow = _global_color_brightness_to_snow;
		holder._snow_slope_factor = _snow_slope_factor;
		holder._snow_edge_definition = _snow_edge_definition;
		holder._snow_height_treshold = _snow_height_treshold;
		holder._snow_height_transition = _snow_height_transition;
		holder._snow_color = _snow_color;
		holder._snow_gloss = _snow_gloss;
		holder._snow_reflectivness = _snow_reflectivness;
		holder._snow_deep_factor = _snow_deep_factor;
		holder._snow_diff_fresnel = _snow_diff_fresnel;
		holder._snow_metallic = _snow_metallic;
		holder._snow_Frost = _snow_Frost;
		holder._snow_MicroTiling = _snow_MicroTiling;
		holder._snow_BumpMicro = _snow_BumpMicro;
		holder._snow_occlusionStrength = _snow_occlusionStrength;
		holder._snow_TranslucencyDeferredLightIndex = _snow_TranslucencyDeferredLightIndex;
		holder._SnowGlitterColor = _SnowGlitterColor;
		holder._GlitterColor = _GlitterColor;
		holder._GlitterTiling = _GlitterTiling;
		holder._GlitterDensity = _GlitterDensity;
		holder._GlitterFilter = _GlitterFilter;
		holder._GlitterColorization = _GlitterColorization;
		holder._SparkleMap = _SparkleMap;
		holder.Bumps = new Texture2D[numLayers];
		holder.Spec = new float[numLayers];
		holder.FarSpecCorrection = new float[numLayers];
		holder.MixScale = new float[numLayers];
		holder.MixBlend = new float[numLayers];
		holder.MixSaturation = new float[numLayers];
		holder.RTP_DiffFresnel = new float[numLayers];
		holder.RTP_metallic = new float[numLayers];
		holder.RTP_glossMin = new float[numLayers];
		holder.RTP_glossMax = new float[numLayers];
		holder.RTP_glitter = new float[numLayers];
		holder.MixBrightness = new float[numLayers];
		holder.MixReplace = new float[numLayers];
		holder.LayerBrightness = new float[numLayers];
		holder.LayerBrightness2Spec = new float[numLayers];
		holder.LayerAlbedo2SpecColor = new float[numLayers];
		holder.LayerSaturation = new float[numLayers];
		holder.LayerEmission = new float[numLayers];
		holder.LayerEmissionColor = new Color[numLayers];
		holder.LayerEmissionRefractStrength = new float[numLayers];
		holder.LayerEmissionRefractHBedge = new float[numLayers];
		holder.GlobalColorPerLayer = new float[numLayers];
		holder.GlobalColorBottom = new float[numLayers];
		holder.GlobalColorTop = new float[numLayers];
		holder.GlobalColorColormapLoSat = new float[numLayers];
		holder.GlobalColorColormapHiSat = new float[numLayers];
		holder.GlobalColorLayerLoSat = new float[numLayers];
		holder.GlobalColorLayerHiSat = new float[numLayers];
		holder.GlobalColorLoBlend = new float[numLayers];
		holder.GlobalColorHiBlend = new float[numLayers];
		holder.PER_LAYER_HEIGHT_MODIFIER = new float[numLayers];
		holder._SuperDetailStrengthMultA = new float[numLayers];
		holder._SuperDetailStrengthMultASelfMaskNear = new float[numLayers];
		holder._SuperDetailStrengthMultASelfMaskFar = new float[numLayers];
		holder._SuperDetailStrengthMultB = new float[numLayers];
		holder._SuperDetailStrengthMultBSelfMaskNear = new float[numLayers];
		holder._SuperDetailStrengthMultBSelfMaskFar = new float[numLayers];
		holder._SuperDetailStrengthNormal = new float[numLayers];
		holder._BumpMapGlobalStrength = new float[numLayers];
		holder.VerticalTextureStrength = new float[numLayers];
		holder.AO_strength = new float[numLayers];
		holder.Heights = new Texture2D[numLayers];
		holder._snow_strength_per_layer = new float[numLayers];
		holder.TERRAIN_LayerWetStrength = new float[numLayers];
		holder.TERRAIN_WaterLevel = new float[numLayers];
		holder.TERRAIN_WaterLevelSlopeDamp = new float[numLayers];
		holder.TERRAIN_WaterEdge = new float[numLayers];
		holder.TERRAIN_WaterGloss = new float[numLayers];
		holder.TERRAIN_WaterGlossDamper = new float[numLayers];
		holder.TERRAIN_Refraction = new float[numLayers];
		holder.TERRAIN_WetRefraction = new float[numLayers];
		holder.TERRAIN_Flow = new float[numLayers];
		holder.TERRAIN_WetFlow = new float[numLayers];
		holder.TERRAIN_WaterMetallic = new float[numLayers];
		holder.TERRAIN_WetGloss = new float[numLayers];
		holder.TERRAIN_WaterColor = new Color[numLayers];
		holder.TERRAIN_WaterEmission = new float[numLayers];
		holder._GlitterStrength = new float[numLayers];
		for (int k = 0; k < numLayers; k++)
		{
			holder.Bumps[k] = Bumps[k];
			holder.FarSpecCorrection[k] = FarSpecCorrection[k];
			holder.MixScale[k] = MixScale[k];
			holder.MixBlend[k] = MixBlend[k];
			holder.MixSaturation[k] = MixSaturation[k];
			CheckAndUpdate(ref RTP_DiffFresnel, 0f, numLayers);
			CheckAndUpdate(ref RTP_metallic, 0f, numLayers);
			CheckAndUpdate(ref RTP_glossMin, 0f, numLayers);
			CheckAndUpdate(ref RTP_glossMax, 1f, numLayers);
			CheckAndUpdate(ref RTP_glitter, 0f, numLayers);
			CheckAndUpdate(ref TERRAIN_WaterGloss, 0.1f, numLayers);
			CheckAndUpdate(ref TERRAIN_WaterGlossDamper, 0f, numLayers);
			CheckAndUpdate(ref TERRAIN_WaterMetallic, 0.1f, numLayers);
			CheckAndUpdate(ref TERRAIN_WetGloss, 0.05f, numLayers);
			CheckAndUpdate(ref TERRAIN_WetFlow, 0.05f, numLayers);
			CheckAndUpdate(ref MixBrightness, 2f, numLayers);
			CheckAndUpdate(ref MixReplace, 0f, numLayers);
			CheckAndUpdate(ref LayerBrightness, 1f, numLayers);
			CheckAndUpdate(ref LayerBrightness2Spec, 0f, numLayers);
			CheckAndUpdate(ref LayerAlbedo2SpecColor, 0f, numLayers);
			CheckAndUpdate(ref LayerSaturation, 1f, numLayers);
			CheckAndUpdate(ref LayerEmission, 0f, numLayers);
			CheckAndUpdate(ref LayerEmissionColor, Color.black, numLayers);
			CheckAndUpdate(ref LayerEmissionRefractStrength, 0f, numLayers);
			CheckAndUpdate(ref LayerEmissionRefractHBedge, 0f, numLayers);
			CheckAndUpdate(ref TERRAIN_WaterEmission, 0.5f, numLayers);
			CheckAndUpdate(ref _GlitterStrength, 0f, numLayers);
			holder.RTP_DiffFresnel[k] = RTP_DiffFresnel[k];
			holder.RTP_metallic[k] = RTP_metallic[k];
			holder.RTP_glossMin[k] = RTP_glossMin[k];
			holder.RTP_glossMax[k] = RTP_glossMax[k];
			holder.RTP_glitter[k] = RTP_glitter[k];
			holder.TERRAIN_WaterEmission[k] = TERRAIN_WaterEmission[k];
			holder.MixBrightness[k] = MixBrightness[k];
			holder.MixReplace[k] = MixReplace[k];
			holder.LayerBrightness[k] = LayerBrightness[k];
			holder.LayerBrightness2Spec[k] = LayerBrightness2Spec[k];
			holder.LayerAlbedo2SpecColor[k] = LayerAlbedo2SpecColor[k];
			holder.LayerSaturation[k] = LayerSaturation[k];
			holder.LayerEmission[k] = LayerEmission[k];
			holder.LayerEmissionColor[k] = LayerEmissionColor[k];
			holder.LayerEmissionRefractStrength[k] = LayerEmissionRefractStrength[k];
			holder.LayerEmissionRefractHBedge[k] = LayerEmissionRefractHBedge[k];
			holder.GlobalColorPerLayer[k] = GlobalColorPerLayer[k];
			holder.GlobalColorBottom[k] = GlobalColorBottom[k];
			holder.GlobalColorTop[k] = GlobalColorTop[k];
			holder.GlobalColorColormapLoSat[k] = GlobalColorColormapLoSat[k];
			holder.GlobalColorColormapHiSat[k] = GlobalColorColormapHiSat[k];
			holder.GlobalColorLayerLoSat[k] = GlobalColorLayerLoSat[k];
			holder.GlobalColorLayerHiSat[k] = GlobalColorLayerHiSat[k];
			holder.GlobalColorLoBlend[k] = GlobalColorLoBlend[k];
			holder.GlobalColorHiBlend[k] = GlobalColorHiBlend[k];
			holder.PER_LAYER_HEIGHT_MODIFIER[k] = PER_LAYER_HEIGHT_MODIFIER[k];
			holder._SuperDetailStrengthMultA[k] = _SuperDetailStrengthMultA[k];
			holder._SuperDetailStrengthMultASelfMaskNear[k] = _SuperDetailStrengthMultASelfMaskNear[k];
			holder._SuperDetailStrengthMultASelfMaskFar[k] = _SuperDetailStrengthMultASelfMaskFar[k];
			holder._SuperDetailStrengthMultB[k] = _SuperDetailStrengthMultB[k];
			holder._SuperDetailStrengthMultBSelfMaskNear[k] = _SuperDetailStrengthMultBSelfMaskNear[k];
			holder._SuperDetailStrengthMultBSelfMaskFar[k] = _SuperDetailStrengthMultBSelfMaskFar[k];
			holder._SuperDetailStrengthNormal[k] = _SuperDetailStrengthNormal[k];
			holder._BumpMapGlobalStrength[k] = _BumpMapGlobalStrength[k];
			holder.VerticalTextureStrength[k] = VerticalTextureStrength[k];
			holder.AO_strength[k] = AO_strength[k];
			holder.Heights[k] = Heights[k];
			holder._snow_strength_per_layer[k] = _snow_strength_per_layer[k];
			holder.TERRAIN_LayerWetStrength[k] = TERRAIN_LayerWetStrength[k];
			holder.TERRAIN_WaterLevel[k] = TERRAIN_WaterLevel[k];
			holder.TERRAIN_WaterLevelSlopeDamp[k] = TERRAIN_WaterLevelSlopeDamp[k];
			holder.TERRAIN_WaterEdge[k] = TERRAIN_WaterEdge[k];
			holder.TERRAIN_WaterGloss[k] = TERRAIN_WaterGloss[k];
			holder.TERRAIN_WaterGlossDamper[k] = TERRAIN_WaterGlossDamper[k];
			holder.TERRAIN_Refraction[k] = TERRAIN_Refraction[k];
			holder.TERRAIN_WetRefraction[k] = TERRAIN_WetRefraction[k];
			holder.TERRAIN_Flow[k] = TERRAIN_Flow[k];
			holder.TERRAIN_WetFlow[k] = TERRAIN_WetFlow[k];
			holder.TERRAIN_WaterMetallic[k] = TERRAIN_WaterMetallic[k];
			holder.TERRAIN_WetGloss[k] = TERRAIN_WetGloss[k];
			holder.TERRAIN_WaterColor[k] = TERRAIN_WaterColor[k];
			holder._GlitterStrength[k] = _GlitterStrength[k];
		}
	}

	public void InterpolatePresets(ReliefTerrainPresetHolder holderA, ReliefTerrainPresetHolder holderB, float t)
	{
		RTP_MIP_BIAS = Mathf.Lerp(holderA.RTP_MIP_BIAS, holderB.RTP_MIP_BIAS, t);
		MasterLayerBrightness = Mathf.Lerp(holderA.MasterLayerBrightness, holderB.MasterLayerBrightness, t);
		MasterLayerSaturation = Mathf.Lerp(holderA.MasterLayerSaturation, holderB.MasterLayerSaturation, t);
		BumpMapGlobalScale = Mathf.Lerp(holderA.BumpMapGlobalScale, holderB.BumpMapGlobalScale, t);
		GlobalColorMapBlendValues = Vector3.Lerp(holderA.GlobalColorMapBlendValues, holderB.GlobalColorMapBlendValues, t);
		GlobalColorMapSaturation = Mathf.Lerp(holderA.GlobalColorMapSaturation, holderB.GlobalColorMapSaturation, t);
		GlobalColorMapSaturationFar = Mathf.Lerp(holderA.GlobalColorMapSaturationFar, holderB.GlobalColorMapSaturationFar, t);
		GlobalColorMapDistortByPerlin = Mathf.Lerp(holderA.GlobalColorMapDistortByPerlin, holderB.GlobalColorMapDistortByPerlin, t);
		GlobalColorMapBrightness = Mathf.Lerp(holderA.GlobalColorMapBrightness, holderB.GlobalColorMapBrightness, t);
		GlobalColorMapBrightnessFar = Mathf.Lerp(holderA.GlobalColorMapBrightnessFar, holderB.GlobalColorMapBrightnessFar, t);
		_GlobalColorMapNearMIP = Mathf.Lerp(holderA._GlobalColorMapNearMIP, holderB._GlobalColorMapNearMIP, t);
		_FarNormalDamp = Mathf.Lerp(holderA._FarNormalDamp, holderB._FarNormalDamp, t);
		blendMultiplier = Mathf.Lerp(holderA.blendMultiplier, holderB.blendMultiplier, t);
		ReliefTransform = Vector4.Lerp(holderA.ReliefTransform, holderB.ReliefTransform, t);
		DIST_STEPS = Mathf.Lerp(holderA.DIST_STEPS, holderB.DIST_STEPS, t);
		WAVELENGTH = Mathf.Lerp(holderA.WAVELENGTH, holderB.WAVELENGTH, t);
		ReliefBorderBlend = Mathf.Lerp(holderA.ReliefBorderBlend, holderB.ReliefBorderBlend, t);
		ExtrudeHeight = Mathf.Lerp(holderA.ExtrudeHeight, holderB.ExtrudeHeight, t);
		LightmapShading = Mathf.Lerp(holderA.LightmapShading, holderB.LightmapShading, t);
		SHADOW_STEPS = Mathf.Lerp(holderA.SHADOW_STEPS, holderB.SHADOW_STEPS, t);
		WAVELENGTH_SHADOWS = Mathf.Lerp(holderA.WAVELENGTH_SHADOWS, holderB.WAVELENGTH_SHADOWS, t);
		SelfShadowStrength = Mathf.Lerp(holderA.SelfShadowStrength, holderB.SelfShadowStrength, t);
		ShadowSmoothing = Mathf.Lerp(holderA.ShadowSmoothing, holderB.ShadowSmoothing, t);
		ShadowSoftnessFade = Mathf.Lerp(holderA.ShadowSoftnessFade, holderB.ShadowSoftnessFade, t);
		distance_start = Mathf.Lerp(holderA.distance_start, holderB.distance_start, t);
		distance_transition = Mathf.Lerp(holderA.distance_transition, holderB.distance_transition, t);
		distance_start_bumpglobal = Mathf.Lerp(holderA.distance_start_bumpglobal, holderB.distance_start_bumpglobal, t);
		distance_transition_bumpglobal = Mathf.Lerp(holderA.distance_transition_bumpglobal, holderB.distance_transition_bumpglobal, t);
		rtp_perlin_start_val = Mathf.Lerp(holderA.rtp_perlin_start_val, holderB.rtp_perlin_start_val, t);
		trees_shadow_distance_start = Mathf.Lerp(holderA.trees_shadow_distance_start, holderB.trees_shadow_distance_start, t);
		trees_shadow_distance_transition = Mathf.Lerp(holderA.trees_shadow_distance_transition, holderB.trees_shadow_distance_transition, t);
		trees_shadow_value = Mathf.Lerp(holderA.trees_shadow_value, holderB.trees_shadow_value, t);
		trees_pixel_distance_start = Mathf.Lerp(holderA.trees_pixel_distance_start, holderB.trees_pixel_distance_start, t);
		trees_pixel_distance_transition = Mathf.Lerp(holderA.trees_pixel_distance_transition, holderB.trees_pixel_distance_transition, t);
		trees_pixel_blend_val = Mathf.Lerp(holderA.trees_pixel_blend_val, holderB.trees_pixel_blend_val, t);
		global_normalMap_multiplier = Mathf.Lerp(holderA.global_normalMap_multiplier, holderB.global_normalMap_multiplier, t);
		global_normalMap_farUsage = Mathf.Lerp(holderA.global_normalMap_farUsage, holderB.global_normalMap_farUsage, t);
		_AmbientEmissiveMultiplier = Mathf.Lerp(holderA._AmbientEmissiveMultiplier, holderB._AmbientEmissiveMultiplier, t);
		_AmbientEmissiveRelief = Mathf.Lerp(holderA._AmbientEmissiveRelief, holderB._AmbientEmissiveRelief, t);
		_SuperDetailTiling = Mathf.Lerp(holderA._SuperDetailTiling, holderB._SuperDetailTiling, t);
		TERRAIN_GlobalWetness = Mathf.Lerp(holderA.TERRAIN_GlobalWetness, holderB.TERRAIN_GlobalWetness, t);
		TERRAIN_RippleScale = Mathf.Lerp(holderA.TERRAIN_RippleScale, holderB.TERRAIN_RippleScale, t);
		TERRAIN_FlowScale = Mathf.Lerp(holderA.TERRAIN_FlowScale, holderB.TERRAIN_FlowScale, t);
		TERRAIN_FlowSpeed = Mathf.Lerp(holderA.TERRAIN_FlowSpeed, holderB.TERRAIN_FlowSpeed, t);
		TERRAIN_FlowCycleScale = Mathf.Lerp(holderA.TERRAIN_FlowCycleScale, holderB.TERRAIN_FlowCycleScale, t);
		TERRAIN_FlowMipOffset = Mathf.Lerp(holderA.TERRAIN_FlowMipOffset, holderB.TERRAIN_FlowMipOffset, t);
		TERRAIN_WetDarkening = Mathf.Lerp(holderA.TERRAIN_WetDarkening, holderB.TERRAIN_WetDarkening, t);
		TERRAIN_WetDropletsStrength = Mathf.Lerp(holderA.TERRAIN_WetDropletsStrength, holderB.TERRAIN_WetDropletsStrength, t);
		TERRAIN_WetHeight_Treshold = Mathf.Lerp(holderA.TERRAIN_WetHeight_Treshold, holderB.TERRAIN_WetHeight_Treshold, t);
		TERRAIN_WetHeight_Transition = Mathf.Lerp(holderA.TERRAIN_WetHeight_Transition, holderB.TERRAIN_WetHeight_Transition, t);
		TERRAIN_RainIntensity = Mathf.Lerp(holderA.TERRAIN_RainIntensity, holderB.TERRAIN_RainIntensity, t);
		TERRAIN_DropletsSpeed = Mathf.Lerp(holderA.TERRAIN_DropletsSpeed, holderB.TERRAIN_DropletsSpeed, t);
		TERRAIN_mipoffset_flowSpeed = Mathf.Lerp(holderA.TERRAIN_mipoffset_flowSpeed, holderB.TERRAIN_mipoffset_flowSpeed, t);
		TERRAIN_CausticsAnimSpeed = Mathf.Lerp(holderA.TERRAIN_CausticsAnimSpeed, holderB.TERRAIN_CausticsAnimSpeed, t);
		TERRAIN_CausticsColor = Color.Lerp(holderA.TERRAIN_CausticsColor, holderB.TERRAIN_CausticsColor, t);
		TERRAIN_CausticsWaterLevel = Mathf.Lerp(holderA.TERRAIN_CausticsWaterLevel, holderB.TERRAIN_CausticsWaterLevel, t);
		TERRAIN_CausticsWaterLevelByAngle = Mathf.Lerp(holderA.TERRAIN_CausticsWaterLevelByAngle, holderB.TERRAIN_CausticsWaterLevelByAngle, t);
		TERRAIN_CausticsWaterDeepFadeLength = Mathf.Lerp(holderA.TERRAIN_CausticsWaterDeepFadeLength, holderB.TERRAIN_CausticsWaterDeepFadeLength, t);
		TERRAIN_CausticsWaterShallowFadeLength = Mathf.Lerp(holderA.TERRAIN_CausticsWaterShallowFadeLength, holderB.TERRAIN_CausticsWaterShallowFadeLength, t);
		TERRAIN_CausticsTilingScale = Mathf.Lerp(holderA.TERRAIN_CausticsTilingScale, holderB.TERRAIN_CausticsTilingScale, t);
		RTP_AOsharpness = Mathf.Lerp(holderA.RTP_AOsharpness, holderB.RTP_AOsharpness, t);
		RTP_AOamp = Mathf.Lerp(holderA.RTP_AOamp, holderB.RTP_AOamp, t);
		_occlusionStrength = Mathf.Lerp(holderA._occlusionStrength, holderB._occlusionStrength, t);
		RTP_LightDefVector = Vector4.Lerp(holderA.RTP_LightDefVector, holderB.RTP_LightDefVector, t);
		EmissionRefractFiltering = Mathf.Lerp(holderA.EmissionRefractFiltering, holderB.EmissionRefractFiltering, t);
		EmissionRefractAnimSpeed = Mathf.Lerp(holderA.EmissionRefractAnimSpeed, holderB.EmissionRefractAnimSpeed, t);
		VerticalTextureGlobalBumpInfluence = Mathf.Lerp(holderA.VerticalTextureGlobalBumpInfluence, holderB.VerticalTextureGlobalBumpInfluence, t);
		VerticalTextureTiling = Mathf.Lerp(holderA.VerticalTextureTiling, holderB.VerticalTextureTiling, t);
		_snow_strength = Mathf.Lerp(holderA._snow_strength, holderB._snow_strength, t);
		_global_color_brightness_to_snow = Mathf.Lerp(holderA._global_color_brightness_to_snow, holderB._global_color_brightness_to_snow, t);
		_snow_slope_factor = Mathf.Lerp(holderA._snow_slope_factor, holderB._snow_slope_factor, t);
		_snow_edge_definition = Mathf.Lerp(holderA._snow_edge_definition, holderB._snow_edge_definition, t);
		_snow_height_treshold = Mathf.Lerp(holderA._snow_height_treshold, holderB._snow_height_treshold, t);
		_snow_height_transition = Mathf.Lerp(holderA._snow_height_transition, holderB._snow_height_transition, t);
		_snow_color = Color.Lerp(holderA._snow_color, holderB._snow_color, t);
		_snow_gloss = Mathf.Lerp(holderA._snow_gloss, holderB._snow_gloss, t);
		_snow_reflectivness = Mathf.Lerp(holderA._snow_reflectivness, holderB._snow_reflectivness, t);
		_snow_deep_factor = Mathf.Lerp(holderA._snow_deep_factor, holderB._snow_deep_factor, t);
		_snow_diff_fresnel = Mathf.Lerp(holderA._snow_diff_fresnel, holderB._snow_diff_fresnel, t);
		_snow_metallic = Mathf.Lerp(holderA._snow_metallic, holderB._snow_metallic, t);
		_snow_Frost = Mathf.Lerp(holderA._snow_Frost, holderB._snow_Frost, t);
		_snow_MicroTiling = Mathf.Lerp(holderA._snow_MicroTiling, holderB._snow_MicroTiling, t);
		_snow_BumpMicro = Mathf.Lerp(holderA._snow_BumpMicro, holderB._snow_BumpMicro, t);
		_snow_occlusionStrength = Mathf.Lerp(holderA._snow_occlusionStrength, holderB._snow_occlusionStrength, t);
		_SnowGlitterColor = Color.Lerp(holderA._SnowGlitterColor, holderB._SnowGlitterColor, t);
		_GlitterColor = Color.Lerp(holderA._GlitterColor, holderB._GlitterColor, t);
		_GlitterTiling = Mathf.Lerp(holderA._GlitterTiling, holderB._GlitterTiling, t);
		_GlitterDensity = Mathf.Lerp(holderA._GlitterDensity, holderB._GlitterDensity, t);
		_GlitterFilter = Mathf.Lerp(holderA._GlitterFilter, holderB._GlitterFilter, t);
		_GlitterColorization = Mathf.Lerp(holderA._GlitterColorization, holderB._GlitterColorization, t);
		for (int i = 0; i < holderA.Spec.Length; i++)
		{
			if (i < FarSpecCorrection.Length)
			{
				FarSpecCorrection[i] = Mathf.Lerp(holderA.FarSpecCorrection[i], holderB.FarSpecCorrection[i], t);
				MixScale[i] = Mathf.Lerp(holderA.MixScale[i], holderB.MixScale[i], t);
				MixBlend[i] = Mathf.Lerp(holderA.MixBlend[i], holderB.MixBlend[i], t);
				MixSaturation[i] = Mathf.Lerp(holderA.MixSaturation[i], holderB.MixSaturation[i], t);
				RTP_DiffFresnel[i] = Mathf.Lerp(holderA.RTP_DiffFresnel[i], holderB.RTP_DiffFresnel[i], t);
				RTP_metallic[i] = Mathf.Lerp(holderA.RTP_metallic[i], holderB.RTP_metallic[i], t);
				RTP_glossMin[i] = Mathf.Lerp(holderA.RTP_glossMin[i], holderB.RTP_glossMin[i], t);
				RTP_glossMax[i] = Mathf.Lerp(holderA.RTP_glossMax[i], holderB.RTP_glossMax[i], t);
				RTP_glitter[i] = Mathf.Lerp(holderA.RTP_glitter[i], holderB.RTP_glitter[i], t);
				MixBrightness[i] = Mathf.Lerp(holderA.MixBrightness[i], holderB.MixBrightness[i], t);
				MixReplace[i] = Mathf.Lerp(holderA.MixReplace[i], holderB.MixReplace[i], t);
				LayerBrightness[i] = Mathf.Lerp(holderA.LayerBrightness[i], holderB.LayerBrightness[i], t);
				LayerBrightness2Spec[i] = Mathf.Lerp(holderA.LayerBrightness2Spec[i], holderB.LayerBrightness2Spec[i], t);
				LayerAlbedo2SpecColor[i] = Mathf.Lerp(holderA.LayerAlbedo2SpecColor[i], holderB.LayerAlbedo2SpecColor[i], t);
				LayerSaturation[i] = Mathf.Lerp(holderA.LayerSaturation[i], holderB.LayerSaturation[i], t);
				LayerEmission[i] = Mathf.Lerp(holderA.LayerEmission[i], holderB.LayerEmission[i], t);
				LayerEmissionColor[i] = Color.Lerp(holderA.LayerEmissionColor[i], holderB.LayerEmissionColor[i], t);
				LayerEmissionRefractStrength[i] = Mathf.Lerp(holderA.LayerEmissionRefractStrength[i], holderB.LayerEmissionRefractStrength[i], t);
				LayerEmissionRefractHBedge[i] = Mathf.Lerp(holderA.LayerEmissionRefractHBedge[i], holderB.LayerEmissionRefractHBedge[i], t);
				GlobalColorPerLayer[i] = Mathf.Lerp(holderA.GlobalColorPerLayer[i], holderB.GlobalColorPerLayer[i], t);
				GlobalColorBottom[i] = Mathf.Lerp(holderA.GlobalColorBottom[i], holderB.GlobalColorBottom[i], t);
				GlobalColorTop[i] = Mathf.Lerp(holderA.GlobalColorTop[i], holderB.GlobalColorTop[i], t);
				GlobalColorColormapLoSat[i] = Mathf.Lerp(holderA.GlobalColorColormapLoSat[i], holderB.GlobalColorColormapLoSat[i], t);
				GlobalColorColormapHiSat[i] = Mathf.Lerp(holderA.GlobalColorColormapHiSat[i], holderB.GlobalColorColormapHiSat[i], t);
				GlobalColorLayerLoSat[i] = Mathf.Lerp(holderA.GlobalColorLayerLoSat[i], holderB.GlobalColorLayerLoSat[i], t);
				GlobalColorLayerHiSat[i] = Mathf.Lerp(holderA.GlobalColorLayerHiSat[i], holderB.GlobalColorLayerHiSat[i], t);
				GlobalColorLoBlend[i] = Mathf.Lerp(holderA.GlobalColorLoBlend[i], holderB.GlobalColorLoBlend[i], t);
				GlobalColorHiBlend[i] = Mathf.Lerp(holderA.GlobalColorHiBlend[i], holderB.GlobalColorHiBlend[i], t);
				PER_LAYER_HEIGHT_MODIFIER[i] = Mathf.Lerp(holderA.PER_LAYER_HEIGHT_MODIFIER[i], holderB.PER_LAYER_HEIGHT_MODIFIER[i], t);
				_SuperDetailStrengthMultA[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultA[i], holderB._SuperDetailStrengthMultA[i], t);
				_SuperDetailStrengthMultASelfMaskNear[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultASelfMaskNear[i], holderB._SuperDetailStrengthMultASelfMaskNear[i], t);
				_SuperDetailStrengthMultASelfMaskFar[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultASelfMaskFar[i], holderB._SuperDetailStrengthMultASelfMaskFar[i], t);
				_SuperDetailStrengthMultB[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultB[i], holderB._SuperDetailStrengthMultB[i], t);
				_SuperDetailStrengthMultBSelfMaskNear[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultBSelfMaskNear[i], holderB._SuperDetailStrengthMultBSelfMaskNear[i], t);
				_SuperDetailStrengthMultBSelfMaskFar[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultBSelfMaskFar[i], holderB._SuperDetailStrengthMultBSelfMaskFar[i], t);
				_SuperDetailStrengthNormal[i] = Mathf.Lerp(holderA._SuperDetailStrengthNormal[i], holderB._SuperDetailStrengthNormal[i], t);
				_BumpMapGlobalStrength[i] = Mathf.Lerp(holderA._BumpMapGlobalStrength[i], holderB._BumpMapGlobalStrength[i], t);
				AO_strength[i] = Mathf.Lerp(holderA.AO_strength[i], holderB.AO_strength[i], t);
				VerticalTextureStrength[i] = Mathf.Lerp(holderA.VerticalTextureStrength[i], holderB.VerticalTextureStrength[i], t);
				_snow_strength_per_layer[i] = Mathf.Lerp(holderA._snow_strength_per_layer[i], holderB._snow_strength_per_layer[i], t);
				TERRAIN_LayerWetStrength[i] = Mathf.Lerp(holderA.TERRAIN_LayerWetStrength[i], holderB.TERRAIN_LayerWetStrength[i], t);
				TERRAIN_WaterLevel[i] = Mathf.Lerp(holderA.TERRAIN_WaterLevel[i], holderB.TERRAIN_WaterLevel[i], t);
				TERRAIN_WaterLevelSlopeDamp[i] = Mathf.Lerp(holderA.TERRAIN_WaterLevelSlopeDamp[i], holderB.TERRAIN_WaterLevelSlopeDamp[i], t);
				TERRAIN_WaterEdge[i] = Mathf.Lerp(holderA.TERRAIN_WaterEdge[i], holderB.TERRAIN_WaterEdge[i], t);
				TERRAIN_WaterGloss[i] = Mathf.Lerp(holderA.TERRAIN_WaterGloss[i], holderB.TERRAIN_WaterGloss[i], t);
				TERRAIN_WaterGlossDamper[i] = Mathf.Lerp(holderA.TERRAIN_WaterGlossDamper[i], holderB.TERRAIN_WaterGlossDamper[i], t);
				TERRAIN_Refraction[i] = Mathf.Lerp(holderA.TERRAIN_Refraction[i], holderB.TERRAIN_Refraction[i], t);
				TERRAIN_WetRefraction[i] = Mathf.Lerp(holderA.TERRAIN_WetRefraction[i], holderB.TERRAIN_WetRefraction[i], t);
				TERRAIN_Flow[i] = Mathf.Lerp(holderA.TERRAIN_Flow[i], holderB.TERRAIN_Flow[i], t);
				TERRAIN_WetFlow[i] = Mathf.Lerp(holderA.TERRAIN_WetFlow[i], holderB.TERRAIN_WetFlow[i], t);
				TERRAIN_WaterMetallic[i] = Mathf.Lerp(holderA.TERRAIN_WaterMetallic[i], holderB.TERRAIN_WaterMetallic[i], t);
				TERRAIN_WetGloss[i] = Mathf.Lerp(holderA.TERRAIN_WetGloss[i], holderB.TERRAIN_WetGloss[i], t);
				TERRAIN_WaterColor[i] = Color.Lerp(holderA.TERRAIN_WaterColor[i], holderB.TERRAIN_WaterColor[i], t);
				TERRAIN_WaterEmission[i] = Mathf.Lerp(holderA.TERRAIN_WaterEmission[i], holderB.TERRAIN_WaterEmission[i], t);
				_GlitterStrength[i] = Mathf.Lerp(holderA._GlitterStrength[i], holderB._GlitterStrength[i], t);
			}
		}
	}

	public void ReturnToDefaults(string what = "", int layerIdx = -1)
	{
		if (what == "" || what == "main")
		{
			ReliefTransform = new Vector4(3f, 3f, 0f, 0f);
			distance_start = 5f;
			distance_transition = 20f;
			RTP_LightDefVector = new Vector4(0.05f, 0.5f, 0.5f, 25f);
			ReliefBorderBlend = 6f;
			LightmapShading = 0f;
			RTP_MIP_BIAS = 0f;
			RTP_AOsharpness = 1.5f;
			RTP_AOamp = 0.1f;
			_occlusionStrength = 1f;
			MasterLayerBrightness = 1f;
			MasterLayerSaturation = 1f;
			EmissionRefractFiltering = 4f;
			EmissionRefractAnimSpeed = 4f;
		}
		if (what == "" || what == "perlin")
		{
			BumpMapGlobalScale = 0.1f;
			_FarNormalDamp = 0.2f;
			distance_start_bumpglobal = 30f;
			distance_transition_bumpglobal = 30f;
			rtp_perlin_start_val = 0f;
		}
		if (what == "" || what == "global_color")
		{
			GlobalColorMapBlendValues = new Vector3(0.2f, 0.4f, 0.5f);
			GlobalColorMapSaturation = 1f;
			GlobalColorMapSaturationFar = 1f;
			GlobalColorMapDistortByPerlin = 0.005f;
			GlobalColorMapBrightness = 1f;
			GlobalColorMapBrightnessFar = 1f;
			_GlobalColorMapNearMIP = 0f;
			trees_shadow_distance_start = 50f;
			trees_shadow_distance_transition = 10f;
			trees_shadow_value = 0.5f;
			trees_pixel_distance_start = 500f;
			trees_pixel_distance_transition = 10f;
			trees_pixel_blend_val = 2f;
			global_normalMap_multiplier = 1f;
			global_normalMap_farUsage = 0f;
			_Phong = 0f;
			tessHeight = 300f;
			_TessSubdivisions = 1f;
			_TessSubdivisionsFar = 1f;
			_TessYOffset = 0f;
			_AmbientEmissiveMultiplier = 1f;
			_AmbientEmissiveRelief = 0.5f;
		}
		if (what == "" || what == "uvblend")
		{
			blendMultiplier = 1f;
		}
		if (what == "" || what == "pom/pm")
		{
			ExtrudeHeight = 0.05f;
			DIST_STEPS = 20f;
			WAVELENGTH = 2f;
			SHADOW_STEPS = 20f;
			WAVELENGTH_SHADOWS = 2f;
			SelfShadowStrength = 0.8f;
			ShadowSmoothing = 1f;
			ShadowSoftnessFade = 0.8f;
		}
		if (what == "" || what == "snow")
		{
			_global_color_brightness_to_snow = 0.5f;
			_snow_strength = 0f;
			_snow_slope_factor = 2f;
			_snow_edge_definition = 2f;
			_snow_height_treshold = -200f;
			_snow_height_transition = 1f;
			_snow_color = Color.white;
			_snow_gloss = 0.7f;
			_snow_reflectivness = 0.7f;
			_snow_deep_factor = 1.5f;
			_snow_diff_fresnel = 0.5f;
			_SnowGlitterColor = new Color(1f, 1f, 1f, 0.1f);
		}
		if (what == "" || what == "superdetail")
		{
			_SuperDetailTiling = 8f;
		}
		if (what == "" || what == "vertical")
		{
			VerticalTextureGlobalBumpInfluence = 0f;
			VerticalTextureTiling = 50f;
		}
		if (what == "" || what == "glitter")
		{
			_GlitterColor = new Color(1f, 1f, 1f, 0.1f);
			_GlitterTiling = 1f;
			_GlitterDensity = 0.1f;
			_GlitterFilter = 0f;
			_GlitterColorization = 0.5f;
		}
		if (what == "" || what == "water")
		{
			TERRAIN_GlobalWetness = 1f;
			TERRAIN_RippleScale = 4f;
			TERRAIN_FlowScale = 1f;
			TERRAIN_FlowSpeed = 0.5f;
			TERRAIN_FlowCycleScale = 1f;
			TERRAIN_RainIntensity = 1f;
			TERRAIN_DropletsSpeed = 10f;
			TERRAIN_mipoffset_flowSpeed = 1f;
			TERRAIN_FlowMipOffset = 0f;
			TERRAIN_WetDarkening = 0.5f;
			TERRAIN_WetDropletsStrength = 0f;
			TERRAIN_WetHeight_Treshold = -200f;
			TERRAIN_WetHeight_Transition = 5f;
		}
		if (what == "" || what == "caustics")
		{
			TERRAIN_CausticsAnimSpeed = 2f;
			TERRAIN_CausticsColor = Color.white;
			TERRAIN_CausticsWaterLevel = 30f;
			TERRAIN_CausticsWaterLevelByAngle = 2f;
			TERRAIN_CausticsWaterDeepFadeLength = 50f;
			TERRAIN_CausticsWaterShallowFadeLength = 30f;
			TERRAIN_CausticsTilingScale = 1f;
		}
		if (what == "" || what == "layer")
		{
			int num = 0;
			int num2 = ((numLayers < 12) ? numLayers : 12);
			if (layerIdx >= 0)
			{
				num = layerIdx;
				num2 = layerIdx + 1;
			}
			for (int i = num; i < num2; i++)
			{
				FarSpecCorrection[i] = 0f;
				MIPmult[i] = 0f;
				MixScale[i] = 0.2f;
				MixBlend[i] = 0.5f;
				MixSaturation[i] = 0.3f;
				RTP_DiffFresnel[i] = 0f;
				RTP_metallic[i] = 0f;
				RTP_glossMin[i] = 0f;
				RTP_glossMax[i] = 1f;
				RTP_glitter[i] = 0f;
				MixBrightness[i] = 2f;
				MixReplace[i] = 0f;
				LayerBrightness[i] = 1f;
				LayerBrightness2Spec[i] = 0f;
				LayerAlbedo2SpecColor[i] = 0f;
				LayerSaturation[i] = 1f;
				LayerEmission[i] = 0f;
				LayerEmissionColor[i] = Color.black;
				LayerEmissionRefractStrength[i] = 0f;
				LayerEmissionRefractHBedge[i] = 0f;
				GlobalColorPerLayer[i] = 1f;
				GlobalColorBottom[i] = 0f;
				GlobalColorTop[i] = 1f;
				GlobalColorColormapLoSat[i] = 1f;
				GlobalColorColormapHiSat[i] = 1f;
				GlobalColorLayerLoSat[i] = 1f;
				GlobalColorLayerHiSat[i] = 1f;
				GlobalColorLoBlend[i] = 1f;
				GlobalColorHiBlend[i] = 1f;
				PER_LAYER_HEIGHT_MODIFIER[i] = 0f;
				_SuperDetailStrengthMultA[i] = 0f;
				_SuperDetailStrengthMultASelfMaskNear[i] = 0f;
				_SuperDetailStrengthMultASelfMaskFar[i] = 0f;
				_SuperDetailStrengthMultB[i] = 0f;
				_SuperDetailStrengthMultBSelfMaskNear[i] = 0f;
				_SuperDetailStrengthMultBSelfMaskFar[i] = 0f;
				_SuperDetailStrengthNormal[i] = 0f;
				_BumpMapGlobalStrength[i] = 0.3f;
				_snow_strength_per_layer[i] = 1f;
				VerticalTextureStrength[i] = 0.5f;
				AO_strength[i] = 1f;
				TERRAIN_LayerWetStrength[i] = 1f;
				TERRAIN_WaterLevel[i] = 0.5f;
				TERRAIN_WaterLevelSlopeDamp[i] = 0.5f;
				TERRAIN_WaterEdge[i] = 2f;
				TERRAIN_WaterGloss[i] = 0.1f;
				TERRAIN_WaterGlossDamper[i] = 0f;
				TERRAIN_Refraction[i] = 0.01f;
				TERRAIN_WetRefraction[i] = 0.2f;
				TERRAIN_Flow[i] = 0.3f;
				TERRAIN_WetFlow[i] = 0.05f;
				TERRAIN_WaterMetallic[i] = 0.1f;
				TERRAIN_WetGloss[i] = 0.05f;
				TERRAIN_WaterColor[i] = new Color(0.9f, 0.9f, 1f, 0.5f);
				TERRAIN_WaterEmission[i] = 0f;
				_GlitterStrength[i] = 0f;
			}
		}
	}

	public bool CheckAndUpdate(ref float[] aLayerPropArray, float defVal, int len)
	{
		if (aLayerPropArray == null || aLayerPropArray.Length < len)
		{
			aLayerPropArray = new float[len];
			for (int i = 0; i < len; i++)
			{
				aLayerPropArray[i] = defVal;
			}
			return true;
		}
		return false;
	}

	public bool CheckAndUpdate(ref Color[] aLayerPropArray, Color defVal, int len)
	{
		if (aLayerPropArray == null || aLayerPropArray.Length < len)
		{
			aLayerPropArray = new Color[len];
			for (int i = 0; i < len; i++)
			{
				aLayerPropArray[i] = defVal;
			}
			return true;
		}
		return false;
	}
}
