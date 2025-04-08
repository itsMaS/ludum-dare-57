using UnityEngine;

namespace MarKit
{
    public class SwayComponent : MonoBehaviour
    {
        [SerializeField, Range(0, 1)] float amount = 0.5f;

        Rigidbody2D rb;
        Quaternion rotationTarget;

        private void Awake()
        {
            rb = GetComponentInParent<Rigidbody2D>();
        }

        private void Update()
        {
            rotationTarget = Quaternion.LookRotation(Vector3.forward - (Vector3)rb.linearVelocity * amount, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotationTarget, Time.deltaTime * 10);
        }

    }
}
