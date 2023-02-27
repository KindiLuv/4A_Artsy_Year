using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
[CanEditMultipleObjects]
public class ArstyEditorWindow : EditorWindow
{
    private GameObject initObject = null;

    [MenuItem("Window/ArstyEditorWindow")]
    public static void ShowWindow()
    {
        GetWindow<ArstyEditorWindow>("Arsty Editeur");
    }

    void OnGUI()
    {
        GUILayout.Label("Arsty Editor", EditorStyles.boldLabel);

        initObject = EditorGUILayout.ObjectField("GameObject � initialiser", initObject, typeof(GameObject), true) as GameObject;

        if (GUILayout.Button("Initialiser les donn�es"))
        {
            if (initObject != null)
            {
                InitializeEditor ie = initObject.GetComponent<InitializeEditor>();
                if (ie != null)
                {
                    ie.InitializeEditor();
                }
            }
            else
            {
                Debug.LogWarning("S�lectionnez un GameObject � initialiser.");
            }
        }
    }
}
#endif