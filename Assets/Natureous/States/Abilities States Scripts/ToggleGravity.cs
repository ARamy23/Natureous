using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AbilityData/ToggleGravity")]
    public class ToggleGravity : StateData
    {
        public bool IsEnabled;
        public bool OnStart;
        public bool OnEnd;

        public override void OnEnter(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            if (OnStart)
            {
                CharacterControl control = characterState.GetCharacterControl(animator);
                HandleTogglingGravity(control);
            }
        }

        public override void UpdateAbility(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {

        }

        public override void OnExit(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            if (OnEnd)
            {
                CharacterControl control = characterState.GetCharacterControl(animator);
                HandleTogglingGravity(control);
            }
        }

        private void HandleTogglingGravity(CharacterControl characterControl)
        {
            characterControl.Rigidbody.useGravity = IsEnabled;
            characterControl.Rigidbody.velocity = Vector3.zero;

        }
    }
}