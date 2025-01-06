using Unity.Netcode;
using UnityEngine;

public class CommandListener : MonoBehaviour
{
    public DroneController droneController;

    void Start()
    {
        // Listen for commands over the network
    }

    public void ProcessCommand(string command)
    {
        if (command == "takeoff")
        {
            droneController.TakeOffServerRpc();
        }
        else if (command == "land")
        {
            droneController.LandServerRpc();
        }
        else if (command.StartsWith("move"))
        {
            var parts = command.Split(' ');
            if (parts.Length == 3)
            {
                string direction = parts[1];
                if (float.TryParse(parts[2], out float amount))
                {
                    droneController.MoveServerRpc(direction, amount);
                }
            }
        }
        else
        {
            Debug.Log($"Unknown command: {command}");
        }
    }
}
