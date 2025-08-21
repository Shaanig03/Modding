
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UWE;
using VanillaExpandedLoreFriendly.Buildables;
using VanillaExpandedLoreFriendly.UI;

namespace VanillaExpandedLoreFriendly
{
    public class VEPlayer: MonoBehaviour
    {

        public Player player;
        private bool loaded = false;

        private UITurretConfiguration ui_turretConfig;

        System.Collections.IEnumerator Start() {

            // instantiate ui turret
            ui_turretConfig = GameObject.Instantiate(Registries.prefab_ui_turretConfiguration).GetComponent<UITurretConfiguration>();
            

            // wait until loading is finished
            yield return new WaitUntil(() => !SaveLoadManager.main.isLoading);

            player = GetComponent<Player>();

            

            loaded = true;
        }


        void Update()
        {
            if (!loaded) { return; }

            
            // turret configuration UI
            if (Input.GetKeyDown(Vars.key_turretConfig))
            {
                if (!ui_turretConfig.state)
                {
                    // get camera
                    Camera cam = player.camRoot.mainCam;

                    // camera forward & position
                    UnityEngine.Vector3 camForward = cam.transform.forward;
                    UnityEngine.Vector3 camPos = cam.transform.position + (camForward * 1.2f);

                    // create ray
                    Ray ray = new Ray(camPos, camForward);
                    RaycastHit hit;

                    // shoot ray
                    if (Physics.Raycast(ray, out hit, 6, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                    {
                        Collider col = hit.collider;


                        TargetingCore targetingCore = col.GetComponent<TargetingCore>();
                        if (targetingCore == null) { targetingCore = col.GetComponentInParent<TargetingCore>(); }

                        // if targeting core was found
                        if (targetingCore != null)
                        {
                            ui_turretConfig.Open(targetingCore);
                        }
                    }
                }
                else
                {
                    ui_turretConfig.Close();
                }
            }

            //#temp
            if (Input.GetKeyDown(KeyCode.U))
            {
                if (!ui_turretConfig.state)
                {
                    ui_turretConfig.Open();
                }
                else
                {
                    ui_turretConfig.Close();
                }
            }

            //#temp
            if (Input.GetKeyDown(KeyCode.Y))
            {
                UnityEngine.Vector3 pos = transform.position + (transform.forward * 3);
                PoolDefs.pools["bullet_blastermk1"].TakeObjectFromPool(transform.position + transform.forward * 3, UnityEngine.Quaternion.identity);
            }

            //#temp
            if (Input.GetKeyDown(KeyCode.T))
            {


                TurretBase turret = GameObject.FindObjectOfType<TurretBase>();

                turret.waitForSec_fireDelay = new WaitForSeconds(turret.fireDelay);
                if (turret.firing) { turret.firing = false; } else { turret.firing = true; }


                //PoolDefs.pools["bullet_blastermk1"].TakeObjectFromPool(transform.position + transform.forward * 3, Quaternion.identity);
            }

            // saves game by pressing the save key
            if (Input.GetKeyDown(Vars.key_save))
            {
                if(IngameMenu.main != null) {
                    IngameMenu.main.SaveGame();
                }
            }
        }
    }
}
