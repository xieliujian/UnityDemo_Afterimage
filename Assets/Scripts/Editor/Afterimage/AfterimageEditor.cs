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

            if (GUILayout.Button("ˢ�²��ɼ�ģ�������б�"))
            {
                script.RefreshPrepareData();
            }
        }
    }
}
