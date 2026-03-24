using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

namespace ClawbearGames
{
    [CustomEditor(typeof(DeadlyObjectCreater))]
    public class DeadlyObjectCreatorCustomEditor : Editor
    {
        private DeadlyObjectCreater objectCreater = null;

        private void OnEnable()
        {
            if (objectCreater == null) { objectCreater = (DeadlyObjectCreater)target; }
            objectCreater.DrawGizmos = true;
        }

        private void OnDisable()
        {
            if (objectCreater == null) { objectCreater = (DeadlyObjectCreater)target; }
            objectCreater.DrawGizmos = false;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (objectCreater == null) { objectCreater = (DeadlyObjectCreater)target; }
            if (GUILayout.Button("Create Objects", GUILayout.Height(30), GUILayout.ExpandWidth(true)))
            {
                objectCreater.CreateDeadlyObjects();
            }
        }
    }
}

