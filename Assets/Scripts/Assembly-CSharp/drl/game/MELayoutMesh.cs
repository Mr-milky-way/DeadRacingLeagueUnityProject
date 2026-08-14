using System;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class MELayoutMesh : MELayoutSurface
	{
		[SerializeField]
		private MARenderer m_renderer;

		public Vector3[] meshPositions;

		public Vector3[] meshNormals;

		public int[] meshTriangles;

		public Vector3[] meshTriangleDither;

		public int meshIndexOffset;

		public bool hasInit;

		public Renderer target;

		public MARenderer renderer
		{
			get
			{
				if (!m_renderer)
				{
					return m_renderer = GetComponent<MARenderer>();
				}
				return m_renderer;
			}
		}

		public override void Generate(int p_count, Action p_callback = null)
		{
			if (!hasInit)
			{
				target = ((renderer.renderers.Count <= 0) ? null : renderer.renderers[0]);
				MeshFilter meshFilter = (target ? target.GetComponent<MeshFilter>() : null);
				if (!meshFilter)
				{
					count = 0;
					Debug.LogWarning("MELayoutMesh> Failed to find proper MeshFilter!");
					return;
				}
				Mesh sharedMesh = meshFilter.sharedMesh;
				if (!sharedMesh)
				{
					count = 0;
					Debug.LogWarning("MELayoutMesh> Failed to find proper Mesh!");
					return;
				}
				meshTriangles = sharedMesh.triangles;
				meshPositions = sharedMesh.vertices;
				meshNormals = sharedMesh.normals;
				meshTriangleDither = new Vector3[0];
				hasInit = true;
			}
			if (p_count != meshTriangleDither.Length)
			{
				meshTriangleDither = new Vector3[p_count];
			}
			for (int i = 0; i < p_count; i++)
			{
				meshTriangleDither[i] = (UnityEngine.Random.insideUnitSphere + Vector3.one) * 0.5f;
			}
			int num = ((meshTriangles != null) ? meshTriangles.Length : 0);
			if (num > 0)
			{
				meshIndexOffset = (meshIndexOffset + UnityEngine.Random.Range(123, 12345)) % num;
			}
			base.Generate(p_count, p_callback);
		}

		protected override TransformVector OnAsyncDataStep(int p_index, float p_ratio, TransformVector p_sample)
		{
			TransformVector result = p_sample;
			Vector3 vector = meshTriangleDither[p_index % meshTriangleDither.Length];
			float num = vector.x + vector.y + vector.z;
			float num2 = ((num <= 0f) ? 0.3333f : (vector.x / num));
			float num3 = ((num <= 0f) ? 0.3333f : (vector.y / num));
			float num4 = ((num <= 0f) ? 0.3333f : (vector.z / num));
			int num5 = meshTriangles.Length / 3;
			int num6 = Mathf.Max(13, num5 / count);
			int num7 = (p_index + meshIndexOffset) * num6 % num5;
			int num8 = meshTriangles[num7 * 3];
			int num9 = meshTriangles[num7 * 3 + 1];
			int num10 = meshTriangles[num7 * 3 + 2];
			Transform transform = (target ? target.transform : base.transform);
			Vector3 position = meshPositions[num8];
			position = transform.TransformPoint(position);
			Vector3 position2 = meshPositions[num9];
			position2 = transform.TransformPoint(position2);
			Vector3 position3 = meshPositions[num10];
			position3 = transform.TransformPoint(position3);
			Vector3 direction = ((meshNormals.Length == 0) ? Vector3.zero : meshNormals[num8]);
			Vector3 direction2 = ((meshNormals.Length == 0) ? Vector3.zero : meshNormals[num9]);
			Vector3 direction3 = ((meshNormals.Length == 0) ? Vector3.zero : meshNormals[num10]);
			if (meshNormals.Length == 0)
			{
				direction = Vector3.Cross(position2 - position, position3 - position);
				direction2 = Vector3.Cross(position - position2, position3 - position2);
				direction3 = Vector3.Cross(position - position3, position2 - position3);
			}
			else
			{
				direction = transform.TransformDirection(direction);
				direction2 = transform.TransformDirection(direction2);
				direction3 = transform.TransformDirection(direction3);
			}
			direction.Normalize();
			direction2.Normalize();
			direction3.Normalize();
			Vector3 vector2 = direction * num2 + direction2 * num3 + direction3 * num4;
			Vector3 rhs = ((Mathf.Abs(Vector3.Dot(vector2, Vector3.up)) > 0.99f) ? (-Vector3.forward) : Vector3.up);
			Vector3 rhs2 = Vector3.Cross(vector2, rhs);
			Vector3 forward = Vector3.Cross(vector2, rhs2);
			result.position = position * num2 + position2 * num3 + position3 * num4;
			result.rotation = Quaternion.LookRotation(forward, vector2);
			result.scale = Vector3.one;
			return result;
		}

		protected override void OnAsyncDataRefresh()
		{
			for (int i = 0; i < count; i++)
			{
				int num = Mathf.Clamp(i, 0, count - 1);
				TransformVector transformVector = base.samples[num];
				transformVector.rotation = ((!orientEnabled) ? Quaternion.identity : transformVector.rotation);
				base.samples[num] = transformVector;
			}
			Vector3 zero = Vector3.zero;
			for (int j = 0; j < count; j++)
			{
				TransformVector transformVector2 = base.samples[j];
				Vector3 vector = orientOffset + zero;
				zero += orientStep;
				Quaternion quaternion = Quaternion.AngleAxis(vector.y, Vector3.up) * Quaternion.AngleAxis(vector.x, Vector3.right) * Quaternion.AngleAxis(vector.z, Vector3.forward);
				Vector3 vector2 = transformVector2.rotation * Vector3.right;
				Vector3 vector3 = transformVector2.rotation * Vector3.up;
				Vector3 vector4 = transformVector2.rotation * Vector3.forward;
				transformVector2.position += vector2 * offsetPosition.x;
				transformVector2.position += vector3 * offsetPosition.y;
				Vector3 vector5 = ditherPosition;
				vector5.Scale(GetRandom(j));
				transformVector2.position += vector2 * vector5.x;
				transformVector2.position += vector3 * vector5.y;
				transformVector2.position += vector4 * vector5.z;
				transformVector2.rotation *= quaternion;
				base.samples[j] = transformVector2;
			}
		}
	}
}
