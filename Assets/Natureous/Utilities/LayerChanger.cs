using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class LayerChanger : MonoBehaviour
    {
        public NatureousLayer LayerType;
        public bool ShouldChangeAllChildren;

        public void ChangeLayer(Dictionary<string, int> layersAndTheirNumbersDictionary)
        {
            if (!ShouldChangeAllChildren)
            {
                Debug.Log(gameObject.name + " changing layer: " + LayerType.ToString());
                this.gameObject.layer = layersAndTheirNumbersDictionary[LayerType.ToString()];
            }
            else
            {
                Transform[] children = this.gameObject.GetComponentsInChildren<Transform>();

                foreach (var child in children)
                {
                    Debug.Log(child.gameObject.name + " changing layer: " + LayerType.ToString());
                    child.gameObject.layer = layersAndTheirNumbersDictionary[LayerType.ToString()];
                }
            }
        }
    }
}


