using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alarm : MonoBehaviour
{
    public bool alarm = false;
    public Vector3 locatie;
    public GameObject target;
    public GameObject TopCam;
    public GameObject UI;

    private void Start()
    {
        UI.SetActive(false);
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                locatie = hit.point;
                Debug.Log("Clicked location: " + locatie);

                // Set the alarm when clicking on a valid object
                SetAlarm();
            }
            else
            {
                Debug.Log("No object hit.");
            }
        }
    }

    public void SetAlarm()
    {
        alarm = true;
        TopCam.SetActive(false);
        UI.SetActive(true);
        Debug.Log("Het alarm is afgezet op " + locatie);
    }

    public void ResetAlarm()
    {
        alarm = false;
        TopCam.SetActive(true);
        UI.SetActive(false);
        Debug.Log("Het alarm is uit gezet");
    }
}
