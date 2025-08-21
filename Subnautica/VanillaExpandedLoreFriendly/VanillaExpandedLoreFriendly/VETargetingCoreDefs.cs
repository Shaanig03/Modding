using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VanillaExpandedLoreFriendly
{
    public static class VETargetingCoreDefs
    {
        public static void StartTargetSearching(this TargetingCore targetingCore)
        {
            // stop coroutine if it already exists
            Coroutine coroutine = targetingCore.coroutine_targetSearching;
            if(coroutine != null) { targetingCore.StopCoroutine(coroutine); targetingCore.coroutine_targetSearching = null; }

            // start coroutine
            targetingCore.coroutine_targetSearching = targetingCore.StartCoroutine(ITargetSearcher(targetingCore));
        }

        private static WaitForSeconds searchDelay = new WaitForSeconds(0.5f);

        public static int i = 0;
        public static System.Collections.IEnumerator ITargetSearcher(TargetingCore targetingCore)
        {
            GameObject core = targetingCore.gameObject;


            Vector3 visionObjectPos = targetingCore.visionObject.transform.position;

            while (core != null)
            {
                LiveMixin coreTarget = targetingCore.Target;
                // search for a target if core has none
                if(coreTarget == null)
                {
                    // search target
                    targetingCore.SearchTarget();
                }
                else
                {
                    // if target is assigned
                    if (coreTarget.IsAlive())
                    {
                        Vector3 targetPos = coreTarget.transform.position;

                        // if target is in range
                        if(Vector3.Distance(targetPos, visionObjectPos) <= targetingCore.visionRadius)
                        {
                            Ray ray = new Ray(visionObjectPos, targetPos - visionObjectPos);
                            RaycastHit hit;

                            if(Physics.Raycast(ray, out hit, 1000, 1 << 0 | Vars.layerMask_terrain, QueryTriggerInteraction.Ignore))
                            {
                                //#tested

                                // get collider & live mixin component
                                Collider col = hit.collider;
                                LiveMixin liveMixin = col.GetComponent<LiveMixin>();
                                if (liveMixin == null) { liveMixin = col.GetComponentInParent<LiveMixin>(); }

                                // if ray didn't collide with target
                                if(liveMixin != coreTarget)
                                {
                                    // search for a new target
                                    targetingCore.Target = null;
                                    targetingCore.SearchTarget();
                                    ErrorMessage.AddMessage($"#temp ray did not collide with the target, looking for a new target");
                                }
                            }
                        }
                        else
                        {
                            //#tested
                            // if not in range, look for a new target
                            targetingCore.Target = null;
                            targetingCore.SearchTarget();
                            ErrorMessage.AddMessage($"#temp target is out of range, looking for a new target");
                        }
                    }
                    else 
                    {
                        //#tested
                        // search for a new target if current one is dead
                        targetingCore.Target = null;
                        targetingCore.SearchTarget();
                        ErrorMessage.AddMessage($"#temp target is dead, searching for a new target");
                    }
                }
                    i++;
                yield return searchDelay;
            }
        }

        public static void SearchTarget(this TargetingCore targetingCore)
        {
            
            Vector3 visionObjectPos = targetingCore.visionObject.transform.position;

            // do a spherical collision test
            Collider[] cols = Physics.OverlapSphere(visionObjectPos, targetingCore.visionRadius, 1 << 0, QueryTriggerInteraction.Ignore);

            // fetch targets
            List<LiveMixin> targets = new List<LiveMixin>();

            // loop through each collision
            foreach(Collider _col in cols) 
            {
                
                LiveMixin target = _col.GetComponent<LiveMixin>();
                if(target == null) { target = _col.GetComponentInParent<LiveMixin>(); }

                // if live mixin component is found && target is alive
                if (target != null && target.IsAlive())
                {
                    // get target name (in lower case)
                    string targetName = _col.gameObject.name.ToLower();

                    // do a target check name
                    foreach (string _specified_targetName in targetingCore.targetNames)
                    {
                        // if target's name contains this specific target name
                        if (targetName.Contains(_specified_targetName))
                        {
                            // add target
                            targets.Add(target);
                            Plugin.Log($"#temp target {_col.gameObject.name} added");
                            break;
                        }
                    }

                    
                }
            }

            // if there is any targets
            if (targets.Count > 0)
            {
                // get targets by nearest
                IOrderedEnumerable<LiveMixin> targetsByNearest = targets.OrderBy(x => (x.transform.position - visionObjectPos).sqrMagnitude);

                foreach(LiveMixin _target in targetsByNearest)
                {
                    // shoot ray towards target
                    Ray ray = new Ray(visionObjectPos, _target.transform.position - visionObjectPos);
                    RaycastHit hit;

                    // if ray hits something
                    if(Physics.Raycast(ray, out hit, 1000, 1 << 0 | Vars.layerMask_terrain, QueryTriggerInteraction.Ignore))
                    {
                        // get collider & live mixin component
                        Collider col = hit.collider;
                        LiveMixin liveMixin = col.GetComponent<LiveMixin>();
                        if (liveMixin == null) { liveMixin = col.GetComponentInParent<LiveMixin>(); }

                        if(liveMixin == _target)
                        {
                            targetingCore.Target = _target;

                            //#temp
                            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            sphere.transform.position = _target.transform.position;
                            ErrorMessage.AddMessage($"#temp target '{liveMixin.gameObject.name} found");
                            return;
                        }

                       
                    }
                }
            }


            ErrorMessage.AddMessage($"#temp visionRadius: {targetingCore.visionRadius} & cols length: {cols.Length}");


        }




        /*
        private static WaitForSeconds delay = new WaitForSeconds(0.5f);

        public static void StartSearchTargetByNameCheck(TargetingCore targetingCore)
        {
            // if a search coroutine already exists, stop it and set it to null
            var coroutine_search = targetingCore.coroutine_search;
            if (coroutine_search != null) { targetingCore.StopCoroutine(coroutine_search); targetingCore.coroutine_search = null; }

            // start search coroutine
            targetingCore.coroutine_search = targetingCore.StartCoroutine(ISearchTargetByNameCheck(targetingCore));
        }

        public static System.Collections.IEnumerator ISearchTargetByNameCheck(TargetingCore targetingCore)
        {
            Constructable constructable = targetingCore.GetComponent<Constructable>();

            while (targetingCore != null)
            {
                
                // search for a target if there is none assigned
                if(targetingCore.target == null)
                {
                    
                    if ((constructable != null && constructable.constructed) || constructable == null)
                    {
                        ErrorMessage.AddMessage($"#temp target went null, now in while loop");
                        VETargetingCoreDefs.SearchTargetByNameCheck(targetingCore);
                    }
                }
                yield return delay;
            }
        }

        public static void SearchTargetByNameCheck(TargetingCore targetingCore)
        {
           
            // get core position
            Vector3 corePos = targetingCore.transform.position;

            Plugin.Log($"#temp visionRadius: {targetingCore.visionRadius}");
            // do a spherical collision test
            Collider[] cols = Physics.OverlapSphere(corePos, targetingCore.visionRadius, 1 << 0, QueryTriggerInteraction.Ignore);

            Plugin.Log($"#temp cols length: {cols.Length}");
            // get targeting names
            string[] targetNames = targetingCore.targetNames;

            // grab living targets
            List<LiveMixin> targets = new List<LiveMixin>();
            foreach (Collider _col in cols)
            {
                LiveMixin target = _col.GetComponent<LiveMixin>();

                // if target exists & is alive
                if (target != null && target.health > 0)
                {
                    bool isATarget = false;

                    // get creature name in lower case
                    string creatureName = _col.gameObject.name.ToLower();

                    // loop through each target name and check if creature is a target
                    foreach (string _targetName in targetNames)
                    {
                        // if creature name contains target name
                        if (creatureName.Contains(_targetName))
                        {
                            isATarget = true;
                            Plugin.Log($"#temp is {creatureName}");
                            break;
                        }
                    }

                    // add target if its a target
                    if (isATarget)
                    {
                        targets.Add( target);
                    }
                }
            }

            if(targets.Count > 0)
            {
                // remove null objects
                targets.RemoveAll(x => x == null);

                Vector3 visionObjectPos = targetingCore.visionObject.transform.position;

                // set nearest creature as target
                IOrderedEnumerable<LiveMixin> targetsByNearest = targets.OrderBy(x => (x.transform.position - visionObjectPos).sqrMagnitude);
                foreach(LiveMixin _target in targetsByNearest)
                {
                    // if target exists
                    if (_target != null) { 
                        Vector3 targetPos = _target.transform.position;

                        Ray ray = new Ray(visionObjectPos, targetPos - visionObjectPos);
                        RaycastHit hit;

                        // shoot a ray towards target
                        if(Physics.Raycast(ray, out hit, 1500, 1 << 0 | Vars.layerMask_terrain))
                        {
                            if(hit.collider.gameObject == _target.gameObject)
                            {
                                targetingCore.target = _target;
                                Plugin.Log($"#temp target found: {_target.gameObject.name}");
                            }
                            return;
                        }
                       
                    }
                   
                }
               
                
            }

            

        }
        */
    }
}
