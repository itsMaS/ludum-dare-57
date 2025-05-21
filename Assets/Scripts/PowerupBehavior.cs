using MarKit;
using UnityEngine;

public class PowerupBehavior : MarKitBehavior
{
    public MarKitEvent OnCollected;
    public MarKitEvent OnExpired;
    public float duration = 5;

    private PlayerController player;
    internal float timeLeftNormalized => timeLeft / duration;

    public bool expired { get; private set; }
    public float timeLeft { get; private set; }

    public string Title = "Unnamed Powerup";

    public void Collect(PlayerController playerController)
    {
        OnCollected.Invoke(this);
        player = playerController;
        timeLeft = duration;
    }

    private void LateUpdate()
    {
        if (!player) return;

        transform.position = player.transform.position;
        timeLeft -= Time.deltaTime;
    }

    public void Expire()
    {
        expired = true;
        OnExpired.Invoke(this);
        Destroy(gameObject, 0.5f);
    }
}
