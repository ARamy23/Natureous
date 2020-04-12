using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AbilityData/TeleportOnLedge")]
    public class TeleportOnLedge : StateData
    {
        public override void OnEnter(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            
        }

        public override void UpdateAbility(CharacterState characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {

        }

        public override void OnExit(CharacterState characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            CharacterControl control = CharacterManager.Instance.GetCharacter(animator);

            Vector3 endPosition = control.LedgeChecker.GrabbedLedge.transform.position + control.LedgeChecker.GrabbedLedge.EndPosition;

            control.transform.position = endPosition;
            control.SkinnedMeshAnimator.transform.position = endPosition;
            control.SkinnedMeshAnimator.transform.parent = control.transform;
        }        
    }
}


