using System.Collections.Generic;
using MalbersAnimations;
using MalbersAnimations.Weapons;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Flame.Gameplay.Player.Throw
{
    [System.Serializable]
    public class ShootableWithPool : MShootable
    {
        [SerializeField] List<ShootablePoolCaller> projectilePool;
        [SerializeField] bool recycleLastActiveOnDemand;
        List<ShootablePoolCaller> activePool;
        List<ShootablePoolCaller> inactivePool;
        
        [SerializeField] float maxRecallDuration = 10f;
        float recallTimer = 0;

        public void Start()
        {
            activePool = new List<ShootablePoolCaller>();
            inactivePool = new List<ShootablePoolCaller>(projectilePool);

            for (int i = 0; i < projectilePool.Count; i++) 
                projectilePool[i].OnEnabled += OnProjEnabled;
        }
        
        public override void EquipProjectile()
        {
            if (!HasAmmo) return;                                           //means there's no Ammo so no equipping!

            if (ProjectileInstance == null) //Means there's no projectile equipped!
            {
                var Pos = ProjectileParent ? ProjectileParent.position : AimOriginPos;
                var Rot = ProjectileParent ? ProjectileParent.rotation : AimOrigin.rotation;

                ShootablePoolCaller proj = CreateThrowable(Pos, Rot);
                ProjectileInstance = proj.gameObject;

                if (proj.MProj)
                {
                    MProjectile = proj.MProj; //Safe in a variable

                    ProjectileInstance.transform.Translate(MProjectile.PosOffset, Space.Self);   //Translate in the offset of the arrow to put it on the hand
                    ProjectileInstance.transform.Rotate(MProjectile.RotOffset, Space.Self);      //Rotate in the offset of the arrow to put it on the hand
                    //ProjectileInstance.transform.localScale = (projectile.ScaleOffset);       //Scale in the offset of the arrow to put it on the hand


                    //Use Weapon Effects on the projectiles
                    if (MProjectile.hitEffects == null || MProjectile.hitEffects.Count == 0)
                    { MProjectile.hitEffects = hitEffects; }
                    if (MProjectile.HitEffect == null)
                    { MProjectile.HitEffect = HitEffect; }
                    if (MProjectile.hitSound == null || MProjectile.hitSound.Value == null)
                    { MProjectile.hitSound = hitSound; }
                }

                if (proj.Body)
                {
                    proj.Body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    proj.Body.isKinematic = true;
                }


                //Disable projectile collider
                if (proj.Coll) proj.Coll.enabled = false;



                OnLoadProjectile.Invoke(ProjectileInstance);

                // ProjectIsReleased = false;

                Debugging($"◘ [Projectile Equiped] [{ProjectileInstance.name}] ", ProjectileInstance);
            }
            else
            {
                Debugging($"◘ [Projectile Already Equipped] Skip", ProjectileInstance, "gray");
            }
        }

        public override bool HasAmmo => recycleLastActiveOnDemand || inactivePool.Count > 0;
        internal ShootablePoolCaller CreateThrowable(Vector3 pos, Quaternion rot)
        {
            ShootablePoolCaller newProj = null;
            if(inactivePool.Count > 0) newProj = inactivePool[0];
            else newProj = activePool[0]; //assumes recycle is turned on, otherwise this moment would be unreachable
            
            newProj.gameObject.SetActive(true);
            return newProj;
        }
        public void RecallThrowables()
        {
            for (int i = 0; i < activePool.Count; i++) 
                activePool[i].Recall(transform);
            
            // if(recallTimer == 0)
            //     UpdateManager.inst?.SuscribeToScaled(UpdateRecallInterval, UpdateRecall);
        }
        const float UpdateRecallInterval = 0.1f;
        void UpdateRecall()
        {
            recallTimer += UpdateRecallInterval;
            if(recallTimer >= maxRecallDuration) EndRecall();
        }
        void EndRecall()
        {
            for (int i = 0; i < activePool.Count; i++) 
                activePool[i].EndRecall();
            
            recallTimer = 0;
            //UpdateManager.inst?.RemoveFromScaled(UpdateRecallInterval, UpdateRecall);
        }
        void OnProjEnabled(ShootablePoolCaller obj, bool isEnabled)
        {
            if (isEnabled)
            {
                inactivePool.Remove(obj);
                activePool.Add(obj);
            }
            else
            {
                activePool.Remove(obj);
                inactivePool.Add(obj);

                if (activePool.Count == 0) EndRecall();
            }
        }
    }
    
            #region Inspector

#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(ShootableWithPool))]
    public class ShootableWithPoolEditor : MShootableEditor
    {
        SerializedProperty
            ProjectilePool, RecycleLastActiveOnDemand;

        ShootableWithPool shootWithPool;

        private void OnEnable()
        {
            shootWithPool = (ShootableWithPool)target;
            mShoot = (ShootableWithPool)target;

            SetOnEnable();
            Tabs2 = new string[] { "Bow", "Shootable", "Sounds", "Events" };

            ProjectilePool = serializedObject.FindProperty("projectilePool");
            RecycleLastActiveOnDemand = serializedObject.FindProperty("recycleLastActiveOnDemand");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Pool Weapons Properties");
            WeaponInspector(false);
            serializedObject.ApplyModifiedProperties();
        }


        protected override void WeaponInspector(bool showAim = true)
        {
            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs1);
            if (Editor_Tabs1.intValue != Tabs1.Length) Editor_Tabs2.intValue = Tabs2.Length;

            Editor_Tabs2.intValue = GUILayout.Toolbar(Editor_Tabs2.intValue, Tabs2);
            if (Editor_Tabs2.intValue != Tabs2.Length) Editor_Tabs1.intValue = Tabs1.Length;


            //First Tabs
            int Selection = Editor_Tabs1.intValue;
            if (Selection == 0) DrawWeapon(showAim);
            else if (Selection == 1) { DrawDamage(); DrawStatModifiers(); }
            else if (Selection == 2) DrawIK();
            else if (Selection == 3) DrawExtras();


            //2nd Tabs
            Selection = Editor_Tabs2.intValue;
            // if (Selection == 0) DrawBow();
            // else if (Selection == 1) DrawAdvancedWeapon();
            // else if (Selection == 2) DrawSound();
            // else if (Selection == 3) DrawEvents();
        }



        protected override string CustomEventsHelp()
        {
            return "\n\nOn Load Arrow: Invoked when the arrow is instantiated.\n (GameObject) the instance of the Arrow. \n\nOnHold: Invoked when the bow is being bent (0 to 1)\n\nOn Release Arrow: Invoked when the Arrow is released.\n (GameObject) the instance of the Arrow.";
        }
        Vector3 Axis(int Index)
        {
            return Index switch
            {
                0 => Vector3.right,
                1 => -Vector3.right,
                2 => Vector3.up,
                3 => -Vector3.up,
                4 => Vector3.forward,
                5 => -Vector3.forward,
                _ => Vector3.zero,
            };
        }
    }

#endif
    #endregion
}
