using Unity.Netcode;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public GameObject dronePrefab; // Assign your drone prefab here

    void Start()
    {
        // Start the server
        NetworkManager.Singleton.StartServer();
        Debug.Log("Server started");

        // Wait until the server is started and then spawn the drone
        Invoke("SpawnDrone", 1f);  // Slight delay to ensure the server is ready
    }

    private void SpawnDrone()
    {
        var drone = Instantiate(dronePrefab, Vector3.zero, Quaternion.identity);
        drone.GetComponent<NetworkObject>().Spawn();  // Spawn it over the network
        Debug.Log("Drone spawned");
    }
}
