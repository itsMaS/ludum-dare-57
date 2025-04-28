using MarKit;
using MarTools;
using MarTools.AI;
using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(GameEntityBehavior), typeof(Rigidbody2D))]
public class EnemyBehavior : MarKitBehavior
{
    Rigidbody2D rb;
    PlayerController target;
    Animator an;

    public Vector2 inertia { get; set; }
    public Vector2 toMovementTarget => movementTarget.position - transform.position;

    private VisionBehavior2D detection;
    private GameEntityBehavior entity;

    public Transform movementTarget;
    public float movementSpeed = 5;

    public int score = 10;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        detection = GetComponent<VisionBehavior2D>();
        an = GetComponentInChildren<Animator>();

        entity = GetComponent<GameEntityBehavior>();

        entity.OnDied.AddListener(Died);
    }

    private void Died()
    {
        GameManager.Instance.AddScore(score, transform.position);
    }

    private void FixedUpdate()
    {
        inertia = Vector3.MoveTowards(inertia, Vector3.zero, Time.fixedDeltaTime * 20);
        rb.linearVelocity = inertia;

        if(movementTarget)
        {
            Vector2 movement = toMovementTarget.normalized * movementSpeed;
            rb.linearVelocity += movement;
        }
    }

    public void SetMovementTarget(Transform target)
    {
        movementTarget = target;
    }

    public void MoveTowardsPlayer()
    {
        movementTarget = FindObjectOfType<PlayerController>().transform;
    }

    public void ClearMovementTarget()
    {
        movementTarget = null;
    }
}
