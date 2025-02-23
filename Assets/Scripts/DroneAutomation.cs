using System.Collections;
using System.Collections.Generic;
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

    private Coroutine trackingCoroutine;
    private Coroutine takeOffCoroutine;
    private Coroutine flyToTargetCoroutine;

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
        Vector3 deltaPosition = new Vector3(0, takeOffHeight, 0);
        float timePassed = 0;

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
        Vector3 deltaPosition = new Vector3(target.x, 0, target.z);
        float timePassed = 0;

        while (timePassed < deltaTime)
        {
            timePassed += Time.deltaTime;
            transform.position += deltaPosition * (Time.deltaTime / deltaTime);

            Vector3 direction = (target - transform.position).normalized;
            Vector3 cameraDirection = (target - droneCamera.transform.position).normalized;
            Quaternion cameraRotation = Quaternion.LookRotation(cameraDirection);
            droneCamera.transform.rotation = Quaternion.Slerp(droneCamera.transform.rotation, cameraRotation, Time.deltaTime * rotationSpeed);

            UIdist = Vector3.Distance(new Vector3(target.x, 0, target.z), new Vector3(transform.position.x, 0, transform.position.z));

            yield return null;
        }

        flyToTargetCoroutine = null;
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

                        RaycastHit hit;
                        if (Physics.Raycast(droneCamera.transform.position, directionToTarget, out hit, distanceToTarget))
                        {
                            // If the hit object is not the target, then something is blocking the view
                            if (hit.collider.gameObject != target.gameObject)
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


}
