using MarTools;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace MarKit
{
    public interface IBulletTarget
    {
        public Transform transform { get; }
        public GameObject gameObject { get; }
        public void Hit(BulletBehavior bullet);
    }

    public class BulletBehavior : MonoBehaviour
    {
        public UnityEvent OnLaunched;
        public UnityEvent OnCollided;
        public UnityEvent OnDespawn;

        public float baseSpeed = 0;
        public float lifetime = 5;


        Vector2 previousPosition;
        Vector2 velocity;

        bool despawning = false;
        public LayerMask collisionMask;

        public BulletBehavior SpawnFromPrefab(Vector3 spawnPosition, Vector3 velocity)
        {
            var instance = Instantiate(this, spawnPosition, Quaternion.LookRotation(Vector3.forward, velocity));

            instance.despawning = false;
            instance.velocity = velocity * baseSpeed;

            instance.OnLaunched.Invoke();


            instance.DelayedAction(lifetime, () => 
            {
                instance.Despawn();
            });

            return instance;
        }

        private void Update()
        {
            if (despawning) return;

            RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.5f, velocity, velocity.magnitude * Time.deltaTime, collisionMask);
            if (hit)
            {
                transform.position = (Vector2)transform.position + velocity * Time.deltaTime;
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

            OnCollided.Invoke();

            Despawn();
        }

        private void Despawn()
        {
            if (despawning) return;

            OnDespawn.Invoke();
            despawning = true;
            Destroy(gameObject, 2f);
        }
    }
}
