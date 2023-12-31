﻿using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace RobDriver.SkillStates.Driver.LunarHammer
{
    public class FireShard : BaseSkillState
    {
        public static float damageCoefficient = 1.8f;
        public static float baseDuration = 0.1f;
        public static float recoilAmplitude = 0.8f;
        public static float spreadBloomValue = 1f;
        public static string muzzleString = "HandL";

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = FireShard.baseDuration / this.attackSpeedStat;

            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();

                FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                fireProjectileInfo.position = aimRay.origin;
                fireProjectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
                fireProjectileInfo.crit = base.characterBody.RollCrit();
                fireProjectileInfo.damage = base.characterBody.damage * FireShard.damageCoefficient;
                fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                fireProjectileInfo.owner = base.gameObject;
                fireProjectileInfo.procChainMask = default(ProcChainMask);
                fireProjectileInfo.force = 0f;
                fireProjectileInfo.useFuseOverride = false;
                fireProjectileInfo.useSpeedOverride = false;
                fireProjectileInfo.target = null;
                fireProjectileInfo.projectilePrefab = Modules.Projectiles.lunarShard;

                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }

            base.PlayAnimation("LeftArm, Override", "FireShard", "Shard.playbackRate", this.duration * 5f);

            float recoil = FireShard.recoilAmplitude / this.attackSpeedStat;
            base.AddRecoil(-0.4f * recoil, -0.8f * recoil, -0.3f * recoil, 0.3f * recoil);
            base.characterBody.AddSpreadBloom(FireShard.spreadBloomValue);

            EffectManager.SimpleMuzzleFlash(Modules.Assets.lunarShardMuzzleFlash, base.gameObject, "HandL", false);
            Util.PlaySound(EntityStates.BrotherMonster.Weapon.FireLunarShards.fireSound, base.gameObject);

            //base.skillLocator.secondary.rechargeStopwatch = 0f;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}