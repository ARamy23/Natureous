using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{

    public abstract class StateData : ScriptableObject
    {
        public abstract void OnEnter(CharacterState  characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo);
        public abstract void UpdateAbility(CharacterState  characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo);
        public abstract void OnExit(CharacterState  characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo);
    }
}