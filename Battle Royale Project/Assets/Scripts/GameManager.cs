using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPun
{
    [Header("Players")]
    private string playerPrefabLocation;
    public PlayerController[] players;
    public Transform[] spawnPoints;
    public int alivePlayers;
    public float postGameTime;

    private int playersInGame;
    private bool skinLook = true;

    // instance
    public static GameManager instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        alivePlayers = players.Length;

        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void ImInGame()
    {
        playersInGame++;

        if (PhotonNetwork.IsMasterClient && playersInGame == PhotonNetwork.PlayerList.Length)
            photonView.RPC("SpawnPlayer", RpcTarget.All);
    }

    [PunRPC]
    void SpawnPlayer()
    {
        for (int s=0; s<GameManager.instance.players.Length; s++)
        {
            if (s % 2 == 0)
                skinLook = false;
            else
                skinLook = true;
        }
        if (skinLook) 
        {
            playerPrefabLocation = "RedPlayer";  
        }
        else
        {
            playerPrefabLocation = "BluePlayer";
        }

        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);
        

        // initialize the player for all other players
        playerObj.GetComponent<PlayerController>().photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
        
    }

    public PlayerController GetPlayer (int playerId)
    {
        // return players.First(x => x.id == playerId);

        // support for if player is null
        foreach(PlayerController player in players)
        {
            if (player != null && player.id == playerId)
                return player;
        }
        return null;
    }

    public PlayerController GetPlayer (GameObject playerObj)
    {
        //return players.First(x => x.gameObject == playerObject);

        // handle disconnected player possibility
        foreach (PlayerController player in players)
        {
            if (player != null && player.gameObject == playerObj)
                return player;
        }
        return null;
    }

    public void CheckWinCondition()
    {
        if (alivePlayers == 1)
            photonView.RPC("WinGame", RpcTarget.All, players.First(x => !x.dead).id);
    }

    [PunRPC]
    void WinGame(int winningPlayer)
    {
        //set the UI win text
        GameUI.instance.SetWinText(GetPlayer(winningPlayer).photonPlayer.NickName);

        Invoke("GoBackToMenu", postGameTime);
    }

    void GoBackToMenu()
    {
        NetworkManager.instance.ChangeScene("Menu");
    }
}
