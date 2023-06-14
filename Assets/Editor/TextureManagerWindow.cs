using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class TextureManagerWindow : EditorWindow
{
   [SerializeField]
   TextureManager m_TextureManager;

   [MenuItem("Textures/Texture Manager")]
   static void CreateMenu() {
        var window = GetWindow<TextureManagerWindow>();
        window.titleContent = new GUIContent("Complex");
   }

   public void OnEnable() {
    m_TextureManager = GameObject.FindGameObjectsWithTag("TextureManager").FirstOrDefault().GetComponent<TextureManager>();
   }

   public void CreateGUI() {
    if(m_TextureManager == null)
      return;

    var scrollView = new ScrollView() { viewDataKey = "WindowScrollView" };
    scrollView.Add(new InspectorElement(m_TextureManager));
    rootVisualElement.Add(scrollView);
   }
}
