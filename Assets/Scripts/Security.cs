using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Security : MonoBehaviour
{
    public Transform drone; // Assign in the Inspector
    public bool goForIt = false;
    public float checkInterval = 0.5f; // How often to update path
    public bool Finish = false; // New boolean to check when reaching a target

    private NavMeshAgent agent;
    private Coroutine runCoroutine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (!agent.isOnNavMesh)
        {
            Debug.LogError(gameObject.name + " is NOT on the NavMesh!");
            return;
        }

        runCoroutine = StartCoroutine(RunTowardsDrone());
    }

    void Update()
    {
        if (!agent.isOnNavMesh)
        {
            Debug.LogError(gameObject.name + " is NOT on the NavMesh!");
        }
    }

    IEnumerator RunTowardsDrone()
    {
        while (true)
        {
            if (goForIt && drone != null)
            {
                Vector3 newTarget = drone.position;

                // Try to find a point on the NavMesh near the drone
                if (NavMesh.SamplePosition(newTarget, out NavMeshHit hit, 120.0f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
                else
                {
                    Debug.LogWarning("No valid NavMesh position near the drone.");
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }


    public void StartChasing()
    {
        goForIt = true;
    }

    public void StopChasing()
    {
        goForIt = false;
        agent.ResetPath();
    }

    private void OnCollisionEnter(Collision other)  // Change Collider to Collision
{
    if (other.gameObject.GetComponent<Target>() != null)
    {
        Debug.Log("Security reached a target!");

        Finish = true;
        
        if (Finish)  
        {
            Debug.Log("Finish is true! Stopping the chase.");
            StopChasing();
        }
    }
}
}
