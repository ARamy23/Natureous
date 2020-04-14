using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class AttackInfo : MonoBehaviour
    {
        public CharacterControl Attacker = null;
        public Attack AttackAbility;

        public List<string> ColliderNames = new List<string>();

        public bool MustCollide;
        public bool MustFaceTheAttacker;

        public float LethalRange;
        public int MaxHits;
        public int CurrentHits;

        public bool IsRegistered;
        public bool IsFinished;

        public DeathType deathType;

        public void ResetInfo(Attack attack, CharacterControl attacker)
        {
            IsRegistered = false;
            IsFinished = false;
            AttackAbility = attack;
            Attacker = attacker;
        }

        public void Register(Attack attack)
        {
            IsRegistered = true;
            AttackAbility = attack;
            ColliderNames = attack.ColliderNames;
            MustCollide = attack.MustCollide;
            MustFaceTheAttacker = attack.MustFaceTheAttacker;
            LethalRange = attack.LethalRange;
            MaxHits = attack.MaxHits;
            CurrentHits = 0;
            deathType = attack.deathType;

        }

        private void OnDisable()
        {
            IsFinished = true;
        }
    }

}