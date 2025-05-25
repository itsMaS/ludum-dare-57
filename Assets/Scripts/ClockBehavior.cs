using MarKit;
using MarTools;
using UnityEngine;

public class ClockBehavior : MarKitBehavior
{
    public MarKitEvent OnTick;
    public Cooldown clockCooldown;

    private void Update()
    {
        if(clockCooldown.TryPerform())
        {
            OnTick.Invoke(this);
        }
    }
}
