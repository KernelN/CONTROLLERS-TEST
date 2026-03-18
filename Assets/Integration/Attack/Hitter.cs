using System;
using System.Collections.Generic;
using UnityEngine;

namespace Flame.Gameplay.Damage
{
    public class Hitter : MonoBehaviour
    {
        [SerializeField] DamageData damage;
        List<Hittable> hittedObjects = new List<Hittable>();

        void Awake()
        {
            hittedObjects = new List<Hittable>();
        }
        void OnEnable()
        {
            hittedObjects.Clear();
        }
        void OnDisable()
        {
            hittedObjects.Clear();
        }
        public void OnTriggerEnter(Collider other)
        {
            if(transform.IsChildOf(other.transform)) return;
            
            if(!other.gameObject.TryGetComponent(out Hittable hittable)) return;
            
            if(hittedObjects.Contains(hittable)) return;
            
            hittable.GetHitted(damage, this);
            hittedObjects.Add(hittable);
        }
    }
}