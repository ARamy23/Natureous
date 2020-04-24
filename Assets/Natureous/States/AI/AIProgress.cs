using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class AIProgress : MonoBehaviour
    {
        public PathFindingAgent pathFindingAgent;

        public bool ShouldStartWalkOnSpawn;

        private void Start()
        {
            if (ShouldStartWalkOnSpawn)
            {
                transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }
}


