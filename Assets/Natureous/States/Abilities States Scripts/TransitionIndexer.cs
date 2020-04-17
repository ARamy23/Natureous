using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public enum TransitionConditionType
    {
        Up,
        Down,
        Left,
        Right,
        Attack,
        Jump,
        GrabbingLedge,
        Sprint,
    }

    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AbilityData/TransitionIndexer")]
    public class TransitionIndexer : StateData
    {
        public int Index;
        public List<TransitionConditionType> transitionConditionTypes = new List<TransitionConditionType>();

        public override void OnEnter(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            CharacterControl control = characterState.GetCharacterControl(animator);
            if (MakeTransition(control))
            {
                animator.SetInteger(TransitionParameter.TransitionIndex.ToString(), Index);
            }
        }

        public override void UpdateAbility(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            CharacterControl control = characterState.GetCharacterControl(animator);
            if (MakeTransition(control))
            {
                animator.SetInteger(TransitionParameter.TransitionIndex.ToString(), Index);
            }
            else
            {
                animator.SetInteger(TransitionParameter.TransitionIndex.ToString(), 0);
            }
        }

        public override void OnExit(CharacterState characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            animator.SetInteger(TransitionParameter.TransitionIndex.ToString(), 0);
        }

        private bool MakeTransition(CharacterControl control)
        {
            foreach (var condition in transitionConditionTypes)
            {
                 switch (condition)
                {
                    case TransitionConditionType.Up:
                        if (!control.MoveUp) return false;
                        break;
                    case TransitionConditionType.Down:
                        if (!control.MoveDown) return false;
                        break;
                    case TransitionConditionType.Left:
                        if (!control.MoveLeft) return false;
                        break;
                    case TransitionConditionType.Right:
                        if (!control.MoveRight) return false;
                        break;
                    case TransitionConditionType.Jump:
                        if (!control.Jump) return false;
                        break;
                    case TransitionConditionType.Attack:
                        if (!control.animationProgress.AttackTriggered) return false;
                        break;
                    case TransitionConditionType.GrabbingLedge:
                        if (!control.LedgeChecker.isGrabbingLedge) return false;
                        break;
                    case TransitionConditionType.Sprint:
                        if (!control.Sprint) return false;
                        break;
                }
            }

            return true;
        }
    }
}