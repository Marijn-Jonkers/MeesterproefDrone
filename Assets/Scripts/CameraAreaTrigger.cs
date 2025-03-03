using UnityEngine;

public class CameraAreaTrigger : MonoBehaviour
{
    public Alarm Alarm;
    public CameraArea CA;
    public CameraManager CM;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Target>() != null)
        {
            Alarm.locatie = gameObject.transform.position;
            Alarm.SetAlarm();
            CM.setActive(1);
            Debug.Log("Target entered camera area: " + gameObject.name);
            CA.RemoveAllCameraZones();
        }
    }
}
