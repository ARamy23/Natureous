using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AbilityData/CheckSprint")]
    public class CheckSprint : StateData
    {
        public bool RequireMovement;

        public override void OnEnter(CharacterState characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {

        }

        public override void UpdateAbility(CharacterState characterStateBase, Animator animator, AnimatorStateInfo stateInfo)
        {
            CharacterControl control = characterStateBase.GetCharacterControl(animator);

            if (control.Sprint)
            {
                if (RequireMovement)
                {
                    animator.SetBool(TransitionParameter.Sprint.ToString(), control.MoveLeft || control.MoveRight);
                }
                else
                {
                    animator.SetBool(TransitionParameter.Sprint.ToString(), true);
                }
            }
            else
            {
                animator.SetBool(TransitionParameter.Sprint.ToString(), false);
            }
            

        }

        public override void OnExit(CharacterState characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {

        }
    }
}