using MarTools;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace MarKit
{
    public class HealthBehavior : MarKitBehavior, IBulletTarget, IProgressProvider
    {
        public MarKitEvent OnReceivedDamage;
        public MarKitEvent OnDied;

        public bool isDead { get; private set; } = false;
        public bool isAlive => !isDead;


        public int maxHealth = 10;
        public int currentHealth { get; private set; }
        public float normalizedHealth => (float)currentHealth / maxHealth;

        public float Progress => normalizedHealth;
        public float destroyDelay = 2;

        public int scoreOnKill = 0;

        protected virtual void Start()
        {
            currentHealth = maxHealth;
        }

        public virtual void Hit(BulletBehavior bullet)
        {
            OnReceivedDamage.Invoke(this);

            currentHealth--;

            if(currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            if (isDead) return;

            isDead = true;

            foreach (var item in GetComponentsInChildren<Collider2D>())
            {
                item.enabled = false;
            }

            Destroy(gameObject, destroyDelay);

            if(scoreOnKill > 0)
            {
                GameManager.Instance.AddScore(scoreOnKill, transform.position);
            }

            OnDied.Invoke(this);
        }

        internal void RestoreHealth()
        {
            currentHealth = maxHealth;
        }
    }
}
