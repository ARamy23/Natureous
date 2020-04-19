using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Natureous
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] int sceneToLoad = -1;

        [SerializeField] float fadeOutTime = 1f;
        [SerializeField] float fadeInTime = 2f;
        [SerializeField] float fadeWaitTime = 0.5f;

        private void OnTriggerEnter(Collider collider)
        {
            if (isTheEnteringObjectThePlayer(collider))
            {
                StartCoroutine(Transition());
            }
        }

        private IEnumerator Transition()
        {
            SceneFader fader = FindObjectOfType<SceneFader>();

            yield return fader.FadeOut(fadeOutTime);
            yield return SceneManager.LoadSceneAsync(sceneToLoad);
            yield return new WaitForSeconds(fadeWaitTime);
            yield return fader.FadeIn(fadeInTime);
        }

        private bool isTheEnteringObjectThePlayer(Collider collider)
        {
            if (collider.GetType() != typeof(BoxCollider)) return false;

            var enteringCharacter = collider.transform.root.GetComponent<CharacterControl>();

            if (enteringCharacter == null) return false;

            var manualInput = enteringCharacter.GetComponent<ManualInput>();
            var enteringCharacterIsThePlayer = manualInput != null && manualInput.enabled;

            return enteringCharacterIsThePlayer;
        }
    }
}


