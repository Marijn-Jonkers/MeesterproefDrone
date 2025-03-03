using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraArea : MonoBehaviour
{
    public float areaSize = 0.5f; // Size of the trigger zones
    public Alarm Alarm;
    public CameraManager CameraManager;
    private List<GameObject> cameraZones = new List<GameObject>();

    void Start()
    {
        CreateCameraZones();
    }

    public void RemoveCameraZones()
    {
        foreach (GameObject zone in cameraZones)
        {
            Destroy(zone);
        }
    }

    void CreateCameraZones()
    {
        Vector3[] corners = new Vector3[]
        {
            new Vector3(1, 0, 1),  // Front-right
            new Vector3(-1, 0, 1), // Front-left
            new Vector3(1, 0, -1), // Back-right
            new Vector3(-1, 0, -1) // Back-left
        };

        foreach (Vector3 corner in corners)
        {
            GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Cube); // Create visible cube
            zone.name = "CameraZone";
            zone.transform.SetParent(transform); // Attach to the cube
            zone.transform.localPosition = corner * 0.5f; // Position at corners
            zone.transform.localScale = new Vector3(areaSize, 1, areaSize); // Match size

            // Add trigger collider
            BoxCollider trigger = zone.AddComponent<BoxCollider>();
            trigger.isTrigger = true;

            // Add trigger script
            CameraAreaTrigger cat = zone.AddComponent<CameraAreaTrigger>();
            cat.Alarm = Alarm;
            cat.CA = gameObject.GetComponent<CameraArea>();
            cat.CM = CameraManager;

            // Make it semi-transparent green
            Renderer renderer = zone.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(0, 1, 0, 0.3f); // RGBA (green, 30% opacity)
            renderer.material.SetFloat("_Mode", 3); // Enable transparency
            renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            renderer.material.SetInt("_ZWrite", 0);
            renderer.material.DisableKeyword("_ALPHATEST_ON");
            renderer.material.EnableKeyword("_ALPHABLEND_ON");
            renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            renderer.material.renderQueue = 3000; // Transparent rendering layer

            cameraZones.Add(zone);
        }
    }
}
