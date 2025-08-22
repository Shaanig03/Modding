using FMODUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static VFXSandSharkDune;

namespace VanillaExpandedLoreFriendly.Buildables
{
    public class TurretBase : MonoBehaviour
    {
        // turret rotators
        public GameObject[] rotators;

        // aim point & rotation
        public Vector3 aimPoint;
        public bool calculateAimPointAngle = true;
        public float rotateSpeed = 10;
        public float rotateSpeedX = 5;
        private Quaternion _cannon_clampUp = Quaternion.Euler(-90, 0, 0);
        private Quaternion _cannon_clampDown = Quaternion.Euler(32.31f, 0, 0);
        private float _offset_angle;

        // animator
        public Animator animator;

        // fire fxs 
        public PoolObjectSpawnpoint[] fire_bullets;
        public ParticleSystem[] fire_fx_muzzles;
        public PoolObjectSpawnpoint[] fire_fxs;

        public int anim_prop_turretFiring;

        // delay
        public WaitForSeconds delay_animFireReset = new WaitForSeconds(0.1f);

        // fire sound
        public string fire_soundName;
        public FMODAsset fire_sound;

        // constructable
        public Constructable constructable;

        // targeting core
        public TargetingCore targetingCore;

        // power
        public PowerRelay powerRelay;
        public PowerSystem.Status powerStatus;

        // firing
        public bool firing;
        public float fireDelay = 0.5f;
        public WaitForSeconds waitForSec_fireDelay;
        public WaitForSeconds waitForSec_resetFireAnimDelay;
        public float requiredAngleToFire = 10;

        public GameObject aimObject;

        // coroutines
        private Coroutine coroutineFire;
        public Coroutine coroutine_resetFireAnim;
        private Coroutine coroutine_powerUpdate;
        private Coroutine coroutine_powerConsumption;

        protected bool _deconstructed = false;
        private bool _loaded = false;

        public StorageContainer container;
        public Transform storageRoot;

        public List<Battery> ammunitions = new List<Battery>();
        public string ammunitionName;

        public virtual void OnEnable()
        {
            if(constructable == null) { constructable = GetComponent<Constructable>(); }

            // start fire coroutine
            if (coroutineFire != null) {StopCoroutine(coroutineFire); coroutineFire = null;}
            coroutineFire = StartCoroutine(IFire());
            
            // start power update coroutine
            if (coroutine_powerUpdate != null) { StopCoroutine(coroutine_powerUpdate); coroutine_powerUpdate = null; }
            coroutine_powerUpdate = StartCoroutine(IPowerUpdate());

            // start target searching
            targetingCore.StartTargetSearching();

            // start power consumption coroutine
            if(coroutine_powerConsumption != null) { StopCoroutine(coroutine_powerConsumption); coroutine_powerConsumption = null;}
            coroutine_powerConsumption = StartCoroutine(VETurretMethods.IPowerConsumption(this));

            ErrorMessage.AddMessage($"#temp OnEnable fired");
        }

        public virtual void OnDisable()
        {
            _deconstructed = (constructable != null && constructable.constructed) ? false : true;
        }




        public virtual void Awake()
        {
            animator = GetComponent<Animator>();
            targetingCore = GetComponent<TargetingCore>();
        }

        public virtual void Start()
        {
            
            powerRelay = GetComponentInParent<PowerRelay>();
            anim_prop_turretFiring = Animator.StringToHash("firing");


            // get storage container
            container = GetComponent<StorageContainer>();
            storageRoot = container.storageRoot.transform;
            container.container.onAddItem += Container_onAddItem;
            container.container.onRemoveItem += Container_onRemoveItem;

            // get fire sound
            fire_sound = Nautilus.Utility.AudioUtils.GetFmodAsset(fire_soundName);

            

            // fire delay
            waitForSec_fireDelay = new WaitForSeconds(fireDelay);

            // add turret collider part components
            Collider[] cols = GetComponentsInChildren<Collider>();
            foreach(Collider _col in cols)
            {
                _col.gameObject.AddComponent<TurretColliderPart>();
            }

            // update container
            UpdateContainer();

            _loaded = true;
        }

        void UpdateContainer(Pickupable removingItem = null)
        {
            ammunitions.Clear();

            foreach(Transform _t_item in storageRoot)
            {
                Pickupable pickupable = _t_item.GetComponent<Pickupable>();
                if(pickupable != null && pickupable != removingItem)
                {
                    string _itemName = pickupable.inventoryItem._techType.ToString();
                    
                    // if its a ammunition, get its battery component and store it
                    if(_itemName == ammunitionName)
                    {
                        Battery battery = pickupable.GetComponent<Battery>();
                        ammunitions.Add(battery);
                    }
                    ErrorMessage.AddMessage($"#temp item: {_itemName}");
                }
            }
        }

        private void Container_onAddItem(InventoryItem item)
        {
            UpdateContainer();
        }

        private void Container_onRemoveItem(InventoryItem item)
        {
            UpdateContainer(item.item);
        }

        

        private System.Collections.IEnumerator IFire()
        {
            yield return new WaitUntil(() => _loaded);

            WaitUntil waitUntil_firing = new WaitUntil(() => firing);

            while(this != null)
            {
                // wait until firing
                yield return waitUntil_firing;

                // if power is on & turret is constructed
                if(powerStatus == PowerSystem.Status.Normal && constructable.constructed)
                {
                    // turret fire
                    this.Fire();
                }

                // fire delay
                yield return waitForSec_fireDelay;
            }
        }
        private System.Collections.IEnumerator IPowerUpdate() {
            yield return new WaitUntil(() => _loaded);

            WaitForSeconds delay = new WaitForSeconds(0.5f);

            while (this != null)
            {
                if (powerRelay != null)
                {
                    // update power status
                    var basePowerStatus = powerRelay.powerStatus;
                    if (powerStatus != basePowerStatus) { powerStatus = basePowerStatus; }
                }
                else
                {
                    // find a power source
                    Base[] bases = GameObject.FindObjectsOfType<Base>();
                    foreach(Base _base in bases)
                    {
                        if(Vector3.Distance(_base.transform.position, transform.position) <= Vars.config.turretPowerSearchRadius)
                        {
                            powerRelay = _base.GetComponent<PowerRelay>();

                            // update power status
                            var basePowerStatus = powerRelay.powerStatus;
                            if (powerStatus != basePowerStatus) { powerStatus = basePowerStatus; }
                            break;
                        }
                    }
                }
                yield return delay;
            }
        }

        void CalculateAngle()
        {
            // get core & cannon
            Transform core = rotators[0].transform;
            Transform cannon = rotators[1].transform;

            Vector3 corePos = core.position;
            Vector3 cannonPos = cannon.position;
            corePos.y = cannonPos.y;

            Vector3 _aimPoint = aimPoint;
            _aimPoint.y = cannonPos.y;

            float targetDistance = Vector3.Distance(_aimPoint, corePos);

            Vector3 VPoint = corePos + (core.forward * Vector3.Distance(corePos, _aimPoint));// + (-core.right * (cannon.position - core.position).magnitude);
            Vector3 WPoint = cannonPos + ((Quaternion.Euler(0, cannon.rotation.eulerAngles.y, 0) * Vector3.forward) * Vector3.Distance(cannonPos, _aimPoint));

            float XDistance = Vector3.Distance(corePos, WPoint);

            //Gizmos.color = Color.white;
            //Gizmos.DrawLine(cannonPos, WPoint);

            //Gizmos.color = Color.cyan;
            //Gizmos.DrawLine(corePos, VPoint);

            //Gizmos.color = Color.red;
            //Gizmos.DrawLine(corePos, WPoint);


            //Gizmos.color = Color.red;
            //Gizmos.DrawRay(cannonPos, cannon.transform.forward * 100);

            float YDistance = Vector3.Distance(VPoint, WPoint);

            float turnRadians = Mathf.Asin(YDistance / XDistance);
            _offset_angle = Mathf.Rad2Deg * turnRadians;
        }

        public virtual void Rotate()
        {
            // get core & cannon
            Transform core = rotators[0].transform;
            Transform cannon = rotators[1].transform;
              

            


            bool functioning = (constructable.constructed && powerStatus != PowerSystem.Status.Offline);

            if (functioning)
            {
                // calculate angle so that the cannon is facing towards the target
                if (calculateAimPointAngle)
                {
                    CalculateAngle();
                }

                // rotate core (on Y axis)
                Quaternion lookRotCore = Quaternion.LookRotation(aimPoint - core.position, Vector3.up);
                float angle = (float.IsNaN(_offset_angle)) ? 0 : _offset_angle;
                core.rotation = Quaternion.RotateTowards(core.rotation, Quaternion.Euler(0, lookRotCore.eulerAngles.y - angle, 0), rotateSpeed * Time.deltaTime);
            }






            if (functioning)
            {
                // rotate cannon (on X axis)
                Quaternion lookRotCannon = Quaternion.LookRotation(aimPoint - cannon.position, Vector3.up);
                Quaternion rotateTargetCannon = Quaternion.Euler(lookRotCannon.eulerAngles.x, 0, 0);

                // clamp cannon (on X axis)
                if (VEMethods.SignedAngle(rotateTargetCannon, _cannon_clampUp) < 0) { rotateTargetCannon = _cannon_clampUp; }
                else if (VEMethods.SignedAngle(rotateTargetCannon, _cannon_clampDown) > 0) { rotateTargetCannon = _cannon_clampDown; }

                cannon.localRotation = Quaternion.Lerp(cannon.localRotation, rotateTargetCannon, rotateSpeedX * Time.deltaTime);
            }
            else
            {
                
                cannon.localRotation = Quaternion.Lerp(cannon.localRotation, Quaternion.Euler(38.902f,0,0), 3 * Time.deltaTime);
            }
        }



        void Update()
        { 
            Rotate();
            UpdateAimPoint();
            TriggerFire();
        }

        void TriggerFire()
        {
            var target = targetingCore.Target;

           
            if (target != null)
            {
                Vector3 aimForward = aimObject.transform.forward;
                Vector3 targetDir = target.transform.position - aimObject.transform.position;

                float angle = Vector3.Angle(targetDir, aimForward);
                Plugin.Log($"#temp angle: {angle}");
                if (angle <= requiredAngleToFire)
                {
                    if (!firing) { firing = true; }
                } else if (firing) { firing = false; }


            } else if (firing) { firing = false; }
           
        }
        void UpdateAimPoint()
        {
            var target = targetingCore.Target;
            if (target != null)
            {
                aimPoint = target.transform.position;
            }
        }


    }

}
