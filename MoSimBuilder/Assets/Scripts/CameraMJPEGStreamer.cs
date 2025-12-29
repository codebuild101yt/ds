using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

public class CameraMJPEGStreamerTCP : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera cam;                 // Camera on Display 8
    public int width = 640;
    public int height = 480;
    public int fps = 15;
    public int port = 8080;

    private RenderTexture renderTexture;
    private byte[] latestFrame;
    private TcpListener tcpListener;
    private bool serverRunning = false;

    void Start()
    {
        // Setup camera
        renderTexture = new RenderTexture(width, height, 24);
        cam.targetTexture = renderTexture;

        // Start server thread
        Thread serverThread = new Thread(StartServer);
        serverThread.IsBackground = true;
        serverThread.Start();

        // Start capturing frames
        StartCoroutine(CaptureFrames());
    }

    IEnumerator CaptureFrames()
    {
        var wait = new WaitForSeconds(1f / fps);
        while (true)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            RenderTexture.active = renderTexture;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            latestFrame = tex.EncodeToJPG();
            Destroy(tex);

            yield return wait;
        }
    }

    void StartServer()
    {
        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();
        serverRunning = true;
        Debug.Log("MJPEG TCP Server started on port " + port);

        while (serverRunning)
        {
            try
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                Debug.Log("Client connected: " + client.Client.RemoteEndPoint);
                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.IsBackground = true;
                clientThread.Start();
            }
            catch { }
        }
    }

    void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        StreamWriter writer = new StreamWriter(stream, Encoding.ASCII);
        try
        {
            // Send HTTP headers
            writer.Write("HTTP/1.0 200 OK\r\n");
            writer.Write("Server: UnityMJPEG\r\n");
            writer.Write("Cache-Control: no-cache\r\n");
            writer.Write("Pragma: no-cache\r\n");
            writer.Write("Content-Type: multipart/x-mixed-replace; boundary=frame\r\n");
            writer.Write("\r\n");
            writer.Flush();

            while (client.Connected)
            {
                if (latestFrame != null)
                {
                    string header = "--frame\r\nContent-Type: image/jpeg\r\nContent-Length: " + latestFrame.Length + "\r\n\r\n";
                    byte[] headerBytes = Encoding.ASCII.GetBytes(header);
                    stream.Write(headerBytes, 0, headerBytes.Length);
                    stream.Write(latestFrame, 0, latestFrame.Length);
                    stream.Write(Encoding.ASCII.GetBytes("\r\n"), 0, 2);
                    stream.Flush();
                }
                Thread.Sleep(1000 / fps);
            }
        }
        catch { }
        finally
        {
            client.Close();
            Debug.Log("Client disconnected");
        }
    }

    void OnApplicationQuit()
    {
        serverRunning = false;
        tcpListener.Stop();
    }
}
