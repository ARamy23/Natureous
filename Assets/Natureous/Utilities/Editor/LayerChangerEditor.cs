using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Natureous
{
    [CustomEditor(typeof(LayerChanger))]
    public class LayerChangerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if(GUILayout.Button("Change Layer"))
            {
                LayerChanger layerChanger = (LayerChanger)target;

                Dictionary<string, int> layersAndTheirNumbersDictionary = LayerAdderEditor.GetAllLayers();

                layerChanger.ChangeLayer(layersAndTheirNumbersDictionary);
            }
        }
    }
}


