using UnityEngine;

[AddComponentMenu("Relief Terrain/Helpers/MIP solver for standalone shader")]
[ExecuteInEditMode]
public class ReliefTerrainVertexBlendTriplanar : MonoBehaviour
{
	public Texture2D tmp_globalColorMap;

	public string save_path_colormap = "";

	public GeometryVsTerrainBlend painterInstance;

	public Material material;

	public void SetupMIPOffsets()
	{
		if (GetComponent<Renderer>() == null && this.material == null)
		{
			return;
		}
		Material material = ((!(GetComponent<Renderer>() != null)) ? this.material : GetComponent<Renderer>().sharedMaterial);
		if (material == null)
		{
			return;
		}
		if (material.HasProperty("_SplatAtlasA"))
		{
			SetupTex("_SplatAtlasA", "rtp_mipoffset_color", 1f, -1f);
		}
		if (material.HasProperty("_SplatA0"))
		{
			SetupTex("_SplatA0", "rtp_mipoffset_color");
		}
		SetupTex("_BumpMap01", "rtp_mipoffset_bump");
		SetupTex("_TERRAIN_HeightMap", "rtp_mipoffset_height");
		if (material.HasProperty("_BumpMapGlobal"))
		{
			SetupTex("_BumpMapGlobal", "rtp_mipoffset_globalnorm", material.GetFloat("_BumpMapGlobalScale"), material.GetFloat("rtp_mipoffset_globalnorm_offset"));
			if (material.HasProperty("_SuperDetailTiling"))
			{
				SetupTex("_BumpMapGlobal", "rtp_mipoffset_superdetail", material.GetFloat("_SuperDetailTiling"));
			}
			if (material.HasProperty("TERRAIN_FlowScale"))
			{
				SetupTex("_BumpMapGlobal", "rtp_mipoffset_flow", material.GetFloat("TERRAIN_FlowScale"), material.GetFloat("TERRAIN_FlowMipOffset"));
			}
		}
		if (material.HasProperty("TERRAIN_RippleMap"))
		{
			SetupTex("TERRAIN_RippleMap", "rtp_mipoffset_ripple", material.GetFloat("TERRAIN_RippleScale"));
		}
		if (material.HasProperty("TERRAIN_CausticsTex"))
		{
			SetupTex("TERRAIN_CausticsTex", "rtp_mipoffset_caustics", material.GetFloat("TERRAIN_CausticsTilingScale"));
		}
	}

	private void SetupTex(string tex_name, string param_name, float _mult = 1f, float _add = 0f)
	{
		if (!(GetComponent<Renderer>() == null) || !(this.material == null))
		{
			Material material = ((!(GetComponent<Renderer>() != null)) ? this.material : GetComponent<Renderer>().sharedMaterial);
			if (!(material == null) && material.GetTexture(tex_name) != null)
			{
				int width = material.GetTexture(tex_name).width;
				material.SetFloat(param_name, (0f - Mathf.Log(1024f / ((float)width * _mult))) / Mathf.Log(2f) + _add);
			}
		}
	}

	public void SetTopPlanarUVBounds()
	{
		MeshFilter meshFilter = GetComponent(typeof(MeshFilter)) as MeshFilter;
		if (meshFilter == null)
		{
			return;
		}
		Vector3[] vertices = meshFilter.sharedMesh.vertices;
		if (vertices.Length == 0)
		{
			return;
		}
		Vector3 vector = base.transform.TransformPoint(vertices[0]);
		Vector4 value = default(Vector4);
		value.x = vector.x;
		value.y = vector.z;
		value.z = vector.x;
		value.w = vector.z;
		for (int i = 1; i < vertices.Length; i++)
		{
			vector = base.transform.TransformPoint(vertices[i]);
			if (vector.x < value.x)
			{
				value.x = vector.x;
			}
			if (vector.z < value.y)
			{
				value.y = vector.z;
			}
			if (vector.x > value.z)
			{
				value.z = vector.x;
			}
			if (vector.z > value.w)
			{
				value.w = vector.z;
			}
		}
		value.z -= value.x;
		value.w -= value.y;
		if (!(GetComponent<Renderer>() == null) || !(this.material == null))
		{
			Material material = ((!(GetComponent<Renderer>() != null)) ? this.material : GetComponent<Renderer>().sharedMaterial);
			if (!(material == null))
			{
				material.SetVector("_TERRAIN_PosSize", value);
			}
		}
	}

	private void Awake()
	{
		SetupMIPOffsets();
		if (Application.isPlaying)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			SetupMIPOffsets();
		}
	}
}
