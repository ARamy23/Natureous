using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public enum DeathType
    {
        None,
        LaunchIntoAir,
        GroundShock
    }

    [CreateAssetMenu(fileName = "New ScriptableObject", menuName = "Natureous/Death/DeathAnimationData")]
    public class DeathAnimationData : ScriptableObject
    {
        public List<GeneralBodyPart> GeneralBodyParts = new List<GeneralBodyPart>();
        public RuntimeAnimatorController Animator;
        public bool IsFacingAttacker;
        public DeathType deathType;
    }
}


