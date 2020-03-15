using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet __packet)
    {
        string __msg = __packet.ReadString();
        int __myId = __packet.ReadInt();

        Debug.Log($"Message from server: {__msg}");
        Client.instance.myId = __myId;
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet __packet)
    {
        int __id = __packet.ReadInt();
        string __username = __packet.ReadString();
        Vector3 __position = __packet.ReadVector3();
        Quaternion __rotation = __packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(__id, __username, __position, __rotation);
    }

    public static void PlayerPosition(Packet __packet)
    {
        int __id = __packet.ReadInt();
        Vector3 __position = __packet.ReadVector3();

        GameManager.players[__id].transform.position = __position;
    }

    public static void PlayerRotation(Packet __packet)
    {
        int __id = __packet.ReadInt();
        Quaternion __roation = __packet.ReadQuaternion();

        GameManager.players[__id].transform.rotation = __roation;
    }

    public static void PlayerDisconnected(Packet __packet)
    {
        int __id = __packet.ReadInt();
        
        Destroy(GameManager.players[__id].gameObject);
        GameManager.players.Remove(__id);
    }

    public static void PlayerHealth(Packet __packet)
    {
        int __id = __packet.ReadInt();
        float __health = __packet.ReadFloat();

        GameManager.players[__id].SetHealth(__health);
    }

    public static void PlayerRespawned(Packet __packet)
    {
        int __id = __packet.ReadInt();

        GameManager.players[__id].Respawn();
    }
}
