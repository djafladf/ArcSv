#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


public class MagTest : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.FlexibleSpace(); 
        EditorGUILayout.EndHorizontal();

    }
}
#endif
