using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AmbusherEnemy : MonoBehaviour
{
    public float dropDistance = 5f;
    public float dropSpeed = 10f;

    private NavMeshAgent agent;
    private EnemyAI enemyAI;

    private bool activated = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAI = GetComponent<EnemyAI>();

        agent.enabled = false;
        enemyAI.enabled = false;
    }

    public void Activate()
    {
        if (activated)
            return;

        activated = true;

        StartCoroutine(DropAndActivate());
    }

    private IEnumerator DropAndActivate()
    {
        Vector3 targetPosition = transform.position + Vector3.down * dropDistance;

        while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                dropSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPosition;

        agent.enabled = true;
        enemyAI.enabled = true;
    }
}