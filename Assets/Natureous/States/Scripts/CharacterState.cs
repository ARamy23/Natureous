using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{

    public class CharacterState : StateMachineBehaviour
    {
        public List<StateData> ListOfAbilityData = new List<StateData>();

        public void UpdateAll(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
             foreach(StateData stateData in ListOfAbilityData)
            {
                stateData.UpdateAbility(characterState, animator, animatorStateInfo);
            }
        }

        

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach(StateData stateData in ListOfAbilityData)
            {
                stateData.OnEnter(this, animator, stateInfo);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            UpdateAll(this, animator, stateInfo);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (StateData stateData in ListOfAbilityData)
            {
                stateData.OnExit(this, animator, stateInfo);
            }
        }

        private CharacterControl characterController;

        public CharacterControl GetCharacterControl(Animator animator)
        {
            if(characterController == null)
            {
                characterController = animator.GetComponentInParent<CharacterControl>();
            }
            
            return characterController;
        }
    }
}