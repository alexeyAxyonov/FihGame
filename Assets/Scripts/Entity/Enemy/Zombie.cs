using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : Entity
{
    Transform player;
    int attackDamage = 20;
    private HealZone healZone;
    private Coroutine destructionCoroutine;

    void Start()
    {
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        healZone = GetComponentInChildren<HealZone>(true);
    }

    protected override void Die()
    {
        isDead = true;
        RagdollController ragdollController = GetComponent<RagdollController>();
        if (ragdollController != null)
        {
            print("Toggling ragdoll for " + gameObject.name);
            ragdollController.ToggleRagdoll(true);
        }

        if (healZone != null)
        {
            healZone.gameObject.SetActive(true);
            Debug.Log("HealZone set active");
        }
            
        destructionCoroutine = StartCoroutine(DestroyAfterDelay(5f));
    }
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
    public void DestroyImmediately()
    {
        if (destructionCoroutine != null)
        {
            StopCoroutine(destructionCoroutine);
            destructionCoroutine = null;
        }

        Destroy(gameObject);
    }

    public void DealDamage()
    {
        IDamageable damageable = player.GetComponent<IDamageable>();
        if (damageable != null)
        {
            Debug.Log("Damageable is found on player, dealing damage.");
            Vector3 hitDirection = (player.position - transform.position).normalized;
            damageable.TakeDamage(attackDamage, transform.position, hitDirection);
        }
        else
        {
            Debug.LogError("Player does not have an IDamageable component!");
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2.5f); // Attack radius //

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 40f); // Detection radius //
    }
}
