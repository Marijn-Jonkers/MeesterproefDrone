using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BaseRigidbody : MonoBehaviour
{
    [Header("Rigidbody Properties")]
    [SerializeField] private float weight = 0.42f;

    protected Rigidbody rb;
    protected float startDrag;
    protected float startAngularDrag;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.mass = weight;
            startDrag = rb.drag;
            startAngularDrag = rb.angularDrag;
        }
    }

    void FixedUpdate()
    {
        if (!rb)
        {
            return;
        }
        HandlePhysics();
    }

    protected virtual void HandlePhysics() { }
}
