using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class Respawner : MonoBehaviour
    {
        Vector3 spawnPosition;

        bool IsFallingIntoTheAbyss()
        {
            return this.gameObject.transform.position.y < -20;
        }

        void Respawn()
        {
            var currentCheckpoint = CheckpointManager.Instance.CurrentCheckpoint;
            if (currentCheckpoint != null)
            {
                this.gameObject.transform.position = currentCheckpoint.transform.position;
            }
            else
            {
                this.gameObject.transform.position = spawnPosition;
            }
        }

        private void Awake()
        {
            spawnPosition = transform.position;
        }

        private void LateUpdate()
        {
            if (IsFallingIntoTheAbyss())
                Respawn();
        }
    }
}

