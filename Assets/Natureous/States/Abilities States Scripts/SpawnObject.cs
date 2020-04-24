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

        public string ParentObjectName = string.Empty;

        public PoolObjectType poolObjectType;

        public bool shouldStickToParent;

        public override void OnEnter(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            if(SpawnTiming == 0f)
            {
                CharacterControl control = characterState.GetCharacterControl(animator);
                SpawnObj(control);
            }
        }

        public override void UpdateAbility(CharacterState characterStateBase, Animator animator, AnimatorStateInfo stateInfo)
        {
            CharacterControl control = characterStateBase.GetCharacterControl(animator);

            if (!control.animationProgress.PoolObjectList.Contains(poolObjectType))
            {
                if (stateInfo.normalizedTime >= SpawnTiming)
                {
                    SpawnObj(control);
                }
            }
        }

        public override void OnExit(CharacterState characterState, Animator animator, AnimatorStateInfo animatorStateInfo)
        {
            var control = characterState.GetCharacterControl(animator);
            if (control.animationProgress.PoolObjectList.Contains(poolObjectType))
                control.animationProgress.PoolObjectList.Remove(poolObjectType);
        }

        private void SpawnObj(CharacterControl control)
        {
            if (control.animationProgress.PoolObjectList.Contains(poolObjectType)) return;

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

            control.animationProgress.PoolObjectList.Add(poolObjectType);
        }
    }
}