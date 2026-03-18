using System.Collections.Generic;
using UnityEngine;

namespace Flame.Gameplay.Lights.Showcase
{
    [System.Serializable]
    class LightDiffThreshold
    {
        public enum ThresholdDirection { OnRaise, OnLow, Both }
        [Header("Threshold")]
        [SerializeField, Range(0,1)] float threshold;
        public ThresholdDirection thresholdTriggerDirection;
        [SerializeField] LightReactionShowcase effect;

        public void Set(Animator anim, GameObject go, LightReactor reactor) 
            => effect.Set(anim,go,reactor);
        public void OnRaised(float levelDiff)
        {
            if(levelDiff >= threshold)
                effect?.React();
        }
        public void OnLowered(float levelDiff)
        {
            if(levelDiff <= -threshold)
                effect?.React();
        }
    }
    [System.Serializable]
    class LightLevelThreshold
    {
        [SerializeField, Range(0,1)] float threshold;
        [SerializeField] LightReactionShowcase effect;

        public void Set(Animator anim, GameObject go, LightReactor reactor) 
            => effect.Set(anim,go,reactor);

        public void OnChanged(float newLevel)
        {
            if(newLevel >= threshold)
                effect?.React();
        }
    }

    [System.Serializable]
    class LightReactionShowcase
    {
        enum AnimParamType { Trigger, BoolOn, BoolOff }
        [Header("Animator")]
        [SerializeField] AnimParamType animatorParamType;
        [SerializeField] string animatorParameter = "";
        Animator animator;
        void AnimatorReact()
        {
            if(!animator || animatorParameter == "") return;
            
            switch (animatorParamType)
            {
                case AnimParamType.Trigger:
                    animator.SetTrigger(animatorParameter);
                    break;
                case AnimParamType.BoolOn:
                    animator.SetBool(animatorParameter, true);
                    break;
                case AnimParamType.BoolOff:
                    animator.SetBool(animatorParameter, false);
                    break;
            }
        }
        
        [Header("Destroy")]
        [SerializeField] bool destroy;
        [SerializeField] float destroyDelay = 0;
        [SerializeField, Tooltip("Optional")] GameObject[] nonDestroyables;
        GameObject destroyable;
        void DestroyReact()
        {
            if(!destroy) return;
            for (int i = 0; i < nonDestroyables.Length; i++)
                nonDestroyables[i].transform.parent = destroyable.transform.parent;
            GameObject.Destroy(destroyable, destroyDelay);
        }

        [Header("Instantiate")]
        [SerializeField] GameObject objToSpawn;
        [SerializeField] Transform transformRefOverride;
        [SerializeField] bool parentToThis;
        void SpawnReact()
        {
            if (!objToSpawn) return;
            Transform refObj = transformRefOverride ?
                transformRefOverride : destroyable.transform;

            if (parentToThis)
                Object.Instantiate(objToSpawn, refObj.position, refObj.rotation, destroyable.transform);
            else
                Object.Instantiate(objToSpawn, refObj.position, refObj.rotation);
        }
        
        enum SetIrreversibleType { Irreversible, Reversible }
        [Header("Fix Effect")]
        [SerializeField] bool changeReactorReversible;
        [SerializeField] SetIrreversibleType setTo;
        LightReactor reactor;
        void SetReversibleReact()
        {
            if(!changeReactorReversible) return;
            reactor.isLightReversible = setTo == SetIrreversibleType.Reversible;
        }
        
        [Header("Trigger Logic")]
        [SerializeField] UnityEngine.Events.UnityEvent logicToTrigger;

        public void Set(Animator anim, GameObject destroyGo, LightReactor reactor)
        {
            animator = anim;
            destroyable = destroyGo;
            this.reactor = reactor;
        }
        public void React()
        {
            AnimatorReact();
            DestroyReact();
            SpawnReact();
            SetReversibleReact();
            logicToTrigger?.Invoke();
        }
    }
    
    public class LightReactorShowcase : LightReactor
    {
        [SerializeField] Animator animator;
        [SerializeField] string lightLevelAnimParam = "LightLevel";
        [SerializeField] LightDiffThreshold[] onLightDifferenceThresholdTriggers;
        [SerializeField] LightLevelThreshold[] onLightLevelThresholdTriggers;
        List<LightDiffThreshold> onRaiseThresholds;
        List<LightDiffThreshold> onLowThresholds;
        
        void Awake()
        {
            onRaiseThresholds = new List<LightDiffThreshold>();
            onLowThresholds = new List<LightDiffThreshold>();
            
            for (int i = 0; i < onLightDifferenceThresholdTriggers.Length; i++)
            {
                onLightDifferenceThresholdTriggers[i].Set(animator, gameObject, this);
                switch (onLightDifferenceThresholdTriggers[i].thresholdTriggerDirection)
                {
                    case LightDiffThreshold.ThresholdDirection.OnRaise:
                        onRaiseThresholds.Add(onLightDifferenceThresholdTriggers[i]);
                        break;
                    case LightDiffThreshold.ThresholdDirection.OnLow:
                        onLowThresholds.Add(onLightDifferenceThresholdTriggers[i]);
                        break;
                    case LightDiffThreshold.ThresholdDirection.Both:
                        onRaiseThresholds.Add(onLightDifferenceThresholdTriggers[i]);
                        onLowThresholds.Add(onLightDifferenceThresholdTriggers[i]);
                        break;
                }
            }

            for (int i = 0; i < onLightLevelThresholdTriggers.Length; i++) 
                onLightLevelThresholdTriggers[i].Set(animator, gameObject, this);
            
            LevelChanged += OnLevelChanged;
        }
        void OnLevelChanged(float levelDiff)
        {
            if(levelDiff > 0)
                for (int i = 0; i < onRaiseThresholds.Count; i++)
                    onRaiseThresholds[i].OnRaised(levelDiff);
            else
                for (int i = 0; i < onLowThresholds.Count; i++)
                    onLowThresholds[i].OnLowered(levelDiff);

            for (int i = 0; i < onLightLevelThresholdTriggers.Length; i++) 
                onLightLevelThresholdTriggers[i].OnChanged(lightLevel);
            
            if(animator && lightLevelAnimParam != "")
                animator.SetFloat(lightLevelAnimParam, lightLevel);
        }
    }
}