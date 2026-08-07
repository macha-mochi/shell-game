using UnityEngine;
using UnityEditor;

public class Generate3DTexture : MonoBehaviour
{
    [MenuItem("GameObject/Create 3D Texture")]
    static void CreateTexture()
    {
        float noiseScale = 0.1f;
        int dim = 40;
        TextureFormat format = TextureFormat.RFloat;
        TextureWrapMode wrapMode = TextureWrapMode.Repeat;

        Texture3D texture = new Texture3D(dim, dim, dim, format, false);
        texture.wrapMode = wrapMode;

        Vector3[] positions = new Vector3[dim * dim * dim];
        for (int z = 0; z < dim; z++)
        {
            int zOffset = z * dim * dim;
            for (int y = 0; y < dim; y++)
            {
                int yOffset = y * dim;
                for (int x = 0; x < dim; x++)
                {
                    positions[x + yOffset + zOffset] = new Vector3(x, y, z);
                }
            }
        }

       texture.SetPixelData<float>(NoiseS3D.NoiseArrayGPU(positions, noiseScale, true), 0);
       texture.Apply();

       AssetDatabase.CreateAsset(texture, "Assets/3DNoiseTexture.asset");
    }
}
