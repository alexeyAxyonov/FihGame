using UnityEngine;

public class HealZone : MonoBehaviour
{
    [SerializeField] private int healAmount = 15;
    private Zombie zombieParent;

    void Start()
    {
        zombieParent = GetComponentInParent<Zombie>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Debug.Log("Healing player for " + healAmount + " damage");
                damageable.Heal(healAmount);

                if (zombieParent != null)
                {
                    zombieParent.DestroyImmediately();
                }
            }
        }
    }
    /*
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 2f); // Heal radius //
        }
    */
}