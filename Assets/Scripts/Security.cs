using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Security : MonoBehaviour
{
    public Transform drone; // Assign in the Inspector
    public bool goForIt = false;
    public float checkInterval = 0.5f; // How often to update path
    public bool Finish = false; // New boolean to check when reaching a target
    public GameObject car;
    public GameObject body;

    private NavMeshAgent agent;
    private Coroutine runCoroutine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (!agent.isOnNavMesh)
        {
            Debug.LogError(gameObject.name + " is NOT on the NavMesh!");
            car.SetActive(false);
            body.SetActive(true);
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
    void ToggleCar(bool toggle)
    {
        if (toggle)
        {
            car.SetActive(true);
            body.SetActive(false);
            agent.speed = 30f;
            agent.acceleration = 15f;
        } else
        {
            car.SetActive(false);
            body.SetActive(true);
            agent.speed = 10f;
            agent.acceleration = 8f;
        }
    }
    IEnumerator RunTowardsDrone()
    {
        while (true)
        {
            Vector3 newTarget = drone.position;
            if (goForIt && drone != null)
            {

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
            float distance = Vector3.Distance(transform.position, newTarget);
            if (distance < 40f)
            {
                ToggleCar(false);
            } else
            {
                ToggleCar(true);
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
