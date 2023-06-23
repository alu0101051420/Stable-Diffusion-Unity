using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(TextureManager))]
public class TextureManagerEditor : Editor
{

  public VisualTreeAsset m_UXML;

  public override VisualElement CreateInspectorGUI() {

    var root = new VisualElement();
    m_UXML.CloneTree(root);

    TextureManager m_TextureManager = (TextureManager)target;
    root.Q<Button>("GenerateButton").clicked += () => m_TextureManager.Generate();
    root.Q<Button>("ResetButton").clicked += () => m_TextureManager.ResetTexture();
    root.Q<Button>("SaveSceneButton").clicked += () => m_TextureManager.AddSceneVersion();
    root.Q<Button>("ResetSceneButton").clicked += () => m_TextureManager.RestoreSceneVersion();
    root.Q<Button>("FlushObjectButton").clicked += () => m_TextureManager.FlushObjectTextures();
    root.Q<Button>("FlushSceneButton").clicked += () => m_TextureManager.FlushSceneTextures();

    return root;
  }
}
#endif
