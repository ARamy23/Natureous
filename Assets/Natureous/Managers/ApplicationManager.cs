using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class ApplicationManager : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
        }
    }
}