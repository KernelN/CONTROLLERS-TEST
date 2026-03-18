using MalbersAnimations.Weapons;
using UnityEngine;

namespace Flame.Gameplay.Player.Throw
{
    public class ShootablePoolCaller : MonoBehaviour
    {
        [SerializeField] MProjectile mProj;
        [SerializeField] Rigidbody rb;
        [SerializeField] Collider coll;
        public MProjectile MProj => mProj;
        public Rigidbody Body => rb;
        public Collider Coll => coll;
        
        [Header("Recall")]
        [SerializeField] float recallMoveSpeed = 10f;
        [SerializeField] float recallLookSpeed = 2;
        [SerializeField] float recallCompleteDist = .5f;
        Transform recallTarget;
        
        public System.Action<ShootablePoolCaller, bool> OnEnabled;
        
        void OnEnable() => OnEnabled?.Invoke(this, true);
        void OnDisable() => OnEnabled?.Invoke(this, false);

        public void Recall(Transform target, bool forceRecall = false)
        {
            if(rb.isKinematic && !forceRecall) return;
            
            recallTarget = target;
            rb.isKinematic = true;
        }
        public void EndRecall()
        {
            if(!recallTarget) return;
            
            recallTarget = null;
            rb.isKinematic = false;
        }
        void Update()
        {
            if(!recallTarget) return;
            
            Vector3 disp = recallTarget.position - transform.position;

            if (disp.sqrMagnitude < recallCompleteDist * recallCompleteDist)
            {
                gameObject.SetActive(false);
                return;
            }
            
            float dt = Time.deltaTime;
            Quaternion newRot = Quaternion.RotateTowards(transform.rotation, 
                                Quaternion.LookRotation(disp.normalized, Vector3.up),
                                                            recallLookSpeed * dt);
            transform.rotation = newRot;
            rb.MovePosition(transform.position + transform.forward * (recallMoveSpeed * dt));
        }

        void OnCollisionEnter(Collision other)
        {
            if (other.transform == recallTarget)
            {
                EndRecall();
                gameObject.SetActive(false);
            }
        }
    }
}