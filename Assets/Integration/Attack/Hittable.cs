using UnityEngine;
using UnityEngine.Events;

namespace Flame.Gameplay.Damage
{
    public struct HitterData
    {
        public Transform transform;
        public object source;
    }
    public class Hittable : MonoBehaviour
    {
        [Header("Set Values")]
        [SerializeField, Min(0)] float invulnerableTime = 0.2f;
        [Header("OPTIONALS")]
        [SerializeField] bool useDefaultKnockback = true;
        [SerializeField] public UnityEvent onHitted;
        [SerializeField] public UnityEvent onBlocked;
        [SerializeField] GameObject blockVFX;
        [HideInInspector] public bool willBlock;
        [SerializeField] Combat.ParryController parry;
        //[Header("Runtime Values")]
        Vector3 knockbackDir;
        float knockbackTimer;
        Vector3 ogPos;
        float timer = 0;
        float hitFrameTime = 0f;

        public DamageData dmg { get; private set; }
        public Transform hitter { get; private set; }
        public object attackOwner { get; private set; }
        public Collision coll { get; private set; }
        public RaycastHit hit { get; private set; }
        
        //Unity Events
        void Update()
        {
            if(timer > 0)
                timer -= Time.deltaTime;

            if (useDefaultKnockback && dmg != null && knockbackTimer < dmg.knockbackDuration)
            {
                knockbackTimer += Time.deltaTime;
                float t = knockbackTimer / dmg.knockbackDuration;
                transform.position = ogPos - knockbackDir * (dmg.knockbackDist * t);
                if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit ground))
                    transform.position = ground.point + Vector3.up * transform.lossyScale.y/2;
            }
        }

        //Methods
        public void SetHitFreezeTime(float time) => hitFrameTime = time;
        public void GetHitted(DamageData dmg, Transform hitter = null)
        {
            if (!dmg.ignoresInvulnerability)
                if (timer > 0)
                    return;

            // If parry returns true, we skip the damage logic completely
            if (parry && parry.TryParry(dmg, hitter, out var parryResult))
                if (parryResult is Combat.ParryState.Perfect or Combat.ParryState.Soft)
                    return;
            
            this.dmg = dmg;
            this.hitter = hitter;

            if (useDefaultKnockback)
            {
                knockbackTimer = 0;
                ogPos = transform.position;
                knockbackDir = hitter.position - transform.position;
                knockbackDir.y = 0;
                knockbackDir.Normalize();
            }
            
            timer = invulnerableTime;
            
            onHitted.Invoke();
        }
        public void GetHitted(DamageData dmg, Hitter hitter)
        {
            attackOwner = hitter;
            
            GetHitted(dmg, hitter.transform);
        }
        public void GetHitted(DamageData dmg, HitterData hitterData)
        {
            attackOwner = hitterData.source;
            
            GetHitted(dmg, hitterData.transform);
        }
        public void GetHitted(DamageData dmg, Hitter hitter, Collision coll)
        {
            this.coll = coll; //Set collision
            hit = default; //Clear hit
            
            GetHitted(dmg, hitter);
        }
        public void GetHitted(DamageData dmg, Transform hitter, RaycastHit hit)
        {
            this.hit = hit; //Set hit
            coll = null; //Clear collision
            
            GetHitted(dmg, hitter);
        }
        public float GetHitFreezeTime() => hitFrameTime;
        public GameObject GetCustomHitVFX() => willBlock ? blockVFX : null;
        //public FMODUnity.EventReference GetCustomHitSFX() => willBlock ? blockSFX : default;
    }
}