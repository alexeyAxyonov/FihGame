using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected Rigidbody rb;
    public bool isReflected = false;
    public virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Target"))
        {
            print("hit " + collision.gameObject.name + " !");
            Destroy(gameObject);
        }
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Call this from the parry zone to redirect the projectile
    public virtual void Reflect(Vector3 newDirection, float speedMultiplier = 1.5f)
    {
        if (rb == null) return;

        // Maintain current speed, or boost it
        float currentSpeed = rb.linearVelocity.magnitude;
        float newSpeed = Mathf.Max(currentSpeed, 10f) * speedMultiplier;

        rb.linearVelocity = newDirection.normalized * newSpeed;

        // Rotate to face the new direction
        transform.rotation = Quaternion.LookRotation(newDirection);

        isReflected = true;
    }
}
