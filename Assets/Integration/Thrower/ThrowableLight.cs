using System;
using Flame.Gameplay.Lights;
using UnityEngine;
using UnityEngine.Serialization;

namespace Flame.Gameplay.Player.Throw
{
    public class ThrowableLight : MonoBehaviour
    {
        static readonly int LightedUpOverrideColor = Shader.PropertyToID("_LightedUpOverrideColor");
        [SerializeField] Rigidbody rb;
        [SerializeField] new LightController light;
        [SerializeField] Renderer model;
        LightManager lightManager;
        
        [Header("Collision")]
        [SerializeField] float forwCollRayLength = .3f;
        [SerializeField] float downCollRayLength = .1f;
        [SerializeField] LayerMask collRayLayers;

        enum SparkOnThresholdType { PathDistance, AbsoluteDistance }
        [Header("Spark On Threshold")]
        [SerializeField] SparkOnThresholdType sparkOnThresholdType = SparkOnThresholdType.PathDistance;
        [SerializeField] float minDistToSpark = 20;
        [SerializeField] float sparkOffSize;
        [SerializeField] float sparkChargedSize;
        [SerializeField] AnimationCurve sparkChargeSizeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] float sparkOnSize;
        [SerializeField] Color sparkOffColor = Color.black;
        [SerializeField] Color sparkChargedColor = Color.yellowNice;
        [SerializeField] AnimationCurve sparkChargeColorCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] Color sparkOnColor = Color.coral;
        [SerializeField] string sparkColorCode = "_BaseColor";
        [SerializeField] ParticleSystem sparkOnEffect;
        Vector3 lastPos;
        float cDistToSpark;
        bool hasSparked;
        bool canSpark = true;
        
        [Header("Light")]
        [SerializeField] float maxLightRange = 10;
        [SerializeField] float maxIntensity = 1;
        [SerializeField] float lightTurnOnLength = 1;
        [SerializeField] AnimationCurve lightOnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] float lightDuration = 10;
        [SerializeField] AnimationCurve lightLifeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        float lifetime;
        float timer;
        bool isTurningOn;

        void Start() => lightManager = LightManager.inst;
        void OnEnable()
        {
            //Connect Update
            Universal.UpdateManager um = Universal.UpdateManager.inst;
            um.SuscribeToScaled(updateRate, SlowUpdate);
            
            //Reset Rigidbody
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            //Reset light
            light.enabled = false;
            light.SetRange(0);
            
            //Reset turn on
            cDistToSpark = 0;
            lastPos = transform.position;
            hasSparked = false;
            canSpark = true;
            for (int i = 0; i < model.materials.Length; i++) 
                model.materials[i].SetColor(sparkColorCode, sparkOffColor);
            model.transform.localScale = sparkOffSize * model.transform.localScale.normalized;
            if(sparkOnEffect)
                sparkOnEffect.transform.parent = transform;
        }
        void OnDisable()
        {
            //Connect Update
            Universal.UpdateManager um = Universal.UpdateManager.inst;
            um?.RemoveFromScaled(updateRate, SlowUpdate);
        }
        void OnCollisionEnter(Collision other)
        {
            canSpark = false;
            if (!hasSparked)
            {
                //gameObject.SetActive(false);
                return;
            }
            
            rb.isKinematic = true;
            light.enabled = true;
            timer = 0;
            isTurningOn = true;
            lifetime = lightDuration * lightManager.GetLightLifeMod(transform.position);
        }
        const int updateRate = 3;
        void SlowUpdate()
        {
            if (!hasSparked && canSpark)
            {
                if (sparkOnThresholdType == SparkOnThresholdType.PathDistance)
                {
                    cDistToSpark += (transform.position - lastPos).magnitude;
                    lastPos = transform.position;
                }
                else
                    cDistToSpark = (transform.position - lastPos).magnitude;
                
                hasSparked = cDistToSpark >= minDistToSpark;

                Color color;
                float size;
                if (hasSparked)
                {
                    color = sparkOnColor;
                    size = sparkOnSize;
                    if (sparkOnEffect)
                    {
                        sparkOnEffect.transform.position = transform.position;
                        sparkOnEffect.transform.parent = null;
                        sparkOnEffect?.Play();
                    }
                }
                else
                {
                    float t = cDistToSpark / minDistToSpark;
                    color = Color.Lerp(sparkOffColor, sparkChargedColor,
                                        sparkChargeColorCurve.Evaluate(t));
                    size = Mathf.Lerp(sparkOffSize, sparkChargedSize,
                                        sparkChargeSizeCurve.Evaluate(t));
                }

                for (int i = 0; i < model.materials.Length; i++) 
                    model.materials[i].SetColor(sparkColorCode, color);
                model.transform.localScale = size * model.transform.localScale.normalized;
            }
            
            if(light.enabled) return;
            
            Vector3 pos = transform.position;
            if(!Physics.Raycast(pos, rb.linearVelocity.normalized, forwCollRayLength, collRayLayers))
                if(!Physics.Raycast(pos, Vector3.down, downCollRayLength, collRayLayers)) return;
            
            rb.isKinematic = true;
            light.enabled = true;
            timer = 0;
        }
        void Update()
        {
            //Only update for light lifetime
            if(!light.enabled) return;
            
            timer += Time.deltaTime;

            float t;
            if (isTurningOn)
            {
                if (timer >= lightTurnOnLength)
                {
                    t = 1;
                    isTurningOn = false;
                }
                else t = lightOnCurve.Evaluate(timer / lightTurnOnLength);
            }
            else
            {
                if (timer >= lightDuration)
                {
                    t = 1;
                    gameObject.SetActive(false);
                }
                else t = lightLifeCurve.Evaluate(timer / lightDuration);
            }
            light.SetRange(t * maxLightRange);
            light.SetIntensity(t * maxIntensity);
        }
    }
}
