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

    var foldout = new Foldout() { viewDataKey = "TextureManagerFullInspectorFoldout" , text = "Full Inspector"};
    InspectorElement.FillDefaultInspector(foldout, serializedObject, this);
    root.Add(foldout);

    return root;
  }
}
#endif
