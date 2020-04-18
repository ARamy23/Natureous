using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AI/JumpPlatform")]
    public class JumpPlatform : StateData
    {
        public override void OnEnter(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            CharacterControl control = characterState.GetCharacterControl(animator);
            control.Jump = true;
            control.MoveUp = true;

            var shouldFaceRightDirection = control.AIProgress.pathFindingAgent.StartSphere.transform.position.z < control.AIProgress.pathFindingAgent.EndSphere.transform.position.z;

            control.ChangeFacingDirection(IsFacingRight: shouldFaceRightDirection);
        }

        public override void UpdateAbility(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            CharacterControl control = characterState.GetCharacterControl(animator);

            float topDistance = control.AIProgress.pathFindingAgent.EndSphere.transform.position.y - control.FrontSpheres[1].transform.position.y;
            float bottomDistance = control.AIProgress.pathFindingAgent.EndSphere.transform.position.y - control.FrontSpheres[0].transform.position.y;


            if (topDistance < 1.5f && bottomDistance > 0.5f)
            {
                var isLookingForward = control.IsFacingRightDirection();
                control.MoveRight = isLookingForward;
                control.MoveLeft = !isLookingForward;
            }

            if (bottomDistance < 0.5f)
            {
                control.MoveRight = false;
                control.MoveLeft = false;
                control.MoveUp = false;
                control.Jump = false;

                animator.gameObject.SetActive(false);
                animator.gameObject.SetActive(true);
            }

        }

        public override void OnExit(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {

        }
    }
}