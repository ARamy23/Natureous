using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AbilityData/ShakeCamera")]
    public class ShakeCamera: StateData
    {
        [Range(0f, 0.99f)]
        public float ShakeTiming;
        public float ShakeDuration = 0.5f;
        private bool isShaking = false;


        public override void OnEnter(CharacterState characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            if (ShakeTiming == 0f)
            {
                CameraManager.Instance.ShakeCamera(ShakeDuration);
                isShaking = true;
            }
                
        }

        public override void UpdateAbility(CharacterState characterStateBase, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (!isShaking && stateInfo.normalizedTime >= ShakeTiming)
            {
                CameraManager.Instance.ShakeCamera(ShakeDuration);
                isShaking = true;
            }
        }

        public override void OnExit(CharacterState characterStateBase, Animator animator, AnimatorStateInfo stateInfo)
        {
            isShaking = false;
        }
    }
}