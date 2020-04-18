using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Natureous
{
    [CustomEditor(typeof(PathFindingAgent))]
    public class PathFindingAgentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PathFindingAgent agent = (PathFindingAgent)target;

            if (GUILayout.Button("Go To Target"))
            {
                agent.GoToTarget();
            }
        }
    }
}

