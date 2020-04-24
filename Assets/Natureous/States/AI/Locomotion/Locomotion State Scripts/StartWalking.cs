﻿using System.Collections;
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
        CharacterControl player;

        public override void OnEnter(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            CharacterControl control = characterState.GetCharacterControl(animator);
            Vector3 direction = control.AIProgress.pathFindingAgent.StartSphere.transform.position - control.transform.position;

            var isMovingForward = direction.z > 0f;

            control.MoveRight = isMovingForward;
            control.MoveLeft = !isMovingForward;

            player = CharacterManager.Instance.GetPlayableCharacter();
        }

        public override void UpdateAbility(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            CharacterControl control = characterState.GetCharacterControl(animator);
            Vector3 distanceToStartSphere = control.AIProgress.pathFindingAgent.StartSphere.transform.position - control.transform.position;
            var startSpehereYPosition = control.AIProgress.pathFindingAgent.StartSphere.transform.position.y;
            var endSphereYPosition = control.AIProgress.pathFindingAgent.EndSphere.transform.position.y;

            if (startSpehereYPosition.Equals(endSphereYPosition)) // on the same platform
            {
                if (Vector3.SqrMagnitude(distanceToStartSphere) < 0.5f)
                {
                    control.MoveRight = false;
                    control.MoveLeft = false;

                    Vector3 distanceToPlayer = control.transform.position - player.transform.position;

                    if (Vector3.SqrMagnitude(distanceToPlayer) > 1f)
                    {
                        animator.gameObject.SetActive(false);
                        animator.gameObject.SetActive(true);
                    }
                    else // Temporary
                    {
                        var player = CharacterManager.Instance.GetPlayableCharacter();
                        if (player == null) return;
                        if (player.damageDetector.DamageTaken != 0) return;
                        var isMovingRight = control.IsFacingRightDirection();
                        control.MoveRight = isMovingRight;
                        control.MoveLeft = !isMovingRight;
                        control.Attack = true;
                    }

                }
            }

            // Jump
            if (startSpehereYPosition < endSphereYPosition)
            {
                if (Vector3.SqrMagnitude(distanceToStartSphere) < 0.01f)
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

            
        }

        public override void OnExit(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            animator.SetBool(AIWalkTransition.JumpPlatform.ToString(), false);
            animator.SetBool(AIWalkTransition.FallPlatform.ToString(), false);

        }
    }
}