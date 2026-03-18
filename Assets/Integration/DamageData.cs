using UnityEngine;

namespace Flame.Gameplay
{
    [System.Serializable]
    public class DamageData
    {
        //[Header("Set Values")]
        public float value;
        [Header("OPTIONALS")]
        public bool canStumble;
        public bool ignoresInvulnerability;
        [Tooltip("In case dmg has a special keyword")]
        public string keyword;
        public bool triggersConstDmgFlag;
        public bool triggersLightDmgFlag;
        public float knockbackDist;
        public float knockbackDuration;
        public Vector3 knockbackRot;
        
        //Runtime Values
        public int ID { get; private set; }
        
        public DamageData()
        {
            value = 0;
            canStumble = false;
            keyword = null;
            ignoresInvulnerability = false;
            ID = GetHashCode();
        }
        public DamageData(DamageData data)
        {
            value = data.value;
            canStumble = data.canStumble;
            keyword = data.keyword;
            triggersConstDmgFlag = data.triggersConstDmgFlag;
            triggersLightDmgFlag = data.triggersLightDmgFlag;
            ignoresInvulnerability = data.ignoresInvulnerability;
            knockbackDist = data.knockbackDist;
            knockbackDuration = data.knockbackDuration;
            knockbackRot = data.knockbackRot;
            ID = data.ID;
        }
    }
}