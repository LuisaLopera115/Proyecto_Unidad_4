using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class TouchOsc : MonoBehaviour
{
    public Text IP;
    public Text localport;

    public Text IPA;
    public Text port;

    public Animator bongo1;
    public Animator bongo2;
    public Animator bobongo1;
    public Animator bobongo2;
    public Animator guitar;
    public Animator piano;
    public Animator maraca1;
    public Animator maraca2;
    public Animator bell;

    private static TouchOsc instance;
    private Thread receiveThread;
    private Thread sendThread;
    private float timer;
    private UdpClient receiveClient;
    private IPEndPoint receiveEndPoint;
    private IPEndPoint sendEndPoint;

    string ip = ""; // = "192.168.1.9"; ip pc
    string ipA = ""; // = "192.168.1.2" ipTouch
    int receivePort = 0; // 50003 localport 
    int sendPort = 0; // 50004 
    private bool isInitialized;
    string push = "";
    private Queue receiveQueue;
    private Queue sendQueue;

    private void Start()
    {
        ip = IP.text;
        ipA = IPA.text;
        receivePort = int.Parse(localport.text);
        sendPort = int.Parse(port.text);

        timer = 0;
        Initialize();
    }

    private void Initialize()
    {
        instance = this;
        receiveEndPoint = new IPEndPoint(IPAddress.Parse(ip), receivePort);
        sendEndPoint = new IPEndPoint(IPAddress.Parse(ipA), sendPort);

        receiveClient = new UdpClient(receivePort);

        receiveQueue = Queue.Synchronized(new Queue());
        sendQueue = Queue.Synchronized(new Queue());

        receiveThread = new Thread(new ThreadStart(ReceiveDataListener));
        sendThread = new Thread(new ThreadStart(SendData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        sendThread.Start();
        isInitialized = true;
    }

    private void ReceiveDataListener()
    {
        while (true)
        {
            try
            {
                byte[] data = receiveClient.Receive(ref receiveEndPoint);
                string text = Encoding.UTF8.GetString(data);
                receiveQueue.Enqueue(text);
                
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex.ToString());
            }
        }
    }

    private void SendData()
    {
        byte[] senddata = { 0x2f, 0x31, 0x2f, 0x70, 0x75, 0x73, 0x68, 0x39}; //0x2c, 0x66, 0x20, 0xac, 0x3f, 0x00, 0x00,0x00
        while (true)
        {
            try
            {
                if (timer >= 1)
                {
                    timer = 0;
                    receiveClient.Send(senddata, 8, sendEndPoint);
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
            sendThread.Abort();
            sendThread = null;
            receiveClient.Close();
            receiveClient = null;
            Debug.Log("Thread1 killed");
            isInitialized = false;
        }
    }
    void Update()
    {
        timer = timer + 1 * Time.deltaTime;
        if (receiveQueue.Count > 0)
        {
            push = receiveQueue.Dequeue().ToString();

            if (push.Contains("/1/push1"))
            {
                bongo1.Play("Base Layer.bongo1");
            }
            else if (push.Contains("/1/push2"))
            {
                bongo2.Play("Base Layer.bongo2");
            }
            else if (push.Contains("/1/push3"))
            {
                bobongo1.Play("Base Layer.bobongo1");
            }
            else if (push.Contains("/1/push4"))
            {
                bobongo2.Play("Base Layer.bobongo2");
            }
            else if (push.Contains("/1/push5"))
            {
                guitar.Play("Base Layer.guitar");
            }
            else if (push.Contains("/1/push6"))
            {
                piano.Play("Base Layer.marimba");
            }
            else if (push.Contains("/1/push7"))
            {
                maraca1.Play("Base Layer.maraca2");
            }
            else if (push.Contains("/1/push8"))
            {
                maraca2.Play("Base Layer.maraca1");
            }
            else if (push.Contains("/1/push9"))
            {
                bell.Play("Base Layer.bell");
            }
            else { Debug.Log("erroneo: "+push); }
        }
    }
}
