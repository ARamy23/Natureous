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

        public GameObject StartSphere;
        public GameObject EndSphere;

        public bool StartWalk;

        List<Coroutine> MoveRoutines = new List<Coroutine>();

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        public void GoToTarget()
        {
            navMeshAgent.enabled = true;
            StartSphere.transform.parent = null;
            EndSphere.transform.parent = null;

            StartWalk = false;

            navMeshAgent.isStopped = false;

            if (ShouldTargetPlayableCharacter)
            {
                Target = CharacterManager.Instance.GetPlayableCharacter().gameObject;
            }

            navMeshAgent.SetDestination(Target.transform.position);

            if (MoveRoutines.Count != 0)
            {
                if (MoveRoutines[0] != null)
                {
                    StopCoroutine(MoveRoutines[0]);
                }
                MoveRoutines.RemoveAt(0);
            }

            MoveRoutines.Add(StartCoroutine(Move()));
        }

        IEnumerator Move()
        {
            while (true)
            {
                
                if (navMeshAgent.isOnOffMeshLink)
                {
                    StartSphere.transform.position = navMeshAgent.currentOffMeshLinkData.startPos;
                    EndSphere.transform.position = navMeshAgent.currentOffMeshLinkData.endPos;

                    navMeshAgent.CompleteOffMeshLink();
                                        
                    navMeshAgent.isStopped = true;
                    StartWalk = true;
                    yield break;
                }

                Vector3 distanceToTarget = transform.position - navMeshAgent.destination;
                float distance = Vector3.SqrMagnitude(distanceToTarget);
                if (distance < 1.1f)
                {
                    StartSphere.transform.position = navMeshAgent.destination;

                    EndSphere.transform.position = navMeshAgent.destination;

                    navMeshAgent.isStopped = true;

                    StartWalk = true;
                    yield break;
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }
}


