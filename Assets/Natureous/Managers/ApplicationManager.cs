using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Natureous
{
    public class ApplicationManager : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
            SceneManager.LoadScene(1);
        }
    }
}