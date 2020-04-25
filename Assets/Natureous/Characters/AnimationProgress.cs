using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class AnimationProgress : MonoBehaviour
    {
        public bool hasPlayerJumped;
        public bool isShakingCamera = false;
        public List<PoolObjectType> PoolObjectList = new List<PoolObjectType>();
        public bool AttackTriggered = false;
        public float MaxPressTime;

        private CharacterControl control;
        private float PressTime;

        private void Awake()
        {
            control = GetComponentInParent<CharacterControl>();
            PressTime = 0f;
        }

        private void Update()
        {
            if (control.Attack)
            {
                PressTime += Time.deltaTime;
                
            }
            else
            {
                PressTime = 0f;
            }

            if (PressTime == 0f)
            {
                AttackTriggered = false;
            }
            else if (PressTime > MaxPressTime)
            {
                AttackTriggered = false;
                PressTime = 0f;
            }
            else
            {
                AttackTriggered = true;
            }
        }
    }
}


