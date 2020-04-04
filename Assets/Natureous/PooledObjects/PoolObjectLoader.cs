using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public enum PoolObjectType
    {
        AttackInfo,
    }

    public class PoolObjectLoader: MonoBehaviour
    {
        
        public static PoolObject InstantiatePrefab(PoolObjectType objectType)
        {
            GameObject gameObject = null;

            switch (objectType ){
                case PoolObjectType.AttackInfo:
                    gameObject = Instantiate(Resources.Load(objectType.ToString(), typeof(GameObject)) as GameObject);
                    break;
            }

            return gameObject.GetComponent<PoolObject>();
        }

    }

}