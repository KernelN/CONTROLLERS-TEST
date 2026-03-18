using UnityEngine;

namespace Universal.Gameplay
{
    [System.Serializable]
    public class SpeedLinesController
    {
        enum States { Idle, Activating, /*Active,*/ Deactivating, _Count}
        
        [Header("Set Values")]
        [SerializeField] Material mat;
        [SerializeField] float offRemap;
        [SerializeField] float onRemap;
        [SerializeField] float turnOnLerpLength;
        [SerializeField] float turnOffLerpLength;
        [SerializeField] Color color;
        //[Header("Runtime Values")]
        States state;
        float timer;
        
        static readonly int SpeedLinesRemap = Shader.PropertyToID("_SpeedLinesRemap");
        static readonly int SpeedLinesColour = Shader.PropertyToID("_Colour");

        //Unity Events
        public void Update(float dt)
        {
            if(state == States.Idle) return;
            if(dt == 0) return;

            if (timer > 0)
            {
                timer -= dt;

                if (timer <= 0)
                {
                    if (state == States.Activating)
                    {
                        state = States.Idle;
                        //state = States.Active;
                        mat.SetFloat(SpeedLinesRemap, onRemap);
                    }
                    else if (state == States.Deactivating)
                    {
                        state = States.Idle;
                        mat.SetFloat(SpeedLinesRemap, offRemap);
                    }
                    
                    return;
                }
            }

            float t = 1;
            float val = 0;

            if (state == States.Activating)
            {
                t -= timer / turnOnLerpLength;
                t = Mathf.Clamp01(t);

                val = Mathf.SmoothStep(offRemap, onRemap, t);
                val = Mathf.Clamp(val, offRemap, onRemap);
            }
            else if (state == States.Deactivating)
            {
                t -= timer / turnOffLerpLength;
                
                val = Mathf.SmoothStep(onRemap, offRemap, t);
                val = Mathf.Clamp(val, onRemap, offRemap);
            }
            
            mat.SetFloat(SpeedLinesRemap, val);
        }

        //Methods
        public void StartFX()
        {
            state = States.Activating;
            timer = turnOnLerpLength;
            
            mat.SetColor(SpeedLinesColour, color);
            mat.SetFloat(SpeedLinesRemap, offRemap);
        }
        public void StopFX(bool forceStop = false)
        {
            if (forceStop)
            {
                state = States.Idle;
                timer = -1;
                
                mat.SetFloat(SpeedLinesRemap, offRemap);
                return;
            }
            
            state = States.Deactivating;
            timer = turnOffLerpLength;
        }
    }
}
