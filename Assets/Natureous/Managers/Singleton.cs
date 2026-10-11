using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class Singleton<T> : MonoBehaviour where T: MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                // Unity's == null is also true for destroyed objects, so a dead instance is looked up again.
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<T>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        _instance = obj.AddComponent<T>();
                        obj.name = typeof(T).ToString();
                    }
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }
    }
}