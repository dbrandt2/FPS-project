using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{ 
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

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

    public void SpawnPlayer(int __id, string __username, Vector3 __position, Quaternion __rotation)
    {
        GameObject __player;
        if (__id == Client.instance.myId)
        {
            __player = Instantiate(localPlayerPrefab, __position, __rotation);
        }
        else 
        {
            __player = Instantiate(playerPrefab, __position, __rotation);
        }

        __player.GetComponent<PlayerManager>().id = __id;
        __player.GetComponent<PlayerManager>().username = __username;
        //__player.GetComponent<PlayerManager>().Initialize(__id, __username);
        players.Add(__id, __player.GetComponent<PlayerManager>());
    }
}
