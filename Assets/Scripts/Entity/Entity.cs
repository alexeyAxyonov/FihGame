using UnityEngine;
using UnityEngine.AI;

public abstract class Entity : MonoBehaviour, IDamageable
{
    [SerializeField] protected int maxHealth = 100;
    protected int currentHealth;
    protected Animator animator;
    protected NavMeshAgent navAgent;
    protected bool isDead = false;

    public virtual int Health
    {
        get { return currentHealth; }
        set
        {
            if (value <= 0)
            {
                if (!isDead)
                {
                    Die();
                }
            }
            else
            {
                currentHealth = value;
            }
        }
    }
    public virtual void TakeDamage(int amount, Vector3 hitPoint, Vector3 hitNormal, float distance)
    {
        // TODO: modify this function
        Health -= amount;
        /* for bosses
        if (Health <= 0)
        {
            animator.SetTrigger("DIE");
        }
        else
        {
            animator.SetTrigger("DAMAGE");
        }
        */
    }

    public virtual void Heal(int amount)
    {
        if (Health + amount >= maxHealth)
        {
            Health = maxHealth;
        }
        else
        {
            Health += amount;
        }
    }

    protected virtual void Die()
    {
        isDead = true;
    }
}
