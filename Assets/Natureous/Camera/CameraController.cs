using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public enum CameraTrigger
    {
        Default,
        Shake,
    }

    public class CameraController : MonoBehaviour
    {
        Animator animator;
        public Animator Animator
        {
            get
            {
                if (animator == null)
                {
                    animator = GetComponent<Animator>();
                }

                return animator;
            } 
        }

        public void TriggerCamera(CameraTrigger trigger)
        {
            animator.SetTrigger(trigger.ToString());
        }
    }
}