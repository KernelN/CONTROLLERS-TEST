using System.Collections.Generic;
using UnityEngine;

namespace Flame.Gameplay.Lights
{
    public class LightManager : Universal.Singleton<LightManager>
    {
        const int MaxLightCount = 10;
        
        List<LightController> lights;
        List<LightEnergySource> energySources;
        List<PointLightController> plights;
        List<SpotLightController> slights;
        float[] lightDistance;
        Vector4[] lightPositions;
        Vector4[] spotDirections;
        float[] spotcutOff;
        float[] spotCutOffs;

        internal override void Awake()
        {
            base.Awake();

            lights = new List<LightController>();
            plights = new List<PointLightController>();
            slights = new List<SpotLightController>();
            energySources = new List<LightEnergySource>();
            
            lightDistance = new float[MaxLightCount];
            lightPositions = new Vector4[MaxLightCount];
            
            spotDirections = new Vector4[MaxLightCount];
            spotcutOff = new float[MaxLightCount];
            spotCutOffs = new float[MaxLightCount];
        }
        
        //Manage Lights
        public void AddLight(LightController light)
        {
            if (lights.Contains(light)) return;
            int index = lights.Count;
            if (index >= MaxLightCount) return;
            
            lights.Add(light);
            lights.Sort(SortLights);

            //Ugly patch, can be optimized?
            for (int i = 0; i < lights.Count; i++)
            {
                lightDistance[i] = lights[i].Range;
                lightPositions[i] = lights[i].Position;
            }
            
            light.UpdatedRange += UpdateRange;
            light.UpdatedPosition += UpdatePos;

            switch (light.Type)
            {
                case LightType.Point:
                    plights.Add((PointLightController)light);
                    break;
                case LightType.Spot:
                    var spot = (SpotLightController)light;
                    index = slights.Count;
                    slights.Add(spot);
                    spotcutOff[index] = spot.CutOff;
                    spotCutOffs[index] = spot.OuterRadius;
                    spot.UpdatedDirection += UpdateSpotDirection;
                    spot.UpdatedCutOff += UpdateSpotCutOff;
                    spot.UpdatedOuterRadius += UpdateSpotOuterRadius;
                    break;
            }
        }
        public void RemoveLight(LightController light)
        {
            int index = lights.IndexOf(light);
            if(index < 0) return;
            
            lights.RemoveAt(index);
            
            //Update lights array
            for (int i = index; i < lights.Count; i++)
            {
                lightDistance[i] = lights[i].Range;
                lightPositions[i] = lights[i].Position;
            }
            
            light.UpdatedRange -= UpdateRange;
            light.UpdatedPosition -= UpdatePos;
            
            switch (light.Type)
            {
                case LightType.Point:
                    plights.Remove((PointLightController)light);
                    break;
                case LightType.Spot:
                    var spot = (SpotLightController)light;
                    slights.Remove(spot);
                    
                    for (int i = index; i < slights.Count; i++)
                    {
                        spotDirections[i] = slights[i].transform.forward;
                        spotcutOff[i] = slights[i].CutOff;
                        spotCutOffs[i] = slights[i].OuterRadius;
                    }
                    
                    spot.UpdatedDirection -= UpdateSpotDirection;
                    spot.UpdatedCutOff -= UpdateSpotCutOff;
                    spot.UpdatedOuterRadius -= UpdateSpotOuterRadius;
                    break;
            }
        }
        int SortLights(LightController a, LightController b) => a.Type.CompareTo(b.Type);
        public float GetLightLevel(Vector3 position)
        {
            Vector3 dist;
            float lightLevel = 0;
            for (int i = 0; i < lights.Count; i++)
            {
                dist = (position - lights[i].Position);
                float sqrRange = lights[i].Range * lights[i].Range; 
                if(dist.sqrMagnitude > sqrRange) continue;
                lightLevel += 1  - (dist.magnitude / lights[i].Range);
            }
            
            lightLevel = Mathf.Clamp01(lightLevel);

            return lightLevel;
        }

        //Receive Value Updates
        void UpdateRange(LightController light, float range)
        {
            int index = lights.IndexOf(light); //Polish this to optimize uses of IndexOf
            lightDistance[index] = range;
        }
        void UpdatePos(LightController light, Vector3 pos)
        {
            int index = lights.IndexOf(light); //Polish this to optimize uses of IndexOf
            lightPositions[index] = pos;
        }
        void UpdateSpotDirection(SpotLightController light, Vector3 dir)
        {
            int index = slights.IndexOf(light);
            spotDirections[index] = dir;
        }
        void UpdateSpotCutOff(SpotLightController light, float cutoff)
        {
            int i = slights.IndexOf(light);
            spotcutOff[i] = cutoff;
        }
        void UpdateSpotOuterRadius(SpotLightController light, float outerRadius)
        {
            int i = slights.IndexOf(light);
            spotCutOffs[i] = outerRadius;
        }

        //Energy Sources
        public void SetEnergySource(LightEnergySource lightEnergySource)
        {
            energySources.Add(lightEnergySource);
        }
        public float GetLightLifeMod(Vector3 position)
        {
            LightEnergySource closestEnergySource = null;
            Vector3 dist = Vector3.positiveInfinity;
            
            for (int i = 0; i < energySources.Count; i++)
            {
                Vector3 d = energySources[i].transform.position - position;
                if(d.sqrMagnitude > dist.sqrMagnitude) continue;
                dist = d;
                closestEnergySource = energySources[i];
            }

            if (closestEnergySource)
                return closestEnergySource.GetLifetimeModByDist(dist.magnitude);

            return 1;
        }
    }
}