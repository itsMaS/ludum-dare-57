using MarKit;
using MarTools;
using MarTools.AI;
using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(HealthBehavior), typeof(Rigidbody2D))]
public class NavigationBehavior : MarKitBehavior
{
    public float movementSpeed = 3;

    Rigidbody2D rb;
    HealthBehavior health;
    
    public Transform targetTransform { get; private set; }
    public Vector2 targetPosition { get; private set; }
    public float distanceToTarget => Vector3.Distance(targetPosition, transform.position);
    public Vector2 toTarget => targetPosition - (Vector2)transform.position;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<HealthBehavior>();

        targetPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (health.isDead) return;

        if (targetTransform) targetPosition = targetTransform.position;

        rb.linearVelocity = toTarget.normalized * movementSpeed;
    }

    public void FollowPlayer()
    {
        targetTransform = GameManager.Instance.player.transform;
    }

    internal void Dash(Vector2 vector2)
    {
        rb.AddForce(vector2 * rb.linearDamping, ForceMode2D.Impulse);
    }

    private void OnEnable()
    {
        FixedUpdate();
    }
}
