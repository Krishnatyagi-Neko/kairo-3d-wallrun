using UnityEngine;
using UnityEngine.AI;

public class BlockerEnemy : MonoBehaviour
{
    public Transform patrolPointA;
    public Transform patrolPointB;

    private NavMeshAgent agent;
    private Transform currentTarget;

    public float detectionRange = 8f;
    public Transform player;

    private bool chasingPlayer = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        currentTarget = patrolPointA;
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distanceToPlayer <= detectionRange)
        {
            chasingPlayer = true;
        }
        else
        {
            chasingPlayer = false;
        }

        if (chasingPlayer)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.SetDestination(currentTarget.position);

            if (Vector3.Distance(transform.position, currentTarget.position) < 1f)
            {
                if (currentTarget == patrolPointA)
                {
                    currentTarget = patrolPointB;
                }
                else
                {
                    currentTarget = patrolPointA;
                }
            }
        }
    }
}