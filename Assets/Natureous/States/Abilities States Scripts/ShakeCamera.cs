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


        public override void OnEnter(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            
            if (ShakeTiming == 0f)
            {
                var control = characterState.GetCharacterControl(animator);
                CameraManager.Instance.ShakeCamera(ShakeDuration);
                control.animationProgress.isShakingCamera = true;
            }
        }

        public override void UpdateAbility(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            var control = characterState.GetCharacterControl(animator);
            if (!control.animationProgress.isShakingCamera && stateInfo.normalizedTime >= ShakeTiming)
            {
                CameraManager.Instance.ShakeCamera(ShakeDuration);
                control.animationProgress.isShakingCamera = true;
            }
        }

        public override void OnExit(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            var control = characterState.GetCharacterControl(animator);
            control.animationProgress.isShakingCamera = false;
        }
    }
}