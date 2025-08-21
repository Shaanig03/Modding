using mset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaExpandedLoreFriendly.Buildables;

namespace VanillaExpandedLoreFriendly
{
    public class TurretBullet : MonoBehaviour
    {
        public PoolObjectBullet poolObject;
        public GameObject firedBy;

        public float damage;
        public float speed;
        private Rigidbody rigi;

        void Awake()
        {
            rigi = GetComponent<Rigidbody>();
        }
        void Update()
        {
            rigi.velocity = (transform.forward * speed * Time.deltaTime);
        }

        void OnTriggerEnter(Collider col)
        {
            // ignore triggers
            if (col.isTrigger) { return; }

            // ignore projectile from colliding with turrets
            if (col.GetComponent<TurretColliderPart>() != null) { return; }


            LiveMixin liveMixin = col.GetComponent<LiveMixin>();
            if(liveMixin != null)
            {
                liveMixin.TakeDamage(damage, transform.position, DamageType.Normal, firedBy);
            }

            // add object to pool
            poolObject.AddObjectToPool();
        }
    }
}
