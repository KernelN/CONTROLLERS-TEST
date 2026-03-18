using UnityEngine;
using UnityEngine.InputSystem;

namespace Universal.InputControllers.Rumble
{
    public enum RumbleType { Small, Mid, Big, Custom }
    
    [System.Serializable]
    public struct RumbleValues
    {
        [Tooltip("This can be overrided in the method call")]
        [Min(0)] public float duration;
        [Range(0,1)] public float lowFreq;
        [Range(0,1)] public float highFreq;
    }
    
    [System.Serializable]
    public class GamepadRumble
    {
        [SerializeField, Min(0)] float delay;
        [SerializeField] RumbleType type;
        [SerializeField] RumbleValues customRumble;
        
        public void Rumble()
        {
            GamepadRumbler rumbler = GamepadRumbler.inst;
            if(rumbler == null) return;
            
            if(type == RumbleType.Custom)
                rumbler.Rumble(customRumble, delay);
            else
                rumbler.Rumble(type, delay);
        }
    }
    
    public class GamepadRumbler : Singleton<GamepadRumbler>
    {
        [Header("Set Values")]
        [SerializeField] RumbleValues smallRumble;
        [SerializeField] RumbleValues midRumble;
        [SerializeField] RumbleValues bigRumble;
        //[Header("Runtime Values")]
        float delay;
        float timer;
        bool usingGamepad;

        //Unity Events
        void Start()
        {
            InputSystem.onDeviceChange += OnDeviceChange;
            usingGamepad = Gamepad.current != null;
        }
        void Update()
        {
            if(delay > 0)
            {
                delay -= Time.deltaTime;
                if(delay > 0) return;
            }
            
            if(timer <= 0) return;
            timer -= Time.deltaTime;
            
            if(timer > 0) return;
            StopRumble();
        }
        public void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            usingGamepad = Gamepad.current != null;
        }

        //Methods
        public void Rumble(RumbleType type, float delay = 0, float durationOverride = -1f)
        {
            if(!usingGamepad) return;
            
            float lowFreq = 0;
            float highFreq = 0;
            float time = 0;
            
            switch (type)
            {
                case RumbleType.Small:
                    lowFreq = smallRumble.lowFreq;
                    highFreq = smallRumble.highFreq;
                    time = smallRumble.duration;
                    break;
                case RumbleType.Mid:
                    lowFreq = midRumble.lowFreq;
                    highFreq = midRumble.highFreq;
                    time = midRumble.duration;
                    break;
                case RumbleType.Big:
                    lowFreq = bigRumble.lowFreq;
                    highFreq = bigRumble.highFreq;
                    time = bigRumble.duration;
                    break;
            }
            
            Gamepad.current.SetMotorSpeeds(lowFreq, highFreq);
            timer = durationOverride < 0 ? time : durationOverride; //If no duration override, use time
            this.delay = delay;
        }
        public void Rumble(RumbleValues customValues, float delay = 0)
        {
            if(!usingGamepad) return;
            
            Gamepad.current.SetMotorSpeeds(customValues.lowFreq, customValues.highFreq);
            timer = customValues.duration;
            this.delay = delay;
        }
        public void StopRumble()
        {
            if(!usingGamepad) return;
            
            Gamepad.current.SetMotorSpeeds(0, 0);
        }
    }
}
