using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AbilityData/GroundDetector")]
    public class GroundDetector : StateData
    {
        [Range(0.01f, 1f)]
        public float CheckTime;
        public float Distance;

        public override void OnEnter(CharacterState  characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {

        }

        public override void UpdateAbility(CharacterState  characterStateBase, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime < CheckTime)
                return;


            CharacterControl playerMovementController = characterStateBase.GetCharacterControl(animator);

            animator.SetBool(TransitionParameter.Grounded.ToString(), IsGrounded(playerMovementController));
        }
         
        public override void OnExit(CharacterState  characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {

        }

        bool IsGrounded(CharacterControl character)
        {
            float yVelocity = character.Rigidbody.velocity.y;
            if (yVelocity > -0.001f && yVelocity <= 0f)
            {
                return true;
            }

            if (yVelocity < 0f)
            {
                foreach (GameObject sphere in character.BottomSpheres)
                {
                    RaycastHit hit;

                    if (Physics.Raycast(sphere.transform.position, -Vector3.up, out hit, Distance))
                    {
                        if (!character.RagdollParts.Contains(hit.collider))
                            return true;
                    }
                }
            }

            return false;
        }
    }
}