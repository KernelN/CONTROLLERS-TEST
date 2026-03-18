using UnityEngine;

namespace Flame.Gameplay.Player.Throw
{
    [System.Serializable]
    public abstract class Thrower
    {
        static readonly int Aiming = Animator.StringToHash("IsAiming");

        [Header("Aim")]
        [SerializeField] Animator aimAnimator;
        [Header("Throw")]
        [SerializeField] Transform forwardRef;
        [SerializeField] Transform throwPoint;
        [SerializeField] float throwForce;
        internal bool isAiming;

        public System.Action<bool> IsAiming;

        public void Aim()
        {
            if(isAiming) return;
            
            aimAnimator?.SetBool(Aiming, true);
            isAiming = true;
            IsAiming?.Invoke(true);
        }
        public void Throw()
        {
            if(!isAiming) return;
            if(!CanThrow()) return;

            Rigidbody rb = CreateThrowable(throwPoint.position, forwardRef.rotation);
            rb.AddForce(forwardRef.forward * throwForce, ForceMode.Impulse);
            
            StopAim();
        }
        public void Cancel()
        {
            StopAim();
        }

        void StopAim()
        {
            aimAnimator?.SetBool(Aiming, false);
            isAiming = false;
            IsAiming?.Invoke(false);
        }
        
        internal virtual bool CanThrow() => true;
        internal abstract Rigidbody CreateThrowable(Vector3 pos, Quaternion rot);
    }
}