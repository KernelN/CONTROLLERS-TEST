using System;
using UnityEngine;

namespace Flame.Gameplay.Lights
{
    public class LightEnergySource : MonoBehaviour
    {
        [SerializeField] float range = 50;
        [SerializeField,Tooltip("0 as close and 1 as far")] AnimationCurve lifetimeByDist = AnimationCurve.EaseInOut(0,1,1,0);
        
        [Header("DEBUG")]
        [SerializeField] bool useGizmos;
        
        void Start()
        {
            LightManager.inst.SetEnergySource(this);
        }
        void OnDrawGizmos()
        {
            if(!useGizmos) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);
        }

        public float GetLifetimeModByDist(float distMagnitude)
        {
            float t = Mathf.Clamp01(distMagnitude / range);
            return lifetimeByDist.Evaluate(t);
        }
    }
}