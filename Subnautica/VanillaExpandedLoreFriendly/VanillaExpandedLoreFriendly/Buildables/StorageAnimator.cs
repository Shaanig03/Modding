using FMOD;
using Nautilus.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace VanillaExpandedLoreFriendly.Buildables
{
    public class StorageAnimator : MonoBehaviour
    {
        private StorageContainer _storageContainer;
        public Animator animator;
        private int animProperty;

        private bool opened = false;

        public string sound_open;
        public string sound_close;


        void Start()
        {
            animProperty = Animator.StringToHash("opened");

            _storageContainer = GetComponent<StorageContainer>();
            _storageContainer.onUse.AddListener(onOpen);

            StartCoroutine(ICloser());
        }

        System.Collections.IEnumerator ICloser()
        {
            WaitForSeconds delay = new WaitForSeconds(0.5f);

            while (this != null)
            {
                // if container is closed && but animated state is set to opened
                if(!_storageContainer.GetOpen() && opened)
                {
                    // animate storage
                    opened = false;
                    animator.SetBool(animProperty, opened);
                    
                    // play sound
                    VEMethods.PlayFMODSound(sound_close, transform, 20);
                }
                yield return delay;
            }
        }

        void onOpen()
        {
            if(animator == null) { return; }

            // animate storage
            opened = true;
            animator.SetBool(animProperty, opened);

            // play sound
            VEMethods.PlayFMODSound(sound_open, transform, 20);
        }
    }
}
