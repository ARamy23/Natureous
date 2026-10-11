using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartlookUnity;

namespace Natureous
{
    public class AnalyticsManager : Singleton<AnalyticsManager>
    {
        private void Awake()
        {
            Smartlook.SetupAndStartRecording("7c105a30a5e7c648c52013fd05fed03a5d2f3f0d");
        }

        public void LogReachedCheckpoint(Checkpoint checkpoint)
        {
            Debug.Log("[analytics] progression Complete " + string.Join("/", new object[] { "First Platform", "Demo Level", checkpoint.transform.root.gameObject.name }));
        }

        public void LogPlayerDied()
        {
            Debug.Log("[analytics] design " + "First Platform - Demo Level - Player killed");
        }

        public void LogPlayerHasFallenIntoAbyss()
        {
            Checkpoint checkpoint = CheckpointManager.Instance.CurrentCheckpoint;
            if (checkpoint != null)
            {
                Debug.Log("[analytics] progression Fail " + string.Join("/", new object[] { "First Platform", "Demo Level", checkpoint.gameObject.name + "-Fell" }));
            }
            
        }

        public void LogPlayerKilledAnEnemy()
        {
            Debug.Log("[analytics] design " + "First Platform - Demo Level - Killed a Dummy");
        }

        public void GameOver()
        {
            Debug.Log("[analytics] progression Complete " + string.Join("/", new object[] { "First Platform", "Demo Level", "Game Over" }));
        }

        public void OpenedSurvey()
        {
            Debug.Log("[analytics] design " + "Survey Opened");
        }

        public void IgnoredSurvey()
        {
            Debug.Log("[analytics] design " + "Survey Ignored");
        }
    }
}


