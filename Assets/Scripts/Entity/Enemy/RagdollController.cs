using UnityEngine;

public class RagdollController : MonoBehaviour, IKnockbackable
{
    [Tooltip("Layer that ragdoll bones are moved to on death. Corpses on this layer pass " +
             "through each other and the living instead of tangling. Leave at 10 unless that " +
             "layer is already used; optionally name it \"Ragdoll\" in Project Settings > Tags and Layers.")]
    [SerializeField] private int ragdollLayer = 10;

    private Animator animator;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;

    // The corpse collision rules are global, so configure them once for the whole session.
    private static bool _collisionsConfigured = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        ConfigureRagdollCollisions();
        ToggleRagdoll(false);
    }

    // Make the Ragdoll layer not collide with itself (so corpses don't tangle/stick) or with
    // the living (Enemy/Player/Weapon). It still collides with the environment, so corpses
    // rest on the floor. Global and one-time.
    private void ConfigureRagdollCollisions()
    {
        if (_collisionsConfigured) return;
        if (ragdollLayer < 0 || ragdollLayer > 31) return;
        _collisionsConfigured = true;

        Physics.IgnoreLayerCollision(ragdollLayer, ragdollLayer, true);

        int enemy  = LayerMask.NameToLayer("Enemy");
        int player = LayerMask.NameToLayer("Player");
        int weapon = LayerMask.NameToLayer("Weapon");
        if (enemy  >= 0) Physics.IgnoreLayerCollision(ragdollLayer, enemy,  true);
        if (player >= 0) Physics.IgnoreLayerCollision(ragdollLayer, player, true);
        if (weapon >= 0) Physics.IgnoreLayerCollision(ragdollLayer, weapon, true);
    }

    public void ToggleRagdoll(bool isRagdoll)
    {
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb.gameObject == gameObject) continue;

            rb.isKinematic = !isRagdoll;
            rb.useGravity = isRagdoll;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col.gameObject == gameObject)
            {
                // Root collider: on while alive (so rockets/bullets can hit the enemy),
                // off once ragdolling so a leftover capsule can't block or shove the living.
                col.enabled = !isRagdoll;
                continue;
            }

            col.enabled = isRagdoll;

            // Move active bones onto the Ragdoll layer so corpses don't collide with each
            // other (or with the living) and get stuck inside one another.
            if (isRagdoll && ragdollLayer >= 0 && ragdollLayer <= 31)
                col.gameObject.layer = ragdollLayer;
        }

        animator.enabled = !isRagdoll;

        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = !isRagdoll;
    }

    // Knockback from explosions etc. Only ragdolled (non-kinematic) bones react, so this
    // affects enemies that have actually died/ragdolled. VelocityChange ignores per-bone
    // mass so the launch strength is consistent regardless of the rig's masses.
    public void ApplyKnockback(Vector3 velocity)
    {
        if (ragdollRigidbodies == null) return;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb == null || rb.gameObject == gameObject) continue;
            if (rb.isKinematic) continue;
            rb.AddForce(velocity, ForceMode.VelocityChange);
        }
    }
}
