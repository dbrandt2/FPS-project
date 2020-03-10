using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    //ip of local host
    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public TCP tcp;
    public UDP udp;


    private bool isConnected = false;
    private delegate void PacketHandler(Packet __packet);
    private static Dictionary<int, PacketHandler> packetHandlers;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object...");
            Destroy(this);
        }
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    private void Start()
    {
        tcp = new TCP();
        udp = new UDP();
    }

    public void ConnectToServer()
    {
        InitializeClientData();
        isConnected = true;
        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;


        public void Connect()
        {
            socket = new TcpClient()
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult __result)
        {
            socket.EndConnect(__result);

            if (!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            receivedData = new Packet();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet __packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(__packet.ToArray(), 0, __packet.Length(), null, null); // send data to server
                }
            }
            catch (Exception __ex)
            {
                Debug.Log($"Error sending data to server via TCP: {__ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult __result)
        {
            try
            {
                int __byteLength = stream.EndRead(__result);
                if (__byteLength <= 0)
                {
                    instance.Disconnect();
                    return;
                }

                byte[] __data = new byte[__byteLength];
                Array.Copy(receiveBuffer, __data, __byteLength);

                receivedData.Reset(HandleData(__data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception __ex)
            {
                Disconnect();
            }
        }

        private bool HandleData(byte[] __data)
        {
            int __packetLength = 0;

            receivedData.SetBytes(__data); //set receivedData to the bytes we just read from the stream

            if (receivedData.UnreadLength() >= 4) //if the unreadLength is 4 or greater then we have the start of one of our packets the first value sent is an int (containing length of packet)  int = (4 bytes)
            {
                __packetLength = receivedData.ReadInt();
                if (__packetLength <= 0)
                {
                    return true;
                }
            }

            while (__packetLength > 0 && __packetLength <= receivedData.UnreadLength()) //while this loop is running it means we have another COMPLETE packet we can handle
            {
                byte[] __packetBytes = receivedData.ReadBytes(__packetLength);
                //Makes sure we are going to run this all on one thread
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet __packet = new Packet(__packetBytes))
                    {
                        int __packetId = __packet.ReadInt();
                        packetHandlers[__packetId](__packet);
                    }
                });

                __packetLength = 0;
                if (receivedData.UnreadLength() >= 4) //if the unreadLength is 4 or greater then we have the start of one of our packets the first value sent is an int (containing length of packet)  int = (4 bytes)
                {
                    __packetLength = receivedData.ReadInt();
                    if (__packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (__packetLength <= 1)
            {
                return true;
            }

            return false; //partial packet left in packetData
        }

        private void Disconnect()
        {
            instance.Disconnect();

            stream = null;
            receiveBuffer = null;
            receivedData = null;
            socket = null;
        }   
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

    public UDP()
    {
        endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
    }

        public void Connect(int __localPort)
        {
            socket = new UdpClient(__localPort);

            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (Packet __packet = new Packet())
            {
                SendData(__packet);
            }
        }

        public void SendData(Packet __packet)
        {
            try
            {
                __packet.InsertInt(instance.myId);
                if (socket != null)
                {
                    socket.BeginSend(__packet.ToArray(), __packet.Length(), null, null);
                }
            }
            catch (Exception __ex)
            {
                Debug.Log($"Error sending data to server via UDP: {__ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult __result)
        {
            try
            {
                byte[] __data = socket.EndReceive(__result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                //TODO: May change in future iterations (tutorials)
                if (__data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(__data);
            }
            catch (Exception __ex)
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] __data)
        {
            using (Packet __packet = new Packet(__data))
            {
                int __packetLength = __packet.ReadInt();
                __data = __packet.ReadBytes(__packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet __packet = new Packet(__data))
                {
                    int __packetId = __packet.ReadInt();
                    packetHandlers[__packetId](__packet);
                }

            });
        }

        private void Disconnect()
        {
            instance.Disconnect();

            endPoint = null;
            socket = null;
        }
    }

private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, ClientHandle.Welcome },
            { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer},
            { (int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },
            { (int)ServerPackets.playerRotation, ClientHandle.PlayerRotation },

        };
        Debug.Log("Initialize client data.");
    }

    private void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();

            Debug.Log("Disconnected from server.");
        }
    }
}

