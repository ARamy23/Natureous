using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class LedgeChecker : MonoBehaviour
    {
        Ledge ledge = null;
        public Ledge GrabbedLedge;
        public bool isGrabbingLedge;

        private void OnTriggerEnter(Collider other)
        {
            ledge = other.gameObject.GetComponent<Ledge>();

            var didCollideWithALedge = ledge != null;
            if (didCollideWithALedge)
            {
                isGrabbingLedge = true;
                GrabbedLedge = ledge;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            ledge = other.gameObject.GetComponent<Ledge>();

            var didCollideWithALedge = ledge != null;
            if (didCollideWithALedge)
            {
                isGrabbingLedge = false;
                GrabbedLedge = null;
            }
        }
    }
}
