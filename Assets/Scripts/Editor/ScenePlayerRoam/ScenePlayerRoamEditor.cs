using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

namespace PlayerRoam
{
    [CustomEditor(typeof(ScenePlayerRoam))]
    public class ScenePlayerRoamEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Label("_______________________________________ע��_____________________________________________");
            GUILayout.Label("���� Ctrl + L��������������������棬�ٴΰ��� Ctrl + L �������Լ������������");
            GUILayout.Label("________________________________________________________________________________________");
            base.OnInspectorGUI();
        }
    }
}

#endif

