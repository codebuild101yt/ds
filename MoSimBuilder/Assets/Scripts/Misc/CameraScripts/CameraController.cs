using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Alliance alliance;
    [SerializeField] private bool isSecondaryCam;

    public Rigidbody rb;
    public float moveSpeed = 0f;

    private Vector2 translateValue;
    private Vector3 startingDirection;
    private Vector3 startingRotation;

    public DriveController robot;

    void Start()
    {
        startingDirection = transform.forward;
        startingRotation = transform.right;

        robot = GetEnabledTarget().gameObject.GetComponent<DriveController>();
    }

    void FixedUpdate()
    {

        translateValue = robot.dsMove;

        if (translateValue == null)
        {
            translateValue = new Vector2();
        }

        if (Math.Abs(translateValue.magnitude) > 0f)
        {
            CameraPan.parentMoving = true;
        }
        else
        {
            CameraPan.parentMoving = false;
        }

        Vector3 moveDirection = startingDirection * translateValue.y + startingRotation * translateValue.x;

        rb.centerOfMass = new Vector3(0, -4, 0);
        rb.AddForceAtPosition(moveDirection * moveSpeed, transform.localPosition-new Vector3(0,4.0f,0));
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        translateValue = ctx.ReadValue<Vector2>();
        
    }

    private Transform GetEnabledTarget()
    {
        if (alliance == Alliance.Blue)
        {
            if (GameObject.FindGameObjectWithTag("Player2") != null || GameObject.FindGameObjectWithTag("Player") != null)
            {
                return isSecondaryCam ? GameObject.FindGameObjectWithTag("Player2").transform : GameObject.FindGameObjectWithTag("Player").transform;
            } else
            {
                return null;
            }
        }
        if (GameObject.FindGameObjectWithTag("RedPlayer2") != null || GameObject.FindGameObjectWithTag("RedPlayer") != null)
        {
            return isSecondaryCam ? GameObject.FindGameObjectWithTag("RedPlayer2").transform : GameObject.FindGameObjectWithTag("RedPlayer").transform;
        } else
        {
            return null;
        }
    }

    public void Restart()
    {
        robot = GetEnabledTarget().gameObject.GetComponent<DriveController>();
    }
}