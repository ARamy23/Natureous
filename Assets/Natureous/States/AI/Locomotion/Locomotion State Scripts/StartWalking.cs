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
        FallPlatform,
    }

    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AI/StartWalking")]
    public class StartWalking : StateData
    {
        public override void OnEnter(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            CharacterControl control = characterState.GetCharacterControl(animator);
            Vector3 direction = control.AIProgress.pathFindingAgent.StartSphere.transform.position - control.transform.position;

            var isMovingForward = direction.z > 0f;

            control.MoveRight = isMovingForward;
            control.MoveLeft = !isMovingForward;
        }

        public override void UpdateAbility(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            CharacterControl control = characterState.GetCharacterControl(animator);
            Vector3 dist = control.AIProgress.pathFindingAgent.StartSphere.transform.position - control.transform.position;
            var startSpehereYPosition = control.AIProgress.pathFindingAgent.StartSphere.transform.position.y;
            var endSphereYPosition = control.AIProgress.pathFindingAgent.EndSphere.transform.position.y;
            // Jump
            if (startSpehereYPosition < endSphereYPosition)
            {
                if (Vector3.SqrMagnitude(dist) < 0.01f)
                {
                    control.MoveRight = false;
                    control.MoveLeft = false;
                    animator.SetBool(AIWalkTransition.JumpPlatform.ToString(), true);
                }
            }

            if (startSpehereYPosition > endSphereYPosition) // fall
            {
                animator.SetBool(AIWalkTransition.FallPlatform.ToString(), true);
            }

            if (startSpehereYPosition == endSphereYPosition) // on the same platform
            {
                if (Vector3.SqrMagnitude(dist) < 0.5f)
                {
                    control.MoveRight = false;
                    control.MoveLeft = false;
                }
            }
        }

        public override void OnExit(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            animator.SetBool(AIWalkTransition.JumpPlatform.ToString(), false);
        }
    }
}