using UnityEngine;
using System.Net.Sockets;
using System.Text;

public class PoseSender : MonoBehaviour
{
    [Header("References")]
    public Transform origin;      // Field origin (world 0,0,0)
    public Transform frontPoint;  // Empty GameObject at robot front

    [Header("UDP Settings")]
    public string ip = "127.0.0.1";
    public int port = 5805;

    private UdpClient udp;

    void Start()
    {
        udp = new UdpClient();
        Debug.Log("UDP PoseSender started → " + ip + ":" + port);
    }

    void Update()
    {
        // Field-relative position
        Vector3 pos = transform.position - origin.position;

        // Heading from frontPoint
        Vector3 fwd = (frontPoint.position - transform.position).normalized;
        float headingDeg = Mathf.Atan2(fwd.x, fwd.z) * Mathf.Rad2Deg;

        // Send as: x,z,heading_deg
        string msg = $"{pos.x:F3},{pos.z:F3},{headingDeg:F2}";
        byte[] data = Encoding.ASCII.GetBytes(msg);

        udp.Send(data, data.Length, ip, port);
    }

    void OnDestroy()
    {
        udp?.Close();
    }
}
