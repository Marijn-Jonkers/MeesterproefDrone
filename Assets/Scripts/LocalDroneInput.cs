using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PlayerInput))]

public class LocalDroneInput : MonoBehaviour
{
    private Vector2 cyclic;
    private float pedals;
    private float throttle;
    private float takeoff;
    private float land;

    public Vector2 Cyclic { get => cyclic; }
    public float Pedals { get => pedals; }
    public float Throttle { get => throttle; }
    public float Takeoff { get => takeoff;}
    public float Land { get => land;}

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnCyclic(InputValue value)
    {
        cyclic = value.Get<Vector2>();
    }
    private void OnPedals(InputValue value)
    {
        pedals = value.Get<float>();
    }
    private void OnThrottle(InputValue value)
    {
        throttle = value.Get<float>();
    }
    private void OnTakeoff(InputValue value)
    {
        takeoff = value.Get<float>();
    }
    private void OnLand(InputValue value)
    {
        land = value.Get<float>();
    }

}
