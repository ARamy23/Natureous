﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;
using GameAnalyticsSDK.Events;

namespace Natureous
{
    public class AnalyticsManager : Singleton<AnalyticsManager>
    {
        private void Awake()
        {
            GameAnalytics.Initialize();
        }

        public void LogReachedCheckpoint(Checkpoint checkpoint)
        {
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "First Platform", "Demo Level", checkpoint.transform.root.gameObject.name);
        }

        public void LogPlayerDied()
        {
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "First Platform", "Demo Level", CheckpointManager.Instance.CurrentCheckpoint.gameObject.name + "-Died");
        }

        public void LogPlayerHasFallenIntoAbyss()
        {
            Checkpoint checkpoint = CheckpointManager.Instance.CurrentCheckpoint;
            if (checkpoint != null)
            {
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "First Platform", "Demo Level", checkpoint.gameObject.name + "-Fell");
            }
            
        }

        public void LogPlayerKilledAnEnemy()
        {
            GameAnalytics.NewDesignEvent("First Platform -> Demo Level -> Killed A Dummy");
        }

        public void GameOver()
        {
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "First Platform", "Demo Level", "Game Over");
        }

        public void OpenedSurvey()
        {
            GameAnalytics.NewDesignEvent("Survey Opened");
        }

        public void IgnoredSurvey()
        {
            GameAnalytics.NewDesignEvent("Survey Ignored");
        }
    }
}


