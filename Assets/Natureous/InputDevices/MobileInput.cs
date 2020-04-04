using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Natureous
{
    public class MobileInput : MonoBehaviour
    {
        [SerializeField]
        public Joystick joystick;

        [SerializeField]
        public Button attackButton;

        [Range(0.0f, 1f)]
        public float sensitivty = 0.2f;

        [Range(0.0f, 1f)]
        public float jumpSensitivity = 0.5f;

        private void Awake()
        {
#if UNITY_STANDALONE
            this.gameObject.SetActive(false);
            joystick.gameObject.SetActive(false);
            attackButton.gameObject.SetActive(false);
#endif
        }

        void Update()
        {
            VirtualInputManager.Instance.MoveRight = joystick.Horizontal >= sensitivty;
            VirtualInputManager.Instance.MoveLeft = joystick.Horizontal <= -sensitivty;
            VirtualInputManager.Instance.Jump = joystick.Vertical >= jumpSensitivity;
        }

        public void OnAttackButtonDown()
        {
            VirtualInputManager.Instance.Attack = true;
        }

        public void OnReleaseAttackButton()
        {
            VirtualInputManager.Instance.Attack = false;
        }
    }
}


