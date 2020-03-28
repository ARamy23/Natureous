using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{

    public enum GeneralBodyPart
    {
        Upper,
        Lower,
        Arm,
        Leg,

    }

    public class TriggerDetector : MonoBehaviour
    {
        public GeneralBodyPart generalBodyPart;

        private CharacterControl theAttackedCharacter;
        public List<Collider> CollidingParts = new List<Collider>();

        private void Awake()
        {
            theAttackedCharacter = this.GetComponentInParent<CharacterControl>();
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (theAttackedCharacter.RagdollParts.Contains(collider))
                return;

            CharacterControl attacker  = collider.transform.root.GetComponent<CharacterControl>();
            bool isTouchingSomethingOtherThanACharacter = attacker == null;

            if (isTouchingSomethingOtherThanACharacter)
                return;

            bool isTouchingCharacterMainBoxCollider = collider.gameObject == attacker.gameObject;

            if (isTouchingCharacterMainBoxCollider)
                return;

            if (!CollidingParts.Contains(collider))
                CollidingParts.Add(collider);
        }

        private void OnTriggerExit(Collider collider)
        {
            if (CollidingParts.Contains(collider))
                CollidingParts.Remove(collider);
        }
    }
}