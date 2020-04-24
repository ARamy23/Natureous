using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class GameOverScreenUIHandler : MonoBehaviour
    {
        public void ExitApplication()
        {
            AnalyticsManager.Instance.IgnoredSurvey();
            Application.Quit();
        }

        public void OpenSurvey()
        {
            AnalyticsManager.Instance.OpenedSurvey();
            Application.OpenURL("https://forms.gle/GT7QqC5sFkXL2ApEA");
        }
    }
}


