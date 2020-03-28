using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class CameraManager : Singleton<CameraManager>
    {
        private Coroutine shakeRoutine;
        private CameraController cameraController;
        public CameraController CameraController
        {
            get
            {
                if (cameraController == null)
                {
                    cameraController = GameObject.FindObjectOfType<CameraController>();
                }

                return cameraController;
            }
        }

        IEnumerator CameraShake(float duration)
        {
            CameraController.TriggerCamera(CameraTrigger.Shake);

            yield return new WaitForSeconds(duration);

            CameraController.TriggerCamera(CameraTrigger.Default);
        }

        public void ShakeCamera(float duration)
        {
            if (shakeRoutine != null)
                StopCoroutine(shakeRoutine);

            shakeRoutine = StartCoroutine(CameraShake(duration));
        }
    }
}