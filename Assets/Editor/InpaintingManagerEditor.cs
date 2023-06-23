using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif
using System.Collections.Generic;

#if UNITY_EDITOR
[CustomEditor(typeof(InpaintingManager))]
public class InpaintingManagerEditor : Editor
{
    public VisualTreeAsset m_UXML;
    private float[] m_Rectangle = { 0f, 0f, 0.5f, 0.5f };
    private VisualElement m_CustomImageField;

 public override VisualElement CreateInspectorGUI()
    {
        var root = m_UXML.CloneTree();

        InpaintingManager inpaintingManager = (InpaintingManager)target;

        root.Q<Button>("SendInpainting").clicked += () => inpaintingManager.Generate(m_Rectangle);

        var objectField = root.Q<ObjectField>("TargetObject");
        objectField.objectType = typeof(GameObject);
        objectField.value = inpaintingManager.targetObject;
        objectField.RegisterValueChangedCallback(e => UpdateCustomImageField());

        var auxTexture = GetTexture2DFromObject(inpaintingManager.targetObject); // Declare and initialize auxTexture

        m_CustomImageField = root.Q<VisualElement>("ImageField");

        var m_PosXSlider = root.Q<Slider>("PosXSlider");
        m_PosXSlider.value = 0f; // Initial X position value
        m_PosXSlider.RegisterValueChangedCallback(evt =>
        {
            float posX = evt.newValue * m_CustomImageField.resolvedStyle.width / auxTexture.width;
            m_Rectangle[0] = posX;
            UpdateCustomImageField();
        });

        var m_PosYSlider = root.Q<Slider>("PosYSlider");
        m_PosYSlider.value = 0f; // Initial Y position value
        m_PosYSlider.RegisterValueChangedCallback(evt =>
        {
            float posY = evt.newValue * m_CustomImageField.resolvedStyle.height / auxTexture.height;
            m_Rectangle[1] = posY;
            UpdateCustomImageField();
        });

        var m_EndXSlider = root.Q<Slider>("EndXSlider");
        m_EndXSlider.value = 0f; // Initial X position value
        m_EndXSlider.RegisterValueChangedCallback(evt =>
        {
            float posX = evt.newValue * m_CustomImageField.resolvedStyle.width / auxTexture.width;
            m_Rectangle[2] = posX;
            UpdateCustomImageField();
        });

        var m_EndYSlider = root.Q<Slider>("EndYSlider");
        m_EndYSlider.value = 0f; // Initial Y position value
        m_EndYSlider.RegisterValueChangedCallback(evt =>
        {
            float posY = evt.newValue * m_CustomImageField.resolvedStyle.height / auxTexture.height;
            m_Rectangle[3] = posY;
            UpdateCustomImageField();
        });

        UpdateCustomImageField();
        return root;
    }

    private Texture2D GetTexture2DFromObject(GameObject gameObject)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null && renderer.sharedMaterial != null)
        {
            Material material = renderer.sharedMaterial;
            return (Texture2D)material.mainTexture;
        }
        return null;
    }

private void UpdateCustomImageField()
{
    InpaintingManager inpaintingManager = (InpaintingManager)target;
    var auxTexture = inpaintingManager.targetObject != null ? GetTexture2DFromObject(inpaintingManager.targetObject) : null;

    if (auxTexture != null)
    {
        // Create a copy of the original texture
        Texture2D modifiedTexture = Instantiate(auxTexture);

        // Calculate the pixel coordinates of the rectangle
        int startX = Mathf.RoundToInt(m_Rectangle[0] * auxTexture.width);
        int startY = Mathf.RoundToInt(m_Rectangle[1] * auxTexture.height);
        int endX = Mathf.RoundToInt(m_Rectangle[2] * auxTexture.width);
        int endY = Mathf.RoundToInt(m_Rectangle[3] * auxTexture.height);

        // Ensure the rectangle stays within the bounds of the texture
        startX = Mathf.Clamp(startX, 0, auxTexture.width - 1);
        startY = Mathf.Clamp(startY, 0, auxTexture.height - 1);
        endX = Mathf.Clamp(endX, 0, auxTexture.width - 1);
        endY = Mathf.Clamp(endY, 0, auxTexture.height - 1);

        // Set the color of the rectangle
        Color rectangleColor = Color.white;

        // Draw the rectangle onto the modified texture
        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                modifiedTexture.SetPixel(x, y, rectangleColor);
            }
        }

        // Apply the modifications to the modified texture
        modifiedTexture.Apply();

        // Assign the modified texture as the background image
        m_CustomImageField.style.backgroundImage = modifiedTexture;

        // Set the size of the element to match the modified texture's size
        m_CustomImageField.style.width = modifiedTexture.width;
        m_CustomImageField.style.height = modifiedTexture.height;
    }
    else
    {
        m_CustomImageField.style.backgroundImage = null;
        m_CustomImageField.style.width = 0;
        m_CustomImageField.style.height = 0;
    }
    m_CustomImageField.MarkDirtyRepaint();
}

}
#endif
