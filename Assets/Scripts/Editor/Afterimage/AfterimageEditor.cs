using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace gtm.Scene
{
    [CustomEditor(typeof(Afterimage))]
    public class AfterimageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var script = (Afterimage)target;
            if (script == null)
                return;

            base.OnInspectorGUI();

            if (GUILayout.Button("刷新不可见模型名字列表"))
            {
                script.RefreshPrepareData();
            }
        }
    }
}
