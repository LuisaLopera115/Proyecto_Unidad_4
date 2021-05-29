using System.Collections;
using UnityEngine;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

public class Arduino1 : MonoBehaviour
{
    public Text IP;
    public Text localport;

    public Text temp;
    private static Arduino1 instance;

    private Thread receiveThread;
    private UdpClient receiveClient;
    private IPEndPoint receiveEndPoint;

    string ip = ""; // = "192.168.1.9";
    int receivePort = 0; // localport 50008
    private bool isInitialized;

    private Queue receiveQueue;

    float temperature = 0;
    private void Start()
    {
        ip = IP.text;
        receivePort = int.Parse(localport.text);
        Initialize();
    }

    private void Initialize()
    {
        instance = this;
        receiveEndPoint = new IPEndPoint(IPAddress.Parse(ip), receivePort);

        receiveClient = new UdpClient(receivePort);

        receiveQueue = Queue.Synchronized(new Queue());

        receiveThread = new Thread(new ThreadStart(ReceiveDataListener));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        isInitialized = true;
    }

    private void ReceiveDataListener()
    {
        while (true)
        {
            try
            {
                byte[] data = receiveClient.Receive(ref receiveEndPoint);
                if (data.Length >= 12)
                {
                    if (data[0] == 0x2f && data[1] == 0x74 && data[2] == 0x5c)
                    {
                        temperature = System.BitConverter.ToSingle(data, 8);
                        receiveQueue.Enqueue(temperature);
                        Debug.Log(temperature.ToString());
                    }

                }
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex.ToString());
            }
        }
    }

    private void OnDestroy()
    {
        TryKillThread();
    }

    private void OnApplicationQuit()
    {
        TryKillThread();
    }

    private void TryKillThread()
    {
        if (isInitialized)
        {
            receiveThread.Abort();
            receiveThread = null;
            receiveClient.Close();
            receiveClient = null;
            Debug.Log("Thread1 killed");
            isInitialized = false;
        }
    }
    void Update()
    {
        if (receiveQueue.Count > 0)
        {
            temp.text = receiveQueue.Dequeue().ToString();
        }
    }
}
