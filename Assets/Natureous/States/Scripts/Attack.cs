using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AbilityData/Attack")]
    public class Attack : StateData
    {
        public float AttackStartTime;
        public float AttackEndTime;

        public List<string> ColliderNames = new List<string>();

        public DeathType deathType;
        public bool MustCollide;
        public bool MustFaceTheAttacker;
        public float LethalRange;
        public int MaxHits;

        private List<AttackInfo> FinishedAttacks = new List<AttackInfo>();

        public override void OnEnter(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            animator.SetBool(TransitionParameter.Attack.ToString(), false);

            GameObject gameObject = PoolManager.Instance.GetObject(PoolObjectType.AttackInfo);
            AttackInfo attackInfo = gameObject.GetComponent<AttackInfo>();

            gameObject.SetActive(true);

            attackInfo.ResetInfo(this, characterState.GetCharacterControl(animator));

            if (!AttackManager.Instance.CurrentAttacks.Contains(attackInfo))
            {
                AttackManager.Instance.CurrentAttacks.Add(attackInfo);
            }
        }

        public override void UpdateAbility(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            ManageRegisterationOfAttack(characterState, animator, stateInfo);
            CheckCombo(characterState, animator, stateInfo);
        }

        public override void OnExit(CharacterState characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            animator.SetBool(TransitionParameter.Attack.ToString(), false);
            ClearAttack();
        }

        private void CheckCombo(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            bool isInComboTimeRange = stateInfo.normalizedTime >= AttackStartTime + ((AttackEndTime - AttackStartTime) / 3f) && stateInfo.normalizedTime < AttackEndTime + ((AttackEndTime - AttackStartTime) / 2f);
            if (isInComboTimeRange)
            {
                CharacterControl character = characterState.GetCharacterControl(animator);
                if (character.Attack)
                {
                    animator.SetBool(TransitionParameter.Attack.ToString(), true);
                }
            }
        }

        /// <summary>
        ///  Listens on stateInfo.normalizedTime and from there it decides to
        ///  register or deregister the attack in the AttackManager
        /// </summary>
        /// <param name="characterStateBase"></param>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        private void ManageRegisterationOfAttack(CharacterState characterStateBase, Animator animator, AnimatorStateInfo stateInfo)
        {
            RegisterAttack(characterStateBase, animator, stateInfo);
            DeregisterAttack(characterStateBase, animator, stateInfo);
        }

        private void RegisterAttack(CharacterState characterStateBase, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (AttackStartTime <= stateInfo.normalizedTime && AttackEndTime > stateInfo.normalizedTime)
            {
                foreach (AttackInfo attackInfo in AttackManager.Instance.CurrentAttacks)
                {
                    if (attackInfo == null)
                        continue;

                    if (!attackInfo.IsRegistered && attackInfo.AttackAbility == this)
                        attackInfo.Register(this);
                }
            }
        }

        private void DeregisterAttack(CharacterState characterStateBase, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= AttackEndTime)
            {
                foreach (AttackInfo attackInfo in AttackManager.Instance.CurrentAttacks)
                {
                    if (attackInfo == null)
                        continue;

                    if (attackInfo.AttackAbility == this && !attackInfo.IsFinished)
                    {
                        attackInfo.IsFinished = true;
                        attackInfo.GetComponent<PoolObject>().TurnOff();

                    }
                }
            }
        }

        private void ClearAttack()
        {

            FinishedAttacks.Clear();

            List<AttackInfo> currentAttacks = AttackManager.Instance.CurrentAttacks;

            foreach (AttackInfo attackInfo in currentAttacks)
            {
                if (attackInfo == null || attackInfo.AttackAbility == this)
                    FinishedAttacks.Add(attackInfo);
            }

            foreach(AttackInfo attackInfo in FinishedAttacks)
            {
                if (AttackManager.Instance.CurrentAttacks.Contains(attackInfo))
                {
                    currentAttacks.Remove(attackInfo);
                }
            }
        }
    }
}