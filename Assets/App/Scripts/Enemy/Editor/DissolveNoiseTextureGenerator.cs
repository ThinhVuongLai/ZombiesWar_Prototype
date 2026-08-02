#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace App.Enemy.Editor
{
    public static class DissolveNoiseTextureGenerator
    {
        const int TextureSize = 512;
        const float NoiseScale = 8f;
        const string AssetPath = "Assets/App/Textures/DissolveNoise.asset";

        [MenuItem("ZombiesWar/Generate Dissolve Noise Texture")]
        static void GenerateNoiseTexture()
        {
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.R8, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Repeat;

            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float sampleX = (float)x / TextureSize * NoiseScale;
                    float sampleY = (float)y / TextureSize * NoiseScale;

                    float noise = Mathf.PerlinNoise(sampleX, sampleY);

                    // Layer 3 octaves for richer pattern
                    noise += Mathf.PerlinNoise(sampleX * 2f + 0.5f, sampleY * 2f + 0.5f) * 0.5f;
                    noise += Mathf.PerlinNoise(sampleX * 4f + 1.7f, sampleY * 4f + 1.7f) * 0.25f;
                    noise *= 0.57f; // Scale back to ~0-1 range

                    byte gray = (byte)Mathf.RoundToInt(Mathf.Clamp01(noise) * 255);
                    texture.SetPixel(x, y, new Color32(gray, gray, gray, 255));
                }
            }

            texture.Apply();

            AssetDatabase.CreateAsset(texture, AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[DissolveNoiseTextureGenerator] Đã tạo noise texture tại: {AssetPath}");
        }
    }
}
#endif
