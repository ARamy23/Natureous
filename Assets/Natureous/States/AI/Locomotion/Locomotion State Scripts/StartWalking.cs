using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Natureous
{
    enum AIWalkTransition
    {
        StartWalking,
        JumpPlatform,
    }

    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AI/StartWalking")]
    public class StartWalking : StateData
    {
        public override void OnEnter(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            CharacterControl control = characterState.GetCharacterControl(animator);
            Vector3 direction = control.AIProgress.pathFindingAgent.StartPosition - control.transform.position;

            var isMovingForward = direction.z > 0f;

            control.MoveRight = isMovingForward;
            control.MoveLeft = !isMovingForward;
        }

        public override void UpdateAbility(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            CharacterControl control = characterState.GetCharacterControl(animator);
            Vector3 dist = control.AIProgress.pathFindingAgent.StartPosition - control.transform.position;

            if (Vector3.SqrMagnitude(dist) < 1.2f)
            {
                control.MoveRight = false;
                control.MoveLeft = false;
            
                if (control.AIProgress.pathFindingAgent.StartSphere.transform.position.y < control.AIProgress.pathFindingAgent.EndSphere.transform.position.y)
                {
                    animator.SetBool(AIWalkTransition.JumpPlatform.ToString(), true);
                }
            }
        }

        public override void OnExit(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            animator.SetBool(AIWalkTransition.JumpPlatform.ToString(), false);
        }
    }
}