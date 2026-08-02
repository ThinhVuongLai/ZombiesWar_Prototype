#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace App.Audio.Editor
{
    public static class AudioConfigCreator
    {
        const string AudioConfigsFolder = "Assets/App/Configs/Audio";

        [MenuItem("ZombiesWar/Create Audio Configs")]
        public static void CreateAudioConfigs()
        {
            if (!AssetDatabase.IsValidFolder(AudioConfigsFolder))
            {
                AssetDatabase.CreateFolder("Assets/App/Configs", "Audio");
            }

            if (AssetDatabase.LoadAssetAtPath<MusicConfig>($"{AudioConfigsFolder}/MusicConfig.asset") == null)
            {
                var musicConfig = ScriptableObject.CreateInstance<MusicConfig>();
                AssetDatabase.CreateAsset(musicConfig, $"{AudioConfigsFolder}/MusicConfig.asset");
            }

            if (AssetDatabase.LoadAssetAtPath<SfxConfig>($"{AudioConfigsFolder}/SfxConfig.asset") == null)
            {
                var sfxConfig = ScriptableObject.CreateInstance<SfxConfig>();
                AssetDatabase.CreateAsset(sfxConfig, $"{AudioConfigsFolder}/SfxConfig.asset");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[AudioConfigCreator] Da tao MusicConfig.asset va SfxConfig.asset trong Assets/App/Configs/Audio/");
        }
    }
}
#endif
