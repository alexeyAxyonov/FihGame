using UnityEngine;
using UnityEngine.AI;

public abstract class Entity : MonoBehaviour, IDamageable
{
    [SerializeField] protected int maxHealth = 100;
    protected int currentHealth;
    protected Animator animator;
    protected NavMeshAgent navAgent;
    protected bool isDead = false;

    // BUG FIX: currentHealth was never initialized, causing instant death on scene load.
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual int Health
    {
        get { return currentHealth; }
        set
        {
            // BUG FIX: the old setter never wrote currentHealth when value <= 0,
            // so currentHealth stayed at its last positive value while the entity was dead.
            // Now we clamp to 0 and always keep currentHealth accurate.
            currentHealth = Mathf.Max(value, 0);

            if (currentHealth <= 0)
            {
                if (!isDead)
                {
                    Die();
                }
            }
        }
    }

    public virtual void TakeDamage(int amount, Vector3 hitPoint, Vector3 hitNormal, float distance)
    {
        Health -= amount;
    }

    public virtual void Heal(int amount)
    {
        Health = Mathf.Min(currentHealth + amount, maxHealth);
    }

    protected virtual void Die()
    {
        isDead = true;
    }
}
