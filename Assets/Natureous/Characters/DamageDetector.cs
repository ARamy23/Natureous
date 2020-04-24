using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Natureous
{
    public class DamageDetector : MonoBehaviour
    {
        CharacterControl character;
        GeneralBodyPart DamagedBodyPart;

        public int DamageTaken;

        private void Awake()
        {
            DamageTaken = 0;
            character = GetComponent<CharacterControl>();
        }

        private void Update()
        {
            if (AttackManager.Instance.CurrentAttacks.Count > 0)
            {
                CheckAttack();
            }
        }

        private void CheckAttack()
        {
            foreach(AttackInfo attack in AttackManager.Instance.CurrentAttacks)
            {
                if (attack == null)
                    continue;

                if (!attack.IsRegistered)
                    continue;

                if (attack.IsFinished)
                    continue; 

                if (attack.CurrentHits >= attack.MaxHits)
                    continue;

                if (attack.Attacker == character)
                    continue;

                if (attack.MustFaceTheAttacker)
                {
                    Vector3 vector = this.transform.position - attack.Attacker.transform.position;
                    if (vector.z * attack.Attacker.transform.forward.z < 0f)
                    {
                        continue;
                    }
                }

                if (attack.MustCollide && DidCollide(attack))
                    TakeDamage(attack);
                else
                {
                    float distanceFromEnemies = Vector3.SqrMagnitude(this.gameObject.transform.position - attack.Attacker.transform.position);

                    if (distanceFromEnemies <= attack.LethalRange)
                    {
                        TakeDamage(attack);
                    }
                }
            }
        } 

        private bool DidCollide(AttackInfo attackInfo) 
        {
            foreach (var collidingBodyPart in character.CollidingBodyParts)
            {
                foreach (var collider in collidingBodyPart.Value)
                {
                    foreach (string name in attackInfo.ColliderNames)
                    {
                        if (name.Equals(collider.name))
                        {
                            if (collider.transform.root.gameObject == attackInfo.Attacker.gameObject)
                            {
                                DamagedBodyPart = collidingBodyPart.Key.generalBodyPart;
                                return true;
                            }
                        }

                    }
                }
            }

            return false;
        }

        private void TakeDamage(AttackInfo attack)
        {
            if (DamageTaken > 0) return;
            character.SkinnedMeshAnimator.runtimeAnimatorController = DeathAnimationsManager.Instance.GetAnimator(DamagedBodyPart, attack);

            attack.CurrentHits++;

            character.GetComponent<BoxCollider>().enabled = false;
            character.LedgeChecker.GetComponent<BoxCollider>().enabled = false;
            character.Rigidbody.useGravity = false;

            if (attack.MustCollide)
                CameraManager.Instance.ShakeCamera(duration: 0.5f);

            DamageTaken++;

            if (character.GetComponent<ManualInput>().enabled)
            {
                AnalyticsManager.Instance.LogPlayerDied();
                SceneManager.LoadScene(0);
            }
            else
            {
                AnalyticsManager.Instance.LogPlayerKilledAnEnemy();
            }
        }
    }
}