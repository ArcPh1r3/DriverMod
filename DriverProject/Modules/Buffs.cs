﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RobDriver.Modules
{
    public static class Buffs
    {
        internal static List<BuffDef> buffDefs = new List<BuffDef>();

        internal static BuffDef dazedDebuff;
        internal static BuffDef woundDebuff;
        internal static BuffDef syringeDamageBuff;
        internal static BuffDef syringeAttackSpeedBuff;
        internal static BuffDef syringeCritBuff;
        internal static BuffDef syringeScepterBuff;

        internal static void RegisterBuffs()
        {
            dazedDebuff = AddNewBuff("RobDriverDazedDebuff", Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Common/bdCloak.asset").WaitForCompletion().iconSprite, Color.grey, false, false);
            woundDebuff = AddNewBuff("RobDriverWoundDebuff", Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Bandit2/bdBanditSkull.asset").WaitForCompletion().iconSprite, Color.red, false, true);
            syringeDamageBuff = AddNewBuff("RobDriverSyringeDamageBuff", Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texBuffSyringe"), new Color(1f, 70f / 255f, 75f / 255f), false, false);
            syringeAttackSpeedBuff = AddNewBuff("RobDriverSyringeAttackSpeedBuff", Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texBuffSyringe"), new Color(1f, 170f / 255f, 45f / 255f), false, false);
            syringeCritBuff = AddNewBuff("RobDriverSyringeCritBuff", Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texBuffSyringe"), new Color(1f, 80f / 255f, 17f / 255f), false, false);
            syringeScepterBuff = AddNewBuff("RobDriverSyringeScepterBuff", Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texBuffSyringe"), Modules.Survivors.Driver.characterColor, false, false);
        }

        // simple helper method
        internal static BuffDef AddNewBuff(string buffName, Sprite buffIcon, Color buffColor, bool canStack, bool isDebuff)
        {
            BuffDef buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = buffName;
            buffDef.buffColor = buffColor;
            buffDef.canStack = canStack;
            buffDef.isDebuff = isDebuff;
            buffDef.eliteDef = null;
            buffDef.iconSprite = buffIcon;

            buffDefs.Add(buffDef);

            return buffDef;
        }
    }
}