using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AbilityData/CheckRunningTurn")]
    public class CheckRunningTurn : StateData
    {
        public override void OnEnter(CharacterState characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {

        }

        public override void UpdateAbility(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            CharacterControl control = characterState.GetCharacterControl(animator);
            if (control.IsFacingRightDirection())
            {
                if (control.MoveLeft)
                {
                    animator.SetBool(TransitionParameter.Turn.ToString(), true);
                }
            }
            
            if (!control.IsFacingRightDirection())
            {
                if (control.MoveRight)
                {
                    animator.SetBool(TransitionParameter.Turn.ToString(), true);
                }
            }
        }

        public override void OnExit(CharacterState characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            animator.SetBool(TransitionParameter.Turn.ToString(), false);
        }
    }
}