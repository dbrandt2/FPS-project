using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System;


public class Server
{
        public static int MaxPlayers { get; private set; }

        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int __fromClient, Packet __packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        public static void Start(int __maxPlayers, int __port)
        {
            MaxPlayers = __maxPlayers;
            Port = __port;

            Debug.Log("Starting Server...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Debug.Log($"Server started on {Port}.");
        }

        private static void TCPConnectCallback(IAsyncResult __result)
        {
            TcpClient __client = tcpListener.EndAcceptTcpClient(__result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Debug.Log($"Incoming connection from {__client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(__client);
                    return;
                }
            }

            Debug.Log($"{__client.Client.RemoteEndPoint} failed to connect: Server full.");
        }

        private static void UDPReceiveCallback(IAsyncResult __result)
        {
            try
            {
                IPEndPoint __clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] __data = udpListener.EndReceive(__result, ref __clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);


                //TODO: may change in future iterations
                if (__data.Length < 4)
                {
                    return;
                }

                using (Packet __packet = new Packet(__data))
                {
                    int __clientId = __packet.ReadInt();

                    //safety check since we are about to pull from a dictionary where a 0 key does not exist
                    if (__clientId == 0)
                    {
                        return;
                    }


                    if (clients[__clientId].udp.endPoint == null) //then this is the first connection that just opens the port and we do not want to handle the data since it should be empty
                    {
                        clients[__clientId].udp.Connect(__clientEndPoint);
                        return;
                    }

                    //prevents hackers from impersonating another user by passing a different client id then their own
                    if (clients[__clientId].udp.endPoint.ToString() == __clientEndPoint.ToString())
                    {
                        clients[__clientId].udp.HandleData(__packet);
                    }
                }
            }
            catch (Exception __ex)
            {
                Debug.Log($"Error receiving UDP data: {__ex}");
            }
        }

        public static void SendUDPData(IPEndPoint __clientEndPoint, Packet __packet)
        {
            try
            {
                if (__clientEndPoint != null)
                {
                    udpListener.BeginSend(__packet.ToArray(), __packet.Length(), __clientEndPoint, null, null);
                }
            }
            catch (Exception __ex)
            {
                Debug.Log($"Error sending data to {__clientEndPoint} via UDP: {__ex}.");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement  },
                { (int)ClientPackets.playerShoot, ServerHandle.PlayerShoot }
            };
            Debug.Log("Initialize packets");
        }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}
