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

        Coroutine MoveCoroutine;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        public void GoToTarget()
        {
            navMeshAgent.isStopped = false;

            if (ShouldTargetPlayableCharacter)
            {
                Target = CharacterManager.Instance.GetPlayableCharacter().gameObject;
            }

            navMeshAgent.SetDestination(Target.transform.position);

            if (MoveCoroutine != null)
            {
                StopCoroutine(MoveCoroutine);
            }

            MoveCoroutine = StartCoroutine(Move());
        }

        IEnumerator Move()
        {
            while (true)
            {
                if (navMeshAgent.isOnOffMeshLink)
                {
                    navMeshAgent.CompleteOffMeshLink();
                    navMeshAgent.isStopped = true;
                    yield break;
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }
}


