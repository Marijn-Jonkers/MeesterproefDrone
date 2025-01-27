using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alarm : MonoBehaviour
{
    public bool alarm = false;
    public Vector3 locatie;

    private void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            locatie = hit.point;
            Debug.Log("Clicked location: " + locatie);

            SetAlarm();
        }
        else
        {
            Debug.Log("No object hit.");
        }
    }
    public void SetAlarm()
    {
        alarm = true;
        Debug.Log("Het alarm is afgezet op " + locatie);
    }
    public void ResetAlarm()
    {
        alarm = false;
        Debug.Log("Het alarm is uit gezet");
    }
}
