using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AbilityData/ToggleBoxCollider")]
    public class ToggleBoxCollider : StateData
    {
        public bool IsEnabled;
        public bool OnStart;
        public bool OnEnd;

        public override void OnEnter(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            if (OnStart)
            {
                CharacterControl control = characterState.GetCharacterControl(animator);
                HandleTogglingBoxCollider(control);
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
                HandleTogglingBoxCollider(control);
            }
        }

        private void HandleTogglingBoxCollider(CharacterControl control)
        {
            control.Rigidbody.velocity = Vector3.zero;
            control.GetComponent<BoxCollider>().enabled = IsEnabled;
        }
    }
}