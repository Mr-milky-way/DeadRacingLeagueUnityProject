using UnityEngine;

namespace drl
{
	public class UnifyInstancingMaterials : MonoBehaviour
	{
		public bool runOnAwake = true;

		public Material materialTemplate;

		[NonReorderable]
		public Material[] materialsReplacement;

		[NonReorderable]
		public MaterialProperty[] materialProperties;

		public MeshRenderer[] meshRenderers;

		private void Awake()
		{
			if (runOnAwake)
			{
				GetMeshRenderers();
				UnifyMaterials();
			}
		}

		public void GetMeshRenderers()
		{
			if (meshRenderers.Length == 0)
			{
				meshRenderers = GetComponentsInChildren<MeshRenderer>();
			}
		}

		public bool UnifyMaterials()
		{
			if (!materialTemplate)
			{
				Debug.LogWarning("UnifyInstancingMaterials >> <b>'Material Template'</b> property is not specified!", base.gameObject);
				return false;
			}
			if (materialsReplacement.Length == 0)
			{
				Debug.LogWarning("UnifyInstancingMaterials >> There are no <b>'Materials Replacement'</b> specified!", base.gameObject);
				return false;
			}
			MeshRenderer[] array = meshRenderers;
			foreach (MeshRenderer meshRenderer in array)
			{
				Material sharedMaterial = meshRenderer.sharedMaterial;
				if (!sharedMaterial)
				{
					continue;
				}
				bool flag = false;
				Material[] array2 = materialsReplacement;
				foreach (Material material in array2)
				{
					if (sharedMaterial == material)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					continue;
				}
				if (sharedMaterial.shader != materialTemplate.shader)
				{
					Debug.LogWarning("UnifyInstancingMaterials >> The material shader from <b>'" + sharedMaterial.name + "'</b> doesn't match with template material shader <b>'" + materialTemplate.name + "'</b>!", meshRenderer.gameObject);
					continue;
				}
				meshRenderer.material = materialTemplate;
				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				MaterialProperty[] array3 = materialProperties;
				foreach (MaterialProperty materialProperty in array3)
				{
					switch (materialProperty.type)
					{
					case MaterialPropertyBlockType.Color:
						materialPropertyBlock.SetColor(materialProperty.name, sharedMaterial.GetColor(materialProperty.name));
						break;
					case MaterialPropertyBlockType.Float:
						materialPropertyBlock.SetFloat(materialProperty.name, sharedMaterial.GetFloat(materialProperty.name));
						break;
					case MaterialPropertyBlockType.Int:
						materialPropertyBlock.SetInt(materialProperty.name, sharedMaterial.GetInt(materialProperty.name));
						break;
					case MaterialPropertyBlockType.Vector:
						materialPropertyBlock.SetVector(materialProperty.name, sharedMaterial.GetVector(materialProperty.name));
						break;
					}
				}
				meshRenderer.SetPropertyBlock(materialPropertyBlock);
			}
			return true;
		}
	}
}
