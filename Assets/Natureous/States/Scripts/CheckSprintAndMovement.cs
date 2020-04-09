using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AbilityData/CheckSprintAndMovement")]
    public class CheckSprintAndMovement : StateData
    {
        public override void OnEnter(CharacterState characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {

        }

        public override void UpdateAbility(CharacterState characterStateBase, Animator animator, AnimatorStateInfo stateInfo)
        {
            CharacterControl control = characterStateBase.GetCharacterControl(animator);

            var isMovingOrSprinting = (control.MoveLeft || control.MoveRight) && control.Sprint;
            animator.SetBool(TransitionParameter.Sprint.ToString(), isMovingOrSprinting);
            animator.SetBool(TransitionParameter.Move.ToString(), isMovingOrSprinting);
        }

        public override void OnExit(CharacterState characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {

        }
    }
}