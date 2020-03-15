using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    private static void SendTCPData(int __toClient, Packet __packet)
    {
        __packet.Writelength();
        Server.clients[__toClient].tcp.SendData(__packet);
    }

    private static void SendUDPData(int __toClient, Packet __packet)
    {
        __packet.Writelength();
        Server.clients[__toClient].udp.SendData(__packet);
    }

    private static void SendTCPDataToAll(Packet __packet)
    {
        __packet.Writelength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(__packet);
        }
    }

    private static void SendTCPDataToAll(int __exceptClient, Packet __packet)
    {
        __packet.Writelength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {

            if (i != __exceptClient)
            {
                Server.clients[i].tcp.SendData(__packet);
            }
        }
    }


    private static void SendUDPDataToAll(Packet __packet)
    {
        __packet.Writelength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(__packet);
        }
    }

    private static void SendUDPDataToAll(int __exceptClient, Packet __packet)
    {
        __packet.Writelength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {

            if (i != __exceptClient)
            {
                Server.clients[i].udp.SendData(__packet);
            }
        }
    }

    #region Packets
    public static void Welcome(int __toClient, string __msg)
    {
        using (Packet __packet = new Packet((int)ServerPackets.welcome))
        {
            __packet.Write(__msg);
            __packet.Write(__toClient);

            SendTCPData(__toClient, __packet);
        }
    }

    public static void SpawnPlayer(int __toClient, Player __player)
    {
        using (Packet __packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            __packet.Write(__player.id);
            __packet.Write(__player.username);
            __packet.Write(__player.transform.position);
            __packet.Write(__player.transform.rotation);

            //Sending the packet using TCP, since we will only be sending this info once and it is important info so we do not want to lose it
            SendTCPData(__toClient, __packet);
        }
    }

    public static void PlayerPosition(Player __player)
    {
        using (Packet __packet = new Packet((int)ServerPackets.playerPosition))
        {
            __packet.Write(__player.id);
            __packet.Write(__player.transform.position);

            SendUDPDataToAll(__packet);
        }
    }

    public static void PlayerRotation(Player __player)
    {
        using (Packet __packet = new Packet((int)ServerPackets.playerRotation))
        {
            __packet.Write(__player.id);
            __packet.Write(__player.transform.rotation);

            SendUDPDataToAll(__player.id, __packet);
        }
    }

    public static void PlayerDisonnected(int __playerId)
    {
        using (Packet __packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            __packet.Write(__playerId);

            SendTCPDataToAll(__packet);
        }
    }

    public static void PlayerHealth(Player __player)
    {
        using (Packet __packet = new Packet((int)ServerPackets.playerHealth))
        {
            __packet.Write(__player.id);
            __packet.Write(__player.health);

            SendTCPDataToAll(__packet);
        }
    }

    public static void PlayerRespawned(Player __player)
    {
        using (Packet __packet = new Packet((int)ServerPackets.playerRespawned))
        {
            __packet.Write(__player);

            SendTCPDataToAll(__packet);
        }
    }
    #endregion

}
