using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AbilityData/CheckMovement")]
    public class CheckMovement : StateData
    {
        public override void OnEnter(CharacterState characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {

        }

        public override void UpdateAbility(CharacterState characterStateBase, Animator animator, AnimatorStateInfo stateInfo)
        {
            CharacterControl control = characterStateBase.GetCharacterControl(animator);
            animator.SetBool(TransitionParameter.Move.ToString(), control.MoveLeft || control.MoveRight);

        }

        public override void OnExit(CharacterState characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {

        }
    }
}