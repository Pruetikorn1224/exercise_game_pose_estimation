using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;


public class UdpReceiver : MonoBehaviour
{
    public PlayerController playerController;

    Thread receiveThread;
    UdpClient client;
    public int port = 5052;
    public bool startReceiving = true;
    public bool printToConsole = false;
    public int[] receivedArray;

    // Start is called before the first frame update
    void Start()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void Update()
    {
        if (playerController.isHitObstacle)
        {
            receiveThread.Abort();
        }
    }

    // Receive thread
    private void ReceiveData()
    {
        client = new UdpClient(port);

        while (startReceiving)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] dataByte = client.Receive(ref anyIP);
                string dataString = Encoding.UTF8.GetString(dataByte);

                // Remove square brackets from the received data string
                dataString = dataString.Replace("[", "").Replace("]", "");

                // Convert the received data string to an array
                string[] stringValues = dataString.Split(',');
                receivedArray = Array.ConvertAll(stringValues, int.Parse);

                if (printToConsole) { Debug.Log(string.Join(", ", receivedArray)); }
            }
            catch (Exception error)
            {
                Debug.LogError(error);
            }
        }
    }
}
