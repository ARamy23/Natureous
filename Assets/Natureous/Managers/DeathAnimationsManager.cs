using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class DeathAnimationsManager : Singleton<DeathAnimationsManager>
    {
        DeathAnimationLoader deathAnimationLoader;
        List<RuntimeAnimatorController> SuitableDeathAnimators = new List<RuntimeAnimatorController>();

        void SetUpDeathAnimationLoader()
        {
            if (deathAnimationLoader == null)
            {
                GameObject gameObject = Instantiate(Resources.Load("DeathAnimationLoader", typeof(GameObject)) as GameObject);
                deathAnimationLoader = gameObject.GetComponent<DeathAnimationLoader>();
            }
        }

        public RuntimeAnimatorController GetAnimator(GeneralBodyPart generalBodyPart, AttackInfo attackInfo)
        {
            SetUpDeathAnimationLoader();

            SuitableDeathAnimators.Clear();

            foreach(DeathAnimationData data in deathAnimationLoader.DeathAnimationDataList)
            {
                if (attackInfo.deathType == data.deathType)
                {
                    if (attackInfo.deathType == DeathType.LaunchIntoAir)
                    {
                        SuitableDeathAnimators.Add(data.Animator);
                        break;
                    }

                    if (!attackInfo.MustCollide)
                    {
                        SuitableDeathAnimators.Add(data.Animator);
                        break;
                    }
                    else
                    {
                        foreach (GeneralBodyPart part in data.GeneralBodyParts)
                        {
                            if (part == generalBodyPart)
                            {
                                SuitableDeathAnimators.Add(data.Animator);
                                break;
                            }
                        }
                    }
                }
            }

            return SuitableDeathAnimators[Random.Range(0, SuitableDeathAnimators.Count)];
        } 
    }
}