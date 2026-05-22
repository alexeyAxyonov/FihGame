using UnityEngine;

public class ZombieIdleState : StateMachineBehaviour
{
    Transform player;

    public float detectionRadius = 18f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float distanceToPlayer = Vector3.Distance(animator.transform.position, player.position);
        if (distanceToPlayer <= detectionRadius)
        {
            animator.SetBool("isChasing", true);
        }
    }
}
