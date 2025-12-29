using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowerSolver : MonoBehaviour
{
    private Rigidbody _rb;
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.solverIterations = 1;
        _rb.solverVelocityIterations = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
