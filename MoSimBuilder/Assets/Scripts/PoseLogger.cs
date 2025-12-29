using System.IO;
using UnityEngine;

public class PoseLogger : MonoBehaviour
{
    [Tooltip("Assign an empty GameObject to define field origin")]
    public Transform origin; // empty GameObject as field origin

    private StreamWriter writer;
    private string filePath;

    public float logRate = 0.05f; // 20 Hz
    private float logTimer = 0f;

    void Start()
    {
        Debug.Log("PoseLogger STARTED");

        filePath = Path.Combine(Application.persistentDataPath, "robot_pose.csv");
        Debug.Log("Pose log path: " + filePath);

        bool newFile = !File.Exists(filePath);
        writer = new StreamWriter(filePath, true);

        if (newFile)
        {
            writer.WriteLine("timestamp,x,y,heading");
            writer.Flush();
        }

        Debug.Log("Logging robot pose to: " + filePath);
    }

    void FixedUpdate()
    {
        logTimer += Time.fixedDeltaTime;
        if (logTimer < logRate) return;
        logTimer = 0f;

        if (origin == null) return;

        float timestamp = Time.time;

        Vector3 relativePos = transform.position - origin.position;

        // Correct mapping for Field2d
        float x = -relativePos.x;     // Unity X → Field2d X
        float y = -relativePos.z;     // Unity Z → Field2d Y
        float headingRad = transform.eulerAngles.y * Mathf.Deg2Rad;

        writer.WriteLine($"{timestamp},{x},{y},{headingRad}");
        writer.Flush();
    }

    void OnApplicationQuit()
    {
        writer?.Close();
    }
}
