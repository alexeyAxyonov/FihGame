using UnityEngine;

public class RagdollController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;

    void Start()
    {
        animator = GetComponent<Animator>();
        // Get all rigidbodies and colliders from the ragdoll bones
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        // Deactivate ragdoll so the Animator is in control
        ToggleRagdoll(false);
    }

    public void ToggleRagdoll(bool isRagdoll)
    {
        // Enable or disable the physics components on the bones
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            // Skip the main character's rigidbody if it has one
            if (rb.gameObject == gameObject) continue;

            rb.isKinematic = !isRagdoll;
            rb.useGravity = isRagdoll;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col.gameObject == gameObject) continue;
            col.enabled = isRagdoll;
        }

        // Disable the Animator to let physics take over
        animator.enabled = !isRagdoll;

        // Optionally, disable the main character controller or collider
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = !isRagdoll;
    }
}