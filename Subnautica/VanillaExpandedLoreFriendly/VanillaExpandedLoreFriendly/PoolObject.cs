using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VanillaExpandedLoreFriendly
{
    public class PoolObject : MonoBehaviour
    {
        public PoolContainer assignedPool;
        public Coroutine coroutine_lifeTime;


        void OnEnable()
        {
            if (assignedPool == null) { return; }

            // start pool object lifetime
            this.StartPoolObjectLifetimeCoroutine();
        }
        void OnDisable()
        {
            // stop pool object lifetime on disable
            this.StopPoolObjectLifetimeCoroutine();
        }
    }

    
}
