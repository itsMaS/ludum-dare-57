using MarTools;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MarKit
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : Singleton<PlayerController>, IVisible2D, IBulletTarget, IMarkitEventCaller
    {
        public MarKitEvent OnDash;
        public MarKitEvent OnStartShooting;
        public MarKitEvent OnEndShooting;
        public MarKitEvent OnDeath;
        public MarKitEvent OnTakeDamage;
        public MarKitEvent OnPowerupCollected;
        public MarKitEvent OnPowerupExpired;

        Rigidbody2D rb;
        Vector2 movementInputVector;
        Vector2 aimInputVector;

        [SerializeField] Transform aimTransform;
        [SerializeField] Transform movementTransform;
        [SerializeField] Transform cameraTargetTransform;

        [SerializeField] Cooldown dashCooldown;
        [SerializeField] Cooldown damageCooldown;

        Vector2 dashInertia;

        public int maxLives;
        public int currentLives { get; private set; }
        public bool isDead = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            currentLives = maxLives;
        }

        private void Update()
        {
            if (GameManager.Instance.isPaused) return;

            if (currentLives <= 0) return;

            movementInputVector = Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1);

            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition.MaskZ(-Camera.main.transform.position.z));

            Debug.DrawLine(transform.position, mouseWorldPosition);

            aimInputVector = (mouseWorldPosition - transform.position) / 10;

            if(Input.GetKeyDown(KeyCode.Space) && dashCooldown.TryPerform())
            {
                Dash();
            }

            aimTransform.rotation = Quaternion.LookRotation(Vector3.forward, aimInputVector);

            if(movementInputVector.magnitude > 0.05f)
            {
                movementTransform.rotation = Quaternion.LookRotation(Vector3.forward, movementInputVector);
            }

            cameraTargetTransform.position = (Vector3)((Vector2)transform.position + aimInputVector);

            if(currentPowerup && currentPowerup.timeLeft <= 0)
            {
                currentPowerup.Expire();
                currentPowerup = null;
                OnPowerupExpired.Invoke(this);
            }

            if(Input.GetMouseButtonDown(0)) { OnStartShooting.Invoke(this); }
            if(Input.GetMouseButtonUp(0)) { OnEndShooting.Invoke(this); }
        }

        private void Dash()
        {
            dashInertia = rb.linearVelocity.normalized * 150;

            Vector3 start = dashInertia;
            this.DelayedAction(0.3f, () =>
            {

            }, t =>
            {
                dashInertia = Vector3.Lerp(start, Vector3.zero, t);
            }, true, Utilities.Ease.OutQuad);

            OnDash.Invoke(this);

        }

        private void FixedUpdate()
        {
            if (currentLives <= 0) return;
            rb.linearVelocity = movementInputVector * 40 + dashInertia;
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.transform.CompareTag("KillZone"))
            {
                Die();
            }
            else if(collision.transform.CompareTag("Collectable"))
            {
                Vector3 startPos = collision.transform.position;

                collision.enabled = false;
                this.DelayedAction(0.2f, () =>
                {
                    Destroy(collision.gameObject);
                }, t =>
                {
                    collision.transform.position = Vector3.Lerp(startPos, transform.position, t);
                }, true, Utilities.Ease.OutQuad);
            }
            else if(collision.transform.CompareTag("Powerup"))
            {
                if(collision.TryGetComponent<PowerupBehavior>(out var powerup))
                {
                    CollectPowerup(powerup);
                }
            }
        }

        public PowerupBehavior currentPowerup { get; private set; }

        private void CollectPowerup(PowerupBehavior powerup)
        {
            if(currentPowerup && !currentPowerup.expired)
            {
                currentPowerup.Expire();
                currentPowerup = null;
                OnPowerupExpired.Invoke(this);
            }

            currentPowerup = powerup;
            powerup.Collect(this);

            OnPowerupCollected.Invoke(this);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(collision.collider.CompareTag("Enemy"))
            {
                TakeDamage();
            }
        }


        private void TakeDamage()
        {
            if (currentLives <= 0) return;

            if(damageCooldown.TryPerform())
            {
                GameManager.Instance.ResetCombo();

                foreach (var item in Physics2D.OverlapCircleAll(transform.position, 5))
                {
                    if(item.gameObject.CompareTag("Enemy") && item.TryGetComponent<NavigationBehavior>(out var enemy))
                    {
                        //enemy.inertia += (Vector2)(enemy.transform.position - transform.position).normalized * 20;
                    }
                }


                currentLives--;
                OnTakeDamage.Invoke(this);

                if(currentLives <= 0)
                {
                    Die();
                }
            }
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            currentLives = 0;

            OnDeath.Invoke(this);
            GameManager.GameOver();

            rb.gravityScale = 5;
            rb.linearDamping = 0;

            GetComponentInChildren<Animator>().Play("Death");
        }

        public void Hit(BulletBehavior bullet)
        {
            TakeDamage();
        }

        internal void Respawn()
        {
            currentLives = maxLives;
            transform.position = new Vector3(0, 60, 0);

            rb.gravityScale = 0;
            rb.linearDamping = 50;

            GetComponentInChildren<Animator>().Play("Fly");

            isDead = false;
        }
    }
}
