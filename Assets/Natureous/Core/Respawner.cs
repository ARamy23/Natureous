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
            var isFallingIntoAbyss = this.gameObject.transform.position.y < -20;
            if (isFallingIntoAbyss)
                AnalyticsManager.Instance.LogPlayerHasFallenIntoAbyss();
            return isFallingIntoAbyss;
        }

        public void Respawn()
        {
            StartCoroutine(Transition());
        }

        public void RespawnPlayer(RuntimeAnimatorController runtimeAnimator)
        {
            StartCoroutine(TransitionBackToLife(runtimeAnimator));
        }

        private IEnumerator Transition()
        {
            SceneFader fader = FindObjectOfType<SceneFader>();

            yield return fader.FadeOut(0.5f);
            yield return new WaitForSeconds(0.5f);
            var currentCheckpoint = CheckpointManager.Instance.CurrentCheckpoint;
            var isThePlayer = GetComponent<ManualInput>() != null;
            if (isThePlayer && currentCheckpoint != null)
            {
                Vector3 checkPointPosition = currentCheckpoint.transform.position;
                this.gameObject.transform.position = new Vector3(0.0f, checkPointPosition.y, checkPointPosition.z);
            }
            else
            {
                this.gameObject.transform.position = spawnPosition;
            }
            yield return fader.FadeIn(0.3f);
        }

        private IEnumerator TransitionBackToLife(RuntimeAnimatorController runtimeAnimator)
        {
            SceneFader fader = FindObjectOfType<SceneFader>();

            yield return fader.FadeOut(0.5f);
            yield return new WaitForSeconds(0.5f);
            var currentCheckpoint = CheckpointManager.Instance.CurrentCheckpoint;
            var character = GetComponent<CharacterControl>();

            if (currentCheckpoint != null)
            {
                Vector3 checkPointPosition = currentCheckpoint.transform.position;
                this.gameObject.transform.position = new Vector3(0.0f, checkPointPosition.y, checkPointPosition.z);
            }
            else
            {
                this.gameObject.transform.position = spawnPosition;
            }

            character.SkinnedMeshAnimator.runtimeAnimatorController = runtimeAnimator;
            character.GetComponent<BoxCollider>().enabled = true;
            character.LedgeChecker.GetComponent<BoxCollider>().enabled = true;
            character.Rigidbody.useGravity = true;
            yield return fader.FadeIn(0.3f);
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

