using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class KeyboardInput : MonoBehaviour
    {

        private void Awake()
        {
#if UNITY_ANDROID || UNITY_IOS
            this.gameObject.SetActive(false);
#endif
        }

        void Update()
        {
            VirtualInputManager.Instance.Sprint = Input.GetKey(KeyCode.LeftShift);
            VirtualInputManager.Instance.MoveRight = Input.GetKey(KeyCode.D);
            VirtualInputManager.Instance.MoveLeft = Input.GetKey(KeyCode.A);
            VirtualInputManager.Instance.Jump = Input.GetKey(KeyCode.Space);
            VirtualInputManager.Instance.Attack = Input.GetKey(KeyCode.J);
        }
    }
}