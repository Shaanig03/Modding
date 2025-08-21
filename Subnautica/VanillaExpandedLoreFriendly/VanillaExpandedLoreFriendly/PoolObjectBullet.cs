using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanillaExpandedLoreFriendly
{
    public class PoolObjectBullet : PoolObject
    {
        private TurretBullet _bullet;
        public TurretBullet Bullet { get {
                if(_bullet == null) { _bullet = GetComponent<TurretBullet>(); };
                return _bullet; 
            } }
        void Awake()
        {

        }
    }
}
