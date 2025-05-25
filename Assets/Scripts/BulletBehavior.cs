using MarTools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarKit
{
    public interface IBulletTarget
    {
        public Transform transform { get; }
        public GameObject gameObject { get; }
        public void Hit(BulletBehavior bullet);
    }

    public class BulletBehavior : MarKitBehavior
    {
        public static Dictionary<BulletBehavior, List<BulletBehavior>> BulletPools = new Dictionary<BulletBehavior, List<BulletBehavior>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void ResetPools()
        {
            BulletPools.Clear();
        }

        private static void ReturnToPool(BulletBehavior bullet)
        {
            bullet.gameObject.SetActive(false);

            List<BulletBehavior> Pool = null;

            if(!BulletPools.TryGetValue(bullet.spawnedPrefab, out Pool)) 
            {
                Pool = new List<BulletBehavior>();
                BulletPools.Add(bullet.spawnedPrefab, Pool);

            }

            Pool.Add(bullet);
        }
        private static BulletBehavior GetBulletFromPool(BulletBehavior prefab)
        {
            if(BulletPools.TryGetValue(prefab, out var pool) && pool.Count > 0)
            {
                var picked = pool.First();
                pool.RemoveAt(0);

                picked.gameObject.SetActive(true);
                return picked;
            }
            else
            {
                var instance = Instantiate(prefab);
                instance.spawnedPrefab = prefab;

                return instance;
            }
        }

        public MarKitEvent OnSpawned;
        public MarKitEvent OnCollided;
        public MarKitEvent OnDespawned;

        public float baseSpeed = 0;
        public float lifetime = 5;
        public float radius = 0.5f;


        Vector2 previousPosition;
        Vector2 velocity;

        bool despawning = false;
        public LayerMask collisionMask;

        private BulletBehavior spawnedPrefab;

        public BulletBehavior SpawnFromPrefab(Vector3 spawnPosition, Vector3 velocity)
        {
            var instance = GetBulletFromPool(this);
            instance.transform.position = spawnPosition;
            instance.transform.rotation = Quaternion.LookRotation(Vector3.forward, velocity);

            instance.despawning = false;
            previousPosition = spawnPosition;

            instance.velocity = velocity * baseSpeed;

            instance.OnSpawned.Invoke(this);


            instance.DelayedAction(lifetime, () => 
            {
                instance.Despawn();
            });

            return instance;
        }

        private void Update()
        {
            if (despawning) return;

            RaycastHit2D hit = Physics2D.CircleCast(transform.position, radius, velocity, velocity.magnitude * Time.deltaTime, collisionMask);
            if (hit)
            {
                transform.position = hit.point;
                Collide(hit.collider);
            }
            else
            {
                transform.position = (Vector2)transform.position + velocity * Time.deltaTime;
            }
        }

        private void Collide(Collider2D col)
        {
            if (despawning) return;

            if(col.gameObject.TryGetComponentInParent<IBulletTarget>(out var target))
            {
                target.Hit(this);
            }

            OnCollided.Invoke(this);

            Despawn();
        }

        private void Despawn()
        {
            if (despawning) return;

            OnDespawned.Invoke(this);
            despawning = true;

            this.DelayedAction(0.5f, () =>
            {
                ReturnToPool(this);
            });
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
