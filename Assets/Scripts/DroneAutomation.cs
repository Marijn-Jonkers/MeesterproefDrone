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

    public Alarm alarmObject;
    private Coroutine takeOffCoroutine;


    LineRenderer laserLine;

    void Awake()
    {
        laserLine = GetComponent<LineRenderer>();
        laserLine.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (alarmObject.alarm)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(1, alarmObject.locatie);
            alarmObject.alarm = false;
            if (takeOffCoroutine == null)
            {
                takeOffCoroutine = StartCoroutine(TakeOff(alarmObject.locatie));
            }

        }
        laserLine.SetPosition(0, transform.position);
    }
    
    IEnumerator TakeOff(Vector3 target)
    {
        float deltaTime = takeOffHeight / maxSpeed;
        Vector3 deltaPosition = new Vector3(0, takeOffHeight, 0);
        float timePassed = 0;
        while (timePassed < deltaTime)
        {
            timePassed += Time.deltaTime;
            transform.position += deltaPosition * (Time.deltaTime/deltaTime);
            Debug.Log(timePassed);
            Vector3 direction = (target - transform.position).normalized;
            Vector3 drone = new 
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }

        takeOffCoroutine = null;
    }
}
