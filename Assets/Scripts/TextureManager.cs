using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class TextureManager : MonoBehaviour
{
  public GameObject targetObject;
  public string prompt;
  public string negativePrompt;
  public int steps = 50;
  public bool removeBg = false;
  public int sizeX = 512;
  public int sizeY = 512;

  public string restoreSceneVersionName;
  public string newSceneVersionName;

  public int objectTextureVersion = 0;

  private TextureVersioningManager versioningManager;

  private void OnEnable()
  {
    versioningManager = GetComponent<TextureVersioningManager>();
  }

  public void Generate()
  {
    if (prompt.Length != 0)
    {
      Debug.Log($"Sending prompt: {prompt}");
      Debug.Log($"Negative prompt: {negativePrompt}");
      Debug.Log($"Size: {sizeX}, {sizeY}");
      ImageAI imageAI = Misc.GetAddComponent<ImageAI>(gameObject);

      StartCoroutine(
          imageAI.GetImage(prompt, (Texture2D texture) =>
          {
            try
            {
              Debug.Log("Done.");

              if (SpriteOrTexture(targetObject))
              {
                if (removeBg)
                {
                  Color fillColor = new Color(0f, 0f, 0.2f, 0f);
                  ImageFloodFill.FillFromSides(texture, fillColor,
                      threshold: 0.075f, contour: 5f, bottomAlignImage: true);
                }
                targetObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
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
          width: sizeX, height: sizeY,
          steps: steps,
          negativePrompt: negativePrompt
      ));
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

  public void ResetTexture()
  {
    versioningManager.ApplyTextureVersion(targetObject, objectTextureVersion);
  }

  public void FlushSceneTextures()
  {
    versioningManager.FlushSceneTextures();
  }

  public void FlushObjectTextures()
  {
    versioningManager.FlushObjectTextures(targetObject);
  }

  public void AddSceneVersion()
  {
    versioningManager.AddSceneVersion(newSceneVersionName);
  }

  public void RestoreSceneVersion()
  {
    versioningManager.RestoreSceneVersion(restoreSceneVersionName);
  }

  private void StoreNewTexture(Texture2D texture)
  {
    versioningManager.AddTextureVersion(targetObject, texture);
  }
}
