using UnityEngine;
using System.Collections;

namespace Emitter
{
    public class Proj_Bullet : Projectile
    {
        // Use this for initialization
        void Start()
        {
            if (GetComponent<Rigidbody>() == null)
            {
                gameObject.AddComponent<Rigidbody>();
            }
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Rigidbody>().useGravity = false;
        }

        public override ProjectileType GetProjectileType()
        {
            return ProjectileType.Bullet;
        }

        public override void OnSpawn(Emitter_Basic bm)
        {
            base.OnSpawn(bm);

            // TODO 某些特殊的子弹类型，自行选择目标
        }
    }
}
