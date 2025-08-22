using UnityEngine;
using UWE;
using VanillaExpandedLoreFriendly.Buildables;

namespace VanillaExpandedLoreFriendly
{
    public static class VETurretMethods
    {
       
        private static WaitForSeconds turretPowerConsumptionDelay = new WaitForSeconds(1f);
        public static System.Collections.IEnumerator IPowerConsumption(this TurretBase turret)
        {
            //#tested
            bool consumeEnergyEnabled = Vars.config.turretConsumesPower;

            if (consumeEnergyEnabled)
            {
                while (turret != null)
                {
                    var powerRelay = turret.powerRelay;

                    // if turret is constructed & power relay exists
                    if (turret.constructable.constructed && powerRelay != null && turret.powerStatus != PowerSystem.Status.Offline)
                    {
                        // consume power
                        float amountConsumed;
                        powerRelay.ConsumeEnergy(Vars.config.turretConsumePowerAmount, out amountConsumed);
                    }
                    yield return turretPowerConsumptionDelay;
                }
            }
            yield return null;
        }
        public static void CreateBulletPool(string poolName, GameObject prefab, int size, float lifetime, float damage, float speed)
        {
            PoolDefs.CreatePool(poolName, 
                prefab, 
                size, 
                lifetime, 
                PoolObjectTakeAction.CREATE_NEW_IF_MAX_REACHED, 
                typeof(PoolObjectBullet), 
                OnBulletPoolObjectCreate,
                OnBulletPoolObjectTake,
                new object[]
            {
                damage,
                speed
            });
            
        }

        public static void CreateBullet(GameObject gameObject, PoolObjectBullet poolObjectBullet, float damage, float speed)
        {
            TurretBullet bullet = gameObject.AddComponent<TurretBullet>();
            bullet.poolObject = poolObjectBullet;
            bullet.damage = damage;
            bullet.speed = speed;
        }
        public static void OnBulletPoolObjectCreate(PoolObject poolObjectBase, object[] _params)
        {

            PoolObjectBullet poolObject = poolObjectBase as PoolObjectBullet;
            CreateBullet(poolObject.gameObject, poolObject, (float)_params[0], (float)_params[1]);
        }
        public static void OnBulletPoolObjectTake(PoolObject poolObjectBase, object[] _params)
        {
            PoolObjectBullet poolObject = poolObjectBase as PoolObjectBullet;
            if (_params != null)
            {
                poolObject.Bullet.firedBy = (GameObject)_params[0];
            }
        }


        public static void RegisterTurret(GameObject prefab, string turretName, TurretBase turretComponent, float visionRadius)
        {

            Collider[] cols = prefab.GetComponentsInChildren<Collider>();

            foreach (Collider _col in cols)
            {
                GameObject col_gameObject = _col.gameObject;
                if(col_gameObject != prefab)
                {
                    col_gameObject.layer = Vars.layerMask_ignoreRaycast;
                }
            }

            TargetingCore targetingCore = prefab.AddComponent<TargetingCore>();
            targetingCore.visionRadius = visionRadius;

            VEMethods.RegisterStorage(prefab, turretName, turretName, 3, 3, "", "");
        }

        public static void Fire(this TurretBase turret)
        {
            bool firePass = false;

            // if not using infinite turret ammo
            if (!Vars.config.infiniteTurretAmmo)
            {
                // loop through each ammunition battery component
                foreach(Battery _battery in turret.ammunitions)
                {
                    // if there is charge
                    if(_battery != null && _battery.charge > 0)
                    {
                        firePass = true;
                        _battery.charge -= 1;
                        break;
                    }
                }
            }
            else { firePass = true; }

            // exit if fire pass failed
            if (!firePass) { return; }

            // play fire fx muzzles
            foreach (ParticleSystem _fx in turret.fire_fx_muzzles) { if (_fx != null) { _fx.Play(); } }

            // spawn fire fxs
            foreach (PoolObjectSpawnpoint _poolObjectSP in turret.fire_fxs)
            {
                Transform spawnpoint = _poolObjectSP.spawnpoint.transform;
                _poolObjectSP.pool.TakeObjectFromPool(spawnpoint.position, spawnpoint.rotation, new object[] { turret.gameObject });
            }

    
            // play fire animation
            Animator animator = turret.animator;
            if (animator != null)
            {
                animator.SetBool(turret.anim_prop_turretFiring, true);

                Coroutine coroutine = turret.coroutine_resetFireAnim;
                if(coroutine != null) { turret.StopCoroutine(coroutine); turret.coroutine_resetFireAnim = null; }
                turret.coroutine_resetFireAnim = turret.StartCoroutine(IResetFireAnim(turret));
            }


            
            // play fire sound
            FMODUWE.PlayOneShot(turret.fire_sound, turret.transform.position, Plugin.ingameConfig.turretFireVolume);
        }


        public static System.Collections.IEnumerator IResetFireAnim(TurretBase turret)
        {
            yield return turret.waitForSec_resetFireAnimDelay;
            if (turret != null)
            {
                turret.animator.SetBool(turret.anim_prop_turretFiring, false);
            }
        }
    }
}
