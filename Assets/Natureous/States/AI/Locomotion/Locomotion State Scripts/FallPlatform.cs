﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AI/FallPlatform")]
    public class FallPlatform : StateData
    {
        public override void OnEnter(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            CharacterControl control = characterState.GetCharacterControl(animator);

            var shouldBeFacingForward = control.transform.position.z < control.AIProgress.pathFindingAgent.EndSphere.transform.position.z;
            control.ChangeFacingDirection(IsFacingRight: shouldBeFacingForward);
        }

        public override void UpdateAbility(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            CharacterControl control = characterState.GetCharacterControl(animator);

            if (control.IsFacingRightDirection())
            {
                if (control.transform.position.z < control.AIProgress.pathFindingAgent.EndSphere.transform.position.z)
                {
                    control.MoveRight = true;
                    control.MoveLeft = false;
                }
                else
                {
                    control.MoveRight = false;
                    control.MoveLeft = false;

                    animator.gameObject.SetActive(false);
                    animator.gameObject.SetActive(true);
                }
                
            }
            else
            {
                if (control.transform.position.z > control.AIProgress.pathFindingAgent.EndSphere.transform.position.z)
                {
                    control.MoveRight = false;
                    control.MoveLeft = true;
                }
                else
                {
                    control.MoveRight = false;
                    control.MoveLeft = false;

                    animator.gameObject.SetActive(false);
                    animator.gameObject.SetActive(true);
                }
            }
        }

        public override void OnExit(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {

        }
    }
}