using System.Collections;
using UnityEngine;
using UnityEngine.AI;  // Required for NavMesh

public class Target : MonoBehaviour
{
    public Transform drone; // Assign the drone in the Inspector
    public float safeDistance = 150f; // Desired distance from the drone
    public float runDistance = 50f;   // Distance at which AI starts running
    public float checkInterval = 0.5f; // How often to check movement

    private NavMeshAgent agent;
    private Coroutine runCoroutine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        runCoroutine = StartCoroutine(RunAwayFromDrone());
    }

    void Update()
    {
        if (!agent.isOnNavMesh)
        {
            Debug.LogError(gameObject.name + " is NOT on the NavMesh!");
        }
    }

    IEnumerator RunAwayFromDrone()
    {
        while (true) // Keep checking every interval
        {
            if (drone == null) yield break; // Exit if no drone is assigned

            float distance = Vector3.Distance(transform.position, drone.position);

            // Only run if the drone is within the "runDistance"
            if (distance < runDistance)
            {
                Vector3 runDirection = (transform.position - drone.position).normalized;
                Vector3 newTarget = transform.position + runDirection * safeDistance;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(newTarget, out hit, safeDistance, NavMesh.AllAreas)) // Find closest valid position
                {
                    agent.SetDestination(hit.position);
                }
            }

            yield return new WaitForSeconds(checkInterval); // Wait before checking again
        }
    }
}
