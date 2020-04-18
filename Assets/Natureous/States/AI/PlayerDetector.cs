using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class PlayerDetector : MonoBehaviour
    {
        private CharacterControl player;
        private bool isSpawned = false;
        // Start is called before the first frame update
        void Start()
        {
            player = CharacterManager.Instance.GetPlayableCharacter();
        }

        // Update is called once per frame
        void Update()
        {
            if (player == null || isSpawned) return;
            SpawnEnemyIfNeeded();
        }

        void SpawnEnemyIfNeeded()
        {
            var distanceToPlayer = GetDistanceToPlayer();

            if (distanceToPlayer <= 200f)
                SpawnEnemy();
        }

        float GetDistanceToPlayer()
        {
            var positionDifferenceVectorToPlayer = transform.position - player.transform.position;
            var distanceToPlayer = Vector3.SqrMagnitude(positionDifferenceVectorToPlayer);
            return distanceToPlayer;
        }

        void SpawnEnemy()
        {
            var enemy = GetComponent<CharacterControl>();
            enemy.enabled = true;
            enemy.ActivateAllComponents();
            isSpawned = true;
        }
    }
}


