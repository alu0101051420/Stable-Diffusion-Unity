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

      Renderer renderer = targetObject.GetComponent<Renderer>();
      Texture2D originalTexture = renderer.sharedMaterial.mainTexture as Texture2D;
      int textureWidth = originalTexture.width;
      int textureHeight = originalTexture.height;

      // Create a new Texture2D object
      Texture2D rectangleTexture = new Texture2D(textureWidth, textureHeight);

      // Create a Color array to represent the pixels of the texture
      Color[] pixels = new Color[textureWidth * textureHeight];
      for (int i = 0; i < pixels.Length; i++)
      {
        pixels[i] = Color.black;
      }

      // Calculate rectangle's boundaries
      int startX = Mathf.RoundToInt(rectangle[0] * textureWidth);
      int endX = Mathf.RoundToInt(rectangle[2] * textureWidth);
      int startY = Mathf.RoundToInt(rectangle[1] * textureHeight);
      int endY = Mathf.RoundToInt(rectangle[3] * textureHeight);

      // Swap start and end positions if necessary
      if (startX > endX)
      {
        int temp = startX;
        startX = endX;
        endX = temp;
      }

      if (startY > endY)
      {
        int temp = startY;
        startY = endY;
        endY = temp;
      }

      // Iterate through the pixels within the rectangle boundaries and set them to white
      for (int y = startY; y <= endY; y++)
      {
        for (int x = startX; x <= endX; x++)
        {
          int pixelIndex = y * textureWidth + x;
          pixels[pixelIndex] = Color.white;
        }
      }

      // Set the modified pixel array to the rectangle texture
      rectangleTexture.SetPixels(pixels);

      // Apply the changes to the rectangle texture
      rectangleTexture.Apply();

      versioningManager.AddTextureMask(targetObject, rectangleTexture);

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
          image: originalTexture.EncodeToPNG(),
          mask: rectangleTexture.EncodeToPNG()
      ));
    }
  }

  private void StoreNewTexture(Texture2D texture)
  {
    versioningManager.AddTextureVersion(targetObject, texture);
  }
}

