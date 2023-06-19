using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public static class AssetBundleBuildHelper
{
    public static void SaveAssetBundle<T>(string filePath, T asset) where T : Object
    {
        var assetBundleBuild = new AssetBundleBuild
        {
            assetBundleName = Path.GetFileName(filePath),
            assetNames = new[] { AssetDatabase.GetAssetPath(asset) }
        };

        BuildPipeline.BuildAssetBundles(Path.GetDirectoryName(filePath), new[] { assetBundleBuild }, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }

    public static T LoadAssetBundle<T>(string filePath) where T : Object
    {
        var assetBundle = AssetBundle.LoadFromFile(filePath);
        var asset = assetBundle.LoadAsset<T>(Path.GetFileNameWithoutExtension(filePath));
        assetBundle.Unload(false);
        return asset;
    }
}

#endif
