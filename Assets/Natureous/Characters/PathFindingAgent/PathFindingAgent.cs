using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Natureous
{
    public class PathFindingAgent : MonoBehaviour
    {
        public bool ShouldTargetPlayableCharacter;
        public GameObject Target;
        NavMeshAgent navMeshAgent;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        public void GoToTarget()
        {
            if (ShouldTargetPlayableCharacter)
            {
                Target = CharacterManager.Instance.GetPlayableCharacter().gameObject;
            }

            navMeshAgent.SetDestination(Target.transform.position);
        }
    }
}


