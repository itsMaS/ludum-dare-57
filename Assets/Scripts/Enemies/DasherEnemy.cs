using MarKit;
using MarTools;
using MarTools.AI;
using UnityEngine;

[RequireComponent(typeof(NavigationBehavior), typeof(VisionBehavior2D))]
public class DasherEnemy : StateMachineBehavior, IMarkitEventCaller
{
    [System.Serializable]
    public class Normal : State<DasherEnemy>
    {
        public float dashRange = 0.5f;
        protected override void OnUpdateState(DasherEnemy b, float deltaTime)
        {
            base.OnUpdateState(b, deltaTime);

            b.nav.enabled = true;

            debugInfo = $"Distance to target: {b.nav.distanceToTarget}";
            if(b.vis.Visible.Count > 0 && b.nav.distanceToTarget <= dashRange)
            {
                b.ChangeState(b.dashState);
            }

        }
    }

    [System.Serializable]
    public class Dash : State<DasherEnemy>
    {
        public float dashStrength = 20;
        public float dashStunDuration = 1f;
        public float dashChargeDuration = 0.25f;

        protected override void OnEnterState(DasherEnemy b, IState previousState)
        {
            base.OnEnterState(b, previousState);

            b.OnChargeStart.Invoke(b);
            b.nav.enabled = false;
            Vector3 playerDirection = b.nav.toTarget.normalized;


            b.DelayedAction(dashChargeDuration, () =>
            {
                b.nav.Dash(playerDirection * dashStrength);
                b.OnDash.Invoke(b);

                b.DelayedAction(dashStunDuration, () =>
                {
                    b.ChangeState(b.normalState);
                    b.OnRested.Invoke(b);
                });
            });

        }

        protected override void OnUpdateState(DasherEnemy b, float deltaTime)
        {
            base.OnUpdateState(b, deltaTime);
        }
    }


    NavigationBehavior nav;
    VisionBehavior2D vis;

    public Normal normalState;
    public Dash dashState;

    public MarKitEvent OnChargeStart;
    public MarKitEvent OnDash;
    public MarKitEvent OnRested;


    protected override void Awake()
    {
        base.Awake();
        nav = GetComponent<NavigationBehavior>();
        vis = GetComponent<VisionBehavior2D>();

        ChangeState(normalState);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, normalState.dashRange);
    }
}
