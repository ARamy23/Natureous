using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public enum PoolObjectType
    {
        AttackInfo,
        ThorHammer,
        HammerDownVFX,
    }

    public class PoolObjectLoader: MonoBehaviour
    {
        
        public static PoolObject InstantiatePrefab(PoolObjectType objectType)
        {
            GameObject gameObject = Instantiate(Resources.Load(objectType.ToString(), typeof(GameObject)) as GameObject, Vector3.zero, Quaternion.identity);
            return gameObject.GetComponent<PoolObject>();
        }

    }

}