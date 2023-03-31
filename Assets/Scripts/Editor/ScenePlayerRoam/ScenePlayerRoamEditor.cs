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
            GUILayout.Label("_______________________________________注意_____________________________________________");
            GUILayout.Label("按下 Ctrl + L键，可以屏蔽摄像机跟随，再次按下 Ctrl + L 键，可以继续摄像机跟随");
            GUILayout.Label("________________________________________________________________________________________");
            base.OnInspectorGUI();
        }
    }
}

#endif

