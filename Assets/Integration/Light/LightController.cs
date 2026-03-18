using UnityEngine;
using UnityEngine.Serialization;

namespace Flame.Gameplay.Lights
{
    public enum LightType { Point, Spot }

    [RequireComponent(typeof(Light))]
    public abstract class LightController : MonoBehaviour
    {
        protected LightManager manager;
        Vector3 lastPos;
        float oldRange;
        [FormerlySerializedAs("vanillaLight"),SerializeField] protected new Light light;

        public abstract LightType Type { get; }

        public float Range => light.range;
        public Vector3 Position => transform.position;

        public System.Action<LightController, float> UpdatedRange;
        public System.Action<LightController, Vector3> UpdatedPosition;

        protected virtual void Awake()
        {
            if(!light) light = GetComponent<Light>();
        }
        protected virtual void Start()
        {
            manager = LightManager.inst;
            if (!manager)
            {
                Debug.LogError($"{name} didn't find a CustomLightManager in the scene.");
                return;
            }

            manager.AddLight(this);
        }
        protected virtual void OnEnable()
        {
            manager?.AddLight(this);
            if(light)
                light.enabled = this;
        }
        protected virtual void OnDisable()
        {
            manager?.RemoveLight(this);
            if(light)
                light.enabled = this;
        }
        protected virtual void LateUpdate()
        {
            if ((lastPos - transform.position).sqrMagnitude > 0.0001f)
            {
                lastPos = transform.position;
                UpdatedPosition?.Invoke(this, lastPos);
            }
        }
        protected virtual void OnValidate()
        {
            if (oldRange - light.range > 0.0001f)
            {
                oldRange = light.range;
                UpdatedRange?.Invoke(this, oldRange);
            }
        }
        public void SetRange(float newRange)
        {
            light.range = newRange;
            UpdatedRange?.Invoke(this, newRange);
        }
        public void SetIntensity(float newIntensity) => light.intensity = newIntensity;
    }
}