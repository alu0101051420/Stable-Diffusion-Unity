using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class InpaintingManager : MonoBehaviour
{
  public GameObject targetObject;
  public string prompt;
  public int steps = 50;

  public List<Rect> rectangles = new List<Rect>();

  private TextureVersioningManager versioningManager;

  private void Awake()
  {
    versioningManager = GetComponent<TextureVersioningManager>();
  }

  public void Generate(float[] rectangle)
  {
    if (prompt.Length != 0)
    {

      // generate mask image, potentially save it

      Debug.Log($"Sending prompt: {prompt}");

      ImageAI imageAI = Misc.GetAddComponent<ImageAI>(gameObject);

      StartCoroutine(
          imageAI.GetImage(prompt, (Texture2D texture) =>
          {
            Debug.Log("Done.");
            Renderer renderer = targetObject.GetComponent<Renderer>();
            Material tempMaterial = new Material(renderer.sharedMaterial);
            tempMaterial.mainTexture = texture;
            renderer.sharedMaterial = tempMaterial;
            StoreNewTexture(texture);
          },
          useCache: false,
          width: 512, height: 512,
          steps: steps
          )
      );
    }
  }
  private void StoreNewTexture(Texture2D texture)
  {
    versioningManager.AddTextureVersion(targetObject, texture);
  }

}

