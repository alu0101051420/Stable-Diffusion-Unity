using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[InitializeOnLoad]
public class TextureVersioningManager : MonoBehaviour
{
    public string saveFolderPath = "MaterialVersions";
    private const string textureHistoryFileName = "TextureHistory.json";
    private Dictionary<GameObject, List<string>> objectTextureHistory = new Dictionary<GameObject, List<string>>();
    private static bool isInitialized = false;
    static TextureVersioningManager()
    {
        EditorApplication.delayCall += InitializeManager;
    }

    private static void InitializeManager()
    {
        if (!isInitialized)
        {
            TextureVersioningManager manager = FindObjectOfType<TextureVersioningManager>();
            if (manager != null)
            {
                manager.LoadTextureHistory();
            }
            isInitialized = true;
        }
    }
    private void Awake()
    {
        LoadTextureHistory();
    }

    public void AddTextureVersion(GameObject gameObject, Texture2D texture)
    {
        string objectFolderPath = GetOrCreateObjectFolderPath(gameObject);
        string textureFileName = GenerateUniqueFileName(objectFolderPath);
        string textureFilePath = Path.Combine(objectFolderPath, textureFileName);
        SaveTextureToFile(texture, textureFilePath);

        if (!objectTextureHistory.ContainsKey(gameObject))
        {
            objectTextureHistory[gameObject] = new List<string>();
        }

        objectTextureHistory[gameObject].Add(textureFilePath);

        SaveTextureHistory();
    }

    public void ApplyTextureVersion(GameObject gameObject, int versionIndex)
    {
        if (objectTextureHistory.ContainsKey(gameObject))
        {
            List<string> textureFilePaths = objectTextureHistory[gameObject];
            if (versionIndex >= 0 && versionIndex < textureFilePaths.Count)
            {
                string textureFilePath = textureFilePaths[versionIndex];
                Texture2D texture = LoadTextureFromFile(textureFilePath);

                Renderer renderer = gameObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial.mainTexture = texture;
                }
            }
        }
    }

    public void ClearTextureHistory()
    {
        objectTextureHistory.Clear();
        Directory.Delete(saveFolderPath, true);
        Directory.CreateDirectory(saveFolderPath);

        SaveTextureHistory();
    }

    private string GetOrCreateObjectFolderPath(GameObject gameObject)
    {
        string objectFolderPath = Path.Combine(saveFolderPath, gameObject.name);
        if (!Directory.Exists(objectFolderPath))
        {
            Directory.CreateDirectory(objectFolderPath);
        }
        return objectFolderPath;
    }

    private string GenerateUniqueFileName(string folderPath)
    {
        int count = 0;
        string fileName = "Texture_" + count + ".png";
        string filePath = Path.Combine(folderPath, fileName);

        while (File.Exists(filePath))
        {
            count++;
            fileName = "Texture_" + count + ".png";
            filePath = Path.Combine(folderPath, fileName);
        }

        return fileName;
    }

    private void SaveTextureToFile(Texture2D texture, string filePath)
    {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
    }

    private Texture2D LoadTextureFromFile(string filePath)
    {
        byte[] bytes = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(bytes);
        return texture;
    }

    public void LoadTextureHistory()
    {
        string filePath = Path.Combine(saveFolderPath, textureHistoryFileName);
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Dictionary<string, List<string>> serializedData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
            objectTextureHistory = new Dictionary<GameObject, List<string>>();
            foreach (var kvp in serializedData)
            {
                GameObject gameObject = GameObject.Find(kvp.Key);
                if (gameObject != null)
                {
                    objectTextureHistory.Add(gameObject, kvp.Value);
                }
            }
        }
        else
        {
            objectTextureHistory = new Dictionary<GameObject, List<string>>();
        }
    }

public void SaveTextureHistory()
{
    Dictionary<string, List<string>> serializedData = new Dictionary<string, List<string>>();
    foreach (var kvp in objectTextureHistory)
    {
        string gameObjectName = kvp.Key.name;
        List<string> textureFilePaths = kvp.Value;
        serializedData.Add(gameObjectName, textureFilePaths);
    }

    string json = JsonConvert.SerializeObject(serializedData, Formatting.Indented);
    string filePath = Path.Combine(saveFolderPath, textureHistoryFileName);
    File.WriteAllText(filePath, json);
}
}
