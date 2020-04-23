using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class SceneFader : MonoBehaviour
    {

        CanvasGroup canvasGroup;

        private void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public IEnumerator FadeOut(float duration)
        {
            while (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha += Time.deltaTime / duration;
                yield return null;
            }
        }

        public IEnumerator FadeIn(float duration)
        {
            while (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha -= Time.deltaTime / duration;
                yield return null;
            }
        }
    }
}


