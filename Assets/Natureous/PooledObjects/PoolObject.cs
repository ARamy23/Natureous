using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{

    public class PoolObject : MonoBehaviour
    {
        public PoolObjectType PoolObjectType;
        public float ScheduledOffTime;
        private Coroutine OffRoutine;

        // This is called when an object is coming out of the pool
        private void OnEnable()
        {
            bool thereIsAPreviousRoutine = OffRoutine != null;

            if (thereIsAPreviousRoutine)
            {
                StopCoroutine(OffRoutine);
            }

            bool thereIsASpecifiedTimingToTurnOffThePoolObject = ScheduledOffTime > 0f;

            if (thereIsASpecifiedTimingToTurnOffThePoolObject)
            {
                OffRoutine = StartCoroutine(ScheduledOff());
            }
        }


        public void TurnOff()
        {
            this.transform.parent = null;
            this.transform.position = Vector3.zero;
            this.transform.rotation = Quaternion.identity;
            PoolManager.Instance.AddObjectToThePool(this);
        }

        IEnumerator ScheduledOff()
        {
            yield return new WaitForSeconds(ScheduledOffTime);

            if (!PoolManager.Instance.PoolDictionary[PoolObjectType].Contains(this.gameObject))
            {
                TurnOff();
            }
        }
    }
}