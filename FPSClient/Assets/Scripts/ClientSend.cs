using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet __packet)
    {
        __packet.Writelength();
        Client.instance.tcp.SendData(__packet);
    }

    private static void SendUDPData(Packet __packet)
    {
        __packet.Writelength();
        Client.instance.udp.SendData(__packet);
    }

    public static void WelcomeReceived()
    {
        using (Packet __packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            __packet.Write(Client.instance.myId);
            __packet.Write(UIManager.instance.usernameField.text);

            SendTCPData(__packet);
        }
    }

    public static void PlayerMovement(bool[] __inputs)
    {
        using (Packet __packet = new Packet((int)ClientPackets.playerMovement))
        {
            __packet.Write(__inputs.Length);
            foreach (bool __input in __inputs)
            {
                __packet.Write(__input);
            }
            __packet.Write(GameManager.players[Client.instance.myId].transform.rotation);

            SendUDPData(__packet);
        }
    }

    public static void PlayerShoot(Vector3 __facing)
    {
        using (Packet __packet = new Packet((int)ClientPackets.playerShoot))
        {
            __packet.Write(__facing);

            SendTCPData(__packet);
        }
    }
}
