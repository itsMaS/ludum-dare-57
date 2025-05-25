using MarKit;
using MarTools;
using System;
using UnityEngine;

public class ProjectileLauncher : MarKitBehavior
{
    public enum InitialDirection
    {
        None,
        Player,
        Transform,
    }


    public BulletBehavior bulletPrefab;

    public Cooldown shootCooldown;
    public int numberOfBullets = 4;
    [Range(0,360)] public float angle = 90;
    public float angleOffset = 0;
    public float angleOffsetPerSecond = 0;
    public float angleOffsetPerShot = 0;

    public InitialDirection directionMode = InitialDirection.Transform;

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

    public void Shoot()
    {
        Vector3 startDirection = Vector3.up;
        if(directionMode == InitialDirection.Player)
        {
            startDirection = (GameManager.Instance.player.transform.position - transform.position).normalized;
        }
        else if(directionMode == InitialDirection.Transform)
        {
            startDirection = transform.up;
        }

        float baseAngle = Mathf.Atan2(startDirection.y, startDirection.x);

        for (int i = 0; i < numberOfBullets; i++)
        {
            float t = (float)i / (numberOfBullets - (numberOfBullets > 1 ? 1 : 0)); // Ensures full arc is covered
            float arc = Mathf.Deg2Rad * (angle != 360 ? angle : angle - 360/numberOfBullets);
            float offset = arc * (t - 0.5f) + angleOffset * Mathf.Deg2Rad; // Spread evenly around center (0 = forward)

            float finalAngle = baseAngle + offset;

            Vector2 direction = new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle));

            var bullet = bulletPrefab.SpawnFromPrefab(transform.position, direction);
        }

        angleOffset += angleOffsetPerShot; // If you still need to keep this mechanic
    }


    private void OnDrawGizmos()
    {
    }

    private void OnEnable()
    {
        if (shootCooldown.TryPerform())
        {
            Shoot();
        }
    }
}
