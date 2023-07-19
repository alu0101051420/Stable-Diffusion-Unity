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
  public float denoisingStrength = 0;

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
      Debug.Log($"Sending prompt: {prompt}");

      ImageAI imageAI = Misc.GetAddComponent<ImageAI>(gameObject);

      Texture2D originalTexture = getOriginalTexture(targetObject);
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

      StartCoroutine(imageAI.GetImage(prompt, (Texture2D texture) =>
      {
        try
        {
          Debug.Log("Done.");

          if (SpriteOrTexture(targetObject))
          {
            targetObject.GetComponent<SpriteRenderer>().sprite =
              Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
          }
          else
          {
            Material tempMaterial = new Material(targetObject.GetComponent<Renderer>().sharedMaterial);
            tempMaterial.mainTexture = texture;
            targetObject.GetComponent<Renderer>().sharedMaterial = tempMaterial;

          }

          StoreNewTexture(texture);
        }
        catch (System.Exception e)
        {
          Debug.LogException(e);
        }
      },
            useCache: false,
            steps: steps,
            width: originalTexture.width,
            height: originalTexture.height,
            denoisingStrength: denoisingStrength,
            image: originalTexture.EncodeToPNG(),
            mask: rectangleTexture.EncodeToPNG()
      ));
    }
  }

  public void GenerateImg2Img()
  {
    if (prompt.Length != 0)
    {
      Debug.Log($"Sending prompt: {prompt}");

      ImageAI imageAI = Misc.GetAddComponent<ImageAI>(gameObject);

      Texture2D originalTexture = getOriginalTexture(targetObject);

      StartCoroutine(imageAI.GetImage(prompt, (Texture2D texture) =>
      {
        try
        {
          Debug.Log("Done.");

          if (SpriteOrTexture(targetObject))
          {
            targetObject.GetComponent<SpriteRenderer>().sprite =
              Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
          }
          else
          {
            Material tempMaterial = new Material(targetObject.GetComponent<Renderer>().sharedMaterial);
            tempMaterial.mainTexture = texture;
            targetObject.GetComponent<Renderer>().sharedMaterial = tempMaterial;

          }

          StoreNewTexture(texture);
        }
        catch (System.Exception e)
        {
          Debug.LogException(e);
        }
      },
      useCache: false,
      steps: steps,
      width: originalTexture.width,
      height: originalTexture.height,
      denoisingStrength: denoisingStrength,
      image: originalTexture.EncodeToPNG()
      ));
    }
  }

  private Texture2D getOriginalTexture(GameObject obj)
  {
    if (SpriteOrTexture(obj))
    {
      SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
      return renderer.sprite.texture as Texture2D;
    }
    else
    {
      Renderer renderer = obj.GetComponent<Renderer>();
      return renderer.sharedMaterial.mainTexture as Texture2D;
    }
  }
  private bool SpriteOrTexture(GameObject obj)
  {
    SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
    if (renderer == null)
    {
      return false;
    }
    else return true;
  }
  private void StoreNewTexture(Texture2D texture)
  {
    versioningManager.AddTextureVersion(targetObject, texture);
  }
}

