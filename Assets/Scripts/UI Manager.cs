using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject drone;
    private DroneAutomation droneAuto;


    public TMP_Text alt;
    public TMP_Text dist;

    public GameObject MessObj;
    public TMP_Text title;
    public TMP_Text subscr;

    public GameObject Compass;
    public TMP_Text Rot;

    // Start is called before the first frame update
    void Start()
    {
        droneAuto = drone.GetComponent<DroneAutomation>();
        MessObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Altitude();
        Distance();
        Rotation();
    }

    private void Altitude()
    {
        alt.text = "Altitude: " + (Mathf.Round(drone.transform.position.y * 10.0f) * 0.1f).ToString() + "m";
    }
    private void Distance()
    {
        dist.text = "Distance: " + (Mathf.Round(droneAuto.UIdist * 10.0f) * 0.1f).ToString() + "m";
    }

    public void Message(string Title, string Message)
    {
        MessObj.SetActive(true);
        title.text = Title;
        subscr.text = Message;
    }

    public void Rotation()
    {
        Rot.text = Mathf.Round(drone.transform.eulerAngles.y).ToString() + "°";
        Compass.transform.rotation = Quaternion.Euler(0, 0, -drone.transform.eulerAngles.y);
    }
}
