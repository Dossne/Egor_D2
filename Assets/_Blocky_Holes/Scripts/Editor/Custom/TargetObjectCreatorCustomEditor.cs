using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

namespace ClawbearGames
{
    [CustomEditor(typeof(TargetObjectCreater))]
    public class TargetObjectCreatorCustomEditor : Editor
    {
        private TargetObjectCreater objectCreater = null;


        private void OnEnable()
        {
            if (objectCreater == null) { objectCreater = (TargetObjectCreater)target; }
            objectCreater.DrawGizmos = true;
        }

        private void OnDisable()
        {
            if (objectCreater == null) { objectCreater = (TargetObjectCreater)target; }
            objectCreater.DrawGizmos = false;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (objectCreater == null) { objectCreater = (TargetObjectCreater)target; }

            if (GUILayout.Button("Create Ball Objects", GUILayout.Height(30), GUILayout.ExpandWidth(true)))
            {
                objectCreater.CreateBallObjects();
            }


            if (GUILayout.Button("Create Hat Objects", GUILayout.Height(30), GUILayout.ExpandWidth(true)))
            {
                objectCreater.CreateHatObjects();
            }

            if (GUILayout.Button("Create Grave Objects", GUILayout.Height(30), GUILayout.ExpandWidth(true)))
            {
                objectCreater.CreateGraveObjects();
            }


            if (GUILayout.Button("Create Gift Box Objects", GUILayout.Height(30), GUILayout.ExpandWidth(true)))
            {
                objectCreater.CreateGiftBoxObjects();
            }


            if (GUILayout.Button("Create Pine Objects", GUILayout.Height(30), GUILayout.ExpandWidth(true)))
            {
                objectCreater.CreatePineObjects();
            }


            if (GUILayout.Button("Create Car Objects", GUILayout.Height(30), GUILayout.ExpandWidth(true)))
            {
                objectCreater.CreateCarObjects();
            }

            if (GUILayout.Button("Create House Objects", GUILayout.Height(30), GUILayout.ExpandWidth(true)))
            {
                objectCreater.CreateHouseObjects();
            }

            if (GUILayout.Button("Create Building Objects", GUILayout.Height(30), GUILayout.ExpandWidth(true)))
            {
                objectCreater.CreateBuildingObjects();
            }
        }
    }
}

