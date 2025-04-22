using MarKit;
using MarTools;
using System;
using UnityEngine;

public class ProjectileLauncher : MarKitBehavior
{
    public BulletBehavior bulletPrefab;

    public Cooldown shootCooldown;
    public Vector2 direction { get; set; }
    public int numberOfBullets = 4;
    public float angle = 90;
    public float angleOffset = 0;
    public float angleOffsetPerSecond = 0;
    public float angleOffsetPerShot = 0;
    public bool aimTowardsPlayer = true;

    Vector2 deltaPosition;
    Vector2 previousPosition;



    private void Start()
    {
        direction = transform.up;
    }

    private void Update()
    {
        if(enabled)
        {
            if(shootCooldown.TryPerform())
            {
                Shoot();
            }

            angleOffset += angleOffsetPerSecond * Time.deltaTime;
        }
    }

    private void Shoot()
    {
        Vector2 toPlayer = GameManager.Instance.player.transform.position - transform.position;
        toPlayer.Normalize(); // Normalize to get a unit vector for consistent direction

        float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x);// + Mathf.PI; // Convert to angle in radians

        for (int i = 0; i < numberOfBullets; i++)
        {
            float t = (float)i / (numberOfBullets - 1); // Ensures full arc is covered
            float arc = Mathf.Deg2Rad * angle;
            float offset = arc * (t - 0.5f) + angleOffset * Mathf.Deg2Rad; // Spread evenly around center (0 = forward)

            float finalAngle = baseAngle + offset;

            Vector2 direction = new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle));

            var bullet = bulletPrefab.SpawnFromPrefab(transform.position, direction);
            bullet.SetCollisionMask(Utilities.LayerMaskFromNames("Player"));
        }

        angleOffset += angleOffsetPerShot; // If you still need to keep this mechanic
    }


    private void OnDrawGizmos()
    {
    }
}
