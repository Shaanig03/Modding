using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace VanillaExpandedLoreFriendly.Buildables
{
    public class TurretPlasma : TurretBase
    {

        public override void OnEnable()
        {
            base.OnEnable();
        }

        public override void Awake()
        {
            base.Awake();
            transform.Find("Model/0/Armature.014").SetParent(transform);
        }

        public override void Start()
        {
            base.Start();

            // reset fire anim delay
            waitForSec_resetFireAnimDelay = new WaitForSeconds(0.320f);

            // set rotation speed
            rotateSpeed = 100;

            Transform t_rifleTurretMK1_turret = gameObject.transform.Find("Armature.014/root/mainRotator/mainCannon");

            // set aim object
            aimObject = t_rifleTurretMK1_turret.gameObject;

            // set ammunition
            ammunitionName = "turretplasma_ammo";

            // set spawn fxs 
            PoolContainer pool_fx_muzzlesmoke = PoolDefs.pools["fx_muzzlesmoke"];
            PoolContainer pool_fx_bullet = PoolDefs.pools["bullet_plasma"];

            

            fire_fxs = new PoolObjectSpawnpoint[]
            {
                new PoolObjectSpawnpoint
                {
                    spawnpoint = t_rifleTurretMK1_turret.Find("sp_fxSmoke").gameObject,
                    pool = pool_fx_muzzlesmoke
                },
                new PoolObjectSpawnpoint
                {
                    spawnpoint = t_rifleTurretMK1_turret.Find("sp_fxSmoke (1)").gameObject,
                    pool = pool_fx_muzzlesmoke
                },
                new PoolObjectSpawnpoint
                {
                    spawnpoint = t_rifleTurretMK1_turret.Find("sp_bullet").gameObject,
                    pool = pool_fx_bullet
                },
                new PoolObjectSpawnpoint
                {
                    spawnpoint = t_rifleTurretMK1_turret.Find("sp_bullet (1)").gameObject,
                    pool = pool_fx_bullet
                }
            };
        }
    }
}
