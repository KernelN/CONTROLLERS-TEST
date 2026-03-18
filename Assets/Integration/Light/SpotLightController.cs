using UnityEngine;
using System;

namespace Flame.Gameplay.Lights
{
    public class SpotLightController : LightController
    {
        public override LightType Type => LightType.Spot;

        float oldRadius;
        float oldOuterRadius;
        
        Vector3 lastDir;

        public float CutOff => CosInnerRadius - CosOuterRadius;
        float CosInnerRadius => Mathf.Cos(light.innerSpotAngle * Mathf.Deg2Rad);
        public float CosOuterRadius => Mathf.Cos(light.spotAngle * Mathf.Deg2Rad);
        public float OuterRadius => light.spotAngle;

        public event Action<SpotLightController, Vector3> UpdatedDirection;
        public event Action<SpotLightController, float> UpdatedCutOff;
        public event Action<SpotLightController, float> UpdatedOuterRadius;

        protected override void LateUpdate()
        {
            base.LateUpdate();
            if ((lastDir - transform.forward).sqrMagnitude > 0.0001f)
            {
                lastDir = transform.forward;
                UpdatedDirection?.Invoke(this, lastDir);
            }
        }
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!Mathf.Approximately(oldRadius, light.innerSpotAngle))
            {
                oldRadius = light.innerSpotAngle;
                UpdatedCutOff?.Invoke(this, CutOff);
            }

            if (!Mathf.Approximately(oldOuterRadius, light.spotAngle))
            {
                oldOuterRadius = light.spotAngle;
                UpdatedCutOff?.Invoke(this, CutOff);
                UpdatedOuterRadius?.Invoke(this, CosOuterRadius);
            }
        }
    }
}