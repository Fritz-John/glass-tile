using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBall : MonoBehaviour
{
    public float forceMagnitude = 10f; // Adjust the force strength as needed

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
       
        Vector3 forceDirection = Vector3.forward;
        rb.AddForce(forceDirection * forceMagnitude, ForceMode.VelocityChange);

    }
}
