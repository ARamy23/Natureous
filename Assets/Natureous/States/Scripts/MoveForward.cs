using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AbilityData/MoveForward")]
    public class MoveForward : StateData
    {
        public bool AllowEarlyTurn;
        public bool LockDirection;
        public bool Constant;
        public AnimationCurve SpeedGraph;
        public float Speed;
        public float BlockDistance;

        public override void OnEnter(CharacterState  characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            if (AllowEarlyTurn)
            {
                CharacterControl control = characterState.GetCharacterControl(animator);

                if (control.MoveLeft)
                {
                    control.ChangeFacingDirection(IsFacingRight: false);
                }

                if (control.MoveRight)
                {
                    control.ChangeFacingDirection(IsFacingRight: true);
                }

            }
        }

        public override void UpdateAbility(CharacterState  characterStateBase, Animator animator, AnimatorStateInfo stateInfo)
        {
            CharacterControl controller = characterStateBase.GetCharacterControl(animator);

            if (Constant)
            {
                ConstantMovement(animator, stateInfo, controller);
            }
            else 
            {
                ControlledMovement(animator, stateInfo, controller);
            }
        }

        private void ConstantMovement(Animator animator, AnimatorStateInfo stateInfo, CharacterControl controller)
        {
            if (!IsBlocked(controller))
            {
                controller.MoveForward(Speed, SpeedGraph.Evaluate(stateInfo.normalizedTime));
            }
        }

        private void ControlledMovement(Animator animator, AnimatorStateInfo stateInfo, CharacterControl controller)
        {
            if (controller.Jump)
            {
                animator.SetBool(TransitionParameter.Jump.ToString(), true);
            }

            if (controller.MoveLeft && controller.MoveRight)
            {
                animator.SetBool(TransitionParameter.Move.ToString(), false);
                return;
            }

            if (!controller.MoveLeft && !controller.MoveRight)
            {
                animator.SetBool(TransitionParameter.Move.ToString(), false);
                return;
            }

            if (controller.MoveRight)
            {
                if (!LockDirection)
                    controller.ChangeFacingDirection(IsFacingRight: true);

                if (IsBlocked(controller))
                    return;
                controller.MoveForward(Speed, SpeedGraph.Evaluate(stateInfo.normalizedTime));
            }

            if (controller.MoveLeft)
            {
                if (!LockDirection)
                    controller.ChangeFacingDirection(IsFacingRight: false);

                if (IsBlocked(controller))
                    return;
                controller.MoveForward(Speed, SpeedGraph.Evaluate(stateInfo.normalizedTime));
            }
        }

        public override void OnExit(CharacterState  characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {

        }
        
        bool IsBlocked(CharacterControl control)
        {
            
            foreach (GameObject sphere in control.FrontSpheres)
            {
                RaycastHit hit;

                if (Physics.Raycast(sphere.transform.position, control.transform.forward, out hit, BlockDistance ))
                {
                    if(!control.RagdollParts.Contains(hit.collider))
                    {
                        if (IsBodyPart(hit.collider))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        bool IsBodyPart(Collider collider)
        {
            CharacterControl control = collider.transform.root.GetComponent<CharacterControl>();

            if (control == null)
                return false;
            else if (control == collider.gameObject)
                return false;
            else if (control.RagdollParts.Contains(collider))
                return true;
            else
                return false;

        }
    }

}