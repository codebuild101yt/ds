using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectSollver : MonoBehaviour
{
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.solverIterations = 2;
        rb.solverVelocityIterations = 1;
    }
}
