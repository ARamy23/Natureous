using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous.Core
{
    public class Respawner : MonoBehaviour
    {
        bool IsFallingIntoTheAbyss()
        {
            return this.gameObject.transform.position.y < -20;
        }

        void Respawn()
        {
            this.gameObject.transform.position = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (IsFallingIntoTheAbyss())
                Respawn();
        }
    }
}

