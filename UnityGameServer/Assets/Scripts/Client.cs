using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class Client
{
    public static int dataBufferSize = 4096;

    public int id;
    public Player player;
    public TCP tcp;
    public UDP udp;

    public Client(int __clientId)
    {
        id = __clientId;
        tcp = new TCP(id);
        udp = new UDP(id);
    }

    public class TCP
    {
        public TcpClient socket;

        private readonly int id;
        private NetworkStream stream;
        private Packet receivedData;

        private byte[] receiveBuffer;

        public TCP(int __id)
        {
            id = __id;
        }

        public void Connect(TcpClient __socket)
        {
            socket = __socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            stream = socket.GetStream();

            receivedData = new Packet();
            receiveBuffer = new byte[dataBufferSize];

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            ServerSend.Welcome(id, "Welcome to the server.");
        }

        public void SendData(Packet __packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(__packet.ToArray(), 0, __packet.Length(), null, null); //send data to appropriate client
                }
            }
            catch (Exception __ex)
            {
                Debug.Log($"Error sending data to player {id} via TCP: {__ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult __result)
        {
            try
            {
                int __byteLength = stream.EndRead(__result);
                if (__byteLength <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }

                byte[] __data = new byte[__byteLength];
                Array.Copy(receiveBuffer, __data, __byteLength);

                receivedData.Reset(HandleData(__data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception __ex)
            {
                Debug.Log($"Error receiving TCP data {__ex}");
                Server.clients[id].Disconnect();
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
                        Server.packetHandlers[__packetId](id, __packet);
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

        public void Disconnect()
        {
            socket.Close();
            stream = null;
            receiveBuffer = null;
            receivedData = null;
            socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint endPoint;
        private int id;

        public UDP(int __id)
        {
            id = __id;
        }

        public void Connect(IPEndPoint __endPoint)
        {
            endPoint = __endPoint;
        }

        public void SendData(Packet __packet)
        {
            Server.SendUDPData(endPoint, __packet);
        }

        public void HandleData(Packet __packetData)
        {
            int __packetLength = __packetData.ReadInt();
            byte[] __packetBytes = __packetData.ReadBytes(__packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet __packet = new Packet(__packetBytes))
                {
                    int __packetId = __packet.ReadInt();
                    Server.packetHandlers[__packetId](id, __packet);
                }
            });
        }

        public void Disconnect()
        {
            endPoint = null;
        }
    }

    public void SendIntoGame(string __playerName)
    {
        player = NetworkManager.instance.InstantiatePlayer();
        player.Initialize(id, __playerName);

        //set the spawn position
        if (player.id == 1)
        {
            player.transform.position = new Vector3(4, .5f, -4);
        }
        else if (player.id == 2)
        {
            player.transform.position = new Vector3(-4, 0.5f, 4);
        }

        //send information to new player about players already connected
        foreach (Client __client in Server.clients.Values)
        {
            if (__client.player != null)
            {
                if (__client.id != id)
                {
                    ServerSend.SpawnPlayer(id, __client.player);
                }
            }
        }

        //sends players information to all other players and to himself
        foreach (Client __client in Server.clients.Values)
        {
            if (__client.player != null)
            {
                ServerSend.SpawnPlayer(__client.id, player);
            }
        }
    }

    private void Disconnect()
    {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has diconnected.");

        ThreadManager.ExecuteOnMainThread(() => {

            UnityEngine.Object.Destroy(player.gameObject);
            player = null;

        });

        tcp.Disconnect();
        udp.Disconnect();

        ServerSend.PlayerDisonnected(id);
    }
}

