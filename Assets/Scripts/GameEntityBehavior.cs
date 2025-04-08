using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace MarKit
{
    public class GameEntityBehavior : MarKitBehavior, IBulletTarget
    {
        public UnityEvent OnTakeDamage;
        public UnityEvent OnDestroyed;
        public UnityEvent<float> OnUpdateHealth;

        public UnityEvent OnDeath;


        public bool isDead { get; private set; } = false;
        public bool isAlive => !isDead;


        public int maxHealth = 10;
        public int currentHealth { get; private set; }
        public float normalizedHealth => (float)currentHealth / maxHealth;

        protected virtual void Start()
        {
            currentHealth = maxHealth;
        }

        public virtual void Hit(BulletBehavior bullet)
        {
            OnTakeDamage.Invoke();

            currentHealth--;

            OnUpdateHealth.Invoke(normalizedHealth);

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

            Destroy(gameObject, 2);

            OnDeath.Invoke();
        }

        private void OnDestroy()
        {
            OnDestroyed.Invoke();
        }
    }
}
