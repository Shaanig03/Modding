using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VanillaExpandedLoreFriendly
{
    public class Config
    {
        public string key_save = "F2";
        public string key_turretConfig = "T";
        public bool infiniteTurretAmmo = false;
        public bool turretConsumesPower = true;
        public float turretConsumePowerAmount = 0.2f;
        public float turretPowerSearchRadius = 250;
        public float portablePowerStation_powerGenDelay = 1f;
        public float portablePowerStation_powerGenUnits = 10;
        public int poolSize_fxMuzzleSmoke = 40;
        public int poolSize_bullet_blastermk1 = 100;

    }
}
