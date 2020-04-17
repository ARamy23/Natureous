using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AbilityData/Jump")]
    public class Jump : StateData
    {
        [Range(0f, 1f)]
        public float JumpTiming;
        public float JumpForce;
        //public AnimationCurve Gravity;
        public AnimationCurve Pull;

        public override void OnEnter(CharacterState  characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            if (JumpTiming == 0f)
            {
                var control = characterStateBase.GetCharacterControl(animator);
                control.Rigidbody.AddForce(Vector3.up * JumpForce);
                control.animationProgress.hasPlayerJumped = true;
            }

            animator.SetBool(TransitionParameter.Grounded.ToString(), false);
        }

        public override void UpdateAbility(CharacterState  characterStateBase, Animator animator, AnimatorStateInfo stateInfo)
        {
            CharacterControl control = characterStateBase.GetCharacterControl(animator);
            //control.GravityMultiplier = Gravity.Evaluate(stateInfo.normalizedTime); 
            control.PullMultiplier = Pull.Evaluate(stateInfo.normalizedTime);

            if (!control.animationProgress.hasPlayerJumped && stateInfo.normalizedTime >= JumpTiming)
            {
                characterStateBase.GetCharacterControl(animator).Rigidbody.AddForce(Vector3.up * JumpForce);
                control.animationProgress.hasPlayerJumped = true;
            }

        }

        public override void OnExit(CharacterState  characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            CharacterControl control = characterStateBase.GetCharacterControl(animator);
            //control.GravityMultiplier = Gravity.Evaluate(stateInfo.normalizedTime); 
            control.PullMultiplier = 0f;
            control.animationProgress.hasPlayerJumped = false;

        }
    }
}