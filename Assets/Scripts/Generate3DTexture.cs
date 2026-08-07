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
        TextureWrapMode wrapMode = TextureWrapMode.Mirror;

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
                    //try to make the positions loop over a grid of [size^3]
                    positions[x + yOffset + zOffset] = new Vector3(x, y, z);
                    //(dim/2) * periodicVector3(x, y, z, dim);
                }
            }
        }

        NoiseS3D.octaves = 4;

       texture.SetPixelData<float>(NoiseS3D.NoiseArrayGPU(positions, noiseScale, true), 0);
       texture.Apply();

       AssetDatabase.CreateAsset(texture, "Assets/MirroredNoiseTexture.asset");
    }

    static Vector3 periodicVector3(float x, float y, float z, float period)
    {
        return new Vector3(periodicX(x, period), periodicY(y, period), periodicZ(z, period));
    }
    static float periodicX(float x, float period)
    {
        return Mathf.Sin(x * 2 * Mathf.PI / period);
    }
    static float periodicY(float y, float period)
    {
        return Mathf.Cos(y * 2 * Mathf.PI / period);
    }
    static float periodicZ(float z, float period)
    {
        return (Mathf.Sin(z * 4 * Mathf.PI / period) + Mathf.Cos(z * 2 * Mathf.PI / period)) / Mathf.Sqrt(2);
    }
}
