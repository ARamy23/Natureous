using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AbilityData/ForceTransition")]
    public class ForceTransition : StateData
    {
        [Range(0.01f, 1f)]
        public float TransitionTiming;

        public override void OnEnter(CharacterState  characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {

        }

        public override void UpdateAbility(CharacterState  characterStateBase, Animator animator, AnimatorStateInfo stateInfo)
        {
            if(stateInfo.normalizedTime >= TransitionTiming)
            {
                animator.SetBool(TransitionParameter.ForceTransition.ToString(), true);
            }
        }

        public override void OnExit(CharacterState  characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            animator.SetBool(TransitionParameter.ForceTransition.ToString(), false);
        }
    }

}