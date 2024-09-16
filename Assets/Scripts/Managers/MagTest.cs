#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(GameManager))]
public class MagTest : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();  //BeginHorizontal() ���� ���ʹ� GUI ���� ���η� �����˴ϴ�.
        GUILayout.FlexibleSpace(); // ������ ������ �ֽ��ϴ�. ( ��ư�� ��� ���� ����)
                                   //��ư�� ����ϴ� . GUILayout.Button("��ư�̸�" , ����ũ��, ����ũ��)

        if (GUILayout.Button("���� ��", GUILayout.Width(120), GUILayout.Height(30)))
        {
            GameManager.instance.UM.ExpUp(0,true);
        }
        GUILayout.FlexibleSpace();  // ������ ������ �ֽ��ϴ�.
        EditorGUILayout.EndHorizontal();  // ���� ���� ��

    }
}
#endif
