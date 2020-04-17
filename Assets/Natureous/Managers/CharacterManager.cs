﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class CharacterManager : Singleton<CharacterManager>
    {
        public List<CharacterControl> Characters = new List<CharacterControl>();

        public CharacterControl GetCharacter(PlayableCharacterType playableCharacterType)
        {
            foreach (CharacterControl control in Characters)
            {
                if (control.playableCharacterType == playableCharacterType)
                {
                    return control;
                }
            }

            return null;
        }

        public CharacterControl GetCharacter(Animator animator)
        {
            foreach (CharacterControl control in Characters)
            {
                if (control.SkinnedMeshAnimator == animator)
                {
                    return control;
                }
            }

            return null;
        }

        public CharacterControl GetPlayableCharacter()
        {
            foreach (CharacterControl control in Characters)
            {
                var manualInput = control.GetComponent<ManualInput>();
                var isControlledByAPlayer = manualInput != null && manualInput.enabled;

                if (isControlledByAPlayer)
                {
                    return control;
                }
                
            }

            return null;
        }
    }
}


