using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

public class TCPVideoSender : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera cam;
    public int frameWidth = 320;
    public int frameHeight = 240;
    public int fps = 10; // lower FPS for reliability

    [Header("TCP Settings")]
    public string ip = "127.0.0.1";
    public int port = 5806;

    private TcpClient client;
    private NetworkStream stream;
    private RenderTexture rt;
    private Texture2D tex;
    private Thread sendThread;
    private bool running = true;
    private float interval;

    private ConcurrentQueue<byte[]> frameQueue = new ConcurrentQueue<byte[]>();

    void Start()
    {
        if (cam == null) cam = Camera.main;

        rt = new RenderTexture(frameWidth, frameHeight, 24);
        tex = new Texture2D(frameWidth, frameHeight, TextureFormat.RGB24, false);

        interval = 1f / fps;

        sendThread = new Thread(SendThread);
        sendThread.IsBackground = true;
        sendThread.Start();
    }

    void Update()
    {
        // Capture on main thread
        cam.targetTexture = rt;
        RenderTexture.active = rt;
        cam.Render();

        tex.ReadPixels(new Rect(0, 0, frameWidth, frameHeight), 0, 0);
        tex.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;

        // Encode to JPEG and enqueue
        byte[] jpg = tex.EncodeToJPG(50);
        frameQueue.Enqueue(jpg);

        // Limit queue size to avoid memory bloat
        while (frameQueue.Count > 5)
        {
            frameQueue.TryDequeue(out _);
        }
    }

    void SendThread()
    {
        while (running)
        {
            try
            {
                Debug.Log($"Attempting TCP connection to {ip}:{port}...");
                client = new TcpClient();
                client.Connect(ip, port);
                stream = client.GetStream();
                Debug.Log($"Connected to Python TCP server at {ip}:{port}");
                break;
            }
            catch
            {
                Debug.Log($"TCP connection to {ip}:{port} failed, retrying in 1s...");
                Thread.Sleep(1000);
            }
        }

        while (running)
        {
            if (frameQueue.TryDequeue(out byte[] jpg))
            {
                try
                {
                    // 4-byte big-endian length prefix
                    byte[] lenBytes = System.BitConverter.GetBytes(jpg.Length);
                    if (System.BitConverter.IsLittleEndian)
                        System.Array.Reverse(lenBytes);

                    stream.Write(lenBytes, 0, 4);
                    stream.Write(jpg, 0, jpg.Length);
                    stream.Flush();
                }
                catch
                {
                    Debug.LogWarning("Connection lost while sending frame.");
                    running = false;
                }
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    void OnDestroy()
    {
        running = false;
        if (sendThread != null && sendThread.IsAlive) sendThread.Join();
        stream?.Close();
        client?.Close();
        Destroy(rt);
        Destroy(tex);
    }
}
