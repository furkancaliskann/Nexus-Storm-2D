using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerMessageManager : NetworkBehaviour
{
    private ServerRoundManager serverRoundManager;

    private int sendedPlayerWaitingMessageId = 0;
    private float playerWaitingMessageTimer = 1f;
    private float playerWaitingMessageTimerMax = 1f;

    void Start()
    {
        if (!isServer) return;
        serverRoundManager = GetComponent<ServerRoundManager>();
    }
    void Update()
    {
        if (!isServer) return;
        if (serverRoundManager.gameStage != GameStage.Warmup) return;

        CheckPlayerWaitingMessageTimer();
    }
    private void CheckPlayerWaitingMessageTimer()
    {
        if (playerWaitingMessageTimer > 0) playerWaitingMessageTimer -= Time.deltaTime;
        else
        {
            playerWaitingMessageTimer = playerWaitingMessageTimerMax;

            GameObject[] players = GameObject.FindGameObjectsWithTag("PlayerInfoDisplay");
            if (players.Length >= 2)
            {
                SendToAllPlayers("Waiting for server command to start the game!");
                return;
            }
            
            SendPlayersWaitingMessage();
        }
    }

    private void SendPlayersWaitingMessage()
    {
        switch (sendedPlayerWaitingMessageId)
        {
            case 0: SendToAllPlayers("Waiting for other players."); sendedPlayerWaitingMessageId = 1; break;
            case 1: SendToAllPlayers("Waiting for other players.."); sendedPlayerWaitingMessageId = 2; break;
            case 2: SendToAllPlayers("Waiting for other players..."); sendedPlayerWaitingMessageId = 0; break;
        }
    }
    public void SendToAllPlayers(string message)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("PlayerInfoDisplay");
        foreach (GameObject player in players)
        {
            player.GetComponent<ServerMessage>().TargetSendMessage(message);
        }
    }
}
