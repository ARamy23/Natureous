using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class Checkpoint : MonoBehaviour
    {
        private void OnTriggerEnter(Collider collider)
        {
            if (collider.GetType() != typeof(BoxCollider)) return;

            var enteringCharacter = collider.transform.root.GetComponent<CharacterControl>();

            if (enteringCharacter == null) return;

            var manualInput = enteringCharacter.GetComponent<ManualInput>();
            var enteringCharacterIsThePlayer = manualInput != null && manualInput.enabled;

            if (!enteringCharacterIsThePlayer) return;

            CheckpointManager.Instance.CurrentCheckpoint = this;
        }
    }
}


