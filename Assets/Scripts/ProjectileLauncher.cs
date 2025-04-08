using MarKit;
using MarTools;
using System;
using UnityEngine;

public class ProjectileLauncher : MarKitBehavior
{
    public BulletBehavior bulletPrefab;

    public Cooldown shootCooldown;
    public Vector2 direction { get; set; }
    public int radialSymmetry = 4;

    private void Start()
    {
        direction = transform.up;
    }

    private void Update()
    {
        if(enabled && shootCooldown.TryPerform())
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        for (int i = 0; i < radialSymmetry; i++)
        {
            float t = (float)i / radialSymmetry;
            direction = new Vector2(Mathf.Cos(Mathf.PI * 2 * t), Mathf.Sin(Mathf.PI*2*t));
            
            var bullet = bulletPrefab.SpawnFromPrefab(transform.position, direction);
            bullet.SetCollisionMask(Utilities.LayerMaskFromNames("Player"));
        }
    }
}
