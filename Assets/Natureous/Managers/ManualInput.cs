using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{

    public class ManualInput : MonoBehaviour
    {
        private CharacterControl characterControl;

        private void Awake()
        {
            characterControl = this.gameObject.GetComponent<CharacterControl>();
        }

        // Update is called once per frame
        void Update()
        {
            characterControl.MoveRight = VirtualInputManager.Instance.MoveRight;
            characterControl.MoveLeft = VirtualInputManager.Instance.MoveLeft;
            characterControl.Jump = VirtualInputManager.Instance.Jump;
            characterControl.Attack = VirtualInputManager.Instance.Attack;
        }
    }
}