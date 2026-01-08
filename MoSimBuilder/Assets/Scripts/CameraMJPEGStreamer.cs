using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

/// <summary>
/// This is the MonoBehaviour you attach to a GameObject.
/// It supports multiple cameras streaming to TCP simultaneously.
/// </summary>
public class MultiTCPVideoSender : MonoBehaviour
{
    [Header("Camera Setup")]
    public Camera[] cameras;              // Assign your 3 cameras here in inspector
    public string[] cameraNames;          // Names like "DriveCam", "ArmCam", "LiftCam"
    public string[] ips;                  // IPs for each camera (usually "127.0.0.1")
    public int[] ports;                   // Ports for each camera (e.g., 8080,8081,8082)
    public int width = 320;               // Default width
    public int height = 240;              // Default height
    public int fps = 10;                  // Default FPS
    public int jpegQuality = 50;          // JPEG encoding quality

    private class CameraState
    {
        public Camera cam;
        public RenderTexture rt;
        public Texture2D tex;
        public ConcurrentQueue<byte[]> frameQueue = new ConcurrentQueue<byte[]>();
        public Thread sendThread;
        public bool running = true;
        public string name;
        public string ip;
        public int port;
    }

    private List<CameraState> states = new List<CameraState>();

    void Start()
    {
        int count = cameras.Length;
        for (int i = 0; i < count; i++)
        {
            Camera cam = cameras[i] != null ? cameras[i] : Camera.main;
            string name = (cameraNames != null && i < cameraNames.Length) ? cameraNames[i] : $"Cam{i}";
            string ip = (ips != null && i < ips.Length) ? ips[i] : "127.0.0.1";
            int port = (ports != null && i < ports.Length) ? ports[i] : 8080 + i;

            CameraState state = new CameraState
            {
                cam = cam,
                rt = new RenderTexture(width, height, 24),
                tex = new Texture2D(width, height, TextureFormat.RGB24, false),
                name = name,
                ip = ip,
                port = port
            };

            state.sendThread = new Thread(() => SendThread(state));
            state.sendThread.IsBackground = true;
            state.sendThread.Start();

            states.Add(state);
            Debug.Log($"[{state.name}] Initialized TCP sender on {state.ip}:{state.port}");
        }
    }

    void Update()
    {
        foreach (var state in states)
        {
            Camera cam = state.cam;
            cam.targetTexture = state.rt;
            RenderTexture.active = state.rt;
            cam.Render();

            state.tex.ReadPixels(new Rect(0, 0, state.rt.width, state.rt.height), 0, 0);
            state.tex.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;

            byte[] jpg = state.tex.EncodeToJPG(jpegQuality);
            state.frameQueue.Enqueue(jpg);

            while (state.frameQueue.Count > 5)
                state.frameQueue.TryDequeue(out _);
        }
    }

    private void SendThread(CameraState state)
    {
        while (state.running)
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(state.ip, state.port);
                NetworkStream stream = client.GetStream();
                Debug.Log($"[{state.name}] Connected to Python server at {state.ip}:{state.port}");

                while (state.running)
                {
                    if (state.frameQueue.TryDequeue(out byte[] jpg))
                    {
                        try
                        {
                            byte[] lenBytes = System.BitConverter.GetBytes(jpg.Length);
                            if (System.BitConverter.IsLittleEndian)
                                System.Array.Reverse(lenBytes);

                            stream.Write(lenBytes, 0, 4);
                            stream.Write(jpg, 0, jpg.Length);
                            stream.Flush();
                        }
                        catch
                        {
                            Debug.LogWarning($"[{state.name}] Connection lost while sending frame.");
                            break;
                        }
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }

                stream.Close();
                client.Close();
            }
            catch
            {
                Debug.LogWarning($"[{state.name}] TCP connection failed, retrying in 1s...");
                Thread.Sleep(1000);
            }
        }
    }

    void OnDestroy()
    {
        foreach (var state in states)
        {
            state.running = false;
            if (state.sendThread != null && state.sendThread.IsAlive)
                state.sendThread.Join();

            if (state.rt != null) Destroy(state.rt);
            if (state.tex != null) Destroy(state.tex);
        }
    }
}
