using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Natureous
{
    enum AIWalkTransition
    {
        StartWalking,
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
            Vector3 direction = control.AIProgress.pathFindingAgent.StartPosition - control.transform.position;

            if (Vector3.SqrMagnitude(direction) < 0.5f)
            {
                control.MoveRight = false;
                control.MoveLeft = false;
            }
        }

        public override void OnExit(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {

        }
    }
}