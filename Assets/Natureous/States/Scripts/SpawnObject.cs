using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    [CreateAssetMenu(fileName = "New State", menuName = "Natureous/AbilityData/SpawnObject")]
    public class SpawnObject : StateData
    {
        [Range(0f, 1f)]
        public float SpawnTiming;

        private bool IsSpawned;

        public string ParentObjectName = string.Empty;

        public PoolObjectType poolObjectType;

        public bool shouldStickToParent;

        public override void OnEnter(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            if(SpawnTiming == 0f)
            {
                CharacterControl control = characterState.GetCharacterControl(animator);
                SpawnObj(control);
                IsSpawned = true;
            }
        }

        public override void UpdateAbility(CharacterState characterStateBase, Animator animator, AnimatorStateInfo stateInfo)
        {

            if (!IsSpawned)
            {
                if (stateInfo.normalizedTime >= SpawnTiming)
                {
                    CharacterControl control = characterStateBase.GetCharacterControl(animator);
                    SpawnObj(control);
                    IsSpawned = true; 
                }
            }
        }

        public override void OnExit(CharacterState characterStateBase, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            IsSpawned = false;
        }

        private void SpawnObj(CharacterControl control)
        {
            GameObject spawnnedObject = PoolManager.Instance.GetObject(poolObjectType);

            if (!string.IsNullOrEmpty(ParentObjectName))
            {
                GameObject parentObjectToEquipSpawnnedObjectTo = control.GetChildObject(ParentObjectName);
                spawnnedObject.transform.parent = parentObjectToEquipSpawnnedObjectTo.transform;
                spawnnedObject.transform.localPosition = Vector3.zero;
                spawnnedObject.transform.localRotation = Quaternion.identity;
            }

            if (!shouldStickToParent)
            {
                spawnnedObject.transform.parent = null;
            }

            spawnnedObject.SetActive(true);

        }
    }
}