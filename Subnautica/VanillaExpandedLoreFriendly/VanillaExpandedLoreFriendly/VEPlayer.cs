using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VanillaExpandedLoreFriendly
{
    public class VEPlayer: MonoBehaviour
    {
        
        private bool loaded = false;


        void Start() { StartCoroutine(IStart()); }
        System.Collections.IEnumerator IStart()
        {
            // if game is not loading
            yield return new WaitUntil(() => !SaveLoadManager.main.isLoading);
            loaded = true;
        }

        void Update()
        {
            if (!loaded) { return; }


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
