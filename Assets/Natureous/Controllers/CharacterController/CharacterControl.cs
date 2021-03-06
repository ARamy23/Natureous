﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{

    public enum TransitionParameter
    {
        Move,
        Jump,
        ForceTransition,
        Landing,
        Grounded,
        Attack,
    }

    public class CharacterControl : MonoBehaviour
    { 
        public Animator SkinnedMeshAnimator;
        public bool MoveLeft;
        public bool MoveRight;
        public bool Jump;
        public bool Attack;
        public GameObject ColliderEdgePrefab;
        public List<GameObject> BottomSpheres = new List<GameObject>();
        public List<GameObject> FrontSpheres = new List<GameObject>();
        public List<Collider> RagdollParts = new List<Collider>();
        public Material material;

        private List<TriggerDetector> TriggerDetectors = new List<TriggerDetector>();

        public float GravityMultiplier;
        public float PullMultiplier;
        
        private Rigidbody rigid;

        public Rigidbody Rigidbody
        {
            get
            {
                if (rigid == null)
                {
                    rigid = GetComponent<Rigidbody>();

                }

                return rigid;
            }
        }

        private void Awake()
        {
            ChangeFacingDirection(IsFacingRight: IsFacingRightDirection());
            SetupColliderSpheres();
        }

        public void SetRagdolParts()
        {
            RagdollParts.Clear();

            Collider[] colliders = this.gameObject.GetComponentsInChildren<Collider>();

            foreach(Collider collider in colliders)
            {
                if (collider.gameObject != this.gameObject)
                {
                    collider.isTrigger = true;
                    RagdollParts.Add(collider);

                    if(collider.GetComponent<TriggerDetector>() == null)
                        collider.gameObject.AddComponent<TriggerDetector>();
                    
                }
            }
        }

        public List<TriggerDetector> GetAllTriggerDetectors()
        {
            if (TriggerDetectors.Count == 0)
            {
                TriggerDetector[] triggers = this.gameObject.GetComponentsInChildren<TriggerDetector>();

                foreach (TriggerDetector trigger in triggers)
                {
                    TriggerDetectors.Add(trigger);
                }
            }

            return TriggerDetectors;
        }

        public void TurnOnRagdoll()
        {
            Rigidbody.useGravity = false;
            Rigidbody.velocity = Vector3.zero;
            this.gameObject.GetComponent<BoxCollider>().enabled = false;
            SkinnedMeshAnimator.enabled = false;
            SkinnedMeshAnimator.avatar = null;

            foreach (Collider collider in RagdollParts)
            {
                collider.isTrigger = false;
                collider.attachedRigidbody.velocity = Vector3.zero;
            }
        }

        private void SetupColliderSpheres()
        {
            BoxCollider box = GetComponent<BoxCollider>();

            float bottom = box.bounds.center.y - box.bounds.extents.y;
            float top = box.bounds.center.y + box.bounds.extents.y;
            float front = box.bounds.center.z + box.bounds.extents.z;
            float back = box.bounds.center.z - box.bounds.extents.z;

            GameObject bottomFront = CreateEdgeSphere(new Vector3(0f, bottom, front));
            GameObject bottomBack = CreateEdgeSphere(new Vector3(0f, bottom, back));
            GameObject topFront = CreateEdgeSphere(new Vector3(0f, top, front));

            bottomFront.transform.parent = this.transform;
            bottomBack.transform.parent = this.transform;
            topFront.transform.parent = this.transform;

            BottomSpheres.Add(bottomFront);
            BottomSpheres.Add(bottomBack);

            FrontSpheres.Add(bottomFront);
            FrontSpheres.Add(topFront);

            float horizontalSectionLength = (bottomFront.transform.position - bottomBack.transform.position).magnitude / 5f;
            CreateMiddleSpheres(bottomFront, -Vector3.forward, horizontalSectionLength, 4, BottomSpheres);

            float verticalSectionLength = (bottomFront.transform.position - topFront.transform.position).magnitude / 10f;
            CreateMiddleSpheres(bottomFront, Vector3.up, verticalSectionLength, 9, FrontSpheres);
        }

        private void FixedUpdate()
        {
            if (Rigidbody.velocity.y < 0f)
            {
                Rigidbody.velocity += Vector3.down * GravityMultiplier;
            }

            if (Rigidbody.velocity.y > 0f && !Jump)
            {
                Rigidbody.velocity += Vector3.down * PullMultiplier;
            }
        }

        public void CreateMiddleSpheres(GameObject start, Vector3 direction, float sectionLength, int numberOfIterations, List<GameObject> spheresList)
        {
            for (int i = 0; i < numberOfIterations; i++)
            {
                Vector3 pos = start.transform.position + (direction * sectionLength * (i + 1));

                GameObject gameObject = CreateEdgeSphere(pos);
                gameObject.transform.parent = this.transform;
                spheresList.Add(gameObject);
            }
        }

        public GameObject CreateEdgeSphere(Vector3 pos)
        {
            GameObject obj = Instantiate(ColliderEdgePrefab, pos, Quaternion.identity);
            return obj;
        }

        public void MoveForward(float Speed, float SpeedGraphEvaluation)
        {
            transform.Translate(Vector3.forward * Speed * SpeedGraphEvaluation * Time.deltaTime);
        }

        public void ChangeFacingDirection(bool IsFacingRight)
        {
            float yRotation = IsFacingRight ? 0f : 180f;
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }

        public bool IsFacingRightDirection()
        {
            return transform.forward.z > 0f;
        }

        public void ChangeMaterial()
        {
            if (material == null)
                Debug.LogError("No Material Specified");

            Renderer[] materials = this.gameObject.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in materials)
            {
                if (renderer.gameObject != this.gameObject)
                {
                    renderer.material = material;
                }
            }
        }
    }
}