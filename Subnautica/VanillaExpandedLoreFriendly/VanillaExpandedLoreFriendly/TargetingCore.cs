using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VanillaExpandedLoreFriendly
{
    public class TargetingCore : MonoBehaviour
    {
        // vision object & radius
        public GameObject visionObject;
        public float visionRadius;



        public Constructable constructable;

        // target
        public LiveMixin Target
        {
            get { return _target; }
            set { _target = value; }
        }
        private LiveMixin _target;

        public string[] targetNames = new string[] { "peep" }; // #temp peep

        public Coroutine coroutine_targetSearching;

        void Awake()
        {
            constructable = GetComponent<Constructable>();
        }
        void Update()
        {
        }
    }
}
