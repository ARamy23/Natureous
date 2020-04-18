using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AI/SendPathFindingAgent")]
    public class SendPathFindingAgent : StateData
    {
        public override void OnEnter(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            CharacterControl control = characterState.GetCharacterControl(animator);

            if (control.AIProgress.pathFindingAgent == null)
            {
                GameObject prefab = Instantiate(Resources.Load("PathFindingAgent", typeof(GameObject)) as GameObject);
                control.AIProgress.pathFindingAgent = prefab.GetComponent<PathFindingAgent>();
            }

            control.AIProgress.pathFindingAgent.GetComponent<NavMeshAgent>().enabled = false;
            control.AIProgress.pathFindingAgent.transform.position = control.transform.position;
            control.AIProgress.pathFindingAgent.GoToTarget();
        }

        public override void UpdateAbility(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            CharacterControl control = characterState.GetCharacterControl(animator);
            if (control.AIProgress.pathFindingAgent.StartWalk)
            {
                animator.SetBool(AIWalkTransition.StartWalking.ToString(), true);
            }
        }

        public override void OnExit(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            animator.SetBool(AIWalkTransition.StartWalking.ToString(), false);
        }
    }
}