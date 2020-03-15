using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class ServerHandle
{

    public static void WelcomeReceived(int __fromClient, Packet __packet)
    {
        int __clientIdCheck = __packet.ReadInt();
        string __username = __packet.ReadString();

        Debug.Log($"{Server.clients[__fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {__fromClient}");
        if (__fromClient != __clientIdCheck)
        {
            Debug.Log($"Player \"{__username}\" (ID: {__fromClient}) has assumed the wrong client ID ({__clientIdCheck}). ");
        }
        Server.clients[__fromClient].SendIntoGame(__username);

    }

    public static void PlayerMovement(int __fromClient, Packet __packet)
    {
        bool[] __inputs = new bool[__packet.ReadInt()];
        for (int i = 0; i < __inputs.Length; i++)
        {
            __inputs[i] = __packet.ReadBool();
        }
        Quaternion __rotation = __packet.ReadQuaternion();

        Server.clients[__fromClient].player.SetInputs(__inputs, __rotation);
    }

    public static void PlayerShoot(int __fromClient, Packet __packet)
    {
        Vector3 __shootDirection = __packet.ReadVector3();

        Server.clients[__fromClient].player.Shoot(__shootDirection);
    }
}
