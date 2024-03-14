﻿using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using EntityStates;

namespace RobDriver.SkillStates.Driver
{
    public class UseSyringe : BaseDriverSkillState
    {
        public float baseDuration = 1.2f;

        protected override string prop => "SyringeModel";
        private bool farded;
        private float duration;
        protected Transform modelTransform;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / this.attackSpeedStat;
            this.modelTransform = this.GetModelTransform();

            base.PlayAnimation("Gesture, Override", "UseSyringe", "Action.playbackRate", this.duration);
            Util.PlaySound("sfx_driver_foley_syringe", this.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!this.farded)
            {
                if (base.fixedAge >= (0.5f * this.duration))
                {
                    this.farded = true;

                    Util.PlaySound("sfx_driver_injection", this.gameObject);
                    Util.PlaySound("sfx_driver_syringe_buff", this.gameObject);

                    if (NetworkServer.active)
                    {
                        this.ApplyBuff();
                    }
                }
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        protected virtual void ApplyBuff()
        {
            switch (this.iDrive.weaponDef.buffType)
            {
                case DriverWeaponDef.BuffType.Damage:
                    this.characterBody.AddTimedBuff(Modules.Buffs.syringeDamageBuff, 6f);
                    EffectManager.SpawnEffect(Modules.Assets.damageBuffEffectPrefab, new EffectData
                    {
                        origin = this.FindModelChild("PistolMuzzle").position,
                        rotation = Quaternion.identity
                    }, true);
                    EffectManager.SpawnEffect(Modules.Assets.damageBuffEffectPrefab2, new EffectData
                    {
                        origin = this.transform.position + new Vector3(0f, 0.5f, 0f),
                        rotation = Quaternion.identity,
                        rootObject = this.gameObject
                    }, true);
                    if (this.modelTransform)
                    {
                        TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                        temporaryOverlay.duration = 12f;
                        temporaryOverlay.animateShaderAlpha = true;
                        temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                        temporaryOverlay.destroyComponentOnEnd = true;
                        temporaryOverlay.originalMaterial = Modules.Assets.syringeDamageOverlayMat;
                        temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
                    }
                    break;
                case DriverWeaponDef.BuffType.AttackSpeed:
                    this.characterBody.AddTimedBuff(Modules.Buffs.syringeAttackSpeedBuff, 6f);
                    EffectManager.SpawnEffect(Modules.Assets.attackSpeedBuffEffectPrefab, new EffectData
                    {
                        origin = this.FindModelChild("PistolMuzzle").position,
                        rotation = Quaternion.identity
                    }, true);
                    EffectManager.SpawnEffect(Modules.Assets.attackSpeedBuffEffectPrefab2, new EffectData
                    {
                        origin = this.transform.position + new Vector3(0f, 0.5f, 0f),
                        rotation = Quaternion.identity,
                        rootObject = this.gameObject
                    }, true);
                    if (this.modelTransform)
                    {
                        TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                        temporaryOverlay.duration = 12f;
                        temporaryOverlay.animateShaderAlpha = true;
                        temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                        temporaryOverlay.destroyComponentOnEnd = true;
                        temporaryOverlay.originalMaterial = Modules.Assets.syringeAttackSpeedOverlayMat;
                        temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
                    }
                    break;
                case DriverWeaponDef.BuffType.Crit:
                    this.characterBody.AddTimedBuff(Modules.Buffs.syringeCritBuff, 6f);
                    EffectManager.SpawnEffect(Modules.Assets.critBuffEffectPrefab, new EffectData
                    {
                        origin = this.FindModelChild("PistolMuzzle").position,
                        rotation = Quaternion.identity
                    }, true);
                    EffectManager.SpawnEffect(Modules.Assets.critBuffEffectPrefab2, new EffectData
                    {
                        origin = this.transform.position + new Vector3(0f, 0.5f, 0f),
                        rotation = Quaternion.identity,
                        rootObject = this.gameObject
                    }, true);
                    if (this.modelTransform)
                    {
                        TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                        temporaryOverlay.duration = 12f;
                        temporaryOverlay.animateShaderAlpha = true;
                        temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                        temporaryOverlay.destroyComponentOnEnd = true;
                        temporaryOverlay.originalMaterial = Modules.Assets.syringeCritOverlayMat;
                        temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
                    }
                    break;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}