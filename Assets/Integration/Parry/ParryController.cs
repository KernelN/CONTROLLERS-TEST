using MalbersAnimations;
using UnityEngine;
using UnityEngine.Events;

namespace Flame.Gameplay.Combat
{
    public class ParryController : MonoBehaviour, IAnimatorListener
    {
        static readonly int Block = Animator.StringToHash("Block");

        [Header("Deathblow charge")]
        [SerializeField] float softParryCharge = 10f;
        [SerializeField] float perfectParryCharge = 25f;
        [Header("FXs")]
        [SerializeField] UnityEvent onSoftParry;
        [SerializeField] UnityEvent onPerfectParry;
        Animator animator;
        System.Action<bool> onBlockingFlag;
        System.Func<bool> canBlock;

        public ParryState CurrentState { get; private set; } = ParryState.None;

        public void Set(Animator animator, System.Action<bool> onBlockingFlag, System.Func<bool> canBlock)
        {
            this.animator = animator;
            this.onBlockingFlag = onBlockingFlag;
            this.canBlock = canBlock;
        }

        //Anim Events
        public void SetParryWindow(int windowState) => CurrentState = (ParryState)windowState;

        //Methods
        public void TryBlock()
        {
            if(!canBlock.Invoke()) return;
            
            onBlockingFlag.Invoke(true);
            
            animator.SetTrigger(Block);
        }
        /// <summary>
        /// Called by Hittable when damage is received. Returns true if damage should be negated.
        /// </summary>
        public bool TryParry(DamageData dmg, Transform attacker, out ParryState resultState)
        {
            resultState = CurrentState;

            if (CurrentState == ParryState.None) return false;

            // Try to find the Enemy's Deathblow Gauge
            var enemyGauge = attacker.GetComponentInParent<DeathblowGauge>();
            if (enemyGauge)
            {
                float chargeAmount = (CurrentState == ParryState.Perfect) ? perfectParryCharge : softParryCharge;
                DeathblowChargeType type = (CurrentState == ParryState.Perfect)
                    ? DeathblowChargeType.PerfectParry
                    : DeathblowChargeType.SoftParry;

                enemyGauge.AddCharge(chargeAmount, type);
            }

            // FXs
            if(CurrentState == ParryState.Perfect)
                onPerfectParry?.Invoke();
            else
                onSoftParry?.Invoke();

            // "Invoke Freeze Action" (Micro-pause for impact)
            // Time.timeScale = 0f; (Handled by your game manager usually)
            
            return true; // Damage negated
        }

        public bool OnAnimatorBehaviourMessage(string message, object value)
        {
            if (message != "SetParryWindow") return false;
            SetParryWindow((int)value);
            return true;
        }
    }
}