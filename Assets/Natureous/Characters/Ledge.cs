using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class Ledge : MonoBehaviour
    {
        public Vector3 Offset;
        public Vector3 EndPosition;

        public static bool IsLedge(GameObject gameObject)
        {
            return gameObject.GetComponent<Ledge>() != null;
        }
    }
}


