using System;
using System.Collections.Generic;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Serialization;

[AddComponentMenu("Wwise/AkSurfaceReflector")]
[ExecuteInEditMode]
public class AkSurfaceReflector : MonoBehaviour
{
	[Tooltip("The mesh to send to Spatial Audio as a Geometry Set. If this GameObject has a MeshFilter component, you can leave this parameter to None to use the same mesh for Spatial Audio. Otherwise, this parameter lets you import a different mesh for Spatial Audio purposes. We recommend using a simplified mesh.")]
	public Mesh Mesh;

	[Tooltip("The acoustic texture per submesh. The acoustic texture represents the surface of the geometry. An acoustic texture is a set of absorption levels that will filter the sound reflected from the geometry.")]
	public AcousticTexture[] AcousticTextures = new AcousticTexture[1];

	[Tooltip("The occlusion value per submesh. The occlusion value is a control value used to adjust sound parameters. Typically, a value of 1.0 represents fully occluded, and a value of 0.0 indicates that sound can be transmitted through the geometry. Default value : 1.0.")]
	[Range(0f, 1f)]
	public float[] OcclusionValues = new float[1] { 1f };

	[Tooltip("Enable or disable geometric diffraction for this mesh.")]
	public bool EnableDiffraction;

	[Tooltip("Enable or disable geometric diffraction on boundary edges for this mesh. Boundary edges are edges that are connected to only one triangle.")]
	public bool EnableDiffractionOnBoundaryEdges;

	[Tooltip("Optional room with which this surface reflector is associated. It is recommended to associate geometry with a particular room if the geometry is fully contained within the room and the room does not share any geometry with any other rooms. Doing so reduces the search space for ray casting performed by reflection and diffraction calculations.")]
	public AkRoom AssociatedRoom;

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("AcousticTexture")]
	private AcousticTexture AcousticTextureInternal = new AcousticTexture();

	[Obsolete("This functionality is deprecated as of Wwise v2019.2.0 and will be removed in a future release.")]
	public AcousticTexture AcousticTexture
	{
		get
		{
			if (AcousticTextures != null && AcousticTextures.Length >= 1)
			{
				return AcousticTextures[0];
			}
			return null;
		}
		set
		{
			int num = ((Mesh == null) ? 1 : Mesh.subMeshCount);
			if (AcousticTextures == null || AcousticTextures.Length < num)
			{
				AcousticTextures = new AcousticTexture[num];
			}
			for (int i = 0; i < num; i++)
			{
				AcousticTextures[i] = new AcousticTexture
				{
					WwiseObjectReference = value?.WwiseObjectReference
				};
			}
		}
	}

	private ulong GetID()
	{
		return (ulong)GetInstanceID();
	}

	private static void SetGeometryFromMesh(Mesh mesh, AcousticTexture[] acousticTextures, float[] occlusionValues, Transform transform, ulong geometryID, ulong associatedRoomID, bool enableDiffraction, bool enableDiffractionOnBoundaryEdges, string name = "")
	{
		Vector3[] vertices = mesh.vertices;
		int[] array = new int[vertices.Length];
		List<Vector3> list = new List<Vector3>();
		Dictionary<Vector3, int> dictionary = new Dictionary<Vector3, int>();
		for (int i = 0; i < vertices.Length; i++)
		{
			int value = 0;
			if (!dictionary.TryGetValue(vertices[i], out value))
			{
				value = list.Count;
				list.Add(vertices[i]);
				dictionary.Add(vertices[i], value);
			}
			array[i] = value;
		}
		int count = list.Count;
		Vector3[] array2 = new Vector3[count];
		for (int j = 0; j < count; j++)
		{
			Vector3 vector = transform.TransformPoint(list[j]);
			array2[j].x = vector.x;
			array2[j].y = vector.y;
			array2[j].z = vector.z;
		}
		int subMeshCount = mesh.subMeshCount;
		int count2 = mesh.triangles.Length / 3;
		if (mesh.triangles.Length % 3 != 0)
		{
			Debug.LogFormat("SetGeometryFromMesh({0}): Wrong number of triangles", mesh.name);
		}
		using AkAcousticSurfaceArray akAcousticSurfaceArray = new AkAcousticSurfaceArray(subMeshCount);
		using AkTriangleArray akTriangleArray = new AkTriangleArray(count2);
		int num = 0;
		for (int k = 0; k < subMeshCount; k++)
		{
			AkAcousticSurface akAcousticSurface = akAcousticSurfaceArray[k];
			int[] triangles = mesh.GetTriangles(k);
			int num2 = triangles.Length / 3;
			if (triangles.Length % 3 != 0)
			{
				Debug.LogFormat("SetGeometryFromMesh({0}): Wrong number of triangles in submesh {1}", mesh.name, k);
			}
			AcousticTexture acousticTexture = null;
			float occlusion = 1f;
			if (k < acousticTextures.Length)
			{
				acousticTexture = acousticTextures[k];
			}
			if (k < occlusionValues.Length)
			{
				occlusion = occlusionValues[k];
			}
			akAcousticSurface.textureID = acousticTexture?.Id ?? BaseType.InvalidId;
			akAcousticSurface.occlusion = occlusion;
			akAcousticSurface.strName = name + "_" + mesh.name + "_" + k;
			for (int l = 0; l < num2; l++)
			{
				AkTriangle akTriangle = akTriangleArray[num];
				akTriangle.point0 = (ushort)array[triangles[3 * l]];
				akTriangle.point1 = (ushort)array[triangles[3 * l + 1]];
				akTriangle.point2 = (ushort)array[triangles[3 * l + 2]];
				akTriangle.surface = (ushort)k;
				if (akTriangle.point0 != akTriangle.point1 && akTriangle.point0 != akTriangle.point2 && akTriangle.point1 != akTriangle.point2)
				{
					num++;
					continue;
				}
				Debug.LogFormat("SetGeometryFromMesh({0}): Skipped degenerate triangle({1}, {2}, {3}) in submesh {4}", mesh.name, 3 * l, 3 * l + 1, 3 * l + 2, k);
			}
		}
		if (num > 0)
		{
			AkSoundEngine.SetGeometry(geometryID, akTriangleArray, (uint)num, array2, (uint)array2.Length, akAcousticSurfaceArray, (uint)akAcousticSurfaceArray.Count(), associatedRoomID, enableDiffraction, enableDiffractionOnBoundaryEdges);
			return;
		}
		Debug.LogFormat("SetGeometry({0}): No valid triangle was found. Geometry was not set", mesh.name);
	}

	public void SetAssociatedRoom(AkRoom room)
	{
		if (AssociatedRoom != room)
		{
			AssociatedRoom = room;
			UpdateGeometry();
			if (AssociatedRoom != null)
			{
				AkRoomManager.RegisterReflector(this);
			}
			else
			{
				AkRoomManager.UnregisterReflector(this);
			}
		}
	}

	public void SetGeometry()
	{
		if (AkSoundEngine.IsInitialized())
		{
			if (Mesh == null)
			{
				Debug.LogFormat("SetGeometry({0}): No mesh found!", base.gameObject.name);
			}
			else
			{
				SetGeometryFromMesh(Mesh, AcousticTextures, OcclusionValues, base.transform, GetID(), AkRoom.GetAkRoomID(((bool)AssociatedRoom && AssociatedRoom.enabled) ? AssociatedRoom : null), EnableDiffraction, EnableDiffractionOnBoundaryEdges, base.name);
			}
		}
	}

	public void UpdateGeometry()
	{
		RemoveGeometry();
		SetGeometry();
	}

	public void RemoveGeometry()
	{
		AkSoundEngine.RemoveGeometry(GetID());
	}

	[Obsolete("This functionality is deprecated as of Wwise v2019.2.0 and will be removed in a future release.")]
	public static void RemoveGeometrySet(MeshFilter meshFilter)
	{
		if (meshFilter != null)
		{
			AkSoundEngine.RemoveGeometry(GetAkGeometrySetID(meshFilter));
		}
	}

	private void Awake()
	{
		if (Mesh == null)
		{
			MeshFilter component = GetComponent<MeshFilter>();
			if (component != null)
			{
				Mesh = component.sharedMesh;
			}
		}
	}

	private void OnEnable()
	{
		SetGeometry();
		if (AssociatedRoom != null)
		{
			AkRoomManager.RegisterReflector(this);
		}
	}

	private void OnDisable()
	{
		RemoveGeometry();
		AkRoomManager.UnregisterReflector(this);
	}

	[Obsolete("This functionality is deprecated as of Wwise v2019.2.0 and will be removed in a future release.")]
	public static ulong GetAkGeometrySetID(MeshFilter meshFilter)
	{
		return (ulong)meshFilter.GetInstanceID();
	}

	[Obsolete("This functionality is deprecated as of Wwise v2019.2.0 and will be removed in a future release.")]
	public static void AddGeometrySet(AcousticTexture acousticTexture, MeshFilter meshFilter, ulong roomID, bool enableDiffraction, bool enableDiffractionOnBoundaryEdges)
	{
		if (AkSoundEngine.IsInitialized())
		{
			if (meshFilter == null)
			{
				Debug.LogFormat("AddGeometrySet: No mesh found!");
				return;
			}
			AcousticTexture[] acousticTextures = new AcousticTexture[1] { acousticTexture };
			float[] occlusionValues = new float[1] { 1f };
			SetGeometryFromMesh(meshFilter.sharedMesh, acousticTextures, occlusionValues, meshFilter.transform, GetAkGeometrySetID(meshFilter), roomID, enableDiffraction, enableDiffractionOnBoundaryEdges, meshFilter.name);
		}
	}
}
