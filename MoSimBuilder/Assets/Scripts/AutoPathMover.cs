using System.Collections;
using UnityEngine;

/// <summary>
/// Makes the robot drive itself toward a target empty for the first 15 seconds of Auto,
/// using physics forces and torque so it obeys collisions, friction, and wheels.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class AutoPhysicsMover : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Assign the empty GameObject at the destination")]
    [SerializeField] private Transform targetPoint;

    [Header("Movement Settings")]
    [SerializeField] private float maxForwardForce = 50f;    // Forward force applied
    [SerializeField] private float maxTurnTorque = 30f;      // Torque for turning
    [SerializeField] private float arriveDistance = 0.2f;    // Stop when this close

    private Rigidbody rb;
    private bool autoActive = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (targetPoint == null)
        {
            Debug.LogError("AutoPhysicsMover: targetPoint not assigned!");
            enabled = false;
            return;
        }

        StartCoroutine(AutoRoutine());
    }

    private IEnumerator AutoRoutine()
    {
        // Wait for GameManager and Auto start
        while (GameManager.Instance == null || GameManager.GameState != GameState.Auto)
            yield return null;

        // Activate AI override
        GameManager.SetAutoMotionOverride(true);
        autoActive = true;

        float elapsed = 0f;

        while (elapsed < 15f && autoActive)
        {
            elapsed += Time.fixedDeltaTime;

            // Vector to target
            Vector3 toTarget = targetPoint.position - transform.position;
            float distance = toTarget.magnitude;

            // Stop if close
            if (distance <= arriveDistance)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                break;
            }

            // Forward movement along robot's local Z
            Vector3 localDir = transform.InverseTransformDirection(toTarget.normalized);
            float forward = Mathf.Clamp(localDir.z, 0f, 1f);

            // Apply forward force
            Vector3 force = transform.forward * forward * maxForwardForce;
            rb.AddForce(force, ForceMode.Force);

            // Turning: simple proportional control
            float targetAngle = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
            float currentYaw = transform.eulerAngles.y;
            float angleDiff = Mathf.DeltaAngle(currentYaw, targetAngle);

            float torque = Mathf.Clamp(angleDiff, -maxTurnTorque, maxTurnTorque);
            rb.AddTorque(Vector3.up * torque, ForceMode.Force);

            yield return new WaitForFixedUpdate();
        }

        // Stop motion
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Release AI override
        GameManager.SetAutoMotionOverride(false);
    }
}
