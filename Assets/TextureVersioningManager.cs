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
    // Constants and Variables
    public string saveFolderPath = "MaterialVersions";
    private const string textureHistoryFileName = "TextureHistory.json";
    private Dictionary<GameObject, List<string>> objectTextureHistory = new Dictionary<GameObject, List<string>>();
    private Dictionary<string, List<KeyValuePair<GameObject, string>>> sceneVersions = new Dictionary<string, List<KeyValuePair<GameObject, string>>>();
    private static bool isInitialized = false;

    // Static Constructor
    static TextureVersioningManager()
    {
        EditorApplication.delayCall += InitializeManager;
    }

    // Initialization Methods
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

    // Texture Management Methods
public string AddTextureVersion(GameObject gameObject, Texture2D texture)
{
    string objectFolderPath = GetOrCreateObjectFolderPath(gameObject);
    string textureFileName = GenerateUniqueFileName(objectFolderPath);
    string textureFilePath = Path.Combine(objectFolderPath, textureFileName);
    SaveTextureToFile(texture, textureFilePath);

    if (!objectTextureHistory.ContainsKey(gameObject))
    {
        objectTextureHistory.Add(gameObject, new List<string>());
    }

    objectTextureHistory[gameObject].Add(textureFilePath);

    SaveTextureHistory();
    return textureFileName;
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

                if (texture == null)
                {
                    Debug.Log("Error when loading texture");
                }
                else
                {
                    Renderer renderer = gameObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.sharedMaterial.mainTexture = texture;
                    }
                }
            }
        }
    }

    public void ApplyTextureVersion(GameObject gameObject, string versionIdentifier)
    {
        if (objectTextureHistory.ContainsKey(gameObject))
        {
            List<string> textureFilePaths = objectTextureHistory[gameObject];
            int versionIndex = textureFilePaths.IndexOf(versionIdentifier);

            if (versionIndex >= 0)
            {
                string textureFilePath = textureFilePaths[versionIndex];
                Texture2D texture = LoadTextureFromFile(textureFilePath);

                if (texture != null)
                {
                    Renderer renderer = gameObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.sharedMaterial.mainTexture = texture;
                    }
                }
                else
                {
                    Debug.LogWarning($"Failed to load texture from file: {textureFilePath}");
                }
            }
            else
            {
                Debug.LogWarning($"Version identifier not found: {versionIdentifier}");
            }
        }
        else
        {
            Debug.LogWarning($"GameObject not found in objectTextureHistory: {gameObject.name}");
        }
    }

    // Scene Versioning Methods
    public void AddSceneVersion(string versionName)
    {
        GameObject[] objectsInScene = GameObject.FindObjectsOfType<GameObject>();
        var textureList = new List<KeyValuePair<GameObject, string>>();

        foreach (GameObject obj in objectsInScene)
        {
            Renderer renderer = obj.GetComponent<Renderer>();

            if (renderer != null)
            {
                // Get the main texture of the shared material
                Texture sharedTexture = renderer.sharedMaterial.mainTexture;
                if (sharedTexture is Texture2D)
                {
                    Texture2D sharedTexture2D = (Texture2D)sharedTexture;

                    // Create a new Texture2D object and copy the texture data from the shared material
                    Texture2D texture = new Texture2D(sharedTexture2D.width, sharedTexture2D.height, TextureFormat.RGBA32, false);
                    texture.SetPixels(sharedTexture2D.GetPixels());
                    texture.Apply();

                    string textureName = AddTextureVersion(obj, texture);
                    textureList.Add(new KeyValuePair<GameObject, string>(obj, textureName));
                }
            }
        }

        if (sceneVersions.ContainsKey(versionName))
        {
            sceneVersions[versionName] = textureList;
        }
        else
        {
            sceneVersions.Add(versionName, textureList);
        }
    }

    public void RestoreSceneVersion(string versionName)
    {
        if (sceneVersions.TryGetValue(versionName, out List<KeyValuePair<GameObject, string>> textureList))
        {
            foreach (KeyValuePair<GameObject, string> pair in textureList)
            {
                GameObject obj = pair.Key;
                string textureName = pair.Value;

                string objectFolderPath = GetOrCreateObjectFolderPath(obj);
                string textureFilePath = Path.Combine(objectFolderPath, textureName);
                ApplyTextureVersion(obj, textureFilePath);
            }
        }
    }

    // History Management Methods
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

    public void ClearTextureHistory()
    {
        objectTextureHistory.Clear();
        Directory.Delete(saveFolderPath, true);
        Directory.CreateDirectory(saveFolderPath);

        SaveTextureHistory();
    }

    public void FlushObjectTextures(GameObject obj)
    {
        string objectFolderPath = Path.Combine(saveFolderPath, obj.name);
        if (Directory.Exists(objectFolderPath))
        {
            objectTextureHistory.Remove(obj);
            FileUtil.DeleteFileOrDirectory(objectFolderPath);
            SaveTextureHistory();
        }
    }

    public void FlushSceneTextures()
    {
        sceneVersions = new Dictionary<string, List<KeyValuePair<GameObject, string>>>();
        SaveTextureHistory();
    }

    // Utility Methods
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
}
