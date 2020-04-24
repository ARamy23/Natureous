using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class CheckpointManager : Singleton<CheckpointManager>
    {
        private Checkpoint currentCheckpoint;

        public Checkpoint CurrentCheckpoint
        {
            get
            {
                return currentCheckpoint;
            }

            set
            {
                AnalyticsManager.Instance.LogReachedCheckpoint(value);
                currentCheckpoint = value;
            }
        }

    }
}


