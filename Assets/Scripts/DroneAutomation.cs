using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(LineRenderer))]
public class DroneAutomation : MonoBehaviour
{
    public float takeOffHeight = 120.0f;
    public float maxSpeed = 18.1f;
    public float rotationSpeed = 2.0f;
    public GameObject droneCamera;

    public Alarm alarmObject;
    public CameraManager CM;
    public Security SG;

    private Coroutine trackingCoroutine;
    private Coroutine takeOffCoroutine;
    private Coroutine flyToTargetCoroutine;
    private Coroutine orbitCoroutine;

    private Plane[] frustumPlanes;
    private Camera droneCameraComponent;

    public float UIdist;
    public UIManager UI;



    LineRenderer laserLine;

    void Awake()
    {
        laserLine = GetComponent<LineRenderer>();
        laserLine.enabled = false;
        droneCameraComponent = droneCamera.GetComponent<Camera>();
        frustumPlanes = GeometryUtility.CalculateFrustumPlanes(droneCameraComponent);

    }

    // Update is called once per frame
    private Coroutine droneRoutine; // Single coroutine for sequential execution

    void Update()
    {
        if (alarmObject.alarm)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(1, alarmObject.locatie);
            alarmObject.alarm = false;

            if (droneRoutine == null)
            {
                droneRoutine = StartCoroutine(DroneSequence(alarmObject.locatie));
                StartCoroutine(CheckForTarget());
            }
        }
        laserLine.SetPosition(0, transform.position);
    }

    IEnumerator DroneSequence(Vector3 target)
    {
        // Start TakeOff and wait for it to complete
        yield return StartCoroutine(TakeOff(target));

        // Start FlyToTarget only after TakeOff is done
        yield return StartCoroutine(FlyToTarget(target));

        // Reset the coroutine reference
        droneRoutine = null;
    }


    IEnumerator TakeOff(Vector3 target)
    {
        float deltaTime = takeOffHeight / maxSpeed;
        Vector3 deltaPosition = new Vector3(0, takeOffHeight - transform.position.y, 0);
        float timePassed = 0;
        float distance = Vector3.Distance(new Vector3(target.x, 0, target.z), new Vector3(transform.position.x, 0, transform.position.z));
        UIdist = distance;

        while (timePassed < deltaTime)
        {
            UI.Message("Alarm", "Op zoek naar potentieel doelwit");
            timePassed += Time.deltaTime;
            transform.position += deltaPosition * (Time.deltaTime / deltaTime);

            // Compute direction towards the target
            Vector3 direction = (target - transform.position).normalized;

            // Ensure only Y-axis rotation by projecting direction onto XZ plane
            direction.y = 0;

            // Rotate the drone only on the Y-axis
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            // Rotate the camera to fully look at the target (all axes)
            Vector3 cameraDirection = (target - droneCamera.transform.position).normalized;
            Quaternion cameraRotation = Quaternion.LookRotation(cameraDirection);
            droneCamera.transform.rotation = Quaternion.Slerp(droneCamera.transform.rotation, cameraRotation, Time.deltaTime * rotationSpeed);

            yield return null;
        }

        takeOffCoroutine = null;
    }
    IEnumerator FlyToTarget(Vector3 target)
    {
        float distance = Vector3.Distance(new Vector3(target.x, 0, target.z), new Vector3(transform.position.x, 0, transform.position.z));
        float deltaTime = distance / maxSpeed;
        Vector3 startPosition = transform.position;
        float timePassed = 0;
        UIdist = distance;

        while (timePassed < deltaTime)
        {
            timePassed += Time.deltaTime;
            float t = timePassed / deltaTime;
            transform.position = Vector3.Lerp(startPosition, new Vector3(target.x, transform.position.y, target.z), t);

            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0; // Keep rotation in XZ plane

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            yield return null;
        }

        // Now start orbiting after reaching the target
        orbitCoroutine = StartCoroutine(OrbitPoint(target));

        flyToTargetCoroutine = null; // Cleanup
    }

    IEnumerator CheckForTarget()
    {
        while (true) // Continuously check for targets
        {
            frustumPlanes = GeometryUtility.CalculateFrustumPlanes(droneCameraComponent); // Update camera frustum

            Target[] targets = FindObjectsOfType<Target>(); // Get all targets

            foreach (Target target in targets)
            {
                Renderer targetRenderer = target.GetComponent<Renderer>();
                if (targetRenderer != null)
                {
                    bool isVisible = GeometryUtility.TestPlanesAABB(frustumPlanes, targetRenderer.bounds);

                    if (isVisible)
                    {
                        // Perform a Raycast to ensure there's no obstacle between the camera and the target
                        Vector3 directionToTarget = (target.transform.position - droneCamera.transform.position).normalized;
                        float distanceToTarget = Vector3.Distance(droneCamera.transform.position, target.transform.position);
                        UIdist = distanceToTarget;

                        RaycastHit hit;
                        if (Physics.Raycast(droneCamera.transform.position, directionToTarget, out hit, distanceToTarget))
                        {
                            // If the hit object is not the target, then something is blocking the view
                            if (hit.collider.gameObject != target.gameObject || distanceToTarget > 400f)
                            {
                                Debug.Log("Target " + target.gameObject.name + " is blocked by " + hit.collider.gameObject.name);
                                continue; // Skip this target since it's not visible
                            }
                        }

                        Debug.Log("Target in view: " + target.gameObject.name);

                        // Stop all coroutines before tracking
                        StopAllCoroutines();

                        // Start tracking only one target
                        trackingCoroutine = StartCoroutine(TrackTarget(target.gameObject));

                        // Exit the function to prevent checking for more targets
                        yield break;
                    }
                }
            }

            yield return new WaitForSeconds(0.2f);
        }
    }


    IEnumerator TrackTarget(GameObject target)
    {
        if (trackingCoroutine != null) yield break; // Prevent multiple tracking coroutines

        trackingCoroutine = StartCoroutine(TrackTargetRoutine(target));
        SG.goForIt = true;
        CM.setActive(3);
        UI.Message("Doelwit gevonden", "Achtervolging doelwit geinitialiseerd");
    }

    IEnumerator TrackTargetRoutine(GameObject target)
    {
        while (true)
        {
            float distance = Vector3.Distance(new Vector3(target.transform.position.x, 10, target.transform.position.z), transform.position);
            float deltaTime = distance / maxSpeed;
            Vector3 targetPosition = new Vector3(target.transform.position.x, 10, target.transform.position.z);
            float timePassed = 0;

            while (timePassed < deltaTime)
            {
                distance = Vector3.Distance(new Vector3(target.transform.position.x, 10, target.transform.position.z), transform.position);
                UIdist = distance;
                deltaTime = distance / maxSpeed;
                targetPosition = new Vector3(target.transform.position.x, 10, target.transform.position.z);
                timePassed += Time.deltaTime;
                transform.position += (targetPosition - transform.position) * (Time.deltaTime / deltaTime);

                Vector3 direction = (target.transform.position - transform.position).normalized;

                // Ensure only Y-axis rotation by projecting direction onto XZ plane
                direction.y = 0;

                // Rotate the drone only on the Y-axis
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                }
                // Make the drone and camera look at the target
                Vector3 cameraDirection = (target.transform.position - droneCamera.transform.position).normalized;
                Quaternion cameraRotation = Quaternion.LookRotation(cameraDirection);
                droneCamera.transform.rotation = Quaternion.Slerp(droneCamera.transform.rotation, cameraRotation, Time.deltaTime * rotationSpeed);


                yield return null;
            }

            yield return null;
        }
    }
    IEnumerator OrbitPoint(Vector3 target)
    {
        float targetSpeed = 6f; // Adjust as needed
        Vector3 orbitCenter = new Vector3(target.x, takeOffHeight, target.z);

        while (true) // Continue orbiting until the target is found
        {
            // Calculate the radius ONCE at the start of the orbit
            float radius = (targetSpeed * Time.timeSinceLevelLoad); // R = v*t + 15
            float angularSpeed = Mathf.Sqrt((maxSpeed * maxSpeed) - (targetSpeed * targetSpeed)) / radius;
            float orbitDuration = (2 * Mathf.PI) / angularSpeed; // Time for one full orbit

            Vector3 startOrbitPosition = orbitCenter + new Vector3(radius, 0, 0);
            yield return StartCoroutine(FlyToOrbitStart(startOrbitPosition));

            Debug.Log($"Starting new orbit with radius: {radius}");

            float elapsedTime = 0f;
            float theta = 0f; // Start angle

            while (elapsedTime < orbitDuration)
            {
                elapsedTime += Time.deltaTime;
                theta += angularSpeed * Time.deltaTime; // Circular motion progression

                float distance = Vector3.Distance(new Vector3(target.x, takeOffHeight, target.z), transform.position);
                UIdist = distance;

                // Compute new position using the constant radius
                float x = radius * Mathf.Cos(theta) + orbitCenter.x;
                float z = radius * Mathf.Sin(theta) + orbitCenter.z;
                Vector3 newPosition = new Vector3(x, orbitCenter.y, z);

                // Move the drone
                transform.position = newPosition;

                // Make the drone face the target
                Vector3 direction = (orbitCenter - transform.position).normalized;
                direction.y = 0; // Lock rotation to Y-axis
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                }

                // Exit orbit if the target is found
                if (trackingCoroutine != null)
                {
                    Debug.Log("Target found, exiting orbit...");
                    yield break;
                }

                yield return null;
            }

            // After one full orbit, the while loop restarts and recalculates the radius.
        }
    }

    IEnumerator FlyToOrbitStart(Vector3 orbitStart)
    {
        float distance = Vector3.Distance(transform.position, orbitStart);
        float travelTime = distance / maxSpeed;
        float timePassed = 0;

        Vector3 startPosition = transform.position;

        while (timePassed < travelTime)
        {
            timePassed += Time.deltaTime;
            float t = timePassed / travelTime;
            transform.position = Vector3.Lerp(startPosition, orbitStart, t);

            // Rotate towards the orbit start point
            Vector3 direction = (orbitStart - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            yield return null;
        }
    }

}
