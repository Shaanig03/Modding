using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace VanillaExpandedLoreFriendly.Buildables
{
    public class TurretRifleMK1 : TurretBase
    {

        public override void OnEnable()
        {
            base.OnEnable();
        }

        public override void Awake()
        {
            base.Awake();
            transform.Find("Model/0/Armature.003").SetParent(transform);
        }

        public override void Start()
        {
            base.Start();

            // reset fire anim delay
            waitForSec_resetFireAnimDelay = new WaitForSeconds(0.240f);

            // set rotation speed
            rotateSpeed = 100;

            Transform t_rifleTurretMK1_turret = gameObject.transform.Find("Armature.003/TurretHolder 1/TurretCore 1/turret");

            // set aim object
            aimObject = t_rifleTurretMK1_turret.gameObject;

            // set ammunition
            ammunitionName = "turretblastermk1_ammo";

            // set spawn fxs 
            PoolContainer pool_fx_muzzlesmoke = PoolDefs.pools["fx_muzzlesmoke"];
            PoolContainer pool_fx_blastermk1Bullet = PoolDefs.pools["bullet_blastermk1"];

            

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
                    pool = pool_fx_blastermk1Bullet
                },
                new PoolObjectSpawnpoint
                {
                    spawnpoint = t_rifleTurretMK1_turret.Find("sp_bullet (1)").gameObject,
                    pool = pool_fx_blastermk1Bullet
                }
            };
        }
    }
}
