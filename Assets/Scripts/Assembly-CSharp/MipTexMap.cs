using UnityEngine;

public class MipTexMap : MonoBehaviour
{
	private static Texture2D mipFilterTex64;

	private static Texture2D mipFilterTex128;

	private static Texture2D mipFilterTex256;

	private static Texture2D mipFilterTex512;

	private static Texture2D mipFilterTex1024;

	private static Texture2D mipFilterTex2048;

	private static void BuildMipFilterTex(int size)
	{
		size = Mathf.ClosestPowerOfTwo(size);
		Texture2D texture2D = new Texture2D(size, size, TextureFormat.Alpha8, mipChain: true);
		texture2D.anisoLevel = 3;
		texture2D.filterMode = FilterMode.Trilinear;
		texture2D.mipMapBias = 0f;
		for (int i = 0; i < texture2D.mipmapCount; i++)
		{
			int num = size * size;
			Color[] array = new Color[num];
			float num2 = 1f * (float)i / (float)(texture2D.mipmapCount - 1);
			Color color = new Color(num2, num2, num2, num2);
			for (int j = 0; j < num; j++)
			{
				array[j] = color;
			}
			texture2D.SetPixels(array, i);
		}
		texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		switch (size)
		{
		case 64:
			mipFilterTex64 = texture2D;
			break;
		case 128:
			mipFilterTex128 = texture2D;
			break;
		case 256:
			mipFilterTex256 = texture2D;
			break;
		case 512:
			mipFilterTex512 = texture2D;
			break;
		case 1024:
			mipFilterTex1024 = texture2D;
			break;
		case 2048:
			mipFilterTex2048 = texture2D;
			break;
		default:
			mipFilterTex512 = texture2D;
			break;
		}
	}

	public static Texture2D GetTex(int size)
	{
		size = Mathf.ClosestPowerOfTwo(size);
		switch (size)
		{
		case 64:
			if ((bool)mipFilterTex64)
			{
				return mipFilterTex64;
			}
			BuildMipFilterTex(size);
			return mipFilterTex64;
		case 128:
			if ((bool)mipFilterTex128)
			{
				return mipFilterTex128;
			}
			BuildMipFilterTex(size);
			return mipFilterTex128;
		case 256:
			if ((bool)mipFilterTex256)
			{
				return mipFilterTex256;
			}
			BuildMipFilterTex(size);
			return mipFilterTex256;
		case 512:
			if ((bool)mipFilterTex512)
			{
				return mipFilterTex512;
			}
			BuildMipFilterTex(size);
			return mipFilterTex512;
		case 1024:
			if ((bool)mipFilterTex1024)
			{
				return mipFilterTex1024;
			}
			BuildMipFilterTex(size);
			return mipFilterTex1024;
		case 2048:
			if ((bool)mipFilterTex2048)
			{
				return mipFilterTex2048;
			}
			BuildMipFilterTex(size);
			return mipFilterTex2048;
		default:
			if ((bool)mipFilterTex512)
			{
				return mipFilterTex512;
			}
			BuildMipFilterTex(size);
			return mipFilterTex512;
		}
	}
}
