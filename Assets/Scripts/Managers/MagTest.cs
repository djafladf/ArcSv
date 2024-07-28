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

        if (GUILayout.Button("�ڼ� ����", GUILayout.Width(120), GUILayout.Height(30)))
        {
            GameManager.instance.IM.MagStart();
        }
        GUILayout.FlexibleSpace();  // ������ ������ �ֽ��ϴ�.
        EditorGUILayout.EndHorizontal();  // ���� ���� ��

    }
}

