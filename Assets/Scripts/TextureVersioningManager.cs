using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

[InitializeOnLoad]
[ExecuteInEditMode]
public class TextureVersioningManager : MonoBehaviour
{
  // Constants and Variables
  public static string saveFolderPath = "MaterialVersions";
  private const string textureHistoryFileName = "TextureHistory.json";
  private Dictionary<GameObject, List<string>> objectTextureHistory = new Dictionary<GameObject, List<string>>();
  private Dictionary<string, List<KeyValuePair<GameObject, string>>> sceneVersions = new Dictionary<string, List<KeyValuePair<GameObject, string>>>();
  private static bool isInitialized = false;

  // Static Constructor
  TextureVersioningManager()
  {
    EditorApplication.delayCall += InitializeManager;
  }
  // Initialization Methods
  private void InitializeManager()
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

  public void OnEnable() {
    saveFolderPath = Path.Combine("MaterialVersions", SceneManager.GetActiveScene().name);
    LoadTextureHistory();
    LoadSceneHistory();
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

  public void AddTextureMask(GameObject gameObject, Texture2D texture)
  {
    string objectFolderPath = GetOrCreateObjectFolderPath(gameObject);
    string masksDirectoryName = Path.Combine(objectFolderPath, "Masks");

    if (!Directory.Exists(masksDirectoryName))
    {
      Directory.CreateDirectory(masksDirectoryName);
    }

    string maskFileName = GenerateUniqueFileName(masksDirectoryName);
    string maskFilePath = Path.Combine(masksDirectoryName, maskFileName);

    SaveTextureToFile(texture, Path.Combine(masksDirectoryName, maskFileName));
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
          SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
          if (spriteRenderer != null)
          {
            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
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
          SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
          if (spriteRenderer != null)
          {
            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
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
      SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();

      if (spriteRenderer != null)
      {
        // Get the sprite texture
        Sprite sprite = spriteRenderer.sprite;
        if (sprite != null)
        {
          Texture2D texture = sprite.texture;
          if (texture != null)
          {
            string textureName = AddTextureVersion(obj, texture);
            textureList.Add(new KeyValuePair<GameObject, string>(obj, textureName));
          }
          else
          {
            Debug.LogWarning($"Failed to retrieve texture from sprite on object: {obj.name}");
          }
        }
        else
        {
          Debug.LogWarning($"Sprite is null on object: {obj.name}");
        }
      }
      else
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
    }

    if (sceneVersions.ContainsKey(versionName))
    {
      sceneVersions[versionName] = textureList;
    }
    else
    {
      sceneVersions.Add(versionName, textureList);
    }
    SaveSceneHistory();
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
      Dictionary<string, List<string>> 
      serializedData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
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
      Debug.Log($"Texture History not found: {filePath}");
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

  public void LoadSceneHistory()
  {
    string filePath = Path.Combine(saveFolderPath, "SceneHistory.json");

    if (File.Exists(filePath))
    {
      string json = File.ReadAllText(filePath);
      Dictionary<string, List<KeyValuePair<string, string>>> serializedData = JsonConvert.DeserializeObject<Dictionary<string, List<KeyValuePair<string, string>>>>(json);
      Dictionary<string, List<KeyValuePair<GameObject, string>>> deserializedData = new Dictionary<string, List<KeyValuePair<GameObject, string>>>();
      foreach (var kvp in serializedData) {
        List<KeyValuePair<GameObject, string>> versionData = new List<KeyValuePair<GameObject, string>>();
        foreach(var kvp2 in kvp.Value) {
          versionData.Add(new KeyValuePair<GameObject, string>(GameObject.Find(kvp2.Key), kvp2.Value));
        }
        deserializedData.Add(kvp.Key, versionData);
      }
      sceneVersions = deserializedData;
    }
    else
    {
      Debug.Log($"Scene History not found: {filePath}");
      sceneVersions.Clear();
    }
  }

  public void SaveSceneHistory()
  {
      Dictionary<string, List<KeyValuePair<string, string>>>
       serializedData = new Dictionary<string, List<KeyValuePair<string, string>>>();
      foreach (var kvp in sceneVersions) {
      List<KeyValuePair<string, string>> versionData = new List<KeyValuePair<string, string>>();
      foreach (var kvp2 in kvp.Value) {
        versionData.Add(new KeyValuePair<string, string>(kvp2.Key.name, kvp2.Value));
      }
      serializedData.Add(kvp.Key, versionData);
      }
    string json = JsonConvert.SerializeObject(serializedData, Formatting.Indented);
    string filePath = Path.Combine(saveFolderPath, "SceneHistory.json");
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
