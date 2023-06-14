using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class TextureManager : MonoBehaviour
{
    public GameObject targetObject;
    public string prompt;
    public int steps = 50;

    public void Generate() {
        if(prompt.Length != 0) {

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
                },
                useCache: false,
                width: 512, height: 512,
                steps: steps
                )
            );
        }
    }
}
