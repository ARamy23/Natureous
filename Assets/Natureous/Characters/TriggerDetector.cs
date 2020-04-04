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
        //public List<Collider> CollidingParts = new List<Collider>();

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

            if (!theAttackedCharacter.CollidingBodyParts.ContainsKey(this))
            {
                theAttackedCharacter.CollidingBodyParts.Add(this, new List<Collider>());
            }

            if (!theAttackedCharacter.CollidingBodyParts[this].Contains(collider))
            {
                theAttackedCharacter.CollidingBodyParts[this].Add(collider);
            }
        }

        private void OnTriggerExit(Collider theAttackingBodyPart)
        {
            if (theAttackedCharacter == null)
            {
                return;
            }


            if (theAttackedCharacter.CollidingBodyParts.ContainsKey(this))
            {
                if (theAttackedCharacter.CollidingBodyParts[this].Contains(theAttackingBodyPart))
                {
                    theAttackedCharacter.CollidingBodyParts[this].Remove(theAttackingBodyPart);
                }

                if (theAttackedCharacter.CollidingBodyParts[this].Count == 0)
                {
                    theAttackedCharacter.CollidingBodyParts.Remove(this);
                }
            }
        }
    }
}