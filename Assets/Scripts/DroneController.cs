using Unity.Netcode;
using UnityEngine;

public class DroneController : NetworkBehaviour
{
    public float speed = 5f; // Movement speed
    public float liftForce = 10f; // Force for takeoff
    private Rigidbody rb;

    private bool isFlying = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    [ServerRpc(RequireOwnership = false)] // Allows clients to request this action
    public void TakeOffServerRpc()
    {
        if (!isFlying)
        {
            rb.AddForce(Vector3.up * liftForce, ForceMode.Impulse);
            isFlying = true;
            Debug.Log("Drone: Takeoff");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void LandServerRpc()
    {
        if (isFlying)
        {
            rb.velocity = Vector3.zero; // Stop all movement
            transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z); // Reset height
            isFlying = false;
            Debug.Log("Drone: Landed");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveServerRpc(string direction, float amount)
    {
        if (!isFlying) return;

        Vector3 movement = direction switch
        {
            "front" => Vector3.forward,
            "back" => Vector3.back,
            "left" => Vector3.left,
            "right" => Vector3.right,
            "up" => Vector3.up,
            "down" => Vector3.down,
            _ => Vector3.zero
        };

        rb.AddForce(movement * amount * speed, ForceMode.Force);
        Debug.Log($"Drone: Moving {direction}");
    }
}
