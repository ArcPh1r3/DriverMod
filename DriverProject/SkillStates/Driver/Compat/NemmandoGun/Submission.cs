﻿using RoR2;
using UnityEngine;
using EntityStates;
using RobDriver.Modules.Components;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;

namespace RobDriver.SkillStates.Driver.Compat.NemmandoGun
{
    public class Submission : BaseDriverSkillState
    {
        public static float damageCoefficient = 1.1f;
        public static int bulletCount = 6;
        public static float procCoefficient = 1f;
        public float baseShotDuration = 0.05f;
        public static float recoil = 11f;

        private bool finishing;

        protected virtual int baseShotCount
        {
            get
            {
                return 7;
            }
        }

        protected virtual float maxSpread
        {
            get
            {
                return 6f;
            }
        }

        protected virtual GameObject tracerPrefab
        {
            get
            {
                return Modules.Assets.nemmandoTracer;
            }
        }

        private int remainingShots;
        private float shotTimer;
        private float shotDuration;
        protected string muzzleString;
        private uint spinPlayID;
        private GameObject spinEffectInstance;

        public override void OnEnter()
        {
            base.OnEnter();
            this.characterBody.SetAimTimer(5f);
            this.muzzleString = "PistolMuzzle";
            this.shotDuration = this.baseShotDuration / this.attackSpeedStat;
            this.remainingShots = Mathf.Clamp(Mathf.RoundToInt(this.baseShotCount * this.attackSpeedStat), this.baseShotCount, 40);

            this.shotTimer = this.shotDuration;
            this.remainingShots--;
            this.Fire();
        }

        protected virtual float _damageCoefficient
        {
            get
            {
                return Submission.damageCoefficient;
            }
        }

        public virtual void Fire()
        {
            if (this.iDrive) this.iDrive.StartTimer(3f / this.baseShotCount);

            base.PlayAnimation("Gesture, Override", "ShootSubmission", "Shoot.playbackRate", 1.4f / this.attackSpeedStat);

            if (DriverPlugin.starstormInstalled) Util.PlaySound("NemmandoSubmissionFire", base.gameObject);
            else Util.PlaySound("sfx_driver_rocket_launcher_shoot", base.gameObject);

            float recoilAmplitude = Submission.recoil / this.attackSpeedStat;

            base.AddRecoil(-0.4f * recoilAmplitude, -0.8f * recoilAmplitude, -0.3f * recoilAmplitude, 0.3f * recoilAmplitude);
            this.characterBody.AddSpreadBloom(2f);
            EffectManager.SimpleMuzzleFlash(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/MuzzleflashSmokeRing.prefab").WaitForCompletion(), this.gameObject, this.muzzleString, false);

            if (base.isAuthority)
            {
                float damage = Shoot.damageCoefficient * this.damageStat;

                Ray aimRay = GetAimRay();

                float spread = this.maxSpread;
                float thiccness = 1f;
                float force = 50;

                BulletAttack bulletAttack = new BulletAttack
                {
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    damage = damage,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Stun1s,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    maxDistance = 150f,
                    force = force,// RiotShotgun.bulletForce,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    isCrit = this.RollCrit(),
                    owner = this.gameObject,
                    muzzleName = this.muzzleString,
                    smartCollision = true,
                    procChainMask = default,
                    procCoefficient = procCoefficient,
                    radius = thiccness,
                    sniper = false,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    weapon = null,
                    tracerEffectPrefab = this.tracerPrefab,
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FireBarrage.hitEffectPrefab,
                    HitEffectNormal = false,
                };

                bulletAttack.minSpread = 0;
                bulletAttack.maxSpread = 0;
                bulletAttack.bulletCount = 1;
                bulletAttack.Fire();

                uint secondShot = (uint)Mathf.CeilToInt(bulletCount / 2f) - 1;
                bulletAttack.minSpread = 0;
                bulletAttack.maxSpread = spread / 1.45f;
                bulletAttack.bulletCount = secondShot;
                bulletAttack.Fire();

                bulletAttack.minSpread = spread / 1.45f;
                bulletAttack.maxSpread = spread;
                bulletAttack.bulletCount = (uint)Mathf.FloorToInt(bulletCount / 2f);
                bulletAttack.Fire();

                //this.characterMotor.ApplyForce(aimRay.direction * -this.selfForce);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.shotTimer -= Time.fixedDeltaTime;

            if (this.shotTimer <= 0f)
            {
                if (this.remainingShots > 0)
                {
                    this.shotTimer = this.shotDuration;
                    this.remainingShots--;
                    this.Fire();
                }
                else
                {
                    if (!this.finishing)
                    {
                        this.finishing = true;
                        this.shotTimer = 1.4f / this.attackSpeedStat;
                        return;
                    }

                    if (base.isAuthority)
                    {
                        this.outer.SetNextStateToMain();
                    }
                }
            }

            if (this.finishing)
            {
                if (!this.spinEffectInstance && this.shotTimer <= (0.5f * (1.4f / this.attackSpeedStat)))
                {
                    this.CreateSpinEffect();
                }

                if (this.shotTimer <= (0.125f * (1.4f / this.attackSpeedStat)))
                {
                    if (this.spinEffectInstance)
                    {
                        EntityState.Destroy(this.spinEffectInstance);
                        AkSoundEngine.StopPlayingID(this.spinPlayID);
                    }
                }
            }

            if (this.iDrive && this.iDrive.weaponDef != this.cachedWeaponDef)
            {
                base.PlayAnimation("Gesture, Override", this.iDrive.weaponDef.equipAnimationString);
                this.outer.SetNextStateToMain();
                return;
            }
        }

        private void CreateSpinEffect()
        {
            this.spinEffectInstance = GameObject.Instantiate(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoReloadFX.prefab").WaitForCompletion());
            this.spinEffectInstance.transform.parent = this.FindModelChild("Pistol");
            this.spinEffectInstance.transform.localRotation = Quaternion.Euler(new Vector3(0f, 80f, 0f));
            this.spinEffectInstance.transform.localPosition = Vector3.zero;

            this.spinPlayID = Util.PlaySound("sfx_driver_pistol_spin", this.gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();

            if (this.spinEffectInstance)
            {
                EntityState.Destroy(this.spinEffectInstance);
                AkSoundEngine.StopPlayingID(this.spinPlayID);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}