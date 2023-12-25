﻿namespace RobDriver.SkillStates.Driver.RocketLauncher
{
    public class NerfedBarrage : Barrage
    {
        protected override float _damageCoefficient => 2.5f;
        protected override float maxSpread => 8f;
        protected override int baseRocketCount => 4;
        //protected override float ammoMod => 4f;
    }
}