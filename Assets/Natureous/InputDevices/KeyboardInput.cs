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
            if (Input.GetKey(KeyCode.LeftShift))
            {
                VirtualInputManager.Instance.Sprint = true;
            }
            else
            {
                VirtualInputManager.Instance.Sprint = false;
            }

            if (Input.GetKey(KeyCode.D))
            {
                VirtualInputManager.Instance.MoveRight = true;
            }
            else
            {
                VirtualInputManager.Instance.MoveRight = false;
            }

            if (Input.GetKey(KeyCode.A))
            {
                VirtualInputManager.Instance.MoveLeft = true;
            }
            else
            {
                VirtualInputManager.Instance.MoveLeft = false;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                VirtualInputManager.Instance.Jump = true;
            }
            else
            {
                VirtualInputManager.Instance.Jump = false;
            }

            if(Input.GetKey(KeyCode.J))
            {
                VirtualInputManager.Instance.Attack = true;
            }
            else
            {
                VirtualInputManager.Instance.Attack = false;
            }
        }
    }
}