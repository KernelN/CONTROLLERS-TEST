using UnityEngine;

namespace Flame.Gameplay.Lights
{
    //CLEAR ABSTRACT FOR FINAL SYSTEM (it will be component based, not inheritance based)
    public abstract class LightReactor : MonoBehaviour
    {
        public bool isLightReversible = true;
        internal float lightLevel;
        LightManager manager;

        public System.Action<float> LevelChanged;
        
        internal virtual void Start()
        {
            manager = LightManager.inst;
            Universal.UpdateManager updateManager = Universal.UpdateManager.inst;
            updateManager?.SuscribeToScaled(UpdateRate, _Update);
        }
        void OnDestroy()
        {
            Universal.UpdateManager updateManager = Universal.UpdateManager.inst;
            updateManager?.RemoveFromScaled(UpdateRate, _Update);
        }
        const int UpdateRate = 4;
        void _Update()
        {
            if(!isLightReversible && lightLevel >= 1) return;
            
            float newLightLevel = manager.GetLightLevel(transform.position);
            
            if(!isLightReversible && newLightLevel < lightLevel) return;
            
            if(Mathf.Approximately(newLightLevel, lightLevel)) return;

            float diff = newLightLevel - lightLevel;
            lightLevel = newLightLevel;
            LevelChanged?.Invoke(diff);
        }
    }
}