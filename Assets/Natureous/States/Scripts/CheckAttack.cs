using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AbilityData/CheckAttack")]
    public class CheckAttack : StateData
    {
        public override void OnEnter(CharacterState characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            
        }

        public override void UpdateAbility(CharacterState characterStateBase, Animator animator, AnimatorStateInfo stateInfo)
        {
            CharacterControl control = characterStateBase.GetCharacterControl(animator);

            if (control.Attack)
            {
                animator.SetBool(TransitionParameter.Attack.ToString(), true);
            }

        }

        public override void OnExit(CharacterState characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            
        }
    }
}