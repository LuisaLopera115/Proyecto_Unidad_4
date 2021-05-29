using System.Collections;
using UnityEngine;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

public class CommArduino2 : MonoBehaviour
{
    public Text IP;
    public Text localport;

    public Text IPA;
    public Text port;

    public Text time;
    private static CommArduino2 instance;

    private Thread receiveThread;
    private Thread sendThread;
    private float timer;
    private UdpClient receiveClient;
    private IPEndPoint receiveEndPoint;
    private IPEndPoint sendEndPoint;

    string ip = ""; // = "192.168.1.9";
    string ipA = ""; // = "192.168.1.8"; Arduino ip
    int receivePort = 0; // = 50001; localport 
    int sendPort = 0; // 50002 port
    private bool isInitialized;

    private Queue receiveQueue;
    private Queue sendQueue;

    string[] wday = new string[7] { " Lunes", " Martes", " Miercoles", " Jueves", " Viernes", " Sabado", " Domingo" };
    string[] Months = new string[12] { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
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
                //string text = Encoding.UTF8.GetString(data);
                //Debug.Log(text);
                //temperature = System.BitConverter.ToSingle(data, 0);
                //receiveQueue.Enqueue(temperature);
                //SerializeMessage(text);
                if (data.Length >= 19)
                {
                    if (data[0] == 0x2f && data[1] == 0x72 && data[2] == 0x5c)
                    {
                        string time = data[12] + ":" + data[13] + ":" + data[14] + wday[data[15]] + ", " + data[16] + " de " + Months[data[17]] + " de " + "20" + data[18];
                        receiveQueue.Enqueue(time);
                    }
                    
                }
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex.ToString());
            }
        }
    }

    private void SendData() {
        byte[] senddata = { 0x2f, 0x63, 0x5c, 0x00, 0x2c, 0x69, 0x69, 0x69, 0x69, 0x69, 0x69, 0x69, 0x00, 0x00, 0x00, 0x02, 0x1a, 0x05, 0x15, 0x00};
        while (true)
        {
            try
            {
                if (timer >= 1)
                {
                    timer = 0;
                    //Debug.Log("Entra");
                    receiveClient.Send(senddata, 20, sendEndPoint);
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
            time.text = receiveQueue.Dequeue().ToString();
        }
    }
}
