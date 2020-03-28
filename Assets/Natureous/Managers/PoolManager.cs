using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{

    public class PoolManager : Singleton<PoolManager>
    {
        public Dictionary<PoolObjectType, List<GameObject>> PoolDictionary = new Dictionary<PoolObjectType, List<GameObject>>();

        public void SetUpDictionary()
        {
            PoolObjectType[] poolObjectTypes = System.Enum.GetValues(typeof(PoolObjectType)) as PoolObjectType[];

            foreach(PoolObjectType poolObjectType in poolObjectTypes)
            {
                if (!PoolDictionary.ContainsKey(poolObjectType))
                {
                    PoolDictionary.Add(poolObjectType, new List<GameObject>());
                }
            }
        }

        public GameObject GetObject(PoolObjectType poolObjectType)
        {
            if (PoolDictionary.Count == 0)
                SetUpDictionary();

            List<GameObject> list = PoolDictionary[poolObjectType];

            GameObject gameObject = null;

            if (list.Count > 0 )
            {
                gameObject = list[0];
                list.RemoveAt(0);
            }
            else
            {
                // perhaps we need to add the insantiated prefab to the PoolDictionary
                gameObject = PoolObjectLoader.InstantiatePrefab(poolObjectType).gameObject;

            }

            return gameObject;
        }

        public void AddObjectToThePool(PoolObject poolObject)
        {
            List<GameObject> gameObjectsInThePool = PoolDictionary[poolObject.PoolObjectType];
            gameObjectsInThePool.Add(poolObject.gameObject);
            poolObject.gameObject.SetActive(false);
        }
    }
}