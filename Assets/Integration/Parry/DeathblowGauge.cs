using UnityEngine;
using Flame.Gameplay.Damage;

namespace Flame.Gameplay.Combat
{
    public class DeathblowGauge : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] float maxCharge = 100f;
        [SerializeField] float decayRate = 5f;
        [SerializeField] float decayDelay = 3f;
        [SerializeField] float deathblowDistance = 2.0f;

        [Header("Health Correlation")]
        [Tooltip("Curve: X = Health % (0 to 1), Y = Charge Multiplier for Normal Hits")]
        [SerializeField] AnimationCurve healthToChargeMult = new AnimationCurve(new Keyframe(0, 2f), new Keyframe(1, 0.5f));
        
        [Header("Full Bar Behavior")]
        [SerializeField] float specialHoldingTime = 5f; // Time before bar drops when full
        [SerializeField] float specialDecreaseValue = 20f; // Faster decay when missed deathblow

        // Dependencies
        [SerializeField] Hittable hittable; 
        // Assuming you have a Health component. Using a placeholder float for now.
        public float currentHealthPct = 1.0f; 

        // Runtime
        public float CurrentCharge { get; private set; }
        public bool IsStaggered { get; private set; }

        float lastChargeTime;
        bool isSpecialDecayActive;

        void OnEnable()
        {
            if(hittable) hittable.onHitted.AddListener(OnTakeNormalHit);
        }

        void OnDisable()
        {
            if(hittable) hittable.onHitted.RemoveListener(OnTakeNormalHit);
        }

        void Update()
        {
            HandleDecay();
        }

        // --- CHARGING LOGIC ---

        // Called automatically when this enemy gets hit by a standard attack
        void OnTakeNormalHit()
        {
            if (IsStaggered) RefillStagger();

            // Calculate Charge based on Damage and Health Multiplier
            float damageVal = hittable.dmg != null ? hittable.dmg.value : 10f;
            float multiplier = healthToChargeMult.Evaluate(currentHealthPct);
            
            AddCharge(damageVal * multiplier, DeathblowChargeType.NormalHit);
        }

        public void AddCharge(float amount, DeathblowChargeType type)
        {
            if (IsStaggered && type != DeathblowChargeType.NormalHit) 
            {
                // Hitting/Parrying an already staggered enemy keeps them staggered longer
                RefillStagger();
                return;
            }

            CurrentCharge = Mathf.Clamp(CurrentCharge + amount, 0, maxCharge);
            lastChargeTime = Time.time;

            if (CurrentCharge >= maxCharge && !IsStaggered)
            {
                TriggerStaggerState();
            }
        }

        // --- STAGGER & DEATHBLOW STATE ---

        void TriggerStaggerState()
        {
            IsStaggered = true;
            isSpecialDecayActive = true;
            
            // "The deathblow stops whatever the enemy is doing and forces them into a deathblow animation"
            // anim.SetTrigger("Staggered"); 
            Debug.Log($"{name} is Staggered! Ready for Deathblow.");
        }

        void RefillStagger()
        {
            // "Once you refill it a little... the times return to normal"
            lastChargeTime = Time.time;
            // Depending on design, you might want to keep the bar full or let it decay naturally
        }

        void HandleDecay()
        {
            if (CurrentCharge <= 0) return;

            float currentDelay = (IsStaggered && isSpecialDecayActive) ? specialHoldingTime : decayDelay;
            float currentRate = (IsStaggered && isSpecialDecayActive) ? specialDecreaseValue : decayRate;

            if (Time.time > lastChargeTime + currentDelay)
            {
                CurrentCharge -= currentRate * Time.deltaTime;

                if (IsStaggered && CurrentCharge < maxCharge)
                {
                    // Stagger ended naturally
                    IsStaggered = false;
                    isSpecialDecayActive = false;
                    Debug.Log($"{name} recovered from Stagger.");
                }
            }
        }

        // --- EXECUTION ---

        public void AttemptDeathblow(Transform playerTransform)
        {
            if (!IsStaggered) return;

            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist <= deathblowDistance)
            {
                PerformDeathblow();
            }
        }

        void PerformDeathblow()
        {
            // Invoke Freeze Action (Player invulnerable, input locked)
            Debug.Log("PERFORMING DEATHBLOW");
            
            // Kill Enemy
            Destroy(gameObject); // Or play death anim then destroy
        }
    }
}